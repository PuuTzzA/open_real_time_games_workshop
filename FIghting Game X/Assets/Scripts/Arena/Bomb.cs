using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D col;

    [Header("Throw Settings")]
    public float throwForce = 10f;
    public float throwAngle = 45f;

    [Header("Explosion Settings")]
    public float explodeAfter = 3f;
    public GameObject explosionIdle;
    public GameObject explosionEffect;
    public float explosionRadius = 2f;
    public float knockbackForce = 15f;
    public float explosionGrowthFactor = 1.2f;
    private bool exploding = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private BaseFighter holder;
    private bool pickedUp = false;
    private bool wasInteractHeld = false;
    private Vector3 originalLocalScale;
    [SerializeField] private Vector3 holdOffset = new Vector3(0.5f, 0f, 0f);

    public bool thrown = false;
    private float throwTime = -999f;
    private float timer = 0;
    private Vector2 initialVelocity;

    private bool hasStuck = false;
    private Transform stuckTarget = null;
    private bool stuckToPlayer = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        col.isTrigger = true;  // ALWAYS trigger collider

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        originalLocalScale = transform.localScale;
    }

    private IEnumerator setHoldingBomb()
    {
        yield return new WaitForSeconds(0.5f);
        if (holder != null)
        {
            holder.holdingBomb = null;
            holder = null;
        }
    }

    void Update()
    {
        if (pickedUp && holder != null && !thrown)
        {
            bool isHeld = holder.fighter_input.interact;
            if (!wasInteractHeld && isHeld)
            {
                Throw();
            }
            else
            {
                wasInteractHeld = isHeld;

                int f = holder.state.get_facing_int();
                transform.localPosition = new Vector3(holdOffset.x * f, holdOffset.y, 0f);
                transform.localScale = new Vector3(originalLocalScale.x * f,
                                                  originalLocalScale.y,
                                                  originalLocalScale.z);
            }
        }

        if (exploding)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / explodeAfter);

            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(originalColor, Color.red, t);

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
        throwTime = Time.time;
        transform.SetParent(null);

        // Check for players overlapping just in front of thrower to stick immediately
        int f = holder.state.get_facing_int();
        Vector2 checkPos = (Vector2)holder.transform.position + new Vector2(f * 0.5f, 0f); // 0.5 units in front
        float checkRadius = 0.4f;

        // LayerMask for players, adjust as needed
        int playerLayerMask = 1 << 7; // Assuming players are on layer 7

        Collider2D[] playersInFront = Physics2D.OverlapCircleAll(checkPos, checkRadius, playerLayerMask);

        foreach (var colInFront in playersInFront)
        {
            BaseFighter player = colInFront.GetComponentInParent<BaseFighter>();
            if (player != null && player != holder)
            {
                // Immediately stick to this player instead of throwing
                StickToTarget(player.transform, true);

                // Reset Rigidbody to kinematic, no throw force needed
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0f;
                return; // Exit Throw early
            }
        }

        // If no players blocking, proceed with normal throw

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;

        float rad = throwAngle * Mathf.Deg2Rad;
        initialVelocity = new Vector2(Mathf.Cos(rad) * throwForce * f,
                                      Mathf.Sin(rad) * throwForce);
        rb.velocity = Vector2.zero;
        rb.AddForce(initialVelocity, ForceMode2D.Impulse);

        pickedUp = false;
        StartCoroutine(setHoldingBomb());
    }


    private void StickToTarget(Transform target, bool isPlayer)
    {
        if (hasStuck)
        {
            // If already stuck to terrain and now a player passes through, switch to player and never switch again
            if (!stuckToPlayer && isPlayer)
            {
                stuckToPlayer = true;
                stuckTarget = target;
                transform.SetParent(target);
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            return;
        }

        hasStuck = true;
        stuckToPlayer = isPlayer;
        stuckTarget = target;

        transform.SetParent(target);
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        exploding = true;
        timer = 0f;
    }

    private void Explode()
    {
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

        int layerMask = 1 << 7;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, layerMask);
        foreach (var c in hits)
        {
            var bf = c.GetComponentInParent<BaseFighter>();
            if (bf != null)
            {
                bf.take_damage(50, gameObject);
                Vector2 knockDir = (bf.transform.position - transform.position).normalized;
                bf.knockback(knockDir * knockbackForce);
            }
        }

        Destroy(gameObject, 0.4f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!thrown) return;

        BaseFighter player = other.GetComponentInParent<BaseFighter>();
        bool isPlayer = (player != null);

        // Prevent sticking to thrower for first 0.2 seconds after throw
        if (isPlayer && player == holder && Time.time - throwTime < 0.2f)
            return;

        if (isPlayer)
        {
            // Stick to player if never stuck or currently stuck to terrain
            StickToTarget(player.transform, true);
        }
        else
        {
            // Stick to terrain or other non-player only if not already stuck
            if (!hasStuck)
            {
                StickToTarget(other.transform, false);
            }
        }
    }
}
