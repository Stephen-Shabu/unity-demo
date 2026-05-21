using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class ScatterTraceProjectile : MonoBehaviour, Projectible
{
    public event Action<WeaponData> OnCreated;
    public event Action<Vector3, Vector3> OnFired;
    public event Action<Projectible> OnCollided;

    public bool HasFired() => hasFired;
    public Vector3 GetPosition() => transform.position;
    public GameObject GetGameObject() => gameObject;

    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileEndDistance;
    [SerializeField] private float bulletRadius = .4f;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private int traceCount = 2;

    private Vector3 rayStart;
    private Vector3 rayEnd;
    private Vector3 rayDirection;
    private Vector3 initialPosition;
    private bool hasFired;
    private int tracesLeft;

    public void Initialise(WeaponData data)
    {
        OnCreated?.Invoke(data);
        gameObject.SetActive(false);
    }

    public void SetProjectile(Vector3 start, Vector3 direction)
    {
        rayStart = start;
        initialPosition = start;
        transform.position = rayStart;
        tracesLeft = traceCount;
        rayDirection = direction;
        transform.forward = rayDirection;
        hasFired = true;
        gameObject.SetActive(true);
        OnFired?.Invoke(start, direction);
    }

    public void UpdatePosition()
    {
        if (hasFired)
        {
            rayEnd = rayStart + (rayDirection * projectileSpeed);

            if(Physics.Linecast(rayStart, rayEnd, out RaycastHit hit, collisionMask))
            {
                OnCollided?.Invoke(this);
                var reflectDirection = Vector3.Reflect(rayDirection, hit.normal);
                reflectDirection.y = 0;
                rayDirection = reflectDirection;

                if (hit.collider != null && hit.collider.TryGetComponent(out HealthComponent healthComp))
                {
                    if (!healthComp.IsHealthZero)
                    {
                        healthComp.ReactToHit(rayDirection);
                    }
                }
                initialPosition = transform.position;
                tracesLeft--;
            }
            Debug.DrawLine(transform.position, rayEnd, Color.red);

            if (tracesLeft == 0)
            {
                StopProjectile();
            }

            if (tracesLeft <  traceCount && Vector3.Distance(initialPosition, rayEnd) >= projectileEndDistance)
            {
                StopProjectile();
            }

            rayStart = rayEnd;
            transform.position = rayStart;
            transform.forward = rayDirection;            
        }
    }

    private void StopProjectile()
    {
        hasFired = false;
        gameObject.SetActive(false);
    }
}
