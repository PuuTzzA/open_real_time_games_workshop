using UnityEngine;

public enum HitType
{
    Start,
    Stay
}

public class Hitbox : MonoBehaviour {

    public void OnTriggerEnter2D(Collider2D collision)
    {
        var obj = collision.gameObject;

        if (obj.layer == 7 /*Hurtbox*/)
        {
            switch(obj.tag)
            {
                case "Fighter":
                    var fighter = obj.transform.parent.gameObject.GetComponent<BaseFighter>();
                    hit(fighter, HitType.Start);
                    break;
            }
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        var obj = collision.gameObject;

        if (obj.layer == 7 /*Hurtbox*/)
        {
            switch (obj.tag)
            {
                case "Fighter":
                    var fighter = obj.transform.parent.gameObject.GetComponent<BaseFighter>();
                    hit(fighter, HitType.Stay);
                    break;
            }
        }
    }

    public virtual void hit(BaseFighter fighter, HitType type)
    {

    }
}