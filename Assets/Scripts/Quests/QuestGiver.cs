
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    public Quest quest;
    public bool isQuestStarted = false;

    public void StartQuest()
    {
        if (!isQuestStarted)
        {
            isQuestStarted = true;
            QuestManager.instance.StartQuest(quest);
        }
    }

    // We will call this method from the player's interaction script
    public void Interact()
    {
        StartQuest();
    }
}
