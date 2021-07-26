using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;

public class PlayfabManager_Login
{
    public void AttemptAnonLogin(Action<PlayFabLoginResult> onResult)
    {
        Debug.Log("[PlayfabManager_Login] Attempting anon login");

        PlayFabLoginResult loginResult = new PlayFabLoginResult();

        void onSuccess(PlayFab.ClientModels.LoginResult _result)
        {
            loginResult.Successfull = true;
            loginResult.NewUser = _result.NewlyCreated;

            Debug.Log($"[PlayfabManager_Login] Successfull anon login - {_result.ToString()}");

            if (onResult != null)
                onResult.Invoke(loginResult);
        }

        void onFailure(PlayFabError _error)
        {
            loginResult.Successfull = false;
            loginResult.error = _error;

            Debug.Log($"[PlayfabManager_Login] Failed anon login - {_error.GenerateErrorReport()}");
            

            if (onResult != null)
                onResult.Invoke(loginResult);
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            PlayFabClientAPI.LoginWithAndroidDeviceID(new PlayFab.ClientModels.LoginWithAndroidDeviceIDRequest
            {
                AndroidDevice = SystemInfo.deviceModel,
                AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
                OS = SystemInfo.operatingSystem,
                CreateAccount = true,
                TitleId = PlayfabManager.Title_ID,
            }, onSuccess, onFailure); 
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            PlayFabClientAPI.LoginWithIOSDeviceID(new PlayFab.ClientModels.LoginWithIOSDeviceIDRequest
            {
                DeviceModel = SystemInfo.deviceModel,
                DeviceId = SystemInfo.deviceUniqueIdentifier,
                OS = SystemInfo.operatingSystem,
                CreateAccount = true,
                TitleId = PlayfabManager.Title_ID,
            }, onSuccess, onFailure);
        }
        else
        {
            PlayFabClientAPI.LoginWithCustomID(new PlayFab.ClientModels.LoginWithCustomIDRequest
            {
                CustomId = "Standalone_" + SystemInfo.deviceUniqueIdentifier,
                TitleId = PlayfabManager.Title_ID,
                CreateAccount = true,
            }, onSuccess, onFailure);
        }
    }

    public void AttemptEmailLogin(string _email, string _password, Action<PlayFabLoginResult> onResult)
    {
        Debug.Log("[PlayfabManager_Login] Attempting email login");

        PlayFabLoginResult loginResult = new PlayFabLoginResult();

        PlayFabClientAPI.LoginWithEmailAddress(new PlayFab.ClientModels.LoginWithEmailAddressRequest 
        {  
            Email = _email,
            Password = _password,
            TitleId = PlayfabManager.Title_ID,

        }, (_result) => {

            Debug.Log($"[PlayfabManager_Login] Successfull email login - {_result.ToString()}");

            loginResult.Successfull = true;
            loginResult.NewUser = _result.NewlyCreated;

            if (onResult != null)
                onResult.Invoke(loginResult);
        },
        (_error) =>
        {
            Debug.Log($"[PlayfabManager_Login] Failed email login - {_error.GenerateErrorReport()}");

            loginResult.Successfull = false;
            loginResult.error = _error;

            if (onResult != null)
                onResult.Invoke(loginResult);
        });
    }
}

public struct PlayFabLoginResult
{
    public bool Successfull;
    public bool NewUser;

    public PlayFabError error;
}