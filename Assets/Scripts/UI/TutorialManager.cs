using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject tutorialPanel; 
    [SerializeField] private TMP_Text tutorialText;
	[SerializeField] private TMP_Text BigtutorialText;
    [SerializeField] private TutorialSequence tutorialSequence;
    
    public bool IsSequenceActive { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
		if (tutorialPanel != null) tutorialPanel.SetActive(true);
		HideBigPrompt();
		HidePrompt();
    }
    
    public void ShowPrompt(string message)
    {
        if (tutorialText != null) 
        {
			tutorialText.text = message;	
            tutorialText.gameObject.SetActive(true); 
        }
    }

    public void HidePrompt()
    {
        if (tutorialText != null) tutorialText.gameObject.SetActive(false); 
    }

    public void ShowBigPrompt(string message)
    {
        if (BigtutorialText != null) 
        {
			BigtutorialText.text = message;
            BigtutorialText.gameObject.SetActive(true); 
        }
    }

    public void HideBigPrompt() // instantly remover text
    {
        if (BigtutorialText != null) BigtutorialText.gameObject.SetActive(false); 
    }

	public void RemoveBigPrompt() // Smoothly remove text
	{
		if (BigtutorialText != null) 
        {
            TypewriterEffect typewriter = BigtutorialText.GetComponent<TypewriterEffect>();
            typewriter.StartRemovingText(); 
        }
	}

    public void BeginTutorialSequence()
    {
        if (tutorialSequence != null)
        {
            IsSequenceActive = true; 
            tutorialSequence.BeginSequence();
        }
    }

    public void EndTutorialSequence()
    {
        IsSequenceActive = false; 
    }
}