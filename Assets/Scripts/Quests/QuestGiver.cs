
using UnityEngine;
using System.Collections.Generic; // Still needed for List in GetAvailableQuests()

public class QuestGiver : MonoBehaviour
{
    public Quest[] quests; // Changed from List<Quest> to Quest[]

    // This method will be called by UI_ShopController to get quests that can be offered
    public List<Quest> GetAvailableQuests()
    {
        List<Quest> availableQuests = new List<Quest>();
        foreach (Quest q in quests)
        {
            // Check if the quest is not already active and not completed
            // And check if its prerequisite quest is null or completed
            if (!QuestManager.instance.activeQuests.Contains(q) && !q.isCompleted && (q.prerequisiteQuest == null || q.prerequisiteQuest.isCompleted))
            {
                availableQuests.Add(q);
            }
        }
        return availableQuests;
    }

    // This method will be called by UI_ShopController to start a specific quest
    public void StartQuest(Quest questToStart)
    {
        if (!QuestManager.instance.activeQuests.Contains(questToStart)) // Check if the specific quest is not already active/started
        {
            QuestManager.instance.StartQuest(questToStart); // QuestManager will handle adding to activeQuests list
        }
    }

    // Removed the old Interact() and StartQuest() methods as they are replaced by the new logic.
}
