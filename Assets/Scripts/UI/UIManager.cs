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

    private Transform playerTransform;
    private Transform cameraTransform;

    private Vector3 savedPlayerPosition;
    private Quaternion savedPlayerRotation;
    private Quaternion savedCameraRotation;

    private void Start()
    {
        if (playerMovement != null)
            playerTransform = playerMovement.transform;

        if (cameraMovement != null)
            cameraTransform = cameraMovement.transform;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (respawnPanel != null)
            respawnPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        EnableGameplay();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (respawnPanel != null && respawnPanel.activeSelf)
                return;

            if (victoryPanel != null && victoryPanel.activeSelf)
                return;

            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        if (isPaused)
            FreezeTransforms();
    }

    // =========================
    // PAUSE
    // =========================

    public void PauseGame()
    {
        isPaused = true;

        if (playerTransform != null)
        {
            savedPlayerPosition = playerTransform.position;
            savedPlayerRotation = playerTransform.rotation;
        }

        if (cameraTransform != null)
        {
            savedCameraRotation = cameraTransform.rotation;
        }

        if (pausePanel != null)
            pausePanel.SetActive(true);

        FreezeGameplay();
    }

    // =========================
    // RESUME
    // =========================

    public void ResumeGame()
    {
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

        if (respawnPanel != null)
            respawnPanel.SetActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        FreezeGameplay();
    }

    public void Replay()
    {
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
        isPaused = true;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (respawnPanel != null)
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

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (cameraMovement != null)
            cameraMovement.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        FreezeTransforms();
    }

    private void EnableGameplay()
    {
        Time.timeScale = 1f;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (cameraMovement != null)
            cameraMovement.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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