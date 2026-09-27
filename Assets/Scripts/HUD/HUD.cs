using UnityEngine;
using UnityEngine.UI;
using TheWard.Character;

namespace TheWard.Combat
{
public class HUD : MonoBehaviour
{
           [SerializeField] private Health health;
           [SerializeField] private Image healthBarFill;
           
           [SerializeField] private Stamina stamina;
           [SerializeField] private Image staminaBarFill;
           
           private void Start()
           {
               if (health != null)
               {
                   UpdateHealthUI(health.CurrentHealth, health.MaxHealth);
               }
               if (stamina != null)
               { 
                   UpdateStaminaUI(stamina.CurrentStamina, stamina.MaxStamina);
               }
           }
   
           private void OnEnable()
           {
               if (health != null)
               {
                   health.HealthChanged += UpdateHealthUI;
               }
                              
               if (stamina != null) 
               { 
                   stamina.StaminaChanged += UpdateStaminaUI;
               }
           }
   
           private void OnDisable()
           {
               if (health != null)
               {
                   health.HealthChanged -= UpdateHealthUI;
               }
               
               if (stamina != null)
               { 
                   stamina.StaminaChanged -= UpdateStaminaUI; 
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
           
           private void UpdateStaminaUI(float current, float max)
           {
               if (staminaBarFill != null)
               {
                   staminaBarFill.fillAmount = max > 0 ? current / max : 0f;
               } 
           }
    }
}
