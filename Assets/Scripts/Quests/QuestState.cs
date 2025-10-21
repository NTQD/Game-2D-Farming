using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Runtime state wrapper for quest definitions. ScriptableObjects remain immutable while the
/// QuestManager tracks progress through these data containers.
/// </summary>
public class QuestState
{
    public Quest Quest { get; }
    public QuestStatus Status { get; internal set; }
    public string FailureReason { get; internal set; }

    public IReadOnlyList<QuestObjectiveState> Objectives => objectives;
    public int StartDay { get; }
    public int CurrentDay { get; private set; }

    private readonly List<QuestObjectiveState> objectives;

    public QuestState(Quest quest, int currentDay)
    {
        Quest = quest;
        Status = QuestStatus.Active;
        StartDay = currentDay;
        CurrentDay = currentDay;
        objectives = quest.objectives.Select(o => new QuestObjectiveState(o)).ToList();
    }

    public bool HasTimeLimit => Quest != null && Quest.timeLimitInDays > 0;

    public int DaysRemaining
    {
        get
        {
            if (!HasTimeLimit)
            {
                return -1;
            }

            int remaining = Quest.timeLimitInDays - (CurrentDay - StartDay);
            return Mathf.Max(0, remaining);
        }
    }

    public void UpdateCurrentDay(int day)
    {
        CurrentDay = day;
    }
}

public class QuestObjectiveState
{
    public QuestObjective Definition { get; }
    public int CurrentAmount { get; private set; }
    public int ConsecutiveSuccess { get; private set; }
    public int ConsecutiveFailure { get; private set; }
    public bool IsCompleted { get; private set; }

    public int Requirement => Definition != null ? Mathf.Max(1, Definition.amount) : 1;
    public int ConsecutiveSuccessTarget => Definition != null && Definition.consecutiveSuccessTarget > 0
        ? Definition.consecutiveSuccessTarget
        : Requirement;

    public int FailureThreshold => Definition != null ? Definition.consecutiveFailureLimit : 0;

    public QuestObjectiveState(QuestObjective definition)
    {
        Definition = definition;
        CurrentAmount = 0;
        ConsecutiveSuccess = 0;
        ConsecutiveFailure = 0;
        IsCompleted = false;
    }

    public void AddProgress(int value)
    {
        if (IsCompleted)
        {
            return;
        }

        CurrentAmount = Mathf.Clamp(CurrentAmount + value, 0, Requirement);
        TryComplete();
    }

    public void SetAbsoluteProgress(int value)
    {
        if (IsCompleted)
        {
            return;
        }

        CurrentAmount = Mathf.Clamp(value, 0, Requirement);
        TryComplete();
    }

    public void RegisterSuccessStreak()
    {
        if (IsCompleted)
        {
            return;
        }

        ConsecutiveSuccess = Mathf.Min(ConsecutiveSuccess + 1, ConsecutiveSuccessTarget);
        ConsecutiveFailure = 0;
        CurrentAmount = Mathf.Min(ConsecutiveSuccess, Requirement);
        TryComplete();
    }

    public void RegisterFailureStreak()
    {
        if (IsCompleted)
        {
            return;
        }

        ConsecutiveSuccess = 0;
        ConsecutiveFailure += 1;
        CurrentAmount = 0;
    }

    public void MarkCompleted()
    {
        IsCompleted = true;
        CurrentAmount = Requirement;
        ConsecutiveSuccess = Mathf.Max(ConsecutiveSuccess, ConsecutiveSuccessTarget);
    }

    private void TryComplete()
    {
        if (CurrentAmount >= Requirement)
        {
            MarkCompleted();
        }
    }

    public string GetProgressSummary()
    {
        if (Definition == null)
        {
            return string.Empty;
        }

        if (Definition.type == ObjectiveType.MaintainCampfire)
        {
            return $"{Definition.description} ({ConsecutiveSuccess}/{ConsecutiveSuccessTarget} nights)";
        }

        if (Definition.amount > 0)
        {
            return $"{Definition.description} ({CurrentAmount}/{Requirement})";
        }

        return IsCompleted ? $"{Definition.description} (Completed)" : Definition.description;
    }
}

public enum QuestStatus
{
    Locked,
    Available,
    Active,
    Completed,
    Failed
}
