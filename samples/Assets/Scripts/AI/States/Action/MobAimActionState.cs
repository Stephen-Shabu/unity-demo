using UnityEngine;

public class MobAimActionState : IMobState
{
    private readonly MobContext ctx;
    private readonly MobStateMachine fsm;
    private float aimDuration = 1.5f;
    private float aimTime = 0f;

    public MobAimActionState(MobContext context, MobStateMachine machine)
    {
        ctx = context;
        fsm = machine;
    }

    public void Enter()
    {
        ctx.AggroMeter = 0;
        ctx.MoveComponent.ApplyLean(MovementDefines.Character.ATTACK_LEAN_ANGLE, Vector3.right);
        ctx.AnimComponent.SetAnimUpdateCallback(UpdateAnimation);
    }

    public void Update()
    {
        if (aimTime < aimDuration)
        {
            aimTime += Time.deltaTime;
        }
        else
        {
            fsm.ChangeState<MobMeleeActionState>();
        }

        ctx.Heading = ctx.Target.position - ctx.Transform.position;
        ctx.MoveComponent.UpdateMovement(Vector3.zero, false);
        ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
        ctx.DetectionComponent.UpdateComponent();
        ctx.AnimComponent.ApplyAnimation();
    }

    public void Exit()
    {
        ctx.MoveComponent.ApplyLean(0, Vector3.right);
        aimTime = 0;
    }

    private void UpdateAnimation(Animator animator)
    {
        animator.SetBool("IsRunning", false);
        animator.SetFloat("MovementBlend", 0);
    }
}
