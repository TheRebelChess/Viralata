using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Database : ScriptableObject
{
    public List<ScriptableObject> scriptableObjects;

    public ScriptableObject GetObject(string name)
    {
        for (int i = 0; i < scriptableObjects.Count; i++)
        {
            if (scriptableObjects[i].name == name)
            {
                return scriptableObjects[i];
            }
        }

        return null;
    }
}
