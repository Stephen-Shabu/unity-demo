using UnityEngine;

public class FollowCameraComponent : BaseCameraComponent
{
    public override void Initialise(Transform target)
    {
        Vector3 offset = transform.position - target.position;
        currentAngle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
    }

    public override void TrackPlayer(Transform target, Vector2 direction)
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
