using System.Collections;
using UnityEngine;

public class BombPlatformMove : MonoBehaviour
{
    [SerializeField] private SpriteRenderer buttonNormal;
    [SerializeField] private SpriteRenderer buttonPressed;
    [SerializeField] private float waitTimeOffScreen;
    [SerializeField] private float cooldownTime;
    [SerializeField] private float dropDistance;
    [SerializeField] private float moveSpeed;
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

    private void PressButton() {
        isPressed = true;
        buttonNormal.enabled = false;
        buttonPressed.enabled = true;
    }
    private void ButtonReady() {
        isPressed = false;
        buttonNormal.enabled = true;
        buttonPressed.enabled = false;
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

        yield return new WaitForSeconds(waitTimeOffScreen);

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
