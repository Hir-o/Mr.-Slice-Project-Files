using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndieMarc.StealthLOS
{
    [RequireComponent(typeof(Enemy2D))]
    public class EnemyPatrol2D : MonoBehaviour
    {
        public float speed_mult = 1f;
        public float wait_time = 1f;

        [Header("Patrol")]
        public GameObject[] patrol_targets;

        [Header("Facing")]
        public float angle_min = 0f;
        public float angle_max = 0f;
        public float angle_speed = 90f;
        public float pause_duration = 1f;

        private Animator _animator;
        private EnemyLOS2D enemyLos2D;
        private Enemy2D enemy;
        private Vector3 start_pos;
        private bool waiting = false;
        private float wait_timer = 0f;

        private int current_path = 0;
        private bool path_rewind = false;
        private Vector3 move_dir;
        private Vector3 face_dir;
        private float current_angle = 0f;
        private bool angle_rewind = false;
        private float pause_timer = 0f;

        private List<Vector3> path_list = new List<Vector3>();

        void Start()
        {
            _animator = GetComponent<Animator>();
            enemyLos2D = GetComponent<EnemyLOS2D>();
            enemy = GetComponent<Enemy2D>();
            start_pos = transform.position;
            path_list.Add(transform.position);
            move_dir = Vector3.right * Mathf.Sign(transform.localScale.x);
            face_dir = move_dir;

            foreach (GameObject patrol in patrol_targets)
            {
                if (patrol)
                    path_list.Add(patrol.transform.position);
            }
            
            current_path = 0;
            if (path_list.Count >= 2)
                current_path = 1; //Dont start at start pos

            if (path_list.Count <= 1)
                path_rewind = Mathf.Sign(transform.localScale.x) < 0f;
        }

        void Update()
        {
            wait_timer += Time.deltaTime;

            move_dir = Vector3.right * Mathf.Sign(transform.localScale.x);
            
            if (enemyLos2D.GetState() != EnemyLOS2DState.Patrol && _animator.GetBool(AnimatorParams.ENEMY_RUNNING))
                _animator.SetBool(AnimatorParams.ENEMY_RUNNING, false);

            //If still in starting path
            if (!waiting && !HasFallen() && path_list.Count > 1)
            {
                //Move
                Vector3 targ = path_list[current_path];
                enemy.MoveTo(targ, speed_mult);
                move_dir = Vector3.right * Mathf.Sign((targ - transform.position).x);

                //Check if reached target
                Vector3 dist_vect = (targ - transform.position);
                dist_vect.z = 0f;
                if (dist_vect.magnitude < 0.1f)
                {
                    waiting = true;
                    wait_timer = 0f;
                }

                //Check if obstacle ahead
                bool fronted = enemy.CheckFronted(dist_vect.normalized);
                if (fronted && wait_timer > 2f)
                {
                    RewindPath();
                    wait_timer = 0f;
                }
            }

            //If can't reach starting path anymore
            if (!waiting && HasFallen())
            {
                //Move
                Vector3 mdir = Vector3.right * (path_rewind ? -2f : 2f);
                Vector3 targ = transform.position + mdir;
                enemy.MoveTo(targ, speed_mult);
                enemy.FaceToward(targ);
                move_dir = Vector3.right * Mathf.Sign((targ - transform.position).x);

                //Check if obstacle ahead
                Vector3 dist_vect = (targ - transform.position);
                bool fronted = enemy.CheckFronted(dist_vect.normalized);
                if (fronted && wait_timer > 2f)
                {
                    path_rewind = !path_rewind;
                    wait_timer = 0f;
                }
            }

            if (waiting)
            {
                if (enemyLos2D.GetState() == EnemyLOS2DState.Patrol)
                    _animator.SetBool(AnimatorParams.ENEMY_RUNNING, false);
                
                //Wait a bit
                if (wait_timer > wait_time)
                {
                    if (enemyLos2D.GetState() == EnemyLOS2DState.Patrol)
                        _animator.SetBool(AnimatorParams.ENEMY_RUNNING, true);
                    
                    GoToNextPath();
                    waiting = false;
                    wait_timer = 0f;
                }
            }
            else
            {
                if (path_list.Count > 1 && enemyLos2D.GetState() == EnemyLOS2DState.Patrol)
                    _animator.SetBool(AnimatorParams.ENEMY_RUNNING, true);
            }

            //Angle
            pause_timer += Time.deltaTime;
            if (pause_timer > pause_duration)
            {
                float angle_target = angle_rewind ? angle_min : angle_max;
                current_angle = Mathf.MoveTowards(current_angle, angle_target, angle_speed * Time.deltaTime);
                face_dir = new Vector3(Mathf.Cos(current_angle * Mathf.Deg2Rad), Mathf.Sin(current_angle * Mathf.Deg2Rad), 0f);
                face_dir = new Vector3(Mathf.Sign(move_dir.x) * face_dir.x, face_dir.y, 0f);
            }
            enemy.FaceToward(transform.position + face_dir * 2f);

            if (!angle_rewind && enemy.GetFacingAngle() >= angle_max - 0.02f)
            {
                angle_rewind = true;
                pause_timer = 0f;
            }
            if (angle_rewind && enemy.GetFacingAngle() <= angle_min + 0.02f)
            {
                angle_rewind = false;
                pause_timer = 0f;
            }
        }

        private void RewindPath()
        {
            path_rewind = !path_rewind;
            current_path += path_rewind ? -1 : 1;
            current_path = Mathf.Clamp(current_path, 0, path_list.Count - 1);
        }

        private void GoToNextPath()
        {
            if (current_path <= 0 || current_path >= path_list.Count - 1)
                path_rewind = !path_rewind;
            current_path += path_rewind ? -1 : 1;
            current_path = Mathf.Clamp(current_path, 0, path_list.Count - 1);
        }

        public float GetVisionAngle()
        {
            return current_angle;
        }

        public bool HasFallen()
        {
            float distY = Mathf.Abs(transform.position.y - start_pos.y);
            return distY > 0.5f;
        }

        [ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3 prev_pos = transform.position;

            foreach (GameObject patrol in patrol_targets)
            {
                if (patrol)
                {
                    Gizmos.DrawLine(prev_pos, patrol.transform.position);
                    prev_pos = patrol.transform.position;
                }
            }
        }

        public Enemy2D GetEnemy()
        {
            return enemy;
        }
    }
}