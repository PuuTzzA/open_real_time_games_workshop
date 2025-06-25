using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private LayerMask fighterLayer;
    [SerializeField] float damagePerSecond;
    [SerializeField] float range;

    private bool isActive = false;

    public void Activate() => isActive = true;
    public void Deactivate() => isActive = false;

    void Update()
    {
        if (!isActive) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, range, fighterLayer);

        if (hit.collider != null && hit.collider.CompareTag("Fighter")) // deal damage to player
        {
            Debug.Log("Damage to fighter!");
            // PlayerHealth2D player = hit.collider.GetComponent<PlayerHealth2D>();
           // if (player != null)
            //{
             //   player.TakeDamage(damagePerSecond * Time.deltaTime);
            //}
        }
    }

    void OnDrawGizmosSelected()
    {
        // Helpful for seeing the laser direction in editor
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * range);
    }
}
