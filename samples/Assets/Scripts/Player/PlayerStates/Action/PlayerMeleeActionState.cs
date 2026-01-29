using UnityEngine;

public class PlayerMeleeActionState : PlayerStateCapable
{
    private readonly PlayerStateMachine actionFSM;
    private readonly PlayerContext ctx;
    private float aimDuration = 0.5f;
    private float aimTime = 0f;
    private bool launchedMeleeAttack = false;

    public PlayerMeleeActionState(PlayerContext context, PlayerStateMachine machine)
    {
        actionFSM = machine;
        ctx = context;
    }

    public void Enter()
    {
        aimTime = 0f;
        launchedMeleeAttack = true;
        ctx.MoveComponent.ApplyLean(MovementDefines.Character.ATTACK_LEAN_ANGLE, Vector3.right);
    }

    public void Exit()
    {

    }

    public void Update()
    {
        if (aimTime < aimDuration)
        {
            aimTime += Time.deltaTime;
        }
        else if (launchedMeleeAttack && aimTime >= aimDuration)
        {
            ctx.MoveComponent.ApplyLean(0, Vector3.right);
            ctx.MeleeComponent.LaunchMeleeAttack(ctx.Transform.forward, () => { aimTime = 0f; actionFSM.ChangeState<PlayerIdleActionState>(); });
            launchedMeleeAttack = false;
        }
    }
}
