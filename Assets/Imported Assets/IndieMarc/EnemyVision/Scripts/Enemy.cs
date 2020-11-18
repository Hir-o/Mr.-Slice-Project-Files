using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

namespace IndieMarc.StealthLOS
{
    public class Enemy : MonoBehaviour
    {
        public float move_speed = 2f;
        public float rotate_speed = 120f;
        public LayerMask obstacle_mask = ~(0);
        public bool use_pathfind = false;

        public UnityAction onDeath;

        private Rigidbody rigid;
        private NavMeshAgent nav_agent;
        private Vector3 move_vect;
        private Vector3 face_vect;
        private float rotate_val;

        private Vector3 current_target;
        private float current_mult = 1f;
        private Vector3 current_rot_target;
        private float current_rot_mult = 1f;

        private static List<Enemy> enemy_list = new List<Enemy>();

        private void Awake()
        {
            enemy_list.Add(this);
            rigid = GetComponent<Rigidbody>();
            nav_agent = GetComponent<NavMeshAgent>();
            move_vect = Vector3.zero;
            current_target = transform.position;
            current_rot_target = transform.position + transform.forward;
            rotate_val = 0f;
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
            Vector3 dist_vect = (current_target - rigid.position);
            move_vect = dist_vect.normalized * move_speed * current_mult * Mathf.Min(dist_vect.magnitude, 1f);

            if (use_pathfind && nav_agent)
            {
                nav_agent.speed = move_speed * current_mult;
                nav_agent.SetDestination(current_target);
            }
            else
            {
                rigid.MovePosition(transform.position + move_vect * Time.fixedDeltaTime);
            }
        }

        private void Update()
        {
            bool controlled_by_agent = use_pathfind && nav_agent && nav_agent.hasPath;
            rotate_val = 0f;

            if (!controlled_by_agent)
            {
                Vector3 dir = current_rot_target - transform.position;
                dir.y = 0f;
                if (dir.magnitude > 0.1f)
                {
                    Quaternion target = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    Quaternion reachedRotation = Quaternion.RotateTowards(transform.rotation, target, rotate_speed * current_rot_mult * Time.deltaTime);
                    rotate_val = Quaternion.Angle(transform.rotation, target);
                    face_vect = dir.normalized;
                    transform.rotation = reachedRotation;
                }
            }
        }

        public bool CheckFronted(Vector3 dir)
        {
            Vector3 origin = transform.position + Vector3.up * 1f;
            RaycastHit hit;
            return Physics.Raycast(new Ray(origin, dir.normalized), out hit, dir.magnitude, obstacle_mask.value);
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

            Destroy(gameObject);
        }
        
        public Vector3 GetMove()
        {
            return move_vect;
        }

        public Vector3 GetFacing()
        {
            return face_vect;
        }

        public float GetRotationVelocity()
        {
            return rotate_val;
        }

        public Vector3 GetMoveTarget()
        {
            if (use_pathfind && nav_agent && nav_agent.hasPath)
                return nav_agent.nextPosition;
            return current_target;
        }

        public static Enemy GetNearest(Vector3 pos, float range = 999f)
        {
            float min_dist = range;
            Enemy nearest = null;
            foreach (Enemy point in enemy_list)
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
        
        public static List<Enemy> GetAll()
        {
            return enemy_list;
        }
    }

}