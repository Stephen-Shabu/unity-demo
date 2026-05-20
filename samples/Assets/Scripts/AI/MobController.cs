using UnityEngine;
using System;

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
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField] private LayerMask wallLayer;

    private AudioSource source;
    private MobContext mobContext;
    private MobStateMachine movementFSM;
    private MobStateMachine actionFSM;
    private MobStateMachine conditionFSM;

    public void Initialize(Transform newTarget, int index)
    {
        mobContext = new MobContext()
        {
            Index = index,
            Rigidbody = rb,
            Transform = transform,
            Target = newTarget,
            MoveComponent = moveComponent,
            DetectionComponent = detectionComponent,
            AnimComponent = animComponent,
            MeleeComponent = meleeComponent,
            AudioComponent = audioComponent,
            HealthComponent = healthComponent,
            StoppingDistance = stoppingDistance,
            WallLayer = wallLayer
        };

        movementFSM = new MobStateMachine();
        movementFSM.AddState(new MobMovementState(mobContext, movementFSM));
        movementFSM.ChangeState<MobMovementState>();

        actionFSM = new MobStateMachine();
        actionFSM.AddState(new MobActionState(mobContext, actionFSM));
        actionFSM.ChangeState<MobActionState>();

        conditionFSM = new MobStateMachine();
        conditionFSM.AddState(new MobConditionState(mobContext, conditionFSM));
        conditionFSM.ChangeState<MobConditionState>();

        healthComponent.Initialise();
        healthFxComponent.Initialise();
        moveComponent.Intialise();
        audioComponent.Initialise();

        target = newTarget;
        source = new GameObject($"{name} death audio source").AddComponent<AudioSource>();
        GameEventsEmitter.EmitEvent(EventType.RequestAttack, new AttackRequestEventData { Type = EventType.RequestAttack, Attacker = gameObject });
    }

    public void Wait(Transform target)
    {
        mobContext.RaiseOnWaitForTarget();
    }

    public void Attack(Transform newTarget)
    {
        mobContext.Target = newTarget;
        mobContext.RaiseOnFollowTarget();
    }

    public void Retreat(){ }

    public void UpdateController()
    {
        conditionFSM.Update();
        movementFSM.Update();
        actionFSM.Update();
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
