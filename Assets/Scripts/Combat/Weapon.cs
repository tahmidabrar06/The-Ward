using UnityEngine;

namespace TheWard.Combat
{
    // A simple melee weapon used by the older WeaponController script.
    public class Weapon : MonoBehaviour
    {
        [Header("Melee Attack")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private float attackCooldown = 0.35f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackRadius = 0.6f;
        [SerializeField] private LayerMask hitMask = ~0;

        // The next time, in seconds, that another attack is allowed.
        private float nextAllowedAttackTime;

        // Returns true when an attack happens and false while cooling down.
        public bool TryAttack(GameObject owner)
        {
            if (Time.time < nextAllowedAttackTime)
            {
                return false;
            }

            // Start the cooldown before checking for a target.
            nextAllowedAttackTime = Time.time + attackCooldown;

            // Check a small sphere in front of the weapon.
            Vector3 attackPosition = GetAttackPosition();
            Collider[] objectsHit = Physics.OverlapSphere(
                attackPosition,
                attackRadius,
                hitMask,
                QueryTriggerInteraction.Ignore);

            foreach (Collider objectHit in objectsHit)
            {
                Health targetHealth = objectHit.GetComponentInParent<Health>();

                // Ignore objects without health and prevent the player hitting themself.
                if (targetHealth != null && targetHealth.transform.root != owner.transform.root)
                {
                    targetHealth.TakeDamage(damage);
                    break;
                }
            }

            return true;
        }

        private Vector3 GetAttackPosition()
        {
            return transform.position + transform.forward * attackRange;
        }

        private void OnDrawGizmosSelected()
        {
            // Shows the melee hit area in the Scene view.
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetAttackPosition(), attackRadius);
        }
    }
}
