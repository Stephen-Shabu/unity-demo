using UnityEngine;

public class MobAvoidState : IMobState
{
    private MobContext ctx;
    private readonly MobStateMachine fsm;
    private float safeDistance = 1.5f;

    public MobAvoidState(MobContext context, MobStateMachine machine)
    {
        ctx = context;
        fsm = machine;
    }

    public void Enter()
    {
        ctx.AnimComponent.SetAnimUpdateCallback(UpdateAnimation);
    }

    public void Update()
    {
        float dist = Vector3.Distance(ctx.Transform.position, ctx.AvoidTarget);

        if (dist > safeDistance)
        {
            ctx.Heading = ctx.AvoidTarget - ctx.Transform.position;
            ctx.MoveComponent.UpdateMovement(ctx.Heading.normalized, false);
            ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
            ctx.AnimComponent.ApplyAnimation();
        }
        else
        {
            ctx.DetectionComponent.ResetDectectedObject();
            fsm.ReturnToLastState();
        }
    }

    public void Exit() { ctx.DetectionComponent.ResetDectectedObject(); }

    private void UpdateAnimation(Animator animator)
    {
        animator.SetBool("IsRunning", ctx.MoveComponent.IsMoving);
        animator.SetFloat("MovementBlend", ctx.MoveComponent.SpeedPercentage);
    }
}
