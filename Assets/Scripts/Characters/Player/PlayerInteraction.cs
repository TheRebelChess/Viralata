using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable interactableGO;
    private PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            interactableGO.NextOption();
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            interactableGO.PreviousOption();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            interactableGO.SelectOption();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            interactableGO.ExitInteraction();

            playerMovement.enabled = true;
            playerMovement.AimAt(null);
            this.enabled = false;
        }
    }

    public void SetInteractableObj(GameObject interactableObj)
    {
        interactableGO = interactableObj.GetComponent<IInteractable>();
    }
}
