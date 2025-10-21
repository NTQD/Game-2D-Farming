
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    public Quest quest;
    public bool isQuestStarted = false;

    public void StartQuest()
    {
        if (QuestManager.instance == null || quest == null)
        {
            return;
        }

        QuestStatus status = QuestManager.instance.GetQuestStatus(quest);

        if (status == QuestStatus.Completed)
        {
            isQuestStarted = false;
            return;
        }

        if (status == QuestStatus.Active)
        {
            isQuestStarted = true;
            return;
        }

        isQuestStarted = QuestManager.instance.StartQuest(quest);
    }

    // We will call this method from the player's interaction script
    public void Interact()
    {
        StartQuest();
    }
}
