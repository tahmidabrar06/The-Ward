using UnityEngine;

public class FirstAidSpawner : MonoBehaviour
{
    private bool hasSpawned = false;
    [SerializeField] private GameObject firstaidPrefab;
    [SerializeField] private Transform waveDefenseTrigger;
    [SerializeField] private Transform[] spawnPoints;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        // has the kits spawned
        if (hasSpawned || waveDefenseTrigger == null)
        {
            return;
        }
        // get collider
        SphereCollider triggerCollider = waveDefenseTrigger.GetComponent<SphereCollider>();

        if (triggerCollider == null)
        {
            return;
        }
            
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return;
        }
        // get radius of wavedefencetrigger
        float triggerRadius = triggerCollider.radius;

        // if the player is near the wave trigger, spawn the health kits -- separate from event manager
        if (Vector3.Distance(player.transform.position, waveDefenseTrigger.position) <= triggerRadius)
        {
            SpawnKits();
        }
    }

    private void SpawnKits()
    {
        if (firstaidPrefab == null)
        {
            // Debug.Log("No kit -- add kit to field");
            return;
        }
        hasSpawned = true;
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null)
                continue;

            Instantiate(firstaidPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("Kits in spawn areas");



            //
        }
    }
}
