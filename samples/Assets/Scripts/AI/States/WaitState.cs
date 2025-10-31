using UnityEngine;

public class WaitState : IMobState
{
    private MobContext ctx;
    private readonly MobStateMachine fsm;

    public WaitState(MobContext context, MobStateMachine machine)
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

        ctx.ChaseIntensity = 0;
    }

    public void Update()
    {
        ctx.MoveComponent.UpdateMovement(Vector3.zero, false);
        ctx.MoveComponent.UpdateLookDirection(Vector3.zero);
        ctx.AnimComponent.SetMovementParameter(false, ctx.MoveComponent.SpeedPercentage);
    }

    public void Exit() { }
}
