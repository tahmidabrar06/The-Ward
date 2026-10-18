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

        // collide, add to inventory, option to heal
        PlayerHeal healInventory = other.GetComponentInParent<PlayerHeal>(); // get from player
        if(healInventory == null)
            {
                Debug.Log("No health kit in the inventory");
                return;
            }
        if (healInventory.addFirstAidKit())
            {
                Debug.Log("Collided with first aid.");
                Destroy(gameObject);
            }

        else
        {
            Debug.Log("Health kit inventory is full.");
        }

        // collide then heal
        // Health health = other.GetComponentInParent<Health>();
        // Debug.Log("Collided with first aid");
        
        // if (health == null || !health.IsAlive)
        //     {
        //         return;
        //     }
        
        // // if player is at full health, don't destroy the first aid
        // if (health.CurrentHealth >= health.MaxHealth)
        //     {
        //         return;
        //     }
        
        // health.HealPlayer(healValue);
        // Destroy(gameObject); 

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

