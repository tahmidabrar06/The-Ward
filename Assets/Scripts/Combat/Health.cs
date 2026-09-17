using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheWard.Combat
{
    // Put this component on anything that can lose health.
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;

        // These events can be connected in the Unity Inspector.
        [SerializeField] private UnityEvent onDamage;
        [SerializeField] private UnityEvent onDeath;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public float NormalizedHealth => CurrentHealth / maxHealth;
        public bool IsAlive => CurrentHealth > 0f;

        // The health UI listens to this event.
        public event Action<float> Damaged;
        public event Action<float, float> HealthChanged;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        // Weapons call this method when they hit this object.
        public void TakeDamage(float damage)
        {
            if (!IsAlive || damage <= 0f)
            {
                return;
            }

            CurrentHealth -= damage;
            CurrentHealth = Mathf.Max(CurrentHealth, 0f);

            onDamage?.Invoke();
            Damaged?.Invoke(damage);
            HealthChanged?.Invoke(CurrentHealth, maxHealth); 

            if (CurrentHealth <= 0f)
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
