using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField]
    private LeaderboardUI_Entry leaderboardEntry;

    [SerializeField]
    private Transform leaderboardParent;

    [SerializeField]
    private GameObject loadingObject;


    private LeaderboardUI_Entry[] spawnedLeaderboardEntries;

    public void ShowLoadingObject(bool _value)
    {
        loadingObject.SetActive(_value);
    }

    public void InitLeaderboard(string leaderboardStatisticName, Action<bool> onLeaderboardInit)
    {
        if (spawnedLeaderboardEntries != null)
        {
            if (spawnedLeaderboardEntries.Length > 0)
            {
                for (int i = 0; i < spawnedLeaderboardEntries.Length; i++)
                {
                    Destroy(spawnedLeaderboardEntries[i].gameObject);
                }
            }
        }

        PlayfabManager.Leaderboards.GetLeaderboardEntries(leaderboardStatisticName, 10, (_result) =>
        {
            if (_result.successfull)
            {
                spawnedLeaderboardEntries = new LeaderboardUI_Entry[_result.leaderboardEntries.Count];

                for (int i = 0; i < _result.leaderboardEntries.Count; i++)
                {
                    LeaderboardUI_Entry newEntry = Instantiate(leaderboardEntry, leaderboardParent);
                    newEntry.gameObject.name += $"_{i}";
                    newEntry.Initialise(_result.leaderboardEntries[i].Position, _result.leaderboardEntries[i].DisplayName, _result.leaderboardEntries[i].StatValue.ToString());
                    spawnedLeaderboardEntries[i] = newEntry;                    
                }

                if (onLeaderboardInit != null)
                    onLeaderboardInit.Invoke(true);
            }
            else
            {
                spawnedLeaderboardEntries = new LeaderboardUI_Entry[0];
                
                if (onLeaderboardInit != null)
                    onLeaderboardInit.Invoke(false);
            }
        });
    }


}
