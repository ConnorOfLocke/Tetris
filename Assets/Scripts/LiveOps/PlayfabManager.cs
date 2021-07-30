using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    private static PlayfabManager_Login _login;
    private static PlayfabManager_Player _player;
    private static PlayfabManager_Leaderboards _leaderboards;
}
