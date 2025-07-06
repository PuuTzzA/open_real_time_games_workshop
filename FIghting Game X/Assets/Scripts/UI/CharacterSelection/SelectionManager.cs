using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Linq;
using System.Reflection.Emit;

public class SelectionManager : MonoBehaviour
{
    private PersistentPlayerManager manager;


    [Header("Color Selection")]
    public int selectedColorIndex = 0;
    // All instances of SelectionManager
    private static List<SelectionManager> _instances = new List<SelectionManager>();

    [Header("Character Selection")]
    public GameObject[] characters;
    public int selectedCharacter = -1;

    [Header("UI References")]
    public Image characterDisplayImage;
    public TMP_Text selectText;
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

    // For testing
    private string fightScene = "TestSceneMartin";


    private enum SelectionState
    {
        ChoosingCharacter,
        ChoosingColor,
        Ready
    }

    private SelectionState _state = SelectionState.ChoosingCharacter;

    private void Awake()
    {
        manager = FindAnyObjectByType<PersistentPlayerManager>();
        _playerInput = GetComponent<PlayerInput>();
        InputActions = _playerInput.actions;

        _instances.Add(this);
        player_id.text = "Player " + _playerInput.playerIndex;
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
        if (scene.name == "TestSceneMartin")
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
        Vector2 navigate = context.ReadValue<Vector2>();

        if (_state == SelectionState.ChoosingCharacter && Mathf.Abs(navigate.x) > 0.5f)
        {
            if (navigate.x > 0) NextCharacter();
            else PreviousCharacter();
        }
        else if (_state == SelectionState.ChoosingColor && Mathf.Abs(navigate.x) > 0.5f)
        {
            if (navigate.x > 0) NextColor();
            else PreviousColor();
        }

        // (optional) update UI selection state like before
    }


    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (Time.time < _submitBlockUntil) return;

        switch (_state)
        {
            case SelectionState.ChoosingCharacter:
                if (selectedCharacter >= 0)
                {
                    GameManager.PlayerChoices[_playerInput.playerIndex] = selectedCharacter;
                    _state = SelectionState.ChoosingColor;
                    selectText.text = "Select your Color:"; // ✅ Correct now
                    ShowCharacter(selectedCharacter); // ✅ Refresh color
                    Debug.Log("Now choosing color");
                }
                break;

            case SelectionState.ChoosingColor:
                GameManager.PlayerColorChoices[_playerInput.playerIndex] = selectedColorIndex;
                _state = SelectionState.Ready;
                selectText.text = "Ready!";
                ApplyReadyState(true);
                Debug.Log("Now ready");
                break;
        }
    }




    public void OnCancel(InputAction.CallbackContext context)
    {
        switch (_state)
        {
            case SelectionState.Ready:
                ApplyReadyState(false);
                GameManager.PlayerColorChoices[_playerInput.playerIndex] = -1;
                _state = SelectionState.ChoosingColor;
                selectText.text = "Select your Color:";
                break;

            case SelectionState.ChoosingColor:
                GameManager.PlayerChoices[_playerInput.playerIndex] = -1;
                _state = SelectionState.ChoosingCharacter;
                selectText.text = "Select your Character:";
                break;

            case SelectionState.ChoosingCharacter:
                selectText.text = "Select your Character:";
                GoBack();
                break;
        }
    }



    private void ApplyReadyState(bool isReady)
    {
        _isReady = isReady;
        readyIcon.SetActive(isReady);
        characterSelectionUI.SetActive(!isReady);
        startGameButton.interactable = !isReady;

        if (_isReady && _instances.All(x => x._state == SelectionState.Ready))
        {
            SceneManager.LoadScene(fightScene);
        }
    }
    void ShowCharacter(int index)
    {
        // Get the sprite *before* toggling active states
        FighterHealth fighterHealth = characters[index].GetComponentInChildren<FighterHealth>();

        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(i == index);
        }

        if (characterDisplayImage != null && fighterHealth != null)
        {
            characterDisplayImage.sprite = fighterHealth.IconWithoutBackground;
            characterNameText.text = characters[index].name;

            if (_state == SelectionState.ChoosingCharacter)
            {
                characterDisplayImage.color = manager.availableColors[index];
            }
            else
            {
                ApplyColorPreview();
            }
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

    private bool IsColorTaken(int colorIndex)
    {
        return GameManager.PlayerColorChoices
            .Where((choice, i) => i != _playerInput.playerIndex) // exclude self
            .Contains(colorIndex);
    }

    public void NextColor()
    {

        int originalIndex = selectedColorIndex;
        do
        {
            selectedColorIndex = (selectedColorIndex + 1) % manager.availableColors.Length;
        } while (IsColorTaken(selectedColorIndex) && selectedColorIndex != originalIndex);

        ApplyColorPreview();
    }

    public void PreviousColor()
    {
        int originalIndex = selectedColorIndex;
        do
        {
            selectedColorIndex = (selectedColorIndex - 1 + manager.availableColors.Length) % manager.availableColors.Length;
        } while (IsColorTaken(selectedColorIndex) && selectedColorIndex != originalIndex);

        ApplyColorPreview();
    }

    private void ApplyColorPreview()
    {
        Debug.Log("" + characterDisplayImage);
        if (characterDisplayImage != null && selectedColorIndex >= 0)
        {
            characterDisplayImage.color = manager.availableColors[selectedColorIndex];
        }
    }


}