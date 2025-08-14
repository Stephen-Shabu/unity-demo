using UnityEngine;
using System;

public class MobController : MonoBehaviour, AttackCapable, Attackable
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
    [SerializeField] private LayerMask wallLayer;

    private bool hasHealthReachedZero = false;
    private bool hasLaunchedAttack = false;
    private AudioSource source;
    private Vector3 repositionTarget = Vector3.zero;
    private bool repositionFlag = false;
    private float repositionCoolDownDuration = 3f;
    private float currentRepositionCoolDown = 0;
    private EnemyState currentState = EnemyState.Wait;

    public void Initialize(Transform newTarget)
    {
        healthComponent.OnHealthReachedZero = HandleOnHealthReachedZero;
        healthComponent.OnDeathComplete = HandleDeathComplete;
        healthComponent.OnDamageRecieved = HandleOnDamageTaken;
        healthComponent.Initialise();
        moveComponent.Intialise();
        target = newTarget;
        source = new GameObject($"{name} death audio source").AddComponent<AudioSource>();
        GameEventsEmitter.EmitEvent(EventType.RequestAttack, new AttackRequestEventData { Type = EventType.RequestAttack, Attacker = gameObject });
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
        GameEventsEmitter.EmitEvent(EventType.EnemyDefeated, new GenericEventData { Type = EventType.EnemyDefeated, Caller = gameObject});
    }

    private void HandleDeathComplete()
    {
        OnHealthReachedZero?.Invoke();
    }

    public void Wait(Transform target)
    {
        currentState = EnemyState.Wait;
        intensity = 0;
    }

    public void Attack(Transform newTarget)
    {
        target = newTarget;
        intensity = 1;
        currentState = EnemyState.Attack;
    }

    public void Retreat()
    {
        currentState = EnemyState.Retreat;
    }

    public void UpdateController()
    {
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            float scaledIntensity = Mathf.Clamp01((distanceToTarget - stoppingDistance + 0.5f) / stoppingDistance + 0.5f);

            if(!currentState.Equals(EnemyState.Reposition))
            {
                if (currentState.Equals(EnemyState.Wander) || currentState.Equals(EnemyState.Attack))
                {
                    intensity = 1;
                }
                else
                {
                    Wait(gameObject.transform);
                }
                heading = target.position - transform.position;
                moveComponent.UpdateMovement(heading.normalized * intensity * scaledIntensity, false);
                moveComponent.UpdateLookDirection(heading);
            }
            else 
            {
                if (!repositionFlag)
                {
                    repositionTarget = CalculateNewTargetPosition();
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

                        Attack(target);
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

            if (meleeComponent.CanAttack(!(distanceToTarget > stoppingDistance)) && currentState.Equals(EnemyState.Attack))
            {
                void HandleAttackStartUp() => moveComponent.ApplyLean(MovementDefines.Character.LEAN_ANGLE);
                
                void HandleAttackStarted() 
                { 
                    moveComponent.ApplyLean(0);
                    hasLaunchedAttack = true;
                }

                void HandleAttackComplete() 
                {
                    currentState = EnemyState.Reposition;
                    hasLaunchedAttack = false; 
                }

                meleeComponent.LaunchMeleeAttack(target, HandleAttackStartUp, HandleAttackStarted, HandleAttackComplete);
            }
        }
    }

   private Vector3 CalculateNewTargetPosition()
    {
        int[] directions = new int[2] {-1, 1};
        int dir = directions[UnityEngine.Random.Range(0, directions.Length)];

        Vector3 offset = Vector3.Cross(Vector3.up, dir * heading);

        var newPosition = target.position + offset * UnityEngine.Random.Range(1f, 2f);
        RaycastHit[] hits = new RaycastHit[1];

        if (Physics.SphereCastNonAlloc(new Ray(newPosition, heading), .1f, hits, .1f, wallLayer) > 0)
        {
            var closePoint = hits[0].collider.ClosestPoint(transform.position);

            offset = Vector3.Cross(Vector3.up, (closePoint - transform.position));
            newPosition = transform.position - offset * UnityEngine.Random.Range(1.2f, 2f);
        }

        return newPosition;
    }

    private void OnDestroy()
    {
        if(source != null) Destroy(source.gameObject);
    }

    public int GetMaxAttackers()
    {
        return 2;
    }
}
