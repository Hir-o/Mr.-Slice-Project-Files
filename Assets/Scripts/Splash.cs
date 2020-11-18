using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class Splash : MonoBehaviour
{
    public Image imgSplash;
    public float fadeTimer = 1f;

    public bool isCoolmathSplash;

    [ShowIf("isCoolmathSplash")]
    public TextMeshProUGUI btnStart;

    private void Start()
    {
        if (isCoolmathSplash)
        {
            if (imgSplash != null && btnStart != null)
            {
                btnStart.DOFontSize(160f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                    
                btnStart.DOFade(0f, 0f);
                btnStart.DOFade(1f, fadeTimer / 2f).SetEase(Ease.OutSine);
                imgSplash.DOFade(0f, 0f);
                imgSplash.DOFade(1f, fadeTimer / 2f).SetEase(Ease.OutSine);
            }
            return;
        }
        
        if (imgSplash != null)
        {
            imgSplash.DOFade(0f, 0f);
            imgSplash.DOFade(1f, fadeTimer / 2f).SetEase(Ease.OutSine).OnComplete(FadeOut);
        }
        else
        {
            NextScene();
        }
    }

    private void FadeOut()
    {
        imgSplash.DOFade(0f, fadeTimer / 2f).SetEase(Ease.OutSine).OnComplete(NextScene);
    }

    private void NextScene()
    {
        LevelController.Instance.LoadNextLevel();

        if (isCoolmathSplash) return;
        
        if (AudioController.Instance != null) AudioController.Instance.PlayMusic();
    }

    public void StartButtonClick()
    {
        btnStart.GetComponent<Button>().interactable = false;
        
        imgSplash.DOFade(0f, fadeTimer / 2f).SetEase(Ease.OutSine).OnComplete(NextScene);
        btnStart.DOFade(0f, fadeTimer / 2f).SetEase(Ease.OutSine);
        
        #if UNITY_WEBGL
        StartGameEvent();
        #endif
    }
    
    [DllImport("__Internal")]
    private static extern void StartGameEvent();
}
