using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class LaserButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer leverNormal;
    [SerializeField] private SpriteRenderer leverSwitched;
    [SerializeField] private SpriteRenderer leverActivatable;
    [SerializeField] private TextMeshPro activatableText;
    [SerializeField] private GameObject laser;
    [SerializeField] public float activeTime;
    [SerializeField] float cooldownTime;
    private bool isPressed = false;
    [HideInInspector]
    public bool isLaserActive = false;
    [HideInInspector]
    public bool isOnCooldown = false;


    private void PressButton()
    {
        isPressed = true;
        leverNormal.enabled = false;
        leverActivatable.enabled = false;
        activatableText.enabled = false;
        leverSwitched.enabled = true;
    }


    public void TriggerActivatable(BaseFighter fighter)
    {
        leverNormal.enabled = false;
        leverActivatable.enabled = true;
        PlayerInput pI = fighter.gameObject.GetComponent<PlayerInput>();
        bool isKeyboard = pI.currentControlScheme == "Keyboard&Mouse";
        activatableText.text = isKeyboard ? pI.actions["Interact"].GetBindingDisplayString() : pI.actions["Interact"].GetBindingDisplayString(group: "Gamepad");
        activatableText.enabled = true;

    }

    public void TriggerDisableActivatable()
    {
        if (!isOnCooldown && !isPressed)
        {
            leverNormal.enabled = true;
            leverActivatable.enabled = false;
            activatableText.enabled = false;
        }
    }

    private void ButtonReady()
    {
        isPressed = false;
        leverNormal.enabled = true;
        leverSwitched.enabled = false;
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
