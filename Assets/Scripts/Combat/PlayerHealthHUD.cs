using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheWard.Combat
{
    [RequireComponent(typeof(Health))]
    public class PlayerHealthHUD : MonoBehaviour
    {
        [Header("Health UI")]
        [SerializeField] private Health health;
        [SerializeField] private Image healthFill;
        [SerializeField] private TMP_Text healthText;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Damaged += UpdateHealthUI;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Damaged -= UpdateHealthUI;
            }
        }

        private void Start()
        {
            UpdateHealthUI(0f);
        }

        private void UpdateHealthUI(float damageAmount)
        {
            if (health == null)
            {
                return;
            }

            float healthPercent = health.NormalizedHealth;

            if (healthFill != null)
            {
                healthFill.fillAmount = healthPercent;
            }

            if (healthText != null)
            {
                healthText.text =
                    $"HEALTH {Mathf.CeilToInt(health.CurrentHealth)} / {Mathf.CeilToInt(health.MaxHealth)}";
            }
        }
    }
}