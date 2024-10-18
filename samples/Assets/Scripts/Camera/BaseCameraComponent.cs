using UnityEngine;

public abstract class BaseCameraComponent : MonoBehaviour
{
    [SerializeField] protected float panTopSpeed;
    [SerializeField] protected float panAcceleration;
    [SerializeField] protected float panDeacceleration;
    [SerializeField] protected float damping;
    [SerializeField] protected float orbitDistance;
    [SerializeField] protected float orbitHeight;

    protected float cameraPanSpeed;
    protected float currentAngle;
    protected Vector3 orbitDirection;
    protected Vector3 lastLookVector;

    public abstract void TrackPlayer(Transform target, Vector2 direction);
}
