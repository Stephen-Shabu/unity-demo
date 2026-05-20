
public class MobConditionState: IMobState
{
    private MobStateMachine conditionFSM;

    public MobConditionState(MobContext context, MobStateMachine machine)
    {
        conditionFSM = new MobStateMachine();
        conditionFSM.AddState(new MobNormalState(context, conditionFSM));
        conditionFSM.AddState(new MobHitReactState(context, conditionFSM));
        conditionFSM.AddState(new MobDeathState(context, conditionFSM));
    }

    void IMobState.Enter() { conditionFSM.ChangeState<MobNormalState>(); }
    void IMobState.Update(){ conditionFSM.Update(); }
    void IMobState.Exit(){}

}
