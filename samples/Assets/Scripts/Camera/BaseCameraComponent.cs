using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BaseCameraComponent : MonoBehaviour
{
    [SerializeField] protected float panTopSpeed;
    [SerializeField] protected float panAcceleration;
    [SerializeField] protected float panDeacceleration;
    [SerializeField] protected float damping;
    [SerializeField] protected float orbitDistance;
    [SerializeField] protected float orbitHeight;
    [SerializeField] protected float startAngle = 225f;

    protected float cameraPanSpeed;
    protected float currentAngle;
    protected Vector3 orbitDirection;
    protected Vector3 lastLookVector;
    protected ControlScheme currentScheme = ControlScheme.KeyboardAndMouse;
    protected Vector2 lookVector = Vector2.zero;

    public abstract void Initialise(Transform target, PlayerInput playerInput);

    public abstract void TrackTarget(Transform target);
}
