
using UnityEngine;

namespace TheWard.Combat
{
    public sealed class WeaponPickup : MonoBehaviour
    {
        [Header("Pickup")]
        [SerializeField] private string weaponId = "Pistol_C";
        [SerializeField] private string displayName = "Weapon";

        [Header("Pickup Area")]
        [SerializeField] private float pickupRange = 2.5f;

        public string WeaponId => weaponId;
        public string DisplayName => displayName;
        public float PickupRange => pickupRange;

        private void OnValidate()
        {
            pickupRange = Mathf.Max(0.1f, pickupRange);
        }
    }
}
