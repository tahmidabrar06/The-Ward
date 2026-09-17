using System;
using NUnit.Framework.Constraints;
using TheWard.Combat;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    //todo:
    //Add sound
    //Add attack event at the end of attack animation
    
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
    private bool attackInProgress;
    private bool isDead;

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
        currentState = ZombieState.Unaware;
    }


    void Update()
    {
        
        if(!isDead)
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
    
        //Debug.Log(currentState);
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
        attackTimer = 0;

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
        // Attack code
        animator.SetTrigger("Attack"); //need to add attack event at the end of zombie attack animation
        Health playerHealth = player.GetComponent<Health>();
        playerHealth.TakeDamage(damage);
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

    //to check if attacking is finished
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
            isDead = true;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (!stealthMechanics)
        {
            
        }
        currentState = ZombieState.Detected;
    }
}