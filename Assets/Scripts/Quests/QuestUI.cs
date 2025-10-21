
using UnityEngine;
using TMPro;
using System.Text;

public class QuestUI : MonoBehaviour
{
    public GameObject questPanel;
    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI questDescription;
    public TextMeshProUGUI questObjectives; // Add this line

    private Quest currentQuest;

    void Start()
    {
        questPanel.SetActive(false);
        QuestManager.instance.OnQuestStarted += ShowQuest;
        QuestManager.instance.OnQuestCompleted += HideQuest;
        QuestManager.instance.OnQuestUpdated += UpdateQuestUI;

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
    }

    void ShowQuest(Quest quest)
    {
        currentQuest = quest;
        questPanel.SetActive(true);
        questTitle.text = quest.title;
        UpdateQuestUI(quest);
    }

    void HideQuest(Quest quest)
    {
        currentQuest = null;
        questPanel.SetActive(false);
    }

    void UpdateQuestUI(Quest quest)
    {
        if (quest == currentQuest)
        {
            questDescription.text = quest.description;

            StringBuilder objectivesText = new StringBuilder();
            foreach (var objective in quest.objectives)
            {
                objectivesText.Append(objective.description);
                if (objective.isCompleted)
                {
                    objectivesText.Append(" (Completed)");
                }
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
}
