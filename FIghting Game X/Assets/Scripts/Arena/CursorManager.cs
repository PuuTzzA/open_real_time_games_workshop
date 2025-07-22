using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  // Locks the cursor to the center of the screen
        Cursor.visible = false;                    // Hides the cursor

        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0; // Disable VSync so Application.targetFrameRate takes effect
    }

}
