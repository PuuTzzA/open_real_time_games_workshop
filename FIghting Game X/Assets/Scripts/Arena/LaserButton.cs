using System.Collections;
using UnityEngine;

public class LaserButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer buttonNormal;
    [SerializeField] private SpriteRenderer buttonPressed;
    [SerializeField] private GameObject laser;
    [SerializeField] public float activeTime;
    [SerializeField] float cooldownTime;
    private bool isPressed = false;
    private bool isLaserActive = false;
    private bool isOnCooldown = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fighter") && !isPressed && !isLaserActive && !isOnCooldown) {
            Debug.Log("Button pressed");
            StartCoroutine(ActivateLaser());
        }
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

    private IEnumerator ActivateLaser() {
        PressButton();
        isLaserActive = true;
        laser.SetActive(true);
        laser.GetComponent<LaserHitbox>()?.Activate();

        yield return new WaitForSeconds(activeTime);

        laser.SetActive(false);
        laser.GetComponent<LaserHitbox>()?.Deactivate();
        isLaserActive = false;

        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
        ButtonReady();
    }
}
