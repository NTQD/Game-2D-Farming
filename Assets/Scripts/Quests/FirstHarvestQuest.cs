using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "First Harvest Quest", menuName = "Quests/First Harvest")]
public class FirstHarvestQuest : Quest
{
    public FirstHarvestQuest()
    {
        title = "First Harvest";
        description = "Plow the land, sow starter seeds and successfully harvest the necessary crops to ensure a stable food supply.";

        stages = new List<QuestStage>
        {
            new QuestStage
            {
                stageName = "Plow the Land",
                stageDescription = "Plow 5 tiles of land.",
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        type = ObjectiveType.Build,
                        description = "Plow 5 tiles",
                        amount = 5,
                        isCompleted = false
                    }
                }
            },
            new QuestStage
            {
                stageName = "Sow Seeds",
                stageDescription = "Sow 5 starter seeds.",
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        type = ObjectiveType.Plant,
                        description = "Plant 5 seeds",
                        item = null, // This will be set in the Unity Editor
                        amount = 5,
                        isCompleted = false
                    }
                }
            },
            new QuestStage
            {
                stageName = "Harvest Crops",
                stageDescription = "Harvest 20 units of crops.",
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        type = ObjectiveType.Gather,
                        description = "Harvest 20 crops",
                        item = null, // This will be set in the Unity Editor
                        amount = 20,
                        isCompleted = false
                    }
                }
            }
        };

        reward = new QuestReward
        {
            itemReward = null, // This will be set in the Unity Editor
            itemAmount = 30 // Gives each crop 30 units of agricultural products.
        };
    }
}
