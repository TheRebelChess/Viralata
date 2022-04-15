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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
