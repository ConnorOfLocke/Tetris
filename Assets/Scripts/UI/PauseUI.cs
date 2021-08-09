using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseUI : MonoBehaviour
{
    [SerializeField]
    private GameObject originObject;

    public void Start()
    {
        originObject.SetActive(false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!BoardManager.Paused)
                OnPauseButtonPressed();
            else
                OnQuitButtonPressed();
        }
    }

    public void ShowPauseUI()
    {
        originObject.SetActive(true);
    }

    public void HidePauseUI()
    {
        originObject.SetActive(false);
    }

    public void OnPauseButtonPressed()
    {
        BoardManager.Paused = true;
        ShowPauseUI();
    }

    public void OnResumeButtonPressed()
    {
        BoardManager.Paused = false;
        HidePauseUI();
    }

    public void OnQuitButtonPressed()
    {
        SceneLoader.Instance.LoadScene(SceneLoader.MenuScene);
    }

    public void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            OnPauseButtonPressed();
        }
    }

}
