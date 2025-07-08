using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
// ReSharper disable InconsistentNaming

public class OldMenuUI : MonoBehaviour
{
    [SerializeField] UIDocument menuDocument;
    [SerializeField] SettingsTabUI settingsTabUI;
    private VisualElement root;
    [SerializeField] private bool isMainMenu = true;


    #region menu options

    private List<Button> menuOptions;
    #endregion

    #region screens
    private VisualElement sidePlaceholder;
    private VisualElement exitScreen;
    private VisualElement playScreen;
    private VisualElement infoScreen;
    private VisualElement settingsScreen;
    #endregion

    #region universal elements
    private Button backButton;
    private Button confirmButton;
    [SerializeField] private InputActionAsset inputActions;

    private InputAction confirmAction;
    private InputAction cancelAction;
    private List<Button> currentOptions;
    private VisualElement menuScreen;
    private VisualElement currentSideScreen;
    #endregion

    private Focusable focusedElement;
    private static bool gamePaused;

    private enum MenuLevel
    {
        menu,
        option_chosen,
        option_specific
    }
    private MenuLevel menuLevel = MenuLevel.menu;

    public bool GetPauseStatus()
    {
        return gamePaused;
    }
    void Awake()
    {
        menuLevel = MenuLevel.menu;
        root = menuDocument.rootVisualElement;
        #region screens initilaization

        menuScreen = root.Q<VisualElement>("Screen");
        sidePlaceholder = root.Q<VisualElement>("Default_side_placeholder");
        exitScreen = root.Q<VisualElement>("Exit_option_screen");
        playScreen = root.Q<VisualElement>("Play_option_screen");
        infoScreen = root.Q<VisualElement>("Info_option_screen");
        settingsScreen = root.Q<VisualElement>("Settings_option_screen");
        currentSideScreen = sidePlaceholder;
        #endregion
        #region menu options intialization
        root.Q<VisualElement>("Play_Option");
        root.Q<VisualElement>("Settings_Option");
        root.Q<VisualElement>("Info_Option");
        root.Q<VisualElement>("Exit_Option");
        menuOptions = root.Query<Button>(className: "menu_option_button").ToList();


        currentOptions = menuOptions;
        foreach (Button option in menuOptions)
        {
            option.RegisterCallback<ClickEvent>(OnOptionClicked);
            option.RegisterCallback<FocusEvent>(evt => focusedElement = evt.target as Focusable);
        }
        #endregion
        #region universal elements initialization
        backButton = root.Q<Button>("Cancel_Option");
        confirmButton = root.Q<Button>("Confirm_Option");
        var gameplayActions = inputActions.FindActionMap("UI");
        confirmAction = gameplayActions.FindAction("Confirm");
        cancelAction = gameplayActions.FindAction("Cancel");

        #endregion
        InitializeStyle();
        focusedElement.Focus();
    }

    private void InitializeStyle()
    {

        var playButton = root.Q<VisualElement>("Play_Option");
        var continueButton = root.Q<VisualElement>("Continue_Option");
        var oldButton = isMainMenu ? continueButton : playButton;
        var newButton = isMainMenu ? playButton : continueButton;
        oldButton.SetEnabled(false);
        oldButton.style.display = DisplayStyle.None;
        oldButton.focusable = false;
        newButton.SetEnabled(true);
        newButton.style.display = DisplayStyle.Flex;
        newButton.focusable = true;
        focusedElement = newButton;
        string[] mainMenuStyle =
            { "pause_screen", "pause_header", "pause_body", "pause_option_selector", "pause_option_specific_screen" };
        string[] pauseStyle =
            {"menu_screen", "menu_header", "menu_body", "option_selector", "option_specific_screen"};
        var oldStyles = isMainMenu ? mainMenuStyle : pauseStyle;
        var newStyles = isMainMenu ? pauseStyle : mainMenuStyle;
        for (var i = 0; i < oldStyles.Length; i++)
        {
            var visualElement = root.Q<VisualElement>(className: $"{oldStyles[i]}");
            visualElement.RemoveFromClassList($"{oldStyles[i]}");
            visualElement.AddToClassList($"{newStyles[i]}");
        }

        root.Q<Label>(className: $"{newStyles[1]}").text = isMainMenu ? "FIGHTING GAME X" : "PAUSE MENU";

        if (!isMainMenu)
        {
            inputActions.FindAction("Pause Game").performed += OnGamePaused;
            menuScreen.SetEnabled(false);
            menuScreen.style.display = DisplayStyle.None;
        }
        else
        {
            inputActions.FindAction("Pause Game").performed -= OnGamePaused;
            confirmAction.performed += OnConfirmPressed;
            cancelAction.performed += OnBackPressed;
        }



    }



    private void OnGamePaused(InputAction.CallbackContext context)
    {
        PauseGame();
    }

    private void PauseGame()
    {
        if (!gamePaused)
        {

            gamePaused = true;
            PauseTime();
            menuScreen.SetEnabled(true);
            menuScreen.style.display = DisplayStyle.Flex;
            confirmAction.performed += OnConfirmPressed;
            cancelAction.performed += OnBackPressed;
            focusedElement.Focus();
        }
        else
        {
            while (menuLevel != MenuLevel.menu)
            {
                Back();
            }

            menuScreen.SetEnabled(false);
            menuScreen.style.display = DisplayStyle.None;
            CleanTheShit();
            gamePaused = false;
            focusedElement = root.Q<VisualElement>("Continue_Option");
            UnpauseTime();
        }
    }
    private void PauseTime()
    {
        Time.timeScale = 0;
    }

    private void UnpauseTime()
    {
        Time.timeScale = 1;
    }
    private void OnOptionClicked(ClickEvent evt)
    {
        if (evt.target is not Button option) return;
        ChooseOption(option);
    }

    private void ChooseOption(Button option)
    {
        menuLevel = MenuLevel.option_chosen;

        Button firstOption = null;
        switch (option.name)
        {
            case "Play_Option":
                currentSideScreen = playScreen;
                firstOption = root.Q<Button>(className: "play_option_button");
                break;
            case "Continue_Option":
                if (!gamePaused) return;
                Back();
                PauseGame();
                return;
            case "Settings_Option":
                currentSideScreen = settingsScreen;
                firstOption = settingsTabUI.FirstSetting();
                root.Q<VisualElement>("Footer").focusable = true;
                break;
            case "Info_Option":
                currentSideScreen = infoScreen;
                break;
            case "Exit_Option":
                currentSideScreen = exitScreen;
                if (!isMainMenu)
                {
                    firstOption = root.Q<Button>("Exit_To_Menu_Option");
                    var exitButton1 = root.Q<VisualElement>("Exit_To_Menu_Option");
                    var exitButton2 = root.Q<VisualElement>("Exit_Game_Option");
                    exitButton1.SetEnabled(true);
                    exitButton1.style.display = DisplayStyle.Flex;
                    exitButton2.SetEnabled(true);
                    exitButton2.style.display = DisplayStyle.Flex;
                    var exitText = root.Q<Label>("Exit_Message");
                    exitText.SetEnabled(false);
                    exitText.style.display = DisplayStyle.None;
                }
                else
                {
                    firstOption = root.Q<Button>("Exit_To_Menu_Option");
                    var exitButton1 = root.Q<VisualElement>("Exit_To_Menu_Option");
                    var exitButton2 = root.Q<VisualElement>("Exit_Game_Option");
                    exitButton1.SetEnabled(false);
                    exitButton1.style.display = DisplayStyle.None;
                    exitButton2.SetEnabled(false);
                    exitButton2.style.display = DisplayStyle.None;
                    var exitText = root.Q<Label>("Exit_Message");
                    exitText.SetEnabled(true);
                    exitText.style.display = DisplayStyle.Flex;
                    confirmButton.SetEnabled(true);
                    confirmButton.style.display = DisplayStyle.Flex;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException("Option not found");

        }
        option.AddToClassList("option_button_chosen");
        foreach (Button opt in currentOptions)
        {
            if (option != opt) opt.SetEnabled(false);
            option.focusable = false;
        }
        OpenOptionScreen();
        backButton.SetEnabled(true);
        backButton.style.display = DisplayStyle.Flex;
        firstOption?.Focus();
    }

    private void OnBackPressed(InputAction.CallbackContext obj)
    {
        Back();
    }
    private void Back()
    {
        switch (menuLevel)
        {
            case MenuLevel.menu:
                return;
            case MenuLevel.option_chosen:
                menuLevel = MenuLevel.menu;
                root.Q<VisualElement>("Footer").focusable = false;
                break;
            case MenuLevel.option_specific:
                menuLevel = MenuLevel.option_chosen;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        foreach (var option in currentOptions)
        {
            if (option.name == "Settings_Option")
            {
                root.Q<VisualElement>("Sound_Tab_Screen").style.display = DisplayStyle.None;
                root.Q<VisualElement>("Sound_Tab_Screen").SetEnabled(false);
            }
            option.SetEnabled(true);
            option.focusable = true;
            if (option.ClassListContains("option_button_chosen"))
            {
                option.RemoveFromClassList("option_button_chosen");
            }
            backButton.style.display = DisplayStyle.None;
            backButton.SetEnabled(false);
            confirmButton.style.display = DisplayStyle.None;
            confirmButton.SetEnabled(false);
            CloseOptionScreen();
        }
        focusedElement.Focus();
    }

    private void OnConfirmPressed(InputAction.CallbackContext context)
    {
        Confirm();
    }
    private void Confirm()
    {
        if (!this.gameObject.activeSelf) return;
        if (menuLevel == MenuLevel.menu)
        {
            if (focusedElement is Button option)
            {
                ChooseOption(option);
            }

        }
        else if (menuLevel == MenuLevel.option_chosen)
        {
            if (root.panel.focusController.focusedElement is not Button option) return;
            if (option.Equals(root.Q<VisualElement>("PvP_Option")))
            {
                SceneManager.LoadScene("Scenes/CharacterSelection");
            }
            else if (option.Equals(root.Q<VisualElement>("Exit_Option")))
            {
                Application.Quit();
            }
            else if (option.Equals(root.Q<VisualElement>("Exit_To_Menu_Option")))
            {
                SceneManager.LoadScene("Scenes/MainMenu");
            }
            else if (option.Equals(root.Q<VisualElement>("Exit_Game_Option")))
            {
                var exitButton1 = root.Q<VisualElement>("Exit_To_Menu_Option");
                var exitButton2 = root.Q<VisualElement>("Exit_Game_Option");
                exitButton1.SetEnabled(false);
                exitButton1.style.display = DisplayStyle.None;
                exitButton2.SetEnabled(false);
                exitButton2.style.display = DisplayStyle.None;
                var exitText = root.Q<Label>("Exit_Message");
                exitText.SetEnabled(true);
                exitText.style.display = DisplayStyle.Flex;
                root.Q<VisualElement>("Exit_Option").Focus();
                confirmButton.SetEnabled(true);
                confirmButton.style.display = DisplayStyle.Flex;
            }

        }
    }
    private void OpenOptionScreen()
    {
        if (currentSideScreen == sidePlaceholder) return;
        DisablePlaceholder();
        currentSideScreen.SetEnabled(true);
        currentSideScreen.style.display = DisplayStyle.Flex;
    }
    private void CloseOptionScreen()
    {
        EnablePlaceholder();
        currentSideScreen.SetEnabled(false);
        currentSideScreen.style.display = DisplayStyle.None;
    }
    private void DisablePlaceholder() //disable side placeholder
    {
        sidePlaceholder.style.display = DisplayStyle.None;
        sidePlaceholder.SetEnabled(false);
    }
    private void EnablePlaceholder() //enable side placeholder
    {
        sidePlaceholder.SetEnabled(true);
        sidePlaceholder.style.display = DisplayStyle.Flex;
    }

    private void CleanTheShit()
    {
        confirmAction.performed -= OnConfirmPressed;
        cancelAction.performed -= OnBackPressed;

    }

    private void OnDestroy()
    {
        CleanTheShit();
    }

}
