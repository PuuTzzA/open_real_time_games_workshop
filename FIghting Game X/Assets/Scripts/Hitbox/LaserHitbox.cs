using UnityEngine;

public class LaserHitbox : Hitbox
{
    [SerializeField] int damage;

    private bool isActive = false;

    public void Activate() => isActive = true;
    public void Deactivate() => isActive = false;
    public override void hit(BaseFighter fighter, HitType type)
    {
        if (!isActive) return;
        fighter.take_arena_damage(damage);
    }
}
