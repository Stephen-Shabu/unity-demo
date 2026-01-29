using UnityEngine;

public class HitReactState : IMobState
{
    private MobContext ctx;
    private readonly MobStateMachine fsm;
    private float reactTimer = 0f;
    private Vector3 hitAxis = Vector3.zero;
    private float hitImpulse = 5f;

    public HitReactState(MobContext context, MobStateMachine machine)
    {
        ctx = context;
        fsm = machine;
    }

    public void Enter(MobContext newContext = null)
    {
        if (newContext != null)
        {
            ctx = newContext;
        }
        ctx.Rigidbody.linearVelocity += ctx.HitDirection * hitImpulse;
        ctx.AudioComponent.PlayAudio(1f, 0.1f, ctx.AudioComponent.AudioProfile.GetAudioByType(AudioType.Hit));
        ctx.AnimComponent.SetHitParameter(true);
        Vector3 localHitDirection = ctx.Transform.InverseTransformDirection(ctx.HitDirection);
        hitAxis = Vector3.Cross(Vector3.up, localHitDirection);
        ctx.MoveComponent.ApplyLean(MovementDefines.Character.HIT_LEAN_ANGLE, hitAxis, 10);
        reactTimer = 0f;
        GameEventsEmitter.EmitEvent(EventType.HitRegistered, new HitRegisterEventData { Type = EventType.HitRegistered, Owner = HitRegisterEventData.HitOwner.Mob });
    }

    public void Exit()
    {
        ctx.MoveComponent.ApplyLean(0, hitAxis, 1);
    }

    public void Update()
    {
        reactTimer += Time.deltaTime;

        float dist = Vector3.Distance(ctx.Transform.position, ctx.Target.position);
        float scaledIntensity = Mathf.Clamp01((dist - ctx.StoppingDistance + 0.5f) / ctx.StoppingDistance + 0.5f);

        ctx.Heading = ctx.Target.position - ctx.Transform.position;
        ctx.MoveComponent.UpdateMovement(ctx.Heading.normalized * ctx.ChaseIntensity * scaledIntensity, false);
        ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
        ctx.DetectionComponent.UpdateComponent();
        ctx.AnimComponent.SetMovementParameter(ctx.MoveComponent.IsMoving, ctx.MoveComponent.SpeedPercentage);

        if (reactTimer > MovementDefines.Character.HIT_REACT_DURATION)
        {
            ctx.MoveComponent.ApplyLean(0, hitAxis, 1);
            fsm.ReturnToLastState();
        }
    }
}
