using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

namespace IndieMarc.StealthLOS
{
    public class Enemy2D : MonoBehaviour
    {
        public float move_speed = 2f;
        public float rotate_speed = 120f;
        public float fall_speed = 5f;
        public float fall_accel = 10f;
        public LayerMask obstacle_mask = ~(0); //All bit 1
        public float ground_raycast_dist = 0.1f;

        public UnityAction onDeath;

        private Rigidbody2D rigid;
        private CapsuleCollider2D capsule_coll;
        private ContactFilter2D contact_filter;
        private Vector3 start_scale;
        private Vector3 move_vect;
        private Vector3 face_vect;
        private Quaternion face_rot;

        private Vector2 current_target;
        private float current_mult = 1f;
        private Vector3 current_rot_target;
        private float current_rot_mult = 1f;
        private float fall_value = 0f;

        private static List<Enemy2D> enemy_list = new List<Enemy2D>();

        public bool IsDead;

        private void Awake()
        {
            enemy_list.Add(this);
            rigid = GetComponent<Rigidbody2D>();
            capsule_coll = GetComponentInChildren<CapsuleCollider2D>();
            move_vect = Vector3.zero;
            current_target = transform.position;
            current_rot_target = transform.position + transform.forward;
            start_scale = transform.localScale;
            face_rot = Quaternion.identity;
            move_vect = Mathf.Sign(start_scale.x) > 0f ? Vector3.right : Vector3.left;

            contact_filter = new ContactFilter2D();
            contact_filter.layerMask = obstacle_mask;
            contact_filter.useLayerMask = true;
            contact_filter.useTriggers = false;
        }

        private void OnDestroy()
        {
            enemy_list.Remove(this);
        }

        void Start()
        {
            
        }

        private void FixedUpdate()
        {
            Vector2 dist_vect = (current_target - rigid.position);
            move_vect = dist_vect.normalized * move_speed * current_mult * Mathf.Min(dist_vect.magnitude, 1f);

            bool grounded = DetectObstacle(Vector3.down);

            if (fall_speed > 0.1f)
            {
                if (grounded)
                    fall_value = 0f;
                else
                    fall_value += fall_accel * Time.deltaTime;
                fall_value = Mathf.Clamp(fall_value, 0f, fall_speed);
                move_vect.y = -fall_value;
                rigid.velocity = move_vect;
            }
        }

        private void Update()
        {
            Vector3 dir = current_rot_target - transform.position;
            dir.z = 0f;
            Debug.DrawRay(transform.position, dir);

            //Side
            if (Mathf.Abs(dir.x) > 0.1f)
            {
                float side = (dir.x < 0f) ? -1f : 1f;
                transform.localScale = new Vector3(Mathf.Abs(start_scale.x) * side, start_scale.y, start_scale.z);
            }
            
            //Vision angle
            if (dir.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x * GetSide()) * Mathf.Rad2Deg;
                Quaternion target = Quaternion.AngleAxis(angle, Vector3.forward);
                face_rot = Quaternion.RotateTowards(face_rot, target, rotate_speed * current_rot_mult * Time.deltaTime);
                face_vect = face_rot * Vector3.right;
                face_vect.x = face_vect.x * GetSide();
                face_vect.Normalize();
            }
        }

        private bool DetectObstacle(Vector3 dir)
        {
            bool grounded = false;
            Vector2[] raycastPositions = new Vector2[3];

            Vector2 raycast_start = rigid.position;
            Vector2 orientation = dir.normalized;
            bool is_up_down = Mathf.Abs(orientation.y) > Mathf.Abs(orientation.x);
            Vector2 perp_ori = is_up_down ? Vector2.right : Vector2.up;
            float radius = GetSize().x * 0.5f;

            if (capsule_coll != null && is_up_down)
            {
                //Adapt raycast to collider
                Vector2 raycast_offset = capsule_coll.offset + orientation * Mathf.Abs(capsule_coll.size.y * 0.5f - capsule_coll.size.x * 0.5f);
                raycast_start = rigid.position + raycast_offset * capsule_coll.transform.lossyScale.y;
            }

            float ray_size = radius + ground_raycast_dist;
            raycastPositions[0] = raycast_start - perp_ori * radius / 2f;
            raycastPositions[1] = raycast_start;
            raycastPositions[2] = raycast_start + perp_ori * radius / 2f;


            for (int i = 0; i < raycastPositions.Length; i++)
            {
                Debug.DrawRay(raycastPositions[i], orientation * ray_size, Color.green);
                if (RaycastObstacle(raycastPositions[i], orientation * ray_size))
                    grounded = true;
            }
            return grounded;
        }

        public bool RaycastObstacle(Vector2 pos, Vector2 dir)
        {
            RaycastHit2D[] hitBuffer = new RaycastHit2D[5];
            Physics2D.Raycast(pos, dir.normalized, contact_filter, hitBuffer, dir.magnitude);
            for (int j = 0; j < hitBuffer.Length; j++)
            {
                if (hitBuffer[j].collider != null && hitBuffer[j].collider != capsule_coll && !hitBuffer[j].collider.isTrigger)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckFronted(Vector3 dir)
        {
            return RaycastObstacle(transform.position, dir);
        }

        public Vector2 GetSize()
        {
            if (capsule_coll != null)
                return new Vector2(Mathf.Abs(capsule_coll.transform.lossyScale.x) * capsule_coll.size.x, Mathf.Abs(capsule_coll.transform.lossyScale.y) * capsule_coll.size.y);
            return new Vector2(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        }

        public void MoveTo(Vector3 pos, float speed_mult = 1f)
        {
            current_target = pos;
            current_mult = speed_mult;
        }

        public void FaceToward(Vector3 pos, float speed_mult = 1f)
        {
            current_rot_target = pos;
            current_rot_mult = speed_mult;
        }
        
        public void Kill()
        {
            if (onDeath != null)
                onDeath.Invoke();

            IsDead = true;

            //Destroy(gameObject);
        }
        
        public Vector3 GetMove()
        {
            return move_vect;
        }

        public Vector3 GetFacing()
        {
            return face_vect;
        }

        public float GetFacingAngle()
        {
            return Mathf.Atan2(face_vect.y, face_vect.x * GetSide()) * Mathf.Rad2Deg;
        }

        public float GetSide()
        {
            return Mathf.Sign(transform.localScale.x);
        }

        public Vector3 GetMoveTarget()
        {
            return current_target;
        }
        
        public static Enemy2D GetNearest(Vector3 pos, float range = 999f)
        {
            float min_dist = range;
            Enemy2D nearest = null;
            foreach (Enemy2D point in enemy_list)
            {
                float dist = (point.transform.position - pos).magnitude;
                if (dist < min_dist)
                {
                    min_dist = dist;
                    nearest = point;
                }
            }
            return nearest;
        }
        
        public static List<Enemy2D> GetAll()
        {
            return enemy_list;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                if (IsDead) return;
                
                if (AudioController.Instance != null)
                    AudioController.Instance.SfxHit();
                
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("Projectile"), true);
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("Enemy"), true);
                Player.Instance.Die(false);
            }
        }
    }
}