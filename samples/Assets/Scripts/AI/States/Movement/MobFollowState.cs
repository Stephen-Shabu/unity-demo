using UnityEngine;

public class MobFollowState : IMobState
{
    private readonly MobContext ctx;
    private readonly MobStateMachine fsm;

    public MobFollowState(MobContext context, MobStateMachine stateMachine)
    {
        ctx = context;
        fsm = stateMachine;
    }

    public void Enter()
    {
        ctx.ChaseIntensity = 1;
        ctx.AnimComponent.SetAnimUpdateCallback(UpdateAnimation);
    }

    public void Update()
    {
        float dist = Vector3.Distance(ctx.Transform.position, ctx.Target.position);
        float scaledIntensity = Mathf.Clamp01((dist - ctx.StoppingDistance + 0.5f) / ctx.StoppingDistance + 0.5f);

        ctx.Heading = ctx.Target.position - ctx.Transform.position;
        ctx.MoveComponent.UpdateMovement(ctx.Heading.normalized * ctx.ChaseIntensity * scaledIntensity, false);
        ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
        ctx.DetectionComponent.UpdateComponent();
        ctx.AnimComponent.ApplyAnimation();
        if (dist <= ctx.StoppingDistance && ctx.MeleeComponent.CanAttack)
            fsm.ChangeState<MobAnticipateState>();
    }

    public void Exit() { }

    private void UpdateAnimation(Animator animator)
    {
        animator.SetBool("IsRunning", ctx.MoveComponent.IsMoving);
        animator.SetFloat("MovementBlend", ctx.MoveComponent.SpeedPercentage);
    }
}
