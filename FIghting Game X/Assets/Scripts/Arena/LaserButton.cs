using System.Collections;
using UnityEngine;

public class LaserButton : MonoBehaviour
{
    [SerializeField] private GameObject buttonNormal;
    [SerializeField] private GameObject buttonPressed;
    [SerializeField] private GameObject laser;

    [SerializeField] public float activeTime;
    [SerializeField] float cooldownTime;
    private bool isPressed;
    private bool isLaserActive = false;
    private bool isOnCooldown = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fighter") && !isPressed && !isLaserActive && !isOnCooldown) {
            PressButton();
            StartCoroutine(ActivateLaser());
        }
    }

    private void PressButton() {
        isPressed = true;
        buttonNormal.SetActive(false);
        buttonPressed.SetActive(true);
    }
    private void ButtonReady() {
        isPressed = false;
        buttonNormal.SetActive(true);
        buttonPressed.SetActive(false);
    }

    private IEnumerator ActivateLaser() {
        isLaserActive = true;
        laser.SetActive(true);
        laser.GetComponent<Laser>()?.Activate();

        yield return new WaitForSeconds(activeTime);

        laser.SetActive(false);
        laser.GetComponent<Laser>()?.Deactivate();
        isLaserActive = false;

        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
        ButtonReady();
    }
}
