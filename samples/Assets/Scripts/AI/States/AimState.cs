using UnityEngine;

public class AimState : IMobState
{
    private MobContext ctx;
    private readonly MobStateMachine fsm;
    private float aimDuration = 0.5f;
    private float aimTime = 0f;

    public AimState(MobContext context, MobStateMachine machine)
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

        ctx.MoveComponent.ApplyLean(MovementDefines.Character.ATTACK_LEAN_ANGLE, Vector3.right);
    }

    public void Update()
    {
        if (aimTime < aimDuration)
        {
            aimTime += Time.deltaTime;
        }
        else
        {
            fsm.ChangeState<AttackState>();
        }

        ctx.Heading = ctx.Target.position - ctx.Transform.position;
        ctx.MoveComponent.UpdateMovement(Vector3.zero, false);
        ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
        ctx.DetectionComponent.UpdateComponent();
        ctx.AnimComponent.SetMovementParameter(false, 0);
    }

    public void Exit()
    {
        ctx.MoveComponent.ApplyLean(0, Vector3.right);
        aimTime = 0;
    }
}
