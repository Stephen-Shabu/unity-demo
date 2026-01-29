using UnityEngine.InputSystem;

public class PlayerIdleActionState : PlayerStateCapable
{
    private readonly PlayerStateMachine actionFSM;
    private readonly PlayerContext ctx;

    public PlayerIdleActionState(PlayerContext context, PlayerStateMachine machine)
    {
        actionFSM = machine;
        ctx = context;
    }

    public void Enter()
    {
        ctx.PlayerInput.actions["SelectSlotOne"].performed += OnWeaponOneSelected;
        ctx.PlayerInput.actions["SelectSlotTwo"].performed += OnWeaponTwoSelected;
        ctx.PlayerInput.actions["Dash"].performed += OnDashAttack;
    }

    public void Exit()
    {
        ctx.PlayerInput.actions["SelectSlotOne"].performed -= OnWeaponOneSelected;
        ctx.PlayerInput.actions["SelectSlotTwo"].performed -= OnWeaponTwoSelected;
        ctx.PlayerInput.actions["Dash"].performed -= OnDashAttack;
    }

    public void Update()
    {
        ctx.ProjectileComponent.UpdateComponent();
    }

    private void OnDashAttack(InputAction.CallbackContext context)
    {
        var isPressed = context.action.IsPressed();
        if (isPressed)
        {
            actionFSM.ChangeState<PlayerMeleeActionState>();
        }
    }

    private void OnWeaponOneSelected(InputAction.CallbackContext context)
    {
        ctx.ProjectileComponent.ChangeWeapon(WeaponName.BLASTER);
    }

    private void OnWeaponTwoSelected(InputAction.CallbackContext context)
    {
        ctx.ProjectileComponent.ChangeWeapon(WeaponName.WAVE_BEAM);
    }
}