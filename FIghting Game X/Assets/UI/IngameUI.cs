using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class IngameUI : MonoBehaviour
{
    [SerializeField] private Sprite[] stocks;
    private VisualElement root;
    private PartialCircle[] healthbar = new PartialCircle[4];
    private Coroutine[] arc2Coroutines = new Coroutine[4];
    private float[] currentHealth = new float[4];
    private PersistentPlayerManager persistentPlayerManager;

    public float blink_duration = 0.2f;
    public float removed_health_duration = 0.2f;
    public Color blink_color = Color.white;

    private float health = 0.5f;
    private Color color;

    void Awake()
    {
        persistentPlayerManager = FindAnyObjectByType<PersistentPlayerManager>();
    }

    void OnEnable()
    {
        int players = persistentPlayerManager.getPlayers().Count;
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null) root = uiDocument.rootVisualElement;

        for (int i = 0; i < 4; i++)
        {
            if (i >= players)
            {
                root.Q<SquareElement>($"healtbar{i}").RemoveFromHierarchy();
                continue;
            }

            var bar = root.Q<SquareElement>($"healtbar{i}").hierarchy.ElementAt(0).hierarchy.ElementAt(0);
            healthbar[i] = bar as PartialCircle;

            float startHealth = 0.5f;
            healthbar[i].Fill = startHealth;
            healthbar[i].Arc2Fill = startHealth;
            healthbar[i].Arc3Fill = startHealth;
            currentHealth[i] = startHealth;

            bar.hierarchy.ElementAt(1).style.backgroundImage = new StyleBackground(
                persistentPlayerManager.getPlayers()[i].GetComponent<FighterHealth>().Icon);
            bar.hierarchy.ElementAt(1).style.unityBackgroundImageTintColor =
                persistentPlayerManager.getPlayers()[i].GetComponentInChildren<BaseFighter>().playerColor;

            setHealth(i, 1);
            (bar.hierarchy.ElementAt(2) as ResponsiveLabel).text = "Player" + (i + 1);
        }

        color = healthbar[0].Arc3Color;
    }

    public void setHealth(int playerid, float health)
    {
        health /= 2;
        this.health = health;
        health = Mathf.Clamp(health, 0f, 1f);
        currentHealth[playerid] = health;

        healthbar[playerid].Arc2Fill = health;
        healthbar[playerid].Arc3Fill = health;
    }

    public void setNewHealth(int playerid, float health)
    {
        health /= 2;
        health = Mathf.Clamp(health, 0f, 1f);
        this.health = health;
        currentHealth[playerid] = health;

        // Update Arc3Fill instantly
        healthbar[playerid].Arc3Fill = health;

        // Cancel old coroutine if it's running
        if (arc2Coroutines[playerid] != null)
        {
            StopCoroutine(arc2Coroutines[playerid]);
        }

        // Start a new smooth animation coroutine
        arc2Coroutines[playerid] = StartCoroutine(AnimateArc2Fill(playerid, health));
    }

    private IEnumerator AnimateArc2Fill(int playerid, float targetFill)
    {
        yield return new WaitForSeconds(removed_health_duration);

        float startFill = healthbar[playerid].Arc2Fill;
        float elapsed = 0f;

        while (elapsed < removed_health_duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / removed_health_duration;
            healthbar[playerid].Arc2Fill = Mathf.SmoothStep(startFill, targetFill, t);
            yield return null;
        }

        healthbar[playerid].Arc2Fill = targetFill;
        arc2Coroutines[playerid] = null;
    }

    public void changeStocks(int playerid, int stocks)
    {
        healthbar[playerid].hierarchy.ElementAt(0).style.backgroundImage = new StyleBackground(this.stocks[stocks]);
    }

    public void removeExtraStock(int playerid)
    {
        healthbar[playerid].hierarchy.ElementAt(0).style.backgroundImage = new StyleBackground(this.stocks[4]);
    }
}
