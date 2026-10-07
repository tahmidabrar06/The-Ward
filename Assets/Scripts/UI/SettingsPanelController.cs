using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AudioSettings = TheWard.Audio.AudioSettings;

public class SettingsPanelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider cinematicSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text volumeValueText;
    [SerializeField] private TMP_Text sfxValueText;
    [SerializeField] private TMP_Text musicValueText;
    [SerializeField] private TMP_Text cinematicValueText;
    [SerializeField] private TMP_Text sensitivityValueText;
    [SerializeField] private TMP_Text savedMessage;

    private void OnEnable()
    {
        LoadSettings();
        Listen(volumeSlider, true);
        Listen(sfxSlider, true);
        Listen(musicSlider, true);
        Listen(cinematicSlider, true);
        Listen(sensitivitySlider, true);
    }

    private void OnDisable()
    {
        Listen(volumeSlider, false);
        Listen(sfxSlider, false);
        Listen(musicSlider, false);
        Listen(cinematicSlider, false);
        Listen(sensitivitySlider, false);
        CancelInvoke(nameof(HideSavedMessage));
        HideSavedMessage();
    }

    private void Listen(Slider slider, bool add)
    {
        if (slider == null) return;
        if (add) slider.onValueChanged.AddListener(OnSliderChanged);
        else slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        AudioSettings.SetVolumes(Value(volumeSlider, AudioSettings.Master),
            Value(sfxSlider, AudioSettings.Sfx), Value(musicSlider, AudioSettings.Music),
            Value(cinematicSlider, AudioSettings.Cinematics));
        HideSavedMessage();
        UpdateSettingsText();
    }

    private static float Value(Slider slider, float fallback) => slider != null ? slider.value : fallback;

    private void LoadSettings()
    {
        Set(volumeSlider, AudioSettings.Master);
        Set(sfxSlider, AudioSettings.Sfx);
        Set(musicSlider, AudioSettings.Music);
        Set(cinematicSlider, AudioSettings.Cinematics);
        Set(sensitivitySlider, PlayerPrefs.GetFloat("MouseSensitivity", 1f));
        HideSavedMessage();
        UpdateSettingsText();
    }

    private static void Set(Slider slider, float value)
    {
        if (slider != null) slider.SetValueWithoutNotify(value);
    }

    public void SaveSettings()
    {
        OnSliderChanged(0f);
        AudioSettings.Save();
        if (sensitivitySlider != null)
            PlayerPrefs.SetFloat("MouseSensitivity", sensitivitySlider.value);
        PlayerPrefs.Save();
        if (savedMessage != null)
        {
            savedMessage.gameObject.SetActive(true);
            CancelInvoke(nameof(HideSavedMessage));
            Invoke(nameof(HideSavedMessage), 2f);
        }
    }

    private void HideSavedMessage()
    {
        if (savedMessage != null) savedMessage.gameObject.SetActive(false);
    }

    private static void Percentage(TMP_Text text, Slider slider)
    {
        if (text != null && slider != null) text.text = $"{Mathf.RoundToInt(slider.value * 100f)}%";
    }

    public void UpdateSettingsText()
    {
        Percentage(volumeValueText, volumeSlider);
        Percentage(sfxValueText, sfxSlider);
        Percentage(musicValueText, musicSlider);
        Percentage(cinematicValueText, cinematicSlider);
        if (sensitivityValueText != null && sensitivitySlider != null)
            sensitivityValueText.text = sensitivitySlider.value.ToString("0.0");
    }
}
