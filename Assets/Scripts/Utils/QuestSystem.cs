using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    public List<Quest> activeQuests;
    public List<Quest> completedQuests;

    public TMP_Text talkText;
    public TMP_Text collectText;
    public TMP_Text killText;
    public TMP_Text reachText;

    private void Start()
    {
        for (int i = 0; i < activeQuests.Count; i++)
        {
            activeQuests[i].InitializeQuest();
        }
    }

    public void RegisterQuest(Quest quest)
    {
        if (activeQuests.Contains(quest))
            return;

        activeQuests.Add(quest);

        switch (quest.objectives[0].type)
        {
            case QuestType.Talking:
                talkText.enabled = true;
                break;

            case QuestType.Collecting:
                collectText.enabled = true;
                break;

            case QuestType.Killing:
                killText.enabled = true;
                break;

            case QuestType.Reaching:
                reachText.enabled = true;
                break;

            default:
                break;
        }

    }

    public void OnPlayerKill(GameObject gameObject)
    {
        for (int i = 0; i < activeQuests.Count; i++)
        {
            for (int j = 0; j < activeQuests[i].objectives.Count; j++)
            {
                if (activeQuests[i].objectives[j].type == QuestType.Killing)
                {

                    if (activeQuests[i].objectives[j].CheckCompletion())
                        continue;

                    // Checar game objects aceitos aqui
                    activeQuests[i].objectives[j].IncrementQuantity();
                }
            }

            if (activeQuests[i].CheckQuestCompletion())
            {
                CompleteQuest(activeQuests[i]);
            }
        }
    }

    public void OnPlayerTalk(GameObject gameObject)
    {
        for (int i = 0; i < activeQuests.Count; i++)
        {
            for (int j = 0; j < activeQuests[i].objectives.Count; j++)
            {
                if (activeQuests[i].objectives[j].type == QuestType.Talking)
                {
                    if (activeQuests[i].objectives[j].CheckCompletion())
                        continue;

                    // Checar game objects aceitos aqui
                    activeQuests[i].objectives[j].IncrementQuantity();
                }
            }

            if (activeQuests[i].CheckQuestCompletion())
            {
                CompleteQuest(activeQuests[i]);
            }
        }
    }

    public void OnPlayerReach(GameObject gameObject)
    {
        for (int i = 0; i < activeQuests.Count; i++)
        {
            for (int j = 0; j < activeQuests[i].objectives.Count; j++)
            {
                if (activeQuests[i].objectives[j].type == QuestType.Reaching)
                {
                    if (activeQuests[i].objectives[j].CheckCompletion())
                        continue;

                    // Checar game objects aceitos aqui
                    activeQuests[i].objectives[j].IncrementQuantity();
                }
            }

            if (activeQuests[i].CheckQuestCompletion())
            {
                CompleteQuest(activeQuests[i]);
            }
        }
    }

    public void OnPlayerCollect(GameObject gameObject)
    {
        for (int i = 0; i < activeQuests.Count; i++)
        {
            for (int j = 0; j < activeQuests[i].objectives.Count; j++)
            {
                if (activeQuests[i].objectives[j].type == QuestType.Collecting)
                {
                    if (activeQuests[i].objectives[j].CheckCompletion())
                        continue;

                    // Checar game objects aceitos aqui
                    activeQuests[i].objectives[j].IncrementQuantity();
                }
            }

            if (activeQuests[i].CheckQuestCompletion())
            {
                CompleteQuest(activeQuests[i]);
            }
        }
    }

    private void CompleteQuest(Quest quest)
    {
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        Debug.Log("Quest Completed");

        switch (quest.objectives[0].type)
        {
            case QuestType.Talking:
                talkText.text = "Talk Quest Completed";
                break;

            case QuestType.Collecting:
                collectText.text = "Collect Quest Completed";
                break;

            case QuestType.Killing:
                killText.text = "Kill Quest Completed";
                break;

            case QuestType.Reaching:
                reachText.text = "Reach Quest Completed";
                break;

            default:
                break;
        }
    }
}
