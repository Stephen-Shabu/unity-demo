public class DeathState : IMobState
{
    private MobContext ctx;
    private readonly MobStateMachine fsm;

    public DeathState(MobContext context, MobStateMachine machine)
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

        ctx.MeleeComponent.CancelMeleeAttack();
        GameEventsEmitter.EmitEvent(EventType.EnemyDefeated, new GenericEventData { Type = EventType.EnemyDefeated, Caller = ctx.Transform.gameObject });
    }

    public void Exit()
    {
    }

    public void Update()
    {
        ctx.MoveComponent.UpdateMovement(ctx.Heading.normalized, false);
        ctx.MoveComponent.UpdateLookDirection(ctx.Heading);
    }
}
