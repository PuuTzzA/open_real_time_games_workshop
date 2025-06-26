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
    [HideInInspector]
    public bool isLaserActive = false;
    [HideInInspector]
    public bool isOnCooldown = false;


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

    public void TriggerLaser()
    {
        if (!isPressed)
        {
            StartCoroutine(ActivateLaser());
        }
    }

    private IEnumerator ActivateLaser()
    {
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
