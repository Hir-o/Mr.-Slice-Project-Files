using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndieMarc.StealthLOS
{
    public class CharacterControls : MonoBehaviour
    {
        public float move_speed = 7f;
        public float move_accel = 40f;

        [Header("Ground")]
        public float gravity = 2f;
        public float ground_dist = 0.2f;
        public LayerMask ground_mask;

        [Header("Hide")]
        public bool can_hide = true;
        public KeyCode hide_key = KeyCode.LeftShift;

        private Vector3 current_move = Vector3.zero;
        private Rigidbody rigid;
        private Animator animator;
        private Collider collide;
        private VisionTarget vision_target;

        void Awake()
        {
            rigid = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            collide = GetComponentInChildren<Collider>();
            vision_target = GetComponent<VisionTarget>();
        }

        void FixedUpdate()
        {
            Vector3 move_dir = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
                move_dir += Vector3.forward;
            if (Input.GetKey(KeyCode.A))
                move_dir += Vector3.left;
            if (Input.GetKey(KeyCode.D))
                move_dir += Vector3.right;
            if (Input.GetKey(KeyCode.S))
                move_dir += Vector3.back;

            bool invisible = can_hide && Input.GetKey(hide_key);
            if (vision_target)
                vision_target.visible = !invisible;
            if (collide)
                collide.enabled = !invisible;

            if (invisible)
                move_dir = Vector3.zero;

            move_dir = move_dir.normalized * Mathf.Min(move_dir.magnitude, 1f);
            current_move = Vector3.MoveTowards(current_move, move_dir, move_accel * Time.fixedDeltaTime);
            rigid.velocity = current_move * move_speed;

            bool grounded = CheckIfGrounded();
            if (!grounded)
                rigid.velocity += Vector3.down * gravity;
            
            animator.SetBool("Move", current_move.magnitude > 0.1f);
            animator.SetBool("Hide", invisible);
        }

        public bool CheckIfGrounded()
        {
            Vector3 origin = transform.position + Vector3.up * ground_dist * 0.5f;
            return RaycastObstacle(origin + Vector3.forward * 0.5f, Vector3.down * ground_dist)
                || RaycastObstacle(origin + Vector3.back * 0.5f, Vector3.down * ground_dist)
                || RaycastObstacle(origin + Vector3.left * 0.5f, Vector3.down * ground_dist)
                || RaycastObstacle(origin + Vector3.right * 0.5f, Vector3.down * ground_dist);
        }

        public bool RaycastObstacle(Vector3 origin, Vector3 dir)
        {
            RaycastHit hit;
            return Physics.Raycast(new Ray(origin, dir.normalized), out hit, dir.magnitude, ground_mask.value);
        }
    }

}
