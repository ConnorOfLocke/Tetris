using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayfabManager : MonoBehaviour
{
    public static PlayfabManager_Login Login
    {
        get 
        {
            if (_login == null)
                _login = new PlayfabManager_Login();
            return _login;
        }
    }

    public const string Title_ID = "18BCC";

    private static PlayfabManager_Login _login;
}
