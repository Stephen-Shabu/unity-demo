using UnityEngine;
using System.Collections;
using System.Threading;

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
    private CancellationTokenSource dodgeCTS;
    private IEnumerator dodgeRoutine;

    public void ApplyJumpVelocity(bool isJumping)
    {
        hasJumped = isJumping;

        if (hasJumped && IsGrounded())
        {
            attachedRigidBody.linearVelocity = Vector3.up * 8f;
        }
    }

    public void ApplyDogde(bool hasDogded, DodgeDirection direction)
    {
        this.hasDogded = hasDogded;

        if (hasDogded && !isDogding)
        {
            isDogding = true;
            pivot = transform.position + (transform.forward) * pivotDistance;

            if (dodgeRoutine != null) StopCoroutine(dodgeRoutine);

            var angle = direction.Equals(DodgeDirection.Right) ? -90f : 90f;
            dodgeRoutine = RotateAroundPoint(pivot, Vector3.up, angle, dodgeDuration);
            StartCoroutine(dodgeRoutine);
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
        if(isDogding)
        {
            if (dodgeRoutine != null) StopCoroutine(dodgeRoutine);

            lastMoveVector = transform.forward;
            isDogding = false;
        }
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

    private IEnumerator RotateAroundPoint(Vector3 point, Vector3 axis, float angle, float duration)
    {
        isDogding = true;

        float elapsed = 0f;
        float previousEasedT = 0f;

        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up) * startRotation;
        Vector3 initialOffset = (transform.localPosition - pivot);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = MathDefines.Easing.EaseInOut(t);

            float targetAngle = Mathf.Lerp(0, angle, easedT);

            float angleIncrement = Mathf.Lerp(0, angle, easedT) - Mathf.Lerp(0, angle, previousEasedT);

            var heading = pivot - transform.position;
            var lookRotation = Quaternion.AngleAxis(MathDefines.GetAngleFromDirectionXZ(heading) * Mathf.Rad2Deg, Vector3.up);
            attachedRigidBody.rotation = lookRotation;

            Quaternion currentRotation = Quaternion.Slerp(startRotation, targetRotation, easedT);
            Vector3 rotatedOffset = currentRotation * initialOffset;

            Vector3 v1 = attachedRigidBody.position;
            Quaternion q = Quaternion.AngleAxis(angleIncrement, axis);
            Vector3 v2 = v1 - pivot;
            v2 = q * v2;
            v1 = pivot + v2;

            attachedRigidBody.position = v1;

            previousEasedT = easedT;

            elapsed += Time.deltaTime;

            Debug.Log($"Initial Offset: {initialOffset}, Current Position: {transform.position}, Pivot: {pivot}, Move Pos: ({(pivot + rotatedOffset)}");
            Debug.DrawLine(transform.position, pivot, UnityEngine.Color.red);
            Debug.DrawLine(pivot, pivot + rotatedOffset, UnityEngine.Color.blue);

            yield return new WaitForFixedUpdate();
        }

        lastMoveVector = transform.forward;

        isDogding = false;
    }
}
