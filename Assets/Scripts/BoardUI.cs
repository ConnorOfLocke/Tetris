using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardUI : MonoBehaviour
{
    [SerializeField]
    Text scoreText;

    [SerializeField]
    Text levelText;

    bool isInitialised = false;
    BoardManager boardManager;

    public void Initialise(BoardManager _boardManager)
    {
        isInitialised = true;
        boardManager = _boardManager;
    }

    void Update()
    {
        if (isInitialised)
        {
            scoreText.text = boardManager.Score.ToString();
            levelText.text = boardManager.CurLevel.ToString();
        }
    }
}
