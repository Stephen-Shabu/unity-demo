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

    public void Initialize(Transform newTarget)
    {
        healthComponent.OnHealthReachedZero = HandleOnHealthReachedZero;
        target = newTarget;
        Debug.Log("Target Changed");
    }

    private void HandleOnHealthReachedZero()
    {
        hasHealthReachedZero = !hasHealthReachedZero;
        OnHealthReachedZero?.Invoke();
        meleeComponent.CancelMeleeAttack();
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            var moveDirection = new Vector3(inputVector.x, 0, inputVector.y);

            if (!hasHealthReachedZero && Vector3.Distance(transform.position, target.position) > stoppingDistance)
            {
                heading = target.position - transform.position;
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
            }

            moveComponent.UpdateMovement(heading.normalized * intensity, false);
        }
    }
}
