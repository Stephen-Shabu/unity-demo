using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerContext
{
    public Rigidbody Rigidbody;
    public Transform Transform;
    public DodgeDirection DodgeDirection;
    public Vector2 InputVector = Vector2.zero;
    public Vector2 LookVector = Vector2.zero;
    public Vector3 HitDirection = Vector3.zero;
    public PlayerInput PlayerInput;
    public AnimationComponent AnimComponent;
    public CharacterMovement MoveComponent;
    public HealthComponent HealthComponent;
    public HealthFXComponent HealthFxComponent;
    public BaseCameraComponent CameraComponent;
    public ProjectileComponent ProjectileComponent;
    public MeleeComponent MeleeComponent;
}