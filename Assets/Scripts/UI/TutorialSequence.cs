using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private string movePrompt = "Use [W A S D] to Move";
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private string shootPrompt = "Press [Left Click] to Shoot";
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private string sprintPromt = "Press [Left Shift] to Sprint";
    [SerializeField] private InputActionReference sprintAction;


    private int currentStep = 0;

    public void BeginSequence()
    {
        currentStep = 1;
        TutorialManager.Instance.ShowPrompt(movePrompt);
    }

    private void OnEnable()
    {
        moveAction?.action.Enable();
        shootAction?.action.Enable();
    }

    private void Update()
    {
        if (currentStep == 1)
        {
            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
            if (moveInput.magnitude > 0.1f)
            {
                currentStep = 2;
                TutorialManager.Instance.ShowPrompt(shootPrompt);
            }
        }
        else if (currentStep == 2)
        {
            if (shootAction.action.WasPressedThisFrame())
            {
                currentStep = 3;
                TutorialManager.Instance.ShowPrompt(sprintPromt);
            }
        }
        else if (currentStep == 3)
        {
            if (sprintAction.action.WasPressedThisFrame())
            {
                currentStep = 0;   
                TutorialManager.Instance.HidePrompt();
                Destroy(this); 
            }
        }
    }
}