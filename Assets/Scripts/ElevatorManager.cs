
using UnityEngine;
using UnityEngine.SceneManagement;
using TheWard.Combat;

public class ElevatorManager : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private string nextFloorScene = "Floor2";
    [SerializeField] private UIManager uiManager;

    [Header("Inventory Persistence")]
    [SerializeField] private WeaponController weaponController;

    private bool hasEntered = false;

    private void Awake()
    {
        // Find the controller automatically if it wasn't assigned.
        if (weaponController == null)
        {
            weaponController =
                FindFirstObjectByType<WeaponController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasEntered)
            return;

        if (other.CompareTag("Player"))
        {
            hasEntered = true;

            if (uiManager != null &&
                SceneManager.GetActiveScene().name != "Floor3")
            {
                uiManager.ShowFloorVictoryScreen(this);
            }
            else if (uiManager != null &&
                     SceneManager.GetActiveScene().name == "Floor3")
            {
                uiManager.ShowVictoryScreen();
            }

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.HidePrompt();
            }
        }
    }

    public void GoToNextFloor()
    {
        if (weaponController == null)
        {
            Debug.LogError(
                "ElevatorManager: WeaponController not found. " +
                "Cannot save the inventory before changing scenes.",
                this);
            return;
        }

        // Save inventory BEFORE unloading the current scene.
        weaponController.SaveInventoryForSceneChange();

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextFloorScene);
    }
}
