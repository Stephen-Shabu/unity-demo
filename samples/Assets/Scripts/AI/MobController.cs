using UnityEngine;
using System;
using NUnit.Framework.Constraints;

public class MobController : MonoBehaviour, AttackCapable, Attackable
{
    [Range(0, 1)][SerializeField] private float intensity = 1;
    [SerializeField] private float stoppingDistance = 5;
    [SerializeField] private Transform target;
    [SerializeField] private AnimationComponent animComponent;
    [SerializeField] private MovementComponent moveComponent;
    [SerializeField] private DetectionComponent detectionComponent;
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private HealthFXComponent healthFxComponent;
    [SerializeField] private MeleeComponent meleeComponent;
    [SerializeField] private AudioComponent audioComponent;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField] private LayerMask wallLayer;

    private AudioSource source;
    private MobContext mobContext;
    private MobStateMachine mobStateMachine;

    public void Initialize(Transform newTarget)
    {
        mobStateMachine = new MobStateMachine();

        mobContext = new MobContext()
        {
            Transform = transform,
            Target = newTarget,
            MoveComponent = moveComponent,
            DetectionComponent = detectionComponent,
            AnimComponent = animComponent,
            MeleeComponent = meleeComponent,
            AudioComponent = audioComponent,
            StoppingDistance = stoppingDistance,
            WallLayer = wallLayer,
            FSM = mobStateMachine
        };


        mobStateMachine.AddState(new WaitState(mobContext, mobStateMachine));
        mobStateMachine.AddState(new FollowState(mobContext, mobStateMachine));
        mobStateMachine.AddState(new AvoidState(mobContext, mobStateMachine));
        mobStateMachine.AddState(new AttackState(mobContext, mobStateMachine));
        mobStateMachine.AddState(new RepositionState(mobContext, mobStateMachine));
        mobStateMachine.AddState(new DeathState(mobContext, mobStateMachine));
        mobStateMachine.AddState(new HitReactState(mobContext, mobStateMachine));

        healthComponent.Initialise();
        healthFxComponent.Initialise();
        moveComponent.Intialise();
        audioComponent.Initialise();

        detectionComponent.OnObstacleDetected = mobContext.HandleObstacleDetected;

        healthComponent.OnDamageTaken -= mobContext.HandleOnDamageTaken;
        healthComponent.OnDamageTaken += mobContext.HandleOnDamageTaken;
        healthComponent.OnDeathStarted -= mobContext.HandleOnDeathStarted;
        healthComponent.OnDeathStarted += mobContext.HandleOnDeathStarted;

        target = newTarget;
        source = new GameObject($"{name} death audio source").AddComponent<AudioSource>();
        GameEventsEmitter.EmitEvent(EventType.RequestAttack, new AttackRequestEventData { Type = EventType.RequestAttack, Attacker = gameObject });
    }

    public void Wait(Transform target)
    {
        mobStateMachine.ChangeState<WaitState>();
    }

    public void Attack(Transform newTarget)
    {
        mobContext.Target = newTarget;
        mobStateMachine.ChangeState<FollowState>(mobContext);
    }

    public void Retreat(){}

    public void UpdateController()
    {
        mobStateMachine.Update();
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
