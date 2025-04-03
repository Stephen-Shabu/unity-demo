using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TopDownOrbitalCameraComponent : BaseCameraComponent
{
    public override void Initialise(Transform target)
    {
        currentAngle = startAngle * Mathf.Rad2Deg;
    }

    public override void TrackPlayer(Transform target, Vector2 direction)
    {
        if (direction.sqrMagnitude > MovementDefines.Camera.MAGNITUDE_THRESHOLD)
        {
            cameraPanSpeed += (panAcceleration * Time.deltaTime);

            lastLookVector = orbitDirection;
            orbitDirection = direction;
        }
        else
        {
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

        currentAngle += (cameraPanSpeed * orbitDirection.normalized.x * Time.deltaTime);
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
}
