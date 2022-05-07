using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    Killing,
    Collecting,
    Reaching,
    Talking
}

[Serializable]
public class QuestObjective
{
    public QuestType type;
    public string description;
    public List<GameObject> acceptedObjects;
    public int totalQuantity;
    [HideInInspector]
    public int currentQuantity;

    public void IncrementQuantity()
    {
        currentQuantity++;
    }

    public bool CheckCompletion()
    {
        if (currentQuantity >= totalQuantity)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

[CreateAssetMenu]
public class Quest : ScriptableObject
{
    [TextArea(15, 20)]
    public string description;

    public List<QuestObjective> objectives;
    public Quest nextQuest;
    [HideInInspector]
    public bool QuestCompleted = false;
    public bool needReturn;
    public List<GameObject> rewards;
    public float xpGained;
    public float moneyGained;

    public bool CheckQuestCompletion()
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            if (!objectives[i].CheckCompletion())
                return false;
        }
        QuestCompleted = true;
        return QuestCompleted;
    }

    public void InitializeQuest()
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            objectives[i].currentQuantity = 0;
        }
        QuestCompleted = false;
    }
}
