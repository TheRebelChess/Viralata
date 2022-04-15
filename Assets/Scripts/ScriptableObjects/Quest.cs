using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    Killing,
    Collecting,
    Reaching,
    Talking
}

[Serializable]
public struct QuestObjective
{
    public QuestType type;
    public string description;
    public List<GameObject> acceptedObjects;
    public int totalQuantity;
    [HideInInspector]
    public int currentQuantity;
    [HideInInspector]
    public bool isComplete;
}

[CreateAssetMenu]
public class Quest : ScriptableObject
{
    [TextArea(15, 20)]
    public string description;

    public List<QuestObjective> objectives;
    public Quest nextQuest;
    [HideInInspector]
    public bool isComplete;
    public bool needReturn;
    public List<GameObject> rewards;
    public float xpGained;
    public float moneyGained;
}
