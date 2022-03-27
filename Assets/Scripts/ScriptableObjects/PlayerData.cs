using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerData : ScriptableObject
{
    [Range(0f, 25f)] public float baseSpeed = 5f;

    [Range(0f, 1f)] public float walkSpeedModifier = 0.225f;

    [Range(1f, 2f)] public float runSpeedModifier = 1f;

    public float timeToReachTargetRotation = 0.14f;
}
