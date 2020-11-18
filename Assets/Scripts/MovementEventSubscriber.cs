using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovementEventSubscriber : MonoBehaviour
{
    private Player            _player;
    private Rigidbody2D       _playerRigidbody;
    private CapsuleCollider2D _capsuleCollider;
    private RaycastHit2D      _hit, _dashHit;
    private LineDrawer        _lineDrawer;
    private Animator          _animator;

    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private Ease      _easeDash = Ease.OutQuad;

    [SerializeField] private TrailRenderer _trailDash;

    public List<Vector2> DashDestinations = new List<Vector2>();

    private Vector2 _firstDestination;

    public float Jumpvelocity = 10f, MoveVelocity = 10f, DashVelocity = 10f;

    private float _extraHeight = .1f, _distance; // the size of the rays coming out of the capsule collider

    private bool _isDashingIntoWall;

    private void Start()
    {
        _player          = Player.Instance;
        _playerRigidbody = _player.Rigidbody;
        _capsuleCollider = _player.CapsuleCollider;
        _lineDrawer      = _player.LineDrawer;
        _animator        = _player.Animator;

        MovementEvents.Instance.OnJump += MovementEvents_OnJump;
        MovementEvents.Instance.OnMove += MovementEvents_OnMove;
        MovementEvents.Instance.OnDash += MovementEvents_OnDash;
    }

    private void MovementEvents_OnJump(object sender, MovementEvents.OnJumpEventArgs e)
    {
        if (IsDashing()) return;

        if (IsGrounded())
        {
            if (AudioController.Instance != null)
                AudioController.Instance.SfxJump();
            
            _playerRigidbody.velocity = Vector3.up * Jumpvelocity;
        }

        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_RUNNING, false);
        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_JUMPING, true);
        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_DASHING, false);
        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_FALLING, false);
        AnimatorParam.SetParams(Player.Instance.Animator, AnimatorParams.AnimParamType.Bool,
                                AnimatorParams.PLAYER_ENDDASH, false);
    }

    private void MovementEvents_OnMove(object sender, MovementEvents.OnMoveEventArgs e)
    {
        if (IsDashing()) return;

        _playerRigidbody.velocity = new Vector2(e.MoveDirection * MoveVelocity, _playerRigidbody.velocity.y);

        if (Player.Instance.Rigidbody.velocity.x > 0)
        {
            Player.Instance.TransformGfx.localScale = new Vector3(Mathf.Abs(Player.Instance.TransformGfx.localScale.x),
                                                                  Player.Instance.TransformGfx.localScale.y,
                                                                  Player.Instance.TransformGfx.localScale.z);
        }
        else if (Player.Instance.Rigidbody.velocity.x < 0)
        {
            Player.Instance.TransformGfx.localScale = new Vector3(-Mathf.Abs(Player.Instance.TransformGfx.localScale.x),
                                                                  Player.Instance.TransformGfx.localScale.y,
                                                                  Player.Instance.TransformGfx.localScale.z);
        }

        if (_animator.GetBool(AnimatorParams.PLAYER_RUNNING)) return;
        if (_animator.GetBool(AnimatorParams.PLAYER_FALLING)) return;
        if (_animator.GetBool(AnimatorParams.PLAYER_JUMPING)) return;

        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_RUNNING, true);
        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_JUMPING, false);
        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_DASHING, false);
        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_FALLING, false);
        AnimatorParam.SetParams(Player.Instance.Animator, AnimatorParams.AnimParamType.Bool,
                                AnimatorParams.PLAYER_ENDDASH, false);
    }

    private void MovementEvents_OnDash(object sender, MovementEvents.OnDashEventArgs e)
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return; }
        
        _dashHit = Physics2D.Raycast(e.MouseClickPosition, Vector2.zero);

        if (_dashHit.collider == null)
        {
            _playerRigidbody.velocity = Vector2.zero;

            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("Obstacle"),
                                           true);

            Player.Instance.isDashing     = true;
            _playerRigidbody.gravityScale = 0f;

            _lineDrawer.SetPosition(DashDestinations, e.MouseClickPosition);

            DashDestinations.Add(e.MouseClickPosition);

            if (DOTween.IsTweening(transform)) return;

            ExecuteDashing();
        }
    }

    private void ExecuteDashing()
    {
        if (AudioController.Instance != null)
            AudioController.Instance.SfxDash();
        
        _firstDestination = DashDestinations.First();

        _distance = Vector2.Distance(transform.position, _firstDestination);

        transform.DOMove(_firstDestination, _distance / DashVelocity)
                 .OnComplete(() => ResetIsDashing(_firstDestination)).SetEase(_easeDash);

        Invoke(nameof(ResetDashingAnimation), _distance / (DashVelocity + 50));

        if (transform.position.x < _firstDestination.x)
        {
            Player.Instance.TransformGfx.localScale = new Vector3(Mathf.Abs(Player.Instance.TransformGfx.localScale.x),
                                                                  Player.Instance.TransformGfx.localScale.y,
                                                                  Player.Instance.TransformGfx.localScale.z);
        }
        else if (transform.position.x > _firstDestination.x)
        {
            Player.Instance.TransformGfx.localScale = new Vector3(-Mathf.Abs(Player.Instance.TransformGfx.localScale.x),
                                                                  Player.Instance.TransformGfx.localScale.y,
                                                                  Player.Instance.TransformGfx.localScale.z);
        }

        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_RUNNING, false);
        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_JUMPING, false);
        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_DASHING, true);
        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_FALLING, false);
        AnimatorParam.SetParams(Player.Instance.Animator, AnimatorParams.AnimParamType.Bool,
                                AnimatorParams.PLAYER_ENDDASH, false);

        _trailDash.emitting = true;
        
        CameraShaker.Instance.Shake();
    }

    private void ResetIsDashing(Vector2 destination)
    {
        DashDestinations.Remove(destination);

        if (DashDestinations.Count <= 0)
        {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("Obstacle"),
                                           false);

            _trailDash.emitting           = false;
            Player.Instance.isDashing     = false;
            _playerRigidbody.gravityScale = 1f;
        }
        else { ExecuteDashing(); }
    }

    private void ResetDashingAnimation()
    {
        AnimatorParam.SetParams(Player.Instance.Animator, AnimatorParams.AnimParamType.Bool,
                                AnimatorParams.PLAYER_ENDDASH, true);
        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_DASHING, false);
    }

    public bool IsGrounded()
    {
        _hit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.bounds.size,
                                     CapsuleDirection2D.Vertical, 0f, Vector2.down, _extraHeight, _groundLayerMask);

        if (_hit.collider != null) return true;

        return false;
    }

    private bool IsDashing() { return Player.Instance.isDashing; }

    public void OnKilled()
    {
        DisableControls();

        DOTween.KillAll();

        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_RUNNING, false);
        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_JUMPING, false);
        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_DASHING, false);
        AnimatorParam.SetParams(_animator, AnimatorParams.AnimParamType.Bool, AnimatorParams.PLAYER_FALLING, false);
        AnimatorParam.SetParams(Player.Instance.Animator, AnimatorParams.AnimParamType.Bool,
                                AnimatorParams.PLAYER_ENDDASH, false);
    }

    public void DisableControls()
    {
        MovementEvents.Instance.OnJump -= MovementEvents_OnJump;
        MovementEvents.Instance.OnMove -= MovementEvents_OnMove;
        MovementEvents.Instance.OnDash -= MovementEvents_OnDash;
    }
}