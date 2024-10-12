using UnityEngine;

public abstract class BaseCameraComponent : MonoBehaviour
{
    protected const float FULL_CIRCLE_DEG = (2 * Mathf.PI) * Mathf.Rad2Deg;

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
