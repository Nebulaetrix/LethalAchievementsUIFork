﻿using System;
using System.Linq;
using BepInEx.Logging;
using LethalAchievements.Base;
using UnityEngine;

namespace LethalAchievements.Config;

/// <summary>
///     An achievement loaded from a .json file.
/// </summary>
public class JsonAchievement : BaseAchievement
{
    /// <inheritdoc />
    public override string Name { get; set; }

    /// <inheritdoc />
    public override string? DisplayText { get; set; }

    /// <inheritdoc />
    public override string? Description { get; set; }

    /// <inheritdoc />
    public override Sprite? Icon { get; set; }

    public object Namespace { get; }

    /// <summary>
    ///    Logs to the console every time the achievement would be achieved (even if it's already achieved).
    ///    Make sure to enable the debug log level in the BepInEx config.
    ///    <br/><br/>
    ///    Note that if the achivemenet is achieved at the start of the game, it will not initialize (might change in the future).
    /// </summary>
    public bool Debug = false;
    
    private readonly int[] _criteriaCompletedCount;
    private readonly Criterion[] _criteria;

    /// <summary>
    ///     Creates a new achievement with the given name and criteria.
    /// </summary>
    public JsonAchievement(string @namespace, string name, params Criterion[] criteria)
    {
        Namespace = @namespace;
        Name = name;
        _criteria = criteria;
        _criteriaCompletedCount = new int[criteria.Length];
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        if (Debug) Log("Initialized in debug mode", LogLevel.Debug);
        
        foreach (var criterion in _criteria)
        {
            criterion.Trigger.Initialize();
            criterion.Subscribe(OnCriterionTriggered);
        }
    }

    /// <inheritdoc />
    public override void Uninitialize()
    {
        foreach (var criterion in _criteria)
        {
            criterion.Trigger.Uninitialize();
            criterion.Unsubscribe(OnCriterionTriggered);
        }
    }
    
    private void OnCriterionTriggered(Criterion criterion, Context context)
    {
        var index = Array.IndexOf(_criteria, criterion);
        if (_criteriaCompletedCount[index] >= criterion.RequiredCount && !Debug) return;

        context.Achievement = this;
        if (!criterion.Conditions?.All(condition => condition.Evaluate(in context)) ?? false) return;

        if (Debug)
        {
            Log($"Criterion with trigger {criterion.Trigger.GetType().Name} completed", LogLevel.Debug);
        }
        
        _criteriaCompletedCount[index]++;
        
        var isCompleted = _criteriaCompletedCount
            .Select((count, i) => count >= _criteria[i].RequiredCount)
            .All(completed => completed);

        if (isCompleted) Complete();
    }

    internal void Log(object message, LogLevel level)
    {
        LethalAchievements.Logger?.Log(level, $"[ACHIEVEMENT {Namespace}.{Name}] {message}");
    }
}