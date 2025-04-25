using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public string Id;
    public string Title;
    public string RequestInformation;
    public string Objective;
    public int TargetAmount;
    public int RewardSoul;
    public string Description;

    [NonSerialized] public int CurrentAmount;
    [NonSerialized] public bool isCompleted;

    public float Progress => (float)CurrentAmount / TargetAmount;

    public override string ToString()
    {
        return $"{Title} ({Id}): {Description}";
    }
}
