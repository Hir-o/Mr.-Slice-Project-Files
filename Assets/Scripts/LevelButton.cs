using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class LevelButton : MonoBehaviour, IPointerDownHandler
{
    public string levelName = "lvl_01";

    private int _index;

    private Char[] _level;
    private string _number;

    private Button          _button;
    private TextMeshProUGUI _tmpNumber;

    private void Awake()
    {
        _button    = GetComponent<Button>();
        _tmpNumber = GetComponentInChildren<TextMeshProUGUI>();

        _level = levelName.ToCharArray();

        for (int i = 0; i < _level.Length; i++)
        {
            if (i > 3) _number += _level[i];
        }

        Int32.TryParse(_number, out _index);

        _tmpNumber.text = _index.ToString();
    }

    private void Start()
    {
        if (_index != 1)
        {
            if (PlayerPrefs.HasKey("level"))
            {
                if (PlayerPrefs.GetInt("level") >= _index)
                    _button.interactable = true;
                else
                    _button.interactable = false;
            }
            else
                _button.interactable = false;
        }
        
        if (_button.interactable == false) _tmpNumber.DOFade(.5f, 0f);
    }

    public void LoadLevel()
    {
//        #if UNITY_WEBGL
//        StartLevelEvent(_index);
//        #endif
        
        if (_button.interactable == false) return;

        LevelController.Instance.LoadLevel(levelName);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (AudioController.Instance != null)
            AudioController.Instance.SfxClick();
        
        LoadLevel();
    }
    
//    [DllImport("__Internal")]
//    private static extern void StartLevelEvent(int level);
}