using UnityEngine;
using UnityEngine.UI;
using TheWard.Character;

namespace TheWard.Core
{
    public class PlayerStaminaHUD : MonoBehaviour
    {
        [SerializeField] private Stamina stamina;
        [SerializeField] private Image staminaBarFill;
        
        private void Start()
        {
            if (stamina != null)
            {
                UpdateStaminaUI(stamina.CurrentStamina, stamina.MaxStamina);
            }
        }
        
        private void OnEnable()
        {
            if (stamina == null) stamina = GetComponentInParent<Stamina>();
            
            if (stamina != null)
            {
                stamina.StaminaChanged += UpdateStaminaUI;
            }
        }

        private void OnDisable()
        {
            if (stamina != null)
            {
                stamina.StaminaChanged -= UpdateStaminaUI;
            }
        }

        private void UpdateStaminaUI(float current, float max)
        {
            if (staminaBarFill != null)
            {
                staminaBarFill.fillAmount = max > 0 ? current / max : 0f;
            }
        }
    }
}