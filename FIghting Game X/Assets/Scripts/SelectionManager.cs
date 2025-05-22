using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    [Header("Character Selection")]
    public GameObject[] characters;
    public int selectedCharacter = 0;
    
    [Header("UI References")]
    public Image characterDisplayImage;
    public TMP_Text characterNameText;
    public Button startGameButton;
    public Button backButton;
    
    // Input Actions
    private InputActionAsset InputActions;
    
    private PlayerInput _playerInput;
    private bool _isNavigatingCharacters = true;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        Debug.Log(_playerInput.user.id);
    }

    void Start()
    {
        ShowCharacter(selectedCharacter);
        
        // Set initial UI selection
        EventSystem.current.SetSelectedGameObject(startGameButton.gameObject);
    }
    
    
    private void OnEnable()
    {
        InputActions.FindActionMap("UI").Enable();
    }
    
    private void OnDisable()
    {
        InputActions.FindActionMap("UI").Disable();
    }
    
    public void OnNavigate(InputAction.CallbackContext context)
    {
        Debug.Log("Navigating");
        Vector2 navigate = context.ReadValue<Vector2>();
        
        // Handle horizontal navigation for character selection
        if (Mathf.Abs(navigate.x) > 0.5f)
        {
            if (navigate.x > 0)
                NextCharacter();
            else
                PreviousCharacter();
        }
        
        // Handle vertical navigation between UI elements
        if (Mathf.Abs(navigate.y) > 0.5f)
        {
            if (navigate.y > 0)
            {
                // Navigate up - could switch between character selection and buttons
                _isNavigatingCharacters = true;
                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                // Navigate down - go to buttons
                _isNavigatingCharacters = false;
                EventSystem.current.SetSelectedGameObject(startGameButton.gameObject);
            }
        }
    }
    
    public void OnSubmit(InputAction.CallbackContext context)
    {
        Debug.Log("Submitting");
        if (_isNavigatingCharacters)
        {
            StartGame();
        }
        else
        {
            // Let the UI system handle button presses
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                Button button = selected.GetComponent<Button>();
                if (button != null)
                    button.onClick.Invoke();
            }
        }
    }
    
    public void OnCancel(InputAction.CallbackContext context)
    {
        Debug.Log("Canceling");
        // Go back or exit
        GoBack();
    }
    
    void ShowCharacter(int index)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(i == index);
        }
        
        if (characterDisplayImage != null)
        {
            SpriteRenderer spriteRenderer = characters[index].GetComponent<SpriteRenderer>();
            characterDisplayImage.sprite = spriteRenderer.sprite;
            characterDisplayImage.color = spriteRenderer.color;
            characterNameText.text = characters[index].name;
        }
    }

    public void NextCharacter()
    {
        selectedCharacter = (selectedCharacter + 1) % characters.Length;
        ShowCharacter(selectedCharacter);
    }
    
    public void PreviousCharacter()
    {
        selectedCharacter = (selectedCharacter - 1 + characters.Length) % characters.Length;
        ShowCharacter(selectedCharacter);
    }
    
    public void StartGame()
    {
        PlayerPrefs.SetInt("SelectedCharacter", selectedCharacter);
        PlayerPrefs.Save();
        SceneManager.LoadScene("CharacterController");
    }
    
    public void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }
}