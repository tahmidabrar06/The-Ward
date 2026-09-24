using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HitIndicator : MonoBehaviour
{
    public static HitIndicator Instance;
    [SerializeField] private Image[] hitImages; 
    [SerializeField] private float displayTime = 0.1f;
    [SerializeField] private float fadeTime = 0.2f;

    private Coroutine flashRoutine;

    private void Awake()
    {
        Instance = this;
        SetImagesColor(Color.white, 0f);
    }

    public void ShowHit(bool isKill)
    {
        if (hitImages == null || hitImages.Length == 0) return;
        
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash(isKill ? Color.red : Color.white));
    }

    private IEnumerator Flash(Color targetColor)
    {
        SetImagesColor(targetColor, 1f);
        yield return new WaitForSeconds(displayTime);
        
        float timer = 0;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            SetImagesColor(targetColor, Mathf.Lerp(1, 0, timer / fadeTime));
            yield return null;
        }
        
        SetImagesColor(Color.white, 0f);
    }
    private void SetImagesColor(Color baseColor, float alpha)
    {
        foreach (Image img in hitImages)
        {
            if (img != null)
            {
                img.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            }
        }
    }
}