using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField]
    private GameObject originObject;

    [SerializeField]
    Text gameOverText;

    [SerializeField]
    Text scoreText;
    
    [SerializeField]
    Text linesClearedText;
    
    [SerializeField]
    Text levelReachedText;

    private Action onRestartCallback;

    public void Start()
    {
        originObject.SetActive(false);
    }

    public void InitialiseAndShow(long _score, int _linesCleared, int _levelReached, Action _OnRestart)
    {
        originObject.SetActive(true);
        gameOverText.text = GameOverLines[UnityEngine.Random.Range(0, GameOverLines.Length)];
        scoreText.text = $"Score: {_score}";
        linesClearedText.text = $"Lines: {_linesCleared}";
        levelReachedText.text = $"Level: {_levelReached}";

        onRestartCallback = _OnRestart;
    }

    public void OnRestartButtonPressed()
    {
        originObject.SetActive(false);

        if (onRestartCallback != null)
            onRestartCallback.Invoke();
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
