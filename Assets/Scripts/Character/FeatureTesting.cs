using UnityEngine;
using UnityEngine.InputSystem;
using TheWard.Combat;

public class FeatureTesting : MonoBehaviour
{
    private Health playerHealth;
    
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private Transform spawnPoint;

    private void Awake()
    {
        playerHealth = GetComponent<Health>();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame && playerHealth != null)
        {
            playerHealth.TakeDamage(10f);
        }
        
        if (Keyboard.current != null && Keyboard.current.yKey.wasPressedThisFrame)
        {
            if (zombiePrefab != null)
            {
                Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position + (transform.forward * 2f);
                Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

                Instantiate(zombiePrefab, spawnPosition, spawnRotation);
            }
            else
            {
                Debug.LogWarning("Zombie Prefab missing");
            }
        }
    }
}