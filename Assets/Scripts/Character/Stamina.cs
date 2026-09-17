using System;
using UnityEngine;

namespace TheWard.Character
{
    public class Stamina : MonoBehaviour
    {
        [Header("Stamina Settings")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float drainRate = 25f; 
        [SerializeField] private float regenRate = 15f;
        [SerializeField] private float regenDelay = 1.5f;

        public float CurrentStamina { get; private set; }
        public float MaxStamina => maxStamina;
        
        public event Action<float, float> StaminaChanged;

        private float lastDrainTime;

        private void Awake()
        {
            CurrentStamina = maxStamina;
        }

        private void Update()
        {
            if (Time.time > lastDrainTime + regenDelay && CurrentStamina < maxStamina)
            {
                CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + regenRate * Time.deltaTime);
                StaminaChanged?.Invoke(CurrentStamina, maxStamina);
            }
        }

        public bool CanSprint()
        {
            return CurrentStamina > 0.1f;
        }

        public void DrainStamina()
        {
            CurrentStamina = Mathf.Max(0f, CurrentStamina - drainRate * Time.deltaTime);
            lastDrainTime = Time.time;
            StaminaChanged?.Invoke(CurrentStamina, maxStamina);
        }
        
        public void RestoreFullStamina()
        {
            CurrentStamina = maxStamina;
            StaminaChanged?.Invoke(CurrentStamina, maxStamina);
        }
    }
}