using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    [SerializeField] private Rigidbody attactedRigidBody;
    [SerializeField] private float topSpeed;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float accelerationFactor;
    [SerializeField] private float deaccelerationFactor;
    [SerializeField] private float stoppingFactor;
    [SerializeField] float inertiaFactor = 5f;

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
    }

    private Vector3 GetMoveVector(Vector2 direction)
    {
        var canAccelarate = direction.sqrMagnitude > 0.25f;
        if (canAccelarate)
        {
            lastMoveVector = currentMoveVector;
            currentMoveVector = direction;
        }
        else
        {
            currentMoveVector = lastMoveVector;
        }
       
        var targetSpeed = canAccelarate ? topSpeed : 0;
        var accelFactor = canAccelarate ? accelerationFactor : deaccelerationFactor;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, accelFactor * Time.smoothDeltaTime);

        var moveVector = currentMoveVector * currentSpeed;
        finalMoveVector.x = moveVector.x;
        finalMoveVector.z = moveVector.y;

        return finalMoveVector;
    }

    private Quaternion GetRotation()
    {
        return  Quaternion.identity;
    }
}
