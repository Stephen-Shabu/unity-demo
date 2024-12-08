using System.IO;
using UnityEngine;

public static class GameDefines
{
    public static string DATABASE_PATH = "URI=file:" + Path.Combine(Application.persistentDataPath, "gameData.db");
    public static string DATABASE_FILE_PATH = Application.persistentDataPath + "/gameData.db";
}
