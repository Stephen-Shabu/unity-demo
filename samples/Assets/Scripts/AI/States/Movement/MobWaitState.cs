using UnityEngine;

public class MobWaitState : IMobState
{
    private readonly MobContext ctx;
    private readonly MobStateMachine fsm;

    public MobWaitState(MobContext context, MobStateMachine machine)
    {
        ctx = context;
        fsm = machine;
    }

    public void Enter()
    {
        ctx.ChaseIntensity = 0;
        ctx.AnimComponent.SetAnimUpdateCallback(UpdateAnimation);
    }

    public void Update()
    {
        ctx.MoveComponent.UpdateMovement(Vector3.zero, false);
        ctx.MoveComponent.UpdateLookDirection(Vector3.zero);
        ctx.AnimComponent.ApplyAnimation();
    }

    public void Exit() { }

    private void UpdateAnimation(Animator animator)
    {
        animator.SetBool("IsRunning", false);
        animator.SetFloat("MovementBlend", ctx.MoveComponent.SpeedPercentage);
    }
}
