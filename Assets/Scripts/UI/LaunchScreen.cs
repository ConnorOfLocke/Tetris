using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchScreen : MonoBehaviour
{
    void Start()
    {
        //Attempt a login
        PlayfabManager.Login.AttemptAnonLogin(false, (result) => 
        {
            if (result.Successfull)
            {
                //attempt to load player data then
                PlayfabManager.Player.GetAccountInfo((infoResult) =>
                {
                    if (infoResult.Successfull)
                    {
                        PlayfabManager.Player.SetDisplayNameLocal(infoResult.info.AccountInfo.TitleInfo.DisplayName);
                        SceneLoader.Instance.LoadScene(SceneLoader.MenuScene);
                    }
                    else
                    {
                        SceneLoader.Instance.LoadScene(SceneLoader.MenuScene);
                        //reset pwd
                    }
                });
            }
            else
            {
                //we are not logged in or something when wrong
                SceneLoader.Instance.LoadScene(SceneLoader.MenuScene);
            }
        });
    }
}
