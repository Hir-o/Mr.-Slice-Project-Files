using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorParams
{
    // Animator parameter types
    public enum AnimParamType
    {
        Trigger,
        Bool,
        Float,
        Int
    }

    public static AnimParamType ParameterTypeTrigger = AnimParamType.Trigger;
    public static AnimParamType ParameterTypeBool    = AnimParamType.Bool;
    public static AnimParamType ParameterTypeFloat   = AnimParamType.Float;
    public static AnimParamType ParameterTypeInt     = AnimParamType.Int;

    // Player animator parameters
    public static readonly string PLAYER_RUNNING = "isRunning";
    public static readonly string PLAYER_JUMPING = "isJumping";
    public static readonly string PLAYER_DASHING = "isDashing";
    public static readonly string PLAYER_FALLING = "isFalling";
    public static readonly string PLAYER_ENDDASH = "isEndingDash";
    
    // Enemy animator parameters
    public static readonly string ENEMY_RUNNING = "isRunning";
    
    // Door
    public static readonly string DOOR_UNLOCK = "isUnlocked";
}