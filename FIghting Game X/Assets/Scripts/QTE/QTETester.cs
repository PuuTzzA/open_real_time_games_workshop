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
        GameObject killer = this.gameObject;
        GameObject fallen = FindClosestOtherFighter(killer);

        if (fallen != null)
        {
            //QTEManager.Instance.StartQTE(fallen, killer, );
        }
        else
        {
        }
    }

    private GameObject FindClosestOtherFighter(GameObject me)
    {
        float closestDist = float.MaxValue;
        GameObject closest = null;

        foreach (var fighter in FindObjectsByType<BaseFighter>(FindObjectsSortMode.None))
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