using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using Newtonsoft.Json;

public class PlayfabManager_UserData
{
    private PlayerData playerData = null;
    public bool hasInitialised => playerData != null;
    private string playerDataKey = "PlayerData";

    public PlayerData PlayerData => playerData;

    public void UpdatePlayerData(Action<UploadPlayerDataResult> callback)
    {
        Debug.Log($"[PlayfabManager_UserData] Attempting to retrieve player data");

        UploadPlayerDataResult callbackResult = new UploadPlayerDataResult();

        PlayFabClientAPI.GetUserData(new PlayFab.ClientModels.GetUserDataRequest() 
        { 
            PlayFabId = PlayfabManager.Player.PlayFabUserId,
            Keys = null
        }, 
        (_result) => {

            Debug.Log($"[PlayfabManager_UserData] Successfully retrieved player data");
            callbackResult.successfull = true;

            if (_result.Data != null && _result.Data.ContainsKey(playerDataKey))
            {
                PlayerData data = JsonConvert.DeserializeObject<PlayerData>(_result.Data[playerDataKey].Value);
                playerData = data;
            }
            else
            {
                Debug.Log($"[PlayfabManager_UserData] User Data was empty for this user, using defaults");
                playerData = new PlayerData();
            }

            if (callback != null)
                callback.Invoke(callbackResult);
        },
        (_error) => {

            Debug.Log($"[PlayfabManager_UserData] Failed to retrieve player data with error {_error.GenerateErrorReport()}");

            callbackResult.successfull = false;
            callbackResult.error = _error;

            if (callback != null)
                callback.Invoke(callbackResult);
        });
    }

    public void UploadPlayerData(Action<UploadPlayerDataResult> callback)
    {
        Debug.Log($"[PlayfabManager_UserData] Attempting to upload player data");

        UploadPlayerDataResult callbackResult = new UploadPlayerDataResult();

        string playerDataString = JsonConvert.SerializeObject(playerData);
        Dictionary<string, string> data = new Dictionary<string, string>();
        data[playerDataKey] = playerDataString;

        PlayFabClientAPI.UpdateUserData(new PlayFab.ClientModels.UpdateUserDataRequest()
        {
           Data = data
        },
        (_result) =>         
        {
            Debug.Log($"[PlayfabManager_UserData] Successfully uploaded player data");

            callbackResult.successfull = true;

            if (callback != null)
                callback.Invoke(callbackResult);
        }, 
        (_error) => 
        {
            Debug.Log($"[PlayfabManager_UserData] Failed to upload player data with error {_error.GenerateErrorReport()}");

            callbackResult.successfull = false;
            callbackResult.error = _error;

            if (callback != null)
                callback.Invoke(callbackResult);
        }); 
    }


    public struct UploadPlayerDataResult
    {
        public bool successfull;
        public PlayFabError error;
    }

}

[Serializable]
public class PlayerData
{
    public int  CustomGame_StartLevel = 0;
    public int  CustomGame_LookAhread = 3;
    public bool CustomGame_ShowShadow = true;
}
