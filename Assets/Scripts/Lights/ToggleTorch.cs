using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleTorch : MonoBehaviour
{
    [SerializeField] private Light torch;
    [SerializeField] private InputActionReference toggleInput;
    [SerializeField] private bool startOn = false;

    void Start()
    {
        // start off
        if (torch != null)
        {
            torch.enabled = startOn;
        }
    }

    void OnEnable()
    {
        if (toggleInput != null)
        {
            toggleInput.action.Enable();
        }
    }

    void OnDisable()
    {
        if (toggleInput != null)
        {
            toggleInput.action.Disable();
        }
    }

    void Update()
    {
        if (toggleInput == null || torch == null)
        {
            return;
        }

        if (toggleInput.action.WasPressedThisFrame())
        {
            torch.enabled = !torch.enabled;
        }
    }
}