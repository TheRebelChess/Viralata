using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct QuestToGive
{
    public int minLevelRequired;
    public Quest previousQuestRequired;
    public Quest quest;

    [TextArea(5, 10)]
    public string Dialog;
}

public class QuestGiver : MonoBehaviour
{
    
    public List<QuestToGive> allQuests;
    List<Quest> questsAvailable;

    private void Awake()
    {
        // checar restrições para adicionar as quests como disponíveis
    }

}
