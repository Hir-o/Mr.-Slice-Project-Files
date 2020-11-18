using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasCameraAttacher : MonoBehaviour
{
    private Canvas _canvas;

    private void Start()
    {
        _canvas = GetComponent<Canvas>();

        _canvas.worldCamera = ObjectHolder.Instance.MainCamera;
    }
}