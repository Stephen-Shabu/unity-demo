using System;
using UnityEngine;

public class RaycastProjectile : MonoBehaviour, Projectible
{
    public bool HasFired() => hasFired;
    public Vector3 GetPosition() => transform.position;
    public Action<Projectible> OnProjectileCollided { get; set; }

    [SerializeField] float projectileSpeed;
    [SerializeField] float projectileEndDistance;
    [SerializeField] float bulletRadius = .4f;
    [SerializeField] float maxDistance = .4f;
    [SerializeField] LayerMask collionLayer;

    private Vector3 rayEnd;
    private Vector3 rayDirection;
    private Vector3 initialPosition;
    private Vector3 rayStart;
    private bool hasFired;
    private Ray projectileRay = new Ray();
    private RaycastHit[] hits = new RaycastHit[1];

    public void Initialise()
    {
        gameObject.SetActive(false);
    }

    public void SetProjectile(Vector3 start, Vector3 direction)
    {
        rayStart = start;
        initialPosition = start;
        transform.position = rayStart;

        rayDirection = direction;
        transform.forward = rayDirection;
        hasFired = true;
        gameObject.SetActive(true);
    }

    public void UpdatePosition()
    {
        if (hasFired)
        {
            rayEnd = rayStart + (rayDirection * projectileSpeed);

            projectileRay.origin = rayStart;
            projectileRay.direction = rayDirection;

            if(IsSphereCast() > 0)
            {
                Debug.DrawLine(transform.position, hits[0].point, Color.red);
                Debug.DrawRay(hits[0].point, hits[0].normal * 0.5f, Color.yellow);
                DebugExtension.DrawWireSphere(hits[0].point, Color.red, .4f);

                OnProjectileCollided?.Invoke(this);

                if (hits[0].collider != null && hits[0].collider.TryGetComponent(out HealthComponent healthComp))
                {
                    if (!healthComp.IsHealthZero)
                    {
                        healthComp.ReactToHit(rayDirection);
                    }
                }

                StopProjectile();
            }

            if (Vector3.Distance(initialPosition, rayEnd) >= projectileEndDistance)
            {
                StopProjectile();
            }

            rayStart = rayEnd;
            transform.position = rayStart;
            transform.forward = rayDirection;
        }
    }

    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    private int IsSphereCast()
    {
        Vector3 origin = transform.position;

        int hitCount = Physics.SphereCastNonAlloc(
            origin,
            bulletRadius,
            rayDirection.normalized,
            hits,
            maxDistance,
            collionLayer
        );

        return hitCount;
    }

    private void StopProjectile()
    {
        hasFired = false;
        Initialise();
    }
}
