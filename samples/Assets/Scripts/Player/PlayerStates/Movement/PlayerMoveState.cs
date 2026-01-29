using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveState : PlayerStateCapable
{
    private readonly PlayerStateMachine moveFSM;
    private readonly PlayerContext ctx;

    public PlayerMoveState(PlayerContext context, PlayerStateMachine machine)
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
        Vector3 cameraForward = ctx.CameraComponent.transform.forward;
        Vector3 cameraRight = ctx.CameraComponent.transform.right;
        cameraForward.y = 0.0f;

        Vector3 movementDirection = (cameraForward * ctx.InputVector.y + cameraRight * ctx.InputVector.x).normalized;
        ctx.MoveComponent.UpdateMovement(movementDirection, false);
        ctx.AnimComponent.SetMovementParameter(ctx.MoveComponent.IsMoving, ctx.MoveComponent.SpeedPercentage);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        ctx.InputVector = context.ReadValue<Vector2>();

        if (ctx.InputVector.Equals(Vector3.zero))
        {
            moveFSM.ChangeState<PlayerIdleState>();
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
