using TheWard.Character;
using UnityEngine;
using UnityEngine.SceneManagement;
using TheWard.Combat;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private WakeUpIntro wakeUpIntro;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject respawnPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject floorVictoryPanel;

    [Header("Player Control")]
    [SerializeField] private PlayerCharacterController playerMovement;
    [SerializeField] private PlayerLookController cameraMovement;
    [SerializeField] private WeaponController weaponController;

    private bool isPaused = false;
    private bool isVictory = false;
    private ElevatorManager currentElevator;

    private static bool hasSeenIntro = false;
    
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

        if (introPanel != null && !hasSeenIntro)
        {
            introPanel.SetActive(true);
            SaveCurrentTransforms();
            FreezeGameplay();
            if (wakeUpIntro != null)
            {
                Camera playerCamera = playerMovement != null
                    ? playerMovement.GetComponentInChildren<Camera>(true) : Camera.main;
                wakeUpIntro.Play(this, introPanel, playerCamera);
            }
        }
        else
        {
            if (introPanel != null) introPanel.SetActive(false);
            EnableGameplay();
        }    
    }

    private void Update()
    {
        if (wakeUpIntro != null && (wakeUpIntro.IsPlaying || wakeUpIntro.FinishedThisFrame)) return;
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
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
            }
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

        HideAllPanels();

        // Completely freeze gameplay
        FreezeGameplay();

        Debug.Log("Victory screen shown. Game frozen.");
    }

    public void ShowFloorVictoryScreen(ElevatorManager elevator)
    {
        isVictory = true;
        isPaused = false;

        currentElevator = elevator;

        SaveCurrentTransforms();

        // Show FLOOR victory panel
        if (floorVictoryPanel != null)
            floorVictoryPanel.SetActive(true);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        HideAllPanels();
        // Freeze gameplay
        FreezeGameplay();

        Debug.Log("Floor victory screen shown.");
    }

    public void ContinueToNextFloor()
    {
        if (currentElevator != null)
        {
            currentElevator.GoToNextFloor();
        }
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
        hasSeenIntro = false;
        SceneManager.LoadScene("MainMenu");
    }
    
    // =========================
    // Intro SCREEN
    // =========================

    public void StartGameAfterIntro()
    {
        if (wakeUpIntro != null && wakeUpIntro.IsPlaying)
        {
            wakeUpIntro.Skip();
            return;
        }
        hasSeenIntro = true;

        if (introPanel != null)
            introPanel.SetActive(false);

        EnableGameplay();
        
        TutorialManager.Instance?.BeginTutorialSequence();
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
        
        // Disable attack and reload actions. 
        if (weaponController != null) weaponController.enabled = false;

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
        if (weaponController != null) weaponController.enabled = true;

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

    void HideAllPanels()
    {
        // Hide other screens
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (respawnPanel != null)
            respawnPanel.SetActive(false);

        GameObject inventoryPanel = GameObject.Find("InventoryPanel");
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        
        GameObject ammoPanel = GameObject.Find("Ammo");
        if (ammoPanel != null)
            ammoPanel.SetActive(false);
    }
}