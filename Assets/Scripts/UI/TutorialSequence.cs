using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference torchAction;

    private int currentStep = 0;

    public void BeginSequence()
    {
        currentStep = 1;
        TutorialManager.Instance.ShowPrompt("Use [W A S D] to Move");
    }

    private void OnEnable()
    {
        // Make sure all input actions are enabled so Unity can read them
        moveAction?.action.Enable();
        shootAction?.action.Enable();
        sprintAction?.action.Enable();
        torchAction?.action.Enable();
    }

    private void Update()
    {
        if (currentStep == 1)
        {
            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
            if (moveInput.magnitude > 0.1f)
            {
                currentStep = 2;
                TutorialManager.Instance.ShowPrompt("Press [Left Click] to Shoot");
            }
        }
        else if (currentStep == 2)
        {
            if (shootAction.action.WasPressedThisFrame())
            {
                currentStep = 3;
                TutorialManager.Instance.ShowPrompt("Press [Left Shift] to Sprint");
            }
        }
        else if (currentStep == 3)
        {
            if (sprintAction.action.WasPressedThisFrame())
            {
                currentStep = 4;   
                TutorialManager.Instance.ShowPrompt("Press [1 - 5] to Switch Weapons");
            }
        }
        else if (currentStep == 4)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (
                    keyboard.digit1Key.wasPressedThisFrame ||
                    keyboard.digit2Key.wasPressedThisFrame ||
                    keyboard.digit3Key.wasPressedThisFrame ||
                    keyboard.digit4Key.wasPressedThisFrame ||
                    keyboard.digit5Key.wasPressedThisFrame))
            {
                currentStep = 5;
                TutorialManager.Instance.ShowPrompt("Press [T] to Toggle Torch");
            }
        }
        else if (currentStep == 5) 
        {
            if (torchAction.action.WasPressedThisFrame()){ 
                currentStep = 0;   
                TutorialManager.Instance.HidePrompt();
                
                Destroy(this); 
            }
        }
    }
}