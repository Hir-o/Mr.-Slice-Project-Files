using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class LevelController : MonoBehaviour
{
    private static LevelController _instance;
    public static  LevelController Instance => _instance;

    public float LevelRestartTimer = 2f;

    public bool isLevelFinished;

    private bool _isTransitioningToLevel;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex >= 4)
        {
            if (PlayerPrefs.HasKey("level"))
            {
                if (PlayerPrefs.GetInt("level") < SceneManager.GetActiveScene().buildIndex - 3)
                    PlayerPrefs.SetInt("level", SceneManager.GetActiveScene().buildIndex - 3);
            }
            else
                PlayerPrefs.SetInt("level", SceneManager.GetActiveScene().buildIndex - 3);
        }
    }

    public void LoadLevel(string level)
    {
        if (_isTransitioningToLevel) return;

        _isTransitioningToLevel = true;

        StartCoroutine(InitLevelChange(level, .25f));
    }

    public void LoadNextLevel()
    {
        if (isLevelFinished) return;
        
        isLevelFinished = true;

        if (Player.Instance != null) Player.Instance.MovementEventSubscriber.DisableControls();

        int levelIndex = SceneManager.GetActiveScene().buildIndex;

        levelIndex++;

//        if (levelIndex > 3)
//        {
//            #if UNITY_WEBGL
//            StartLevelEvent(levelIndex - 3);
//            #endif
//        }

        if (levelIndex <= SceneManager.sceneCountInBuildSettings - 1)
            StartCoroutine(InitLevelChange(levelIndex, LevelRestartTimer));
        else
            StartCoroutine(InitLevelChange(2, LevelRestartTimer));
    }

    public void RestartLevel()
    {
//        #if UNITY_WEBGL
//        ReplayEvent();
//        #endif
        
        int levelIndex = SceneManager.GetActiveScene().buildIndex;

        StartCoroutine(InitLevelChange(levelIndex, LevelRestartTimer));
    }

    public void RestartLevelImmediately()
    {
//        #if UNITY_WEBGL
//        ReplayEvent();
//        #endif
        
        int levelIndex = SceneManager.GetActiveScene().buildIndex;

        StartCoroutine(InitLevelChange(levelIndex, .25f));
    }

    private IEnumerator InitLevelChange(int index, float timer)
    {
        yield return new WaitForSeconds(timer);

        GameCanvas.Instance.CloseTransistioner();

        yield return new WaitForSeconds(GameCanvas.Instance.TransitionerDuration);

        SceneManager.LoadScene(index);
    }

    private IEnumerator InitLevelChange(string level, float timer)
    {
        yield return new WaitForSeconds(timer);

        GameCanvas.Instance.CloseTransistioner();

        yield return new WaitForSeconds(GameCanvas.Instance.TransitionerDuration);

        SceneManager.LoadScene(level);
    }

    [Button]
    public void ResetLevelProgress()
    {
        if (PlayerPrefs.HasKey("level")) { PlayerPrefs.DeleteKey("level"); }
    }

//    [DllImport("__Internal")]
//    private static extern void StartLevelEvent(int level);
//    
//    [DllImport("__Internal")]
//    private static extern void ReplayEvent();
}