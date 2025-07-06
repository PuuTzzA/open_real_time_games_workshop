using System.Collections;
using UnityEngine;

public class BombPlatformMove : MonoBehaviour
{
    [SerializeField] private SpriteRenderer leverNormal;
    [SerializeField] private SpriteRenderer leverSwitched;
    [SerializeField] private SpriteRenderer leverActivatable;
    [SerializeField] private float waitTimeOffScreen;
    [SerializeField] private float cooldownTime;
    [SerializeField] private float dropDistance;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Collider2D platformCollider;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform bombSpawnPoint;
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
    }

    private void SpawnBomb()
    {
        Vector3 bombPos = bombSpawnPoint.position;
        GameObject bomb = Instantiate(bombPrefab, bombPos, Quaternion.identity);
        bomb.transform.SetParent(this.transform);
    }

    private void PressButton()
    {
        isPressed = true;
        leverNormal.enabled = false;
        leverActivatable.enabled = false;
        leverSwitched.enabled = true;
    }
    private void ButtonReady()
    {
        isPressed = false;
        leverNormal.enabled = true;
        leverSwitched.enabled = false;
    }

    public void TriggerActivatable()
    {
        leverNormal.enabled = false;
        leverActivatable.enabled = true;
    }

    public void TriggerDisableActivatable()
    {
        if (!isMoving && !isPressed)
        {
            leverNormal.enabled = true;
            leverActivatable.enabled = false;
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

        // Move down
        while (Vector3.Distance(transform.position, dropPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, dropPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = dropPosition;
        platformCollider.enabled = false;

        yield return new WaitForSeconds(waitTimeOffScreen);

        platformCollider.enabled = true;
        SpawnBomb();

        while (Vector3.Distance(transform.position, originalPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = originalPosition;
        isMoving = false;

        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
        ButtonReady();
    }
}
