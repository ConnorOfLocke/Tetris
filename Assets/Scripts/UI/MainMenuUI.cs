using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuUI : UIPanel
{
    [SerializeField]
    TMP_Text userName;


    public override void OnShow()
    {
        if (PlayfabManager.Player.PlayFabDisplayName != null)
            userName.text = PlayfabManager.Player.PlayFabDisplayName;        
    }

    public override void OnHide()
    {
        
    }

    public void OnCustomGameButton()
    {
        uiManager.SwapToUI(UIManager.MenuState.CustomGameUI);
    }

    public void OnQuickGameButton()
    {
        SceneLoader.Instance.LoadScene(SceneLoader.GameScene);
    }

    public void OnLogoutButton()
    {
        uiManager.SwapToUI(UIManager.MenuState.LoginMenu);
    }

}
