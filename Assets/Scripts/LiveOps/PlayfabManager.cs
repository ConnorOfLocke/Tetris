using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;

public class PlayfabManager : MonoBehaviour
{
    public const string Title_ID = "18BCC";

    public static PlayfabManager_Login Login
    {
        get 
        {
            if (_login == null)
                _login = new PlayfabManager_Login();
            return _login;
        }
    }

    public static PlayfabManager_Player Player
    {
        get
        {
            if (_player == null)
                _player = new PlayfabManager_Player();
            return _player;
        }
    }

    public static PlayfabManager_Leaderboards Leaderboards
    {
        get
        {
            if (_leaderboards == null)
                _leaderboards = new PlayfabManager_Leaderboards();
            return _leaderboards;
        }
    }

    public static void CallCloudScript(string functionName, object functionParams, Action<CallCloudScriptResult> _callback)
    {
        CallCloudScriptResult callScriptResult = new CallCloudScriptResult();
        Debug.Log($"[PlayfabManager] Calling cloudscript function {functionName}");


        PlayFabClientAPI.ExecuteCloudScript(new PlayFab.ClientModels.ExecuteCloudScriptRequest()
        {
            FunctionName = functionName,
            FunctionParameter = functionParams
        }, (_result) =>
        {
            Debug.Log($"[PlayfabManager] Returned cloudscript function {functionName}");

            if (_result.Error != null)
            {
                Debug.Log($"[PlayfabManager] Cloudscript {functionName} returned an script execution error{_result.Error.Error} \n {_result.Error.Message} \n {_result.Error.StackTrace} ");
            }

            if (_result.LogsTooLarge.HasValue && _result.LogsTooLarge.Value)
            {
                Debug.Log($"[PlayfabManager] Function Logs too big for {functionName} (< 350Kb), Return had to drop some messages");
            }
            //log out any returned messages
            foreach (var log in _result.Logs)
            {
                Debug.Log(log.Message);
            }

            if (_result.FunctionResultTooLarge.HasValue && _result.FunctionResultTooLarge.Value)
            {
                Debug.Log($"[PlayfabManager] Function Result too big for {functionName} (< 350Kb), Return had to drop it");
            }

            callScriptResult.successfull = true;
            callScriptResult.returnValue = _result.FunctionResult.ToString();

            if (_callback != null)
                _callback.Invoke(callScriptResult);
        },
        (_error) =>
        {
            Debug.Log($"[PlayfabManager] Failed to returned from cloudscript function {functionName} with error {_error.GenerateErrorReport()}");


            callScriptResult.successfull = false;
            callScriptResult.error = _error;


            if (_callback != null)
                _callback.Invoke(callScriptResult);
        });
    }

    private static PlayfabManager_Login _login;
    private static PlayfabManager_Player _player;
    private static PlayfabManager_Leaderboards _leaderboards;

    public struct CallCloudScriptResult
    {
        public bool successfull;
        public PlayFabError error;
        public string returnValue;
    }

}
