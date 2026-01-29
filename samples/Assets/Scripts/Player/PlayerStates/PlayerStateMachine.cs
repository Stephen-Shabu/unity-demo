using UnityEngine;
using Samples;

public interface PlayerStateCapable: StateCapable{}

public class PlayerStateMachine : BaseStateMachine<PlayerStateCapable>
{
    public PlayerContext Context { get; private set; }

    public PlayerStateMachine(PlayerContext context)
    {
        Context = context;
    }
}
