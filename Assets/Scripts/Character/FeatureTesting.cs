using UnityEngine;
using UnityEngine.InputSystem;
using TheWard.Combat;

public class FeatureTesting : MonoBehaviour
{
    private Health playerHealth;

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
    }
}