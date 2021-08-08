using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private UIPanel mainMenuUI;

    [SerializeField]
    private UIPanel customGameUI;

    [SerializeField]
    private UIPanel loginMenuUI;


    private MenuState curState;

    public void Start()
    {
        loginMenuUI.uiManager = this;
        mainMenuUI.uiManager = this;
        customGameUI.uiManager = this;

        if (PlayfabManager.Player.LoggedIn || PlayfabManager.Player.PlayFabDisplayName != null)        
            curState = MenuState.MainMenu;        
        else
            curState = MenuState.LoginMenu;

        loginMenuUI.gameObject.SetActive(!PlayfabManager.Player.LoggedIn);
        mainMenuUI.gameObject.SetActive(PlayfabManager.Player.LoggedIn);
        customGameUI.gameObject.SetActive(false);

        mainMenuUI.OnShow();
    }

    public void SwapToUI(MenuState _menuState)
    {
        switch (curState)
        {
            case MenuState.MainMenu:
                mainMenuUI.OnHide();
                mainMenuUI.gameObject.SetActive(false);
                break;
            case MenuState.CustomGameUI:
                customGameUI.OnHide();
                customGameUI.gameObject.SetActive(false);
                break;
            case MenuState.LoginMenu:
                loginMenuUI.OnHide();
                loginMenuUI.gameObject.SetActive(false);
                break;
        }

        switch (_menuState)
        {
            case MenuState.MainMenu:
                mainMenuUI.gameObject.SetActive(true);
                mainMenuUI.OnShow();
                break;
            case MenuState.CustomGameUI:
                customGameUI.gameObject.SetActive(true);
                customGameUI.OnShow();
                break;
            case MenuState.LoginMenu:
                loginMenuUI.gameObject.SetActive(true);
                loginMenuUI.OnShow();
                break;
        }

        curState = _menuState;
    }

    public enum MenuState
    {
        MainMenu,
        CustomGameUI,
        LoginMenu,
    }

}
