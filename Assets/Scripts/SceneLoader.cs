using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public const string SplashScene = "Splash";
    public const string GameScene = "Game";
    public const string MenuScene = "Menu";
    public const string LoadingScene = "Loading";

    public static bool isLoading = false;
    public static float loadingProgress = 0.0f;
    private static Action OnLoadComplete;

    private static SceneLoader _instance;
    public static SceneLoader Instance
    {
        get 
        { 
            if (_instance == null)
            {
                GameObject newObject = new GameObject("SceneLoader(Instantiated)");
                _instance = newObject.AddComponent<SceneLoader>();
                DontDestroyOnLoad(newObject);
            }

            return _instance;
        }
    }

    public void LoadScene(string sceneName, Action onLoadComplete = null)
    {
        isLoading = true;
        loadingProgress = 0.0f;
        OnLoadComplete = onLoadComplete;

        StartCoroutine(_LoadScene(sceneName));
    }

    public IEnumerator _LoadScene(string sceneName)
    {
        //Load loading scene
        string activeScene = SceneManager.GetActiveScene().name;
        var loadingSceneUp = SceneManager.LoadSceneAsync(LoadingScene, LoadSceneMode.Additive);
        loadingSceneUp.allowSceneActivation = true;
        yield return loadingSceneUp;

        //Unload Active Scene
        var activeSceneDown = SceneManager.UnloadSceneAsync(activeScene);
        yield return activeSceneDown;

        yield return new WaitForSeconds(1.0f);

        //Load scene ya want 
        var newSceneUp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return newSceneUp;

        //Unload Loading scene
        var loadingScenedown = SceneManager.UnloadSceneAsync(LoadingScene);
        yield return loadingScenedown;

        isLoading = false;
        if (OnLoadComplete != null)
            OnLoadComplete.Invoke();
    }

}
