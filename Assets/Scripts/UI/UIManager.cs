using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private UIPanel mainMenuUI;

    [SerializeField]
    private UIPanel customGameUI;

    private MenuState curState;

    public void Start()
    {
        curState = MenuState.MainMenu;
       
        mainMenuUI.uiManager = this;
        customGameUI.uiManager = this;

        mainMenuUI.gameObject.SetActive(true);
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
        }

        curState = _menuState;
    }

    public enum MenuState
    {
        MainMenu,
        CustomGameUI
    }

}
