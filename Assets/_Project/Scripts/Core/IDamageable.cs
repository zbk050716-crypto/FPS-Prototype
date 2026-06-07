/// <summary>
/// Interface for any object that can take damage from weapons.
/// Implement on EnemyAI, Boss, Turret, DestructibleObject, etc.
/// </summary>
public interface IDamageable
{
    void TakeDamage(int damage);
}
