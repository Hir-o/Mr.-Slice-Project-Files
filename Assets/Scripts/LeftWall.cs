using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftWall : MonoBehaviour
{
    private bool _isTransitioning;
    
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_isTransitioning) return;
        
        if (col.gameObject.CompareTag("Player"))
        {
            Player.Instance.Rigidbody.bodyType = RigidbodyType2D.Static;
            LevelController.Instance.LoadNextLevel();
        }
    }
}
