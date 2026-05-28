using Samples;
using UnityEngine.InputSystem;

public class PlayerActionStateMachine : PlayerStateMachine
{
    private bool isWeaponChargeable = false;

    public PlayerActionStateMachine(PlayerContext context) : base(context)
    {
        context.PlayerInput.actions["Attack"].performed -= OnFireWeapon;
        context.PlayerInput.actions["Attack"].performed += OnFireWeapon;
        GameEventsEmitter.OnEvent(EventType.ChangeWeapon, OnWeaponChanged);
    }

    private void OnFireWeapon(InputAction.CallbackContext context)
    {
        var isPressed = context.action.IsPressed();

        if (isPressed)
        {
            if (isWeaponChargeable)
            {
                ChangeState<PlayerRangedChargeActionState>();
            }
            else
            {
                ChangeState<PlayerRangedActionState>();
            }
        }
        else
        {
            ChangeState<PlayerIdleActionState>();
        }
    }

    private void OnWeaponChanged(EventData e)
    {
        if (e is WeaponChangeEventData value)
        {
            isWeaponChargeable = value.Schema.IsChargeable;
        }
    }
}