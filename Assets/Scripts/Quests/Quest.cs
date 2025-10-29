
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string title;
    [TextArea]
    public string description;
    public Quest prerequisiteQuest; // New: Quest that must be completed before this one can start
    public List<QuestStage> stages; // Changed to stages
    public QuestReward reward;
    public bool isCompleted = false; // Added to track quest completion
    public int currentStageIndex = 0; // New: To track current stage

    [System.Serializable]
    public class QuestReward
    {
        public Item itemReward;
        public int itemAmount;
    }
}

[System.Serializable]
public class QuestStage
{
    public string stageName;
    [TextArea]
    public string stageDescription;
    public List<QuestObjective> objectives;
    public bool isStageCompleted = false; // New: To track stage completion
}

[System.Serializable]
public class QuestObjective
{
    public ObjectiveType type;
    public string description;
    public Item item; // For gathering objectives
    public int amount;
    public int currentProgress; // New: To track current progress for the objective
    public bool isCompleted;
}

public enum ObjectiveType
{
    Gather,
    Build,
    Explore,
    Trade,
    Plant
}
