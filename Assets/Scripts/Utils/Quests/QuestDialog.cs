using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestDialog : MonoBehaviour
{
    public GameObject dialogCanvas;
    public TMP_Text dialogText;
    public GameObject questSelectionBox;
    public GameObject questSelectionText;

    private List<GameObject> quests = new List<GameObject>();

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
        questSelectionBox.SetActive(true);
    }

    public void CreateQuestSelectionOption(string text)
    {
        GameObject questText = Instantiate(questSelectionText, questSelectionBox.transform);
        questText.SetActive(true);
        questSelectionText.GetComponent<TMP_Text>().text = text;
        quests.Add(questText);
    }
}
