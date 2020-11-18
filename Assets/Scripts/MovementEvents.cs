using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MovementEvents : MonoBehaviour
{
    private static MovementEvents _instance;
    public static  MovementEvents Instance => _instance;

    public event EventHandler<OnJumpEventArgs> OnJump;
    public event EventHandler<OnMoveEventArgs> OnMove;
    public event EventHandler<OnDashEventArgs> OnDash;

    [SerializeField]
    private KeyCode _positiveLeft, _negativeLeft, _positiveRight, _negativeRight, _positiveJump, _negativeJump;

    public class OnJumpEventArgs : EventArgs
    {
    }

    public class OnMoveEventArgs : EventArgs
    {
        public float MoveDirection;
    }

    public class OnDashEventArgs : EventArgs
    {
        public Vector2 MouseClickPosition;
    }

    private float _movementSpeed;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (ObjectHolder.Instance.MainCamera.WorldToScreenPoint(Player.Instance.transform.position).y < -20f)
            Player.Instance.Die(false);

//        if (ObjectHolder.Instance.MainCamera.WorldToScreenPoint(Player.Instance.transform.position).x >
//            Screen.width + 19f)
//        {
//            Player.Instance.Rigidbody.bodyType = RigidbodyType2D.Static;
//            LevelController.Instance.LoadNextLevel();
//        }

        _movementSpeed = Input.GetAxis("Horizontal");

        if (_movementSpeed != 0f)
            OnMove?.Invoke(this, new OnMoveEventArgs {MoveDirection = _movementSpeed});
        else
            Player.Instance.Rigidbody.velocity = new Vector2(0f, Player.Instance.Rigidbody.velocity.y);
        
        if (Input.GetKeyDown(_positiveJump) || Input.GetKeyDown(_negativeJump))
            OnJump?.Invoke(this, new OnJumpEventArgs());

        if (Input.GetMouseButtonDown(0))
            OnDash?.Invoke(this,
                           new OnDashEventArgs
                           {
                               MouseClickPosition =
                                   ObjectHolder.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition)
                           });

        if (_movementSpeed == 0f)
            AnimatorParam.SetParams(Player.Instance.Animator, AnimatorParams.AnimParamType.Bool,
                                    AnimatorParams.PLAYER_RUNNING, false);

        if (Player.Instance.Rigidbody.velocity.y < 0f && Player.Instance.MovementEventSubscriber.IsGrounded() == false)
        {
            AnimatorParam.SetParams(Player.Instance.Animator, AnimatorParams.AnimParamType.Bool,
                                    AnimatorParams.PLAYER_RUNNING, false);
            AnimatorParam.SetParams(Player.Instance.Animator, AnimatorParams.AnimParamType.Bool,
                                    AnimatorParams.PLAYER_JUMPING, false);
            AnimatorParam.SetParams(Player.Instance.Animator, AnimatorParams.AnimParamType.Bool,
                                    AnimatorParams.PLAYER_DASHING, false);
            AnimatorParam.SetParams(Player.Instance.Animator, AnimatorParams.AnimParamType.Bool,
                                    AnimatorParams.PLAYER_FALLING, true);
            AnimatorParam.SetParams(Player.Instance.Animator, AnimatorParams.AnimParamType.Bool,
                                    AnimatorParams.PLAYER_ENDDASH, false);
        }
        else
        {
            if (Player.Instance.Animator.GetBool(AnimatorParams.PLAYER_FALLING))
                AnimatorParam.SetParams(Player.Instance.Animator, AnimatorParams.AnimParamType.Bool,
                                        AnimatorParams.PLAYER_FALLING, false);
        }
    }
}