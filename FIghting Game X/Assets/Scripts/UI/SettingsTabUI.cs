using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsTabUI : MonoBehaviour
{
    [SerializeField] UIDocument menuDocument;
    
    private VisualElement root;
    private List <SettingsTab> tabs;
    private SettingsTab currentTab;


    void Awake()
    {
        root = menuDocument.rootVisualElement;
        var tabScreens = root.Query<VisualElement>(className: "settings_tab_screen").ToList();;
        tabScreens.Sort((a,b) => (a.tabIndex.CompareTo(b.tabIndex)));
        var tabHeaders = root.Query<VisualElement>(className: "settings_tab_header").ToList(); 
        tabHeaders.Sort((a,b) => (a.tabIndex.CompareTo(b.tabIndex)));
        tabs = (from tabHeader in tabHeaders
                join tabScreen in tabScreens on tabHeader.tabIndex equals tabScreen.tabIndex
                select new SettingsTab(tabHeader, tabScreen)).ToList();
        foreach (SettingsTab tab in tabs)
        {
            tab.GetHeader().RegisterCallback<FocusInEvent>((evt) => OnTabChosen(evt, tab)
            );
        }
        currentTab = tabs[0];
        OpenTab(currentTab);
    }

    public Button FirstSetting()
    {
        currentTab = tabs[0];
        return currentTab.GetHeader() as Button;
    }
    private void OnTabChosen(FocusInEvent evt, SettingsTab tab)
    {
        OpenTab(tab);
    }

    private void OpenTab(SettingsTab tab)
    {
        if (currentTab != tab)
        {
            CloseTab(currentTab);
            currentTab = tab;
        }
        tab.GetScreen().SetEnabled(true);
        tab.GetScreen().style.display = DisplayStyle.Flex;
    }

    private void CloseTab(SettingsTab tab)
    {
        tab.GetScreen().SetEnabled(false);
        tab.GetScreen().style.display = DisplayStyle.None;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class SettingsTab
{
    private VisualElement tabHeader;
    private VisualElement tabScreen;

    public VisualElement GetHeader()
    {
        return tabHeader;
    }
    public VisualElement GetScreen()
    {
        return tabScreen;
    }
   
    public SettingsTab(VisualElement tabHeader, VisualElement tabScreen)
    {
        this.tabHeader = tabHeader;
        this.tabScreen = tabScreen;
    }
}