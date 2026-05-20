using UnityEngine;

public class MobRangedActionState : IMobState
{
    private MobContext ctx;
    private MobStateMachine fsm;
    public MobRangedActionState(MobContext context, MobStateMachine stateMachine)
    {
        this.ctx = context;
        this.fsm = stateMachine;
    }

    public void Enter()
    {

    }

    public void Update()
    {
    }

    public void Exit()
    {
        
    }
}
