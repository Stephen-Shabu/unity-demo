using UnityEngine;

public class PlayerDeadState : PlayerStateCapable
{
    private readonly PlayerStateMachine conditionFSM;
    private readonly PlayerContext ctx;
    private float deathImpulse = 10f;

    public PlayerDeadState(PlayerContext context, PlayerStateMachine machine)
    {
        conditionFSM = machine;
        ctx = context;
    }

    public void Enter()
    {
        ctx.Rigidbody.linearVelocity += ctx.HitDirection * deathImpulse;
    }

    public void Exit()
    {
    }

    public void Update()
    {

    }
}
