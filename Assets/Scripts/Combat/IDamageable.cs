namespace TheWard.Combat
{
    // Any object with this interface can receive damage.
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}
