
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    public event Action<QuestState> OnQuestStarted;
    public event Action<QuestState> OnQuestCompleted;
    public event Action<QuestState> OnQuestUpdated;
    public event Action<QuestState> OnQuestFailed;

    private readonly List<QuestState> activeQuests = new List<QuestState>();
    private readonly Dictionary<Quest, QuestState> questLookup = new Dictionary<Quest, QuestState>();
    private readonly HashSet<Quest> completedQuests = new HashSet<Quest>();
    private readonly HashSet<Quest> failedQuests = new HashSet<Quest>();

    private int currentDay;

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
        DayTimeController.OnDayChanged += HandleDayChanged;
        currentDay = DayTimeController.Instance != null ? DayTimeController.Instance.day : 0;
    }

    private void OnDisable()
    {
        DayTimeController.OnDayChanged -= HandleDayChanged;
    }

    public IReadOnlyList<QuestState> ActiveQuests => activeQuests;

    public int CurrentDay => currentDay;

    public bool StartQuest(Quest quest)
    {
        if (quest == null)
        {
            Debug.LogWarning("QuestManager: Attempted to start a null quest.");
            return false;
        }

        if (completedQuests.Contains(quest))
        {
            Debug.LogWarning($"QuestManager: Quest '{quest.title}' has already been completed.");
            return false;
        }

        if (!ArePrerequisitesCompleted(quest))
        {
            Debug.Log($"QuestManager: Quest '{quest.title}' prerequisites not met.");
            return false;
        }

        if (questLookup.TryGetValue(quest, out QuestState existingState) && existingState.Status == QuestStatus.Active)
        {
            return false;
        }

        failedQuests.Remove(quest);

        QuestState questState = new QuestState(quest, currentDay);
        questLookup[quest] = questState;
        activeQuests.Add(questState);

        OnQuestStarted?.Invoke(questState);
        OnQuestUpdated?.Invoke(questState);
        Debug.Log("Quest started: " + quest.title);
        return true;
    }

    public QuestStatus GetQuestStatus(Quest quest)
    {
        if (quest == null)
        {
            return QuestStatus.Locked;
        }

        if (completedQuests.Contains(quest))
        {
            return QuestStatus.Completed;
        }

        if (failedQuests.Contains(quest))
        {
            return QuestStatus.Failed;
        }

        if (questLookup.TryGetValue(quest, out QuestState state))
        {
            return state.Status;
        }

        return ArePrerequisitesCompleted(quest) ? QuestStatus.Available : QuestStatus.Locked;
    }

    public QuestState GetQuestState(Quest quest)
    {
        questLookup.TryGetValue(quest, out QuestState state);
        return state;
    }

    public void CheckGatherObjective(Item item, int amount)
    {
        foreach (QuestState questState in activeQuests)
        {
            foreach (QuestObjectiveState objectiveState in questState.Objectives)
            {
                if (objectiveState.Definition.type == ObjectiveType.Gather &&
                    !objectiveState.IsCompleted &&
                    objectiveState.Definition.item == item)
                {
                    if (objectiveState.Definition.useInventoryTotal)
                    {
                        int totalAmount = amount;
                        if (GameManager.instance != null && GameManager.instance.inventoryContainer != null)
                        {
                            totalAmount = GameManager.instance.inventoryContainer.GetItemCount(item);
                        }

                        ApplyAbsoluteProgress(questState, objectiveState, totalAmount);
                    }
                    else
                    {
                        ApplyIncrementalProgress(questState, objectiveState, amount);
                    }
                }
            }
        }
    }

    public void CheckBuildObjective(int amount, string structureId = null)
    {
        foreach (QuestState questState in activeQuests)
        {
            foreach (QuestObjectiveState objectiveState in questState.Objectives)
            {
                if ((objectiveState.Definition.type == ObjectiveType.Build || objectiveState.Definition.type == ObjectiveType.ConstructStructure) &&
                    !objectiveState.IsCompleted)
                {
                    if (string.IsNullOrEmpty(objectiveState.Definition.targetId) || objectiveState.Definition.targetId == structureId)
                    {
                        ApplyIncrementalProgress(questState, objectiveState, amount);
                    }
                }
            }
        }
    }

    public void RegisterStructureProgress(string structureId, int amount = 1)
    {
        CheckBuildObjective(amount, structureId);
    }

    public void CheckPlantObjective(Item item, int amount)
    {
        foreach (QuestState questState in activeQuests)
        {
            foreach (QuestObjectiveState objectiveState in questState.Objectives)
            {
                if (objectiveState.Definition.type == ObjectiveType.Plant &&
                    !objectiveState.IsCompleted &&
                    objectiveState.Definition.item == item)
                {
                    ApplyIncrementalProgress(questState, objectiveState, amount);
                }
            }
        }
    }

    public void RegisterBlueprintAcquired(string blueprintId)
    {
        foreach (QuestState questState in activeQuests)
        {
            foreach (QuestObjectiveState objectiveState in questState.Objectives)
            {
                if (objectiveState.Definition.type == ObjectiveType.AcquireBlueprint &&
                    !objectiveState.IsCompleted &&
                    objectiveState.Definition.targetId == blueprintId)
                {
                    CompleteObjective(questState, objectiveState);
                    break;
                }
            }
        }
    }

    public void RegisterTrade(string tradeId)
    {
        foreach (QuestState questState in activeQuests)
        {
            foreach (QuestObjectiveState objectiveState in questState.Objectives)
            {
                if (objectiveState.Definition.type == ObjectiveType.Trade &&
                    !objectiveState.IsCompleted &&
                    (string.IsNullOrEmpty(objectiveState.Definition.targetId) || objectiveState.Definition.targetId == tradeId))
                {
                    CompleteObjective(questState, objectiveState);
                    break;
                }
            }
        }
    }

    public void RegisterExplorationPoint(string pointId)
    {
        foreach (QuestState questState in activeQuests)
        {
            foreach (QuestObjectiveState objectiveState in questState.Objectives)
            {
                if (objectiveState.Definition.type == ObjectiveType.Explore &&
                    !objectiveState.IsCompleted &&
                    (string.IsNullOrEmpty(objectiveState.Definition.targetId) || objectiveState.Definition.targetId == pointId))
                {
                    CompleteObjective(questState, objectiveState);
                    break;
                }
            }
        }
    }

    public void RegisterInteraction(string interactionId)
    {
        foreach (QuestState questState in activeQuests)
        {
            foreach (QuestObjectiveState objectiveState in questState.Objectives)
            {
                if (objectiveState.Definition.type == ObjectiveType.Interact &&
                    !objectiveState.IsCompleted &&
                    (string.IsNullOrEmpty(objectiveState.Definition.targetId) || objectiveState.Definition.targetId == interactionId))
                {
                    CompleteObjective(questState, objectiveState);
                    break;
                }
            }
        }
    }

    public void RegisterStockpile(Item item, int totalAmount)
    {
        foreach (QuestState questState in activeQuests)
        {
            foreach (QuestObjectiveState objectiveState in questState.Objectives)
            {
                if (objectiveState.Definition.type == ObjectiveType.Stockpile &&
                    !objectiveState.IsCompleted &&
                    objectiveState.Definition.item == item)
                {
                    ApplyAbsoluteProgress(questState, objectiveState, totalAmount);
                    break;
                }
            }
        }
    }

    public void RegisterBoatAssembled(string boatId = null)
    {
        foreach (QuestState questState in activeQuests)
        {
            foreach (QuestObjectiveState objectiveState in questState.Objectives)
            {
                if (objectiveState.Definition.type == ObjectiveType.AssembleBoat &&
                    !objectiveState.IsCompleted &&
                    (string.IsNullOrEmpty(objectiveState.Definition.targetId) || objectiveState.Definition.targetId == boatId))
                {
                    CompleteObjective(questState, objectiveState);
                    break;
                }
            }
        }
    }

    public void ReportCampfireNight(bool campfireWasLit)
    {
        foreach (QuestState questState in activeQuests.ToList())
        {
            foreach (QuestObjectiveState objectiveState in questState.Objectives)
            {
                if (objectiveState.Definition.type != ObjectiveType.MaintainCampfire || objectiveState.IsCompleted)
                {
                    continue;
                }

                int previousSuccess = objectiveState.ConsecutiveSuccess;
                int previousFailure = objectiveState.ConsecutiveFailure;

                if (campfireWasLit)
                {
                    objectiveState.RegisterSuccessStreak();
                }
                else
                {
                    objectiveState.RegisterFailureStreak();
                }

                bool streakChanged = previousSuccess != objectiveState.ConsecutiveSuccess || previousFailure != objectiveState.ConsecutiveFailure;

                if (!campfireWasLit && objectiveState.FailureThreshold > 0 && objectiveState.ConsecutiveFailure >= objectiveState.FailureThreshold)
                {
                    FailQuest(questState, !string.IsNullOrEmpty(questState.Quest.failureDescription)
                        ? questState.Quest.failureDescription
                        : "You let the fire go out for too long.");
                    break;
                }

                if (streakChanged)
                {
                    OnQuestUpdated?.Invoke(questState);
                }

                if (objectiveState.IsCompleted)
                {
                    CheckQuestCompletion(questState);
                }
            }
        }
    }

    private bool ArePrerequisitesCompleted(Quest quest)
    {
        if (quest.prerequisites == null || quest.prerequisites.Count == 0)
        {
            return true;
        }

        foreach (Quest prerequisite in quest.prerequisites)
        {
            if (prerequisite == null)
            {
                continue;
            }

            if (!completedQuests.Contains(prerequisite))
            {
                return false;
            }
        }

        return true;
    }

    private void HandleDayChanged(int newDay)
    {
        currentDay = newDay;

        foreach (QuestState questState in activeQuests.ToList())
        {
            questState.UpdateCurrentDay(newDay);

            if (questState.HasTimeLimit && (newDay - questState.StartDay) > questState.Quest.timeLimitInDays)
            {
                FailQuest(questState, !string.IsNullOrEmpty(questState.Quest.failureDescription)
                    ? questState.Quest.failureDescription
                    : "The time limit for this quest has expired.");
            }
        }
    }

    private void ApplyIncrementalProgress(QuestState questState, QuestObjectiveState objectiveState, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int previousAmount = objectiveState.CurrentAmount;
        bool wasCompleted = objectiveState.IsCompleted;

        objectiveState.AddProgress(amount);

        HandleObjectiveChange(questState, objectiveState, previousAmount, wasCompleted);
    }

    private void ApplyAbsoluteProgress(QuestState questState, QuestObjectiveState objectiveState, int amount)
    {
        int previousAmount = objectiveState.CurrentAmount;
        bool wasCompleted = objectiveState.IsCompleted;

        objectiveState.SetAbsoluteProgress(amount);

        HandleObjectiveChange(questState, objectiveState, previousAmount, wasCompleted);
    }

    private void CompleteObjective(QuestState questState, QuestObjectiveState objectiveState)
    {
        if (objectiveState.IsCompleted)
        {
            return;
        }

        objectiveState.MarkCompleted();
        OnQuestUpdated?.Invoke(questState);
        CheckQuestCompletion(questState);
    }

    private void HandleObjectiveChange(QuestState questState, QuestObjectiveState objectiveState, int previousAmount, bool wasCompleted)
    {
        if (previousAmount != objectiveState.CurrentAmount || wasCompleted != objectiveState.IsCompleted)
        {
            OnQuestUpdated?.Invoke(questState);
        }

        if (!wasCompleted && objectiveState.IsCompleted)
        {
            CheckQuestCompletion(questState);
        }
    }

    private void CheckQuestCompletion(QuestState questState)
    {
        if (questState.Objectives.All(o => o.IsCompleted))
        {
            CompleteQuest(questState);
        }
    }

    private void CompleteQuest(QuestState questState)
    {
        questState.Status = QuestStatus.Completed;
        activeQuests.Remove(questState);
        completedQuests.Add(questState.Quest);

        GrantReward(questState.Quest);

        OnQuestCompleted?.Invoke(questState);
        Debug.Log("Quest completed: " + questState.Quest.title);
    }

    private void GrantReward(Quest quest)
    {
        if (quest.reward != null && quest.reward.itemReward != null && GameManager.instance != null)
        {
            GameManager.instance.inventoryContainer.Add(quest.reward.itemReward, quest.reward.itemAmount);
            Debug.Log($"Rewarded {quest.reward.itemAmount} {quest.reward.itemReward.Name}");
        }
    }

    private void FailQuest(QuestState questState, string reason)
    {
        questState.Status = QuestStatus.Failed;
        questState.FailureReason = reason;
        activeQuests.Remove(questState);
        failedQuests.Add(questState.Quest);

        OnQuestFailed?.Invoke(questState);
        Debug.LogWarning($"Quest failed: {questState.Quest.title} - {reason}");
    }
}
