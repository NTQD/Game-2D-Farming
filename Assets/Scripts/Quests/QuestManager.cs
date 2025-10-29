
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

    public GameObject boatPrefab; // New: Reference to the boat prefab to instantiate upon quest completion

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

    private void OnEnable()
    {
        if (CampfireManager.instance != null)
        {
            CampfireManager.instance.OnCampfireStateChanged += HandleCampfireStateChanged;
        }
    }

    private void OnDisable()
    {
        if (CampfireManager.instance != null)
        {
            CampfireManager.instance.OnCampfireStateChanged -= HandleCampfireStateChanged;
        }
    }

    private void HandleCampfireStateChanged()
    {
        Quest warmNightQuest = activeQuests.FirstOrDefault(q => q.title == "Warm Night");
        if (warmNightQuest != null && warmNightQuest.stages.Count > 0 && warmNightQuest.currentStageIndex < warmNightQuest.stages.Count)
        {
            QuestStage currentStage = warmNightQuest.stages[warmNightQuest.currentStageIndex];
            // Check objective for maintaining campfire
            QuestObjective campfireObjective = currentStage.objectives.FirstOrDefault(o => o.description.Contains("Maintain the main camp's red fire"));
            if (campfireObjective != null && !campfireObjective.isCompleted)
            {
                campfireObjective.currentProgress = CampfireManager.instance.GetConsecutiveNightsLit();
                if (campfireObjective.currentProgress >= campfireObjective.amount)
                {
                    campfireObjective.isCompleted = true;
                    OnQuestUpdated?.Invoke(warmNightQuest);
                    CheckQuestCompletion(warmNightQuest);
                }
            }

            // Check for failure condition
            if (CampfireManager.instance.GetConsecutiveNightsUnlit() > 2) // More than two consecutive nights unlit
            {
                // Implement quest failure logic here
                Debug.Log("Warm Night quest failed: Campfire unlit for too long!");
                // You might want to remove the quest, reset its state, or trigger a specific failure event.
                // For now, let's just mark it as failed and remove it from active quests.
                warmNightQuest.isCompleted = true; // Mark as completed (failed)
                activeQuests.Remove(warmNightQuest);
                OnQuestCompleted?.Invoke(warmNightQuest); // Invoke completion event even for failure
            }
        }
    }

    public void StartQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            quest.currentStageIndex = 0; // Initialize current stage
            // Reset objectives for the first stage
            if (quest.stages.Count > 0)
            {
                foreach (var objective in quest.stages[0].objectives)
                {
                    objective.isCompleted = false;
                    objective.currentProgress = 0; // Initialize currentProgress
                }
                quest.stages[0].isStageCompleted = false;
            }
            OnQuestStarted?.Invoke(quest);
            Debug.Log("Quest started: " + quest.title);
        }
    }

    public void CheckGatherObjective(Item item, int amount)
    {
        foreach (var quest in activeQuests)
        {
            if (quest.stages.Count > 0 && quest.currentStageIndex < quest.stages.Count)
            {
                QuestStage currentStage = quest.stages[quest.currentStageIndex];
                foreach (var objective in currentStage.objectives)
                {
                    if (objective.type == ObjectiveType.Gather && !objective.isCompleted && objective.item == item)
                    {
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
    }

    public void CheckBuildObjective(int amount)
    {
        foreach (var quest in activeQuests)
        {
            if (quest.stages.Count > 0 && quest.currentStageIndex < quest.stages.Count)
            {
                QuestStage currentStage = quest.stages[quest.currentStageIndex];
                foreach (var objective in currentStage.objectives)
                {
                                    if (objective.type == ObjectiveType.Build && !objective.isCompleted)
                                    {
                                        objective.currentProgress += amount; // Increment currentProgress
                                        if (objective.currentProgress >= objective.amount)
                                        {
                                            objective.isCompleted = true;
                                            OnQuestUpdated?.Invoke(quest);
                                            CheckQuestCompletion(quest);
                                        }
                                    }                }
            }
        }
    }

    public void CheckPlantObjective(Item item, int amount)
    {
        foreach (var quest in activeQuests)
        {
            if (quest.stages.Count > 0 && quest.currentStageIndex < quest.stages.Count)
            {
                QuestStage currentStage = quest.stages[quest.currentStageIndex];
                foreach (var objective in currentStage.objectives)
                {
                                    if (objective.type == ObjectiveType.Plant && !objective.isCompleted && objective.item == item)
                                    {
                                        objective.currentProgress += amount; // Increment currentProgress
                                        if (objective.currentProgress >= objective.amount)
                                        {
                                            objective.isCompleted = true;
                                            OnQuestUpdated?.Invoke(quest);
                                            CheckQuestCompletion(quest);
                                        }
                                    }                }
            }
        }
    }

    private void CheckQuestCompletion(Quest quest)
    {
        if (quest.stages.Count > 0 && quest.currentStageIndex < quest.stages.Count)
        {
            QuestStage currentStage = quest.stages[quest.currentStageIndex];
            if (currentStage.objectives.All(o => o.isCompleted))
            {
                currentStage.isStageCompleted = true;
                quest.currentStageIndex++;
                OnQuestUpdated?.Invoke(quest);

                if (quest.currentStageIndex >= quest.stages.Count)
                {
                    CompleteQuest(quest);
                }
                else
                {
                    // Reset objectives for the new current stage
                    foreach (var objective in quest.stages[quest.currentStageIndex].objectives)
                    {
                        objective.isCompleted = false;
                    }
                    quest.stages[quest.currentStageIndex].isStageCompleted = false;
                    Debug.Log($"Quest {quest.title}: Advanced to stage {quest.currentStageIndex + 1}");
                }
            }
        }
    }

    private void CompleteQuest(Quest quest)
    {
        activeQuests.Remove(quest);
        quest.isCompleted = true; // Mark the quest as completed
        OnQuestCompleted?.Invoke(quest);
        Debug.Log("Quest completed: " + quest.title);

        // Grant reward
        if (quest.reward != null && quest.reward.itemReward != null)
        { 
            GameManager.instance.inventoryContainer.Add(quest.reward.itemReward, quest.reward.itemAmount);
            Debug.Log("Rewarded " + quest.reward.itemAmount + " " + quest.reward.itemReward.Name);
        }

        // Special handling for Ship of Hope Quest completion
        if (quest.title == "Ship of Hope" && boatPrefab != null)
        {
            GameObject boatInstance = Instantiate(boatPrefab, new Vector3(0, 0, 0), Quaternion.identity); // Adjust spawn position as needed
            BoatController boatController = boatInstance.GetComponent<BoatController>();
            if (boatController != null)
            {
                // Assuming a shore position, e.g., (10, 0, 0)
                boatController.AppearOnShore(new Vector3(10, 0, 0)); // Placeholder position
            }
        }
    }

    public int GetCurrentObjectiveProgress(QuestObjective objective)
    {
        switch (objective.type)
        {
            case ObjectiveType.Gather:
                if (objective.item != null && GameManager.instance != null && GameManager.instance.inventoryContainer != null)
                {
                    return GameManager.instance.inventoryContainer.GetItemCount(objective.item);
                }
                break;
            case ObjectiveType.Build:
            case ObjectiveType.Plant:
                return objective.currentProgress;
            case ObjectiveType.Explore:
            case ObjectiveType.Trade:
                // For these types, currentProgress should be updated directly by specific game events.
                // For now, return currentProgress.
                return objective.currentProgress;
        }
        return 0; // Default or unknown objective type
    }
}
