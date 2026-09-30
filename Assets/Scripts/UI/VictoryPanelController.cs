using UnityEngine;

public class VictoryPanelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIManager uiManager;

    public void PlayAgain()
    {
        uiManager.PlayAgain();
    }

    public void MainMenu()
    {
        uiManager.MainMenu();
    }

    public void QuitGame()
    {
        uiManager.QuitGame();
    }
}