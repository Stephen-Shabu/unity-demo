using UnityEngine;

public class MobContext
{
    public Rigidbody Rigidbody;
    public Transform Transform;
    public Transform Target;
    public MovementComponent MoveComponent;
    public DetectionComponent DetectionComponent;
    public AnimationComponent AnimComponent;
    public MeleeComponent MeleeComponent;
    public AudioComponent AudioComponent;
    public LayerMask WallLayer;
    public float StoppingDistance;
    [Range(0, 1)] public float ChaseIntensity;
    public Vector3 Heading;
    public Vector3 AvoidTarget;
    public Vector3 RepositionTarget;
    public Vector3 HitDirection;
    public MobStateMachine FSM;

    public void LookAtTarget() => MoveComponent.UpdateLookDirection(Target.position - Transform.position);

    public void HandleOnDeathStarted(Vector3 hitDirection)
    {
        FSM.ChangeState<DeathState>();
    }

    public void HandleOnDeathFinished()
    {
        Transform.gameObject.SetActive(false);
        GameEventsEmitter.EmitEvent(EventType.EnemyDefeated, new GenericEventData { Type = EventType.EnemyDefeated, Caller = Transform.gameObject });
    }

    public void HandleOnDamageTaken(Vector3 hitDirection, int newHealth)
    {
        HitDirection = hitDirection;
        FSM.ChangeState<HitReactState>();
    }

    public void HandleObstacleDetected(Vector3 obstaclePosition)
    {
        Vector3 heading = Target.position - Transform.position;
        Vector3 cross = Vector3.Cross(heading, (obstaclePosition - Transform.position).normalized);
        float side = Vector3.Dot(cross, Vector3.up);

        Quaternion rotation = Quaternion.AngleAxis(side > 0 ? -90 : 90, Vector3.up);
        Vector3 dir = rotation * (obstaclePosition - Transform.position);
        Vector3 offset = obstaclePosition + dir * 1.2f;
        Vector3 candidate = new Vector3(offset.x, Transform.position.y, offset.z);

        RaycastHit[] hits = new RaycastHit[1];
        if (Physics.SphereCastNonAlloc(new Ray(candidate, heading), 0.1f, hits, 0.1f, WallLayer) > 0)
        {
            var closePoint = hits[0].collider.ClosestPoint(Transform.position);
            Vector3 correction = Transform.position - (closePoint - Transform.position) * UnityEngine.Random.Range(1.2f, 2f);
            AvoidTarget = correction;
        }
        else
        {
            AvoidTarget = candidate;
        }

        FSM.ChangeState<AvoidState>();
    }
}
