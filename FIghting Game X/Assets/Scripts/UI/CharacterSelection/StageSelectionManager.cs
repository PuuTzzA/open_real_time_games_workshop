using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class StageSelectionManager : MonoBehaviour
{
    [Header("Stage Data")]
    public List<string> stageSceneNames;           // e.g. "VolcanoScene", "ForestScene"
    public List<Sprite> stagePreviewImages;        // Preview images for each stage
    public List<string> stageDisplayNames;         // Optional: Friendly names

    [Header("UI References")]
    public Image previewImage;                     // Image showing current stage
    public TMP_Text stageNameText;                 // Stage name display
    public TMP_Text instructionText;               // "Press A to select"

    [Header("Input")]
    public InputActionAsset inputActions;          // Reference to your input actions

    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    private int selectedStageIndex = 0;

    private void Awake()
    {
        var uiMap = inputActions.FindActionMap("UI");
        navigateAction = uiMap.FindAction("Navigate");
        submitAction = uiMap.FindAction("Submit");
        cancelAction = uiMap.FindAction("Cancel");

        navigateAction.performed += OnNavigate;
        submitAction.performed += OnSubmit;
        //cancelAction.performed += OnCancel;

        uiMap.Enable();


    }



    private void OnDestroy()
    {
        navigateAction.performed -= OnNavigate;
        submitAction.performed -= OnSubmit;
    }

    private void Start()
    {
        UpdateStageUI();
    }

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        if (Mathf.Abs(input.x) > 0.5f)
        {
            if (input.x > 0) selectedStageIndex++;
            else selectedStageIndex--;

            selectedStageIndex = (selectedStageIndex + stageSceneNames.Count) % stageSceneNames.Count;
            UpdateStageUI();
        }
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // Store selection
        GameManager.SelectedStageScene = stageSceneNames[selectedStageIndex];

        // ToDo: Change this to your real character select scene (if needed)
        SceneManager.LoadScene(GameManager.SelectedStageScene);
    }

/*     public void OnCancel(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        SceneManager.LoadScene("CharacterSelection");
    } */

    private void UpdateStageUI()
    {
        if (previewImage != null && selectedStageIndex < stagePreviewImages.Count)
        {
            previewImage.sprite = stagePreviewImages[selectedStageIndex];
        }

        if (stageNameText != null && selectedStageIndex < stageDisplayNames.Count)
        {
            stageNameText.text = stageDisplayNames[selectedStageIndex];
        }

        if (instructionText != null)
        {
            instructionText.text = "Press A to select";
        }
    }
}
