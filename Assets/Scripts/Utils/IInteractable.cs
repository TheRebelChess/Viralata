using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void OnInteract();
    public void NextOption();
    public void PreviousOption();
    public void SelectOption();
    public void ExitInteraction();
}
