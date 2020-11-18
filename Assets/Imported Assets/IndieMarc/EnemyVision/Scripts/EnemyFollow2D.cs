using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndieMarc.StealthLOS
{
    [RequireComponent(typeof(Enemy2D))]
    public class EnemyFollow2D : MonoBehaviour
    {
        public float speed_mult = 1f;
        public float memory_duration = 4f;
        public GameObject target;
        
        private Enemy2D enemy;

        private Vector3 last_seen_pos;
        private GameObject last_target;
        private float memory_timer = 0f;

        void Start()
        {
            enemy = GetComponent<Enemy2D>();
            
        }

        void Update()
        {
            Vector3 targ = target ? target.transform.position : last_seen_pos;

            //Use memory if no more target
            if (target == null && last_target != null && memory_duration > 0.1f)
            {
                memory_timer += Time.deltaTime;
                if (memory_timer < memory_duration)
                {
                    last_seen_pos = last_target.transform.position;
                    targ = last_seen_pos;
                }
            }

            //Move to target
            enemy.MoveTo(targ, speed_mult);
            enemy.FaceToward(enemy.GetMoveTarget(), 2f);

            if (target != null)
            {
                last_target = target;
                last_seen_pos = target.transform.position;
                memory_timer = 0f;
            }
        }

        public bool HasReachedTarget()
        {
            Vector3 targ = target ? target.transform.position : last_seen_pos;
            return (targ - transform.position).magnitude < 0.5f;
        }

        public Enemy2D GetEnemy()
        {
            return enemy;
        }
    }

}