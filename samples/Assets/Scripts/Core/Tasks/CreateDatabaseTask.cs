using Mono.Data.Sqlite;
using System.Data;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public static class CreateDatabaseTask
{
    public static async Task<bool> Execute()
    {
        return await Request();
    }

    private static async Task<bool> Request()
    {
        bool success = false;

        try
        {
            using (var connection = new SqliteConnection(GameDefines.DATABASE_PATH))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CREATE TABLE IF NOT EXISTS profiles (id INTEGER PRIMARY KEY AUTO_INCREMENT NOT NULL, xplevel INTEGER, xptotal INTEGER, lastcompletedlevel INTEGER)";
                    command.ExecuteNonQuery();
                }
            }
            success = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("SQLite error: " + ex.Message);
        }

        return success;
    }
}
