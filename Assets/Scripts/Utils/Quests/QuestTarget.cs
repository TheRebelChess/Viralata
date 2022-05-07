using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TalkQuest
{
    public Quest questRelated;

    [TextArea(5, 10)]
    public string dialog;
}

public class QuestTarget : MonoBehaviour
{
    public List<TalkQuest> quests;
    public QuestSystem questSystem;

    // Start is called before the first frame update
    
    public void OnInteract()
    {
        questSystem.OnPlayerTalk(this.gameObject);
    }
}
