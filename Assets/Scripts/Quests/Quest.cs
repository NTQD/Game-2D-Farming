
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string title;
    [TextArea]
    public string description;
    public List<QuestObjective> objectives;
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
    public string description;
    public Item item; // For gathering objectives
    public int amount;
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
