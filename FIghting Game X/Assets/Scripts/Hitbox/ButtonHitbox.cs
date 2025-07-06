using UnityEngine;

public class ButtonHitbox : Hitbox
{
    public BombPlatformMove platform;

    private bool lastFrame = false;
    private bool thisFrame = false;


    public override void hit(BaseFighter fighter, HitType type)
    {
        if (fighter.fighter_input.interact == true && !platform.isPressed && !platform.isOnCooldown && !fighter.holdingBomb)
        {
            platform.TriggerDrop();
        }
        else if (!platform.isPressed && !platform.isOnCooldown && !fighter.holdingBomb)
        {
            thisFrame = true;
        }
    }

    private void FixedUpdate()
    {
        if (thisFrame && !lastFrame)
        {
            platform.TriggerActivatable();
        }
        if (!thisFrame && lastFrame)
        {
            platform.TriggerDisableActivatable();
        }

        lastFrame = thisFrame;
        thisFrame = false;
    }
}
