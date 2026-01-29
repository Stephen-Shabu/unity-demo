using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerIdleState : PlayerStateCapable
{
    private readonly PlayerStateMachine moveFSM;
    private readonly PlayerContext ctx;

    public PlayerIdleState(PlayerContext context, PlayerStateMachine machine)
    {
        moveFSM = machine;
        ctx = context;
    }

    public void Enter()
    {
        ctx.PlayerInput.actions["Move"].performed += OnMove;
        ctx.PlayerInput.actions["Move"].canceled += OnMove;

        ctx.PlayerInput.actions["Dodge"].performed += OnRightDodge;
        ctx.PlayerInput.actions["LeftDodge"].performed += OnLeftDodge;
    }

    public void Exit()
    {
        ctx.PlayerInput.actions["Move"].performed -= OnMove;
        ctx.PlayerInput.actions["Move"].canceled -= OnMove;

        ctx.PlayerInput.actions["Dodge"].performed -= OnRightDodge;
        ctx.PlayerInput.actions["LeftDodge"].performed -= OnLeftDodge;
    }

    public void Update()
    {
        ctx.MoveComponent.UpdateMovement(Vector3.zero, false);
        ctx.AnimComponent.SetMovementParameter(ctx.MoveComponent.IsMoving, ctx.MoveComponent.SpeedPercentage);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        ctx.InputVector = context.ReadValue<Vector2>();

        if (ctx.InputVector.sqrMagnitude > .25f)
        {
            moveFSM.ChangeState<PlayerMoveState>();
        }
    }

    private void OnRightDodge(InputAction.CallbackContext context)
    {
        ctx.DodgeDirection = DodgeDirection.Right;
        moveFSM.ChangeState<PlayerDodgeState>();
    }

    private void OnLeftDodge(InputAction.CallbackContext context)
    {
        ctx.DodgeDirection = DodgeDirection.Left;
        moveFSM.ChangeState<PlayerDodgeState>();
    }
}
