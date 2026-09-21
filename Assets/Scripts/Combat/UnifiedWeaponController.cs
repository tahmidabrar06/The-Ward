using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Combat
{
    // This component reads player input and sends it to the equipped weapon.
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

        private int activeWeaponIndex;

        private void Awake()
        {
            if (weapons == null || weapons.Length == 0)
            {
                return;
            }

            // Make sure the starting number fits inside the weapons array.
            activeWeaponIndex = Mathf.Clamp(startingWeaponIndex, 0, weapons.Length - 1);
            ShowSelectedWeapon();
        }

        private void OnEnable()
        {
            // Unity Input Actions only work while they are enabled.
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

            // Different weapons use these values differently. For example,
            // the machine gun uses held and the grenade uses released.
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

            // Number keys select a weapon directly.
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

            int newIndex = activeWeaponIndex + direction;

            // Wrap around when moving past either end of the list.
            if (newIndex < 0)
            {
                newIndex = weapons.Length - 1;
            }
            else if (newIndex >= weapons.Length)
            {
                newIndex = 0;
            }

            SelectWeapon(newIndex);
        }

        public void SelectWeapon(int index)
        {
            // Ignore invalid weapon numbers.
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
            // Hide every weapon except the selected one.
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
            TheWard.Character.PlayerCharacterController player =
                GetComponent<TheWard.Character.PlayerCharacterController>();
            UnifiedWeapon activeWeapon = GetActiveWeapon();
            if (player != null && activeWeapon != null)
            {
                player.SetMeleeLocomotion(activeWeapon.Mode == WeaponMode.Melee);
                player.SetGunEquipped(activeWeapon.Mode == WeaponMode.Firearm);
            }
        }

        private void EnableAction(InputActionReference actionReference)
        {
            if (actionReference != null)
            {
                actionReference.action.Enable();
            }
        }

        private void DisableAction(InputActionReference actionReference)
        {
            if (actionReference != null)
            {
                actionReference.action.Disable();
            }
        }
    }
}
