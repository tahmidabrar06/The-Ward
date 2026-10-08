using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerDoorArea : MonoBehaviour
{
    [SerializeField] private RotateDoor door;
    [SerializeField] private InputActionReference pressKey;
    [SerializeField] private float closeDelay = 1.5f;

    // check if the player ahs triggered the cube
    private readonly HashSet<Collider> playerColliders = new HashSet<Collider>();
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
        // The action asset is shared; another trigger may still be using this action.
        playerColliders.Clear();
        StopAllCoroutines();
        closeRoutine = null;
    }

    void Update()
    {
        // no F pressed, but the player is in the triggered area
        playerColliders.RemoveWhere(collider => collider == null || !collider.enabled ||
            !collider.gameObject.activeInHierarchy);
        if (Time.timeScale <= 0f || playerColliders.Count == 0 || pressKey == null ||
            pressKey.action == null || door == null)
        {
            // Debug.Log("F not pressed");
            return;
        }

        // press F,  open door
        if (pressKey.action.WasPressedThisFrame() && !door.IsOpen)
        {
            
            // Debug.Log("Pressed F - Opened door");
            door.Open();
            TutorialManager.Instance.HidePrompt();
        }
    }

    private static bool IsPlayer(Collider other)
    {
        Transform current = other.transform;
        while (current != null)
        {
            if (current.CompareTag("Player")) return true;
            current = current.parent;
        }
        return false;
    }

    private void RegisterPlayer(Collider other)
    {
        if (!IsPlayer(other)) return;
        playerColliders.Add(other);
        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null;
        }
        
        if (!door.IsOpen && !TutorialManager.Instance.IsSequenceActive)
        {
            TutorialManager.Instance.ShowPrompt("Press [F] to Open Door");
        }
    }

    void OnTriggerEnter(Collider other) => RegisterPlayer(other);

    // Recover occupancy after script reload or enabling while already inside the trigger.
    void OnTriggerStay(Collider other) => RegisterPlayer(other);

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        playerColliders.Remove(other);
        if (playerColliders.Count == 0 && door != null && door.IsOpen && closeRoutine == null)
            closeRoutine = StartCoroutine(CloseAfterDelay());
        
        if (!TutorialManager.Instance.IsSequenceActive) TutorialManager.Instance.HidePrompt();
    }

    // close door
    IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        if (door != null && playerColliders.Count == 0) door.Close();
        closeRoutine = null;
    }
}