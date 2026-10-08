using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Combat
{
    [DefaultExecutionOrder(100)]
    public sealed class WeaponController : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private InputActionReference reloadAction;
        [SerializeField] private InputActionReference previousWeaponAction;
        [SerializeField] private InputActionReference nextWeaponAction;

        [Header("Player Weapons")]
        [SerializeField] private Weapon[] weapons;
        [SerializeField] private int startingWeaponIndex;

        private int activeWeaponIndex;

        public Weapon ActiveWeapon => weapons != null && activeWeaponIndex >= 0 && activeWeaponIndex < weapons.Length
            ? weapons[activeWeaponIndex]
            : null;

        private void Awake()
        {
            if (weapons == null || weapons.Length == 0)
            {
                return;
            }

            activeWeaponIndex = Mathf.Clamp(startingWeaponIndex, 0, weapons.Length - 1);
            ShowSelectedWeapon();
        }

        private void OnEnable()
        {
            attackAction?.action.Enable();
            reloadAction?.action.Enable();
            previousWeaponAction?.action.Enable();
            nextWeaponAction?.action.Enable();
        }

        private void OnDisable()
        {
            attackAction?.action.Disable();
            reloadAction?.action.Disable();
            previousWeaponAction?.action.Disable();
            nextWeaponAction?.action.Disable();
        }

        private void Update()
        {
            // Select the grips before the arm Animator evaluates IK.
            ReadWeaponSwitchInput();
        }

        private void LateUpdate()
        {
            // Aim after mouse look and the head's eye anchor have moved the camera.
            Weapon weapon = ActiveWeapon;
            if (weapon == null) return;

            if (attackAction != null)
            {
                weapon.HandleInput(attackAction.action.WasPressedThisFrame(),
                    attackAction.action.IsPressed(), attackAction.action.WasReleasedThisFrame());
            }

            if (reloadAction != null && reloadAction.action.WasPressedThisFrame())
                weapon.Reload();
        }

        private void ReadWeaponSwitchInput()
        {
            if (previousWeaponAction != null && previousWeaponAction.action.WasPressedThisFrame())
            {
                ChangeWeapon(-1);
            }

            if (nextWeaponAction != null && nextWeaponAction.action.WasPressedThisFrame())
            {
                ChangeWeapon(1);
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.digit1Key.wasPressedThisFrame) SelectWeapon(0);
            else if (keyboard.digit2Key.wasPressedThisFrame) SelectWeapon(1);
            else if (keyboard.digit3Key.wasPressedThisFrame) SelectWeapon(2);
            else if (keyboard.digit4Key.wasPressedThisFrame) SelectWeapon(3);
            else if (keyboard.digit5Key.wasPressedThisFrame) SelectWeapon(4);
        }

        private void ChangeWeapon(int direction)
        {
            if (weapons == null || weapons.Length == 0)
            {
                return;
            }

            int newIndex = (activeWeaponIndex + direction + weapons.Length) % weapons.Length;
            SelectWeapon(newIndex);
        }

        public void SelectWeapon(int index)
        {
            if (weapons == null || index < 0 || index >= weapons.Length || weapons[index] == null)
            {
                return;
            }

            // Selecting the equipped slot again should not replay its equip sound.
            if (activeWeaponIndex == index) return;
            activeWeaponIndex = index;
            ShowSelectedWeapon();
            if (ActiveWeapon is GunWeapon gun) gun.PlayEquipSound();
        }

        private void ShowSelectedWeapon()
        {
            /*for (int index = 0; index < weapons.Length; index++)
            {
                if (weapons[index] != null)
                {
                    weapons[index].gameObject.SetActive(index == activeWeaponIndex);
                }
            }*/
            for (int index = 0; index < weapons.Length; index++)
            {
                if (weapons[index] != null)
                {
                    weapons[index].gameObject.SetActive(false);
                }
            }
                weapons[activeWeaponIndex].gameObject.SetActive(true);
        }
    }
}
