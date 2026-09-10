using System;
using UnityEngine;
using UnityEngine.AI;

public class ZombieMovement : MonoBehaviour
{
    public bool stealthMechanics = false;
    private GameObject player;
    private NavMeshAgent agent;
    public bool isDetected;
    public bool isAttacking;

    [SerializeField]
    private float walkSpeed, chaseSpeed;

    [SerializeField]
    private float attackRange, detectionDist, viewDist, viewAngle;
    [SerializeField]
    private float attackInterval;
    private float attackTimer;
    public LayerMask obstructionMask;
    
    [SerializeField]
    private Animator animator;
    int stealthLayer;
    int normalLayer;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = gameObject.GetComponent<NavMeshAgent>();

        if (stealthMechanics)
        {
            animator.SetLayerWeight(stealthLayer, 1f);
            animator.SetLayerWeight(normalLayer, 0f);
        }
        else
        {
            animator.SetLayerWeight(stealthLayer, 0f);
            animator.SetLayerWeight(normalLayer, 1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ZombieCurrentState() == ZombieStates.Attacking && isDetected)
        {
            RotateToPlayer();
            if(attackTimer <= 0)
            {
                Attack();
                attackTimer = attackInterval;
            }
            else
            {
                attackTimer -= Time.deltaTime;
            }
        }
        else
        {
            if(!isAttacking)
            {
                attackTimer = 0;
                Movement();
            }
        }        
        Debug.Log(ZombieCurrentState());
    }

    void Movement()
    {
        if (stealthMechanics)
        {
            //set movement speed to walk speed if unaware
            if(ZombieCurrentState() == ZombieStates.Unaware)
            {
                isDetected = false;
                agent.speed = walkSpeed;
                animator.SetBool("isSuspiciousState", false);
                animator.SetBool("isDetectedState", false);
                return;
            }
            //move the zombie towards player in walkspeed
            if(ZombieCurrentState() == ZombieStates.Suspicious)
            {
                agent.SetDestination(player.transform.position);
                animator.SetBool("isSuspiciousState", true);
                animator.SetBool("isDetectedState", false);
            }
            //on player detection make isDetected=True
            if(ZombieCurrentState() == ZombieStates.Detected)
            {
                isDetected = true;
                agent.isStopped = false;
                animator.SetBool("isSuspiciousState", false);
                animator.SetBool("isDetectedState", true);
            }
            //chase player in runspeed until zombie becomes unaware of player location
            if (isDetected)
            {
                agent.speed = chaseSpeed;
                agent.SetDestination(player.transform.position);
            }
        }
        else
        {
            //initiate chase permanently if player in suspicious distance
            if(ZombieCurrentState() == ZombieStates.Suspicious)
            {
                isDetected = true;
            }
            if (isDetected)
            {
                if(ZombieCurrentState() != ZombieStates.Attacking)
                {
                    agent.speed = chaseSpeed;
                    agent.isStopped = false;
                    agent.SetDestination(player.transform.position); 
                    animator.SetBool("isDetectedState", true);
                }            
            }
        }

    }
    void Attack()
    {
        //attack code
        animator.SetBool("isSuspiciousState", false);
        animator.SetBool("isDetectedState", false);
        animator.SetTrigger("Attack");
        //gameObject.GetComponent<Renderer>().material.color = Color.black;
    }
    void RotateToPlayer()
    {
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public enum ZombieStates
    {
        Unaware,
        Suspicious,
        Detected,
        Attacking
    }
    
    //Player detection logic
    ZombieStates ZombieCurrentState()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDist)
        {
            return ZombieStates.Unaware;
        }

        directionToPlayer.Normalize();
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        
        if (angle > viewAngle / 2)
        {
            return ZombieStates.Unaware;
        }
    
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, viewDist, obstructionMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                if (distanceToPlayer <= detectionDist)
                {
                    if(distanceToPlayer <= attackRange)
                    {
                        return ZombieStates.Attacking;
                    }
                    return ZombieStates.Detected;
                }

                return ZombieStates.Suspicious;
            }
        }

        return ZombieStates.Unaware;
    }
}
