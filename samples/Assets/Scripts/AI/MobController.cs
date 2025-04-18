using UnityEngine;
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
    [SerializeField] private AnimationComponent animComponent;
    [SerializeField] private MovementComponent moveComponent;
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private MeleeComponent meleeComponent;
    [SerializeField] private AudioClip deathSFX;

    private bool hasHealthReachedZero = false;
    private bool canLaunchAttack = false;
    private int neighborCount;
    private MobController[] neighbors;
    private AudioSource source;

    public void Initialize(Transform newTarget)
    {
        healthComponent.OnHealthReachedZero = HandleOnHealthReachedZero;
        healthComponent.OnDeathComplete = HandleDeathComplete;
        healthComponent.OnDamageRecieved = HandleOnDamageTaken;
        healthComponent.Initialise();
        moveComponent.Intialise();
        target = newTarget;
        Debug.Log("Target Changed");
        source = new GameObject($"{name} death audio source").AddComponent<AudioSource>();
    }

    private void HandleOnDamageTaken()
    {
        animComponent.SetHitParameter(true);
    }

    private void HandleOnHealthReachedZero()
    {
        hasHealthReachedZero = !hasHealthReachedZero;
        meleeComponent.CancelMeleeAttack();
        source.PlayOneShot(deathSFX);
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

            source.transform.position = transform.position;
            animComponent.SetMovementParameter(moveComponent.IsMoving, moveComponent.SpeedPercentage);
            moveComponent.UpdateLookDirection(heading);
        }
    }

    private void OnDestroy()
    {
        Destroy(source.gameObject);
    }
}
