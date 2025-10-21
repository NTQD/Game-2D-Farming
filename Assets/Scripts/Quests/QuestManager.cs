
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    public List<Quest> activeQuests = new List<Quest>();

    // Events for UI updates
    public System.Action<Quest> OnQuestStarted;
    public System.Action<Quest> OnQuestCompleted;
    public System.Action<Quest> OnQuestUpdated;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            // Reset objectives
            foreach (var objective in quest.objectives)
            {
                objective.isCompleted = false;
            }
            OnQuestStarted?.Invoke(quest);
            Debug.Log("Quest started: " + quest.title);
        }
    }

    public void CheckGatherObjective(Item item, int amount)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.type == ObjectiveType.Gather && !objective.isCompleted && objective.item == item)
                {
                    // This is a simplified check. A real implementation would check the inventory total.
                    // For now, we assume any pickup contributes.
                    // We will refine this later.
                    if (GameManager.instance.inventoryContainer.GetItemCount(item) >= objective.amount)
                    {
                        objective.isCompleted = true;
                        OnQuestUpdated?.Invoke(quest);
                        CheckQuestCompletion(quest);
                    }
                }
            }
        }
    }

    public void CheckBuildObjective(int amount)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.type == ObjectiveType.Build && !objective.isCompleted)
                {
                    // For build objectives, we assume each call to this method signifies one unit of progress.
                    // You might need more complex logic here depending on how 'building' is defined.
                    objective.amount -= amount; // Decrementing the required amount
                    if (objective.amount <= 0)
                    {
                        objective.isCompleted = true;
                        OnQuestUpdated?.Invoke(quest);
                        CheckQuestCompletion(quest);
                    }
                }
            }
        }
    }

    public void CheckPlantObjective(Item item, int amount)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.type == ObjectiveType.Plant && !objective.isCompleted && objective.item == item)
                {
                    objective.amount -= amount; // Decrementing the required amount
                    if (objective.amount <= 0)
                    {
                        objective.isCompleted = true;
                        OnQuestUpdated?.Invoke(quest);
                        CheckQuestCompletion(quest);
                    }
                }
            }
        }
    }

    private void CheckQuestCompletion(Quest quest)
    {
        if (quest.objectives.All(o => o.isCompleted))
        {
            CompleteQuest(quest);
        }
    }

    private void CompleteQuest(Quest quest)
    {
        activeQuests.Remove(quest);
        OnQuestCompleted?.Invoke(quest);
        Debug.Log("Quest completed: " + quest.title);

        // Grant reward
        if (quest.reward != null && quest.reward.itemReward != null)
        { 
            GameManager.instance.inventoryContainer.Add(quest.reward.itemReward, quest.reward.itemAmount);
            Debug.Log("Rewarded " + quest.reward.itemAmount + " " + quest.reward.itemReward.Name);
        }
    }
}
