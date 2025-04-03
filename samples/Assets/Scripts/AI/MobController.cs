using UnityEngine;
using static GameEvents.EventsCollection;
using System;

public class MobController : MonoBehaviour
{
    public Action OnHealthReachedZero;

    [SerializeField] private Vector2 inputVector = Vector2.zero;
    [SerializeField] private Vector2 lookVector = Vector2.zero;
    [SerializeField] private Vector3 heading = Vector3.zero;
    [Range(0, 1)][SerializeField] private float intensity = 1;
    [SerializeField] private float stoppingDistance = 5;
    [SerializeField] private Transform target;
    [SerializeField] private MovementComponent moveComponent;
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private MeleeComponent meleeComponent;

    private bool hasHealthReachedZero = false;
    private bool canLaunchAttack = false;
    private int neighborCount;
    private MobController[] neighbors;

    public void Initialize(Transform newTarget)
    {
        healthComponent.OnHealthReachedZero = HandleOnHealthReachedZero;
        healthComponent.OnDeathComplete = HandleDeathComplete;
        healthComponent.OnDamageRecieved = HandleOnDamageTaken;
        healthComponent.Initialise();
        moveComponent.Intialise();
        target = newTarget;
        Debug.Log("Target Changed");
    }

    private void HandleOnDamageTaken()
    {
        animComponent.SetHitParameter(true);
    }

    private void HandleOnHealthReachedZero()
    {
        hasHealthReachedZero = !hasHealthReachedZero;
        meleeComponent.CancelMeleeAttack();
    }

    private void HandleDeathComplete()
    {
        OnHealthReachedZero?.Invoke();
    }

    public void SetNeighbors(MobController[] mobs)
    {
        neighbors = mobs;
    }

    public void UpdateController()
    {
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (!hasHealthReachedZero && distanceToTarget > stoppingDistance)
            {
                float scaledIntensity = Mathf.Clamp01((distanceToTarget - stoppingDistance) / stoppingDistance);
                heading = target.position - transform.position;
                moveComponent.UpdateMovement(heading.normalized * intensity * scaledIntensity, false);
            }
            else
            {
                if (!canLaunchAttack)
                {
                    canLaunchAttack = true;
                    meleeComponent.LaunchMeleeAttack(heading, () => { canLaunchAttack = false; });
                    Debug.Log("Lauch Attack");
                }

                heading = Vector3.zero;
                moveComponent.UpdateMovement(heading, false);
            }

            moveComponent.UpdateLookDirection(heading);
        }
    }
}
