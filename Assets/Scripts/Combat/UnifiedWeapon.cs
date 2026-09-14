using System.Collections;
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
        [SerializeField] private Vector3 meleeSwingEuler = new Vector3(55f, -20f, -25f);
        [SerializeField, Min(0.05f)] private float meleeSwingDuration = 0.25f;

        [Header("Grenade")]
        [SerializeField] private GameObject grenadePrefab;
        [SerializeField, Min(0f)] private float minimumThrowForce = 6f;
        [SerializeField, Min(0f)] private float maximumThrowForce = 18f;
        [SerializeField, Min(0.01f)] private float maximumChargeTime = 1.5f;
        [SerializeField, Min(0f)] private float explosionRadius = 5f;
        [SerializeField, Min(0f)] private float explosionForce = 650f;
        [SerializeField, Min(0f)] private float explosionDamage = 90f;
        [SerializeField] private GameObject explosionEffectPrefab;

        [Header("Grenade View Animation")]
        [SerializeField] private Vector3 grenadeChargeOffset = new Vector3(0f, -0.08f, -0.12f);
        [SerializeField] private Vector3 grenadeChargeEuler = new Vector3(18f, 0f, -12f);
        [SerializeField] private Vector3 grenadeReleaseOffset = new Vector3(0f, 0.04f, 0.18f);
        [SerializeField, Min(0.05f)] private float grenadeReleaseDuration = 0.2f;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string reloadTrigger = "Reload";

        private float nextAttackTime;
        private float chargeStartTime;
        private bool charging;
        private int ammo;
        private int reserveAmmo;
        private Quaternion restLocalRotation;
        private Vector3 restLocalPosition;
        private Coroutine meleeSwingRoutine;
        private Coroutine grenadeReleaseRoutine;

        public string DisplayName => displayName;
        public int Ammo => ammo;
        public int ReserveAmmo => reserveAmmo;
        public bool UsesAmmo => mode == WeaponMode.Firearm;

        private void Awake()
        {
            ammo = magazineSize;
            reserveAmmo = startingReserveAmmo;
            restLocalRotation = transform.localRotation;
            restLocalPosition = transform.localPosition;
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        public void ReadAttack(GameObject owner, bool pressed, bool held, bool released)
        {
            if (mode == WeaponMode.Grenade)
            {
                if (pressed && Time.time >= nextAttackTime)
                {
                    if (grenadeReleaseRoutine != null) StopCoroutine(grenadeReleaseRoutine);
                    chargeStartTime = Time.time;
                    charging = true;
                }
                if (held && charging) AnimateGrenadeCharge();
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
            else
            {
                Melee(owner);
                if (animator == null)
                {
                    if (meleeSwingRoutine != null) StopCoroutine(meleeSwingRoutine);
                    meleeSwingRoutine = StartCoroutine(AnimateMeleeSwing());
                }
            }

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
            grenadeReleaseRoutine = StartCoroutine(AnimateGrenadeRelease());
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

        private IEnumerator AnimateMeleeSwing()
        {
            Quaternion swingRotation = restLocalRotation * Quaternion.Euler(meleeSwingEuler);
            float halfDuration = meleeSwingDuration * 0.5f;

            for (float elapsed = 0f; elapsed < halfDuration; elapsed += Time.deltaTime)
            {
                transform.localRotation = Quaternion.Slerp(restLocalRotation, swingRotation,
                    elapsed / halfDuration);
                yield return null;
            }

            for (float elapsed = 0f; elapsed < halfDuration; elapsed += Time.deltaTime)
            {
                transform.localRotation = Quaternion.Slerp(swingRotation, restLocalRotation,
                    elapsed / halfDuration);
                yield return null;
            }

            transform.localRotation = restLocalRotation;
            meleeSwingRoutine = null;
        }

        private void AnimateGrenadeCharge()
        {
            float charge = Mathf.Clamp01((Time.time - chargeStartTime) / maximumChargeTime);
            float easedCharge = Mathf.SmoothStep(0f, 1f, charge);
            transform.localPosition = Vector3.Lerp(restLocalPosition,
                restLocalPosition + grenadeChargeOffset, easedCharge);
            transform.localRotation = Quaternion.Slerp(restLocalRotation,
                restLocalRotation * Quaternion.Euler(grenadeChargeEuler), easedCharge);
        }

        private IEnumerator AnimateGrenadeRelease()
        {
            Vector3 chargedPosition = transform.localPosition;
            Quaternion chargedRotation = transform.localRotation;
            Vector3 releasePosition = restLocalPosition + grenadeReleaseOffset;
            float halfDuration = grenadeReleaseDuration * 0.5f;

            for (float elapsed = 0f; elapsed < halfDuration; elapsed += Time.deltaTime)
            {
                float t = elapsed / halfDuration;
                transform.localPosition = Vector3.Lerp(chargedPosition, releasePosition, t);
                transform.localRotation = Quaternion.Slerp(chargedRotation, restLocalRotation, t);
                yield return null;
            }

            for (float elapsed = 0f; elapsed < halfDuration; elapsed += Time.deltaTime)
            {
                transform.localPosition = Vector3.Lerp(releasePosition, restLocalPosition,
                    elapsed / halfDuration);
                yield return null;
            }

            transform.localPosition = restLocalPosition;
            transform.localRotation = restLocalRotation;
            grenadeReleaseRoutine = null;
        }

        private void OnDisable()
        {
            if (meleeSwingRoutine != null) StopCoroutine(meleeSwingRoutine);
            if (grenadeReleaseRoutine != null) StopCoroutine(grenadeReleaseRoutine);
            meleeSwingRoutine = null;
            grenadeReleaseRoutine = null;
            transform.localPosition = restLocalPosition;
            transform.localRotation = restLocalRotation;
            charging = false;
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
