using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D), typeof(SpriteRenderer))]
public class ColliderUpdater : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;
    private Sprite lastSprite;

    private List<Vector2> physicsShape = new List<Vector2>();

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    void LateUpdate()
    {
        if (spriteRenderer.sprite != null && spriteRenderer.sprite != lastSprite)
        {
            lastSprite = spriteRenderer.sprite;

            int shapeCount = spriteRenderer.sprite.GetPhysicsShapeCount();
            polygonCollider.pathCount = shapeCount;

            for (int i = 0; i < shapeCount; i++)
            {
                physicsShape.Clear();
                spriteRenderer.sprite.GetPhysicsShape(i, physicsShape);
                polygonCollider.SetPath(i, physicsShape);
            }
        }
    }
}
