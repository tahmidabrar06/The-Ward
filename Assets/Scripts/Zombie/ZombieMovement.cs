using UnityEngine;
using UnityEngine.AI;

public class ZombieMovement : MonoBehaviour
{
    private GameObject player;
    private NavMeshAgent agent;
    public bool playerDetected = false;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = gameObject.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= 5)
        {
            playerDetected = true;
        }
        if (distanceToPlayer >= 1.5f && playerDetected == true)
        {
            agent.SetDestination(player.transform.position);
        }
        
    }
}
