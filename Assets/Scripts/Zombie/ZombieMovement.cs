using System;
using UnityEngine;
using UnityEngine.AI;

public class ZombieMovement : MonoBehaviour
{
    public bool stealthMechanics = false;
    private GameObject player;
    private NavMeshAgent agent;
    public bool isDetected;

    [SerializeField]
    private float walkSpeed, chaseSpeed;

    [SerializeField]
    private float attackRange, detectionDist, viewDist, viewAngle;
    public LayerMask obstructionMask;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = gameObject.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ZombieCurrentState() == ZombieStates.Attacking && isDetected)
        {
            Attack();
        }
        else
        {
            Movement();
        }        
    }

    void Movement()
    {
        if (stealthMechanics)
        {
            //set movement speed to walk speed if unaware
            if(ZombieCurrentState() == ZombieStates.Unaware)
            {
                //temp
                gameObject.GetComponent<Renderer>().material.color = Color.blue;
                //temp
                //isDetected = false on losing the player
                isDetected = false;
                agent.speed = walkSpeed;
                return;
            }
            //move the zombie towards player in walkspeed
            if(ZombieCurrentState() == ZombieStates.Suspicious)
            {
                //temp
                gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                //Debug.Log("investigating"); 
                //temp
                agent.SetDestination(player.transform.position);
            }
            //on player detection make isDetected=True
            if(ZombieCurrentState() == ZombieStates.Detected)
            {
                isDetected = true;
                agent.isStopped = false;
            }
            //chase player in runspeed until zombie becomes unaware of player location
            if (isDetected)
            {
                //temp
                gameObject.GetComponent<Renderer>().material.color = Color.red;
                //temp
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
                //temp
                gameObject.GetComponent<Renderer>().material.color = Color.red;
                                //temp
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                agent.SetDestination(player.transform.position);
            }
        }

    }
    void Attack()
    {
        //attack code
    
        //temp
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
        
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);

        gameObject.GetComponent<Renderer>().material.color = Color.black;
        //temp
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
