using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndieMarc.StealthLOS
{
    public enum EnemyPatrolType
    {
        Rewind = 0,
        Loop = 2,
        FacingOnly = 5,
    }

    [RequireComponent(typeof(Enemy))]
    public class EnemyPatrol : MonoBehaviour
    {
        public EnemyPatrolType type;
        public float speed_mult = 1f;
        public float wait_time = 1f;

        [Header("Patrol")]
        public GameObject[] patrol_targets;

        private Vector3 start_pos;
        private Enemy enemy;
        private bool waiting = false;
        private float wait_timer = 0f;
        
        private int current_path = 0;
        private bool path_rewind = false;
        private bool alerted = false;
        private Vector3 rand_facing;

        private List<Vector3> path_list = new List<Vector3>();

        void Start()
        {
            enemy = GetComponent<Enemy>();
            start_pos = transform.position;

            if (type != EnemyPatrolType.FacingOnly)
                path_list.Add(transform.position);

            foreach (GameObject patrol in patrol_targets)
            {
                if (patrol)
                    path_list.Add(patrol.transform.position);
            }
            
            current_path = 0;
            if (path_list.Count >= 2)
                current_path = 1; //Dont start at start pos
        }

        void Update()
        {
            wait_timer += Time.deltaTime;
            
            bool facing_only = type == EnemyPatrolType.FacingOnly;
            float dist = (transform.position - start_pos).magnitude;
            bool is_far = dist > 0.5f;

            
            //Alerted, rotate randomly
            if (!waiting && alerted)
            {
                Vector3 targ = rand_facing;
                enemy.FaceToward(targ);
                CheckIfFacingReachedTarget(targ);
            }

            //Facing only
            if (!waiting && facing_only && !alerted)
            {
                if (is_far)
                {
                    //Return to starting pos
                    enemy.MoveTo(start_pos, speed_mult);
                    enemy.FaceToward(start_pos);
                }
                else
                {
                    //Rotate only
                    Vector3 targ = path_list[current_path];
                    enemy.FaceToward(targ);
                    CheckIfFacingReachedTarget(targ);
                }
            }

            //Regular patrol
            if (!waiting && !facing_only && !alerted)
            {
                //Move following path
                Vector3 targ = path_list[current_path];
                enemy.MoveTo(targ, speed_mult);
                enemy.FaceToward(enemy.GetMoveTarget());

                //Check if reached target
                Vector3 dist_vect = (targ - transform.position);
                dist_vect.y = 0f;
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
            
            //Waiting
            if (waiting)
            {
                //Wait a bit
                if (wait_timer > wait_time)
                {
                    GoToNextPath();
                    rand_facing = GetRandomFacingTarget();
                    waiting = false;
                    wait_timer = 0f;
                }
            }
        }

        private void CheckIfFacingReachedTarget(Vector3 targ)
        {
            //Check if reached target
            Vector3 dist_vect = (targ - transform.position);
            dist_vect.y = 0f;
            float dot = Vector3.Dot(transform.forward, dist_vect.normalized);
            if (dot > 0.99f)
            {
                waiting = true;
                wait_timer = 0f;
            }
        }

        private Vector3 GetRandomFacingTarget()
        {
            Vector3 center = transform.position;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            return center + offset;
        }

        private void RewindPath()
        {
            if (type != EnemyPatrolType.FacingOnly)
            {
                path_rewind = !path_rewind;
                current_path += path_rewind ? -1 : 1;
                current_path = Mathf.Clamp(current_path, 0, path_list.Count - 1);
            }
        }

        private void GoToNextPath()
        {
            if (type == EnemyPatrolType.FacingOnly)
            {
                if (current_path <= 0 || current_path >= path_list.Count - 1)
                    path_rewind = !path_rewind;
                current_path += path_rewind ? -1 : 1;
                current_path = Mathf.Clamp(current_path, 0, path_list.Count - 1);
            }
            else if (type == EnemyPatrolType.Loop)
            {
                current_path = (current_path + 1) % path_list.Count;
                current_path = Mathf.Clamp(current_path, 0, path_list.Count - 1);
            }
            else
            {
                if (current_path <= 0 || current_path >= path_list.Count - 1)
                    path_rewind = !path_rewind;
                current_path += path_rewind ? -1 : 1;
                current_path = Mathf.Clamp(current_path, 0, path_list.Count - 1);
            }
        }

        [ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3 prev_pos = transform.position;

            if (type != EnemyPatrolType.FacingOnly)
            {
                foreach (GameObject patrol in patrol_targets)
                {
                    if (patrol)
                    {
                        Gizmos.DrawLine(prev_pos, patrol.transform.position);
                        prev_pos = patrol.transform.position;
                    }
                }

                if(type == EnemyPatrolType.Loop)
                    Gizmos.DrawLine(prev_pos, transform.position);
            }

            if (type == EnemyPatrolType.FacingOnly)
            {
                foreach (GameObject patrol in patrol_targets)
                {
                    if (patrol)
                    {
                        Gizmos.DrawLine(transform.position, patrol.transform.position);
                    }
                }
            }
        }

        //Enemy becomes alerted the first time it chases a target
        public void SetAlerted(bool alerted)
        {
            this.alerted = alerted;
            rand_facing = GetRandomFacingTarget();
        }

        public Enemy GetEnemy()
        {
            return enemy;
        }
    }
}