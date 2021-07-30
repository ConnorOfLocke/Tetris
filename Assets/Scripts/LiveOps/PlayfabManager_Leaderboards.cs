using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;

public class PlayfabManager_Leaderboards
{
    public const string QuickPlayScore = "QuickPlayScore";

    public void GetLeaderboardEntries(string statisticName, int maxResults, Action<PlayFabLeaderboardResult> callback)
    {
        Debug.Log($"[PlayfabManager_Leaderboards] Attempting leaderboard to get entries for {statisticName}");

        PlayFabLeaderboardResult returnVal = new PlayFabLeaderboardResult();

        PlayFabClientAPI.GetLeaderboard(new PlayFab.ClientModels.GetLeaderboardRequest
        {
            StatisticName = statisticName,
            StartPosition = 0,
            MaxResultsCount = maxResults

        },
        (_result) =>
        {
            Debug.Log($"[PlayfabManager_Leaderboards] Successfully retrieved leaderboard entries {statisticName}");

            returnVal.successfull = true;
            returnVal.leaderboardEntries = _result.Leaderboard;

            if (callback != null)
                callback.Invoke(returnVal);
        },
        (_error) =>
        {
            Debug.Log($"[PlayfabManager_Leaderboards] Failed to get leaderboard entries {_error.GenerateErrorReport()}");

            returnVal.successfull = false;
            returnVal.error = _error;

            if (callback != null)
                callback.Invoke(returnVal);
        });
    }

    public void SetPlayerStatistic(string statisticName, int value, Action<PlayFabSetStatisticResult> callback)
    {
        Debug.Log($"[PlayfabManager_Leaderboards] Attempting to update statistic for {statisticName}");
        PlayFabSetStatisticResult returnObject = new PlayFabSetStatisticResult();

        PlayFabClientAPI.UpdatePlayerStatistics( new PlayFab.ClientModels.UpdatePlayerStatisticsRequest {
            Statistics = new List<PlayFab.ClientModels.StatisticUpdate>() {
                new PlayFab.ClientModels.StatisticUpdate()
                {
                    StatisticName = statisticName,
                    Value = value,
                }
            }
        },
        (_result) => 
        { 
            Debug.Log($"[PlayfabManager_Leaderboards] Successfully updated statistic for {statisticName}");
            returnObject.successfull = true;

            if (callback != null)
                callback.Invoke(returnObject);
        },
        (_error) => 
        {
            Debug.Log($"[PlayfabManager_Leaderboards] Failed to update statistic for {statisticName}");
            Debug.Log($"[PlayfabManager_Leaderboards] Failed to update statistic for {_error.GenerateErrorReport()}");
            returnObject.successfull = false;
            returnObject.error = _error;

            if (callback != null)
                callback.Invoke(returnObject);
        });
    }

    public struct PlayFabLeaderboardResult
    {
        public bool successfull;
        public PlayFabError error;
        public List<PlayFab.ClientModels.PlayerLeaderboardEntry> leaderboardEntries;
    }

    public struct PlayFabSetStatisticResult
    {
        public bool successfull;
        public PlayFabError error;
    }

}
