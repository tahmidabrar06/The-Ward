using System.Collections.Generic;
using UnityEngine;

namespace TheWard.Combat
{
    public enum WeaponMode { Firearm, Melee, Grenade }
    public enum TriggerMode { PressOnce, HoldAutomatic }

    public sealed class UnifiedWeapon : MonoBehaviour
    {
        [Header("Weapon")]
        [SerializeField] private string displayName = "Weapon";
        [SerializeField] private WeaponMode mode = WeaponMode.Melee;
        [SerializeField] private TriggerMode triggerMode = TriggerMode.PressOnce;
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField, Min(0f)] private float damage = 25f;
        [SerializeField, Min(0.01f)] private float cooldown = 0.35f;
        [SerializeField, Min(0f)] private float range = 2f;
        [SerializeField, Min(0f)] private float impactForce = 4f;

        [Header("Firearm")]
        [SerializeField, Min(0f)] private float spreadDegrees = 0.5f;
        [SerializeField, Min(0)] private int magazineSize = 12;
        [SerializeField, Min(0)] private int startingReserveAmmo = 48;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private GameObject bulletImpactPrefab;

        [Header("Melee")]
        [SerializeField, Min(0f)] private float meleeRadius = 0.75f;

        [Header("Grenade")]
        [SerializeField] private GameObject grenadePrefab;
        [SerializeField, Min(0f)] private float minimumThrowForce = 6f;
        [SerializeField, Min(0f)] private float maximumThrowForce = 18f;
        [SerializeField, Min(0.01f)] private float maximumChargeTime = 1.5f;
        [SerializeField, Min(0f)] private float explosionRadius = 5f;
        [SerializeField, Min(0f)] private float explosionForce = 650f;
        [SerializeField, Min(0f)] private float explosionDamage = 90f;
        [SerializeField] private GameObject explosionEffectPrefab;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string reloadTrigger = "Reload";

        private float nextAttackTime;
        private float chargeStartTime;
        private bool charging;
        private int ammo;
        private int reserveAmmo;

        public string DisplayName => displayName;
        public int Ammo => ammo;
        public int ReserveAmmo => reserveAmmo;
        public bool UsesAmmo => mode == WeaponMode.Firearm;

        private void Awake()
        {
            ammo = magazineSize;
            reserveAmmo = startingReserveAmmo;
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        public void ReadAttack(GameObject owner, bool pressed, bool held, bool released)
        {
            if (mode == WeaponMode.Grenade)
            {
                if (pressed && Time.time >= nextAttackTime)
                {
                    chargeStartTime = Time.time;
                    charging = true;
                }
                if (released && charging) ThrowGrenade(owner);
                return;
            }

            if ((triggerMode == TriggerMode.PressOnce && pressed) ||
                (triggerMode == TriggerMode.HoldAutomatic && held))
                TryAttack(owner);
        }

        public bool TryAttack(GameObject owner)
        {
            if (owner == null || Time.time < nextAttackTime ||
                (mode == WeaponMode.Firearm && ammo <= 0)) return false;

            nextAttackTime = Time.time + cooldown;
            if (mode == WeaponMode.Firearm)
            {
                ammo--;
                Fire(owner);
                if (muzzleFlash != null) muzzleFlash.Play();
            }
            else Melee(owner);

            TriggerAnimation(attackTrigger);
            return true;
        }

        public void Reload()
        {
            if (!UsesAmmo || ammo >= magazineSize || reserveAmmo <= 0) return;
            int loaded = Mathf.Min(magazineSize - ammo, reserveAmmo);
            ammo += loaded;
            reserveAmmo -= loaded;
            TriggerAnimation(reloadTrigger);
        }

        private void Fire(GameObject owner)
        {
            Transform origin = attackOrigin != null ? attackOrigin : transform;
            Vector2 spread = Random.insideUnitCircle * spreadDegrees;
            Vector3 direction = Quaternion.Euler(-spread.y, spread.x, 0f) * origin.forward;
            if (!Physics.Raycast(origin.position, direction, out RaycastHit hit,
                    range, hitMask, QueryTriggerInteraction.Ignore)) return;

            Damage(hit.collider, owner);
            Rigidbody body = hit.rigidbody != null ? hit.rigidbody : hit.collider.GetComponentInParent<Rigidbody>();
            if (body != null && !body.isKinematic)
                body.AddForceAtPosition(direction * impactForce, hit.point, ForceMode.Impulse);
            if (bulletImpactPrefab != null)
                Destroy(Instantiate(bulletImpactPrefab, hit.point,
                    Quaternion.FromToRotation(Vector3.up, hit.normal)), 2f);
        }

        private void Melee(GameObject owner)
        {
            Transform origin = attackOrigin != null ? attackOrigin : transform;
            Collider[] hits = Physics.OverlapSphere(origin.position + origin.forward * range,
                meleeRadius, hitMask, QueryTriggerInteraction.Ignore);
            HashSet<IDamageable> damaged = new HashSet<IDamageable>();
            foreach (Collider hit in hits)
            {
                IDamageable target = hit.GetComponentInParent<IDamageable>();
                if (target == null || !target.IsAlive || damaged.Contains(target) ||
                    hit.transform.root == owner.transform.root) continue;
                damaged.Add(target);
                target.TakeDamage(damage, owner);
                Rigidbody body = hit.attachedRigidbody;
                if (body != null && !body.isKinematic)
                    body.AddForce((hit.bounds.center - origin.position).normalized * impactForce,
                        ForceMode.Impulse);
            }
        }

        private void ThrowGrenade(GameObject owner)
        {
            charging = false;
            if (grenadePrefab == null) { Debug.LogWarning("Assign a grenade prefab.", this); return; }
            float charge = Mathf.Clamp01((Time.time - chargeStartTime) / maximumChargeTime);
            float force = Mathf.Lerp(minimumThrowForce, maximumThrowForce, charge);
            nextAttackTime = Time.time + cooldown;
            Transform origin = attackOrigin != null ? attackOrigin : transform;
            GameObject grenade = Instantiate(grenadePrefab, origin.position + origin.forward * 0.4f, origin.rotation);
            Rigidbody body = grenade.GetComponent<Rigidbody>();
            if (body == null) body = grenade.AddComponent<Rigidbody>();
            body.isKinematic = false;
            body.useGravity = true;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            if (grenade.GetComponentInChildren<Collider>() == null) grenade.AddComponent<SphereCollider>();
            UnifiedGrenadeProjectile projectile = grenade.GetComponent<UnifiedGrenadeProjectile>();
            if (projectile == null) projectile = grenade.AddComponent<UnifiedGrenadeProjectile>();
            projectile.Configure(owner, explosionDamage, explosionRadius, explosionForce, explosionEffectPrefab);
            body.AddForce((origin.forward + Vector3.up * 0.18f).normalized * force, ForceMode.Impulse);
            TriggerAnimation(attackTrigger);
        }

        private void Damage(Collider hit, GameObject owner)
        {
            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target != null && target.IsAlive && hit.transform.root != owner.transform.root)
                target.TakeDamage(damage, owner);
        }

        private void TriggerAnimation(string trigger)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(trigger)) animator.SetTrigger(trigger);
        }

        private void OnDrawGizmosSelected()
        {
            Transform origin = attackOrigin != null ? attackOrigin : transform;
            Gizmos.color = Color.red;
            if (mode == WeaponMode.Melee)
                Gizmos.DrawWireSphere(origin.position + origin.forward * range, meleeRadius);
            else Gizmos.DrawRay(origin.position, origin.forward * range);
        }
    }
}
