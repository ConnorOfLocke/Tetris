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
    [SerializeField]
    GameObject setUserNameArea;

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


    [Space(10)]
    [SerializeField]
    TMP_InputField userNameSet;

    [SerializeField]
    TMP_Text userNameInfo;

    private bool showingCrateAccountArea = false;

    private bool validCreatedLogin = false;
    private bool validUserName = false;

    private bool newLocalUser = false;

    private const string isValidMessage = "Looks good";
    private const string pwdDontMatchMessage = "Passwords do not match";
    private const string pwdTooShortMessage = "Password is too short";
    private const string emailAlreadtLinkedMessage = "Email already linked to an account";
    private const string userNameNotCorrectLength = "Username needs to be 3-20 characters";

    public override void OnHide()
    {

    }

    public override void OnShow()
    {
        SetLoginMenuState(LoginMenuState.LoginAccount);
    }

    public void SetLoginMenuState(LoginMenuState loginState)
    {
        loginAccountArea.SetActive(false);
        createAccountArea.SetActive(false);
        setUserNameArea.SetActive(false);

        switch (loginState)
        {
            case LoginMenuState.LoginAccount:
                loginAccountArea.SetActive(true);
                break;
            case LoginMenuState.CreateAccount:
                createAccountArea.SetActive(true);
                break;
            case LoginMenuState.CreateUsername:
                setUserNameArea.SetActive(true);
                break;
        };
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
                newLocalUser = _result.NewUser;
                SetLoginMenuState(LoginMenuState.CreateAccount);
            }
            else
            {
                loginInfo.text = _result.error.ErrorMessage;
            }
        });
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
            PlayfabManager.Login.LinkEmailAndPwd(email, pwd, newLocalUser, LinkEmailCallback);
        }
    }

    public void OnBackToLogin()
    {
        SetLoginMenuState(LoginMenuState.LoginAccount);
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
            PlayfabManager.Login.LastEmailUsed = createEmailField.text;
            PlayfabManager.Login.LastPwdUsed = createPwdField1.text;

            if (emailLinkResult.NewUser)
            {
                SetLoginMenuState(LoginMenuState.CreateUsername);
            }
            else
            {
                uiManager.SwapToUI(UIManager.MenuState.MainMenu);
            }
        }
    }

    private void LoginCallback(PlayFabLoginResult loginResult)
    {
        if (loginResult.Successfull)
        {
            PlayfabManager.Login.LastEmailUsed = emailField.text;
            PlayfabManager.Login.LastPwdUsed = pwdField.text;

            if (loginResult.NewUser)
            {
                SetLoginMenuState(LoginMenuState.CreateUsername);
            }
            else
            {
                uiManager.SwapToUI(UIManager.MenuState.MainMenu);
            }
        }
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

    //Create Username
    public void OnNewUsernameFieldChange()
    {
        string checkUsername = userNameSet.text;
        bool isValid = true;
        if (checkUsername.Length < 3 && checkUsername.Length > 25)
        {
            isValid = false;
            userNameInfo.text = userNameNotCorrectLength;
        }

        if (isValid)
            userNameInfo.text = "";

        validUserName = isValid;
    }

    public void OnNewUsernameButton()
    {
        if (validUserName)
        {
            PlayfabManager.Player.SetDisplayName(userNameSet.text, (_result) =>
            {
                if (_result.Successfull)
                {
                    uiManager.SwapToUI(UIManager.MenuState.MainMenu);
                }
                else
                {
                    validUserName = false;
                    userNameInfo.text = _result.error.ErrorMessage;
                }
            });
        }
    }


    public enum LoginMenuState
    {
        LoginAccount,
        CreateAccount,
        CreateUsername
    }
}
