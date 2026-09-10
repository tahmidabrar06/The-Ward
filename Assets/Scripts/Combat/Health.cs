using UnityEngine;
using UnityEngine.Events;

namespace TheWard.Combat
{
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private UnityEvent onDeath;

        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;

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
            if (!IsAlive)
            {
                onDeath?.Invoke();
            }
        }

        public void RestoreFullHealth()
        {
            CurrentHealth = maxHealth;
        }
    }
}