using UnityEngine;

public class MobAnticipateState : IMobState
{
    enum ShuffleState
    {
        Shuffle,
        Pause
    }

    enum StrafeDirectionState
    {
        Left = -1,
        Right = 1
    }

    private readonly MobContext ctx;
    private readonly MobStateMachine fsm;

    private float shuffleTimer;
    private float shuffleInterval;

    private const float MIN_ORBIT_RADIUS = 3.5f;
    private const float MAX_ORBIT_RADIUS = 7f;
    private float ORBIT_RADIUS = 0f;  

    private ShuffleState currentShuffleState = ShuffleState.Shuffle;
    private StrafeDirectionState strafeDirection;
    private Vector3 lastTargetPosition;

    public MobAnticipateState(MobContext context, MobStateMachine machine)
    {
        ctx = context;
        fsm = machine;
        ORBIT_RADIUS = Mathf.SmoothStep(MIN_ORBIT_RADIUS, MAX_ORBIT_RADIUS, (float)ctx.Index / 3);
    }

    public void Enter()
    {
        shuffleTimer = 0;

        if (Random.value > .5)
        {
            PickNewShuffle();
        }
        else
        {
            SetPauseState();
        }

        ctx.AnimComponent.SetAnimUpdateCallback(UpdateAnimation);
        ctx.AnimComponent.SetAnimUpatedOneShot(OneshotAnimation);
        ctx.AnimComponent.ApplyOneShotAnimation();
    }

    public void Exit()
    {
        SetPauseState();
    }

    public void Update()
    {
        float dist = Vector3.Distance(ctx.Transform.position, ctx.Target.position);
        shuffleTimer += Time.deltaTime;

        if (shuffleTimer >= shuffleInterval)
            ChangeShuffleState();

        if (currentShuffleState.Equals(ShuffleState.Shuffle))
        {
            if (dist > ctx.StoppingDistance + ORBIT_RADIUS)
            {
                fsm.ChangeState<MobFollowState>();
            }
            OrbitPlayer();
            ctx.AggroMeter += Time.deltaTime * 10f;
        }
        else
        {
            ctx.MoveComponent.UpdateMovement(Vector3.zero, false);
            ctx.AnimComponent.ApplyAnimation();

            if (ctx.AggroMeter >= MobContext.MAX_AGGRO_METER && dist <= ctx.StoppingDistance)
            {
                ctx.RaiseOnAimAttack();
                fsm.ChangeState<MobWaitState>();
            }
        }

        FacePlayer();
    }

    private void ChangeShuffleState()
    {
        if (currentShuffleState == ShuffleState.Shuffle)
        {
            SetPauseState();
        }
        else
        {
            PickNewShuffle();
        }
    }   

    private void PickNewShuffle()
    {
        shuffleTimer = 0f;
        currentShuffleState = ShuffleState.Shuffle;
        lastTargetPosition = ctx.Target.position;
        shuffleInterval = Random.Range(.5f, 2f);
        strafeDirection = Random.value > 0.5f ? StrafeDirectionState.Right : StrafeDirectionState.Left;
        ctx.AnimComponent.ApplyOneShotAnimation();
    }

    private void SetPauseState()
    {
        currentShuffleState = ShuffleState.Pause;
        shuffleTimer = 0f;
        shuffleInterval = Random.Range(1f, 3f);
        ctx.AnimComponent.ApplyOneShotAnimation();
    }

    private void OrbitPlayer()
    {
        Vector3 toPlayer = ctx.Transform.position - lastTargetPosition;
        Vector3 flatToPlayer = Vector3.ProjectOnPlane(toPlayer, Vector3.up);

        float distanceError = flatToPlayer.magnitude - ORBIT_RADIUS;

        Vector3 tangent = Vector3.Cross(Vector3.up, flatToPlayer.normalized)
                          * (int)strafeDirection;

        Vector3 movement =
            tangent +
            (-flatToPlayer.normalized * distanceError * 2f);

        ctx.MoveComponent.UpdateMovement(movement.normalized, false);
        ctx.AnimComponent.ApplyAnimation();
    }

    private void FacePlayer()
    {
        Vector3 lookDir = ctx.Target.position - ctx.Transform.position;
        lookDir.y = 0;
        ctx.Heading = lookDir;
        ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
        ctx.DetectionComponent.UpdateComponent();
    }

    private void UpdateAnimation(Animator animator)
    {
        animator.SetBool("IsRunning", ctx.MoveComponent.IsMoving);
        animator.SetFloat("MovementBlend", ctx.MoveComponent.SpeedPercentage);
    }

    private void OneshotAnimation(Animator animator)
    {
        var param = strafeDirection == StrafeDirectionState.Right ? "IsShufflingRight" : "IsShufflingLeft";
        animator.SetBool(param, currentShuffleState == ShuffleState.Shuffle);
    }
}
