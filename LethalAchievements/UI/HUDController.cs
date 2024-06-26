﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using LethalAchievements.Features;
using LethalAchievements.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalAchievements.UI;

internal class HUDController : MonoBehaviour
{
    
    private static Button OpenButton, ExitButton;
    private TMP_Text OpenButtonText, ExitButtonText;
    private Transform UI;
    private static Transform AchievementContainer;
    private static Transform AchievementTemplate;
    private static Transform ModContainer;
    private static Transform ModTemplate;
   internal static AudioSource ClickSFX;
    
    private static string ModName;

    internal static List<ModEntry> ModList = [];
    private void Awake()
    {
        // Get all OpenButton components
        OpenButton = transform.Find("OpenButton").GetComponent<Button>();
        OpenButtonText = transform.Find("OpenButton/Text").GetComponent<TMP_Text>();
        
        // Get all ExitButton components
        ExitButton = transform.Find("ExitButton").GetComponent<Button>();
        ExitButtonText = transform.Find("ExitButton/Text").GetComponent<TMP_Text>();
        
        // Get UI components
        UI = transform.Find("UI");
        AchievementContainer = transform.Find("UI/AchievementListContainer/Scroll View/Viewport/Content");
        AchievementTemplate = AchievementContainer.Find("Achievement");
        
        ModContainer = transform.Find("UI/ModTabContainer/Scroll View/Viewport/Content");
        ModTemplate = ModContainer.Find("Mod");
        
        ClickSFX = ModContainer.Find("ClickSFX").GetComponent<AudioSource>();
        
        // Add method calls on button clicks
        OpenButton.onClick.AddListener(() => OpenUI());
        ExitButton.onClick.AddListener(() => CloseUI());
        
        // Set initial pane to only display the achievements button
        UI.gameObject.SetActive(false);
        ModTemplate.gameObject.SetActive(false);
        AchievementTemplate.gameObject.SetActive(false);
        ExitButton.gameObject.SetActive(false);
        OpenButton.gameObject.SetActive(true);
        
        LethalAchievements.Logger?.LogInfo("UI loaded!");
    }

    private void OpenUI()
    {
        OpenButton.gameObject.SetActive(false);
        UI.gameObject.SetActive(true);
        ExitButton.gameObject.SetActive(true);
    }
    private void CloseUI()
    {
        ExitButton.gameObject.SetActive(false);
        UI.gameObject.SetActive(false);
        OpenButton.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (OpenButton.gameObject.activeSelf)
            OpenButtonText.color = OpenButton.isPointerInside
                ? Color.black
                : new Color32(255, 126, 63, 150);
        if (ExitButton.gameObject.activeSelf)
            ExitButtonText.color = ExitButton.isPointerInside
                ? Color.black
                : new Color32(255, 50, 0, 255);
    }

    internal void InitializeUI()
    {
        // Logic to add mods that have achievements
        foreach (var mod in AchievementManager.AchievementRegistry.GetAchievementsByPlugins())
        {
            // Get RegEx'd name to display on tab
            var regEx = Regex.Match(mod.Key.Metadata.Name, @"^([^.]*)$|^(.*?)\.(.*)$");
            var modName = regEx.Groups.Count < 2 ? regEx.Groups[1].Value : regEx.Groups[3].Value;
            
            LethalAchievements.Logger?.LogInfo($"Adding {mod.Key.Metadata.Name} to the mod tab");
            
            // Create tab for given mod
            var modObj = Instantiate(ModTemplate, ModContainer);
            var modEntry = modObj.gameObject.AddComponent<ModEntry>();
            modEntry.Init(modName, mod.Value, ModList, AchievementContainer);
            modObj.gameObject.SetActive(true);
            
            ModList.Add(modEntry);
        }
    }
}