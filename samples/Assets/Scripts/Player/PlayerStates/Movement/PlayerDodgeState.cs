using UnityEngine;

public class PlayerDodgeState : PlayerStateCapable
{
    private readonly PlayerStateMachine moveFSM;
    private readonly PlayerContext ctx;
    private float elapsed = 0f;
    private const float DURATION = .5f;

    public PlayerDodgeState(PlayerContext context, PlayerStateMachine machine)
    {
        moveFSM = machine;
        ctx = context;
    }

    public void Enter()
    {
        elapsed = 0;
        ctx.MoveComponent.SetDodge(ctx.DodgeDirection);
        ctx.AnimComponent.SetAnimUpdateCallback(UpdateAnimation);
        ctx.AnimComponent.ApplyAnimation();
    }

    public void Update()
    {
        if (elapsed < DURATION)
        {
            float t = elapsed / DURATION;
            float easedT = MathDefines.Easing.EaseInOut(t);

            ctx.MoveComponent.UpdateDodge(easedT);

            elapsed += Time.deltaTime;
        }
        else
        {
            moveFSM.ChangeState<PlayerIdleState>();

            return;
        }

        ctx.MoveComponent.UpdateMovement(Vector3.zero, false);
    }

    public void Exit()
    {
        ctx.MoveComponent.CompleteDodge();
    }

    private void UpdateAnimation(Animator animator)
    {
        var trigger = ctx.DodgeDirection.Equals(DodgeDirection.Left) ? "DODGE_L" : "DODGE_R";
        animator.SetTrigger(trigger);
    }   
}