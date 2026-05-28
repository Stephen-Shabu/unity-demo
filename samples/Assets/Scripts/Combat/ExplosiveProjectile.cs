using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class ExplosiveProjectile : MonoBehaviour, Projectible
{
    public struct BlastRay
    {
    }

    public event Action<WeaponData> OnCreated;
    public event Action<Vector3, Vector3> OnFired;
    public event Action<Projectible> OnCollided;

    public bool HasFired() => hasFired;
    public Vector3 GetPosition() => transform.position;
    public GameObject GetGameObject() => gameObject;

    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileEndDistance;
    [SerializeField] private float bulletRadius = .4f;
    [SerializeField] private float maxDistance = .4f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private LayerMask damageablesMask;
    [SerializeField] private LayerMask environmentMask;

    private Vector3 rayStart;
    private Vector3 rayEnd;
    private Vector3 rayDirection;
    private Vector3 initialPosition;
    private bool hasFired;
    private RaycastHit[] hits = new RaycastHit[1];
    private const int blastRadiusSegments = 16;
    private HashSet<HealthComponent> alreadyHit = new();

    public void Initialise(WeaponData data)
    {
        OnCreated?.Invoke(data);
        gameObject.SetActive(false);
    }

    public void SetProjectile(Vector3 start, Vector3 direction)
    {
        rayDirection = direction;
        rayStart = start;
        initialPosition = start;

        transform.position = start;
        transform.forward = direction;
        transform.rotation = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up);
        
        hasFired = true;
        gameObject.SetActive(true);
        OnFired?.Invoke(start, direction);
        alreadyHit.Clear();
    }

    public void UpdatePosition()
    {
        if (hasFired)
        {
            bool shouldExplode = CanUpdatePrimayRay();

            if (shouldExplode)
            {
                shouldExplode = false;
                GenerateBlastRadius();
                hasFired = false;
                OnCollided?.Invoke(this);
            }
        }
    }

    private bool CanUpdatePrimayRay()
    {
        rayEnd = rayStart + (rayDirection * projectileSpeed);

        if (IsSphereCast() > 0)
        {
            return IsSphereCast() > 0;
        }

        if (Vector3.Distance(initialPosition, rayEnd) >= projectileEndDistance)
        {
            return true;
        }

        rayStart = rayEnd;
        DebugExtension.DrawWireSphere(transform.position, Color.red, bulletRadius);
        transform.position = rayStart;
        transform.forward = rayDirection;

        return false;
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
            damageablesMask | environmentMask
        );

        return hitCount;
    }

    private void GenerateBlastRadius()
    {
        for (int i = 0; i < blastRadiusSegments; i++)
        {
            var angleSpacing = 360 / blastRadiusSegments;
            float angle = angleSpacing * i;
            Vector3 blastDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            Vector3 origin = transform.position;
            Vector3 target = origin + blastDirection * explosionRadius;

            Debug.DrawLine(origin, target, Color.red);

            if(Physics.Raycast(origin, blastDirection, out RaycastHit hit, explosionRadius, environmentMask | damageablesMask))
            {
                Vector3 blockedPoint = hit.point;

                HandleBlastInDirection(origin);
            }
            else
            {
                Vector3 fullPoint = target;

                HandleBlastInDirection(origin);
            }
        }
    }

    private void HandleBlastInDirection(Vector3 origin)
    {
        var hitColliders = Physics.OverlapSphere(origin, explosionRadius, damageablesMask);

        if (hitColliders.Length > 0)
        {
            for(int i = 0, length = hitColliders.Length; i < length; i++)
            {
                float dist = Vector3.Distance(transform.position, hitColliders[i].transform.position);
                Vector3 dir = (hitColliders[i].transform.position - transform.position).normalized;

                if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, environmentMask))
                {
                    continue;
                }

                var healthComp = hitColliders[i].GetComponent<HealthComponent>();
                if (healthComp != null && !alreadyHit.Contains(healthComp))
                {
                    alreadyHit.Add(healthComp);
                    healthComp.ReactToHit(dir);
                }
            }
        }
    }

    private void StopProjectile()
    {
        hasFired = false;
        gameObject.SetActive(false);
    }
}
