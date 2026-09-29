using UnityEngine;

public class TestDeathTrigger : MonoBehaviour
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

            uiManager.ShowRespawnScreen();
        }
    }
}