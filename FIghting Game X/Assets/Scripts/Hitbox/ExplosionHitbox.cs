using UnityEngine;

public class ExplosionHitbox : CooldownHitbox
{
    public int damage;
    public float force;


    public ExplosionHitbox()
    {
        cooldown = 10;
    }

    public override void hit(BaseFighter fighter, HitType type)
    {
        if (type != HitType.Start) return;

        if (can_be_hit(fighter.id))
        {
            Vector2 knockDir = (fighter.transform.position - transform.position).normalized;
            fighter.knockback_heavy(knockDir * force, 20);
            put_on_cooldown(fighter.id);
            fighter.take_damage(damage, transform.parent.gameObject.GetComponent<Bomb>().holder.gameObject, true);
        }
    }
}