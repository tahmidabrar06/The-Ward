using System;
using UnityEngine;
using UnityEngine.AI;

public class ZombieMovement : MonoBehaviour
{
    public bool stealthMechanics = false;
    private GameObject player;
    private NavMeshAgent agent;
    [SerializeField]
    private bool isDetected;

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
        float distToPlayer = Vector3.Distance(gameObject.transform.position, player.transform.position);
        if (distToPlayer <= attackRange)
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
            if(PlayerDetectionState() == DetectionState.Unaware)
            {
                //temp
                gameObject.GetComponent<Renderer>().material.color = Color.blue;
                //temp
                isDetected = false;
                agent.speed = walkSpeed;
                return;
            }
            if(PlayerDetectionState() == DetectionState.Suspicious)
            {
                //temp
                gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                Debug.Log("investigating"); 
                //temp
                agent.SetDestination(player.transform.position);
            }
            if(PlayerDetectionState() == DetectionState.Detected)
            {
                //temp
                gameObject.GetComponent<Renderer>().material.color = Color.red;
                //temp
                isDetected = true;
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                agent.SetDestination(player.transform.position);
            }
        }
        else
        {
            if(PlayerDetectionState() == DetectionState.Suspicious)
            {
                isDetected = true;
            }
            if (isDetected)
            {
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                agent.SetDestination(player.transform.position);
            }
        }

    }
    void Attack()
    {
        if(isDetected)
        {
            //attack code
        
            //temp
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            gameObject.GetComponent<Renderer>().material.color = Color.black;
            //temp
        }
    }

    public enum DetectionState
    {
        Unaware,
        Suspicious,
        Detected
    }
    DetectionState PlayerDetectionState()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDist)
        {
            return DetectionState.Unaware;
        }

        directionToPlayer.Normalize();
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        
        if (angle > viewAngle / 2)
        {
            return DetectionState.Unaware;
        }
    
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, viewDist, obstructionMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                if (distanceToPlayer <= detectionDist)
                {
                    return DetectionState.Detected;
                }

                return DetectionState.Suspicious;
            }
        }

        return DetectionState.Unaware;
    }
}
