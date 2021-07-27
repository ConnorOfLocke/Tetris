using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoginMenuUI : UIPanel
{
    [SerializeField]
    GameObject loginAccountArea;
    [SerializeField]
    GameObject createAccountArea;   

    [Space(10)]
    [SerializeField]
    TMP_InputField emailField;

    [SerializeField]
    TMP_InputField pwdField;

    [SerializeField]
    TMP_Text loginInfo;


    [Space(10)]
    [SerializeField]
    TMP_InputField createEmailField;

    [SerializeField]
    TMP_InputField createPwdField1;

    [SerializeField]
    TMP_InputField createPwdField2;

    [SerializeField]
    TMP_Text createLoginInfo;

    private bool showingCrateAccountArea = false;

    private bool validCreatedLogin = false;

    private const string isValidMessage = "Looks good";
    private const string pwdDontMatchMessage = "Passwords do not match";
    private const string pwdTooShortMessage = "Password is too short";
    private const string emailAlreadtLinkedMessage = "Email already linked to an account";

    public override void OnHide()
    {

    }

    public override void OnShow()
    {
        loginAccountArea.SetActive(true);
        createAccountArea.SetActive(false);
    }

    //Login Area here
    public void OnEmailFieldChanged()
    { 
    
    }

    public void OnPwdFieldChanged()
    {

    }

    public void OnCreateAccountButton()
    {
        //Anon login first to link an email to an existing account
        PlayfabManager.Login.AttemptAnonLogin(true, (_result) =>
        {
            if (_result.Successfull)
            {
                loginAccountArea.SetActive(false);
                createAccountArea.SetActive(true);
            }
            else
            {
                loginInfo.text = _result.error.ErrorMessage;
            }
        });
    }

    public void OnCreateAccountBackButton()
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
        PlayfabManager.Login.AttemptAnonLogin(true, LoginCallback);
    }

    //Create Area here
    public void OnEmailCreateFieldChange()
    {

    }

    public void OnPWD1CreateFieldChange()
    {
        if (createPwdField1.text != createPwdField2.text)
        {
            validCreatedLogin = false;
            createLoginInfo.text = pwdDontMatchMessage;
        }
    }

    public void OnPWD2CreateFieldChange()
    {
        if (createPwdField1.text != createPwdField2.text)
        {
            validCreatedLogin = false;
            createLoginInfo.text = pwdDontMatchMessage;
        }
        else if (createPwdField1.text.Length < 6)
        {            
            validCreatedLogin = false;
            createLoginInfo.text = pwdTooShortMessage;
        }
        else
        {
            validCreatedLogin = true;
            createLoginInfo.text = isValidMessage;
        }
    }

    public void OnFinaliseNewAccount()
    {
        if (validCreatedLogin)
        {
            string email = createEmailField.text;
            string pwd = createPwdField1.text;
            PlayfabManager.Login.LinkEmailAndPwd(email, pwd, LinkEmailCallback);
        }
    }

    public void OnBackToLogin()
    {
        loginAccountArea.SetActive(true);
        createAccountArea.SetActive(false);
    }

    private void LinkEmailCallback(PlayFabLinkEmailResult emailLinkResult)
    {
        if (emailLinkResult.state == PlayFabLinkEmailResult.EmailLinkState.EmailAlreadyLinked)
        {
            createLoginInfo.text = emailAlreadtLinkedMessage;
        }
        else if (emailLinkResult.state == PlayFabLinkEmailResult.EmailLinkState.FailedToLink)
        {
            createLoginInfo.text = $"Failed to link {emailLinkResult.error.ErrorMessage}";
        }
        else //success
        {
            uiManager.SwapToUI(UIManager.MenuState.MainMenu);
        }
    }

    private void LoginCallback(PlayFabLoginResult loginResult)
    {
        if (loginResult.Successfull)
            uiManager.SwapToUI(UIManager.MenuState.MainMenu);
        else
        {
            if (showingCrateAccountArea)
            {
                createLoginInfo.text = loginResult.error.ErrorMessage;
            }
            else
            {
                loginInfo.text = loginResult.error.ErrorMessage;
            }
        }
    }
}
