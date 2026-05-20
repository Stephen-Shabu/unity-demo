using UnityEngine;

public class MobDeathState : IMobState
{
    private MobContext ctx;
    private readonly MobStateMachine fsm;
    private float deathImpulse = 10f;

    public MobDeathState(MobContext context, MobStateMachine machine)
    {
        ctx = context;
        fsm = machine;
    }

    public void Enter()
    {
        ctx.Rigidbody.linearVelocity += ctx.HitDirection * deathImpulse;

        ctx.AnimComponent.SetAnimUpdateCallback(UpdateAnimation);

        ctx.AudioComponent.PlayAudio(1f, 0.1f, ctx.AudioComponent.AudioProfile.GetAudioByType(AudioType.Death));
        ctx.AnimComponent.ApplyAnimation();
        ctx.MeleeComponent.CancelMeleeAttack();
    }


    public void Update()
    {
        ctx.MoveComponent.UpdateMovement(Vector3.zero, false);
        ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
    }

    public void Exit() { }

    private void UpdateAnimation(Animator animator)
    {
        animator.SetBool("IsRunning", false);
        animator.SetFloat("MovementBlend", ctx.MoveComponent.SpeedPercentage);
    }
}
