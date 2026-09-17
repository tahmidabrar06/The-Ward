using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Combat
{
    // Reads the player's input and decides which weapon is active.
    public class WeaponController : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private InputActionReference previousWeaponAction;
        [SerializeField] private InputActionReference nextWeaponAction;

        [Header("Player Weapons")]
        [SerializeField] private Weapon[] weapons;

        private int activeWeaponIndex;

        private void OnEnable()
        {
            // Input actions must be enabled before Unity can read them.
            EnableAction(attackAction);
            EnableAction(previousWeaponAction);
            EnableAction(nextWeaponAction);
        }

        private void OnDisable()
        {
            DisableAction(attackAction);
            DisableAction(previousWeaponAction);
            DisableAction(nextWeaponAction);
        }

        private void Update()
        {
            ReadAttackInput();
            ReadWeaponSwitchInput();
        }

        private void ReadAttackInput()
        {
            if (attackAction != null && attackAction.action.WasPressedThisFrame())
            {
                Weapon activeWeapon = GetActiveWeapon();

                if (activeWeapon != null)
                {
                    activeWeapon.TryAttack(gameObject);
                }
            }
        }

        private void ReadWeaponSwitchInput()
        {
            if (previousWeaponAction != null && previousWeaponAction.action.WasPressedThisFrame())
            {
                SelectPreviousWeapon();
            }

            if (nextWeaponAction != null && nextWeaponAction.action.WasPressedThisFrame())
            {
                SelectNextWeapon();
            }
        }

        private Weapon GetActiveWeapon()
        {
            if (weapons == null || weapons.Length == 0)
            {
                return null;
            }

            return weapons[activeWeaponIndex];
        }

        private void SelectPreviousWeapon()
        {
            if (weapons == null || weapons.Length == 0)
            {
                return;
            }

            activeWeaponIndex--;

            // Move from the first weapon to the last weapon.
            if (activeWeaponIndex < 0)
            {
                activeWeaponIndex = weapons.Length - 1;
            }

            ShowActiveWeapon();
        }

        private void SelectNextWeapon()
        {
            if (weapons == null || weapons.Length == 0)
            {
                return;
            }

            activeWeaponIndex++;

            // Move from the last weapon back to the first weapon.
            if (activeWeaponIndex >= weapons.Length)
            {
                activeWeaponIndex = 0;
            }

            ShowActiveWeapon();
        }

        private void ShowActiveWeapon()
        {
            // Only the selected weapon should be visible.
            for (int index = 0; index < weapons.Length; index++)
            {
                if (weapons[index] != null)
                {
                    weapons[index].gameObject.SetActive(index == activeWeaponIndex);
                }
            }
        }

        private static void EnableAction(InputActionReference actionReference)
        {
            if (actionReference != null)
            {
                actionReference.action.Enable();
            }
        }

        private static void DisableAction(InputActionReference actionReference)
        {
            if (actionReference != null)
            {
                actionReference.action.Disable();
            }
        }
    }
}
