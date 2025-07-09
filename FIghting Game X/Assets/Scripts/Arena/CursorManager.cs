using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  // Locks the cursor to the center of the screen
        Cursor.visible = false;                    // Hides the cursor
    }
}
