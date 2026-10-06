using UnityEngine;
using TheWard.Audio;

namespace TheWard.Combat
{
    public sealed class GrenadeWeapon : Weapon
    {
        [Header("Throw")]
        [SerializeField] private float cooldown = 0.35f;
        [SerializeField] private GameObject grenadePrefab;
        [SerializeField] private float minimumThrowForce = 6f;
        [SerializeField] private float maximumThrowForce = 18f;
        [SerializeField] private float maximumChargeTime = 1.5f;

        [Header("Projectile")]
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private float explosionForce = 650f;
        [SerializeField] private float explosionDamage = 90f;
        [SerializeField] private GameObject explosionEffectPrefab;

        [Header("Throw Animation")]
        [SerializeField] private AnimationClip throwAnimation;
        [SerializeField, Min(0.1f)] private float throwDuration = 0.8f;

        private float nextAttackTime;
        private float chargeStartTime;
        private bool isCharging;
        private float throwForce;
        private Renderer[] grenadeRenderers;

        protected override void Awake()
        {
            base.Awake();
            grenadeRenderers = GetComponentsInChildren<Renderer>();
        }

        public override void HandleInput(bool pressed, bool held, bool released)
        {
            if (isAnimating) return;
            if (pressed && Time.time >= nextAttackTime)
            {
                chargeStartTime = Time.time;
                isCharging = true;
            }

            if (released && isCharging)
            {
                ThrowGrenade();
            }
        }

        private void ThrowGrenade()
        {
            isCharging = false;

            if (grenadePrefab == null)
            {
                Debug.LogWarning("The grenade prefab has not been assigned.");
                return;
            }

            float chargeTime = Time.time - chargeStartTime;
            float chargeAmount = Mathf.Clamp01(chargeTime / maximumChargeTime);
            throwForce = Mathf.Lerp(minimumThrowForce, maximumThrowForce, chargeAmount);

            if (PlayAnimation(throwAnimation, throwDuration))
                nextAttackTime = Time.time + Mathf.Max(cooldown, throwDuration);
        }

        public override void OnAnimationContact()
        {
            if (!isAnimating) return;
            SetGrenadeVisible(false);
            // Play at the animation's release event, when the projectile actually leaves the hand.
            GameAudio.Play(AudioCue.GrenadeThrow, transform.position);

            Transform origin = attackOrigin != null ? attackOrigin : transform;
            Vector3 spawnPosition = origin.position + origin.forward * 0.4f;
            GameObject grenade = Instantiate(grenadePrefab, spawnPosition, origin.rotation);

            Rigidbody grenadeBody = grenade.GetComponent<Rigidbody>();
            if (grenadeBody == null)
            {
                grenadeBody = grenade.AddComponent<Rigidbody>();
            }

            if (grenade.GetComponentInChildren<Collider>() == null)
            {
                grenade.AddComponent<SphereCollider>();
            }

            UnifiedGrenadeProjectile projectile = grenade.GetComponent<UnifiedGrenadeProjectile>();
            if (projectile == null)
            {
                projectile = grenade.AddComponent<UnifiedGrenadeProjectile>();
            }

            Health playerHealth = transform.root.GetComponent<Health>();
            projectile.Setup(
                explosionDamage,
                explosionRadius,
                explosionForce,
                explosionEffectPrefab,
                playerHealth);

            grenadeBody.useGravity = true;
            grenadeBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Vector3 throwDirection = (origin.forward + Vector3.up * 0.18f).normalized;
            grenadeBody.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }

        public override void OnAnimationComplete()
        {
            if (!isAnimating) return;
            SetGrenadeVisible(true);
            base.OnAnimationComplete();
        }

        private void SetGrenadeVisible(bool visible)
        {
            if (grenadeRenderers == null) return;
            foreach (Renderer renderer in grenadeRenderers) renderer.enabled = visible;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            isCharging = false;
            SetGrenadeVisible(true);
        }
    }
}
