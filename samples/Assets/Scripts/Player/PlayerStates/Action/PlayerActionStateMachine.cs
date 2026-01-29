using Samples;
using UnityEngine.InputSystem;

public class PlayerActionStateMachine : PlayerStateMachine
{
    public PlayerActionStateMachine(PlayerContext context) : base(context)
    {
        context.PlayerInput.actions["Attack"].performed -= OnFireWeapon;
        context.PlayerInput.actions["Attack"].performed += OnFireWeapon;
    }

    private void OnFireWeapon(InputAction.CallbackContext context)
    {
        var isPressed = context.action.IsPressed();

        if(isPressed)
            ChangeState<PlayerRangedActionState>();
        else
            ChangeState<PlayerIdleActionState>();
    }
}