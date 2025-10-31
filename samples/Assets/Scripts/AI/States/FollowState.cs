using UnityEngine;

public class FollowState : IMobState
{
    private MobContext ctx;
    private readonly MobStateMachine fsm;

    public FollowState(MobContext context, MobStateMachine stateMachine)
    {
        ctx = context;
        fsm = stateMachine;
    }

    public void Enter(MobContext newContext = null) 
    {
        if (newContext != null)
        {
            ctx = newContext;
        }

        ctx.ChaseIntensity = 1;
    }

    public void Update()
    {
        float dist = Vector3.Distance(ctx.Transform.position, ctx.Target.position);
        float scaledIntensity = Mathf.Clamp01((dist - ctx.StoppingDistance + 0.5f) / ctx.StoppingDistance + 0.5f);

        ctx.Heading = ctx.Target.position - ctx.Transform.position;
        ctx.MoveComponent.UpdateMovement(ctx.Heading.normalized * ctx.ChaseIntensity * scaledIntensity, false);
        ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
        ctx.DetectionComponent.UpdateComponent();
        ctx.AnimComponent.SetMovementParameter(ctx.MoveComponent.IsMoving, ctx.MoveComponent.SpeedPercentage);

        if (dist <= ctx.StoppingDistance)
            fsm.ChangeState<AttackState>();
    }

    public void Exit() { }
}
