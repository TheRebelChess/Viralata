using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestToGive
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
    public QuestSystem questSystem;

    private void Start()
    {
        // checar restri��es para adicionar as quests como dispon�veis


        allQuests[0].quest.InitializeQuest();
    }

    public void OnInteract()
    {
        // Mostrar sele��o de quest
        questSystem.RegisterQuest(allQuests[0].quest);
    }

}
