using UnityEngine;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using System.Data;
using System;

public static class GetProfileTask
{
    public static async Task<Profile> Execute(int id)
    {
        return await Request(id);
    }

    private static async Task<Profile> Request(int id)
    {
        var profile = new Profile();

        try
        {
            using (var connection = new SqliteConnection(GameDefines.DATABASE_PATH))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT xplevel, xptotal, lastcompletedlevel FROM profiles WHERE id = @id";
                    command.Parameters.Add(new SqliteParameter("@id", id));

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            profile.XpLevel = reader["xplevel"] is DBNull ? 0 : Convert.ToInt32(reader["xplevel"]);
                            profile.XpTotal = reader["xptotal"] is DBNull ? 0 : Convert.ToInt32(reader["xptotal"]);
                            profile.LastCompletedLevel = reader["lastcompletedlevel"] is DBNull ? 0 : Convert.ToInt32(reader["lastcompletedlevel"]);
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
