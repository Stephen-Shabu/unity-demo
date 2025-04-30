using UnityEngine;
using System;
using TMPro;
using System.Collections;

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
    private bool hasLaunchedAttack = false;
    private bool canReposition = false;
    private int neighborCount;
    private MobController[] neighbors;
    private AudioSource source;

    public float formationRadius = 5f;
    private int unitIndex;
    private int totalUnits;
    private Vector3 repositionTarget = Vector3.zero;
    private bool repositionFlag = false;
    private float repositionCoolDownDuration = 3f;
    private float currentRepositionCoolDown = 0;

    public void Initialize(Transform newTarget)
    {
        healthComponent.OnHealthReachedZero = HandleOnHealthReachedZero;
        healthComponent.OnDeathComplete = HandleDeathComplete;
        healthComponent.OnDamageRecieved = HandleOnDamageTaken;
        healthComponent.Initialise();
        moveComponent.Intialise();
        target = newTarget;
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
        GameEventsEmitter.EmitEvent(EventType.EnemyDefeated, new GenericEventData { Type = EventType.EnemyDefeated });
    }

    private void HandleDeathComplete()
    {
        OnHealthReachedZero?.Invoke();
    }

    public void SetNeighbors(MobController[] mobs, int idx)
    {
        unitIndex = idx;
        neighbors = mobs;
        totalUnits = neighbors.Length;
        float radiusMultiplier = 1f + (totalUnits / 10f);
        formationRadius = formationRadius * radiusMultiplier;
    }

    public void UpdateController()
    {
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            float scaledIntensity = Mathf.Clamp01((distanceToTarget - stoppingDistance + 0.5f) / stoppingDistance + 0.5f);

            if (!canReposition)
            {
                heading = target.position - transform.position;
                moveComponent.UpdateMovement(heading.normalized * intensity * scaledIntensity, false);
                moveComponent.UpdateLookDirection(heading);
            }
            else 
            {
                if (!repositionFlag)
                {
                    repositionTarget = CalculateCirclePosition();
                    repositionFlag = true;
                }

                distanceToTarget = Vector3.Distance(transform.position, repositionTarget);
                scaledIntensity = Mathf.Clamp01((distanceToTarget - 1f) / 1f);

                if (distanceToTarget < 1f)
                {
                    heading = Vector3.zero;

                    if (currentRepositionCoolDown < repositionCoolDownDuration)
                    {
                        currentRepositionCoolDown += Time.deltaTime;
                        moveComponent.UpdateLookDirection(target.position - transform.position);
                    }
                    else if (currentRepositionCoolDown >= repositionCoolDownDuration)
                    {
                        currentRepositionCoolDown = 0;
                        canReposition = false;
                        repositionFlag = false;
                        distanceToTarget = Vector3.Distance(transform.position, target.position);
                    }
                }
                else
                {
                    currentRepositionCoolDown = 0;

                    if (currentRepositionCoolDown == 0)
                    {
                        heading = repositionTarget - transform.position;
                    }
                    moveComponent.UpdateLookDirection(heading);
                }

                moveComponent.UpdateMovement(heading.normalized * intensity * scaledIntensity, false);

                Debug.DrawLine(repositionTarget, repositionTarget + Vector3.left * .2f, UnityEngine.Color.red);
                Debug.DrawLine(repositionTarget, repositionTarget + Vector3.right * .2f, UnityEngine.Color.red);
                Debug.DrawLine(repositionTarget, repositionTarget + Vector3.up * .2f, UnityEngine.Color.red);
                Debug.DrawLine(repositionTarget, repositionTarget + Vector3.down * .2f, UnityEngine.Color.red);
            }

            animComponent.SetMovementParameter(moveComponent.IsMoving, moveComponent.SpeedPercentage);
            source.transform.position = transform.position;

            if (meleeComponent.CanAttack(!(distanceToTarget > stoppingDistance)))
            {
                void HandleAttackStartUp() => moveComponent.ApplyLean(MovementDefines.Character.LEAN_ANGLE);
                
                void HandleAttackStarted() 
                { 
                    moveComponent.ApplyLean(0);
                    hasLaunchedAttack = true;
                }

                void HandleAttackComplete() 
                {
                    canReposition = true;

                    hasLaunchedAttack = false; 
                }

                meleeComponent.LaunchMeleeAttack(target, HandleAttackStartUp, HandleAttackStarted, HandleAttackComplete);
            }
        }
    }

   private Vector3 CalculateCirclePosition()
    {
        float angle = UnityEngine.Random.Range(0f, 1f) * MathDefines.FULL_CIRCLE_RAD;

        Vector3 offset = new Vector3(
            Mathf.Sin(angle) * formationRadius, 0f, Mathf.Cos(angle) * formationRadius);

        return target.position + offset;
    }

    private void OnDestroy()
    {
        if(source != null) Destroy(source.gameObject);
    }
}
