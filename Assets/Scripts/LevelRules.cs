using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class LevelRules : MonoBehaviour
{
    private static LevelRules _instance;
    public static  LevelRules Instance => _instance;

    public bool AllowEnergy = true;
    public bool AllowKeys   = false;

    [ShowIf("AllowEnergy")]
    public int Energy = 3;

//    [HideInInspector]
    public int KeysToCollect;

    [ShowIf("AllowKeys")]
    public Door Door;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);

        KeysToCollect = 0;
    }

    public void UpdateKeys() { StartCoroutine(InitializeUpdate()); }

    private IEnumerator InitializeUpdate()
    {
        yield return new WaitForSeconds(.25f);

        KeysToCollect = FindObjectsOfType<Key>().Length;

        if (GameCanvas.Instance != null) GameCanvas.Instance.UpdateKeysNeededText();

        if (Door != null && KeysToCollect == 0)
        {
            if (GameCanvas.Instance != null)
            {
                GameCanvas.Instance.TmpKeysNeeded.DOFade(0f, 1f).SetEase(Ease.OutSine).OnComplete(DisableTmpKeysNeeded);
            }

            Door.UnlockDoor();
        }
    }

    private void DisableTmpKeysNeeded() { GameCanvas.Instance.TmpKeysNeeded.gameObject.SetActive(false); }
}