using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Deserted Island House Quest", menuName = "Quests/Deserted Island House")]
public class DesertedIslandHouseQuest : Quest
{
    public DesertedIslandHouseQuest()
    {
        title = "Deserted Island House";
        description = "Use the collected resources to build a sturdy shelter that meets the material checklist for a permanent base.";

        prerequisiteQuest = null; // Will be set to WarmNightQuest in Editor

        stages = new List<QuestStage>
        {
            new QuestStage
            {
                stageName = "Lay Foundation",
                stageDescription = "Lay the first foundation for your house.",
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        type = ObjectiveType.Build,
                        description = "Lay the house foundation",
                        amount = 1,
                        currentProgress = 0,
                        isCompleted = false
                    }
                }
            },
            new QuestStage
            {
                stageName = "Gather Materials",
                stageDescription = "Gather necessary materials for construction.",
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        type = ObjectiveType.Gather,
                        description = "Collect 100 Wood",
                        item = null, // Set in Editor
                        amount = 100,
                        currentProgress = 0,
                        isCompleted = false
                    },
                    new QuestObjective
                    {
                        type = ObjectiveType.Gather,
                        description = "Collect 50 Stone",
                        item = null, // Set in Editor
                        amount = 50,
                        currentProgress = 0,
                        isCompleted = false
                    }
                }
            },
            new QuestStage
            {
                stageName = "Build House",
                stageDescription = "Complete the construction of your house.",
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        type = ObjectiveType.Build,
                        description = "Finish building the house",
                        amount = 1,
                        currentProgress = 0,
                        isCompleted = false
                    }
                }
            }
        };

        reward = new QuestReward
        {
            itemReward = null, // Will be set to Ship Blueprint in Editor
            itemAmount = 1
        };
    }
}
