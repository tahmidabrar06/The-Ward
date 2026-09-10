using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Combat
{
    public sealed class UnifiedWeaponController : MonoBehaviour
    {
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private InputActionReference reloadAction;
        [SerializeField] private InputActionReference previousWeaponAction;
        [SerializeField] private InputActionReference nextWeaponAction;
        [SerializeField] private UnifiedWeapon[] weapons;
        [SerializeField, Min(0)] private int startingWeaponIndex;
        private int activeIndex;

        public UnifiedWeapon ActiveWeapon => weapons != null && weapons.Length > 0 ? weapons[activeIndex] : null;

        private void Awake()
        {
            if (weapons == null || weapons.Length == 0) return;
            activeIndex = Mathf.Clamp(startingWeaponIndex, 0, weapons.Length - 1);
            RefreshModels();
        }

        private void OnEnable()
        {
            SetAction(attackAction, true); SetAction(reloadAction, true);
            SetAction(previousWeaponAction, true); SetAction(nextWeaponAction, true);
        }

        private void OnDisable()
        {
            SetAction(attackAction, false); SetAction(reloadAction, false);
            SetAction(previousWeaponAction, false); SetAction(nextWeaponAction, false);
        }

        private void Update()
        {
            if (ActiveWeapon != null && attackAction != null)
                ActiveWeapon.ReadAttack(gameObject, attackAction.action.WasPressedThisFrame(),
                    attackAction.action.IsPressed(), attackAction.action.WasReleasedThisFrame());
            if (reloadAction != null && reloadAction.action.WasPressedThisFrame()) ActiveWeapon?.Reload();
            if (previousWeaponAction != null && previousWeaponAction.action.WasPressedThisFrame()) Cycle(-1);
            if (nextWeaponAction != null && nextWeaponAction.action.WasPressedThisFrame()) Cycle(1);

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.digit1Key.wasPressedThisFrame) Select(0);
            else if (keyboard.digit2Key.wasPressedThisFrame) Select(1);
            else if (keyboard.digit3Key.wasPressedThisFrame) Select(2);
            else if (keyboard.digit4Key.wasPressedThisFrame) Select(3);
            else if (keyboard.digit5Key.wasPressedThisFrame) Select(4);
        }

        public void Select(int index)
        {
            if (weapons == null || index < 0 || index >= weapons.Length || weapons[index] == null) return;
            activeIndex = index;
            RefreshModels();
        }

        private void Cycle(int direction)
        {
            if (weapons == null || weapons.Length == 0) return;
            for (int step = 1; step <= weapons.Length; step++)
            {
                int index = (activeIndex + direction * step + weapons.Length) % weapons.Length;
                if (weapons[index] != null) { Select(index); return; }
            }
        }

        private void RefreshModels()
        {
            for (int i = 0; i < weapons.Length; i++)
                if (weapons[i] != null) weapons[i].gameObject.SetActive(i == activeIndex);
        }

        private static void SetAction(InputActionReference reference, bool enabled)
        {
            if (reference == null) return;
            if (enabled) reference.action.Enable(); else reference.action.Disable();
        }
    }
}
