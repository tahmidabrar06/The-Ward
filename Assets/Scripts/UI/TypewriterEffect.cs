// https://github.com/elifanous/Unity-Typewriter-Effect/blob/main/TypewriterEffect.cs

using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TypewriterEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private Button continueButton;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;

    private string fullText;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (textComponent != null)
        {
            fullText = textComponent.text;
        }
    }

    private void OnEnable()
    {
		Debug.Log(fullText);
        if (textComponent == null) return;
        fullText = textComponent.text;
        
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        textComponent.text = "";
        
        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        foreach (char c in fullText)
        {
            textComponent.text += c;
            
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        if (continueButton != null) continueButton.gameObject.SetActive(true);
    }
}