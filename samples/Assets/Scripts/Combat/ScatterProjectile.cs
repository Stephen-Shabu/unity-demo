using System;
using System.Collections.Generic;
using UnityEngine;

public class ScatterProjectile : MonoBehaviour, Projectible
{
    struct ScatterRay
    {
        public Ray Ray;
        public Vector3 InitialPosition;
        public Vector3 Direction;
        public Vector3 Start;
        public Vector3 End;
        public bool HasCollided;
        public float MaxProjectileDistance;
        public bool HasExpired => Vector3.Distance(InitialPosition, End) >= MaxProjectileDistance;
    }

    [Serializable]
    public class ScatterRayVisual
    {
        public Transform Transform;
        public MeshRenderer Renderer;

        private float maxDistance;

        public void Init(float maxDist)
        {
            maxDistance = maxDist;
            if (Renderer != null)
                Renderer.enabled = false;
        }

        public void UpdateVisual(Vector3 start, Vector3 end)
        {
            if (Renderer == null || Transform == null)
                return;

            Renderer.enabled = true;

            Vector3 dir = (end - start);
            float length = dir.magnitude;

            if (length <= Mathf.Epsilon)
                return;

            Transform.position = (start + end) * 0.5f; ;
            Transform.forward = dir.normalized;
        }

        public void Hide()
        {
            if (Renderer != null)
                Renderer.enabled = false;
        }
    }

    public event Action<WeaponData> OnCreated;
    public event Action<Vector3, Vector3> OnFired;
    public event Action<Projectible> OnCollided;

    public bool HasFired() => hasFired || hasScattered;
    public Vector3 GetPosition() => transform.position;
    public GameObject GetGameObject() => gameObject;

    [SerializeField] float projectileSpeed;
    [SerializeField] float projectileEndDistance;
    [SerializeField] int ScatterCount = 5;
    [SerializeField] float bulletRadius = .4f;
    [SerializeField] float maxDistance = .4f;
    [SerializeField] LayerMask collionLayer;

    [SerializeField] Renderer primaryVisual;
    [SerializeField] ScatterRayVisual[] scatterVisuals;

    private Vector3 rayEnd;
    private Vector3 rayDirection;
    private Vector3 initialPosition;
    private Vector3 rayStart;
    private bool hasFired;
    private Ray primaryRay = new Ray();
    private RaycastHit[] hits = new RaycastHit[1];
    private List<ScatterRay> scatterRays = new List<ScatterRay>();
    private bool hasScattered;

    public void Initialise(WeaponData data)
    {
        OnCreated?.Invoke(data);
        for (int i = 0; i < ScatterCount; i++)
        {
            scatterRays.Add(new ScatterRay
            {
                Ray = new Ray(),
                Direction = Vector3.zero,
                Start = Vector3.zero,
                End = Vector3.zero,
                MaxProjectileDistance = projectileEndDistance
            });
        }

        if (scatterVisuals != null)
        {
            foreach (var visual in scatterVisuals)
                visual.Init(projectileEndDistance);
        }

        gameObject.SetActive(false);
    }

    public void SetProjectile(Vector3 start, Vector3 direction)
    {
        primaryVisual.enabled = true;
        rayStart = start;
        initialPosition = start;
        transform.position = rayStart;
        for (int i = 0; i < scatterRays.Count; i++)
        {
            var ray = scatterRays[i];
            ray.HasCollided = false;
            scatterRays[i] = ray;
        }

        if (scatterVisuals != null)
        {
            foreach (var visual in scatterVisuals)
                visual.Hide();
        }

        rayDirection = direction;
        transform.forward = rayDirection;
        hasFired = true;
        hasScattered = false;
        gameObject.SetActive(true);
        OnFired?.Invoke(start, direction);
    }

    public void UpdatePosition()
    {
        if (hasFired)
        {
            bool shouldScatter = UpdatePrimaryRay();

            if (shouldScatter)
            {
                OnCollided?.Invoke(this);
                GenerateScatterRays(scatterSpreadAngle:360);
                hasScattered = true;
                hasFired = false;
                primaryVisual.enabled = false;
            }
        }

        if (hasScattered)
        {
            UpdateScatterRays();
        }
    }


    private bool UpdatePrimaryRay()
    {
        rayEnd = rayStart + (rayDirection * projectileSpeed);

        primaryRay.origin = rayStart;
        primaryRay.direction = rayDirection;

        if (IsSphereCast() > 0)
        {
            return IsSphereCast() > 0;    
        }

        if (Vector3.Distance(initialPosition, rayEnd) >= projectileEndDistance)
        {
            return true;
        }

        rayStart = rayEnd;
        transform.position = rayStart;
        transform.forward = rayDirection;

        return false;
    }

    private void UpdateScatterRays()
    {
        int alive = 0;

        for (int i = 0; i < scatterRays.Count; i++)
        {
            var ray = scatterRays[i];
            ray.End = ray.Start + (ray.Direction * projectileSpeed);

            if (ray.HasCollided || ray.HasExpired)
            {
                UpdateVisual(i, ray, hideIfDead: true);

                continue;
            }

            if (Physics.SphereCast(ray.Start, bulletRadius, ray.Direction, out RaycastHit hit, maxDistance, collionLayer))
            {
                if (hit.collider != null && hit.collider.TryGetComponent(out HealthComponent healthComp))
                {
                    if (!healthComp.IsHealthZero)
                    {
                        healthComp.ReactToHit(ray.Direction);
                    }
                }

                ray.HasCollided = true;
                scatterRays[i] = ray;
                UpdateVisual(i, ray, hideIfDead: true);

                continue;
            }

            UpdateVisual(i, ray, hideIfDead: false);
            ray.Start = ray.End;
            ray.Ray.origin = ray.Start;
            ray.Ray.direction = ray.Direction;
            scatterRays[i] = ray;

            alive++;
        }

        if (alive == 0)
        {
            hasScattered = false;
            gameObject.SetActive(false);
        }
    }

    private void UpdateVisual(int index, ScatterRay ray, bool hideIfDead)
    {
        if (scatterVisuals == null || index < 0 || index >= scatterVisuals.Length)
            return;

        var visual = scatterVisuals[index];
        if (visual == null)
            return;

        if (ray.HasCollided || ray.HasExpired)
        {
            if (hideIfDead)
                visual.Hide();
        }
        else
        {
            visual.UpdateVisual(ray.Start, ray.End);
        }
    }

    private void GenerateScatterRays(float scatterSpreadAngle)
    {
        for (int i = 0; i < ScatterCount; i++)
        {
            var angleSpacing = scatterSpreadAngle / ScatterCount;

            var scatterAngle = angleSpacing * i;

            var scatterDirection = Quaternion.Euler(0, scatterAngle + 90, 0) * rayDirection;
            var ray = new Ray(rayStart, scatterDirection);

            var scatterRay = scatterRays[i];
            scatterRay.InitialPosition = transform.position;
            scatterRay.Ray = ray;
            scatterRay.Direction = scatterDirection;
            scatterRay.Start = rayStart;
            scatterRay.End = rayStart;
            scatterRays[i] = scatterRay;
        }
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
}
