using TheWard.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHeal : MonoBehaviour
{
    // intialise empty
    private int kitAmount = 0;
    public int actualAmount => kitAmount;

    [SerializeField] private Health health;
    [SerializeField] private InputActionReference healWithKitInput;
    [SerializeField] private float healValue = 25f;
    [SerializeField] private int maxKits = 3;

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }
    }

    private void OnEnable()
    {
        if (healWithKitInput != null)
        {
            healWithKitInput.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (healWithKitInput != null)
        {
            healWithKitInput.action.Disable();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (healWithKitInput == null)
        {
            return;
        }

        if (healWithKitInput.action.WasPressedThisFrame())
        {
            UseHealthKit();
        }
    }


    public bool addFirstAidKit()
    {
        if (kitAmount >= maxKits)
        {
            return false;
        }

        kitAmount++;
        Debug.Log($"Health kits amount in inventory: {kitAmount}");
        return true;
    }

    public void UseHealthKit()
    {
        if (kitAmount <= 0 || health == null || !health.IsAlive)
        {
            return;
        }

        if (health.CurrentHealth >= health.MaxHealth)
        {
            Debug.Log("Health is already full");
            return;
        }

        health.HealPlayer(healValue);
        kitAmount--;
        Debug.Log("Pressed [H] to Heal");

        Debug.Log($"Health kits remianing after use: {kitAmount}");
    }
}
