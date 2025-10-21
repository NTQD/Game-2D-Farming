
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    [Tooltip("Unique identifier so the quest manager can track state across sessions.")]
    public string questId;

    public string title;
    [TextArea]
    public string description;

    [Tooltip("Optional quests that must be completed before this one becomes available.")]
    public List<Quest> prerequisites = new List<Quest>();

    [Tooltip("Time limit expressed in in-game days. Use -1 for no limit.")]
    public int timeLimitInDays = -1;

    [TextArea]
    [Tooltip("Message displayed when the quest fails. Leave empty to hide.")]
    public string failureDescription;

    public List<QuestObjective> objectives = new List<QuestObjective>();
    public QuestReward reward;

    [System.Serializable]
    public class QuestReward
    {
        public Item itemReward;
        public int itemAmount;
    }
}

[System.Serializable]
public class QuestObjective
{
    public ObjectiveType type;
    [TextArea]
    public string description;

    [Tooltip("Item reference for gather/plant/stockpile style objectives.")]
    public Item item;

    [Tooltip("Generic identifier for world events, blueprints, or structures.")]
    public string targetId;

    [Tooltip("How much progress is required for the objective to succeed.")]
    public int amount = 1;

    [Tooltip("When greater than zero, progress must be achieved in a single streak (e.g. consecutive nights).")] 
    public int consecutiveSuccessTarget;

    [Tooltip("Failure threshold for consecutive misses. Zero disables failure tracking.")]
    public int consecutiveFailureLimit;

    [Tooltip("Should the manager evaluate inventory totals instead of additive progress (useful for stockpiles).")]
    public bool useInventoryTotal;

    [HideInInspector]
    public bool isCompleted;
}

public enum ObjectiveType
{
    Gather,
    Build,
    Explore,
    Trade,
    Plant,
    MaintainCampfire,
    AcquireBlueprint,
    ConstructStructure,
    Stockpile,
    AssembleBoat,
    Interact
}
