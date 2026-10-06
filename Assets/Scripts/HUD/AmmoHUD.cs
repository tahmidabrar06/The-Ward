using UnityEngine;
using TMPro; 

namespace TheWard.Combat
{
    public class AmmoHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text ammoCountText;
        [SerializeField] private TMP_Text warningText;
        
        [Header("Settings")]
        [SerializeField] private int lowAmmoThreshold = 3; 

        private void Awake()
        {
            if (warningText != null)
            {
                warningText.gameObject.SetActive(false);
            }
        }

        public void UpdateAmmoState(int currentClipAmmo, int totalReserveAmmo, int magazineSize)
        {
            if (ammoCountText != null)
            {
                ammoCountText.text = $"{totalReserveAmmo}\n{currentClipAmmo} / {magazineSize}";
            }
            if (warningText == null) return;

            if (currentClipAmmo == 0 && totalReserveAmmo == 0)
            {
                warningText.text = "OUT OF AMMO";
                warningText.color = Color.red;
                warningText.gameObject.SetActive(true);
            }
            else if (currentClipAmmo == 0 && totalReserveAmmo > 0)
            {
                warningText.text = "PRESS 'R' TO RELOAD";
                warningText.color = Color.yellow;
                warningText.gameObject.SetActive(true);
            }
            else if (currentClipAmmo <= lowAmmoThreshold)
            {
                warningText.text = "LOW AMMO";
                warningText.color = new Color(1f, 0.5f, 0f); // Orange
                warningText.gameObject.SetActive(true);
            }
            else
            {
                warningText.gameObject.SetActive(false);
            }
        }
        public void EnableAmmoText()
        {
            ammoCountText.gameObject.SetActive(true);
        }

        public void DisableAmmoText()
        {
            ammoCountText.gameObject.SetActive(false);
        }
    }
}
