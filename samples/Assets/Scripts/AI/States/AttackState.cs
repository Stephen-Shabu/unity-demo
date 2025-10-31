using UnityEngine;

public class AttackState : IMobState
{
    private MobContext ctx;
    private readonly MobStateMachine fsm;
    private bool hasLaunchedAttack;

    public AttackState(MobContext context, MobStateMachine machine)
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

       hasLaunchedAttack = false;
    }

    public void Update()
    {
        float dist = Vector3.Distance(ctx.Transform.position, ctx.Target.position);

        if (ctx.MeleeComponent.CanAttack(dist <= ctx.StoppingDistance))
        {
            ctx.MeleeComponent.LaunchMeleeAttack(
                ctx.Target,
                () => ctx.MoveComponent.ApplyLean(MovementDefines.Character.ATTACK_LEAN_ANGLE, Vector3.right),
                () => ctx.MoveComponent.ApplyLean(0, Vector3.right),
                () =>
                {
                    hasLaunchedAttack = false;
                    fsm.ChangeState<RepositionState>();
                });
            hasLaunchedAttack = true;
        }

        ctx.Heading = ctx.Target.position - ctx.Transform.position;
        ctx.MoveComponent.UpdateMovement(Vector3.zero, false);
        ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
        ctx.DetectionComponent.UpdateComponent();
        ctx.AnimComponent.SetMovementParameter(ctx.MoveComponent.IsMoving, ctx.MoveComponent.SpeedPercentage);

        if (!hasLaunchedAttack || dist > ctx.StoppingDistance)
        {
            fsm.ChangeState<FollowState>();
        }
    }

    public void Exit() { ctx.MoveComponent.ApplyLean(0, Vector3.right); }
}
