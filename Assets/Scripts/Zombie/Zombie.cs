using TheWard.Combat;
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

    private Health health;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        //currentState = ZombieState.Unaware;
    }

    void Update()
    {

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
