using UnityEngine;

public class PlayerRangedChargeActionState: PlayerStateCapable
{
    private readonly PlayerStateMachine actionFSM;
    private readonly PlayerContext ctx;
    private bool isFiring = true;
    private float chargeTimer;
    private int chargeStage;

    public PlayerRangedChargeActionState(PlayerContext context, PlayerStateMachine machine)
    {
        actionFSM = machine;
        ctx = context;
    }

    public void Enter()
    {
        chargeTimer = 0;
        isFiring = true;
        ctx.AnimComponent.SetAnimUpatedOneShot(OneShotAnimation);
        ctx.AnimComponent.ApplyOneShotAnimation();
        //isFiring = true;
        //ctx.AnimComponent.SetAnimUpatedOneShot(OneShotAnimation);
        //ctx.ProjectileComponent.Fire(true);
        //ctx.AnimComponent.ApplyOneShotAnimation();
        ctx.ProjectileComponent.SetProjectileIndex();
        ctx.ProjectileComponent.StartCharge();
    }

    public void Update()
    {
        chargeTimer += Time.deltaTime;

        Vector3 cameraForward = ctx.CameraComponent.transform.forward;
        Vector3 cameraRight = ctx.CameraComponent.transform.right;
        cameraForward.y = 0.0f;

        Vector3 movementDirection = (cameraForward * ctx.InputVector.y + cameraRight * ctx.InputVector.x).normalized;
        ctx.MoveComponent.UpdateMovement(movementDirection, true);
        ctx.ProjectileComponent.UpdateComponent(chargeTimer);
    }

    public void Exit()
    {
        isFiring = false;
        ctx.ProjectileComponent.Fire(true);
        ctx.AnimComponent.ApplyOneShotAnimation();
    }

    private void OneShotAnimation(Animator animator)
    {
        animator.SetBool("IsFiring", isFiring);
    }
}
