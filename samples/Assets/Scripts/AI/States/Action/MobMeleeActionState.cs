using UnityEngine;

public class MobMeleeActionState : IMobState
{
    private MobContext ctx;
    private readonly MobStateMachine fsm;
    private bool hasLaunchedAttack;

    public MobMeleeActionState(MobContext context, MobStateMachine machine)
    {
        ctx = context;
        fsm = machine;
    }

    public void Enter()
    {
        hasLaunchedAttack = true;
        var direction = (ctx.Target.position - ctx.Transform.position).normalized;

        ctx.MeleeComponent.LaunchMeleeAttack(direction, () => 
        { 
            ctx.RaiseOnAnticipateAttack(); 
            hasLaunchedAttack = false;
            fsm.ChangeState<MobIdleActionState>();
        });
        ctx.AnimComponent.SetAnimUpdateCallback(UpdateAnimation);
    }

    public void Update()
    {
        float dist = Vector3.Distance(ctx.Transform.position, ctx.Target.position);

        ctx.Heading = ctx.Target.position - ctx.Transform.position;
        ctx.MoveComponent.UpdateMovement(Vector3.zero, false);
        ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
        ctx.DetectionComponent.UpdateComponent();
        ctx.AnimComponent.ApplyAnimation();

        if (!hasLaunchedAttack || dist > ctx.StoppingDistance)
        {
            ctx.RaiseOnFollowTarget();
            fsm.ChangeState<MobIdleActionState>();
        }
    }

    public void Exit() { hasLaunchedAttack = false; }

    private void UpdateAnimation(Animator animator)
    {
        animator.SetBool("IsRunning", ctx.MoveComponent.IsMoving);
        animator.SetFloat("MovementBlend", ctx.MoveComponent.SpeedPercentage);
    }
}
