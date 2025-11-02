using System;
using System.Collections.Generic;
using UnityEngine;

public class SweepcastProjectile : MonoBehaviour, Projectible
{
    public event Action<WeaponData> OnCreated;
    public event Action<Vector3, Vector3> OnFired;
    public event Action<Projectible> OnCollided;
    public bool HasFired() => hasFired;
    public Vector3 GetPosition() => transform.position;
    public GameObject GetGameObject() => gameObject;

    [SerializeField] float projectileSpeed;
    [SerializeField] float projectileEndDistance;
    [SerializeField] float minSweepLength = .4f;
    [SerializeField] float maxSweepLength = 1f;
    [SerializeField] LayerMask collionLayer;
    [SerializeField] ParticleSystem projectileVfx;
    [SerializeField] ParticleSystem projectileTrailVfx;

    private RaycastHit hit = new RaycastHit();
    private float sweepLength = 0f;
    private bool hasFired;
    private Vector3 castStart;
    private Vector3 castEnd;
    private Vector3 castDirection;
    private Vector3 initialPosition;
    private const float SPREAD_SPEED = 5f;
    private Collider[] hits = { };
    private HashSet<HealthComponent> alreadyHit = new();

    public void Initialise(WeaponData data)
    {
        OnCreated?.Invoke(data);
        gameObject.SetActive(false);
        sweepLength = minSweepLength;
    }

    public void SetProjectile(Vector3 start, Vector3 direction)
    {
        castStart = start;
        initialPosition = start;
        transform.position = castStart;

        castDirection = direction;
        transform.forward = castDirection;
        hasFired = true;
        gameObject.SetActive(true);
        OnFired?.Invoke(start, direction);
    }

    private void Update()
    {
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (hasFired)
        {
            castEnd = castStart + (castDirection * projectileSpeed);

            if (IsSweepCast())
            {
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent(out HealthComponent healthComp))
                    {
                        if (!healthComp.IsHealthZero && !alreadyHit.Contains(healthComp))
                        {
                            alreadyHit.Add(healthComp);
                            healthComp.ReactToHit(castDirection);
                        }
                    }
                    else
                    {
                        OnCollided?.Invoke(this);
                        StopProjectile();
                    }
                }
            }

            if (Vector3.Distance(initialPosition, castEnd) >= projectileEndDistance)
            {
                StopProjectile();
            }

            castStart = castEnd;
            transform.position = castStart;
            transform.forward = castDirection;
        }

    }

    private bool IsSweepCast()
    {
        sweepLength = Mathf.Lerp(sweepLength, maxSweepLength, SPREAD_SPEED * Time.deltaTime);
        
        var sizeModule = projectileVfx.sizeOverLifetime;
        var mainModule = projectileVfx.main;
        var trailShapeModule = projectileTrailVfx.sizeOverLifetime;

        mainModule.startSize = minSweepLength;
        sizeModule.size = sweepLength * 4;
        trailShapeModule.size.curve.keys[0].value = sweepLength * 4;

        Vector3 line = new Vector3(sweepLength, .01f, .01f);
        hits = Physics.OverlapBox(transform.position, line, Quaternion.LookRotation(castDirection), collionLayer);

        return hits.Length > 0;
    }

    private void StopProjectile()
    {
        hasFired = false;
        gameObject.SetActive(false);
        sweepLength = 0;
        alreadyHit.Clear();
    }

    private void OnDrawGizmos()
    {
        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.LookRotation(castDirection), new Vector3(sweepLength, .01f, .01f) * 2f);

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = oldMatrix;
    }
}
