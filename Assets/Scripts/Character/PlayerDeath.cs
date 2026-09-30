using UnityEngine;
using TheWard.Character;
using TheWard.Combat;
using UnityEngine.SceneManagement;

namespace TheWard.Core
{
    public class PlayerDeath : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private Stamina playerStamina;
        [SerializeField] private PlayerCharacterController moveController;
        [SerializeField] private PlayerLookController lookController;
        [SerializeField] private WeaponController weaponController;
        
        [Header("UI References")]
        [SerializeField] private UIManager uiManager;

        [Header("Respawn Settings")]
        [SerializeField] private Transform respawnPoint;

        
        public void HandleDeath()
        {   
            uiManager.ShowRespawnScreen();
            if (moveController != null) moveController.enabled = false;
            if (lookController != null) lookController.enabled = false;
            if (weaponController != null) weaponController.enabled = false;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            //Respawn();   //respawn is redundant. levels are small enough that we can reload scene instead.
        }
        //removed respawn
        public void ReturnToMainMenu()
        {
            Debug.Log("TO DO");
        }
    }
}