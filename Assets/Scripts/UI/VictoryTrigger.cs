using UnityEngine;

public class VictoryTrigger : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            if(uiManager != null) uiManager.ShowVictoryScreen();
        }
    }
}