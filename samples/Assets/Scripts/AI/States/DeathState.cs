using UnityEngine;

public class DeathState : IMobState
{
    private MobContext ctx;
    private readonly MobStateMachine fsm;
    private float deathImpulse = 10f;

    public DeathState(MobContext context, MobStateMachine machine)
    {
        ctx = context;
        fsm = machine;
    }

    public void Enter(MobContext newContext = null)
    {
        ctx.Rigidbody.linearVelocity += ctx.HitDirection * deathImpulse;

        if (newContext != null)
        {
            ctx = newContext;
        }

        ctx.AudioComponent.PlayAudio(1f, 0.1f, ctx.AudioComponent.AudioProfile.GetAudioByType(AudioType.Death));
        ctx.AnimComponent.SetMovementParameter(false, ctx.MoveComponent.SpeedPercentage);
        ctx.MeleeComponent.CancelMeleeAttack();
    }

    public void Exit()
    {
    }

    public void Update()
    {
        ctx.MoveComponent.UpdateMovement(Vector3.zero, false);
        ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
    }
}
