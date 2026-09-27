using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject respawnPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Player Control")]
    [SerializeField] private FirstPerson playerMovement;
    [SerializeField] private CameraMovement cameraMovement;

    private bool isPaused = false;

    private void Start()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        respawnPanel.SetActive(false);
        victoryPanel.SetActive(false);

        Time.timeScale = 1f;

        EnableGameplay();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (respawnPanel.activeSelf || victoryPanel.activeSelf)
                return;

            if (settingsPanel.activeSelf)
            {
                CloseSettings();
                return;
            }

            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // =========================
    // PAUSE
    // =========================

    public void PauseGame()
    {
        isPaused = true;

        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);

        FreezeGameplay();
    }

    public void ResumeGame()
    {
        isPaused = false;

        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        EnableGameplay();
    }

    // =========================
    // SETTINGS
    // =========================

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);

        FreezeGameplay();
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);

        if (isPaused)
            pausePanel.SetActive(true);
        else
            EnableGameplay();
    }

    // =========================
    // DEATH / RESPAWN
    // =========================

    public void ShowRespawnScreen()
    {
        respawnPanel.SetActive(true);
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        victoryPanel.SetActive(false);

        FreezeGameplay();
    }

    public void Replay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // =========================
    // VICTORY
    // =========================

    public void ShowVictoryScreen()
    {
        victoryPanel.SetActive(true);
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        respawnPanel.SetActive(false);

        FreezeGameplay();
    }

    // =========================
    // MAIN MENU
    // =========================

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // =========================
    // QUIT
    // =========================

    public void QuitGame()
    {
        Debug.Log("Quitting game...");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // =========================
    // GAMEPLAY CONTROL
    // =========================

    private void FreezeGameplay()
    {
        Time.timeScale = 0f;

        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraMovement != null) cameraMovement.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void EnableGameplay()
    {
        Time.timeScale = 1f;

        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraMovement != null) cameraMovement.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}