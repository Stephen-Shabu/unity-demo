using Mono.Data.Sqlite;
using System.Data;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public static class UpdateProfileTask
{
    public static async Task<bool> Execute(int id, int xptotal, int lastLevel)
    {
        return await Request(id, xptotal, lastLevel);
    }

    private static async Task<bool> Request(int id, int xptotal, int lastLevel)
    {
        bool success = false;

        try
        {
            using (var connection = new SqliteConnection(GameDefines.DATABASE_PATH))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE profiles SET xptotal = @xptotal, lastcompletedlevel = @lastlevel";
                    command.Parameters.Add(new SqliteParameter("@xptotal", xptotal));
                    command.Parameters.Add(new SqliteParameter("@lastlevel", lastLevel));
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
