using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMobState
{
    void Enter();
    void Update();
    void Exit();
}

public class MobStateMachine
{
    public IMobState CurrentState => currentState;

    private IMobState currentState;
    private IMobState previousState;
    private Dictionary<Type, IMobState> states = new();

    public void AddState(IMobState state)
    {
        states[state.GetType()] = state;
    }

    public void ChangeState<T>() where T : IMobState
    {
        currentState?.Exit();
        previousState = currentState;
        currentState = states[typeof(T)];
        currentState.Enter();
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
