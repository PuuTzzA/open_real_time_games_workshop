using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ControlsUI : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    private readonly string[] actionNames = { "Jab", "Heavy", "Ult", "Block", "Interact", "Jump", "Dash" };
    private Dictionary<string, Button> actionKeyBingings= new Dictionary<string, Button>();
    [SerializeField] UIDocument menuDocument;
    private VisualElement root;
    private VisualElement focusAnchor;
    void Awake()
    {
        root = menuDocument.rootVisualElement;
        var actions = inputActions.FindActionMap("Player");
        focusAnchor = root.Q<VisualElement>(className: "action_option");
        focusAnchor.RegisterCallback<FocusEvent>(OnAnchorFocused);
        foreach (var actionName in actionNames)
        {
            var action = actions.FindAction(actionName);
            var button = root.Q<Button>(actionName + "_Keybinding");
            var changeButton = root.Q<Button>(actionName + "_Change_Button");

            if (action == null)
            {
                Debug.LogWarning($"Action missing for: {actionName}");
                continue;
            }
            if (button == null)
            {
                Debug.LogWarning($"button missing for: {actionName}");
                continue;
            }
            if (changeButton == null)
            {
                Debug.LogWarning($"Change Button is missing for: {actionName}");
                continue;
            }

            actionKeyBingings[actionName] = button;

            button.text = GetBindingDisplayString(action);
            
            changeButton.clicked += () => StartRebind(action, button);
        }
    }

    private void OnAnchorFocused(FocusEvent evt)
    {
        root.Q<VisualElement>(className: "action_change_button").Focus();
    }

    string GetBindingDisplayString(InputAction action)
    {
        foreach (InputBinding keyBinding in action.bindings)
        {
            if (keyBinding.isComposite || keyBinding.isPartOfComposite)
                continue;
            if (Gamepad.all.Count == 0 && (keyBinding.effectivePath.Contains("Keyboard") ||
                                           keyBinding.effectivePath.Contains("Mouse")) ||
                Gamepad.all.Count > 0 && keyBinding.effectivePath.Contains("Gamepad"))
                return action.GetBindingDisplayString();
        }
        return "None";
    }
    void StartRebind(InputAction action, Button button)
    {
        button.text = "Press a key...";
        action.Disable();

        var rebindOperation = action.PerformInteractiveRebinding()
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                operation.Dispose();
                action.Enable();
                inputActions.Enable();

                button.text = GetBindingDisplayString(action);
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
        inputActions.RemoveAllBindingOverrides(); // Entfernt alle Änderungen
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
                }
            }
        }

        Debug.Log("All controls have been reset to default.");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
