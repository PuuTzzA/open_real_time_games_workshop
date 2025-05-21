using UnityEngine;
using UnityEngine.InputSystem;

public class BaseFighter : MonoBehaviour
{
    public FighterActions fighter_actions;
    public Rigidbody2D rigidbody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fighter_actions.init();

        rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        var dir = fighter_actions.direction.ReadValue<Vector2>();

        rigidbody.linearVelocityX = dir.x * 4.0f;

        if(fighter_actions.jump.WasPressedThisFrame())
        {
            rigidbody.linearVelocityY = 10.0f;
        }
    }
}
