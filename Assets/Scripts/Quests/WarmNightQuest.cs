using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Warm Night Quest", menuName = "Quests/Warm Night")]
public class WarmNightQuest : Quest
{
    public WarmNightQuest()
    {
        title = "Warm Night";
        description = "Explore the island to chop enough wood for the night campfire and stock up on materials needed for future construction.";

        stages = new List<QuestStage>
        {
            new QuestStage
            {
                stageName = "Gather Wood",
                stageDescription = "Chop and store 50 bundles of dry wood.",
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        type = ObjectiveType.Gather,
                        description = "Collect 50 Dry Wood",
                        item = null, // This will be set in the Unity Editor
                        amount = 50,
                        isCompleted = false
                    }
                }
            },
            new QuestStage
            {
                stageName = "Maintain Campfire",
                stageDescription = "Maintain the main camp's red fire for three consecutive nights.",
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        type = ObjectiveType.Build, // Using Build type for campfire state tracking
                        description = "Maintain the main camp's red fire for 3 consecutive nights",
                        item = null, // Not directly tied to an item for this objective
                        amount = 3, // Represents 3 consecutive nights
                        isCompleted = false
                    }
                }
            }
        };

        reward = new QuestReward
        {
            itemReward = null, // This will be set in the Unity Editor (House Blueprint)
            itemAmount = 1
        };
    }
}
