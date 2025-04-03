using System.Runtime.CompilerServices;
using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    public bool IsMoving => forwardSpeed > MovementDefines.Character.MAGNITUDE_THRESHOLD;
    public float Speed => forwardSpeed;
    public float SpeedPercentage => Mathf.InverseLerp(0, topSpeed, Speed);

    [SerializeField] protected Rigidbody attachedRigidBody;
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected float topSpeed;
    [SerializeField] protected float turnRate;
    [SerializeField] protected float accelerationFactor;
    [SerializeField] protected float deaccelerationFactor;
    [SerializeField] protected float inertiaFactor = 5f;

    protected float currentTurnSpeed;
    protected float currentLinearSpeed;
    protected Vector3 lastMoveVector;
    protected Vector3 currentMoveVector;
    protected Vector3 finalMoveVector;

    protected Vector3 groundCollisionCenter;
    protected Collider characterCollider;
    protected Collider[] groundCollisionResult;

    protected bool hasJumped;
    protected bool isMoving;
    protected float forwardSpeed;

    public virtual void Intialise()
    {
        if (attachedRigidBody == null)
        {
            Debug.LogError("No Rigidbody assigned in the inspector");
        }

        characterCollider = GetComponent<Collider>();
        groundCollisionResult = new Collider[MovementDefines.Character.FLOOR_COLLIDER_COUNT];
    }

    public virtual void UpdateMovement(Vector3 direction, bool isFiring)
    {
        if (attachedRigidBody.linearVelocity.y < 0 && !IsGrounded())
        {
            attachedRigidBody.linearVelocity += Vector3.up * Physics.gravity.y * (2.5f - 1) * Time.fixedDeltaTime;
        }
        else if (attachedRigidBody.linearVelocity.y > 0 && !hasJumped)
        {
            attachedRigidBody.linearVelocity += Vector3.up * Physics.gravity.y * (2.0f - 1) * Time.fixedDeltaTime;
        }

        var targetVelocity = GetMoveVector(direction);
        targetVelocity.y = attachedRigidBody.linearVelocity.y;

        attachedRigidBody.linearVelocity = isFiring ? Vector3.zero : Vector3.Lerp(attachedRigidBody.linearVelocity, targetVelocity, inertiaFactor * Time.fixedDeltaTime);
        forwardSpeed = Mathf.Abs(transform.InverseTransformDirection(attachedRigidBody.linearVelocity).z);
    }

    public virtual void UpdateLookDirection(Vector3 directionVector)
    {
        attachedRigidBody.rotation = GetRotation(directionVector);
    }

    protected Vector3 GetMoveVector(Vector3 direction)
    {
        var canAccelerate = direction.sqrMagnitude > MovementDefines.Character.MAGNITUDE_THRESHOLD;

        if (canAccelerate)
        {
            lastMoveVector = currentMoveVector;
            currentMoveVector = direction;
        }
        else
        {
            currentMoveVector = lastMoveVector;
        }

        currentLinearSpeed += canAccelerate ? accelerationFactor * Time.fixedDeltaTime : -(deaccelerationFactor * Time.fixedDeltaTime);

        if (currentLinearSpeed > topSpeed)
        {
            currentLinearSpeed = topSpeed;
        }
        else if (currentLinearSpeed < 0)
        {
            currentLinearSpeed = 0;
        }

        var moveVector = currentMoveVector * currentLinearSpeed;
        finalMoveVector.x = moveVector.x;
        finalMoveVector.z = moveVector.z;

        return moveVector;
    }

    protected bool IsGrounded()
    {
        groundCollisionCenter.x = transform.position.x;
        groundCollisionCenter.z = transform.position.z;
        groundCollisionCenter.y = transform.position.y - characterCollider.bounds.extents.y;

        var grounded = Physics.OverlapBoxNonAlloc(groundCollisionCenter, Vector3.one * MovementDefines.Character.GROUNDED_COLLIDER_SIZE_MUTIPLIER, groundCollisionResult, transform.rotation, groundLayer);

        return grounded > 0;
    }

    private Quaternion GetRotation(Vector3 direction)
    {
        Vector3 turnDirection = new Vector3(direction.x, 0, direction.z);
        float forwardTiltAmount = Mathf.Abs(transform.InverseTransformDirection(attachedRigidBody.linearVelocity).z) * 2.5f;
        float angularTiltAmount = Vector3.Dot(attachedRigidBody.angularVelocity, Vector3.up) * 5f;
        currentTurnSpeed = turnRate * Time.smoothDeltaTime;

        var start = MathDefines.GetAngleFromDirectionXZ(transform.forward);
        var target = MathDefines.GetAngleFromDirectionXZ(turnDirection);
        var angle = MathDefines.InterpolateAngle(start, target, currentTurnSpeed);

        var finalRotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.up);

        return finalRotation;
    }
}
