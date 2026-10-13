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

        [Header("Original Player Weapons")]
        [Tooltip("Order: Pistol_C, AK74, Baseball Bat, Grenades, Sword.")]
        [SerializeField] private Weapon[] weapons;

        [Header("World Pickup Prefabs")]
        [Tooltip("Use the same order as the Original Player Weapons array.")]
        [SerializeField] private WeaponPickup[] pickupPrefabs;

        [Header("Pickup Settings")]
        [SerializeField] private float pickupDistance = 2.5f;
        [SerializeField] private Key pickupKey = Key.E;
        [SerializeField] private Camera playerCamera;

        private WeaponPickup nearbyPickup;

        private const int SlotCount = 2;

        private readonly int[] slots = { 0, -1 };
        private int activeSlot;

        public int ActiveSlot => activeSlot;

        public Weapon GetWeaponInSlot(int slot)
        {
            if (slot < 0 || slot >= SlotCount)
                return null;

            int weaponIndex = slots[slot];

            if (weaponIndex < 0 || weaponIndex >= weapons.Length)
                return null;

            return weapons[weaponIndex];
        }

        public int GetWeaponIndexInSlot(int slot)
        {
            if (slot < 0 || slot >= SlotCount)
                return -1;

            return slots[slot];
        }
                
        public Weapon ActiveWeapon =>
            activeSlot >= 0 && activeSlot < SlotCount &&
            slots[activeSlot] >= 0 &&
            slots[activeSlot] < weapons.Length
                ? weapons[slots[activeSlot]]
                : null;
    
        private void Awake()
        {
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            if (weapons == null || weapons.Length != 5)
            {
                Debug.LogError(
                    "WeaponController needs all five original weapon references.",
                    this);
                enabled = false;
                return;
            }

            if (pickupPrefabs == null || pickupPrefabs.Length != weapons.Length)
            {
                Debug.LogError(
                    "Assign five pickup prefabs in the same order as the weapons.",
                    this);
                enabled = false;
                return;
            }

            activeSlot = 0;
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
            ReadWeaponSwitchInput();

            if (Keyboard.current != null &&
                Keyboard.current[pickupKey].wasPressedThisFrame)
            {
                if (nearbyPickup != null)
                    TryPickup(nearbyPickup);
                else
                    Debug.Log("No weapon nearby to pick up.");
            }
        }


        private void LateUpdate()
        {
            Weapon weapon = ActiveWeapon;
            if (weapon == null) return;

            if (attackAction != null)
            {
                weapon.HandleInput(
                    attackAction.action.WasPressedThisFrame(),
                    attackAction.action.IsPressed(),
                    attackAction.action.WasReleasedThisFrame());
            }

            if (reloadAction != null &&
                reloadAction.action.WasPressedThisFrame())
            {
                weapon.Reload();
            }
        }
        
        private void ReadWeaponSwitchInput()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard != null)
            {
                //select a specific inventory slot
                if (keyboard.digit1Key.wasPressedThisFrame)
                    SelectSlot(0);
                else if (keyboard.digit2Key.wasPressedThisFrame)
                    SelectSlot(1);
            }

            //switch between occupied slots using the middle mouse scroll wheel
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;

                if (scroll > 0f)
                    ChangeSlot(-1);
                else if (scroll < 0f)
                    ChangeSlot(1);
            }

        }

        private void ChangeSlot(int direction)
        {
            int otherSlot = activeSlot == 0 ? 1 : 0;

            if (slots[otherSlot] >= 0)
                SelectSlot(otherSlot);
        }

        public void SelectSlot(int slot)
        {
            if (slot < 0 || slot >= SlotCount || slots[slot] < 0)
                return;

            if (activeSlot == slot) return;

            activeSlot = slot;
            ShowSelectedWeapon();

            if (ActiveWeapon is GunWeapon gun)
                gun.PlayEquipSound();
        }

        
        private void TryPickup(WeaponPickup pickup)
        {
            if (pickup == null)
                return;

            int pickupIndex = FindPickupIndex(pickup);

            if (pickupIndex < 0)
                return;

            int ownedSlot = FindOwnedSlot(pickupIndex);

            // Duplicate guns refill ammunition.
            if (ownedSlot >= 0)
            {
                if (weapons[pickupIndex] is GunWeapon ownedGun)
                {
                    ownedGun.RefillAmmo();
                    Debug.Log("Ammo refilled: " + pickup.DisplayName);
                    Destroy(pickup.gameObject);
                    TutorialManager.Instance.HidePrompt();
                }
                else
                {
                    Debug.Log("You already have this weapon.");
                }

                return;
            }

            int emptySlot = FindEmptySlot();

            // Use the empty slot if one is available.
            if (emptySlot >= 0)
            {
                slots[emptySlot] = pickupIndex;
                activeSlot = emptySlot;
                Destroy(pickup.gameObject);
                ShowSelectedWeapon();

                if (ActiveWeapon is GunWeapon gun)
                    gun.PlayEquipSound();
                
                TutorialManager.Instance.HidePrompt();
                Debug.Log("Picked up: " + pickup.DisplayName);
                return;
            }

            int replacedIndex = slots[activeSlot];

            if (!DropWeapon(
                    replacedIndex,
                    pickup.transform.position,
                    pickup.transform.rotation))
            {
                return;
            }

            Debug.Log("Weapon dropped successfully.");

            slots[activeSlot] = pickupIndex;
            Destroy(pickup.gameObject);
            ShowSelectedWeapon();
            
            TutorialManager.Instance.HidePrompt();

            if (ActiveWeapon is GunWeapon newGun)
                newGun.PlayEquipSound();

            Debug.Log("Replaced weapon with: " + pickup.DisplayName);
        }


        private int FindPickupIndex(WeaponPickup pickup)
        {
            for (int i = 0; i < pickupPrefabs.Length; i++)
            {
                if (pickupPrefabs[i] != null &&
                    pickupPrefabs[i].WeaponId == pickup.WeaponId)
                    return i;
            }

            Debug.LogWarning("No matching weapon configured for pickup: " +
                pickup.WeaponId, pickup);
            return -1;
        }

        private int FindOwnedSlot(int weaponIndex)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (slots[i] == weaponIndex)
                    return i;
            }

            return -1;
        }

        private int FindEmptySlot()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (slots[i] < 0)
                    return i;
            }

            return -1;
        }

        
        
        
        private bool DropWeapon(
            int weaponIndex,
            Vector3 dropPosition,
            Quaternion dropRotation)
        {
            if (weaponIndex < 0 || weaponIndex >= pickupPrefabs.Length)
            {
                Debug.LogError($"Invalid weapon index: {weaponIndex}", this);
                return false;
            }

            WeaponPickup prefab = pickupPrefabs[weaponIndex];

            if (prefab == null)
            {
                Debug.LogError(
                    $"Missing pickup prefab for weapon index {weaponIndex}.", this);
                return false;
            }

            // Spawn the outgoing weapon where the incoming pickup was located.
            WeaponPickup dropped = Instantiate(
                prefab,
                dropPosition,
                dropRotation);

            Debug.Log(
                $"Dropped {dropped.DisplayName} at the incoming pickup's position.",
                dropped);

            return true;
        }



        private void ShowSelectedWeapon()
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                    weapons[i].gameObject.SetActive(false);
            }

            Weapon active = ActiveWeapon;
            if (active != null)
                active.gameObject.SetActive(true);
        }

        
        private void OnTriggerEnter(Collider other)
        {
            WeaponPickup pickup = other.GetComponentInParent<WeaponPickup>();
            if (pickup != null)
            {
                nearbyPickup = pickup;
                if(!TutorialManager.Instance.IsSequenceActive) TutorialManager.Instance.ShowPrompt("Press [E] to pick up weapon");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            WeaponPickup pickup = other.GetComponentInParent<WeaponPickup>();
            if (pickup != null && nearbyPickup == pickup)
            {
                nearbyPickup = null;
                if(!TutorialManager.Instance.IsSequenceActive) TutorialManager.Instance.HidePrompt();
            }
        }

    }
    
}
