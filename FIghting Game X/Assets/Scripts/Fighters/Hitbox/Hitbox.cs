using UnityEngine;

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
                    hit(fighter);
                    break;
            }
        }
    }

    public virtual void hit(BaseFighter fighter)
    {

    }
}