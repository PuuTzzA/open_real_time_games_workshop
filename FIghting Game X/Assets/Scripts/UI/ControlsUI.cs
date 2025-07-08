using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class ControlsUI : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private UIDocument menuDocument;

    private readonly string[] actionNames = { "jab", "heavy", "ult", "block", "interact", "jump", "dash" };
    private VisualElement root;
    private InputActionMap actions;

    private VisualElement controlsTabHeader;
    private VisualElement focusEntrance;
    private VisualElement focusExit;

    private void Awake()
    {
        root = menuDocument.rootVisualElement;
        actions = playerInput.actions.FindActionMap("Player");

        controlsTabHeader = root.Q<VisualElement>("Controls_Tab_Header");
        focusEntrance = root.Q<VisualElement>(className: "focus_entrance");
        focusEntrance.RegisterCallback<FocusEvent>(OnEntranceFocused);
        focusExit = root.Q<VisualElement>(className: "focus_exit");
        focusExit.RegisterCallback<FocusEvent>(OnExitFocused);

        // Register pointer and navigation event filtering
        root.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        root.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit, TrickleDown.TrickleDown);
        root.RegisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);

        // Setup UI buttons and bindings
        foreach (var actionName in actionNames)
        {
            var button = root.Q<Button>(actionName + "_Keybinding");
            var changeButton = root.Q<Button>(actionName + "_Change_Button");
            var action = actions.FindAction(actionName);

            if (button == null || changeButton == null || action == null)
                continue;

            button.text = GetBindingDisplayString(action);

            changeButton.clicked += () =>
            {
                if (PlayerOwnsInput())
                    StartRebind(action);
            };
        }

        root.Q<Button>("Restore_Defaults_Button").clicked += () =>
        {
            if (PlayerOwnsInput())
                ResetToDefaults();
        };
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        // Get device from pointer ID or pointer type
        InputDevice device = null;

        if (evt.pointerId == PointerId.mousePointerId)
            device = Mouse.current;
        else if (evt.pointerId >= PointerId.touchPointerIdBase && evt.pointerId < PointerId.touchPointerIdBase + 10)
            device = Touchscreen.current;
        else if (evt.pointerType == UnityEngine.UIElements.PointerType.pen)
            device = Pen.current;

        if (device == null || !IsFromThisPlayer(device))
        {
            evt.StopImmediatePropagation();
        }
    }

    private void OnNavigationSubmit(NavigationSubmitEvent evt)
    {
        if (!PlayerOwnsInput())
            evt.StopImmediatePropagation();
    }

    private void OnNavigationMove(NavigationMoveEvent evt)
    {
        if (!PlayerOwnsInput())
            evt.StopImmediatePropagation();
    }

    private bool IsFromThisPlayer(InputDevice device)
    {
        return playerInput.user.valid && playerInput.user.pairedDevices.Contains(device);
    }

    private bool PlayerOwnsInput()
    {
        var focused = root.focusController?.focusedElement as VisualElement;
        return focused != null && root.Contains(focused);
    }

    private string GetBindingDisplayString(InputAction action)
    {
        var usingGamepad = playerInput.currentControlScheme == "Gamepad";
        string group = usingGamepad ? "Gamepad" : "Keyboard&Mouse";
        if (action.bindings.Any(b => b.groups.Contains(group)))
            return action.GetBindingDisplayString(group: group);
        return "None";
    }

    private void StartRebind(InputAction action)
    {
        var keyBinding = root.Q<Button>($"{action.name}_Keybinding");
        var changeButton = root.Q<Button>($"{action.name}_Change_Button");

        keyBinding.text = "...";
        var originalText = changeButton.text;
        changeButton.text = "Press a key...";

        action.Disable();

        var rebind = action.PerformInteractiveRebinding()
            .WithCancelingThrough("<Keyboard>/escape")
            .WithBindingGroup(playerInput.currentControlScheme)
            .OnComplete(operation =>
            {
                operation.Dispose();
                action.Enable();
                playerInput.actions.Enable();

                keyBinding.text = GetBindingDisplayString(action);
                changeButton.text = originalText;
                SaveBindings();
            });

        if (playerInput.user.valid)
        {
            foreach (var device in InputSystem.devices)
            {
                if (!playerInput.user.pairedDevices.Contains(device))
                    rebind.WithControlsExcluding($"<{device.layout}>");
            }
        }

        rebind.Start();
    }

    private void SaveBindings()
    {
        string json = playerInput.actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString($"inputBindings_{playerInput.playerIndex}", json);
        PlayerPrefs.Save();
    }

    private void ResetToDefaults()
    {
        var usingGamepad = playerInput.currentControlScheme == "Gamepad";

        foreach (var actionName in actionNames)
        {
            var action = actions.FindAction(actionName);
            if (action == null)
                continue;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (binding.isComposite || binding.isPartOfComposite)
                    continue;

                if ((usingGamepad && binding.groups.Contains("Gamepad")) ||
                    (!usingGamepad && binding.groups.Contains("Keyboard&Mouse")))
                {
                    action.RemoveBindingOverride(i);
                }
            }
        }

        SaveBindings();

        foreach (var actionName in actionNames)
        {
            var button = root.Q<Button>(actionName + "_Keybinding");
            var action = actions.FindAction(actionName);
            if (button != null && action != null)
                button.text = GetBindingDisplayString(action);
        }
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
        }
        else root.Q<VisualElement>(className: "action_change_button").Focus();
    }

}
