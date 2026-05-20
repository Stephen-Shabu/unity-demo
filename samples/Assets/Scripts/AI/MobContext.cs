using UnityEngine;
using System;
public class MobContext
{
    public event Action OnAimAttack;
    public event Action OnFollowTarget;
    public event Action OnAnticpateAttack;
    public event Action OnAvoidObstacle;
    public event Action OnWaitFortarget;
    public event Action OnAttackerPositionChange;

    public void RaiseOnAimAttack() => OnAimAttack?.Invoke();
    public void RaiseOnFollowTarget() => OnFollowTarget?.Invoke();
    public void RaiseOnAnticipateAttack() => OnAnticpateAttack?.Invoke();
    public void RaiseOnAvoidObstacle() => OnAvoidObstacle?.Invoke();
    public void RaiseOnWaitForTarget() => OnWaitFortarget?.Invoke();  
    
    public void RaiseOnAttackerPositionChange() => OnAttackerPositionChange?.Invoke();

    public int Index;
    public Rigidbody Rigidbody;
    public Transform Transform;
    public Transform Target;
    public MovementComponent MoveComponent;
    public DetectionComponent DetectionComponent;
    public AnimationComponent AnimComponent;
    public MeleeComponent MeleeComponent;
    public AudioComponent AudioComponent;
    public HealthComponent HealthComponent;
    public LayerMask WallLayer;
    public float StoppingDistance;
    [Range(0, 1)] public float ChaseIntensity;
    public Vector3 Heading;
    public Vector3 AvoidTarget;
    public Vector3 RepositionTarget;
    public Vector3 HitDirection;
    public float AggroMeter;
    public const float MAX_AGGRO_METER = 100f;
}
