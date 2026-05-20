using System;
using UnityEngine;

public class MobIdleActionState : IMobState
{
    private readonly MobContext ctx;
    private readonly MobStateMachine fsm;

    public MobIdleActionState(MobContext ctx, MobStateMachine fsm)
    {
        this.ctx = ctx;
        this.fsm = fsm;
    }

    void IMobState.Enter(){}

    void IMobState.Update(){}

    void IMobState.Exit(){}
}
