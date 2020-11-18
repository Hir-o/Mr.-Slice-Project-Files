using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player _instance;
    public static  Player Instance => _instance;

    [HideInInspector]
    public Rigidbody2D Rigidbody;

    [HideInInspector]
    public CapsuleCollider2D CapsuleCollider;

    [HideInInspector]
    public LineDrawer LineDrawer;

    [HideInInspector]
    public Animator Animator;

    [HideInInspector]
    public MovementEventSubscriber MovementEventSubscriber;

    [HideInInspector]
    public HardLight2D HardLight2D;

    public TrailRenderer TrailRenderer;
    public Transform     TransformGfx;
    public GameObject    Explosion, LowExplosion, VfxDie, VfxHitHazard;

    private GameObject _explosion;

    public bool isDashing, isDead, hasHitHazard;

    [Header("Child Rigidbodies")]
    [SerializeField] private Rigidbody2D[] _childRigidbody2Ds;

    [Header("Child Colliders")]
    [SerializeField] private Collider2D[] _childCollider2Ds;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);

        Rigidbody               = GetComponent<Rigidbody2D>();
        CapsuleCollider         = GetComponent<CapsuleCollider2D>();
        Animator                = GetComponent<Animator>();
        MovementEventSubscriber = GetComponent<MovementEventSubscriber>();
        LineDrawer              = GetComponentInChildren<LineDrawer>();
        HardLight2D             = GetComponentInChildren<HardLight2D>();

        Slicer2DController.instance.trailRenderer = TrailRenderer;
    }

    private void Start()
    {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("Enemy"),      false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("Projectile"), false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("Obstacle"),
                                       false);
    }

    public void Die(bool isHitByProjectile)
    {
        if (LevelController.Instance.isLevelFinished) return;
        if (isDead) return;

        isDead = true;

        Rigidbody.bodyType = RigidbodyType2D.Static;
        MovementEventSubscriber.OnKilled();
        LineDrawer.gameObject.SetActive(false);
        Animator.enabled       = false;
        Rigidbody.constraints  = RigidbodyConstraints2D.None;
        Rigidbody.velocity     = Vector2.zero;
        Rigidbody.bodyType     = RigidbodyType2D.Dynamic;
        Rigidbody.gravityScale = 1f;

        if (VfxDie != null) Instantiate(VfxDie, transform.position, Quaternion.identity);

        SplitParts();

        if (isHitByProjectile)
        {
            _explosion = Instantiate(Explosion, transform.position, Quaternion.identity);
            Invoke(nameof(DisableExplosionObject), .1f);
        }
        else
        {
            _explosion = Instantiate(LowExplosion, transform.position, Quaternion.identity);
            Invoke(nameof(DisableExplosionObject), .1f);
        }

        if (LevelController.Instance != null) LevelController.Instance.RestartLevel();

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("Obstacle"),
                                       false);
    }

    private void DisableExplosionObject() { _explosion.SetActive(false); }

    private void SplitParts()
    {
        if (_childRigidbody2Ds.Length > 0)
            foreach (Rigidbody2D rg in _childRigidbody2Ds)
            {
                rg.transform.parent = null;
                rg.isKinematic      = false;
            }

        if (_childCollider2Ds.Length > 0)
            foreach (Collider2D col in _childCollider2Ds)
                col.enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (isDead) return;
        if (hasHitHazard) return;

        if (col.gameObject.CompareTag("Hazard"))
        {
            if (AudioController.Instance != null)
                AudioController.Instance.SfxHit();

            hasHitHazard = true;
            
            Invoke(nameof(DieAfterTimer), .2f);
            if (VfxHitHazard != null) Instantiate(VfxHitHazard, transform.position, Quaternion.identity);
        }
    }

    private void DieAfterTimer() { Die(false); }

    public void WalkSounds() // called from animation event
    {
        if (AudioController.Instance != null)
            AudioController.Instance.SfxWalk();
    }
}