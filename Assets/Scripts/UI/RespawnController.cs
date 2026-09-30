using UnityEngine;

public class RespawnController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIManager uiManager;

    public void Respawn()
    {
        uiManager.Respawn();
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