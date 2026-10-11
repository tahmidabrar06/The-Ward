using System.Collections;
using System.Collections.Generic;
using TMPro;
using AudioSettings = TheWard.Audio.AudioSettings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class WakeUpIntro : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField, Min(4f)] private float duration = 8f;
    [SerializeField, Min(0.1f)] private float fadeDuration = 1.2f;

    [Header("Camera")]
    [SerializeField] private Vector3 startingOffset = new Vector3(0f, -0.45f, 0f);
    [SerializeField] private Vector3 startingTilt = new Vector3(-25f, 0f, -8f);

    [Header("Audio")]
    [SerializeField] private AudioClip tensionMusic;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.45f;

    [Header("Text")]
    [SerializeField, TextArea] private string wakingText = "You wake up.\nThe hospital is compromised.";
    [SerializeField, TextArea] private string objectiveText = "Find the elevator\nand ascend to the next floor.";

    public bool IsPlaying { get; private set; }
    public bool FinishedThisFrame => finishedFrame == Time.frameCount;
    private int finishedFrame = -1;

    private UIManager uiManager;
    private Camera playerCamera;
    private Camera introCamera;
    private AudioSource musicSource;
    private Canvas overlay;
    private Image fade;
    private TMP_Text caption;
    private Coroutine routine;
    private bool cameraWasEnabled;
    private readonly List<Canvas> hiddenCanvases = new List<Canvas>();

    public void Play(UIManager manager, GameObject oldPanel, Camera camera)
    {
        if (IsPlaying) return;
        if (camera == null)
        {
            manager.StartGameAfterIntro();
            return;
        }

        uiManager = manager;
        playerCamera = camera;
        cameraWasEnabled = camera.enabled;
        IsPlaying = true;

        TMP_Text style = null;
        if (oldPanel != null)
        {
            foreach (TMP_Text text in oldPanel.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.font == null) continue;
                style = text;
                break;
            }
        }

        // Hide the HUD during the intro, without disabling its scripts or changing saved UI.
        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (canvas.gameObject.scene != gameObject.scene || !canvas.enabled) continue;
            hiddenCanvases.Add(canvas);
            canvas.enabled = false;
        }

        CreateCamera();
        CreateOverlay(style);
        PlayMusic();
        routine = StartCoroutine(PlaySequence());
    }

    private void CreateCamera()
    {
        introCamera = new GameObject("Wake-up Camera").AddComponent<Camera>();
        introCamera.CopyFrom(playerCamera);
        introCamera.enabled = true;
        introCamera.rect = new Rect(0f, 0f, 1f, 1f);
        // Keep held weapons and the player body out of the waking-up shot.
        introCamera.cullingMask &= ~(1 << playerCamera.transform.root.gameObject.layer);

        UniversalAdditionalCameraData original = playerCamera.GetUniversalAdditionalCameraData();
        UniversalAdditionalCameraData data = introCamera.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = original.renderPostProcessing;
        data.volumeLayerMask = original.volumeLayerMask;
        data.volumeTrigger = original.volumeTrigger;
        data.antialiasing = original.antialiasing;
        playerCamera.enabled = false;
    }

    private void PlayMusic()
    {
        if (tensionMusic == null) return;
        musicSource = introCamera.gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.ignoreListenerPause = true;
        musicSource.volume = 0f;
        musicSource.clip = tensionMusic;
        // Cinematic audio continues while gameplay time is paused.
        musicSource.Play();
    }

    private void CreateOverlay(TMP_Text style)
    {
        GameObject root = new GameObject("Wake-up Overlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        overlay = root.GetComponent<Canvas>();
        overlay.renderMode = RenderMode.ScreenSpaceOverlay;
        overlay.sortingOrder = 1000;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CreateImage("Top Bar", new Vector2(0f, 0.9f), Vector2.one);
        CreateImage("Bottom Bar", Vector2.zero, new Vector2(1f, 0.1f));
        caption = CreateText("Story", style, new Vector2(0.1f, 0.14f), new Vector2(0.9f, 0.3f), 38f);
        TMP_Text hint = CreateText("Skip", style, new Vector2(0.6f, 0.01f), new Vector2(0.96f, 0.08f), 22f);
        hint.text = "Space / Enter / Esc to skip";
        hint.alignment = TextAlignmentOptions.MidlineRight;
        hint.color = new Color(0.8f, 0.8f, 0.8f);
        fade = CreateImage("Eyes Closed", Vector2.zero, Vector2.one);
    }

    private Image CreateImage(string name, Vector2 min, Vector2 max)
    {
        GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        child.transform.SetParent(overlay.transform, false);
        SetRect(child.GetComponent<RectTransform>(), min, max);
        Image image = child.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;
        return image;
    }

    private TMP_Text CreateText(string name, TMP_Text style, Vector2 min, Vector2 max, float size)
    {
        GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        child.transform.SetParent(overlay.transform, false);
        TMP_Text text = child.AddComponent<TextMeshProUGUI>();
        SetRect(text.rectTransform, min, max);
        if (style != null)
        {
            text.font = style.font;
            text.fontSharedMaterial = style.fontSharedMaterial;
            text.color = style.color;
        }
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private IEnumerator PlaySequence()
    {
        Vector3 endPosition = playerCamera.transform.position;
        Quaternion endRotation = playerCamera.transform.rotation;
        Vector3 startPosition = endPosition + endRotation * startingOffset;
        Quaternion startRotation = endRotation * Quaternion.Euler(startingTilt);
        float elapsed = 0f;
        float length = Mathf.Max(4f, duration);
        float fadeTime = Mathf.Clamp(fadeDuration, 0.1f, length * 0.25f);
        Color textColor = caption.color;

        while (elapsed < length)
        {
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / (length * 0.8f)));
            introCamera.transform.SetPositionAndRotation(
                Vector3.Lerp(startPosition, endPosition, progress),
                Quaternion.Slerp(startRotation, endRotation, progress));
            fade.color = new Color(0f, 0f, 0f, 1f - Mathf.Clamp01((elapsed - 0.4f) / fadeTime));

            bool firstLine = elapsed < length * 0.5f;
            caption.text = firstLine ? wakingText : objectiveText;
            float textStart = firstLine ? fadeTime : length * 0.5f;
            float textEnd = firstLine ? length * 0.5f : length;
            textColor.a = Mathf.Clamp01(Mathf.Min(elapsed - textStart, textEnd - elapsed) / 0.5f);
            caption.color = textColor;
            if (musicSource != null)
            {
                float fadeAmount = Mathf.Clamp01(Mathf.Min(elapsed, length - elapsed) / fadeTime);
                musicSource.volume = musicVolume * AudioSettings.Cinematics * fadeAmount;
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Finish();
    }

    private void Update()
    {
        if (!IsPlaying || Keyboard.current == null) return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame ||
            keyboard.escapeKey.wasPressedThisFrame) Skip();
    }

    public void Skip()
    {
        if (!IsPlaying) return;
        if (routine != null) StopCoroutine(routine);
        Finish();
    }

    private void Finish()
    {
        IsPlaying = false;
        finishedFrame = Time.frameCount;
        routine = null;
        RestoreView();
        if (uiManager != null) uiManager.StartGameAfterIntro();
    }

    private void RestoreView()
    {
        if (musicSource != null) musicSource.Stop();
        if (introCamera != null)
        {
            introCamera.enabled = false;
            Destroy(introCamera.gameObject);
        }
        if (playerCamera != null) playerCamera.enabled = cameraWasEnabled;
        foreach (Canvas canvas in hiddenCanvases)
            if (canvas != null) canvas.enabled = true;
        hiddenCanvases.Clear();
        if (overlay != null) Destroy(overlay.gameObject);
    }

    private void OnDisable()
    {
        if (IsPlaying) Skip();
    }
}
