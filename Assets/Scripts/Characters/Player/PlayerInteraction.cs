using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private QuestGiver npcToInteract;
    private PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            npcToInteract.NextOption();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            npcToInteract.PreviousOption();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            npcToInteract.SelectQuest();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            npcToInteract.ExitInteraction();

            playerMovement.enabled = true;
            playerMovement.AimAt(null);
            this.enabled = false;
        }
    }

    public void SetNPC(GameObject npc)
    {
        npcToInteract = npc.GetComponent<QuestGiver>();
    }
}
