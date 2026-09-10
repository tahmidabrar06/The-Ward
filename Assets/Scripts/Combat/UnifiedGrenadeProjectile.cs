using System.Collections.Generic;
using UnityEngine;

namespace TheWard.Combat
{
    public sealed class UnifiedGrenadeProjectile : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float lifetime = 10f;
        private GameObject owner, effectPrefab;
        private float damage, radius, force;
        private bool exploded;

        public void Configure(GameObject source, float amount, float range,
            float explosionForce, GameObject effect)
        { owner = source; damage = amount; radius = range; force = explosionForce; effectPrefab = effect; }

        private void Start() { Destroy(gameObject, lifetime); }
        private void OnCollisionEnter(Collision collision) { Explode(); }

        private void Explode()
        {
            if (exploded) return;
            exploded = true;
            HashSet<IDamageable> damaged = new HashSet<IDamageable>();
            foreach (Collider hit in Physics.OverlapSphere(transform.position, radius,
                         ~0, QueryTriggerInteraction.Ignore))
            {
                Rigidbody body = hit.attachedRigidbody;
                if (body != null && body.gameObject != gameObject)
                    body.AddExplosionForce(force, transform.position, radius, 0.8f, ForceMode.Impulse);
                IDamageable target = hit.GetComponentInParent<IDamageable>();
                if (target == null || !target.IsAlive || damaged.Contains(target)) continue;
                MonoBehaviour component = target as MonoBehaviour;
                if (component != null && owner != null && component.transform.root == owner.transform.root) continue;
                float distance = Vector3.Distance(transform.position, hit.ClosestPoint(transform.position));
                target.TakeDamage(damage * (1f - Mathf.Clamp01(distance / Mathf.Max(radius, 0.01f))), owner);
                damaged.Add(target);
            }
            if (effectPrefab != null) Destroy(Instantiate(effectPrefab, transform.position, Quaternion.identity), 5f);
            Destroy(gameObject);
        }
    }
}
