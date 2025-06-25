using UnityEngine;
using UnityEngine.InputSystem;

public class QTETriggerTester : MonoBehaviour
{
    private PlayerInput input;
    private BaseFighter self;
    private InputAction test_minigame_action;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        self = GetComponent<BaseFighter>();
        test_minigame_action = input.actions["test-minigame"];
    }

    private void OnEnable()
    {
        test_minigame_action.performed += OnTestMinigame;
        test_minigame_action.Enable();
    }

    private void OnDisable()
    {
        test_minigame_action.performed -= OnTestMinigame;
        test_minigame_action.Disable();
    }

    public void OnTestMinigame(InputAction.CallbackContext ctx)
    {
        Debug.Log("Test minigame action triggered");
        GameObject killer = this.gameObject;
        GameObject fallen = FindClosestOtherFighter(killer);

        if (fallen != null)
        {
            Debug.Log($"{killer.name} triggering test QTE against {fallen.name}");
            QTEManager.Instance.StartQTE(fallen, killer);
        }
        else
        {
            Debug.LogWarning("No nearby fighter found to target.");
        }
    }

    private GameObject FindClosestOtherFighter(GameObject me)
    {
        float closestDist = float.MaxValue;
        GameObject closest = null;

        foreach (var fighter in FindObjectsOfType<BaseFighter>())
        {
            if (fighter.gameObject == me) continue;

            float dist = Vector2.Distance(me.transform.position, fighter.transform.position);
            if (dist < closestDist)
            {
                closest = fighter.gameObject;
                closestDist = dist;
            }
        }

        return closest;
    }
}