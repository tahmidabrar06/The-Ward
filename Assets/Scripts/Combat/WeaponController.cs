using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Combat
{
    [DefaultExecutionOrder(100)]
    public class WeaponController : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private InputActionReference reloadAction;
        [SerializeField] private InputActionReference previousWeaponAction;
        [SerializeField] private InputActionReference nextWeaponAction;

        [Header("Player Weapons")]
        [SerializeField] private Weapon[] weapons;
        [SerializeField] private int startingWeaponIndex;

        public Weapon ActiveWeapon => GetActiveWeapon();

        private int activeWeaponIndex;

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
            EnableAction(attackAction);
            EnableAction(reloadAction);
            EnableAction(previousWeaponAction);
            EnableAction(nextWeaponAction);
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
            // Select the grips before the arm Animator evaluates IK.
            ReadWeaponSwitchInput();
        }

        private void LateUpdate()
        {
            // Aim after mouse look and the head's eye anchor have moved the camera.
            ReadAttackInput();
            ReadReloadInput();
        }

        private void OnGUI()
        {
            Weapon weapon = ActiveWeapon;
            if (weapon == null || !(weapon is GunWeapon) || weapon.AimCamera == null) return;

            Vector2 center = weapon.AimCamera.pixelRect.center;
            center.y = Screen.height - center.y;
            Color previousColor = GUI.color;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(center.x - 3f, center.y - 3f, 6f, 6f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(center.x - 1f, center.y - 1f, 2f, 2f), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private void ReadAttackInput()
        {
            Weapon activeWeapon = GetActiveWeapon();

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
                Weapon activeWeapon = GetActiveWeapon();

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
        }

        private Weapon GetActiveWeapon()
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
