using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchScreen : MonoBehaviour
{
    void Start()
    {
        string lastUsedEmail = PlayfabManager.Login.LastEmailUsed;
        if (lastUsedEmail != null)
        {
            PlayfabManager.Login.AttemptEmailLogin(lastUsedEmail, PlayfabManager.Login.LastPwdUsed, (result) =>
            {
                Debug.Log("[LaunchScreen] Auto Email login with result " + result.Successfull);
                SceneLoader.Instance.LoadScene(SceneLoader.MenuScene);
            });
        }
        else
        {
            //Attempt a login
            Debug.Log("[LaunchScreen] Attempting Anon login");

            PlayfabManager.Login.AttemptAnonLogin(false, (result) =>
            {
                Debug.Log("[LaunchScreen] Anon login with result " + result.Successfull );
                SceneLoader.Instance.LoadScene(SceneLoader.MenuScene);
            });
        }
    }
}
