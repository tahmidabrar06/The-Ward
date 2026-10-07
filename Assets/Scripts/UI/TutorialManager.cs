using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private UIManager uiManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    public void ShowPrompt(string message)
    {
        if (uiManager != null)
        {
            uiManager.ShowTutorialPrompt(message);
        }
    }

    public void HidePrompt()
    {
        if (uiManager != null)
        {
            uiManager.HideTutorialPrompt();
        }
    }
}