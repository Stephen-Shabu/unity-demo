using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

public class DatabaseManager : MonoBehaviour
{
    private string dbPath;

    public void CreateDatabase()
    {
        dbPath = "URI=file:" + Path.Combine(Application.persistentDataPath, "gameData.db");

        if (!File.Exists(Application.persistentDataPath + "/gameData.db"))
        {
            using (var connection = new SqliteConnection(dbPath))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CREATE TABLE IF NOT EXISTS profiles (id INTEGER PRIMARY KEY, xplevel INTEGER, xptotal INTEGER, lastcompletedlevel INTEGER)";
                    command.ExecuteNonQuery();
                }
            }

            Debug.Log("Database created successfully at " + dbPath);
        }
        else
        {
            Debug.Log("Database already exists at " + dbPath);
        }
    }
}
