using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public enum EventType
{
    ChangeState    
}

public interface EventData
{
}

public struct StateEventData: EventData
{
    public GameState State;
}

public enum GameState
{
    Booting,
    MainMenu,
    Settings,
    InGame,
    Paused,
    Results
}

public enum UIEventKey { OpenPauseMenu, BackToMainMenu }

public static class GameEventsEmitter
{
    private static Dictionary<EventType, List<Action<EventData>>> events = new Dictionary<EventType, List<Action<EventData>>>();

    public static void EmitEvent(EventType e, EventData payload) 
    {
        var actions = events[e];

        if (actions.Count > 0)
        {
            foreach (var action in actions)
            {
                action.Invoke(payload);
            }            
        }
    }

    public static void OnEvent(EventType e, Action<EventData> payload)
    {
        if (!events.ContainsKey(e))
        {
            events[e] = new List<Action<EventData>>();
        }

        var actions = events[e];

        if (actions != null)
        {
            if (actions.IndexOf(payload) < 0)
            {
                actions.Add(payload);
            }
        }
    }
}

public class GameStateController
{
    public static GameStateController Instance;
    public GameState State => currentState;

   private readonly Dictionary<UIEventKey, HashSet<GameState>> AllowedStates = new()
    {
        { UIEventKey.OpenPauseMenu, new HashSet<GameState> { GameState.InGame, GameState.Paused } },
        { UIEventKey.BackToMainMenu, new HashSet<GameState> { GameState.Results, GameState.Paused } },
    };

    private GameState currentState;

    public GameStateController()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        GameEventsEmitter.OnEvent(EventType.ChangeState, (data) =>
        {
            StateEventData stateData;

            if (data is StateEventData value)
            {
                stateData = value;
                SetState(stateData.State);
            }
        });
    }

    public bool IsEventAllowed(UIEventKey eventKey)
    {
        var state = currentState;
        return AllowedStates.TryGetValue(eventKey, out var allowedStates) &&
               allowedStates.Contains(state);
    }

    private void SetState(GameState newState)
    {
        currentState = newState;

        Debug.Log($"State Changed to {currentState}");
    }
}
