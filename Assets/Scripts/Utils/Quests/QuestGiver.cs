using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

public class QuestGiver : MonoBehaviour, IInteractable
{
    
    public List<QuestToGive> allQuests;
    List<Quest> activeQuests = new List<Quest>();
    public QuestSystem questSystem;

    public GameObject dialogCanvas;
    public TMP_Text dialogText;
    public GameObject questSelectionBox;
    public GameObject questSelectionText;

    private List<TMP_Text> questOptions = new List<TMP_Text>();
    private int selectQuestIndex = 0;

    private void Start()
    {
        for (int i = 0; i < allQuests.Count; i++)
        {
            allQuests[i].quest.InitializeQuest();
        }
    }

    public void OnInteract()
    {
        ActivateDialogUI();
        SetDialogText("Hello, pick a quest");
        ActivateQuestSelectionUI();

        if (allQuests != null && allQuests.Count > 0)
        {
            for (int i = 0; i < allQuests.Count; i++)
            {
                Quest currentQuest = allQuests[i].quest;
                CreateQuestSelectionOption(currentQuest.name);

            }

            selectQuestIndex = 0;
            questOptions[selectQuestIndex].color = Color.red;
        }
        //questSystem.RegisterQuest(allQuests[0].quest);
    }

    public void SetDialogText(string dialog)
    {
        dialogText.text = dialog;
    }

    public void ActivateDialogUI()
    {
        dialogCanvas.SetActive(true);
    }

    public void ActivateQuestSelectionUI()
    {
        if (questSelectionBox != null)
            questSelectionBox.SetActive(true);
    }

    public void CreateQuestSelectionOption(string text)
    {
        GameObject questText = Instantiate(questSelectionText, questSelectionBox.transform);
        questText.SetActive(true);
        questText.GetComponent<TMP_Text>().text = text;
        questOptions.Add(questText.GetComponent<TMP_Text>());
    }

    public void SelectOption()
    {
        Quest quest = allQuests[selectQuestIndex].quest;

        activeQuests.Add(quest);
        questSystem.RegisterQuest(quest);
        allQuests.RemoveAt(selectQuestIndex);

        GameObject questOption = questOptions[selectQuestIndex].gameObject;
        questOptions.RemoveAt(selectQuestIndex);

        Destroy(questOption);

        if (questOptions.Count > 0)
        {
            PreviousOption();
        }
        else
        {
            Destroy(questSelectionBox);
        }


    }

    public void NextOption()
    {
        if (questOptions.Count == 0)
        {
            return;
        }
        questOptions[selectQuestIndex].color = Color.white;

        selectQuestIndex = (selectQuestIndex + 1) % questOptions.Count;

        questOptions[selectQuestIndex].color = Color.red;
    }

    public void PreviousOption()
    {
        if (questOptions.Count == 0)
        {
            return;
        }
        questOptions[selectQuestIndex].color = Color.white;

        selectQuestIndex = Math.Abs((selectQuestIndex - 1)) % questOptions.Count;

        questOptions[selectQuestIndex].color = Color.red;
    }

    public void DeactivateDialogUI()
    {
        dialogCanvas.SetActive(false);
    }

    public void DeactivateQuestSelectionUI()
    {
        if (questSelectionBox != null)
            questSelectionBox.SetActive(false);
    }

    public void CleanOptions()
    {
        for (int i = questOptions.Count - 1; i >= 0; i--)
        {
            GameObject questOption = questOptions[i].gameObject;
            questOptions.RemoveAt(i);

            Destroy(questOption);
        }
    }

    public void ExitInteraction()
    {
        DeactivateDialogUI();
        CleanOptions();
        DeactivateQuestSelectionUI();
    }

}
