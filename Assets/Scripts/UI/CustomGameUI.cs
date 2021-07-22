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

    private int selectedLevel = 0;
    private const int maxSelectedLevel = 29;

    public override void OnShow()
    {
        levelSlider.value = ((float)selectedLevel / (float)maxSelectedLevel);
    }

    public override void OnHide()
    {
        
    }

    public void OnLevelSliderChanged()
    {
        selectedLevel = (int)(levelSlider.value * maxSelectedLevel);
        levelSliderText.text = $"{selectedLevel}";
    }

    public void OnStartGamePressed()
    {
        BoardManager.StartingLevel = selectedLevel;

        SceneLoader.Instance.LoadScene(SceneLoader.GameScene);
    }

    public void OnBackButtonPressed()
    {
        uiManager.SwapToUI(UIManager.MenuState.MainMenu);
    }

}
