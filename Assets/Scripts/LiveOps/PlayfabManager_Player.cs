using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;

public class PlayfabManager_Player
{
    public bool LoggedIn => !string.IsNullOrEmpty(PlayFabUserId);
    public string PlayFabUserId { private set; get; }
    public string PlayFabAttachedEmail { private set; get; }
    public string PlayFabDisplayName { private set; get; } 

    public void SetPlayFabUserID(string _playFabUserID)
    {
        Debug.Log($"[PlayfabManager_Player] Setting PlayfabUserID to {_playFabUserID}");
        PlayFabUserId = _playFabUserID;
    }

    public void SetUserEmail(string _email)
    {
        Debug.Log($"[PlayfabManager_Player] Setting PlayfabUserID to {_email}");
        PlayFabAttachedEmail = _email;
    }

    public void SetDisplayNameLocal(string _name)
    {
        PlayFabDisplayName = _name;
    }

    public void SetDisplayName(string _name, Action<PlayFabAccountDisplayNameChangeResult> callback)
    {
        PlayFabAccountDisplayNameChangeResult returnInfo = new PlayFabAccountDisplayNameChangeResult();

        PlayFabClientAPI.UpdateUserTitleDisplayName(new PlayFab.ClientModels.UpdateUserTitleDisplayNameRequest
        {
            DisplayName = _name
        }, (_result) =>
        {
            Debug.Log($"[PlayfabManager_Player] Set Display Name Successfully - {_result.DisplayName}");

            SetDisplayNameLocal(_name);

            returnInfo.Successfull = true;

            if (callback != null)
                callback.Invoke(returnInfo);

        },
        (_error) => {

            Debug.Log($"[PlayfabManager_Player] Set Display Name Failed - {_error.GenerateErrorReport()}");

            returnInfo.Successfull = false;
            returnInfo.error = _error;

            if (callback != null)
                callback.Invoke(returnInfo);
        });
    }

    public void GetAccountInfo(Action<PlayFabAccountInfoResult> onResult)
    {
        PlayFabAccountInfoResult infoResult = new PlayFabAccountInfoResult();

        PlayFabClientAPI.GetAccountInfo(new PlayFab.ClientModels.GetAccountInfoRequest
        {
            PlayFabId = PlayfabManager.Player.PlayFabUserId
        }, (_result) =>
        {
            Debug.Log($"[PlayfabManager_Player] Grabbed Account Info - {_result.AccountInfo.ToJson()}");
            infoResult.Successfull = true;
            infoResult.info = _result;

            if (onResult != null)
                onResult.Invoke(infoResult);

        }, (_error) =>
        {
            infoResult.Successfull = false;
            infoResult.error = _error;

            Debug.Log($"[PlayfabManager_Player] Failed to Grab Account Info - {_error.GenerateErrorReport()}");


            if (onResult != null)
                onResult.Invoke(infoResult);
        });
    }

    public struct PlayFabAccountInfoResult
    {
        public bool Successfull;

        public PlayFab.ClientModels.GetAccountInfoResult info;
        public PlayFabError error;
    }

    public struct PlayFabAccountDisplayNameChangeResult
    {
        public bool Successfull;

        public PlayFabError error;
    }

}
