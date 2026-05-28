using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class TimedProjectile : MonoBehaviour, Projectible, Chargeable
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
    [SerializeField] private float maxDistance = .4f;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private Renderer projectileRenderer;
    [SerializeField] private Color chargeBaseColor;
    [SerializeField] private Color chargeMiddleColor;
    [SerializeField] private Color chargeFinalColor;   
    [SerializeField] private float chargeStage1Time = 1.4f;
    [SerializeField] private float chargeStage2Time = 1.7f;
    [SerializeField] private float chargeBaseScale = 1.0f;
    [SerializeField] private float chargeStage1Scale = 1.2f;
    [SerializeField] private float chargeStage2Scale = 1.5f;

    private Vector3 rayStart;
    private Vector3 rayEnd;
    private Vector3 rayDirection;
    private Vector3 initialPosition;
    private bool hasFired;
    private bool isCharging;
    private int chargeStage;
    private Color chargeColor;
    private float scale = 1f;

    public void Initialise(WeaponData data)
    {
        OnCreated?.Invoke(data);
        gameObject.SetActive(false);
        isCharging = false;
    }

    public void StartCharge(Vector3 start, Vector3 direction)
    {
        gameObject.SetActive(true);
        projectileRenderer.material.color = chargeBaseColor;
        projectileRenderer.gameObject.transform.localScale = new Vector3(1, 1, 1);
        isCharging = true;
        transform.position = start;
        transform.forward = direction;
    }

    public void UpdateCharge(float chargeTime, Vector3 start, Vector3 direction)
    {
            transform.position = start;
            transform.forward = direction;        
            UpdateCharge(chargeTime);
    }

    public void SetProjectile(Vector3 start, Vector3 direction)
    {
        isCharging = false;
        initialPosition = start;
        rayDirection = direction;
        rayStart = start;
        transform.position = start;
        transform.forward = direction;
        hasFired = true;
        chargeStage = 0;
        gameObject.SetActive(true);
        OnFired?.Invoke(start, direction);
    }

    public void UpdatePosition()
    {
        if (hasFired)
        {
            Vector3 travel = rayDirection * projectileSpeed;
            float travelDistance = travel.magnitude;
            rayEnd = rayStart + travel;

            if (Physics.SphereCast(rayStart, bulletRadius, rayDirection, out RaycastHit hitInfo, travelDistance, collisionMask))
            {
                transform.position = hitInfo.point;

                StopProjectile();
                OnCollided?.Invoke(this);

                return;
            }
            transform.position = rayEnd;

            if (Vector3.Distance(initialPosition, transform.position) >= projectileEndDistance)
            {
                StopProjectile();
            }

            DebugExtension.DrawWireSphere(rayStart, Color.red, bulletRadius);
            rayStart = rayEnd;            
        }
    }

    public void UpdateCharge(float chargeTime)
    {
        if (chargeTime >= chargeStage2Time)
        {
            chargeStage = 2;

            var perc = (chargeTime - chargeStage2Time) / chargeStage2Time;
            chargeColor = Color.Lerp(chargeMiddleColor, chargeFinalColor, perc);
            scale = Mathf.Lerp(chargeStage1Scale, chargeStage2Scale, perc);
            projectileRenderer.material.color = chargeColor;
            projectileRenderer.gameObject.transform.localScale = new Vector3(scale, scale, scale);
        }
        else if (chargeTime >= chargeStage1Time)
        {
            chargeStage = 1;

            var perc = chargeTime / chargeStage2Time;
            chargeColor = Color.Lerp(chargeBaseColor, chargeMiddleColor, perc);
            scale = Mathf.Lerp(chargeBaseScale, chargeStage1Scale, perc);
            projectileRenderer.material.color = chargeColor;
            projectileRenderer.gameObject.transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            chargeStage = 0;
        }
    }

    private void StopProjectile()
    {
        hasFired = false;
        gameObject.SetActive(false);
    }
}
