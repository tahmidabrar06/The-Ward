
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheWard.Combat
{
    public class WeaponInventoryHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WeaponController weaponController;

        [Header("Slot 1")]
        [SerializeField] private Image slot1Highlight;
        [SerializeField] private Image slot1Icon;
        [SerializeField] private TMP_Text slot1Name;

        [Header("Slot 2")]
        [SerializeField] private Image slot2Highlight;
        [SerializeField] private Image slot2Icon;
        [SerializeField] private TMP_Text slot2Name;

        [Header("Weapon Icons - Match WeaponController Order")]
        [SerializeField] private Sprite[] weaponIcons = new Sprite[5];

        [Header("Highlight Colours")]
        [SerializeField] private Color activeColour =
            new Color(0.84f, 0.66f, 0.29f, 1f);

        [SerializeField] private Color inactiveColour =
            new Color(0.84f, 0.66f, 0.29f, 0f);

        private int previousSlot = -1;
        private Weapon previousWeapon1;
        private Weapon previousWeapon2;

        private void Start()
        {
            if (weaponController == null)
            {
                Debug.LogError(
                    "WeaponInventoryHUD: Assign the WeaponController.",
                    this);
                return;
            }

            RefreshHUD();
        }

        private void Update()
        {
            if (weaponController == null)
                return;

            RefreshHUD();
        }

        private void RefreshHUD()
        {
            UpdateHighlights();

            Weapon activeWeapon = weaponController.ActiveWeapon;

            if (previousSlot != weaponController.ActiveSlot ||
                previousWeapon1 != GetSlotWeapon(0) ||
                previousWeapon2 != GetSlotWeapon(1))
            {
                UpdateSlotDisplay(0, GetSlotWeapon(0));
                UpdateSlotDisplay(1, GetSlotWeapon(1));

                previousSlot = weaponController.ActiveSlot;
                previousWeapon1 = GetSlotWeapon(0);
                previousWeapon2 = GetSlotWeapon(1);
            }
        }

        private void UpdateHighlights()
        {
            if (slot1Highlight != null)
                slot1Highlight.color =
                    weaponController.ActiveSlot == 0
                        ? activeColour
                        : inactiveColour;

            if (slot2Highlight != null)
                slot2Highlight.color =
                    weaponController.ActiveSlot == 1
                        ? activeColour
                        : inactiveColour;
        }

        private Weapon GetSlotWeapon(int slot)
        {
            return weaponController.GetWeaponInSlot(slot);
        }

        private void UpdateSlotDisplay(int slot, Weapon weapon)
        {
            Image icon = slot == 0 ? slot1Icon : slot2Icon;
            TMP_Text nameText = slot == 0 ? slot1Name : slot2Name;

            if (icon != null)
            {
                if (weapon == null)
                {
                    icon.sprite = null;
                    icon.enabled = false;
                }
                else
                {
                    int index = weaponController.GetWeaponIndexInSlot(slot);

                    icon.sprite =
                        index >= 0 && index < weaponIcons.Length
                            ? weaponIcons[index]
                            : null;

                    icon.enabled = icon.sprite != null;
                    icon.preserveAspect = true;
                }
            }

            if (nameText != null)
            {
                nameText.text = weapon != null
                    ? weapon.DisplayName
                    : "Empty";

                nameText.enabled = true;
            }
        }
    }
}
