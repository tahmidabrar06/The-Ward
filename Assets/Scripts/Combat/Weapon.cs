using System.Collections.Generic;
using UnityEngine;

namespace TheWard.Combat
{
    public sealed class Weapon : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float damage = 25f;
        [SerializeField, Min(0.01f)] private float attackCooldown = 0.35f;
        [SerializeField, Min(0f)] private float attackRange = 2f;
        [SerializeField, Min(0f)] private float attackRadius = 0.6f;
        [SerializeField] private LayerMask hitMask = ~0;

        private float nextAttackTime;

        public bool TryAttack(GameObject owner)
        {
            if (Time.time < nextAttackTime)
            {
                return false;
            }

            nextAttackTime = Time.time + attackCooldown;
            Vector3 origin = transform.position;
            Vector3 direction = transform.forward;
            Collider[] hits = Physics.OverlapSphere(origin + direction * attackRange, attackRadius, hitMask);
            HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

            foreach (Collider hit in hits)
            {
                IDamageable target = hit.GetComponentInParent<IDamageable>();
                if (target == null || damagedTargets.Contains(target) || hit.transform.root == owner.transform.root)
                {
                    continue;
                }

                damagedTargets.Add(target);
                target.TakeDamage(damage, owner);
            }

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, attackRadius);
        }
    }
}