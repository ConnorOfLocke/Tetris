using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : UIPanel
{
    public override void OnShow()
    {
        
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
        
    }

}
