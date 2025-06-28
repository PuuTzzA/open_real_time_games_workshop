using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    [SerializeField] UIDocument menuDocument;
    [SerializeField] SettingsTabUI settingsTabUI;
    private VisualElement root;
   
    #region menu options
    
    private VisualElement playOption;
    private VisualElement infoOption;
    private VisualElement settingsOption; 
    private VisualElement exitOption;
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
    private VisualElement currentSideScreen;
    #endregion

    private Focusable focusedElement;
    public enum MenuLevel
    {
        menu,
        option_chosen,
        option_specific
    }
    private MenuLevel menuLevel = MenuLevel.menu;
    
    void Awake()
    {
        root = menuDocument.rootVisualElement;
        #region screens initilaization
        sidePlaceholder = root.Q<VisualElement>("Default_side_placeholder");
        exitScreen = root.Q<VisualElement>("Exit_option_screen");
        playScreen = root.Q<VisualElement>("Play_option_screen");
        infoScreen = root.Q<VisualElement>("Info_option_screen");
        settingsScreen = root.Q<VisualElement>("Settings_option_screen");
        #endregion
        #region menu options intialization
        playOption = root.Q<VisualElement>("Play_Option");
        settingsOption = root.Q<VisualElement>("Settings_Option");
        infoOption = root.Q<VisualElement>("Info_Option");
        exitOption = root.Q<VisualElement>("Exit_Option");
        menuOptions = root.Query<Button>(className: "menu_option_button").ToList();
        focusedElement = root.Q<Button>(className: "menu_option_button");
        focusedElement.Focus();
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
        confirmAction.performed += ctx => OnConfirmPressed();
        cancelAction.performed += ctx => OnBackPressed();
        #endregion
        
    }

    private void OnOptionClicked(ClickEvent evt)
    {
        if (evt.target is not Button option) return;
        ChooseOption(option);
    }

    private void ChooseOption(Button option)
    {
        menuLevel = MenuLevel.option_chosen;
        option.AddToClassList("option_button_chosen");
        Button firstOption = null;
        switch (option.name)
        {
            case "Play_Option":
                currentSideScreen = playScreen;
                firstOption = root.Q<Button>(className: "play_option_button");
                break;
            case "Settings_Option":
                currentSideScreen = settingsScreen;
                firstOption = settingsTabUI.FirstSetting();
                break;
            case "Info_Option":
                currentSideScreen = infoScreen;
                break;
            case "Exit_Option":
                currentSideScreen = exitScreen;
                confirmButton.SetEnabled(true);
                confirmButton.style.display = DisplayStyle.Flex;
                break;
            default:
                throw new ArgumentOutOfRangeException("Option not found");
                    
        }
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
    private void OnBackPressed()
    {
        switch (menuLevel)
        {
            case MenuLevel.menu:
                return;
            case MenuLevel.option_chosen:
                menuLevel = MenuLevel.menu;
                break;
            case MenuLevel.option_specific:
                menuLevel = MenuLevel.option_chosen;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        foreach (Button option in currentOptions)
        {
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

    private void OnConfirmPressed()
    {
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

        }
    }
    private void OpenOptionScreen() {
        DisablePlaceholder();
        currentSideScreen.SetEnabled(true);
        currentSideScreen.style.display = DisplayStyle.Flex;
    }
    private void CloseOptionScreen() {
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
    



    void Start()
    {
       
    }



    private void OnClickEvent(ClickEvent evt)
    {
        //;
    }
    


    


    // Update is called once per frame
    void Update()
    {
        
    }
}
