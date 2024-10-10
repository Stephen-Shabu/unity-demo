using UnityEditor;
using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    [SerializeField] private Rigidbody attactedRigidBody;
    [SerializeField] private float topSpeed;
    [SerializeField] private float topAngularSpeed;
    [SerializeField] private float accelerationFactor;
    [SerializeField] private float deaccelerationFactor;
    [SerializeField] private float angularAccelerationFactor;
    [SerializeField] float inertiaFactor = 5f;

    private float currentTurnSpeed;
    private float currentLinearSpeed;
    private Vector3 lastMoveVector;
    private Vector3 currentMoveVector;
    private Vector3 finalMoveVector;

    private void Start()
    {
        if (attactedRigidBody == null)
        {
            Debug.LogError("No Rigidbody assigned in the inspector");
        }
    }

    public void UpdateMovement(Vector2 direction)
    {
        var targetVelocity = GetMoveVector(direction);
        targetVelocity.y = attactedRigidBody.linearVelocity.y;
        attactedRigidBody.linearVelocity = Vector3.Lerp(attactedRigidBody.linearVelocity, targetVelocity, inertiaFactor * Time.deltaTime);
        attactedRigidBody.rotation = GetRotation(currentMoveVector);
    }

    private Vector3 GetMoveVector(Vector2 direction)
    {
        var canAccelerate = direction.sqrMagnitude > 0.25f;
        if (canAccelerate)
        {
            lastMoveVector = currentMoveVector;
            currentMoveVector = new Vector3(direction.x, 0, direction.y);
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
        currentTurnSpeed = topAngularSpeed * Time.smoothDeltaTime;

        float GetAngleFromDirection(Vector3 direction)
        {
            return Mathf.Atan2(direction.x, direction.z);
        }

        float InterpolateAngle(float startAngle, float targetAngle, float t)
        {
            var diff = targetAngle - startAngle;

            diff += 2 * Mathf.PI;

            diff = (diff + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;

            var interpolatedaAngle = startAngle + t * diff;

            return interpolatedaAngle;
        }


        var start = GetAngleFromDirection(transform.forward);
        var target = GetAngleFromDirection(turnDirection);
        var angle = InterpolateAngle(start, target, currentTurnSpeed);

        var finalRotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.up);

        return finalRotation;
    }
}
