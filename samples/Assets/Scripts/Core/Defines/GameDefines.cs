using System.IO;
using UnityEngine;

public static class GameDefines
{
    public static string DATABASE_PATH = "URI=file:" + Path.Combine(Application.persistentDataPath, "gameData.db");
    public static string DATABASE_FILE_PATH = Application.persistentDataPath + "/gameData.db";
    public static string TIMER_FORMAT = "{0}m:{1}s";

    public class CameraSettings
    {
        public const float BASE_TOP_PAN_SPEED = 300f;
        public const float MK_ACC_PAN_SPEED = 20f;
        public const float GPAD_ACC_PAN_SPEED = 100f;
    }
}
