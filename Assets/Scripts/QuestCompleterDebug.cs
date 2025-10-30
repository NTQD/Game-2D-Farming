using UnityEngine;
using System.Collections.Generic;

public class QuestCompleterDebug : MonoBehaviour
{
    public QuestGiver questGiver; // Assign the QuestGiver in the Inspector

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12)) // Press F12 to complete all quests
        {
            CompleteAllQuests();
        }
    }

    public void CompleteAllQuests()
    {
        if (questGiver == null)
        {
            Debug.LogError("QuestCompleterDebug: QuestGiver not assigned!");
            return;
        }

        if (QuestManager.instance == null)
        {
            Debug.LogError("QuestCompleterDebug: QuestManager.instance is null. Make sure QuestManager is in the scene and initialized.");
            return;
        }

        List<Quest> allQuests = questGiver.GetAllQuests();
        if (allQuests == null || allQuests.Count == 0)
        {
            Debug.LogWarning("QuestCompleterDebug: No quests found in QuestGiver.");
            return;
        }

        foreach (Quest quest in allQuests)
        {
            if (!quest.isCompleted)
            {
                // Mark quest as completed
                quest.isCompleted = true;
                // Also mark all stages and objectives as completed
                foreach (QuestStage stage in quest.stages)
                {
                    stage.isStageCompleted = true;
                    foreach (QuestObjective objective in stage.objectives)
                    {
                        objective.isCompleted = true;
                        objective.currentProgress = objective.amount; // Set progress to max
                    }
                }
                Debug.Log($"Debug: Quest '{quest.title}' marked as completed and QuestManager notified.");

                // Notify QuestManager to process completion and rewards
                QuestManager.instance.CompleteQuest(quest);
            }
            else
            {
                Debug.Log($"Debug: Quest '{quest.title}' was already completed.");
            }
        }
        Debug.Log("Debug: Attempted to complete all quests for testing purposes.");
    }
}
