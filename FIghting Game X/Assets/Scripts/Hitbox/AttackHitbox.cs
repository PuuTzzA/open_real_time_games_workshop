using UnityEngine;

public class AttackHitbox : Hitbox
{
    public int damage;
    public Vector2 knockback;
    public Vector2Int direction;
    public BaseFighter source_fighter;

    public override void hit(BaseFighter fighter, HitType type)
    {
        if (type != HitType.Start) { return; }
        if (fighter.is_blocking(direction * source_fighter.state.get_facing_ivec())) { return; }

        int forced_facing = -source_fighter.state.get_facing_int();
        fighter.state.force_facing(forced_facing);
        fighter.knockback(knockback * source_fighter.state.get_facing_vec());
        fighter.take_damage(damage, fighter.gameObject);

        if (direction.Equals(Vector2Int.down))
        {
            source_fighter.rigidbody.linearVelocityY = 7.0f;
        }
    }
}