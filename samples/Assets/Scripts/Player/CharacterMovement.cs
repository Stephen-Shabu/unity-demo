using UnityEngine;

public enum DodgeDirection
{
    Left,
    Right,
}

public class CharacterMovement : MovementComponent
{
    [SerializeField] private float pivotDistance = 2f;
    [SerializeField] private float dodgeDuration = .5f;

    private Vector3 pivot;
    private bool hasDogded;
    private bool isDogding;
    private float previousEasedT = 0f;
    private float dodgeAngle = 0f;
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private Vector3 initialOffset;

    public void ApplyJumpVelocity(bool isJumping)
    {
        hasJumped = isJumping;

        if (hasJumped && IsGrounded())
        {
            attachedRigidBody.linearVelocity = Vector3.up * 8f;
        }
    }

    public override void UpdateMovement(Vector3 direction, bool isFiring)
    {
        Debug.DrawLine(pivot, pivot + Vector3.left * .2f, UnityEngine.Color.red);
        Debug.DrawLine(pivot, pivot + Vector3.right * .2f, UnityEngine.Color.red);
        Debug.DrawLine(pivot, pivot + Vector3.up * .2f, UnityEngine.Color.red);
        Debug.DrawLine(pivot, pivot + Vector3.down * .2f, UnityEngine.Color.red);

        if (attachedRigidBody.linearVelocity.y < 0 && !IsGrounded())
        {
            attachedRigidBody.linearVelocity += Vector3.up * Physics.gravity.y * (2.5f - 1) * Time.fixedDeltaTime;
        }
        else if (attachedRigidBody.linearVelocity.y > 0 && !hasJumped)
        {
            attachedRigidBody.linearVelocity += Vector3.up * Physics.gravity.y * (2.0f - 1) * Time.fixedDeltaTime;
        }

        if (!isDogding)
        {
            var targetVelocity = GetMoveVector(direction);
            targetVelocity.y = attachedRigidBody.linearVelocity.y;

            attachedRigidBody.linearVelocity = isFiring ? Vector3.zero : Vector3.Lerp(attachedRigidBody.linearVelocity, targetVelocity, inertiaFactor * Time.fixedDeltaTime);
            forwardSpeed = Mathf.Abs(transform.InverseTransformDirection(attachedRigidBody.linearVelocity).z);
            UpdateLookDirection(currentMoveVector);
        }
    }

    public virtual void InterupDodge()
    {
        lastMoveVector = transform.forward;
        isDogding = false;
    }

    public void SetDodge(DodgeDirection direction)
    {
        isDogding = true;
        previousEasedT = 0;
        dodgeAngle = direction.Equals(DodgeDirection.Right) ? -90f : 90f;
        pivot = transform.position + (transform.forward) * pivotDistance;

        startRotation = transform.localRotation;
        targetRotation = Quaternion.AngleAxis(dodgeAngle, Vector3.up) * startRotation;
        initialOffset = (transform.localPosition - pivot);
    }

    public void UpdateDodge(float easedT)
    {
        float targetAngle = Mathf.Lerp(0, dodgeAngle, easedT);

        float angleIncrement = Mathf.Lerp(0, dodgeAngle, easedT) - Mathf.Lerp(0, dodgeAngle, previousEasedT);

        var heading = pivot - transform.position;
        var lookRotation = Quaternion.AngleAxis(MathDefines.GetAngleFromDirectionXZ(heading) * Mathf.Rad2Deg, Vector3.up);
        attachedRigidBody.rotation = lookRotation;

        Quaternion currentRotation = Quaternion.Slerp(startRotation, targetRotation, easedT);
        Vector3 rotatedOffset = currentRotation * initialOffset;

        Vector3 v1 = attachedRigidBody.position;
        Quaternion q = Quaternion.AngleAxis(angleIncrement, Vector3.up);
        Vector3 v2 = v1 - pivot;
        v2 = q * v2;
        v1 = pivot + v2;

        attachedRigidBody.position = v1;

        previousEasedT = easedT;

        Debug.Log($"Initial Offset: {initialOffset}, Current Position: {transform.position}, Pivot: {pivot}, Move Pos: ({(pivot + rotatedOffset)}");
        Debug.DrawLine(transform.position, pivot, UnityEngine.Color.red);
        Debug.DrawLine(pivot, pivot + rotatedOffset, UnityEngine.Color.blue);
    }

    public void CompleteDodge()
    {
        lastMoveVector = transform.forward;
        isDogding = false;
    }
}
