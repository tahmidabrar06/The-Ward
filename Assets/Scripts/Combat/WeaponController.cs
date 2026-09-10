using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Combat
{
    public sealed class WeaponController : MonoBehaviour
    {
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private InputActionReference previousWeaponAction;
        [SerializeField] private InputActionReference nextWeaponAction;
        [SerializeField] private Weapon[] weapons;

        private int activeWeaponIndex;

        public Weapon ActiveWeapon => weapons != null && weapons.Length > 0 ? weapons[activeWeaponIndex] : null;

        private void OnEnable()
        {
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
            if (attackAction != null && attackAction.action.WasPressedThisFrame())
            {
                ActiveWeapon?.TryAttack(gameObject);
            }

            if (previousWeaponAction != null && previousWeaponAction.action.WasPressedThisFrame())
            {
                SelectWeapon(-1);
            }

            if (nextWeaponAction != null && nextWeaponAction.action.WasPressedThisFrame())
            {
                SelectWeapon(1);
            }
        }

        private void SelectWeapon(int direction)
        {
            if (weapons == null || weapons.Length == 0)
            {
                return;
            }

            activeWeaponIndex = (activeWeaponIndex + direction + weapons.Length) % weapons.Length;
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
            actionReference?.action.Enable();
        }

        private static void DisableAction(InputActionReference actionReference)
        {
            actionReference?.action.Disable();
        }
    }
}