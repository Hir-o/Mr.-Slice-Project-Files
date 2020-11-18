using System.Collections;
using System.Collections.Generic;
using IndieMarc.StealthLOS;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public Transform  TransformWeapon, TransformProjectileSpawnPoint;
    public Projectile Projectile;

    private EnemyLOS2D   _enemyLos2D;
    private VisionTarget _target;
    private Vector3      _direction;
    private Quaternion   _rotation;
    private Projectile   _projectile;

    [SerializeField] private float _weaponRotateSpeed = 5f, _faceAtAngle = 10f, _projectileSpeed = 30f;
    [SerializeField] private int   _projectileDamage  = 1;
    [SerializeField] private float _shootInterval     = .5f;

    private float _angle, _shootTimer;
    private bool  _canShoot = true;

    private void Awake() { _enemyLos2D = GetComponent<EnemyLOS2D>(); }

    private void Update()
    {
        if (_canShoot == false)
        {
            _shootTimer += Time.deltaTime;

            if (_shootTimer > _shootInterval)
            {
                _shootTimer = 0f;
                _canShoot   = true;
            }
        }

        if (_enemyLos2D != null)
        {
            _target = _enemyLos2D.GetSeenCharacter();

            if (_target != null && _enemyLos2D.CanSeeVisionTarget(_target))
            {
                _direction = _target.transform.position - transform.position;

                if (transform.localScale.x > 0)
                    _angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;

                else if (transform.localScale.x < 0)
                    _angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg + 180f;

                _rotation = Quaternion.Euler(new Vector3(0, 0, _angle));

                TransformWeapon.rotation =
                    Quaternion.Slerp(TransformWeapon.rotation, _rotation, _weaponRotateSpeed * Time.deltaTime);

                if (Vector2.Angle(TransformWeapon.right, TransformWeapon.position - _target.transform.position) >
                    _faceAtAngle) { Shoot(); }
            }
            else
            {
                TransformWeapon.rotation =
                    Quaternion.Slerp(TransformWeapon.rotation, new Quaternion(0f, 0f, 0f, 0f),
                                     _weaponRotateSpeed * Time.deltaTime);
            }
        }
    }

    private void Shoot()
    {
        if (LevelController.Instance.isLevelFinished) return;
        if (Player.Instance.isDead) return;
        if (_enemyLos2D.GetState() != EnemyLOS2DState.Chase) return;

        if (_canShoot)
        {
            _canShoot = false;

            _projectile =
                Instantiate(Projectile, TransformProjectileSpawnPoint.position, Quaternion.identity);

            if (AudioController.Instance != null) AudioController.Instance.SfxBullet();

            _projectile.Speed          = _projectileSpeed;
            _projectile.Damage         = _projectileDamage;
            _projectile.ShootDirection = Player.Instance.transform.position;
        }
    }
}