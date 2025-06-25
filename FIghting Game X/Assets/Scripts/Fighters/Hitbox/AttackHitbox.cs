using UnityEngine;

public class AttackHitbox : Hitbox
{
    public float damage;
    public Vector2 knockback;
    public Vector2Int direction;
    public BaseFighter source_fighter;

    public override void hit(BaseFighter fighter)
    {
        int forced_facing = -direction.x * source_fighter.state.get_facing_int();
        Debug.Log(forced_facing);
        fighter.state.force_facing(forced_facing);
        fighter.knockback(knockback * source_fighter.state.get_facing_vec());
    }
}