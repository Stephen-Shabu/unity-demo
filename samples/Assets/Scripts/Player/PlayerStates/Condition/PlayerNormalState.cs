using UnityEngine;

public class PlayerNormalState : PlayerStateCapable
{
    private readonly PlayerStateMachine conditionFSM;
    private readonly PlayerContext ctx;

    public PlayerNormalState(PlayerContext context, PlayerStateMachine machine)
    {
        conditionFSM = machine;
        ctx = context;
    }

    public void Enter()
    {
        ctx.HealthComponent.OnDamageTaken += HandleOnDamageTaken;
        ctx.HealthComponent.OnDeathStarted += HandleOnDeathStarted;
        ctx.HealthComponent.OnDeathFinished += HandleOnDeathFinished;
    }

    public void Exit()
    {
        ctx.HealthComponent.OnDamageTaken -= HandleOnDamageTaken;
        ctx.HealthComponent.OnDeathStarted -= HandleOnDeathStarted;
        ctx.HealthComponent.OnDeathFinished -= HandleOnDeathFinished;
    }

    public void Update()
    {

    }

    private void HandleOnDeathFinished()
    {
        ctx.Transform.gameObject.SetActive(false);
        GameEventsEmitter.EmitEvent(EventType.PlayerDefeated, new GenericEventData { Type = EventType.PlayerDefeated });
    }

    private void HandleOnDeathStarted(Vector3 hitDirection)
    {
        ctx.HitDirection = hitDirection;
        conditionFSM.ChangeState<PlayerDeadState>();
    }

    private void HandleOnDamageTaken(Vector3 direction, int newHealth)
    {
        ctx.HitDirection = direction;
        conditionFSM.ChangeState<PlayerHitReactState>();
    }
}