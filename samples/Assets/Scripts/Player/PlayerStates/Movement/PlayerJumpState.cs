using UnityEngine;

public class PlayerJumpState : PlayerStateCapable
{
    private readonly PlayerStateMachine moveFSM;
    private readonly PlayerContext ctx;

    public PlayerJumpState(PlayerContext context, PlayerStateMachine machine)
    {
        moveFSM = machine;
        ctx = context;
    }

    public void Enter()
    {
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public void Update()
    {
        throw new System.NotImplementedException();
    }
}
