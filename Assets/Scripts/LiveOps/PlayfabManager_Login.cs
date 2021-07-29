using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;

public class PlayfabManager_Login
{
    public void AttemptAnonLogin(bool createAccount, Action<PlayFabLoginResult> onResult)
    {
        Debug.Log("[PlayfabManager_Login] Attempting anon login");

        PlayFabLoginResult loginResult = new PlayFabLoginResult();

        void onSuccess(PlayFab.ClientModels.LoginResult _result)
        {
            loginResult.Successfull = true;
            loginResult.NewUser = _result.NewlyCreated;

            PlayfabManager.Player.SetPlayFabUserID(_result.PlayFabId);
            Debug.Log($"[PlayfabManager_Login] Successfull anon login - {_result.ToString()}");

            if (!_result.NewlyCreated)
            {
                //Get User Info
                Debug.Log($"[PlayfabManager_Login] Successfull anon login - {_result.ToString()}");
                PlayfabManager.Player.GetAccountInfo((_infoResult) =>
                {
                    if (_infoResult.Successfull)
                    {
                        PlayfabManager.Player.SetDisplayNameLocal(_infoResult.info.AccountInfo.TitleInfo.DisplayName);

                        if (onResult != null)
                            onResult.Invoke(loginResult);
                    }
                    else
                    {
                        if (onResult != null)
                            onResult.Invoke(loginResult);
                    }
                });
            }
            else
            {
                if (onResult != null)
                    onResult.Invoke(loginResult);
            }
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
                CreateAccount = createAccount,
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
                CreateAccount = createAccount,
                TitleId = PlayfabManager.Title_ID,
            }, onSuccess, onFailure);
        }
        else
        {
            PlayFabClientAPI.LoginWithCustomID(new PlayFab.ClientModels.LoginWithCustomIDRequest
            {
                CustomId = "Standalone_" + SystemInfo.deviceUniqueIdentifier,
                TitleId = PlayfabManager.Title_ID,
                CreateAccount = createAccount,
            }, onSuccess, onFailure);
        }
    }

    public void LinkEmailAndPwd(string _email, string _password, bool newLocalUser, Action<PlayFabLinkEmailResult> onResult)
    {
        PlayFabLinkEmailResult emailResult = new PlayFabLinkEmailResult();

        PlayfabManager.Player.GetAccountInfo((result) =>
        {
            if (result.Successfull)
            {
                Debug.Log($"[PlayfabManager_Login] Successfully fetched user data, now linking email ");
                //Already linked, we good
                if (result.info.AccountInfo.PrivateInfo.Email == _email)
                {
                    Debug.Log($"[PlayfabManager_Login] Email is already linked to this account");

                    emailResult.state = PlayFabLinkEmailResult.EmailLinkState.EmailAlreadyLinked;
                    emailResult.NewUser = false;

                    if (onResult != null)
                        onResult.Invoke(emailResult);
                }
                else if (!string.IsNullOrEmpty(result.info.AccountInfo.PrivateInfo.Email))
                {
                    Debug.Log($"[PlayfabManager_Login] Logged in account is attached to different email, making a new account");

                    //Need a new account 
                    PlayFabClientAPI.RegisterPlayFabUser(new PlayFab.ClientModels.RegisterPlayFabUserRequest
                    {
                        Email = _email,
                        Password = _password,
                        RequireBothUsernameAndEmail = false,
                    }, (_result) =>
                    {
                        Debug.Log($"[PlayfabManager_Login] Successfully made a new email linked account");

                        PlayfabManager.Player.SetPlayFabUserID(_result.PlayFabId);
                        PlayfabManager.Player.SetUserEmail(_email);

                        emailResult.state = PlayFabLinkEmailResult.EmailLinkState.SuccessfullyLinkedEmail;
                        emailResult.NewUser = true;

                        if (onResult != null)
                            onResult.Invoke(emailResult);

                    },
                    //Something bad happened
                    (_error) =>
                    {
                        Debug.Log($"[PlayfabManager_Login] Failed to make a new {_error.GenerateErrorReport()}");

                        emailResult.state = PlayFabLinkEmailResult.EmailLinkState.FailedToLink;
                        emailResult.error = _error;

                        if (onResult != null)
                            onResult.Invoke(emailResult);
                    });
                }
                else
                {
                    Debug.Log($"[PlayfabManager_Login] No email found, attempting to link email");

                    //Account does not have an email attached, so we can link it
                    string newGUID = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);

                    PlayFabClientAPI.AddUsernamePassword(new PlayFab.ClientModels.AddUsernamePasswordRequest
                    {
                        Email = _email,
                        Password = _password,

                        //temp username for the moment
                        Username = newGUID
                    }, (_result) =>
                    {
                        Debug.Log($"[PlayfabManager_Login] Successfully linked email to account");

                        emailResult.state = PlayFabLinkEmailResult.EmailLinkState.SuccessfullyLinkedEmail;
                        emailResult.NewUser = newLocalUser;

                        if (onResult != null)
                            onResult.Invoke(emailResult);

                    }, (_error) =>
                    {
                        Debug.Log($"[PlayfabManager_Login] Fail to link email {_error.GenerateErrorReport()}");

                        emailResult.state = PlayFabLinkEmailResult.EmailLinkState.FailedToLink;
                        emailResult.error = _error;

                        if (onResult != null)
                            onResult.Invoke(emailResult);
                    });
                }
            }
            else
            {
                Debug.Log($"[PlayfabManager_Login] Failed to fetch user data, now linking email ");

                //unable to grab current user data
                emailResult.state = PlayFabLinkEmailResult.EmailLinkState.FailedToLink;
                emailResult.error = result.error;

                if (onResult != null)
                    onResult.Invoke(emailResult);
            }
        });
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
            
            PlayfabManager.Player.SetPlayFabUserID(_result.PlayFabId);

            //Get account info
            if (_result.NewlyCreated)
            {
                //Set Username
                if (onResult != null)
                    onResult.Invoke(loginResult);
            }
            else
            {
                //Get User Info
                PlayfabManager.Player.GetAccountInfo((_infoResult) =>
                {
                    if (_infoResult.Successfull)
                    {
                        PlayfabManager.Player.SetDisplayNameLocal(_infoResult.info.AccountInfo.TitleInfo.DisplayName);

                        if (onResult != null)
                            onResult.Invoke(loginResult);
                    }
                    else
                    {
                        if (onResult != null)
                            onResult.Invoke(loginResult);
                    }
                });
            }           
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

    public string LastEmailUsed
    {
        get
        {
            return PlayerPrefs.GetString("LastUsedEmail", "");
        }
        set
        {
            PlayerPrefs.SetString("LastUsedEmail", value);
            PlayerPrefs.Save();
        }
    }

    public string LastPwdUsed
    {
        get
        {
            return PlayerPrefs.GetString("LastPwdUsed", "");
        }
        set
        {
            PlayerPrefs.SetString("LastPwdUsed", value);
            PlayerPrefs.Save();
        }
    }


}

public struct PlayFabLinkEmailResult
{
    public EmailLinkState state;
    public PlayFabError error;
    public bool NewUser;

    public enum EmailLinkState
    {
        EmailAlreadyLinked,
        FailedToLink,
        SuccessfullyLinkedEmail
    }
}



public struct PlayFabLoginResult
{
    public bool Successfull;
    public bool NewUser;

    public PlayFabError error;
}