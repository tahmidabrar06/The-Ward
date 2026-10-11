using UnityEngine;

namespace TheWard.Combat
{
    public class FirstAid : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameObject player;

    [SerializeField] private float healValue = 25f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        // get health
        Health health = other.GetComponentInParent<Health>();
        Debug.Log("Collided with first aid");
        
        // 
        if (health == null || !health.IsAlive)
            {
                return;
            }
        
        // if player is at full health, don't destroy the first aid
        if (health.CurrentHealth >= health.MaxHealth)
            {
                return;
            }
        
        health.HealPlayer(healValue);
        Destroy(gameObject); 
    }
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
    }
}
}

