using UnityEngine;

public class MobNormalState : IMobState
{
    private readonly MobContext ctx;
    private readonly MobStateMachine fsm;

    public MobNormalState(MobContext ctx, MobStateMachine fsm)
    {
        this.ctx = ctx;
        this.fsm = fsm;
    }

    void IMobState.Enter()
    {
        ctx.HealthComponent.OnDamageTaken += HandleOnDamageTaken;
        ctx.HealthComponent.OnDeathStarted += HandleOnDeathStarted;
        ctx.HealthComponent.OnDeathFinished += HandleOnDeathFinished;
    }

    void IMobState.Update(){}

    void IMobState.Exit()
    {
        ctx.HealthComponent.OnDamageTaken -= HandleOnDamageTaken;
        ctx.HealthComponent.OnDeathStarted -= HandleOnDeathStarted;
        ctx.HealthComponent.OnDeathFinished -= HandleOnDeathFinished;
    }

    private void HandleOnDeathFinished()
    {
        ctx.Transform.gameObject.SetActive(false);
        GameEventsEmitter.EmitEvent(EventType.EnemyDefeated, 
            new GenericEventData { Type = EventType.EnemyDefeated, Caller = ctx.Transform.gameObject });
    }

    private void HandleOnDeathStarted(Vector3 hitDirection)
    {
        ctx.HitDirection = hitDirection;
        fsm.ChangeState<MobDeathState>();
    }

    private void HandleOnDamageTaken(Vector3 direction, int newHealth)
    {
        ctx.HitDirection = direction;
        fsm.ChangeState<MobHitReactState>();
    }
}
