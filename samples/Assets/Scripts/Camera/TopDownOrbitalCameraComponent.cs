using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownOrbitalCameraComponent : BaseCameraComponent
{
    public override void Initialise(Transform target, PlayerInput playerInput)
    {
        currentAngle = startAngle * Mathf.Rad2Deg;
        SetControlSchemeParams();

        playerInput.actions["Look"].performed -= OnLook;
        playerInput.actions["Look"].performed += OnLook;
        playerInput.actions["Look"].canceled -= OnLook;
        playerInput.actions["Look"].canceled += OnLook;

        GameEventsEmitter.OnEvent(EventType.ControlsChanged, HandleControlSchemeChanged);
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookVector = context.ReadValue<Vector2>();
    }

    private void HandleControlSchemeChanged(EventData e)
    {
        ControlSchemeEventData data;

        if (e is ControlSchemeEventData value)
        {
            data = value;
            currentScheme = data.Scheme;

            SetControlSchemeParams();
        }
    }

    public override void TrackTarget(Transform target)
    {
        if (lookVector.sqrMagnitude > MovementDefines.Camera.MAGNITUDE_THRESHOLD)
        {
            cameraPanSpeed += (panAcceleration * Time.deltaTime);

            lastLookVector = orbitDirection;
            orbitDirection = lookVector;
        }
        else
        {
            if (currentScheme.Equals(ControlScheme.KeyboardAndMouse))
                orbitDirection = Vector3.zero;
            else
                orbitDirection = lastLookVector;

            cameraPanSpeed -= (panDeacceleration * Time.deltaTime);
        }

        if (cameraPanSpeed > panTopSpeed)
        {
            cameraPanSpeed = panTopSpeed;
        }
        else if (cameraPanSpeed < 0)
        {
            cameraPanSpeed = 0;
        }

        if (currentScheme.Equals(ControlScheme.KeyboardAndMouse))
        {
            currentAngle += (cameraPanSpeed * -orbitDirection.x * Time.deltaTime);
        }
        else
        {
            currentAngle += (cameraPanSpeed * -orbitDirection.normalized.x * Time.deltaTime);
        }

        currentAngle = (currentAngle % MathDefines.FULL_CIRCLE_DEG + MathDefines.FULL_CIRCLE_DEG) % MathDefines.FULL_CIRCLE_DEG;
        float radAngle = (currentAngle * Mathf.Deg2Rad);

        float xPosition = target.position.x + Mathf.Cos(radAngle) * orbitDistance;
        float zPosition = target.position.z + Mathf.Sin(radAngle) * orbitDistance;

        var heading = target.position - transform.position;
        Vector3 directionXZ = new Vector3(heading.x, 0, heading.z);
        float verticalAngle = Vector3.Angle(directionXZ.normalized, heading.normalized);

        if (heading.y > 0)
        {
            verticalAngle = -verticalAngle;
        }

        transform.position = Vector3.Lerp(transform.position, new Vector3(xPosition, orbitHeight, zPosition), damping * Time.deltaTime);
        transform.rotation = Quaternion.AngleAxis(MathDefines.GetAngleFromDirectionXZ(heading) * Mathf.Rad2Deg, Vector3.up) *
            Quaternion.AngleAxis(verticalAngle, Vector3.right);
    }

    private void SetControlSchemeParams()
    {
        switch (currentScheme)
        {
            case ControlScheme.KeyboardAndMouse:
                panAcceleration = GameDefines.CameraSettings.MK_ACC_PAN_SPEED;
                break;
            case ControlScheme.Gamepad:
                panAcceleration = GameDefines.CameraSettings.GPAD_ACC_PAN_SPEED;
                break;
        }
    }
}
