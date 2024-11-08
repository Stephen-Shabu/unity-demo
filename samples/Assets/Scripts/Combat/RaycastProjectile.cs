using UnityEngine;

public class RaycastProjectile : MonoBehaviour
{
    public bool HasFired => hasFired;

    [SerializeField] float projectileSpeed;
    [SerializeField] float projectileEndDistance;
    [SerializeField] LayerMask collionLayer;

    private Vector3 rayEnd;
    private Vector3 rayDirection;
    private Vector3 initialPosition;
    private Vector3 rayStart;
    private bool hasFired;
    private Ray projectileRay = new Ray();
    private RaycastHit hit = new RaycastHit();

    public void Initialise()
    {
        gameObject.SetActive(false);
    }

    public void SetProjectile(Vector3 start, Vector3 direction)
    {
        rayStart = start;
        initialPosition = start;
        rayDirection = direction;
        hasFired = true;
        gameObject.SetActive(true);
    }

    public void UpdatePosition()
    {
        if (hasFired)
        {
            rayEnd = rayStart + (rayDirection * projectileSpeed);

            projectileRay.origin = rayEnd;
            projectileRay.direction = rayDirection;

            if (Physics.Raycast(projectileRay, out hit, projectileSpeed, collionLayer))
            {
                StopProjecticle();
            }

            if (Vector3.Distance(initialPosition, rayEnd) >= projectileEndDistance)
            {
                StopProjecticle();
            }

            rayStart = rayEnd;
            transform.position = rayStart;
            transform.forward = rayDirection;
        }
    }

    private void StopProjecticle()
    {
        hasFired = false;
        Initialise();
    }
}
