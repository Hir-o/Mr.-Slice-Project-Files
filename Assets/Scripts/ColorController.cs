using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorController : MonoBehaviour
{
    private static ColorController _instance;
    public static  ColorController Instance => _instance;

    public Color EnemyColor;
    public Color CameraColor;
    public Color HardLightColor;
    public Color GroundColor;
    public float EnemyVisionAlpha = .4f;

    public Material VisionMat2D;

    [SerializeField] private Color _tmpColor;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ObjectHolder.Instance.MainCamera.backgroundColor = CameraColor;

        if (Player.Instance != null) Player.Instance.HardLight2D.Color = HardLightColor;

        _tmpColor   = HardLightColor;
        _tmpColor.a = EnemyVisionAlpha;

        VisionMat2D.SetColor("_Color", _tmpColor);
    }
}