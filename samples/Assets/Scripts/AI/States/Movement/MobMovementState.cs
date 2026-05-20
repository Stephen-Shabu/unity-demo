
using System;
using System.Threading;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MobMovementState: IMobState
{
    private MobStateMachine movementFSM;
    private readonly MobContext ctx;

    public MobMovementState(MobContext context, MobStateMachine machine)
    {
        ctx = context;
        movementFSM = new MobStateMachine();
        movementFSM.AddState(new MobWaitState(context, movementFSM));
        movementFSM.AddState(new MobFollowState(context, movementFSM));
        movementFSM.AddState(new MobAvoidState(context, movementFSM));
        movementFSM.AddState(new MobAnticipateState(context, movementFSM));
        movementFSM.AddState(new MobRepositionState(context, movementFSM));
    }

    void IMobState.Enter()
    {
        movementFSM.ChangeState<MobWaitState>();
        ctx.OnFollowTarget -= HandleOnFollowTarget;
        ctx.OnFollowTarget += HandleOnFollowTarget;
        ctx.OnAnticpateAttack -= HandleAnticipateAttack;
        ctx.OnAnticpateAttack += HandleAnticipateAttack;
        ctx.DetectionComponent.OnObstacleDetected -= HandleOnTargetDetected;
        ctx.DetectionComponent.OnObstacleDetected += HandleOnTargetDetected;
    }

    void IMobState.Exit()
    {
        ctx.DetectionComponent.OnObstacleDetected -= HandleOnTargetDetected;
        ctx.OnAnticpateAttack -= HandleAnticipateAttack;
        ctx.OnFollowTarget -= HandleOnFollowTarget;
    }

    void IMobState.Update()
    {
        movementFSM.Update();
    }

    private void HandleOnTargetDetected(Vector3 obstaclePosition)
    {
        if (movementFSM.CurrentState is MobAnticipateState)
            return;

        Vector3 heading = ctx.Target.position - ctx.Transform.position;
        Vector3 cross = Vector3.Cross(heading, (obstaclePosition - ctx.Transform.position).normalized);
        float side = Vector3.Dot(cross, Vector3.up);

        Quaternion rotation = Quaternion.AngleAxis(side > 0 ? -90 : 90, Vector3.up);
        Vector3 dir = rotation * (obstaclePosition - ctx.Transform.position);
        Vector3 offset = obstaclePosition + dir * 1.2f;
        Vector3 candidate = new Vector3(offset.x, ctx.Transform.position.y, offset.z);

        RaycastHit[] hits = new RaycastHit[1];
        if (Physics.SphereCastNonAlloc(new Ray(candidate, heading), 0.1f, hits, 0.1f, ctx.WallLayer) > 0)
        {
            var closePoint = hits[0].collider.ClosestPoint(ctx.Transform.position);
            Vector3 correction = ctx.Transform.position - (closePoint - ctx.Transform.position) * UnityEngine.Random.Range(1.2f, 2f);
            ctx.AvoidTarget = correction;
        }
        else
        {
            ctx.AvoidTarget = candidate;
        }

        movementFSM.ChangeState<MobAvoidState>();
    }

    private void HandleAnticipateAttack()
    {
        movementFSM.ChangeState<MobAnticipateState>();
    }

    private void HandleOnFollowTarget()
    {
        movementFSM.ChangeState<MobFollowState>();
    }
}
