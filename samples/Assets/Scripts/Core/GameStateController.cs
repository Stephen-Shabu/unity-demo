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
    HitRegistered,
    MeleeHitRegistered,
    RequestAttack,
    RoundComplete,
    ChangeWeapon
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

public struct WeaponChangeEventData : EventData
{
    public EventType Type { get; set; }

    public WeaponName Name { get; set; }
}

public struct ControlSchemeEventData : EventData
{
    public EventType Type { get; set; }

    public ControlScheme Scheme { get; set; }
}

public struct HitRegisterEventData : EventData
{
    public enum HitOwner
    {
        Player,
        Mob,
    }

    public EventType Type { get; set; }

    public HitOwner Owner { get; set; } 
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

public enum ControlScheme { Gamepad, KeyboardAndMouse }

public static class GameEventsEmitter
{
    private static Dictionary<EventType, List<Action<EventData>>> events = new Dictionary<EventType, List<Action<EventData>>>();

    public static void EmitEvent(EventType e, EventData payload) 
    {
        if (events.ContainsKey(e))
        {
            var actions = events[e];

            if (actions.Count > 0)
            {
                foreach (var action in actions)
                {
                    if (actions.Contains(action))
                        action.Invoke(payload);
                }
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

    public void HandleOnControlsChanged(PlayerInput input)
    {
        Debug.Log(input.currentControlScheme);

        if (input.currentControlScheme == "Keyboard&Mouse")
        {
            if (IsEventAllowed(UIEventKey.InMenu) || IsEventAllowed(UIEventKey.OpenPauseMenu))
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
            GameEventsEmitter.EmitEvent(EventType.ControlsChanged, new ControlSchemeEventData { Type = EventType.ControlsChanged, Scheme = ControlScheme.KeyboardAndMouse });
        }
        else if (input.currentControlScheme == "Gamepad")
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameEventsEmitter.EmitEvent(EventType.ControlsChanged, new ControlSchemeEventData { Type = EventType.ControlsChanged, Scheme = ControlScheme.Gamepad });
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
