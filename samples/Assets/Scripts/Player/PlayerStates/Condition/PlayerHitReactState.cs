using UnityEngine;

public class PlayerHitReactState: PlayerStateCapable
{
    private readonly PlayerStateMachine conditionFSM;
    private readonly PlayerContext ctx;
    private float reactTimer = 0f;
    private Vector3 hitAxis = Vector3.zero;
    private float hitImpulse = 5f;

    public PlayerHitReactState(PlayerContext context, PlayerStateMachine machine)
    {
        conditionFSM = machine;
        ctx = context;
    }

    public void Enter()
    {
        ctx.Rigidbody.linearVelocity += ctx.HitDirection * hitImpulse;
        reactTimer = 0;
        ctx.AnimComponent.SetHitParameter(true);
        Vector3 localHitDirection = ctx.Transform.InverseTransformDirection(ctx.HitDirection);
        hitAxis = Vector3.Cross(Vector3.up, localHitDirection);
        ctx.MoveComponent.ApplyLean(MovementDefines.Character.HIT_LEAN_ANGLE, hitAxis, 10);
        reactTimer = 0f;

        ctx.MoveComponent.InterupDodge();
        GameEventsEmitter.EmitEvent(EventType.HitRegistered, new HitRegisterEventData { Type = EventType.HitRegistered, Owner = HitRegisterEventData.HitOwner.Player });
    }

    public void Exit()
    {

    }

    public void Update()
    {
        reactTimer += Time.fixedDeltaTime;

        if (reactTimer > MovementDefines.Character.HIT_REACT_DURATION)
        {
            ctx.MoveComponent.ApplyLean(0, hitAxis, 1);
            conditionFSM.ChangeState<PlayerNormalState>();
        }
    }
}