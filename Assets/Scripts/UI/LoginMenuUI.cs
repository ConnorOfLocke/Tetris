using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoginMenuUI : UIPanel
{
    [SerializeField]
    TMP_InputField emailField;

    [SerializeField]
    TMP_InputField pwdField;


    public override void OnHide()
    {
        
    }

    public override void OnShow()
    {
        
    }

    public void OnEmailFieldChanged()
    { 
    
    }

    public void OnPwdFieldChanged()
    {

    }

    public void OnEmailLoginButton()
    {
        string email = emailField.text;
        string pwd = pwdField.text;

        PlayfabManager.Login.AttemptEmailLogin(email, pwd, LoginCallback);
    }

    public void OnSkipLoginButton()
    {
        PlayfabManager.Login.AttemptAnonLogin(LoginCallback);
    }

    private void LoginCallback(PlayFabLoginResult loginResult)
    {

    }
}
