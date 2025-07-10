using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BombPlatformMove : MonoBehaviour
{
    [SerializeField] private SpriteRenderer leverNormal;
    [SerializeField] private SpriteRenderer leverSwitched;
    [SerializeField] private SpriteRenderer leverActivatable;
    [SerializeField] private TextMeshPro activatableText;
    [SerializeField] private GameObject propulsion;
    [SerializeField] private ParticleSystem propulsionParticleSystem;
    [SerializeField] private float waitTimeOffScreen;
    [SerializeField] private float cooldownTime;
    [SerializeField] private float dropDistance;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Collider2D platformCollider;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject[] bombSpawnPointList;
    private Vector3 originalPosition;
    private Vector3 dropPosition;

    [HideInInspector]
    public bool isPressed = false;
    [HideInInspector]
    public bool isMoving = false;
    [HideInInspector]
    public bool isOnCooldown = false;

    void Start()
    {
        originalPosition = transform.position;
        dropPosition = originalPosition + Vector3.down * dropDistance;
        propulsion.SetActive(false);
        propulsionParticleSystem.Stop();
    }

    private void SpawnBomb()
    {
        foreach (var bombSpawnPoint in bombSpawnPointList)
        {
            if (bombSpawnPoint.transform.childCount == 0)
            {
                Vector3 bombPos = bombSpawnPoint.transform.position;
                GameObject bomb = Instantiate(bombPrefab, bombPos, Quaternion.identity);
                bomb.transform.SetParent(bombSpawnPoint.transform);
            }
        }
    }

    private void PressButton()
    {
        isPressed = true;
        leverNormal.enabled = false;
        leverActivatable.enabled = false;
        activatableText.enabled = false;
        leverSwitched.enabled = true;
    }
    private void ButtonReady()
    {
        isPressed = false;
        leverNormal.enabled = true;
        leverSwitched.enabled = false;
    }

    public void TriggerActivatable(BaseFighter fighter)
    {
        leverNormal.enabled = false;
        leverActivatable.enabled = true;
        PlayerInput pI = fighter.gameObject.GetComponent<PlayerInput>();
        bool isKeyboard = pI.currentControlScheme == "Keyboard&Mouse";
        activatableText.text = isKeyboard ? pI.actions["Interact"].GetBindingDisplayString().ToLower() : pI.actions["Interact"].GetBindingDisplayString(group: "Gamepad").ToLower();
        activatableText.enabled = true;
    }

    public void TriggerDisableActivatable()
    {
        if (!isMoving && !isPressed)
        {
            leverNormal.enabled = true;
            leverActivatable.enabled = false;
            activatableText.enabled = false;
        }
    }

    public void TriggerDrop()
    {
        if (!isMoving)
            StartCoroutine(MovePlatform());
    }

    private IEnumerator MovePlatform()
    {
        PressButton();
        isMoving = true;
        propulsion.SetActive(true);
        propulsionParticleSystem.Play();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Move down
        while (transform.position.y > dropPosition.y)
        {
            rb.linearVelocity = Vector2.down * moveSpeed * 0.5f;
            // transform.position = Vector3.MoveTowards(transform.position, dropPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = dropPosition;
        platformCollider.enabled = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        yield return new WaitForSeconds(waitTimeOffScreen);

        platformCollider.enabled = true;
        SpawnBomb();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        while (transform.position.y < originalPosition.y)
        {
            rb.linearVelocity = Vector2.up * moveSpeed;
            //transform.position = Vector3.MoveTowards(transform.position, originalPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        propulsion.SetActive(false);
        propulsionParticleSystem.Stop();

        transform.position = originalPosition;
        isMoving = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
        ButtonReady();
    }
}
