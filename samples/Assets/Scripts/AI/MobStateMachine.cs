using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMobState
{
    void Enter(MobContext newContext = null);
    void Update();
    void Exit();
}

public class MobStateMachine
{
    private IMobState currentState;
    private IMobState previousState;
    private Dictionary<Type, IMobState> states = new();

    public void AddState(IMobState state)
    {
        states[state.GetType()] = state;
    }

    public void ChangeState<T>(MobContext newContext = null) where T : IMobState
    {
        currentState?.Exit();
        previousState = currentState;
        currentState = states[typeof(T)];
        currentState.Enter(newContext);
    }

    public void ReturnToLastState()
    {
        if (previousState != null)
        {
            currentState.Exit();
            var tempState = currentState;
            currentState = previousState;
            previousState = tempState;

            currentState.Enter();
        }
    }

    public void Update() => currentState?.Update();
}
