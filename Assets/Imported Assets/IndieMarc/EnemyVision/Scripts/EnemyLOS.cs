using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IndieMarc.StealthLOS
{

    public enum EnemyLOSState
    {
        Patrol = 0,
        Alert = 5,
        Chase = 10,
        Confused = 15, //After lost track of target
    }

    [RequireComponent(typeof(Enemy))]
    public class EnemyLOS : MonoBehaviour
    {
        [Header("Detection")]
        public float vision_angle = 30f;
        public float vision_range = 10f;
        public float vision_height = 1f;
        public float touch_range = 1f;
        public float detect_time = 1f;
        public float alerted_time = 3f;
        public LayerMask vision_mask = ~(0);

        [Header("Chase")]
        public float follow_time = 10f;
        public bool dont_return = false;

        [Header("Ref")]
        public Transform eye;
        public GameObject vision_prefab;
        public GameObject death_fx_prefab;

        public UnityAction<VisionTarget> onSeeTarget; //As soon as seen (Patrol->Alert)
        public UnityAction<VisionTarget> onDetectTarget; //detect_time seconds after seen (Alert->Chase)
        public UnityAction<VisionTarget> onTouchTarget;
        public UnityAction onDeath;

        private EnemyPatrol enemy_patrol;
        private EnemyFollow enemy_follow;
        private Enemy enemy;

        private EnemyLOSState state = EnemyLOSState.Patrol;
        private VisionTarget seen_character = null;
        private EnemyVision vision;
        private float state_timer = 0f;
        
        void Start()
        {
            enemy_patrol = GetComponent<EnemyPatrol>();
            enemy_follow = GetComponent<EnemyFollow>();
            enemy = GetComponent<Enemy>();
            enemy.onDeath += OnDeath;

            if (vision_prefab)
            {
                GameObject vis = Instantiate(vision_prefab, GetEye(), Quaternion.identity);
                vis.transform.parent = transform;
                vision = vis.GetComponent<EnemyVision>();
                vision.target = this;
                vision.vision_angle = vision_angle;
                vision.vision_range = vision_range;
            }
            
            ChangeState(EnemyLOSState.Patrol);
        }

        void Update()
        {
            state_timer += Time.deltaTime;

            //While patroling, detect targets
            if (state == EnemyLOSState.Patrol)
            {
                DetectVisionTarget();
                DetectTouchVisionTarget();
            }

            //When just seen the VisionTarget, enemy alerted
            if (state == EnemyLOSState.Alert)
            {
                if (seen_character == null)
                {
                    ChangeState(EnemyLOSState.Patrol);
                    return;
                }

                bool could_see_target = CouldSeeObject(seen_character.gameObject);
                if (could_see_target)
                    enemy.FaceToward(seen_character.transform.position);

                if (state_timer > detect_time)
                {
                    bool can_see_target = CanSeeVisionTarget(seen_character);
                    if (enemy_follow && can_see_target)
                    {
                        ChangeState(EnemyLOSState.Chase);
                        enemy_follow.target = seen_character.gameObject;

                        if (dont_return && enemy_patrol)
                            enemy_patrol.SetAlerted(true);

                        if (onDetectTarget != null)
                            onDetectTarget.Invoke(seen_character);
                    }
                }

                if (state_timer > alerted_time)
                {
                    ChangeState(EnemyLOSState.Patrol);
                }

                DetectTouchTarget();
            }

            //If seen long enough (detect time), will go into a chase
            if (state == EnemyLOSState.Chase)
            {
                if (seen_character == null) {
                    ChangeState(EnemyLOSState.Patrol);
                    return;
                }

                bool can_see_target = CanSeeVisionTarget(seen_character);
                enemy_follow.target = can_see_target ? seen_character.gameObject : null;

                if (state_timer > follow_time)
                {
                    if(!can_see_target)
                        ChangeState(EnemyLOSState.Patrol);
                }

                if (enemy_follow.HasReachedTarget() && !can_see_target)
                    ChangeState(EnemyLOSState.Confused);

                DetectTouchTarget();
            }

            //After the chase, if VisionTarget is unseen, enemy will be confused
            if (state == EnemyLOSState.Confused)
            {
                bool can_see_target = CanSeeVisionTarget(seen_character);
                if (can_see_target)
                    ChangeState(EnemyLOSState.Chase);

                if (state_timer > alerted_time)
                    ChangeState(EnemyLOSState.Patrol);
            }
        }
        
        //Look for possible seen targets
        public void DetectVisionTarget()
        {
            //Detect character
            foreach (VisionTarget character in VisionTarget.GetAll())
            {
                if (character == seen_character)
                    continue;

                if (CanSeeVisionTarget(character))
                {
                    seen_character = character;
                    ChangeState(EnemyLOSState.Alert);

                    if (onSeeTarget != null)
                        onSeeTarget.Invoke(seen_character);
                }
            }
        }

        //Look for possible touch targets
        public void DetectTouchVisionTarget()
        {
            //Detect character touch
            foreach (VisionTarget character in VisionTarget.GetAll())
            {
                if (character == seen_character)
                    continue;

                if (character.CanBeSeen() && CanTouchObject(character.gameObject))
                {
                    seen_character = character;
                    ChangeState(EnemyLOSState.Alert);

                    if (onSeeTarget != null)
                        onSeeTarget.Invoke(seen_character);
                    
                }
            }
        }

        public void DetectTouchTarget()
        {
            //Detect character touch
            foreach (VisionTarget character in VisionTarget.GetAll())
            {
                if (CanTouchObject(character.gameObject))
                {
                    if (onTouchTarget != null)
                        onTouchTarget.Invoke(seen_character);
                }
            }
        }

        //Can the enemy see a vision target?
        public bool CanSeeVisionTarget(VisionTarget target, float range_offset = 0f, float angle_offset = 0f)
        {
            return target != null && target.CanBeSeen() && CanSeeObject(target.gameObject, range_offset, angle_offset);
        }

        //Can the enemy see an object ?
        public bool CanSeeObject(GameObject obj, float range_offset = 0f, float angle_offset = 0f)
        {
            Vector3 forward = transform.forward;
            Vector3 dir = obj.transform.position - GetEye();
            Vector3 dir_touch = dir; //Preserve Y for touch
            dir.y = 0f; //Remove Y for cone vision range

            float vis_range = vision_range + range_offset;
            float vis_angle = vision_angle + angle_offset;
            float losangle = Vector3.Angle(forward, dir);
            float losheight = obj.transform.position.y - GetEye().y;
            bool can_see_cone = losangle < vis_angle / 2f && dir.magnitude < vis_range && losheight < vision_height;
            bool can_see_touch = dir_touch.magnitude < touch_range;
            if (obj.activeSelf && (can_see_cone || can_see_touch)) //In range and in angle
            {
                RaycastHit hit;
                bool raycast = Physics.Raycast(new Ray(GetEye(), dir.normalized), out hit, dir.magnitude, vision_mask.value);
                if (!raycast)
                    return true; //No obstacles in the way (in case character not in layer)
                if (raycast && (hit.collider.gameObject == obj || hit.collider.transform.IsChildOf(obj.transform))) //See character
                    return true; //The only obstacles is the character
            }
            return false;
        }

        //Is the enemy right next to the object ?
        public bool CanTouchObject(GameObject obj)
        {
            Vector3 dir = obj.transform.position - transform.position;
            if (dir.magnitude < touch_range) //In range and in angle
            {
                return true;
            }
            return false;
        }

        //There's no wall between the two, could be seen if the enemy changes facing
        public bool CouldSeeObject(GameObject obj, float range_offset = 0f)
        {
            Vector3 forward = transform.forward;
            Vector3 dir = obj.transform.position - GetEye();
            Vector3 dir_touch = dir;
            dir.y = 0f;
            float vis_range = vision_range + range_offset;
            float losheight = obj.transform.position.y - GetEye().y;
            bool can_see_cone = dir.magnitude < vis_range && losheight < vision_height;
            bool can_see_touch = dir_touch.magnitude < touch_range;
            if (obj.activeSelf && (can_see_cone || can_see_touch)) //In range and in angle
            {
                RaycastHit hit;
                bool raycast = Physics.Raycast(new Ray(GetEye(), dir.normalized), out hit, dir.magnitude, vision_mask.value);
                if (!raycast)
                    return true; //No obstacles in the way (in case character not in layer)
                if (raycast && (hit.collider.gameObject == obj || hit.collider.transform.IsChildOf(obj.transform))) //See character
                    return true; //The only obstacles is the character
            }
            return false;
        }

        public void ChangeState(EnemyLOSState state)
        {
            this.state = state;
            state_timer = 0f;

            if (state == EnemyLOSState.Patrol)
                seen_character = null;

            if (enemy_patrol)
                enemy_patrol.enabled = (state == EnemyLOSState.Patrol);
            if(enemy_follow)
                enemy_follow.enabled = (state == EnemyLOSState.Chase);
        }

        public Vector3 GetEye()
        {
            return eye ? eye.position : transform.position;
        }

        public Enemy GetEnemy()
        {
            return enemy;
        }

        private void OnDeath()
        {
            if(vision)
                vision.gameObject.SetActive(false);

            if (onDeath != null)
                onDeath.Invoke();
        }
    }

}
