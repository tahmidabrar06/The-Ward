using UnityEngine;
using TMPro;

public class TallyMarker : MonoBehaviour
{
    public static TallyMarker Instance { get; private set; }

    [Header("Wave HUD")]
    [SerializeField] private GameObject waveHUDContainer;
    [SerializeField] private GameObject[] tallyMarks;
    [SerializeField] private TMP_Text ZombieCountText;

    private int currentTallyIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void UpdateZombieCount(int remainingZombies)
    {
        if (ZombieCountText != null)
        {
            ZombieCountText.text = remainingZombies.ToString();
        }
    }

    public void AddTally()
    {
        if (currentTallyIndex < tallyMarks.Length)
        {
            if (tallyMarks[currentTallyIndex] != null) 
            {
                tallyMarks[currentTallyIndex].SetActive(true);
            }
            currentTallyIndex++; 
        }
    }

    public void ShowHUD()
    {
        if (waveHUDContainer != null) waveHUDContainer.SetActive(true);
        
        foreach (GameObject mark in tallyMarks)
        {
            if (mark != null) mark.SetActive(false);
        }
    }
    
    public void HideHUD()
    {
        if (waveHUDContainer != null) waveHUDContainer.SetActive(false);
    }
}