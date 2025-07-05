using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class KeyBindsUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInput playerInput; // Im Inspector setzen

    private VisualElement _root;
    private string controlScheme;

    // Liste der unterstützten Aktionen
    private readonly string[] actionNames = { "jab", "ult", "heavy", "block", "jump", "interact", "dash" };

    // Button-Referenzen zwischenspeichern
    private Dictionary<string, Button> actionButtons = new Dictionary<string, Button>();

    void OnEnable()
    {
        controlScheme = GetControlScheme();
        _root = GetComponent<UIDocument>().rootVisualElement;

        LoadBindings(); // optional

        var playerMap = playerInput.actions.FindActionMap("Player");

        foreach (string actionName in actionNames)
        {
            var action = playerMap.FindAction(actionName);
            var button = _root.Q<Button>(actionName + "button");
            var resetbutton = _root.Q<Button>(actionName + "reset");

            if (action == null)
            {
                continue;
            }
            if (button == null)
            {
                continue;
            }
            if (resetbutton == null)
            {
                continue;
            }

            actionButtons[actionName] = button;

            button.text = GetBindingDisplayString(action);
            button.clicked += () => StartRebind(action, button);
            resetbutton.clicked += () => ResetSingleBinding(action, button);
        }

        // Back-Button
        var backButton = _root.Q<Button>("back");
        if (backButton != null)
        {
            backButton.clicked += () => gameObject.SetActive(false);
        }
        var resetAllButton = _root.Q<Button>("resetall");
        if (resetAllButton != null)
        {
            resetAllButton.clicked += () => ResetToDefaults();
        }
    }

    string GetControlScheme()
    {
        return Gamepad.all.Count > 0 ? "Gamepad" : "Keyboard&Mouse";
    }

    string GetBindingDisplayString(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            if (binding.isComposite || binding.isPartOfComposite)
                continue;

            string path = binding.effectivePath;

            if (controlScheme == "Keyboard&Mouse" && (path.Contains("Keyboard") || path.Contains("Mouse")))
                return action.GetBindingDisplayString(i);

            if (controlScheme == "Gamepad" && path.Contains("Gamepad"))
                return action.GetBindingDisplayString(i);
        }

        return "Unbound";
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
                playerInput.actions.Enable();

                button.text = GetBindingDisplayString(action);
                SaveBindings();
            });

        rebindOperation.Start();
    }

    void SaveBindings()
    {
        string json = playerInput.actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("inputBindings", json);
        PlayerPrefs.Save();
    }

    void LoadBindings()
    {
        if (PlayerPrefs.HasKey("inputBindings"))
        {
            string json = PlayerPrefs.GetString("inputBindings");
            playerInput.actions.LoadBindingOverridesFromJson(json);
        }
    }

    void ResetToDefaults()
    {
        playerInput.actions.RemoveAllBindingOverrides(); // Entfernt alle Änderungen
        SaveBindings(); // Speichert leere Overrides (also Default)

        // Alle Button-Texte aktualisieren
        var playerMap = playerInput.actions.FindActionMap("Player");
        foreach (string actionName in actionNames)
        {
            if (actionButtons.TryGetValue(actionName, out var button))
            {
                var action = playerMap.FindAction(actionName);
                if (action != null)
                {
                    button.text = GetBindingDisplayString(action);
                }
            }
        }

    }
    void ResetSingleBinding(InputAction action, Button button)
    {
        // Alle nicht-kompositen Bindings zurücksetzen
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (!action.bindings[i].isComposite && !action.bindings[i].isPartOfComposite)
            {
                action.RemoveBindingOverride(i);
            }
        }

        SaveBindings(); // Optional: Änderungen speichern
        button.text = GetBindingDisplayString(action); // UI aktualisieren
    }


}
