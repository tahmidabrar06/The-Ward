using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;

    [SerializeField] private TMP_Text volumeValueText;
    [SerializeField] private TMP_Text sensitivityValueText;

    [SerializeField] private TMP_Text savedMessage;

    public void PlayGame()
    {
        SceneManager.LoadScene("Floor1");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);

        UpdateSettingsText();
    }

    public void BackToMainMenu()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivitySlider.value);

        PlayerPrefs.Save();

        UpdateSettingsText();

        if (savedMessage != null)
        {
            savedMessage.gameObject.SetActive(true);

            CancelInvoke(nameof(HideSavedMessage));
            Invoke(nameof(HideSavedMessage), 2f);
        }

        Debug.Log("Settings saved.");
    }

    private void HideSavedMessage()
{
    if (savedMessage != null)
    {
        savedMessage.gameObject.SetActive(false);
    }
}

    public void UpdateSettingsText()
    {
        if (volumeValueText != null)
        {
            volumeValueText.text =
                $"{Mathf.RoundToInt(volumeSlider.value * 100f)}%";
        }

        if (sensitivityValueText != null)
        {
            sensitivityValueText.text =
                sensitivitySlider.value.ToString("0.0");
        }
    }
}