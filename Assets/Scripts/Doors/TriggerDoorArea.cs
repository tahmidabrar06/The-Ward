using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerDoorArea : MonoBehaviour
{
    [SerializeField] private RotateDoor door;
    [SerializeField] private InputActionReference pressKey;
    [SerializeField] private float closeDelay = 1.5f;

    // check if the player ahs triggered the cube
    private bool playerTriggered;
    private Coroutine closeRoutine;

    // for the trigger: F is pressed
    void OnEnable()
    {
        if (pressKey != null)
        {
            pressKey.action.Enable();
        }
    }

    // for the trigger: F is no longer pressed
    void OnDisable()
    {
        if (pressKey != null)
        {
            pressKey.action.Disable();
        }
    }

    void Update()
    {
        // no F pressed, but the player is in the triggered area
        if (!playerTriggered || pressKey == null)
        {
            // Debug.Log("F not pressed");
            return;
        }
        if(TutorialManager.Instance.IsSequenceActive) return;

        // press F,  open door
        if (pressKey.action.WasPressedThisFrame() && !door.IsOpen)
        {
            // Debug.Log("Pressed F - Opened door");
            door.Open();
            
            // Close tutorial prompt
            TutorialManager.Instance.HidePrompt();
        }
    }

    // 
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        playerTriggered = true; // triggered the cube
        
        if(TutorialManager.Instance.IsSequenceActive) return;
        
        // Show tutorial prompt
        if (!door.IsOpen) TutorialManager.Instance.ShowPrompt("Press [F] to Open Door");
        
        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        playerTriggered = false;
        
        if(TutorialManager.Instance.IsSequenceActive) return;
        
        // Close tutorial prompt
        TutorialManager.Instance.HidePrompt();
        
        // close door when opened later (delay a bit)
        if (door.IsOpen)
        {
            closeRoutine = StartCoroutine(CloseAfterDelay());
        }
    }

    // close door
    IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        door.Close();
        closeRoutine = null;
    }
}