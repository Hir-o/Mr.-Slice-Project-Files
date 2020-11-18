using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class GameCanvas : MonoBehaviour
{
    private static GameCanvas _instance;
    public static  GameCanvas Instance => _instance;

    public                   TextMeshProUGUI TmpKeysNeeded;
    public                   RectTransform   RectTransitioner;
    public                   float           TransitionerDuration = .5f;
    [SerializeField] private Ease            _easeTransitioner    = Ease.OutQuad;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        Invoke(nameof(OpenTransitioner), TransitionerDuration);

        if (LevelRules.Instance.AllowKeys == false)
        {
            if (TmpKeysNeeded != null)
                TmpKeysNeeded.gameObject.SetActive(false);
        }
        else
            LevelRules.Instance.UpdateKeys();
    }

    public void UpdateKeysNeededText()
    {
        TmpKeysNeeded.text = "Keys Needed: " + LevelRules.Instance.KeysToCollect;
    }

    public void OpenTransitioner()
    {
        RectTransitioner.DOLocalMoveX(960, TransitionerDuration).SetEase(_easeTransitioner);
    }

    public void CloseTransistioner()
    {
        RectTransitioner.DOLocalMoveX(-960f, TransitionerDuration).SetEase(_easeTransitioner);
    }
}