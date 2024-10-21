using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementComponent : MonoBehaviour
{
    [SerializeField] private Rigidbody attactedRigidBody;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float topSpeed;
    [SerializeField] private float turnRate;
    [SerializeField] private float accelerationFactor;
    [SerializeField] private float deaccelerationFactor;
    [SerializeField] float inertiaFactor = 5f;

    private float currentTurnSpeed;
    private float currentLinearSpeed;
    private Vector3 lastMoveVector;
    private Vector3 currentMoveVector;
    private Vector3 finalMoveVector;

    private int groundedRes;
    private bool hasJumped;
    private Vector3 groundCollisionCenter;
    private Collider characterCollider;
    private Collider[] groundCollisionResult;

    private void Start()
    {
        if (attactedRigidBody == null)
        {
            Debug.LogError("No Rigidbody assigned in the inspector");
        }

        characterCollider = GetComponent<Collider>();
        groundCollisionResult = new Collider[MovementDefines.Character.FLOOR_COLLIDER_COUNT];
    }

    public void ApplyJumpVelocity(bool isJumping)
    {
        hasJumped = isJumping;

        if (hasJumped && groundedRes > 0)
        {
            attactedRigidBody.linearVelocity = Vector3.up * 8f;
        }
    }

    public void UpdateMovement(Vector3 direction)
    {
        groundCollisionCenter.x = transform.position.x;
        groundCollisionCenter.z = transform.position.z;
        groundCollisionCenter.y = transform.position.y - characterCollider.bounds.extents.y;

        groundedRes = Physics.OverlapBoxNonAlloc(groundCollisionCenter, Vector3.one * MovementDefines.Character.GROUNDED_COLLIDER_SIZE_MUTIPLIER, groundCollisionResult, transform.rotation, groundLayer);

        if (attactedRigidBody.linearVelocity.y < 0 && groundedRes == 0)
        {
            attactedRigidBody.linearVelocity += Vector3.up * Physics.gravity.y * (2.5f - 1) * Time.smoothDeltaTime;
        }
        else if (attactedRigidBody.linearVelocity.y > 0 && !hasJumped)
        {
            attactedRigidBody.linearVelocity += Vector3.up * Physics.gravity.y * (2.0f - 1) * Time.smoothDeltaTime;
        }

        var targetVelocity = GetMoveVector(direction);
        targetVelocity.y = attactedRigidBody.linearVelocity.y;
        attactedRigidBody.linearVelocity = Vector3.Lerp(attactedRigidBody.linearVelocity, targetVelocity, inertiaFactor * Time.deltaTime);
        attactedRigidBody.rotation = GetRotation(currentMoveVector);
    }

    private Vector3 GetMoveVector(Vector3 direction)
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

        currentLinearSpeed += canAccelerate ? accelerationFactor * Time.deltaTime : -(deaccelerationFactor * Time.deltaTime);

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

    private Quaternion GetRotation(Vector3 direction)
    {
        Vector3 turnDirection = new Vector3(direction.x, 0, direction.z);
        float forwardTiltAmount = Mathf.Abs(transform.InverseTransformDirection(attactedRigidBody.linearVelocity).z) * 2.5f;
        float angularTiltAmount = Vector3.Dot(attactedRigidBody.angularVelocity, Vector3.up) * 5f;
        currentTurnSpeed = turnRate * Time.smoothDeltaTime;

        var start = MathDefines.GetAngleFromDirectionXZ(transform.forward);
        var target = MathDefines.GetAngleFromDirectionXZ(turnDirection);
        var angle = MathDefines.InterpolateAngle(start, target, currentTurnSpeed);

        var finalRotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.up);

        return finalRotation;
    }
}
