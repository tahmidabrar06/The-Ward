using UnityEngine;

namespace TheWard.Combat
{
    // Controls the grenade after the player throws it.
    public class UnifiedGrenadeProjectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 10f;

        private float damage;
        private float explosionRadius;
        private float explosionForce;
        private GameObject explosionEffect;
        private Health ownerHealth;

        // Gets the explosion values from the equipped grenade weapon.
        public void Setup(
            float newDamage,
            float newRadius,
            float newForce,
            GameObject newEffect,
            Health newOwnerHealth)
        {
            damage = newDamage;
            explosionRadius = newRadius;
            explosionForce = newForce;
            explosionEffect = newEffect;
            ownerHealth = newOwnerHealth;
        }

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            Explode();
        }

        private void Explode()
        {
            // Create the visible explosion.
            if (explosionEffect != null)
            {
                GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                Destroy(effect, 5f);
            }

            // Find objects inside the explosion radius.
            Collider[] objectsHit = Physics.OverlapSphere(transform.position, explosionRadius);

            foreach (Collider objectHit in objectsHit)
            {
                Rigidbody body = objectHit.attachedRigidbody;

                if (body != null && body.gameObject != gameObject)
                {
                    body.AddExplosionForce(
                        explosionForce,
                        transform.position,
                        explosionRadius,
                        1f,
                        ForceMode.Impulse);
                }

                Health health = objectHit.GetComponentInParent<Health>();

                // The player's own grenade must not damage the player.
                if (health != null && health != ownerHealth)
                {
                    health.TakeDamage(damage);
                }
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
