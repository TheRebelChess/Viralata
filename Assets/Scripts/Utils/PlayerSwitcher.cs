using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerSwitcher : MonoBehaviour, IInteractable
{
    public GameObject dialogCanvas;
    public TMP_Text dialogText;
    public GameObject charSelectionBox;
    public GameObject charSelectionText;
    public int numPlayerChars = 3;
    public string[] classNames;

    private List<TMP_Text> charOptions = new List<TMP_Text>();
    private GameObject player;
    private int selectCharIndex = 0;


    public void OnInteract()
    {
        ActivateDialogUI();
        SetDialogText("Which class do you wish to change to?");
        ActivateQuestSelectionUI();

        for (int i = 0; i < numPlayerChars; i++)
        {
            CreateCharSelectionOption(classNames[i]);
        }
        charOptions[0].color = Color.red;
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
        if (charSelectionBox != null)
            charSelectionBox.SetActive(true);
    }

    public void CreateCharSelectionOption(string text)
    {
        GameObject charText = Instantiate(charSelectionText, charSelectionBox.transform);
        charText.SetActive(true);
        charText.GetComponent<TMP_Text>().text = text;
        charOptions.Add(charText.GetComponent<TMP_Text>());
    }

    public void SelectOption()
    {
        var numChildren = player.transform.childCount;

        for (int i = numChildren - numPlayerChars; i < numChildren; i++)
        {
            if (selectCharIndex + numPlayerChars == i)
            {
                player.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                player.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public void NextOption()
    {
        if (charOptions.Count == 0)
        {
            return;
        }
        charOptions[selectCharIndex].color = Color.white;

        selectCharIndex = (selectCharIndex + 1) % charOptions.Count;

        charOptions[selectCharIndex].color = Color.red;
    }

    public void PreviousOption()
    {
        if (charOptions.Count == 0)
        {
            return;
        }
        charOptions[selectCharIndex].color = Color.white;

        selectCharIndex = Math.Abs((selectCharIndex - 1)) % charOptions.Count;

        charOptions[selectCharIndex].color = Color.red;
    }

    public void DeactivateDialogUI()
    {
        dialogCanvas.SetActive(false);
    }

    public void DeactivateQuestSelectionUI()
    {
        if (charSelectionBox != null)
            charSelectionBox.SetActive(false);
    }

    public void CleanOptions()
    {
        for (int i = numPlayerChars - 1; i >= 0; i--)
        {
            GameObject charOption = charOptions[i].gameObject;
            charOptions.RemoveAt(i);

            Destroy(charOption);
        }
        selectCharIndex = 0;
    }

    public void ExitInteraction()
    {
        DeactivateDialogUI();
        CleanOptions();
        DeactivateQuestSelectionUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            player = other.gameObject;
            other.gameObject.GetComponent<PlayerMovement>().ActivateInteraction(this.gameObject);
        }
    }
}
