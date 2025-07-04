using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    private Rigidbody2D rb;
    
    [Header("Throw Settings")]
    public float throwForce = 10f;
    public float throwAngle = 45f; 
    
    [Header("Explosion Settings")]
    public float explodeAfter = 3f;
    public GameObject explosionIdle;
    public GameObject explosionEffect; // Prefab for explosion effect
    public float explosionRadius = 2f;
    public float knockbackForce = 15f; // Force applied to fighters hit by the explosion
    public float explosionGrowthFactor = 1.2f; // How much the explosion grows over time
    private bool exploding = false;
    
    
    // Animations
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // Pickup
    private BaseFighter holder;
    private bool pickedUp = false;
    private bool wasInteractHeld = false; // 👈 track previous interact state
    private Vector3 originalLocalScale;
    [SerializeField] private Vector3 holdOffset = new Vector3(0.5f, 0f, 0f);
    
    // Throw
    public bool thrown = false;
    private float timer = 0;
    private float gravity = -30f;
    private Vector2 initialVelocity;
    private Vector3 startPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        
        
        originalLocalScale = transform.localScale;
        Debug.Log($"{gameObject.name}: Bomb spawned and initialized.");
    }

    private IEnumerator setHoldingBomb()
    {
        yield return new WaitForSeconds(0.5f);
        if (holder != null)
        {
            holder.holdingBomb = null;
            holder = null;
            Debug.Log($"{gameObject.name}: Bomb holder cleared after delay.");
        }
    }

    void Update()
    {
        if (pickedUp && holder != null && !thrown)
        {
            // 1) edge‐detect interact press to Throw()
            bool isHeld = holder.fighter_input.interact;
            if (!wasInteractHeld && isHeld)
            {
                Debug.Log($"{gameObject.name}: Throw triggered by player {holder.name}.");
                Throw();
            }
            else
            {
                wasInteractHeld = isHeld;

                // 2) drive position & scale from holder’s facing
                int f = holder.state.get_facing_int();  // +1 or -1
                transform.localPosition = new Vector3(holdOffset.x * f, holdOffset.y, 0f);
                transform.localScale    = new Vector3(originalLocalScale.x * f,
                    originalLocalScale.y,
                    originalLocalScale.z);
            }
        }

        if (exploding)
        {
            timer += Time.deltaTime;
            
            // ⚠️ Calculate progress [0, 1]
            float t = Mathf.Clamp01(timer / explodeAfter);

            // 🔴 Color lerp
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(originalColor, Color.red, t);

            // ⏫ Scale lerp
            float scaleMult = Mathf.Lerp(1f, explosionGrowthFactor, t);
            transform.localScale = originalLocalScale * scaleMult;

            if (timer >= explodeAfter)
            {
                Explode();
            }
        }
    }

    public void Pickup(BaseFighter fighter)
    {
        if (holder != null) return;

        fighter.holdingBomb = this.gameObject;
        holder = fighter;
        transform.SetParent(fighter.transform);
        transform.localPosition = new Vector3(holdOffset.x * fighter.state.get_facing_int(), holdOffset.y, 0f);
        pickedUp = true;
        wasInteractHeld = true;

    }

    public bool HasHolder()
    {
        return holder != null;
    }

    private void Throw()
    {
        thrown = true;
        // detach
        transform.SetParent(null);
        
        // Re-enable physics
        rb.bodyType  = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        
        // compute initial velocity
        int f = holder.state.get_facing_int();
        float rad = throwAngle * Mathf.Deg2Rad;
        initialVelocity = new Vector2(Mathf.Cos(rad) * throwForce * f,
                                      Mathf.Sin(rad) * throwForce);
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(initialVelocity, ForceMode2D.Impulse);
        
        var col = GetComponent<Collider2D>();
        
        foreach (var fighter in FindObjectsByType<BaseFighter>(FindObjectsSortMode.None))
        {
            var fighterCollider = fighter.GetComponent<Collider2D>();
            if (fighterCollider != null)
            {
                Physics2D.IgnoreCollision(col, fighterCollider, true);
            }
        }

        col.isTrigger = false;
        
        pickedUp = false;
        StartCoroutine(setHoldingBomb());
    }


    private void Explode()
    {
        var col = GetComponent<Collider2D>();
        // Re-enable physics for all fighters
        foreach (var fighter in FindObjectsOfType<BaseFighter>())
        {
            var fighterCollider = fighter.GetComponent<Collider2D>();
            if (fighterCollider != null)
            {
                Physics2D.IgnoreCollision(col, fighterCollider, false);
            }
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }
        
        Physics2D.IgnoreLayerCollision(gameObject.layer, 6, false);
        Physics2D.IgnoreLayerCollision(gameObject.layer, 7, false);
        

        // damage via OverlapCircle
        int layerMask = 1 << 7;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, layerMask);
        foreach (var c in hits)
        {
            var bf = c.GetComponentInParent<BaseFighter>();
            if (bf != null)
            {
                Debug.Log($"{name}: Damaging {bf.name}");
                bf.take_damage(50, gameObject);  // or your damage logic
                
                // 💥 Add knockback
                Vector2 knockDir = (bf.transform.position - transform.position).normalized;
                bf.knockback(knockDir * knockbackForce);
            }
        }

        Destroy(gameObject, 0.4f);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y > 0.7f)
            {
                Debug.Log($"{name}: Landed on ground.");
                exploding = true;
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
                rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
            }
        }
    }
}
