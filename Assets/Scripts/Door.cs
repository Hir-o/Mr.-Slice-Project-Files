using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator _animator;

    private void Awake() { _animator = GetComponent<Animator>(); }

    private void Start()
    {
        ObjectHolder.Instance.Door = this;

        LevelRules.Instance.Door = this;
    }
    
    public void UnlockDoor() { transform.DOMoveY((transform.position.y + 50f), 4f).SetEase(Ease.OutSine); }
}