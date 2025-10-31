using UnityEngine;

public class RepositionState : IMobState
{
    private MobContext ctx;
    private readonly MobStateMachine fsm;
    private float cooldown;

    public RepositionState(MobContext context, MobStateMachine machine)
    {
        ctx = context;
        fsm = machine;
    }

    public void Enter(MobContext newContext = null)
    {
        if (newContext != null)
        {
            ctx = newContext;
        }
        cooldown = 0;
        ctx.RepositionTarget = CalculateNewTargetPosition();
        ctx.ChaseIntensity = 1;
    }

    public void Update()
    {
        float dist = Vector3.Distance(ctx.Transform.position, ctx.RepositionTarget);
        cooldown += Time.deltaTime;
        var movementDirection = Vector3.zero;
        float scaledIntensity = Mathf.Clamp01((dist - 1f) / 1f);

        if (dist > 1f)
        {
            movementDirection = ctx.RepositionTarget - ctx.Transform.position;
            ctx.Heading = movementDirection;
        }
        else if (cooldown > 3f)
        {
            fsm.ChangeState<FollowState>();
        }
        else
        {
            ctx.Heading = ctx.Target.position - ctx.Transform.position;
        }

        ctx.MoveComponent.UpdateMovement(movementDirection.normalized, false);
        ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
        ctx.AnimComponent.SetMovementParameter(ctx.MoveComponent.IsMoving, ctx.MoveComponent.SpeedPercentage);
    }

    private Vector3 CalculateNewTargetPosition()
    {
        // 1️⃣ Choose random side (left/right)
        int dir = UnityEngine.Random.value > 0.5f ? 1 : -1;
        Vector3 toTarget = ctx.Target.position - ctx.Transform.position;
        Vector3 sideOffset = Vector3.Cross(Vector3.up, toTarget.normalized) * dir;

        // 2️⃣ Offset position around target
        Vector3 candidate = ctx.Target.position + sideOffset * UnityEngine.Random.Range(1.2f, 2.5f);

        // 3️⃣ Check for wall penetration
        const float probeRadius = 0.5f;
        const float pushDistance = 1.5f;

        if (Physics.CheckSphere(candidate, probeRadius, ctx.WallLayer))
        {
            // We are inside or too close to a wall
            // Push the candidate outward along wall normal
            if (Physics.Raycast(candidate + Vector3.up * 0.5f, -Vector3.up, out var downHit, 2f))
            {
                candidate.y = downHit.point.y; // Snap to ground if possible
            }

            // Find the closest hit direction around the mob
            if (Physics.SphereCast(ctx.Target.position, probeRadius, (candidate - ctx.Target.position).normalized,
                                   out RaycastHit hit, Vector3.Distance(ctx.Target.position, candidate), ctx.WallLayer))
            {
                Vector3 awayFromWall = Vector3.ProjectOnPlane(candidate - hit.point, hit.normal).normalized;
                candidate = hit.point + hit.normal * pushDistance + awayFromWall * 0.2f;
            }
            else
            {
                // fallback — just nudge away from current position
                candidate += (candidate - ctx.Target.position).normalized * pushDistance;
            }
        }

#if UNITY_EDITOR
        Debug.DrawLine(ctx.Target.position, candidate, Color.cyan, 2f);
        Debug.DrawRay(candidate, Vector3.up * 0.5f, Color.magenta, 2f);
#endif

        return candidate;
    }

    public void Exit() { }
}
