using UnityEngine;

public static class UIDefines
{
    public const string START_TEXT = "START";
    public const string RESUME_TEXT = "RESUME";
    public const string ROUND_COMPLETE_TEXT = "Well Done! \nYou completed round {0}";
    public const string TIME_UP_TEXT = "Time Up \n Try Again?";
    public const string YOU_DIED_TEXT = "You Died \n Try Again?";

    public enum ResultPanelState
    {
        Round_Complete,
        Game_Complete,
        Time_Up,
        You_Died
    }
}
