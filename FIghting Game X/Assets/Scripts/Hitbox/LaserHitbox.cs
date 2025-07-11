using Unity.VisualScripting;
using UnityEngine;

public class LaserHitbox : CooldownHitbox
{
    [SerializeField] int damage;

    private bool isActive = false;

    public void Activate() => isActive = true;
    public void Deactivate() => isActive = false;

    public LaserHitbox() {
        cooldown = 7;
    }

    public override void hit(BaseFighter fighter, HitType type)
    {
        if (!isActive) return;

        if(can_be_hit(fighter.id))
        {
            fighter.stun(3, true);
            put_on_cooldown(fighter.id);
            fighter.take_damage(damage, null);
        }
    }
}
