using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public int EnergyValue = 2;

    private bool _isCollected;

    [SerializeField] private float _yValue = 1f, _duration = .5f;

    public GameObject VfxCollect;

    private void Start()
    {
        transform.DOMoveY(transform.position.y + _yValue, _duration).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_isCollected) return;

        if (col.gameObject.CompareTag("Player"))
        {
            if (AudioController.Instance != null)
                AudioController.Instance.SfxCollect();
            
            _isCollected               =  true;
//            LevelRules.Instance.Energy += EnergyValue;
//            GameCanvas.Instance.UpdateEnergyText();

            if (VfxCollect != null) Instantiate(VfxCollect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}