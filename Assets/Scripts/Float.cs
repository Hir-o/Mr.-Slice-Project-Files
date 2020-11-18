using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Float : MonoBehaviour
{
    [SerializeField] private float _duration = 1f;
        
    private void Start()
    {
        transform.parent = null;
        transform.DOScale(new Vector3(2f, 2f, 2f),  0f);
        transform.DOMoveY(transform.position.y + .25f, _duration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
}
