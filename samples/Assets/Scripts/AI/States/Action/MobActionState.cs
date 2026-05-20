using System;
using UnityEngine;

public class MobActionState: IMobState
{
    private MobStateMachine actionFSM;
    private readonly MobContext ctx;

    public MobActionState(MobContext context, MobStateMachine machine)
    {
        ctx = context;
        actionFSM = new MobStateMachine();
        actionFSM.AddState(new MobIdleActionState(context, actionFSM));
        actionFSM.AddState(new MobAimActionState(context, actionFSM));
        actionFSM.AddState(new MobMeleeActionState(context, actionFSM));  
        actionFSM.AddState(new MobRangedActionState(context, actionFSM));
    }

    void IMobState.Enter()
    {
        actionFSM.ChangeState<MobIdleActionState>();
        ctx.OnAimAttack -= HandleOnAimAttack;
        ctx.OnAimAttack += HandleOnAimAttack;
    }

    void IMobState.Update(){ actionFSM.Update(); }

    void IMobState.Exit()
    {
        ctx.OnAimAttack -= HandleOnAimAttack;
    }

    private void HandleOnAimAttack()
    {
        actionFSM.ChangeState<MobAimActionState>();
    }
}
