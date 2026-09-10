using UnityEngine;

namespace TheWard.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }

        void TakeDamage(float amount, GameObject source);
    }
}