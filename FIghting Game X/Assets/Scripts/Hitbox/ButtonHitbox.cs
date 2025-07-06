using UnityEngine;

public class ButtonHitbox : Hitbox
{
    public BombPlatformMove platform;

    private bool lastFrame = false;
    private bool thisFrame = false;

    private BaseFighter _fighter;


    public override void hit(BaseFighter fighter, HitType type)
    {
        if (fighter.fighter_input.interact == true && !platform.isPressed && !platform.isOnCooldown && !fighter.holdingBomb)
        {
            platform.TriggerDrop();
        }
        else if (!platform.isPressed && !platform.isOnCooldown && !fighter.holdingBomb)
        {
            thisFrame = true;
            _fighter = fighter;
        }
    }

    private void FixedUpdate()
    {
        if (thisFrame && !lastFrame)
        {
            platform.TriggerActivatable(_fighter);
        }
        if (!thisFrame && lastFrame)
        {
            platform.TriggerDisableActivatable();
        }

        lastFrame = thisFrame;
        thisFrame = false;
    }
}
