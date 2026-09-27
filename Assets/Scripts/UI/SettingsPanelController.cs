using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanelController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text volumeValueText;
    [SerializeField] private TMP_Text sensitivityValueText;
    [SerializeField] private TMP_Text savedMessage;

    private void OnEnable()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        UpdateSettingsText();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivitySlider.value);
        PlayerPrefs.Save();

        AudioListener.volume = volumeSlider.value;

        UpdateSettingsText();

        if (savedMessage != null)
        {
            savedMessage.gameObject.SetActive(true);
            CancelInvoke(nameof(HideSavedMessage));
            Invoke(nameof(HideSavedMessage), 2f);
        }
    }

    private void HideSavedMessage()
    {
        if (savedMessage != null)
            savedMessage.gameObject.SetActive(false);
    }

    public void UpdateSettingsText()
    {
        if (volumeValueText != null)
            volumeValueText.text = $"{Mathf.RoundToInt(volumeSlider.value * 100f)}%";

        if (sensitivityValueText != null)
            sensitivityValueText.text = sensitivitySlider.value.ToString("0.0");
    }
}