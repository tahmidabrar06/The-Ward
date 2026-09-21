using System.Collections;
using UnityEngine;

namespace TheWard.Combat
{
    public enum WeaponMode { Firearm, Melee, Grenade }
    public enum TriggerMode { PressOnce, HoldAutomatic }

    // One simple weapon script is used for the pistol, machine gun, bat and grenade.
    public class UnifiedWeapon : MonoBehaviour
    {
        [Header("Basic Weapon Settings")]
        [SerializeField] private string displayName = "Weapon";
        [SerializeField] private WeaponMode mode = WeaponMode.Melee;
        [SerializeField] private TriggerMode triggerMode = TriggerMode.PressOnce;
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float cooldown = 0.35f;
        [SerializeField] private float range = 2f;
        [SerializeField] private float impactForce = 4f;

        [Header("Gun Settings")]
        [SerializeField] private float spreadDegrees = 0.5f;
        [SerializeField] private int magazineSize = 12;
        [SerializeField] private int startingReserveAmmo = 48;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private GameObject bulletImpactPrefab;
        [SerializeField] private float gunRecoilDistance = 0.05f;
        [SerializeField] private float gunRecoilAngle = 6f;
        [SerializeField] private bool showShotTracer = true;
        [SerializeField] private bool aimHeldGunAtCrosshair = true;
        [SerializeField] private bool muzzlePointsBackward;

        [Header("Bat Settings")]
        [SerializeField] private bool playBatStrike;
        [SerializeField] private bool playSwordSlash;
        [SerializeField] private float meleeRadius = 0.75f;
        [SerializeField] private Vector3 meleeSwingEuler = new Vector3(55f, -20f, -25f);
        [SerializeField] private float meleeSwingDuration = 0.25f;

        [Header("Grenade Settings")]
        [SerializeField] private GameObject grenadePrefab;
        [SerializeField] private float minimumThrowForce = 6f;
        [SerializeField] private float maximumThrowForce = 18f;
        [SerializeField] private float maximumChargeTime = 1.5f;
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private float explosionForce = 650f;
        [SerializeField] private float explosionDamage = 90f;
        [SerializeField] private GameObject explosionEffectPrefab;
        [SerializeField, Min(0f)] private float grenadeReleaseDelay = 0.9f;

        [Header("Simple View Animations")]
        [SerializeField] private Vector3 grenadeChargeOffset = new Vector3(0f, -0.08f, -0.12f);
        [SerializeField] private Vector3 grenadeChargeEuler = new Vector3(18f, 0f, -12f);

        private float nextAttackTime;
        private float chargeStartTime;
        private bool isCharging;
        private int ammo;
        private int reserveAmmo;
        private Vector3 startingPosition;
        private Quaternion startingRotation;
        private Coroutine recoilRoutine;
        private Material tracerMaterial;

        public string DisplayName => displayName;
        public WeaponMode Mode => mode;
        public int Ammo => ammo;
        public int ReserveAmmo => reserveAmmo;

        private void Awake()
        {
            ammo = magazineSize;
            reserveAmmo = startingReserveAmmo;
            startingPosition = transform.localPosition;
            startingRotation = transform.localRotation;
        }

        private void LateUpdate()
        {
            // The animated hand controls the gun's position, but the gun barrel
            // must still follow the camera aim. These two models face opposite
            // directions in their source files, hence muzzlePointsBackward.
            if (mode != WeaponMode.Firearm || !aimHeldGunAtCrosshair || attackOrigin == null)
                return;

            Vector3 forward = muzzlePointsBackward ? -attackOrigin.forward : attackOrigin.forward;
            transform.rotation = Quaternion.LookRotation(forward, attackOrigin.up);
        }

        // The weapon controller calls this method every frame.
        public void HandleInput(bool pressed, bool held, bool released)
        {
            if (mode == WeaponMode.Grenade)
            {
                HandleGrenadeInput(pressed, held, released);
                return;
            }

            bool shouldAttack = false;

            if (triggerMode == TriggerMode.PressOnce && pressed)
            {
                shouldAttack = true;
            }
            else if (triggerMode == TriggerMode.HoldAutomatic && held)
            {
                shouldAttack = true;
            }

            if (shouldAttack)
            {
                Attack();
            }
        }

        private void Attack()
        {
            if (Time.time < nextAttackTime)
            {
                return;
            }

            if (mode == WeaponMode.Firearm && ammo <= 0)
            {
                return;
            }

            nextAttackTime = Time.time + cooldown;

            if (mode == WeaponMode.Firearm)
            {
                ammo--;
                Shoot();
            }
            else if (mode == WeaponMode.Melee)
            {
                MeleeAttack();
                // A weapon held by the animated hand follows the arm swing.
                // Do not rotate it a second time or the strike looks exaggerated.
                if (playBatStrike)
                {
                    // The player owns the Animator so movement and attacks cannot compete.
                    TheWard.Character.PlayerCharacterController player =
                        transform.root.GetComponent<TheWard.Character.PlayerCharacterController>();
                    if (player != null)
                    {
                        player.PlayAttackAnimation();
                    }
                }
                else if (playSwordSlash)
                {
                    TheWard.Character.PlayerCharacterController player =
                        transform.root.GetComponent<TheWard.Character.PlayerCharacterController>();
                    if (player != null) player.PlaySwordSlash();
                }
                else
                {
                    StartCoroutine(SwingBat());
                }
            }
        }

        private void Shoot()
        {
            Transform origin = attackOrigin != null ? attackOrigin : transform;

            // The random angle gives the machine gun its spread.
            Vector2 spread = Random.insideUnitCircle * spreadDegrees;
            Vector3 direction = Quaternion.Euler(-spread.y, spread.x, 0f) * origin.forward;

            // Ignore the player's capsule and the gun itself. A held model can
            // sit in front of the camera and otherwise absorb every shot.
            RaycastHit hit = default;
            bool hitSomething = false;
            float nearestDistance = range;
            RaycastHit[] candidates = Physics.RaycastAll(
                origin.position, direction, range, hitMask,
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

            TheWard.Character.PlayerCharacterController player =
                transform.root.GetComponent<TheWard.Character.PlayerCharacterController>();
            if (player != null) player.PlayGunShot(triggerMode == TriggerMode.HoldAutomatic);
            if (recoilRoutine != null) StopCoroutine(recoilRoutine);
            recoilRoutine = StartCoroutine(GunRecoil());

            // A raycast applies damage instantly, so show where the shot went.
            if (showShotTracer)
            {
                Vector3 endPoint = hitSomething ? hit.point : origin.position + direction * range;
                ShowShotTracer(origin, endPoint);
            }

            if (!hitSomething)
            {
                return;
            }

            // Damage the enemy if it has a Health component.
            Health enemyHealth = hit.collider.GetComponentInParent<Health>();
            if (enemyHealth != null && enemyHealth.transform.root != transform.root)
            {
                enemyHealth.TakeDamage(damage);
            }

            // Push barrels and other physics objects.
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

        private void MeleeAttack()
        {
            Transform origin = attackOrigin != null ? attackOrigin : transform;
            Vector3 attackPosition = origin.position + origin.forward * range;

            Collider[] objectsHit = Physics.OverlapSphere(
                attackPosition,
                meleeRadius,
                hitMask,
                QueryTriggerInteraction.Ignore);

            foreach (Collider objectHit in objectsHit)
            {
                Health enemyHealth = objectHit.GetComponentInParent<Health>();

                if (enemyHealth != null && enemyHealth.transform.root != transform.root)
                {
                    enemyHealth.TakeDamage(damage);
                    break;
                }
            }
        }

        private void HandleGrenadeInput(bool pressed, bool held, bool released)
        {
            if (pressed && Time.time >= nextAttackTime)
            {
                chargeStartTime = Time.time;
                isCharging = true;
            }

            if (held && isCharging)
            {
                ShowGrenadeCharge();
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
            float throwForce = Mathf.Lerp(minimumThrowForce, maximumThrowForce, chargeAmount);

            // Start the arm motion now, but wait for its release pose before spawning.
            TheWard.Character.PlayerCharacterController player =
                transform.root.GetComponent<TheWard.Character.PlayerCharacterController>();
            if (player != null) player.PlayGrenadeThrow();
            ResetWeaponTransform();
            nextAttackTime = Time.time + Mathf.Max(cooldown, grenadeReleaseDelay + 0.8f);
            StartCoroutine(SpawnGrenadeAfterWindup(throwForce));
        }

        private IEnumerator SpawnGrenadeAfterWindup(float throwForce)
        {
            yield return new WaitForSeconds(grenadeReleaseDelay);

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

        public void Reload()
        {
            if (mode != WeaponMode.Firearm || ammo >= magazineSize || reserveAmmo <= 0)
            {
                return;
            }

            int bulletsNeeded = magazineSize - ammo;
            int bulletsToLoad = Mathf.Min(bulletsNeeded, reserveAmmo);

            ammo += bulletsToLoad;
            reserveAmmo -= bulletsToLoad;

            TheWard.Character.PlayerCharacterController player =
                transform.root.GetComponent<TheWard.Character.PlayerCharacterController>();
            if (player != null) player.PlayGunReload();
        }

        private IEnumerator SwingBat()
        {
            Quaternion swingRotation = startingRotation * Quaternion.Euler(meleeSwingEuler);
            float halfTime = meleeSwingDuration / 2f;

            yield return RotateWeapon(startingRotation, swingRotation, halfTime);
            yield return RotateWeapon(swingRotation, startingRotation, halfTime);

            transform.localRotation = startingRotation;
        }

        private IEnumerator GunRecoil()
        {
            // The arm clip animates the player; this short motion makes the gun
            // itself visibly kick on each shot, including automatic fire.
            Vector3 kickedPosition = startingPosition + Vector3.back * gunRecoilDistance;
            Quaternion kickedRotation = startingRotation * Quaternion.Euler(-gunRecoilAngle, 0f, 0f);
            transform.localPosition = kickedPosition;
            if (!aimHeldGunAtCrosshair) transform.localRotation = kickedRotation;

            float timer = 0f;
            while (timer < 0.12f)
            {
                timer += Time.deltaTime;
                float amount = Mathf.Clamp01(timer / 0.12f);
                transform.localPosition = Vector3.Lerp(kickedPosition, startingPosition, amount);
                if (!aimHeldGunAtCrosshair)
                    transform.localRotation = Quaternion.Slerp(kickedRotation, startingRotation, amount);
                yield return null;
            }

            ResetWeaponTransform();
            recoilRoutine = null;
        }

        private void ShowShotTracer(Transform origin, Vector3 endPoint)
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
            line.SetPosition(0, origin.position + origin.right * 0.22f - origin.up * 0.14f + origin.forward * 0.35f);
            line.SetPosition(1, endPoint);
            Destroy(tracer, 0.08f);
        }

        private IEnumerator RotateWeapon(Quaternion from, Quaternion to, float duration)
        {
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                transform.localRotation = Quaternion.Lerp(from, to, timer / duration);
                yield return null;
            }
        }

        private void ShowGrenadeCharge()
        {
            float chargeTime = Time.time - chargeStartTime;
            float chargeAmount = Mathf.Clamp01(chargeTime / maximumChargeTime);

            transform.localPosition = Vector3.Lerp(
                startingPosition,
                startingPosition + grenadeChargeOffset,
                chargeAmount);

            transform.localRotation = Quaternion.Lerp(
                startingRotation,
                startingRotation * Quaternion.Euler(grenadeChargeEuler),
                chargeAmount);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            recoilRoutine = null;
            isCharging = false;
            ResetWeaponTransform();
        }

        private void OnDestroy()
        {
            if (tracerMaterial != null) Destroy(tracerMaterial);
        }

        private void ResetWeaponTransform()
        {
            transform.localPosition = startingPosition;
            transform.localRotation = startingRotation;
        }

        private void OnDrawGizmosSelected()
        {
            Transform origin = attackOrigin != null ? attackOrigin : transform;
            Gizmos.color = Color.red;

            if (mode == WeaponMode.Melee)
            {
                Gizmos.DrawWireSphere(origin.position + origin.forward * range, meleeRadius);
            }
            else
            {
                Gizmos.DrawRay(origin.position, origin.forward * range);
            }
        }
    }
}
