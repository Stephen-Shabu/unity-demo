using UnityEngine;

public class PlayerRangedActionState : PlayerStateCapable
{
    private readonly PlayerStateMachine actionFSM;
    private readonly PlayerContext ctx;
    private bool isFiring = true;  

    public PlayerRangedActionState(PlayerContext context, PlayerStateMachine machine)
    {
        actionFSM = machine;
        ctx = context;
    }

    public void Enter()
    {
        isFiring = true;
        ctx.AnimComponent.SetAnimUpatedOneShot(OneShotAnimation);
        ctx.ProjectileComponent.Fire(true);
        ctx.AnimComponent.ApplyOneShotAnimation();
    }

    public void Update()
    {
        Vector3 cameraForward = ctx.CameraComponent.transform.forward;
        Vector3 cameraRight = ctx.CameraComponent.transform.right;
        cameraForward.y = 0.0f;

        Vector3 movementDirection = (cameraForward * ctx.InputVector.y + cameraRight * ctx.InputVector.x).normalized;
        ctx.MoveComponent.UpdateMovement(movementDirection, true);
        ctx.ProjectileComponent.UpdateComponent();
    }

    public void Exit()
    {
        isFiring = false;
        ctx.ProjectileComponent.Fire(false);
        ctx.AnimComponent.ApplyOneShotAnimation();
    }

    private void OneShotAnimation(Animator animator)
    {
        animator.SetBool("IsFiring", isFiring);
    }
}
