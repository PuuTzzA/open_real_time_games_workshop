using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class ControlsUI : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    private readonly string[] actionNames = { "jab", "heavy", "ult", "block", "interact", "jump", "dash" };
    private Dictionary<string, Button> actionKeyBingings= new Dictionary<string, Button>();
    [SerializeField] UIDocument menuDocument;
    private VisualElement root;
    private VisualElement focusEntrance;
    private VisualElement focusExit;
    private VisualElement controlsTabHeader;
    private bool usingGamepad = false;
    private InputActionMap actions;

    private void Awake()
    {
        LoadBindings();
        root = menuDocument.rootVisualElement;
        controlsTabHeader = root.Q<VisualElement>("Controls_Tab_Header");
        actions = inputActions.FindActionMap("Player");
        focusEntrance = root.Q<VisualElement>(className: "focus_entrance");
        focusEntrance.RegisterCallback<FocusEvent>(OnEntranceFocused);
        focusExit = root.Q<VisualElement>(className: "focus_exit");
        focusExit.RegisterCallback<FocusEvent>(OnExitFocused);
        foreach (var actionName in actionNames)
        {
            
            var button = root.Q<Button>(actionName + "_Keybinding");
            var changeButton = root.Q<Button>(actionName + "_Change_Button");
            var allActions = inputActions.FindActionMap("UI").actions.ToList();
            allActions.AddRange(actions.actions);
            allActions.Remove(inputActions.FindAction("Point"));
            foreach (var a in allActions)
            {
                a.performed += OnInputDeviceChanged;
            }
            var action = actions.FindAction(actionName);
            if (action == null)
            {
                continue;
            }
            if (button == null)
            {
                continue;
            }
            if (changeButton == null)
            {
                continue;
            }

            actionKeyBingings[actionName] = button;

            button.text = GetBindingDisplayString(action);
            
            changeButton.clicked += () => StartRebind(action);
        }

        var restoreDefaultsButton = root.Q<VisualElement>("Restore_Defaults_Button") as Button;
        restoreDefaultsButton.clicked += ResetToDefaults;
    }
    



    private void OnInputDeviceChanged(InputAction.CallbackContext context)
    {

        if (!this.gameObject.activeSelf) return;
        bool isGamepadInput = context.control.device is Gamepad;


        if (isGamepadInput == usingGamepad) return;
        usingGamepad = isGamepadInput;
        foreach (var actionName in actionKeyBingings.Keys)
        {
            actionKeyBingings[actionName].text = GetBindingDisplayString(actions.FindAction(actionName));
        }

        var cancelButton = root.Q<VisualElement>("Cancel_Button_Name") as Button;
        cancelButton.text = GetBindingDisplayString(inputActions.FindAction("Cancel"));
        var confirmButton = root.Q <VisualElement>("Confirm_Button_Name") as Button;
        confirmButton.text = GetBindingDisplayString(inputActions.FindAction("Confirm"));

    }




    private void OnEntranceFocused(FocusEvent evt)
    {
        root.Q<VisualElement>(className: "action_change_button").Focus();
    }

    private void OnExitFocused(FocusEvent evt)
    {
        if (evt.relatedTarget is not VisualElement element)
            return;
        if (evt.relatedTarget.tabIndex == 0)
        {
            controlsTabHeader.Focus();
        } else root.Q<VisualElement>(className: "action_change_button").Focus();
    }

    string GetBindingDisplayString(InputAction action)
    {
        if (usingGamepad && action.bindings.Count(keyBinding => keyBinding.groups.Contains("Gamepad")) > 0)
            return action.GetBindingDisplayString(group:"Gamepad");
        if (!usingGamepad && action.bindings.Count(keyBinding => keyBinding.groups.Contains("Keyboard&Mouse")) > 0)
            return action.GetBindingDisplayString(group:"Keyboard&Mouse");
        return "None";
    }
    void StartRebind(InputAction action)
    {
        Button keyBinding = root.Q<VisualElement>($"{action.name}_Keybinding") as Button;
        keyBinding.text = "...";
        action.Disable();
        Button changeButton = root.Q<VisualElement>($"{action.name}_Change_Button") as Button;
        var changeButtonText = changeButton.text;
        changeButton.text = "Press a key...";
        var rebindOperation = action.PerformInteractiveRebinding()
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f);
        rebindOperation.WithBindingGroup(usingGamepad ? "Gamepad" : "Keyboard&Mouse" );
        rebindOperation.OnComplete(operation =>
            {
                operation.Dispose();
                action.Enable();
                inputActions.Enable();

                keyBinding.text = GetBindingDisplayString(action);
                changeButton.text = changeButtonText;
                SaveBindings();
            });
        
        rebindOperation.Start();
    }
    private void SaveBindings()
    {
        var json = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("inputBindings", json);
        PlayerPrefs.Save();
    }
    void LoadBindings()
    {
        if (!PlayerPrefs.HasKey("inputBindings")) return;
        var json = PlayerPrefs.GetString("inputBindings");
        inputActions.LoadBindingOverridesFromJson(json);
    }
    void ResetToDefaults()
    {

        //inputActions.RemoveAllBindingOverrides(); // Entfernt alle Änderungen
        foreach (var actionName in actionKeyBingings.Keys)
        {
            var action = inputActions.FindAction(actionName);
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (binding.isComposite || binding.isPartOfComposite)
                    continue;
                if (usingGamepad && binding.groups.Contains("Gamepad") ||
                    !usingGamepad && binding.groups.Contains("Keyboard&Mouse"))
                    action.RemoveBindingOverride(i);
            }
        }
        SaveBindings(); // Speichert leere Overrides (also Default)

        // Alle Button-Texte aktualisieren
        var playerMap = inputActions.FindActionMap("Player");
        foreach (string actionName in actionNames)
        {
            if (actionKeyBingings.TryGetValue(actionName, out var button))
            {
                var action = playerMap.FindAction(actionName);
                if (action != null)
                {
                    button.text = GetBindingDisplayString(action);
                    //print(button.text);
                }
            }
        }

    }

    private void CleanTheShit()
    {
        var allActions = inputActions.FindActionMap("UI").actions.ToList();
        allActions.AddRange(actions.actions);
        allActions.Remove(inputActions.FindAction("Point"));
        foreach (var a in allActions)
        {
            a.performed -= OnInputDeviceChanged;
        }
    }

    private void OnDestroy()
    {
        CleanTheShit();
    }
}
