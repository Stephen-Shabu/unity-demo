using UnityEngine;

public class OrbitalCameraComponent : BaseCameraComponent
{
    [Range(-89, 0)]
    [SerializeField] float minYAngle = 0;

    [Range(0, 89f)]
    [SerializeField] float maxYAngle = 0;

    private float currentYAngle;

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
            orbitDirection = Vector3.zero;
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

        float targetXAngle = currentAngle + (cameraPanSpeed * orbitDirection.x * Time.deltaTime);
        float targetYAngle = currentYAngle + (cameraPanSpeed * -orbitDirection.y * Time.deltaTime);

        currentAngle = Mathf.Lerp(currentAngle, targetXAngle, damping * Time.deltaTime);
        currentYAngle = Mathf.Lerp(currentYAngle, targetYAngle, damping * Time.deltaTime);

        currentYAngle = Mathf.Clamp(currentYAngle, minYAngle, maxYAngle);

        float xRadians = targetXAngle * Mathf.Deg2Rad;
        float yRadians = Mathf.Clamp(targetYAngle, minYAngle, maxYAngle) * Mathf.Deg2Rad;

        float xPosition = orbitDistance * Mathf.Sin(xRadians) * Mathf.Cos(yRadians);
        float yPosition = orbitDistance * Mathf.Sin(yRadians);
        float zPosition = orbitDistance * Mathf.Cos(xRadians) * Mathf.Cos(yRadians);

        Vector3 orbitPosition = new Vector3(xPosition, yPosition, zPosition) + target.position;
        transform.position = Vector3.Lerp(transform.position, orbitPosition, damping * Time.deltaTime);

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
