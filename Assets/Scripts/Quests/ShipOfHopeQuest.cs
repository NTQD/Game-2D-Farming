using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ship of Hope Quest", menuName = "Quests/Ship of Hope")]
public class ShipOfHopeQuest : Quest
{
    public ShipOfHopeQuest()
    {
        title = "Ship of Hope";
        description = "Build a seaworthy boat and prepare enough food to brave the journey back to civilization.";

        prerequisiteQuest = null; // Will be set to DesertedIslandHouseQuest in Editor

        stages = new List<QuestStage>
        {
            new QuestStage
            {
                stageName = "Obtain Ship Blueprint",
                stageDescription = "Trade for the Ship Blueprint.",
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        type = ObjectiveType.Trade,
                        description = "Acquire Ship Blueprint",
                        item = null, // Set in Editor
                        amount = 1,
                        currentProgress = 0,
                        isCompleted = false
                    }
                }
            },
            new QuestStage
            {
                stageName = "Gather Ship Materials",
                stageDescription = "Gather necessary materials for boat construction.",
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        type = ObjectiveType.Gather,
                        description = "Collect 200 Special Wood",
                        item = null, // Set in Editor
                        amount = 200,
                        currentProgress = 0,
                        isCompleted = false
                    },
                    new QuestObjective
                    {
                        type = ObjectiveType.Gather,
                        description = "Collect 100 Metal Parts",
                        item = null, // Set in Editor
                        amount = 100,
                        currentProgress = 0,
                        isCompleted = false
                    }
                }
            },
            new QuestStage
            {
                stageName = "Build Boat",
                stageDescription = "Assemble the boat to seaworthy standards.",
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        type = ObjectiveType.Build,
                        description = "Assemble the boat",
                        amount = 1,
                        currentProgress = 0,
                        isCompleted = false
                    }
                }
            },
            new QuestStage
            {
                stageName = "Prepare for Journey",
                stageDescription = "Store enough food for the journey.",
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        type = ObjectiveType.Gather,
                        description = "Store 50 units of Food",
                        item = null, // Set in Editor
                        amount = 50,
                        currentProgress = 0,
                        isCompleted = false
                    }
                }
            }
        };

        // No physical item reward, game completion is the reward.
        reward = new QuestReward
        {
            itemReward = null,
            itemAmount = 0
        };
    }
}
