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
        BoardManager.ShowShapeShadow = true;
        BoardManager.SendScoreAtEndOfRound = false;

        SceneLoader.Instance.LoadScene(SceneLoader.GameScene);
    }

    public void OnBackButtonPressed()
    {
        uiManager.SwapToUI(UIManager.MenuState.MainMenu);
    }

}
