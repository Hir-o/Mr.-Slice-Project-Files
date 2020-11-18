using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyColor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] _sprites;
    private void Start()
    {
        foreach (var spr in _sprites) { spr.color = ColorController.Instance.EnemyColor; }
    }
}
