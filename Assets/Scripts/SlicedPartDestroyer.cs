using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicedPartDestroyer : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Obstacle")) col.gameObject.SetActive(false);
    }
}