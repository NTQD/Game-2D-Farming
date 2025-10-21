
using UnityEngine;
using TMPro;
using System.Text;

public class QuestUI : MonoBehaviour
{
    public GameObject questPanel;
    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI questDescription;
    public TextMeshProUGUI questObjectives; // Add this line

    private QuestState currentQuest;

    void Start()
    {
        questPanel.SetActive(false);
        QuestManager.instance.OnQuestStarted += ShowQuest;
        QuestManager.instance.OnQuestCompleted += HideQuest;
        QuestManager.instance.OnQuestUpdated += UpdateQuestUI;
        QuestManager.instance.OnQuestFailed += ShowQuestFailure;

        if (questObjectives == null)
        {
            questObjectives = questPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (questObjectives == null)
            {
                Debug.LogError("QuestUI: Could not find the objectives TextMeshProUGUI component in the children of the quest panel.");
            }
        }
    }

    private void OnDestroy()
    {
        QuestManager.instance.OnQuestStarted -= ShowQuest;
        QuestManager.instance.OnQuestCompleted -= HideQuest;
        QuestManager.instance.OnQuestUpdated -= UpdateQuestUI; // Add this line
        QuestManager.instance.OnQuestFailed -= ShowQuestFailure;
    }

    void ShowQuest(QuestState quest)
    {
        currentQuest = quest;
        questPanel.SetActive(true);
        questTitle.text = quest.Quest.title;
        UpdateQuestUI(quest);
    }

    void HideQuest(QuestState quest)
    {
        if (quest == currentQuest)
        {
            currentQuest = null;
            questPanel.SetActive(false);
        }
    }

    void UpdateQuestUI(QuestState quest)
    {
        if (quest == currentQuest)
        {
            questTitle.text = quest.Status == QuestStatus.Failed
                ? $"{quest.Quest.title} (Failed)"
                : quest.Quest.title;

            questDescription.text = quest.Quest.description;

            StringBuilder objectivesText = new StringBuilder();
            foreach (var objective in quest.Objectives)
            {
                objectivesText.Append(objective.GetProgressSummary());
                if (objective.IsCompleted)
                {
                    objectivesText.Append(" (Completed)");
                }
                objectivesText.Append("\n");
            }

            if (quest.HasTimeLimit)
            {
                objectivesText.Append("Time remaining: ");
                objectivesText.Append(quest.DaysRemaining);
                objectivesText.Append(" day(s)\n");
            }

            if (quest.Status == QuestStatus.Failed && !string.IsNullOrEmpty(quest.FailureReason))
            {
                objectivesText.Append("Failure: ");
                objectivesText.Append(quest.FailureReason);
                objectivesText.Append("\n");
            }

            if (questObjectives != null)
            {
                questObjectives.text = objectivesText.ToString();
            }
            else
            {
                Debug.LogError("QuestUI: questObjectives TextMeshProUGUI is not assigned in the Inspector.");
            }
        }
    }

    void ShowQuestFailure(QuestState quest)
    {
        if (currentQuest == quest)
        {
            UpdateQuestUI(quest);
        }
        else
        {
            ShowQuest(quest);
        }
    }
}
