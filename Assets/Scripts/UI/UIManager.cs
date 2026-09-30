using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject respawnPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Player Control")]
    [SerializeField] private FirstPerson playerMovement;
    [SerializeField] private CameraMovement cameraMovement;

    private bool isPaused = false;
    private bool isVictory = false;

    private Transform playerTransform;
    private Transform cameraTransform;

    private Vector3 savedPlayerPosition;
    private Quaternion savedPlayerRotation;
    private Quaternion savedCameraRotation;

    private void Start()
    {
        // Get player transform
        if (playerMovement != null)
            playerTransform = playerMovement.transform;

        // Get camera transform
        if (cameraMovement != null)
            cameraTransform = cameraMovement.transform;

        // Hide panels at start
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (respawnPanel != null)
            respawnPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        // Make sure the game starts normally
        Time.timeScale = 1f;

        isPaused = false;
        isVictory = false;

        EnableGameplay();
    }

    private void Update()
    {
        // Do nothing while victory screen is active
        if (isVictory)
        {
            FreezeTransforms();
            return;
        }

        // Pause / Resume with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (respawnPanel != null && respawnPanel.activeSelf)
                return;

            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        // Keep player and camera frozen while paused
        if (isPaused)
        {
            FreezeTransforms();
        }
    }

    // =========================
    // PAUSE
    // =========================

    public void PauseGame()
    {
        if (isVictory)
            return;

        isPaused = true;

        SaveCurrentTransforms();

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (respawnPanel != null)
            respawnPanel.SetActive(false);

        FreezeGameplay();
    }

    // =========================
    // RESUME
    // =========================

    public void ResumeGame()
    {
        if (isVictory)
            return;

        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        EnableGameplay();
    }

    // =========================
    // DEATH / RESPAWN
    // =========================

    public void ShowRespawnScreen()
    {
        isPaused = true;

        SaveCurrentTransforms();

        if (respawnPanel != null)
            respawnPanel.SetActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        FreezeGameplay();
    }

    public void Respawn()
    {
    // Restart the current floor
    Time.timeScale = 1f;

    SceneManager.LoadScene(
        SceneManager.GetActiveScene().name
    );
    }

    // =========================
    // VICTORY
    // =========================

    public void ShowVictoryScreen()
    {
        // Mark the game as finished
        isVictory = true;
        isPaused = false;

        // Save the exact position and rotation
        SaveCurrentTransforms();

        // Show victory screen
        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        // Hide other screens
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (respawnPanel != null)
            respawnPanel.SetActive(false);

        // Completely freeze gameplay
        FreezeGameplay();

        Debug.Log("Victory screen shown. Game frozen.");
    }

    public void PlayAgain()
    {
    // Restart the game from Floor 1
    Time.timeScale = 1f;

    SceneManager.LoadScene("Floor1");
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
        // Freeze Unity game time
        Time.timeScale = 0f;

        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Disable camera movement
        if (cameraMovement != null)
        {
            cameraMovement.enabled = false;
        }

        // Unlock cursor for UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Force player and camera to stay still
        FreezeTransforms();
    }

    private void EnableGameplay()
    {
        Time.timeScale = 1f;

        // Enable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Enable camera movement
        if (cameraMovement != null)
        {
            cameraMovement.enabled = true;
        }

        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // =========================
    // TRANSFORM CONTROL
    // =========================

    private void SaveCurrentTransforms()
    {
        if (playerTransform != null)
        {
            savedPlayerPosition = playerTransform.position;
            savedPlayerRotation = playerTransform.rotation;
        }

        if (cameraTransform != null)
        {
            savedCameraRotation = cameraTransform.rotation;
        }
    }

    private void FreezeTransforms()
    {
        if (playerTransform != null)
        {
            playerTransform.position = savedPlayerPosition;
            playerTransform.rotation = savedPlayerRotation;
        }

        if (cameraTransform != null)
        {
            cameraTransform.rotation = savedCameraRotation;
        }
    }
}