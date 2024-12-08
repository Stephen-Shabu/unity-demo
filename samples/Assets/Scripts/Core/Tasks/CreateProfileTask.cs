using UnityEngine;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using System.Data;
using System;

public static class CreateProfileTask
{
    public static async Task<Profile> Execute()
    {
        return await Request();
    }

    private static async Task<Profile> Request()
    {
        var profile = new Profile();

        try
        {
            using (var connection = new SqliteConnection(GameDefines.DATABASE_PATH))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO profiles (xplevel, xptotal, lastcompletedlevel) VALUES (0, 0, 0)";
                    command.ExecuteNonQuery();

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            profile.Id = Convert.ToInt32(reader["id"]);
                            profile.XpLevel = Convert.ToInt32(reader["xplevel"]);
                            profile.XpTotal = Convert.ToInt32(reader["xptotal"]);
                            profile.LastCompletedLevel = Convert.ToInt32(reader["lastcompletedlevel"]);
                        }
                    }
                }
            }
        }
        catch (Exception ex) 
        {
            Debug.LogError("SQLite error: " + ex.Message);
            profile = null;
        }

        return profile;
    }
}
