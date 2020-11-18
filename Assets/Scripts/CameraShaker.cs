using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    private static CameraShaker _instance;
    public static  CameraShaker Instance => _instance;

    [SerializeField] private float _duration, _strength, _randomness;
    [SerializeField] private int   _vibrato;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    public void Shake()
    {
        ObjectHolder.Instance.MainCamera.DOShakePosition(_duration, _strength, _vibrato, _randomness);
    }
}