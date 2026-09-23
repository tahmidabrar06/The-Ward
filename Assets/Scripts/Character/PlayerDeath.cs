using UnityEngine;
using TheWard.Character;
using TheWard.Combat;

namespace TheWard.Core
{
    public class PlayerDeath : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private Stamina playerStamina;
        [SerializeField] private PlayerCharacterController moveController;
        [SerializeField] private PlayerLookController lookController;
        [SerializeField] private UnifiedWeaponController weaponController;
        
        [Header("UI References")]
        [SerializeField] private GameObject deathUIPanel;

        [Header("Respawn Settings")]
        [SerializeField] private Transform respawnPoint;

        private void Start()
        {
            if (deathUIPanel != null) deathUIPanel.SetActive(false);
        }
        
        public void HandleDeath()
        {
            if (deathUIPanel != null) deathUIPanel.SetActive(true);
            
            if (moveController != null) moveController.enabled = false;
            if (lookController != null) lookController.enabled = false;
            if (weaponController != null) weaponController.enabled = false;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public void Respawn()
        {
            if (deathUIPanel != null) deathUIPanel.SetActive(false);
            
            CharacterController cc = moveController.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            
            if (respawnPoint != null)
            {
                moveController.transform.position = respawnPoint.position;
                moveController.transform.rotation = respawnPoint.rotation;
            }
            
            if (cc != null) cc.enabled = true;
            
            playerHealth.RestoreFullHealth();
            playerStamina.RestoreFullStamina();
            
            if (moveController != null) moveController.enabled = true;
            if (lookController != null) lookController.enabled = true;
            if (weaponController != null) weaponController.enabled = true;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        public void ReturnToMainMenu()
        {
            Debug.Log("TO DO");
        }
    }
}