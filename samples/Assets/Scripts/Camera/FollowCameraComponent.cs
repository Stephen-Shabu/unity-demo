using UnityEngine;
using UnityEngine.InputSystem;

public class FollowCameraComponent : BaseCameraComponent
{
    public override void Initialise(Transform target, PlayerInput playerInput)
    {
        Vector3 offset = transform.position - target.position;
        currentAngle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;

        playerInput.actions["Look"].performed -= OnLook;
        playerInput.actions["Look"].performed += OnLook;
        playerInput.actions["Look"].canceled -= OnLook;
        playerInput.actions["Look"].canceled += OnLook;
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookVector = context.ReadValue<Vector2>();
    }

    public override void TrackTarget(Transform target)
    {
        var eye = target.position - target.forward * orbitDistance + target.up * orbitHeight;
        var camForward = target.position - eye;
        camForward.Normalize();

        var camLeft = Vector3.Cross(target.up, camForward);
        camLeft.Normalize();

        var camUp = Vector3.Cross(camForward, camLeft);
        camUp.Normalize();

        transform.position = eye;
        transform.LookAt(target.position, camUp);
    }
}
