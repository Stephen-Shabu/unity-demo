using System;
using System.Collections.Generic;

public interface StateCapable
{
    void Enter();
    void Update();
    void Exit();
}

public abstract class BaseStateMachine<T> where T : StateCapable
{
    private T currentState;
    private T previousState;
    private Dictionary<Type, T> states = new();

    public void AddState(T state)
    {
        states[state.GetType()] = state;
    }

    public void ChangeState<T>()
    {
        currentState?.Exit();
        UnityEngine.Debug.Log($"Exited {currentState?.ToString()}");
        previousState = currentState;
        currentState = states[typeof(T)];
        currentState.Enter();
        UnityEngine.Debug.Log($"Entered {currentState.ToString()}");
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
