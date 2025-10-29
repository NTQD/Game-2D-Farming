using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic; // Added for List

public class QuestUI : MonoBehaviour
{
    public GameObject questPanel;
    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI questDescription;
    public TextMeshProUGUI questObjectives;

    public UnityEngine.UI.Button acceptButton; // New: Reference to the Accept button

    public Quest firstHarvestQuest;
    public Quest warmNightQuest;
    public Quest desertedIslandHouseQuest;
    public Quest shipOfHopeQuest;

    private List<Quest> availableQuests = new List<Quest>();
    private QuestGiver questGiver; // Reference to the QuestGiver

    private Quest currentlyDisplayedQuest; // New: To track the quest currently shown in the UI

    void Start()
    {
        if (questPanel == null)
        {
            questPanel = gameObject;
            Debug.LogWarning("QuestUI: questPanel was not assigned. Defaulting to the GameObject this script is attached to.");
        }

        questPanel.SetActive(false);
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

        questGiver = FindObjectOfType<QuestGiver>();
        if (questGiver == null)
        {
            Debug.LogError("QuestUI: QuestGiver not found in the scene!");
        }
    }

    private void OnDestroy()
    {
        QuestManager.instance.OnQuestStarted -= OnQuestStartedHandler;
        QuestManager.instance.OnQuestCompleted -= OnQuestCompletedHandler;
        QuestManager.instance.OnQuestUpdated -= OnQuestUpdatedHandler;
    }

    public void AcceptQuestOrAdvance()
    {
        if (currentlyDisplayedQuest == null || questGiver == null) return;

        // If the quest is completed, grant reward and advance to next quest
        if (currentlyDisplayedQuest.isCompleted)
        {
            // Reward is granted by QuestManager.CompleteQuest, which is called by CheckQuestCompletion
            // We just need to find and display the next available quest.
            int currentQuestIndex = availableQuests.IndexOf(currentlyDisplayedQuest);
            if (currentQuestIndex != -1 && currentQuestIndex < availableQuests.Count - 1)
            {
                ShowQuest(availableQuests[currentQuestIndex + 1]);
            }
            else
            {
                // All quests completed or no more quests to show
                questPanel.SetActive(false);
            }
        }
        else // Quest is not completed, so start it
        {
            questGiver.StartQuest(currentlyDisplayedQuest);
            questPanel.SetActive(false);
        }
    }

    public void CancelQuestUI()
    {
        questPanel.SetActive(false);
        if (UI_ShopController.instance != null)
        {
            UI_ShopController.instance.ShowShopPanel();
        }
    }

    // Handlers for QuestManager events
    void OnQuestStartedHandler(Quest quest)
    {
        questPanel.SetActive(false);
    }

    void OnQuestCompletedHandler(Quest quest)
    {
        if (questPanel.activeInHierarchy)
        {
            if (questGiver != null)
            {
                DisplayAvailableQuests(questGiver.GetAvailableQuests());
            }
        }
    }

    void OnQuestUpdatedHandler(Quest quest)
    {
        // Update the displayed quest if it's currently being shown
        if (questPanel.activeInHierarchy && currentlyDisplayedQuest == quest)
        {
            ShowQuest(quest);
        }
    }

    void Update()
    {
        // Update method is now empty as UI navigation is handled by buttons.
    }

    public void DisplayAvailableQuests(List<Quest> quests)
    {
        availableQuests = quests ?? new List<Quest>();

        if (availableQuests.Count == 0)
        {
            questPanel.SetActive(true);
            if (questTitle != null) questTitle.text = "No Quests Available";
            if (questDescription != null) questDescription.text = string.Empty;
            if (questObjectives != null) questObjectives.text = string.Empty;
            currentlyDisplayedQuest = null;
            return;
        }

        // If there are available quests, show the first one by default
        ShowQuest(availableQuests[0]);
    }

    public void ShowQuest(Quest quest)
    {
        if (quest == null)
        {
            questPanel.SetActive(false);
            currentlyDisplayedQuest = null;
            return;
        }

        currentlyDisplayedQuest = quest;

        if (questTitle != null) questTitle.text = quest.title;
        if (questDescription != null) questDescription.text = quest.description;

        UpdateQuestObjectivesText(quest);
        questPanel.SetActive(true);

        // Enable/disable Accept button based on quest completion status
        if (acceptButton != null)
        {
            acceptButton.interactable = !quest.isCompleted; // Only interactable if not completed
        }
    }

    private void UpdateQuestObjectivesText(Quest quest)
    {
        if (questObjectives == null)
        {
            Debug.LogError("QuestUI: questObjectives TextMeshProUGUI is not assigned in the Inspector.");
            return;
        }

        StringBuilder objectivesText = new StringBuilder();

        if (quest.stages == null || quest.stages.Count == 0)
        {
            questObjectives.text = "No stages or objectives available for this quest.";
            return;
        }

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

                // Get current progress using the QuestManager
                int currentProgress = QuestManager.instance.GetCurrentObjectiveProgress(objective);

                // Format progress display based on objective type
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
