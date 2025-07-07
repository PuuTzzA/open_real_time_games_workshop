using UnityEngine;

public class MaterialSelector : MonoBehaviour
{
    public PhysicsMaterial2D default_material;
    public PhysicsMaterial2D elastic_material;

    public new Rigidbody2D rigidbody;

    public void set_elastic(bool elastic)
    {
        rigidbody.sharedMaterial = elastic ? elastic_material : default_material;
    }
}