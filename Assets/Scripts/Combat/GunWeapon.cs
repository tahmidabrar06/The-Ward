using System.Collections;
using TheWard.Audio;
using UnityEngine;

namespace TheWard.Combat
{
    public enum TriggerMode { PressOnce, HoldAutomatic }

    public enum GunSoundProfile { Automatic, Handgun, AssaultRifle, Shotgun }

    public sealed class GunWeapon : Weapon
    {
        [Header("UI")]
        [SerializeField] private AmmoHUD ammoUI;
        
        [Header("Shooting")]
        [SerializeField] private TriggerMode triggerMode = TriggerMode.PressOnce;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float cooldown = 0.35f;
        [SerializeField] private float range = 60f;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private float impactForce = 4f;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private int magazineSize = 12;
        [SerializeField] private int startingReserveAmmo = 48;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private GameObject bulletImpactPrefab;
        [SerializeField] private bool showShotTracer = true;

        [Header("Recoil and Reload")]
        [SerializeField] private float gunRecoilDistance = 0.05f;
        [SerializeField] private float gunRecoilAngle = 6f;
        [SerializeField, Min(0.1f)] private float reloadDuration = 1.2f;
        [SerializeField] private AnimationClip reloadAnimation;

        [Header("Audio")]
        [SerializeField] private GunSoundProfile soundProfile;
        // Automatic picks by firing mode; an Inspector profile can override it for a specific gun.
        private AudioCue GetGunSound(AudioCue handgun, AudioCue rifle, AudioCue shotgun)
        {
            GunSoundProfile profile = soundProfile;
            if (profile == GunSoundProfile.Automatic)
            {
                profile = GunSoundProfile.Handgun;
                if (triggerMode == TriggerMode.HoldAutomatic) profile = GunSoundProfile.AssaultRifle;
            }

            switch (profile)
            {
                case GunSoundProfile.AssaultRifle:
                    return rifle;
                case GunSoundProfile.Shotgun:
                    return shotgun;
                default:
                    return handgun;
            }
        }

        public void PlayEquipSound()
        {
            AudioCue sound = GetGunSound(AudioCue.HandgunEquip, AudioCue.RifleEquip, AudioCue.ShotgunEquip);
            GameAudio.Play(sound, transform.position);
        }

        private int ammo;
        private int reserveAmmo;
        private float nextAttackTime;
        private bool hasPendingAmmoRestore;
        private bool hasInitializedAmmo;
        private int pendingAmmo;
        private int pendingReserveAmmo;
        private Coroutine recoilRoutine;
        private Material tracerMaterial;

        public int Ammo => ammo;
        public int ReserveAmmo => reserveAmmo;

        // adding the reload ammo when picking up the same gun weapon, but not exceeding the maximum reserve ammo limit
        
        public void RefillAmmo()
        {
            ammo = magazineSize;
            reserveAmmo = startingReserveAmmo;

            if (ammoUI != null)
            {
                ammoUI.UpdateAmmoState(ammo, reserveAmmo, magazineSize);
            }
        }


        protected override void Awake()
        {
            base.Awake();

            if (hasPendingAmmoRestore)
            {
                ammo = Mathf.Clamp(pendingAmmo, 0, magazineSize);
                reserveAmmo = Mathf.Max(0, pendingReserveAmmo);
                hasPendingAmmoRestore = false;
            }
            else
            {
                ammo = magazineSize;
                reserveAmmo = startingReserveAmmo;
            }

            hasInitializedAmmo = true;
        }
        
        private void OnEnable()
        {
            if (ammoUI != null)
            {
                ammoUI.EnableAmmoText();
                ammoUI.UpdateAmmoState(ammo, reserveAmmo, magazineSize);
            }
        }
        
        public override void HandleInput(bool pressed, bool held, bool released)
        {
            bool firing = triggerMode == TriggerMode.HoldAutomatic ? held : pressed;
            if (!firing || isAnimating || Time.time < nextAttackTime || ammo <= 0) return;
            nextAttackTime = Time.time + cooldown;
            ammo--;
            Shoot();
            
            if (ammoUI != null) ammoUI.UpdateAmmoState(ammo, reserveAmmo, magazineSize);
        }

        private void Shoot()
        {
            AudioCue sound = GetGunSound(AudioCue.HandgunShot, AudioCue.RifleShot, AudioCue.ShotgunShot);
            GameAudio.Play(sound, transform.position);
            Ray aim = GetAimRay();
            Vector3 direction = aim.direction;

            // Ignore the player's capsule and the gun itself. A held model can
            // sit in front of the camera and otherwise absorb every shot.
            RaycastHit hit = default;
            bool hitSomething = false;
            float nearestDistance = range;
            RaycastHit[] candidates = Physics.RaycastAll(
                aim.origin, direction, range, hitMask,
                QueryTriggerInteraction.Ignore);
            foreach (RaycastHit candidate in candidates)
            {
                if (candidate.collider.transform.root == transform.root ||
                    candidate.distance >= nearestDistance)
                {
                    continue;
                }

                hit = candidate;
                nearestDistance = candidate.distance;
                hitSomething = true;
            }

            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            if (recoilRoutine != null) StopCoroutine(recoilRoutine);
            recoilRoutine = StartCoroutine(GunRecoil());

            if (showShotTracer)
            {
                Vector3 endPoint = hitSomething ? hit.point : aim.origin + direction * range;
                ShowShotTracer(endPoint);
            }

            if (!hitSomething)
            {
                return;
            }

            Health enemyHealth = hit.collider.GetComponentInParent<Health>();
            if (enemyHealth != null && enemyHealth.transform.root != transform.root)
            {
                enemyHealth.TakeDamage(damage);
                if (HitIndicator.Instance != null)
                {
                    if (!enemyHealth.IsDead()) HitIndicator.Instance.ShowHit(false);
                    else HitIndicator.Instance.ShowHit(true);
                }
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(direction * impactForce, hit.point, ForceMode.Impulse);
            }

            if (bulletImpactPrefab != null)
            {
                Quaternion effectRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                GameObject effect = Instantiate(bulletImpactPrefab, hit.point, effectRotation);
                Destroy(effect, 2f);
            }
        }

        public override void Reload()
        {
            if (isAnimating || ammo >= magazineSize || reserveAmmo <= 0)
            {
                return;
            }

            if (recoilRoutine != null) StopCoroutine(recoilRoutine);
            recoilRoutine = null;
            ResetWeaponTransform();
            if (PlayAnimation(reloadAnimation, reloadDuration))
            {
                AudioCue sound = GetGunSound(AudioCue.HandgunReload, AudioCue.RifleReload, AudioCue.ShotgunReload);
                // Tie reload audio to this weapon so switching away can stop it.
                StartCoroutine(GameAudio.PlayReload(sound, transform, reloadDuration, this));
            }
        }

        public override void OnAnimationComplete()
        {
            if (!isAnimating) return;
            int bulletsNeeded = magazineSize - ammo;
            int bulletsToLoad = Mathf.Min(bulletsNeeded, reserveAmmo);

            ammo += bulletsToLoad;
            reserveAmmo -= bulletsToLoad;
            
            if (ammoUI != null) ammoUI.UpdateAmmoState(ammo, reserveAmmo, magazineSize);
            
            base.OnAnimationComplete();
        }

        private IEnumerator GunRecoil()
        {
            Vector3 kickedPosition = startingPosition + Vector3.back * gunRecoilDistance;
            Quaternion kickedRotation = startingRotation * Quaternion.Euler(-gunRecoilAngle, 0f, 0f);
            transform.localPosition = kickedPosition;
            transform.localRotation = kickedRotation;

            float timer = 0f;
            while (timer < 0.12f)
            {
                timer += Time.deltaTime;
                float amount = Mathf.Clamp01(timer / 0.12f);
                transform.localPosition = Vector3.Lerp(kickedPosition, startingPosition, amount);
                transform.localRotation = Quaternion.Slerp(kickedRotation, startingRotation, amount);
                yield return null;
            }

            ResetWeaponTransform();
            recoilRoutine = null;
        }

        private void ShowShotTracer(Vector3 endPoint)
        {
            if (tracerMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) return;
                tracerMaterial = new Material(shader);
                tracerMaterial.color = new Color(1f, 0.85f, 0.25f);
            }

            GameObject tracer = new GameObject("Shot Tracer");
            LineRenderer line = tracer.AddComponent<LineRenderer>();
            line.sharedMaterial = tracerMaterial;
            line.positionCount = 2;
            line.startWidth = 0.018f;
            line.endWidth = 0.003f;
            line.startColor = Color.yellow;
            line.endColor = Color.yellow;
            line.SetPosition(0, muzzlePoint != null ? muzzlePoint.position : transform.position);
            line.SetPosition(1, endPoint);
            Destroy(tracer, 0.08f);
        }

        private void OnDestroy()
        {
            if (tracerMaterial != null) Destroy(tracerMaterial);
        }

        protected override void OnDisable()
        {
            // Reloads are cancelled on a weapon switch; fired shots are allowed to finish.
            GameAudio.StopOwned(this);
            base.OnDisable();
            recoilRoutine = null;
            if (ammoUI != null)
            {
                ammoUI.DisableAmmoText();
            }
        }
        public void RestoreAmmoState(int currentAmmo, int currentReserveAmmo)
        {
            pendingAmmo = Mathf.Clamp(currentAmmo, 0, magazineSize);
            pendingReserveAmmo = Mathf.Max(0, currentReserveAmmo);

            if (hasInitializedAmmo)
            {
                ammo = pendingAmmo;
                reserveAmmo = pendingReserveAmmo;
            }
            else
            {
                hasPendingAmmoRestore = true;
            }

            if (ammoUI != null && hasInitializedAmmo)
            {
                ammoUI.UpdateAmmoState(
                    ammo,
                    reserveAmmo,
                    magazineSize);
            }
        }
    }
}
