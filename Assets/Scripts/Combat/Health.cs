using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheWard.Combat
{
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private UnityEvent onDeath;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public float NormalizedHealth => maxHealth > 0f ? CurrentHealth / maxHealth : 0f;
        public bool IsAlive => CurrentHealth > 0f;
        public event Action<float, float> HealthChanged;
        public event Action<float> Damaged;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(float amount, GameObject source)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            Damaged?.Invoke(amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            if (!IsAlive)
            {
                onDeath?.Invoke();
            }
        }

        public void RestoreFullHealth()
        {
            CurrentHealth = maxHealth;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}
