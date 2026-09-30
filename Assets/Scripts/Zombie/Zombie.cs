using TheWard.Combat;
using TheWard.Audio;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    public enum ZombieState
    {
        Unaware,
        Suspicious,
        Detected,
        Attacking
    }

    [SerializeField]
    private ZombieState currentState;

    public bool stealthMechanics = false;
    [SerializeField]
    private float aggroRange;
    private bool attackInProgress;
    public bool isDead;
    private bool hordeAggroed = false;

    private GameObject player;
    private NavMeshAgent agent;
    [SerializeField]
    private float damage;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float walkSpeed, chaseSpeed;

    [SerializeField]
    private float attackRange, detectionDist, viewDist, viewAngle;

    [SerializeField]
    private float attackInterval;

    private float attackTimer;

    [SerializeField]
    private LayerMask obstructionMask;

    [Header("Audio")]
    [Tooltip("Automatic distributes zombies across the attack voice sets. Choose a set to give a prefab a specific voice.")]
    [SerializeField] private ZombieAttackVoice attackVoice = ZombieAttackVoice.Automatic;
    private int assignedAttackVoice = -1;
    private int previousAttackClip = -1;

    private Health health;
    private float nextVocalTime;
    private float nextHurtTime;
    private float footstepDistance;
    private Vector3 previousPosition;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        // Choose once per zombie so its attack voice remains consistent across encounters.
        assignedAttackVoice = GameAudio.AssignZombieVoice(attackVoice);
        //currentState = ZombieState.Unaware;
        previousPosition = transform.position;
        nextVocalTime = Time.time + Random.Range(2f, 6f);
    }

    void Update()
    {

        UpdateAudio();
        if (!isDead && player != null && agent.isOnNavMesh)
        {
            switch (currentState)
            {
                case ZombieState.Unaware:
                    UnawareState();
                    break;

                case ZombieState.Suspicious:
                    SuspiciousState();
                    break;

                case ZombieState.Detected:
                    DetectedState();
                    break;

                case ZombieState.Attacking:
                    AttackingState();
                    break;
            }
        }

    }

    private void UpdateAudio()
    {
        Vector3 displacement = transform.position - previousPosition;
        displacement.y = 0f;
        previousPosition = transform.position;
        if (isDead || Time.timeScale <= 0f) return;
        if (currentState != ZombieState.Unaware)
            GameAudio.ReportThreat(transform.position,
                currentState == ZombieState.Detected || currentState == ZombieState.Attacking);
        // Stagger idle calls so groups do not all vocalize together or talk over their attacks.
        if (Time.time >= nextVocalTime && currentState != ZombieState.Attacking)
        {
            GameAudio.Play(AudioCue.ZombieIdle, transform.position, true, owner: this);
            nextVocalTime = Time.time + Random.Range(5f, 9f);
        }
        // Ignore large position jumps such as teleports when counting walking distance.
        if (displacement.magnitude < 2f) footstepDistance += displacement.magnitude;
        if (footstepDistance >= 1.4f)
        {
            GameAudio.Play(AudioCue.ZombieFootstep, transform.position, true, owner: this);
            footstepDistance = 0f;
        }
    }

    private void OnEnable()
    {
        health = GetComponent<Health>();
        // Subscribe to actual damage, regardless of which weapon caused it.
        if (health != null) health.Damaged += PlayHurtSound;
        previousPosition = transform.position;
        footstepDistance = 0f;
    }

    private void OnDisable()
    {
        // Unsubscribe on disable to avoid duplicate callbacks when the zombie is enabled again.
        if (health != null) health.Damaged -= PlayHurtSound;
        GameAudio.StopOwned(this);
    }

    private void PlayHurtSound(float damage)
    {
        // Skip fatal-hit vocals and limit rapid gunfire to one hurt sound per cooldown.
        if (health != null && health.IsAlive && Time.time >= nextHurtTime)
        {
            GameAudio.StopOwned(this);
            GameAudio.Play(AudioCue.ZombieHurt, transform.position, true, owner: this);
            nextHurtTime = Time.time + 0.45f;
            nextVocalTime = Time.time + 5f;
        }
    }

    void UnawareState()
    {
        attackTimer = 0;
        agent.isStopped = true;
        agent.ResetPath();

        if (stealthMechanics)
        {
            agent.speed = walkSpeed;
            agent.isStopped = false;

            animator.SetBool("isSuspiciousState", false);
            animator.SetBool("isDetectedState", false);
        }
        if (PlayerInView())
        {
            currentState = ZombieState.Suspicious;
        }
    }

    void SuspiciousState()
    {
        attackTimer = 0;

        if (stealthMechanics)
        {
            agent.speed = walkSpeed;
            agent.isStopped = false;

            animator.SetBool("isSuspiciousState", true);
            animator.SetBool("isDetectedState", false);

            if (!PlayerInView())
            {
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    currentState = ZombieState.Unaware;
                }

                return;
            }

            agent.SetDestination(player.transform.position);

            if (PlayerInDetectionRange())
            {
                currentState = ZombieState.Detected;
            }
        }
        else
        {
            agent.speed = chaseSpeed;
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);

            animator.SetBool("isSuspiciousState", false);
            animator.SetBool("isDetectedState", true);

            if (PlayerInAttackRange())
            {
                currentState = ZombieState.Attacking;
            }
            else
            {
                currentState = ZombieState.Detected;
            }
        }
    }

    void DetectedState()
    {
        //Debug.Log("Detected");
        attackTimer = 0;
        if (!hordeAggroed)
        {
            AggroHorde();
            hordeAggroed = true;
        }
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(player.transform.position);

        animator.SetBool("isSuspiciousState", false);
        animator.SetBool("isDetectedState", true);

        if (PlayerInAttackRange())
        {
            currentState = ZombieState.Attacking;
            return;
        }

        if (stealthMechanics)
        {
            if (!PlayerInView() && !PlayerInDetectionRange())
            {
                currentState = ZombieState.Suspicious;
            }
        }
    }

    void AttackingState()
    {
        RotateToPlayer();

        animator.SetBool("isSuspiciousState", false);
        animator.SetBool("isDetectedState", false);

        if (!attackInProgress && attackTimer <= 0)
        {
            Attack();
            attackInProgress = true;
            attackTimer = attackInterval;
        }

        if (!attackInProgress)
        {
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }

            if (!PlayerInAttackRange())
            {
                attackTimer = 0;
                currentState = ZombieState.Detected;
            }
        }
    }

    void Attack()
    {
        GameAudio.StopOwned(this);
        GameAudio.PlayZombieAttack(assignedAttackVoice, ref previousAttackClip, transform.position, this);
        nextVocalTime = Time.time + Random.Range(4f, 7f);
        animator.SetTrigger("Attack");
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null) playerHealth.TakeDamage(damage);
    }

    void RotateToPlayer()
    {
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    bool PlayerInView()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        if (distanceToPlayer > viewDist)
        {
            return false;
        }

        directionToPlayer.Normalize();
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle / 2)
        {
            return false;
        }

        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, viewDist, obstructionMask))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    bool PlayerInDetectionRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position );
        return distanceToPlayer <= detectionDist;
    }

    bool PlayerInAttackRange()
    {
        float distanceToPlayer =  Vector3.Distance(transform.position, player.transform.position);
        return distanceToPlayer <= attackRange;
    }

    public void AttackAnimationFinished()
    {
        attackInProgress = false;
    }

    public void Death()
    {
        if(!isDead)
        {
            GameAudio.StopOwned(this);
            // No owner: let the body-fall sound finish even if this zombie is disabled.
            GameAudio.Play(AudioCue.ZombieDeath, transform.position, true);
            agent.enabled = false;
            animator.SetTrigger("isDead");
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
            isDead = true;
        }
    }

    public void TakeDamage()
    {
        //make zombie immortal, by restoring health every hit
        if (stealthMechanics)
        {
            health.RestoreFullHealth();
        }
        currentState = ZombieState.Detected;
    }

    void AggroHorde()
    {
        Collider[] zombies = Physics.OverlapSphere(transform.position, aggroRange);

        foreach (Collider collider in zombies)
        {
            Zombie zombie = collider.GetComponentInParent<Zombie>();
            if (zombie != null && zombie != this && !zombie.isDead)
            {
                zombie.currentState = ZombieState.Detected;
            }
        }
    }

    public void SetZombieState(ZombieState state)
    {
        currentState = state;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}
