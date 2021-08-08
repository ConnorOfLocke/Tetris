using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomGameUI : UIPanel
{
    [SerializeField]
    private Slider levelSlider;
    [SerializeField]
    private Text levelSliderText;

    [SerializeField]
    private Slider lookAheadSlider;
    [SerializeField]
    private Text lookAheadSliderText;

    [SerializeField]
    private Toggle useShadowToggle;
    
    private int selectedLevel = 0;
    private const int maxSelectedLevel = 29;

    private int lookAheadMoves = 3;
    private const int maxLookAheadMoves = 3;

    public override void OnShow()
    {
        selectedLevel = PlayfabManager.UserData.PlayerData.CustomGame_StartLevel;
        lookAheadMoves = PlayfabManager.UserData.PlayerData.CustomGame_LookAhread;
        useShadowToggle.isOn = PlayfabManager.UserData.PlayerData.CustomGame_ShowShadow;

        levelSlider.value = ((float)selectedLevel / (float)maxSelectedLevel);
        lookAheadSlider.value = ((float)lookAheadMoves / (float)maxLookAheadMoves);
    }

    public override void OnHide()
    {
        
    }

    public void OnLevelSliderChanged()
    {
        selectedLevel = (int)(levelSlider.value * maxSelectedLevel);
        levelSliderText.text = $"{selectedLevel}";
    }

    public void OnLookAheadSliderChanged()
    {
        lookAheadMoves = (int)(lookAheadSlider.value * maxLookAheadMoves);
        lookAheadSliderText.text = $"{lookAheadMoves}";
    }

    public void OnStartGamePressed()
    {
        BoardManager.StartingLevel = selectedLevel;
        BoardManager.LookAheadSteps = lookAheadMoves;
        BoardManager.ShowShapeShadow = useShadowToggle.isOn;
        BoardManager.SendScoreAtEndOfRound = false;

        PlayfabManager.UserData.PlayerData.CustomGame_StartLevel = selectedLevel;
        PlayfabManager.UserData.PlayerData.CustomGame_LookAhread = lookAheadMoves;
        PlayfabManager.UserData.PlayerData.CustomGame_ShowShadow = useShadowToggle.isOn;

        PlayfabManager.UserData.UploadPlayerData((result) => 
        {            
            SceneLoader.Instance.LoadScene(SceneLoader.GameScene);
        });
    }

    public void OnBackButtonPressed()
    {
        uiManager.SwapToUI(UIManager.MenuState.MainMenu);
    }

}
