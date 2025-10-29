using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class QuestUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questPanel;
    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI questDescription;
    public TextMeshProUGUI questObjectives;
    public Button acceptButton;

    [Header("Quest References")]
    public Quest firstHarvestQuest;
    public Quest warmNightQuest;
    public Quest desertedIslandHouseQuest;
    public Quest shipOfHopeQuest;

    [SerializeField] private QuestGiver questGiver;

    private readonly List<Quest> availableQuests = new List<Quest>();
    private int currentQuestIndex = -1;
    private Quest currentlyDisplayedQuest;

    private void Start()
    {
        if (questPanel == null)
        {
            questPanel = gameObject;
            Debug.LogWarning("QuestUI: questPanel was not assigned. Defaulting to the GameObject this script is attached to.");
        }

        // questPanel.SetActive(false); // Removed: QuestPanel should be initially inactive in Editor
        QuestManager.instance.OnQuestStarted += OnQuestStartedHandler;
        QuestManager.instance.OnQuestCompleted += OnQuestCompletedHandler;
        QuestManager.instance.OnQuestUpdated += OnQuestUpdatedHandler;

        if (questObjectives == null)
        {
            questObjectives = questPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (questObjectives == null)
            {
                Debug.LogError("QuestUI: Could not find the objectives TextMeshProUGUI component in the children of the quest panel.");
            }
        }

        if (questGiver == null)
        {
            questGiver = FindObjectOfType<QuestGiver>();
            if (questGiver == null)
            {
                Debug.LogError("QuestUI: QuestGiver not found in the scene!");
            }
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.instance != null)
        {
            QuestManager.instance.OnQuestStarted -= OnQuestStartedHandler;
            QuestManager.instance.OnQuestCompleted -= OnQuestCompletedHandler;
            QuestManager.instance.OnQuestUpdated -= OnQuestUpdatedHandler;
        }
    }

    public void AcceptQuestOrAdvance()
    {
        if (currentlyDisplayedQuest == null)
        {
            return;
        }

        int index = availableQuests.IndexOf(currentlyDisplayedQuest);
        if (index != -1 && index < availableQuests.Count - 1)
        {
            ShowQuest(availableQuests[index + 1]);
        }
        else
        {
            questPanel.SetActive(false);
        }
    }

    public void StartCurrentlyDisplayedQuest()
    {
        if (currentlyDisplayedQuest == null || questGiver == null)
        {
            return;
        }

        if (!QuestManager.instance.activeQuests.Contains(currentlyDisplayedQuest) && !currentlyDisplayedQuest.isCompleted)
        {
            questGiver.StartQuest(currentlyDisplayedQuest);
            questPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Quest {currentlyDisplayedQuest.title} cannot be started. It's either active or completed.");
        }
    }

    public void ShowNextQuest()
    {
        if (availableQuests.Count <= 1)
        {
            return;
        }

        currentQuestIndex = (currentQuestIndex + 1) % availableQuests.Count;
        ShowQuest(availableQuests[currentQuestIndex]);
    }

    public void ShowPreviousQuest()
    {
        if (availableQuests.Count <= 1)
        {
            return;
        }

        currentQuestIndex--;
        if (currentQuestIndex < 0)
        {
            currentQuestIndex = availableQuests.Count - 1;
        }
        ShowQuest(availableQuests[currentQuestIndex]);
    }

    public void CancelQuestUI()
    {
        questPanel.SetActive(false);
        if (UI_ShopController.instance != null)
        {
            UI_ShopController.instance.ShowShopPanel();
        }
    }

    private void OnQuestStartedHandler(Quest quest)
    {
        questPanel.SetActive(false);
    }

    private void OnQuestCompletedHandler(Quest quest)
    {
        if (questPanel.activeInHierarchy && questGiver != null)
        {
            DisplayAvailableQuests(questGiver.GetAvailableQuests());
        }
    }

    private void OnQuestUpdatedHandler(Quest quest)
    {
        if (questPanel.activeInHierarchy && currentlyDisplayedQuest == quest)
        {
            ShowQuest(quest);
        }
    }

    private void Update()
    {
        if (!questPanel.activeInHierarchy || availableQuests.Count == 0)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ShowPreviousQuest();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ShowNextQuest();
        }
    }

    public void DisplayAvailableQuests(List<Quest> quests)
    {
        availableQuests.Clear();
        if (quests != null)
        {
            availableQuests.AddRange(quests);
        }

        if (availableQuests.Count == 0)
        {
            questPanel.SetActive(true);
            if (questTitle != null) questTitle.text = "No Quests Available";
            if (questDescription != null) questDescription.text = string.Empty;
            if (questObjectives != null) questObjectives.text = string.Empty;
            currentlyDisplayedQuest = null;
            currentQuestIndex = -1;
            return;
        }

        currentQuestIndex = Mathf.Clamp(currentQuestIndex, 0, availableQuests.Count - 1);
        ShowQuest(availableQuests[currentQuestIndex]);
    }

    public void ShowQuest(Quest quest)
    {
        if (quest == null)
        {
            questPanel.SetActive(false);
            currentlyDisplayedQuest = null;
            currentQuestIndex = -1;
            return;
        }

        currentlyDisplayedQuest = quest;
        currentQuestIndex = availableQuests.IndexOf(quest);

        if (questTitle != null) questTitle.text = quest.title;
        if (questDescription != null) questDescription.text = quest.description;

        UpdateQuestObjectivesText(quest);
        questPanel.SetActive(true);

        if (acceptButton != null)
        {
            acceptButton.interactable = !quest.isCompleted;
        }
    }

    private void UpdateQuestObjectivesText(Quest quest)
    {
        if (questObjectives == null)
        {
            Debug.LogError("QuestUI: questObjectives TextMeshProUGUI is not assigned in the Inspector.");
            return;
        }

        if (quest.stages == null || quest.stages.Count == 0)
        {
            questObjectives.text = "No stages or objectives available for this quest.";
            return;
        }

        StringBuilder objectivesText = new StringBuilder();

        for (int i = 0; i < quest.stages.Count; i++)
        {
            QuestStage stage = quest.stages[i];
            objectivesText.Append($"\n<b>Stage {i + 1}: {stage.stageName}</b>");

            if (stage.isStageCompleted)
            {
                objectivesText.Append(" (Completed)");
            }
            else if (i < quest.currentStageIndex)
            {
                objectivesText.Append(" (Past Stage)");
            }
            else if (i == quest.currentStageIndex)
            {
                objectivesText.Append(" (Current Stage)");
            }

            objectivesText.Append($"\n<i>{stage.stageDescription}</i>\n");

            if (stage.objectives == null || stage.objectives.Count == 0)
            {
                objectivesText.Append("  No objectives for this stage.\n");
                continue;
            }

            foreach (var objective in stage.objectives)
            {
                string objectiveProgress = objective.description;
                int currentProgress = QuestManager.instance.GetCurrentObjectiveProgress(objective);

                if (objective.type == ObjectiveType.Gather || objective.type == ObjectiveType.Build || objective.type == ObjectiveType.Plant || objective.type == ObjectiveType.Trade)
                {
                    objectiveProgress = $"  - {objective.description} ({currentProgress}/{objective.amount})";
                }
                else if (objective.type == ObjectiveType.Explore)
                {
                    objectiveProgress = $"  - {objective.description} {(objective.isCompleted ? "(Explored)" : "(Not Explored)")}";
                }

                objectivesText.Append(objectiveProgress);
                if (objective.isCompleted)
                {
                    objectivesText.Append(" (Completed)");
                }
                objectivesText.Append("\n");
            }
        }

        questObjectives.text = objectivesText.ToString();
    }
}
