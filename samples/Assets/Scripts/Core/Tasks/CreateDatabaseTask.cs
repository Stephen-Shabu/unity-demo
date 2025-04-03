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
                    command.CommandText = "CREATE TABLE IF NOT EXISTS profiles (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, xplevel INTEGER, xptotal INTEGER, lastcompletedlevel INTEGER)";
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE TABLE xp_thresholds (level INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, exp_required INTEGER NOT NULL)";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (15);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (37);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (62);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (94);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (131);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (175);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (225);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (292);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (368);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (449);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (550);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (670);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (820);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (1022);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO xp_thresholds(exp_required) VALUES (1309);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"CREATE TRIGGER update_xp_level
                                            AFTER UPDATE OF xptotal ON profiles
                                            FOR EACH ROW
                                            BEGIN
                                                UPDATE profiles
                                                SET xplevel = 
                                                (
                                                    SELECT level
                                                    FROM xp_thresholds
                                                    WHERE NEW.xptotal >= exp_required
                                                    ORDER BY level DESC
                                                    LIMIT 1
                                                )
                                                WHERE id = NEW.id;
                                            END;";

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
