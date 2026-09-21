using TheWard.Character;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Combat
{
    public class UnifiedWeaponController : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private InputActionReference reloadAction;
        [SerializeField] private InputActionReference previousWeaponAction;
        [SerializeField] private InputActionReference nextWeaponAction;

        [Header("Player Weapons")]
        [SerializeField] private UnifiedWeapon[] weapons;
        [SerializeField] private int startingWeaponIndex;

        private PlayerCharacterController player;
        private int activeWeaponIndex;

        private void Awake()
        {
            player = GetComponent<PlayerCharacterController>();
            if (weapons == null || weapons.Length == 0)
            {
                return;
            }

            activeWeaponIndex = Mathf.Clamp(startingWeaponIndex, 0, weapons.Length - 1);
            ShowSelectedWeapon();
        }

        private void OnEnable()
        {
            EnableAction(attackAction);
            EnableAction(reloadAction);
            EnableAction(previousWeaponAction);
            EnableAction(nextWeaponAction);
        }

        private void Start()
        {
            UpdateMovementStyle();
        }

        private void OnDisable()
        {
            DisableAction(attackAction);
            DisableAction(reloadAction);
            DisableAction(previousWeaponAction);
            DisableAction(nextWeaponAction);
        }

        private void Update()
        {
            ReadAttackInput();
            ReadReloadInput();
            ReadWeaponSwitchInput();
        }

        private void ReadAttackInput()
        {
            UnifiedWeapon activeWeapon = GetActiveWeapon();

            if (activeWeapon == null || attackAction == null)
            {
                return;
            }

            bool pressed = attackAction.action.WasPressedThisFrame();
            bool held = attackAction.action.IsPressed();
            bool released = attackAction.action.WasReleasedThisFrame();

            activeWeapon.HandleInput(pressed, held, released);
        }

        private void ReadReloadInput()
        {
            if (reloadAction != null && reloadAction.action.WasPressedThisFrame())
            {
                UnifiedWeapon activeWeapon = GetActiveWeapon();

                if (activeWeapon != null)
                {
                    activeWeapon.Reload();
                }
            }
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
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                SelectWeapon(0);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                SelectWeapon(1);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame)
            {
                SelectWeapon(2);
            }
            else if (keyboard.digit4Key.wasPressedThisFrame)
            {
                SelectWeapon(3);
            }
            else if (keyboard.digit5Key.wasPressedThisFrame)
            {
                SelectWeapon(4);
            }
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
            if (weapons == null || index < 0 || index >= weapons.Length)
            {
                return;
            }

            if (weapons[index] == null)
            {
                return;
            }

            activeWeaponIndex = index;
            ShowSelectedWeapon();
            UpdateMovementStyle();
        }

        private UnifiedWeapon GetActiveWeapon()
        {
            if (weapons == null || weapons.Length == 0)
            {
                return null;
            }

            return weapons[activeWeaponIndex];
        }

        private void ShowSelectedWeapon()
        {
            for (int index = 0; index < weapons.Length; index++)
            {
                if (weapons[index] != null)
                {
                    weapons[index].gameObject.SetActive(index == activeWeaponIndex);
                }
            }
        }

        private void UpdateMovementStyle()
        {
            UnifiedWeapon activeWeapon = GetActiveWeapon();
            if (player != null && activeWeapon != null)
            {
                player.SetMeleeLocomotion(activeWeapon.Mode == WeaponMode.Melee);
                player.SetGunEquipped(activeWeapon.Mode == WeaponMode.Firearm);
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
