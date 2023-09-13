using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace LenkasLittleHelper.Database
{
    static class DBHelper
    {
        private const string DBSource = "Data Source=LenkasLittleHelper.db";
        private static SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(DBSource);
            connection.Open();
            return connection;
        }

        public static string? ExecuteReader(string sql, Dictionary<string, object> cmdParams, Action<SqliteDataReader> e)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return "Порожній запит!";
            }

            try
            {
                using (var conn = GetConnection())
                {
                    var command = conn.CreateCommand();
                    command.CommandText = sql;

                    foreach (var kv in cmdParams)
                    {
                        command.Parameters.AddWithValue($"@{kv.Key}", kv.Value);
                    }

                    var reader = command.ExecuteReader();

                    e(reader);
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return null;
        }

        public static string? ExecuteReader(string sql, Action<SqliteDataReader> e)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return "Порожній запит!";
            }

            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = sql;

                    var reader = command.ExecuteReader();

                    e(reader);
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return null;
        }

        public static string? DoCommand(string? sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return "Порожній запит!";
            }
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = sql;

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return null;
        }
    }
}