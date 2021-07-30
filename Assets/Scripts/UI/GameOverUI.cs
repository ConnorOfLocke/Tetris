using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField]
    private GameObject originObject;

    [SerializeField]
    private TMP_Text gameOverText;

    [SerializeField]
    private Text scoreText;

    [SerializeField]
    private Text linesClearedText;

    [SerializeField]
    private Text levelReachedText;

    [SerializeField]
    private LeaderboardUI leaderboardUI;

    [SerializeField]
    private GameObject leaderboardUIOrigin;

    private Action onRestartCallback;

    public void Start()
    {
        originObject.SetActive(false);
    }

    public void InitialiseAndShow(long _score, int _linesCleared, int _levelReached, bool useLeaderboard, Action _OnRestart)
    {
        originObject.SetActive(true);
        gameOverText.text = GameOverLines[UnityEngine.Random.Range(0, GameOverLines.Length)];
        scoreText.text = $"Score: {_score}";
        linesClearedText.text = $"Lines: {_linesCleared}";
        levelReachedText.text = $"Level: {_levelReached}";

        if (useLeaderboard)
        {
            leaderboardUIOrigin.SetActive(true);
            leaderboardUI.ShowLoadingObject(true);

            PlayfabManager.Leaderboards.SetPlayerStatistic(PlayfabManager_Leaderboards.QuickPlayScore, (int)_score, (_result) => 
            { 
                if (_result.successfull)
                {
                    leaderboardUI.InitLeaderboard(PlayfabManager_Leaderboards.QuickPlayScore, (success) => {
                        leaderboardUI.ShowLoadingObject(false);
                    });
                }
            });

        }
        else
        {
            leaderboardUIOrigin.SetActive(false);
        }
        
        onRestartCallback = _OnRestart;
    }

    public void OnRestartButtonPressed()
    {
        originObject.SetActive(false);

        if (onRestartCallback != null)
            onRestartCallback.Invoke();
    }

    public void OnMainMenuButtonPressed()
    {
        SceneLoader.Instance.LoadScene(SceneLoader.MenuScene);
    }

    string[] GameOverLines = new string[]{
        "Tetr'isnt",
        "None too shabby",
        "Game Oooover",
        "An attempt was made!",
        "Proud of ya!",
        "That was good run :D",
        "GG",
        "Ya fams are proud",
        "Good show!"
    };

}
