using UnityEngine;
using UnityEngine.SceneManagement;

public class UnlockAllLevels : MonoBehaviour
{
    public void UnlockAll()
    {
        PlayerPrefs.SetInt("level", SceneManager.sceneCountInBuildSettings - 3);
        
        if (SceneManager.GetActiveScene().name == "menu")
            LevelController.Instance.RestartLevel();
    }
}
