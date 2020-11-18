using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public GameObject VfxCollect;

    private bool _isTaken;
    
    private void Start() { LevelRules.Instance.KeysToCollect++; }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_isTaken) return;
        
        if (col.gameObject.CompareTag("Player"))
        {
            if (AudioController.Instance != null)
                AudioController.Instance.SfxCollect();
            
            _isTaken = true;
            LevelRules.Instance.UpdateKeys();
            Physics2D.IgnoreCollision(col.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>());
            if (VfxCollect != null) Instantiate(VfxCollect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}