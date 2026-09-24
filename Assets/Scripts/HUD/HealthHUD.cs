using UnityEngine;
using UnityEngine.UI;
using TMPro; 

namespace TheWard.Combat
{
    public class PlayerHealthCanvasHUD : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private Image healthBarFill;

        private float damageMessageUntil;
        
        private void Start()
        {
            if (health != null)
            {
                UpdateHealthUI(health.CurrentHealth, health.MaxHealth);
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.HealthChanged += UpdateHealthUI;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.HealthChanged -= UpdateHealthUI;
            }
        }

        private void UpdateHealthUI(float current, float max)
        {
            if (healthBarFill != null)
            {
                float normalized = max > 0 ? current / max : 0f;
                healthBarFill.fillAmount = normalized;
            }
        }
    }
}