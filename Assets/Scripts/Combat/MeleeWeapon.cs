using System.Collections;
using UnityEngine;

namespace TheWard.Combat
{
    public sealed class MeleeWeapon : Weapon
    {
        [Header("Hit Detection")]
        [SerializeField] private float damage = 25f;
        [SerializeField, Min(0f)] private float cooldown = 0.35f;
        [SerializeField, Min(0f)] private float range = 2f;
        [SerializeField, Min(0.01f)] private float meleeRadius = 0.75f;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Swing")]
        [SerializeField] private Vector3 meleeSwingEuler = new Vector3(55f, -20f, -25f);
        [SerializeField, Min(0.01f)] private float meleeSwingDuration = 0.25f;

        private float nextAttackTime;

        public override void HandleInput(bool pressed, bool held, bool released)
        {
            if (!pressed || isAnimating || Time.time < nextAttackTime) return;
            nextAttackTime = Time.time + Mathf.Max(cooldown, meleeSwingDuration);
            StartCoroutine(Swing());
        }

        private IEnumerator Swing()
        {
            isAnimating = true;
            Quaternion swingRotation = startingRotation * Quaternion.Euler(meleeSwingEuler);
            // A quick strike followed by recovery keeps contact close to the click.
            yield return MoveWeapon(startingPosition, swingRotation, meleeSwingDuration * 0.25f);
            Hit();
            yield return MoveWeapon(startingPosition, startingRotation, meleeSwingDuration * 0.75f);
            isAnimating = false;
        }

        private void Hit()
        {
            Ray aim = GetAimRay();
            // Cover the whole reach, including enemies close to the player.
            Collider[] candidates = Physics.OverlapCapsule(aim.origin, aim.GetPoint(range),
                meleeRadius, hitMask, QueryTriggerInteraction.Ignore);
            Health target = null;
            float nearestDistance = range;

            foreach (Collider candidate in candidates)
            {
                if (candidate.transform.root == transform.root) continue;
                Health health = candidate.GetComponentInParent<Health>();
                if (health == null || !health.IsAlive) continue;

                Vector3 point = candidate.ClosestPoint(aim.origin);
                Vector3 offset = point - aim.origin;
                float distance = offset.magnitude;
                if (Vector3.Dot(aim.direction, offset) < 0f || distance > nearestDistance) continue;
                if (IsBlocked(aim.origin, point, health)) continue;

                nearestDistance = distance;
                target = health;
            }

            // Multiple colliders on the same zombie still receive one hit.
            if (target != null) target.TakeDamage(damage);
        }

        private bool IsBlocked(Vector3 origin, Vector3 point, Health target)
        {
            Vector3 offset = point - origin;
            foreach (RaycastHit hit in Physics.RaycastAll(origin, offset.normalized,
                offset.magnitude, hitMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.root == transform.root) continue;
                if (hit.collider.GetComponentInParent<Health>() != target) return true;
            }
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Ray aim = GetAimRay();
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(aim.origin, meleeRadius);
            Gizmos.DrawWireSphere(aim.GetPoint(range), meleeRadius);
            Gizmos.DrawLine(aim.origin, aim.GetPoint(range));
        }
    }
}
