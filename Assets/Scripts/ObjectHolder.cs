using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
    private static ObjectHolder _instance;
    public static  ObjectHolder Instance => _instance;

    [HideInInspector]
    public Camera MainCamera;

    [HideInInspector]
    public Door Door;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);

        MainCamera = Camera.main;
    }
}