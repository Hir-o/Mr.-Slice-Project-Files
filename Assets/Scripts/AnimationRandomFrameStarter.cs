using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRandomFrameStarter : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private float value;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        value = Random.Range(0, 1f);
        
        _animator.Play(0, -1, value);
    }
}
