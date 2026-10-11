using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorManager : MonoBehaviour
{
    [SerializeField] private string nextFloorScene = "Floor2";
    [SerializeField] private UIManager uiManager;

    private bool hasEntered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasEntered)
            return;

        if (other.CompareTag("Player"))
        {
            hasEntered = true;

            if (uiManager != null)
            {
                uiManager.ShowFloorVictoryScreen(this);
            }

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.HidePrompt();
            }
        }
    }

    public void GoToNextFloor()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextFloorScene);
    }
}