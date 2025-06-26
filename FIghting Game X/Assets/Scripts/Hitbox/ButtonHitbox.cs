using UnityEngine;

public class ButtonHitbox : Hitbox
{
    public BombPlatformMove platform;
    public override void hit(BaseFighter fighter, HitType type)
    {
        if (fighter.fighter_input.interact == true && !platform.isPressed && !platform.isOnCooldown)
        {
            platform.TriggerDrop();
        }
    }
}
