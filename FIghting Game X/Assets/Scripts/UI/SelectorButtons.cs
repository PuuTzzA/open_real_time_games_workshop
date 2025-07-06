using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SelectorButtons : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Sprite normalButtonLook;
    [SerializeField] private Sprite focusButtonLook;
    private string backButtonText;
    private string readyButtonText;
    private string backKey;
    private string readyKey;
    private bool usingGamepad;
    private PlayerInput playerInput;
    private InputActionAsset inputActions;

    private void OnDeviceChange(PlayerInput input)
    {
        usingGamepad = input.currentControlScheme == "Gamepad";
        backButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{GetBindingDisplayString(inputActions.FindAction("Cancel")).ToUpper()} {backButtonText}";
        
        readyButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{GetBindingDisplayString(inputActions.FindAction("Confirm")).ToUpper()} {readyButtonText}" ;
    }
    void OnEnable()
    {
        backButton.image.sprite = normalButtonLook;
        readyButton.image.sprite = normalButtonLook;
        backButtonText = backButton.GetComponentInChildren<TextMeshProUGUI>().text;
        readyButtonText = readyButton.GetComponentInChildren<TextMeshProUGUI>().text;
        playerInput = GetComponent<PlayerInput>();
        inputActions = playerInput.actions;
        playerInput.onControlsChanged += OnDeviceChange;
        usingGamepad = playerInput.currentControlScheme == "Gamepad";
        backButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{GetBindingDisplayString(inputActions.FindAction("Cancel")).ToUpper()} {backButtonText}";
        readyButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{GetBindingDisplayString(inputActions.FindAction("Confirm")).ToUpper()} {readyButtonText}" ;
    }

    public void OnConfirm()
    {
       // readyButton.image.sprite = focusButtonLook;
        readyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ready";
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void OnCancel()
    {
        readyButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{GetBindingDisplayString(inputActions.FindAction("Confirm"))} Confirm";
        readyButton.image.sprite = normalButtonLook;
    }
    
    private string GetBindingDisplayString(InputAction action)
    {

        if (usingGamepad && action.bindings.Count(keyBinding => keyBinding.groups.Contains("Gamepad")) > 0)
        {
            return action.GetBindingDisplayString(group:"Gamepad");
        }
        if (!usingGamepad && action.bindings.Count(keyBinding => keyBinding.groups.Contains("Keyboard&Mouse")) > 0)
        {
            return action.GetBindingDisplayString(group:"Keyboard&Mouse");
        }
        return "None";
    }

    private void OnDestroy()
    {
        playerInput.onControlsChanged -= OnDeviceChange;
    }

}
