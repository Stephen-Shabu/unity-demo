using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum EventType
{
    ChangeState,
    ControlsChanged,
    EnemyDefeated,
    PlayerDefeated,
    MeleeHitRegistered,
    RequestAttack,
    RoundComplete
}

public interface EventData
{
    public EventType Type { get; set; }
}

public struct GenericEventData: EventData
{
    public EventType Type { get; set; }
    public GameObject Caller { get; set; }
}

public struct StateEventData: EventData
{
    public EventType Type { get; set; }

    public GameState State;
}

public struct AttackRequestEventData : EventData
{
    public EventType Type { get; set; }

    public GameObject Attacker;
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

public enum UIEventKey { OpenPauseMenu, BackToMainMenu, InMenu}

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
    public static GameStateController Instance { get { return instance ?? (instance = new GameStateController()); } private set { } }

    public GameState State => currentState;

    private readonly Dictionary<UIEventKey, HashSet<GameState>> AllowedStates = new()
    {
        { UIEventKey.OpenPauseMenu, new HashSet<GameState> { GameState.InGame, GameState.Paused } },
        { UIEventKey.BackToMainMenu, new HashSet<GameState> { GameState.Results, GameState.Paused } },
        { UIEventKey.InMenu, new HashSet<GameState> { GameState.Results, GameState.Paused, GameState.MainMenu, GameState.Settings } }
    };

    private GameState currentState;
    private static GameStateController instance;

    public GameStateController()
    {
        if (instance == null)
        {
            instance = this;
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

    private void HandleOnControlsChanged(PlayerInput input)
    {
        Debug.Log(input.currentControlScheme);

        if (input.currentControlScheme == "Keyboard&Mouse")
        {
            if (IsEventAllowed(UIEventKey.InMenu) || IsEventAllowed(UIEventKey.OpenPauseMenu))
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
        }
        else if (input.currentControlScheme == "Gamepad")
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
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
