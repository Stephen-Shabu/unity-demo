using UnityEngine;

public class AvoidState : IMobState
{
    private MobContext ctx;
    private readonly MobStateMachine fsm;
    private float safeDistance = 1.5f;

    public AvoidState(MobContext context, MobStateMachine machine)
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
    }

    public void Update()
    {
        float dist = Vector3.Distance(ctx.Transform.position, ctx.AvoidTarget);

        if (dist > safeDistance)
        {
            ctx.Heading = ctx.AvoidTarget - ctx.Transform.position;
            ctx.MoveComponent.UpdateMovement(ctx.Heading.normalized, false);
            ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
            ctx.AnimComponent.SetMovementParameter(ctx.MoveComponent.IsMoving, ctx.MoveComponent.SpeedPercentage);
        }
        else
        {
            ctx.DetectionComponent.ResetDectectedObject();
            fsm.ReturnToLastState();
        }
    }

    public void Exit() { }
}
