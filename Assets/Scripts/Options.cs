using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public Button btnRestart, btnAudio, btnExit;

    public Image  imgSound;
    public Sprite sprSoundOn, sprSoundOff;

    private Canvas _canvas;

    public bool enableRestartOption = true, enableAudioOption = true, enableExitOption = true;

    private bool isAudioOn;

    private void Start()
    {
        _canvas = GetComponent<Canvas>();

        _canvas.sortingOrder = 8;
            
        if (PlayerPrefs.HasKey("volume"))
        {
            AudioListener.volume = PlayerPrefs.GetFloat("volume");
            
            if (PlayerPrefs.GetFloat("volume") > 0f)
                imgSound.sprite = sprSoundOn;
            else
                imgSound.sprite = sprSoundOff;
        }
        else { imgSound.sprite = sprSoundOn; }

        if (enableRestartOption == false) btnRestart.gameObject.SetActive(false);

        if (enableAudioOption == false) btnAudio.gameObject.SetActive(false);

        if (enableExitOption == false) btnExit.gameObject.SetActive(false);
    }

    public void Restart()
    {
        if (AudioController.Instance != null)
            AudioController.Instance.SfxClick();
        
        if (LevelController.Instance != null) LevelController.Instance.RestartLevelImmediately();
    }

    public void ToggleAudio()
    {
        if (AudioController.Instance != null)
            AudioController.Instance.SfxClick();
        
        if (AudioListener.volume > 0f)
        {
            AudioListener.volume = 0f;
            PlayerPrefs.SetFloat("volume", 0f);
            imgSound.sprite = sprSoundOff;
        }
        else
        {
            AudioListener.volume = 1f;
            PlayerPrefs.SetFloat("volume", 1f);
            imgSound.sprite = sprSoundOn;
        }
    }

    public void Exit()
    {
        if (AudioController.Instance != null)
            AudioController.Instance.SfxClick();
        
        if (LevelController.Instance != null) LevelController.Instance.LoadLevel("_title screen");
    }
}