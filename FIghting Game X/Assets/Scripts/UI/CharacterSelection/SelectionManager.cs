using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Linq;

public class SelectionManager : MonoBehaviour
{
    // All instances of SelectionManager
    private static List<SelectionManager> _instances = new List<SelectionManager>();
    
    [Header("Character Selection")]
    public GameObject[] characters;
    public int selectedCharacter = -1;
    [SerializeField] private List<SpriteRenderer> spriteRenderers;
    
    [Header("UI References")]
    public Image characterDisplayImage;
    public TMP_Text characterNameText;
    public Button startGameButton;
    public Button backButton;
    public TMP_Text player_id;
    public GameObject characterSelectionUI;
    public GameObject readyIcon;
    
    // Input Actions
    private InputActionAsset InputActions;
    
    // General attributes
    private PlayerInput _playerInput;
    private bool _isNavigatingCharacters = true;
    private bool _isReady;
    
    // Block onSubmit to prevent accidentaly getting ready by pressing x to join
    [SerializeField] private float submitBlockDuration = 0.3f;
    private float _submitBlockUntil;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        InputActions = _playerInput.actions;
        
        _instances.Add(this);
        
        player_id.text = "Player" + _playerInput.user.id;
        readyIcon.SetActive(false);
        _isReady = false;

        _submitBlockUntil = Time.time + submitBlockDuration;
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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        InputActions.FindActionMap("UI").Disable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "CharacterController")
        {
            // Hide selection UI
            characterSelectionUI.SetActive(false);
            readyIcon.SetActive(false);

            // Disable the UI camera (if any)
            var uiCam = GetComponentInChildren<Camera>();
            if (uiCam != null) uiCam.enabled = false;
        }
    }
    
    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (_isReady) return;
        Debug.Log("NAVIGATE");
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
        if (Time.time < _submitBlockUntil || _isReady) return;
        
        Debug.Log("SUBMIT");
        ToggleReady();
    }
    
    public void OnCancel(InputAction.CallbackContext context)
    {
        Debug.Log("CANCEL");
        if (_isReady)
        {
            ToggleReady();
        }
        // Go back or exit
        GoBack();
    }

    private void ToggleReady()
    {
        if (!_isReady)
        {
            GameManager.PlayerChoices[_playerInput.playerIndex] = selectedCharacter;
        }
        else
        {
            GameManager.PlayerChoices[_playerInput.playerIndex] = -1;
        }
        
        _isReady = !_isReady;
        readyIcon.SetActive(_isReady);
        characterSelectionUI.SetActive(!_isReady);
        startGameButton.interactable = !_isReady;

        if (_isReady)
        {
            if (_instances.All(x => x._isReady))
                SceneManager.LoadScene("CharacterController");
        }
    }
    
    void ShowCharacter(int index)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(i == index);
        }
        
        if (characterDisplayImage != null)
        {
            SpriteRenderer spriteRenderer = spriteRenderers[index].GetComponent<SpriteRenderer>();
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
    
    public void GoBack()
    {
        //SceneManager.LoadScene("MainMenu");
    }
}