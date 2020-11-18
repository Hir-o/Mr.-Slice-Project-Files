using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using IndieMarc.StealthLOS;
using UnityEngine;

public class EnemyDie : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _bottomTrigger;

    [SerializeField] private SpriteRenderer[] _spriteRenderers;
    [SerializeField] private SpriteRenderer   _mask;

    [SerializeField] private Color _newColor;
    [SerializeField] private Color _newMaskColor;

    private Enemy2D           _enemy2D;
    private EnemyPatrol2D     _enemyPatrol2D;
    private EnemyLOS2D        _enemyLos2D;
    private EnemyFollow2D     _enemyFollow2D;
    private EnemyDemo2D       _enemyDemo2D;
    private EnemyWeapon       _enemyWeapon;
    private Animator          _animator;
    private Rigidbody2D       _rigidbody2D;
    private PolygonCollider2D _polygonCollider2D;

    [SerializeField] private float _fallTimer = .8f, _timer;
    [SerializeField] private bool  _isFalling,       _dieInContact, _fallForceApplied;

    private float _sign;

    public void EnableBottomTrigger() { _bottomTrigger.enabled = true; }

    private void Awake()
    {
        _enemy2D           = GetComponent<Enemy2D>();
        _enemyPatrol2D     = GetComponent<EnemyPatrol2D>();
        _enemyLos2D        = GetComponent<EnemyLOS2D>();
        _enemyFollow2D     = GetComponent<EnemyFollow2D>();
        _enemyDemo2D       = GetComponent<EnemyDemo2D>();
        _enemyWeapon       = GetComponent<EnemyWeapon>();
        _animator          = GetComponent<Animator>();
        _rigidbody2D       = GetComponent<Rigidbody2D>();
        _polygonCollider2D = GetComponent<PolygonCollider2D>();
    }

    private void Update()
    {
        if (_dieInContact) return;

        if (_isFalling)
        {
            _timer += Time.deltaTime;

            var rotationVector = transform.rotation.eulerAngles;
            rotationVector.x   =  0f;
            rotationVector.y   =  0f;
            rotationVector.z   += 20f * Time.deltaTime;
            transform.rotation =  Quaternion.Euler(rotationVector);

            if (_timer > _fallTimer)
            {
                _dieInContact          = true;
                _bottomTrigger.enabled = true;
            }
        }
    }

    private void Die()
    {
        if (_spriteRenderers.Length <= 0) return;

        _enemy2D.Kill();
        _enemy2D.enabled       = false;
        _enemyPatrol2D.enabled = false;
        _enemyLos2D.enabled    = false;
        _enemyFollow2D.enabled = false;
        _enemyDemo2D.enabled   = false;
        _enemyWeapon.enabled   = false;
        _animator.enabled      = false;

        _rigidbody2D.constraints = RigidbodyConstraints2D.None;

        foreach (var spr in _spriteRenderers) { spr.DOColor(ColorController.Instance.EnemyColor, 1f); }

        _mask.DOColor(_newMaskColor, 1f);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Obstacle")) Die();
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (_dieInContact) return;

        if (col.gameObject.layer == LayerMask.NameToLayer("Obstacle")) { _isFalling = true; }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            if (_fallForceApplied == false && _dieInContact) _fallForceApplied = true;

            if (_dieInContact) return;

            _isFalling = false;
            _timer     = 0f;
        }
    }
}