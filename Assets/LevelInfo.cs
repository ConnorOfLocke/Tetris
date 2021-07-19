using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Info", menuName = "ScriptableObjects/LevelInfo", order = 1)]
public class LevelInfo : ScriptableObject
{
    [SerializeField]
    private int[] StepsPerLevel;

    public int GetStepsPerLevel(int levelIndex)
    {
        if (levelIndex >= StepsPerLevel.Length)
        {
            return StepsPerLevel[StepsPerLevel.Length - 1];
        }
        return StepsPerLevel[levelIndex];
    }

}
