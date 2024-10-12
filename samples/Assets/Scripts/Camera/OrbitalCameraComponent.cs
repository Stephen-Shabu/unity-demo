using UnityEngine;

public class OrbitalCameraComponent : BaseCameraComponent
{
    private float currentYAngle;

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
        currentAngle = (currentAngle % FULL_CIRCLE_DEG + FULL_CIRCLE_DEG) % FULL_CIRCLE_DEG;
        float xRadians = (currentAngle * Mathf.Deg2Rad);

        currentYAngle += (cameraPanSpeed * orbitDirection.normalized.y * Time.deltaTime);
        currentYAngle = (currentYAngle % FULL_CIRCLE_DEG + FULL_CIRCLE_DEG) % FULL_CIRCLE_DEG;
        float yRadians = (currentYAngle * Mathf.Deg2Rad);

        float x = Mathf.Sin(xRadians) * Mathf.Cos(yRadians);
        float y = Mathf.Sin(xRadians) * Mathf.Sin(yRadians);
        float z = Mathf.Cos(xRadians);

        Vector3 pointOnSphere = target.position + new Vector3(x, y, z) * orbitDistance;

        float xPosition = target.position.x + (Mathf.Cos(xRadians) * orbitDistance);
        float x2Position = target.position.x + (Mathf.Cos(yRadians) * orbitDistance);
        float yPosition = target.position.y + (Mathf.Sin(yRadians) * orbitDistance);
        float zPosition = target.position.z + Mathf.Sin(xRadians) * orbitDistance;

        float s = target.position.x + Mathf.Cos(xRadians) * Mathf.Cos(yRadians) * orbitDistance;

        //transform.position = Vector3.Lerp(transform.position, pointOnSphere, damping * Time.deltaTime);
        //transform.position = Vector3.Lerp(transform.position, new Vector3(xPosition, 0, zPosition), damping * Time.deltaTime);
        //transform.position = Vector3.Lerp(transform.position, new Vector3(x2Position, yPosition, 0), damping * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, new Vector3(s, yPosition, zPosition), damping * Time.deltaTime);

        var heading = target.position - transform.position;
        Vector3 directionXZ = new Vector3(heading.x, 0, heading.z);
        float verticalAngle = Vector3.Angle(directionXZ.normalized, heading.normalized);

        if (heading.y > 0)
        {
            verticalAngle = -verticalAngle;
        }

        transform.rotation = Quaternion.AngleAxis(MathDefines.GetAngleFromDirectionXZ(heading) * Mathf.Rad2Deg, Vector3.up) *
            Quaternion.AngleAxis(verticalAngle, Vector3.right);
    }
}
