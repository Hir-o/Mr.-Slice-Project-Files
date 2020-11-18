using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int     Damage;
    public float   Speed;
    public Vector3 ShootDirection;

    public ParticleSystem VfxSplash;

    private Rigidbody2D _rigidbody2D;
    private Vector3     _direction;
    private Quaternion  _rotation;

    private float _angle, _destroyTime = 5f, _timer;

    private void Awake() { _rigidbody2D = GetComponent<Rigidbody2D>(); }

    private void Start()
    {
        _direction = ShootDirection - transform.position;

        _angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;

        _rotation = Quaternion.Euler(new Vector3(0, 0, _angle));

        transform.rotation = _rotation;

        _rigidbody2D.AddForce(transform.right * Speed, ForceMode2D.Impulse);
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer > _destroyTime) Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        Instantiate(VfxSplash, transform.position, Quaternion.identity);

        if (col.gameObject.CompareTag("Player"))
        {
            if (AudioController.Instance != null)
                AudioController.Instance.SfxHit();
            
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("Projectile"), true);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("Enemy"), true);
            Player.Instance.Die(true);
        }

        Destroy(gameObject);
    }
}