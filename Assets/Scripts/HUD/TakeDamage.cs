using UnityEngine;
using UnityEngine.Rendering; 
using UnityEngine.Rendering.Universal;
using System.Collections;
using TheWard.Combat; 

public class TakeDamage : MonoBehaviour
{
    [SerializeField] private Health health;

    [SerializeField] private float GoalIntesity = 0.2f;
    private float intensity;
    public float timer = 0.4f;
    
    private Volume _volume; 
    private Vignette _vignette;
    private Coroutine _damageCoroutine;

    void Start()
    {
        _volume = GetComponent<Volume>();
        
        if (_volume == null || _volume.profile == null)
        {
            return;
        }
        
        if (!_volume.profile.TryGet(out _vignette))
        {
            Debug.LogError("Error: Vignette not found");
        }
        else
        {
            _vignette.intensity.Override(0f);
        }
    }

    private void OnEnable()
    {
        if (health != null) health.Damaged += OnPlayerDamaged;
    }

    private void OnDisable()
    {
        if (health != null) health.Damaged -= OnPlayerDamaged;
    }

    private void OnPlayerDamaged(float damageAmount)
    {
        if (_vignette == null) return;

        if (_damageCoroutine != null)
        {
            StopCoroutine(_damageCoroutine);
        }
        _damageCoroutine = StartCoroutine(TakeDamageEffect());
    }

    private IEnumerator TakeDamageEffect()
    {
        intensity += GoalIntesity;

        _vignette.intensity.Override(intensity);

        yield return new WaitForSeconds(timer);

        while (intensity > 0)
        {
            intensity -= 0.05f;
            if (intensity < 0) intensity = 0;
            
            _vignette.intensity.Override(intensity);

            yield return new WaitForSeconds(0.05f);
        }
        _vignette.intensity.Override(0f);
    }
}