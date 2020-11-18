using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IndieMarc.StealthLOS
{
    public enum EnemyLOS2DState
    {
        Patrol   = 0,
        Alert    = 5,
        Chase    = 10,
        Confused = 15,
    }

    [RequireComponent(typeof(Enemy2D))]
    public class EnemyLOS2D : MonoBehaviour
    {
        [Header("Detection")]
        public float vision_angle = 30f;

        public float     vision_range = 10f;
        public float     touch_range  = 1f;
        public float     detect_time  = 1f;
        public float     alerted_time = 3f;
        public LayerMask vision_mask  = ~(0);

        [Header("Chase")]
        public float follow_time = 10f;

        [Header("Ref")]
        public Transform eye;

        public GameObject vision_prefab;
        public GameObject death_fx_prefab;

        public UnityAction<VisionTarget> onSeeTarget;    //As soon as seen (Patrol->Alert)
        public UnityAction<VisionTarget> onDetectTarget; //detect_time seconds after seen (Alert->Chase)
        public UnityAction<VisionTarget> onTouchTarget;
        public UnityAction               onDeath;

        private EnemyPatrol2D   enemy_patrol;
        private EnemyFollow2D   enemy_follow;
        private Enemy2D         enemy;
        private ContactFilter2D contact_filter;

        private EnemyLOS2DState state          = EnemyLOS2DState.Patrol;
        private VisionTarget    seen_character = null;
        private EnemyVision2D   vision;
        private float           state_timer = 0f;

        public EnemyLOS2DState GetState() { return state; }

        void Start()
        {
            enemy_patrol  =  GetComponent<EnemyPatrol2D>();
            enemy_follow  =  GetComponent<EnemyFollow2D>();
            enemy         =  GetComponent<Enemy2D>();
            enemy.onDeath += OnDeath;

            contact_filter              = new ContactFilter2D();
            contact_filter.layerMask    = vision_mask;
            contact_filter.useLayerMask = true;
            contact_filter.useTriggers  = false;

            if (vision_prefab)
            {
                GameObject vis = Instantiate(vision_prefab, GetEye(), Quaternion.identity);
                vis.transform.parent = transform;
                vision               = vis.GetComponent<EnemyVision2D>();
                vision.target        = this;
                vision.vision_angle  = vision_angle;
                vision.vision_range  = vision_range;
            }

            ChangeState(EnemyLOS2DState.Patrol);
        }

        void Update()
        {
            state_timer += Time.deltaTime;

            //While patroling, detect targets
            if (state == EnemyLOS2DState.Patrol)
            {
                DetectVisionTarget();
                DetectTouchVisionTarget();
            }

            //When just seen the VisionTarget, enemy alerted
            if (state == EnemyLOS2DState.Alert)
            {
                if (seen_character == null)
                {
                    ChangeState(EnemyLOS2DState.Patrol);
                    return;
                }

                bool could_see_target = CouldSeeObject(seen_character.gameObject) ||
                                        CanTouchObject(seen_character.gameObject);
                if (could_see_target) enemy.FaceToward(seen_character.transform.position);

                if (state_timer > detect_time)
                {
                    bool can_see_target =
                        CanSeeVisionTarget(seen_character) || CanTouchObject(seen_character.gameObject);
                    if (enemy_follow && can_see_target)
                    {
                        ChangeState(EnemyLOS2DState.Chase);
                        enemy_follow.target = seen_character.gameObject;

                        if (onDetectTarget != null) onDetectTarget.Invoke(seen_character);
                    }
                }

                if (state_timer > alerted_time) { ChangeState(EnemyLOS2DState.Patrol); }

                DetectTouchTarget();
            }

            //If seen long enough (detect time), will go into a chase
            if (state == EnemyLOS2DState.Chase)
            {
                if (seen_character == null)
                {
                    ChangeState(EnemyLOS2DState.Patrol);
                    return;
                }

                bool can_see_target = CanSeeVisionTarget(seen_character);
                enemy_follow.target = can_see_target ? seen_character.gameObject : null;

                if (state_timer > follow_time)
                {
                    if (!can_see_target) ChangeState(EnemyLOS2DState.Patrol);
                }

                if (enemy_follow.HasReachedTarget() && !can_see_target) ChangeState(EnemyLOS2DState.Confused);

                DetectTouchTarget();
            }

            //After the chase, if VisionTarget is unseen, enemy will be confused
            if (state == EnemyLOS2DState.Confused)
            {
                bool can_see_target = CanSeeVisionTarget(seen_character);
                if (can_see_target) ChangeState(EnemyLOS2DState.Chase);

                if (state_timer > alerted_time) ChangeState(EnemyLOS2DState.Patrol);
            }
        }

        //Look for possible seen targets
        public void DetectVisionTarget()
        {
            //Detect character
            foreach (VisionTarget character in VisionTarget.GetAll())
            {
                if (character == seen_character) continue;

                if (CanSeeVisionTarget(character))
                {
                    seen_character = character;
                    ChangeState(EnemyLOS2DState.Alert);

                    if (onSeeTarget != null) onSeeTarget.Invoke(seen_character);
                }
            }
        }

        //Look for possible touch targets
        public void DetectTouchVisionTarget()
        {
            //Detect character touch
            foreach (VisionTarget character in VisionTarget.GetAll())
            {
                if (character == seen_character) continue;

                if (character.CanBeSeen() && CanTouchObject(character.gameObject))
                {
                    seen_character = character;
                    ChangeState(EnemyLOS2DState.Alert);

                    if (onSeeTarget != null) onSeeTarget.Invoke(seen_character);
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
                    if (onTouchTarget != null) onTouchTarget.Invoke(seen_character);
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
            Vector3 dir     = obj.transform.position - GetEye();
            dir.z = 0f;
            float vis_range     = vision_range + range_offset;
            float vis_angle     = vision_angle + angle_offset;
            float losangle      = Vector3.Angle(enemy.GetFacing(), dir.normalized);
            bool  can_see_cone  = losangle < vis_angle / 2f && dir.magnitude < vis_range;
            bool  can_see_touch = dir.magnitude < touch_range;
            if (obj.activeSelf && (can_see_cone || can_see_touch)) //In range and in angle
            {
                RaycastHit2D hit = Physics2D.Raycast(GetEye(), dir.normalized, vis_range, vision_mask.value);
                if (hit.collider && (hit.collider.gameObject == obj || hit.collider.transform.IsChildOf(obj.transform))
                ) //See character
                {
                    return true;
                }
            }

            return false;
        }

        //Is the enemy right next to the object ?
        public bool CanTouchObject(GameObject obj)
        {
            Vector3 dir = obj.transform.position - transform.position;
            dir.z = 0f;
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
            Vector3 dir     = obj.transform.position - GetEye();
            dir.z = 0f;
            float vis_range = vision_range             + range_offset;
            float losheight = obj.transform.position.y - GetEye().y;
            if (obj.activeSelf && dir.magnitude < vis_range) //In range and in angle
            {
                RaycastHit hit;
                bool raycast = Physics.Raycast(new Ray(GetEye(), dir.normalized), out hit, dir.magnitude,
                                               vision_mask.value);
                if (!raycast) return true; //No obstacles in the way (in case character not in layer)
                if (raycast && (hit.collider.gameObject == obj || hit.collider.transform.IsChildOf(obj.transform))
                )                //See character
                    return true; //The only obstacles is the character
            }

            return false;
        }

        public void ChangeState(EnemyLOS2DState state)
        {
            this.state  = state;
            state_timer = 0f;

            if (state == EnemyLOS2DState.Patrol) seen_character = null;

            if (enemy_patrol) enemy_patrol.enabled = (state == EnemyLOS2DState.Patrol);
            if (enemy_follow) enemy_follow.enabled = (state == EnemyLOS2DState.Chase);
        }

        public float GetFaceAngle() { return enemy.GetFacingAngle(); }

        public float GetSide() { return enemy.GetSide(); }

        public VisionTarget GetSeenCharacter()
        {
            return seen_character;
        }

        public Vector3 GetEye() { return eye ? eye.position : transform.position; }

        public Enemy2D GetEnemy() { return enemy; }

        private void OnDeath()
        {
            if (vision) vision.gameObject.SetActive(false);

            if (onDeath != null) onDeath.Invoke();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            EnemyLOS2D enemy_other = other.gameObject.GetComponent<EnemyLOS2D>();
            if (enemy_other)
            {
                if (state == EnemyLOS2DState.Patrol && enemy_other.state == EnemyLOS2DState.Chase)
                {
                    VisionTarget target = enemy_other.seen_character;
                    if (target)
                    {
                        ChangeState(EnemyLOS2DState.Chase);
                        enemy_follow.target = target.gameObject;
                        seen_character      = target;
                    }
                }
            }
        }
    }
}