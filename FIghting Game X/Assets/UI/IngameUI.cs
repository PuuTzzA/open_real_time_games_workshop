using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class IngameUI : MonoBehaviour
{
    [SerializeField]
    private Sprite[] stocks;
    private VisualElement root;
    private PartialCircle[] healthbar = new PartialCircle[4];
    Color color;

    public float blink_duration = 0.2f;
    public float removed_health_duration = 0.2f;

    public Color blink_color = Color.white;

    private float health = 0.5f;

    private PersistentPlayerManager persistentPlayerManager;

    void Awake()
    {
        persistentPlayerManager = FindAnyObjectByType<PersistentPlayerManager>();
    }

    void OnEnable()
    {
        int players = persistentPlayerManager.getPlayers().Count;
        var uiDocument = GetComponent<UIDocument>();

        if (uiDocument != null)
        {
            root = uiDocument.rootVisualElement;
        }
        for (int i = 0; i < 4; i++)
        {
            if (i >= players)
            {
                root.Q<SquareElement>($"healtbar{i}").RemoveFromHierarchy();
                continue;
            }
            healthbar[i] = root.Q<SquareElement>($"healtbar{i}").hierarchy.ElementAt(0).hierarchy.ElementAt(0) as PartialCircle;
            healthbar[i].Fill = health;
            healthbar[i].Arc2Fill = health;
            healthbar[i].Arc3Fill = health;
            healthbar[i].hierarchy.ElementAt(1).style.backgroundImage = new StyleBackground(persistentPlayerManager.getPlayers()[i].GetComponent<FighterHealth>().Icon);
            setHealth(i, 1);
        }
        color = healthbar[0].Arc3Color;
    }

    public void setHealth(int playerid, float health)
    {
        this.health = health;
        healthbar[playerid].Arc2Fill = health / 2;
        healthbar[playerid].Arc3Fill = health / 2;
    }

    public void setNewHealth(int playerid, float health)
    {
        this.health = health;
        healthbar[playerid].Arc3Fill = health / 2;
        //healthbar[playerid].Arc3Color = blink_color;
        StartCoroutine(DelayedArc2Fill(playerid, health));
        StartCoroutine(DelayedColorChange(playerid, color));
    }

    IEnumerator DelayedArc2Fill(int playerid, float health)
    {
        yield return new WaitForSeconds(removed_health_duration);
        healthbar[playerid].Arc2Fill = health / 2;

    }

    IEnumerator DelayedColorChange(int playerid, Color color)
    {
        yield return new WaitForSeconds(blink_duration);
        //healthbar[playerid].Arc3Color = color;
    }

    public void changeStocks(int playerid, int stocks)
    {
        healthbar[playerid].parent.hierarchy.ElementAt(0).style.backgroundImage = new StyleBackground(this.stocks[stocks]);
    }
    public void removeExtraStock(int playerid)
    {
        healthbar[playerid].parent.hierarchy.ElementAt(0).style.backgroundImage = new StyleBackground(this.stocks[4]);

    }
}
