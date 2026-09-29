using UnityEngine;
using TheWard.Audio;

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
        [SerializeField] private AnimationClip swingAnimation;
        [SerializeField, Min(0.01f)] private float meleeSwingDuration = 0.25f;
        [SerializeField] private bool useForwardSwing;
        [SerializeField, Range(0.2f, 0.55f)] private float swingContactFraction = 0.32727274f;

        public bool UsesForwardSwing => useForwardSwing;
        // Existing bat prefabs are identified by display name; other melee weapons use sword sounds.
        private bool IsBluntWeapon => DisplayName.IndexOf("bat", System.StringComparison.OrdinalIgnoreCase) >= 0;

        public void ApplyForwardSwing(Transform view, float progress)
        {
            // The bat's length runs along local +Y. Rotate about the handle,
            // aiming its barrel through the crosshair at the contact event.
            Quaternion readyRotation = Quaternion.Inverse(view.rotation) *
                transform.parent.rotation * startingRotation;
            Vector3 gripOffset = Vector3.Scale(rightHandGrip.localPosition, transform.localScale);
            Vector3 readyPosition = view.InverseTransformPoint(
                transform.parent.TransformPoint(startingPosition)) + readyRotation * gripOffset;
            Vector3 contactPosition = new Vector3(0.08f, -0.2f, 0.65f);
            Vector3 contactDirection = (new Vector3(0f, 0f, range) - contactPosition).normalized;
            Vector3 direction;
            Vector3 position;
            float windupEnd = swingContactFraction * 0.45f;
            if (progress < windupEnd)
            {
                float t = Mathf.SmoothStep(0f, 1f, progress / windupEnd);
                position = Vector3.Lerp(readyPosition, readyPosition + new Vector3(0.12f, 0.06f, -0.08f), t);
                direction = Vector3.Slerp(readyRotation * Vector3.up, new Vector3(0.85f, 0.5f, -0.1f).normalized, t);
            }
            else if (progress < swingContactFraction)
            {
                float t = Mathf.SmoothStep(0f, 1f,
                    (progress - windupEnd) / (swingContactFraction - windupEnd));
                position = Vector3.Lerp(readyPosition + new Vector3(0.12f, 0.06f, -0.08f), contactPosition, t);
                direction = Vector3.Slerp(new Vector3(0.85f, 0.5f, -0.1f).normalized, contactDirection, t);
            }
            else if (progress < 0.6f)
            {
                float t = Mathf.SmoothStep(0f, 1f, (progress - swingContactFraction) / (0.6f - swingContactFraction));
                position = Vector3.Lerp(contactPosition, new Vector3(-0.22f, -0.32f, 0.55f), t);
                direction = Vector3.Slerp(contactDirection, new Vector3(-0.8f, -0.2f, 0.55f).normalized, t);
            }
            else
            {
                float t = Mathf.SmoothStep(0f, 1f, (progress - 0.6f) / 0.35f);
                position = Vector3.Lerp(new Vector3(-0.22f, -0.32f, 0.55f), readyPosition, t);
                direction = Vector3.Slerp(new Vector3(-0.8f, -0.2f, 0.55f).normalized, readyRotation * Vector3.up, t);
            }
            Quaternion rotation = Quaternion.FromToRotation(readyRotation * Vector3.up, direction) * readyRotation;
            transform.rotation = view.rotation * rotation;
            transform.position = view.TransformPoint(position - rotation * gripOffset);
        }

        private float nextAttackTime;

        public override void HandleInput(bool pressed, bool held, bool released)
        {
            if (!pressed || isAnimating || Time.time < nextAttackTime) return;
            if (PlayAnimation(swingAnimation, meleeSwingDuration))
            {
                nextAttackTime = Time.time + Mathf.Max(cooldown, meleeSwingDuration);
                GameAudio.Play(IsBluntWeapon ? AudioCue.BatSwing : AudioCue.SwordSwing, transform.position);
            }
        }

        public override void OnAnimationContact()
        {
            if (!isAnimating) return;
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
            if (target != null)
            {
                // Swing sounds can miss, but impact sounds require a confirmed target.
                GameAudio.Play(IsBluntWeapon ? AudioCue.BatHit : AudioCue.SwordHit, target.transform.position, true);
                target.TakeDamage(damage);
                if (HitIndicator.Instance != null)
                {
                    if (!target.IsDead()) HitIndicator.Instance.ShowHit(false);
                    else HitIndicator.Instance.ShowHit(true);
                }
            }
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
