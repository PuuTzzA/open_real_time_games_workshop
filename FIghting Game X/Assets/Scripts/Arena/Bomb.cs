using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Bomb : MonoBehaviour
{
    [SerializeField] private TextMeshPro activatableText;

    private Rigidbody2D rb;
    private Collider2D col;

    public float throwForce = 10f;
    public float throwAngle = 45f;

    public float explodeAfter = 3f;
    public GameObject explosionIdle;
    public float explosionRadius = 2f;
    public float knockbackForce = 15f;
    public float explosionGrowthFactor = 1.2f;
    private bool exploding = false;

    private Animator animator;
    private Color originalColor;

    public BaseFighter holder;
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

    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        audioSource = GetComponent<AudioSource>();

        animator = GetComponent<Animator>();

        originalLocalScale = transform.localScale;
        activatableText.enabled = false;
    }

    private IEnumerator setHoldingBomb()
    {
        yield return new WaitForSeconds(0.5f);
        if (holder.holdingBomb != null)
        {
            holder.holdingBomb = null;
            //holder = null;
        }
    }

    void Update()
    {
        if (pickedUp)
        {
            TriggerDisableInteractionText();
        }

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
                // Note: position will be set in LateUpdate for smoother visuals
            }
        }

        if (exploding)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / explodeAfter);

            float scaleMult = Mathf.Lerp(1f, explosionGrowthFactor, t);
            transform.localScale = originalLocalScale * scaleMult;

            if (timer >= explodeAfter)
            {
                Explode();
            }
        }
    }

    void LateUpdate()
    {
        // If held by a player and not thrown, follow holder with offset
        if (pickedUp && holder != null && !thrown)
        {
            int f = holder.state.get_facing_int();
            transform.position = holder.transform.position + new Vector3(holdOffset.x * f, holdOffset.y, 0f);
        }

        // Cancel out parent scale changes to keep constant screen size
        if (transform.parent != null)
        {
            Vector3 parentScale = transform.parent.lossyScale;
            transform.localScale = new Vector3(
                originalLocalScale.x / parentScale.x,
                originalLocalScale.y / parentScale.y,
                originalLocalScale.z / parentScale.z
            );
        }
        else
        {
            // If no parent, just keep original scale
            transform.localScale = originalLocalScale;
        }
    }

    public void Pickup(BaseFighter fighter)
    {
        if (holder != null) return;

        fighter.holdingBomb = this.gameObject;
        holder = fighter;

        ReparentKeepWorldScale(fighter.transform);

        pickedUp = true;
        wasInteractHeld = true;
    }

    public bool HasHolder()
    {
        return holder != null;
    }

    private void Throw()
    {
        var inputComponent = holder.GetComponent<PlayerInput>();
        Vector2 aimVector = inputComponent.actions["direction"].ReadValue<Vector2>();

        Vector2 baseDir;

        if (aimVector.sqrMagnitude > 0.01f)
        {
            aimVector.Normalize();
            float angle = Mathf.Atan2(aimVector.y, aimVector.x) * Mathf.Rad2Deg;
            angle = Mathf.Round(angle / 45f) * 45f;
            throwAngle = angle;

            float rad = throwAngle * Mathf.Deg2Rad;
            baseDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
        else
        {
            int f = holder.state.get_facing_int(); // 1 = right, -1 = left
            baseDir = new Vector2(f, 0f);
        }

        // ✅ Apply a consistent upward tilt (relative to direction)
        int facing = holder.state.get_facing_int(); // 1 = right, -1 = left
        float angleOffset = facing; // Positive tilt if facing right, negative if left
        if (baseDir.y > 0.5f) // upward or diagonally up
            angleOffset *= -5f;
        else
            angleOffset *= 5f;

        Vector2 throwDir = RotateVector(baseDir.normalized, angleOffset);
        throwAngle = Mathf.Atan2(throwDir.y, throwDir.x) * Mathf.Rad2Deg;

        thrown = true;
        throwTime = Time.time;

        ReparentKeepWorldScale(null);

        // Check stick-to-target
        Vector2 checkPos = (Vector2)holder.transform.position + throwDir * 0.5f;
        float checkRadius = 0.4f;
        int playerLayerMask = 1 << 7;

        Collider2D[] playersInFront = Physics2D.OverlapCircleAll(checkPos, checkRadius, playerLayerMask);
        foreach (var colInFront in playersInFront)
        {
            BaseFighter player = colInFront.GetComponentInParent<BaseFighter>();
            if (player != null && player != holder)
            {
                StickToTarget(player.transform, true);
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
                return;
            }
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0.2f;

        initialVelocity = throwDir * throwForce;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(initialVelocity, ForceMode2D.Impulse);

        pickedUp = false;
        StartCoroutine(setHoldingBomb());
    }

    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    private void StickToTarget(Transform target, bool isPlayer)
    {
        animator.SetTrigger("cd");

        if (!hasStuck)
        {
            audioSource.Play();
        }

        if (hasStuck)
        {
            if (!stuckToPlayer && isPlayer)
            {
                stuckToPlayer = true;
                stuckTarget = target;
                ReparentKeepWorldScale(target);

                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            return;
        }

        hasStuck = true;
        stuckToPlayer = isPlayer;
        stuckTarget = target;

        ReparentKeepWorldScale(target);

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        exploding = true;
        timer = 0f;
    }

    private void Explode()
    {
        animator.SetTrigger("ex");
        ReparentKeepWorldScale(null);

        //foreach (var fighter in FindObjectsOfType<BaseFighter>())
        //{
        //    var fighterCollider = fighter.GetComponent<Collider2D>();
        //    if (fighterCollider != null)
        //    {
        //        Physics2D.IgnoreCollision(col, fighterCollider, false);
        //    }
        //}

        //int layerMask = 1 << 7;
        //Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, layerMask);
        //foreach (var c in hits)
        //{
        //    var bf = c.GetComponentInParent<BaseFighter>();
        //    if (bf != null)
        //    {
        //        bf.take_arena_damage(50);
        //        Vector2 knockDir = (bf.transform.position - transform.position).normalized;
        //        bf.knockback_heavy(knockDir * knockbackForce, 6);
        //    }
        //}

        Destroy(gameObject, 1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseFighter player = other.GetComponentInParent<BaseFighter>();

        bool isPlayer = (player != null);

        if (isPlayer && !thrown)
        {
            TriggerInteractionText(player);
        }

        if (!thrown) return;

        if (other.GetComponent<Bomb>() || other.GetComponent<ExplosionHitbox>() || other.GetComponent<LaserHitbox>())
        {
            return;
        }

        if (isPlayer && player == holder && Time.time - throwTime < 0.2f)
            return;

        if (isPlayer)
        {
            StickToTarget(player.transform, true);
        }
        else
        {
            if (!hasStuck)
            {
                StickToTarget(other.transform, false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BaseFighter player = other.GetComponentInParent<BaseFighter>();
        if (player != null)
        {
            TriggerDisableInteractionText();
        }
    }

    private void ReparentKeepWorldScale(Transform newParent)
    {
        Vector3 worldPosition = transform.position;
        Quaternion worldRotation = transform.rotation;
        Vector3 worldScale = transform.lossyScale;

        transform.SetParent(newParent, true);

        Vector3 parentScale = newParent != null ? newParent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(
            worldScale.x / parentScale.x,
            worldScale.y / parentScale.y,
            worldScale.z / parentScale.z
        );

        transform.position = worldPosition;
        transform.rotation = worldRotation;
    }

    public void TriggerInteractionText(BaseFighter fighter)
    {
        PlayerInput pI = fighter.gameObject.GetComponent<PlayerInput>();
        bool isKeyboard = pI.currentControlScheme == "Keyboard&Mouse";
        activatableText.text = isKeyboard ? pI.actions["Interact"].GetBindingDisplayString().ToLower() : pI.actions["Interact"].GetBindingDisplayString(group: "Gamepad").ToLower();
        activatableText.enabled = true;
    }

    public void TriggerDisableInteractionText()
    {
        activatableText.enabled = false;
    }

}
