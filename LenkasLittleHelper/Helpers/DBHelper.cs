using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
        public static string? DoCommand(string? sql, Dictionary<string,object> cmdParams)
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

                    foreach (var kv in cmdParams)
                    {
                        command.Parameters.AddWithValue($"@{kv.Key}", kv.Value);
                    }

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

        //public static T? GetValueOrDefault<T>(this SqliteDataReader reader, int idx, T? defaultVal = default)
        //{
        //    if (reader[idx] is DBNull)
        //    {
        //        return defaultVal;
        //    }

        //    string val = reader.GetString(idx);

        //    return ParseObject<T>(val);
        //}

        public static T? GetValueOrDefault<T>(this SqliteDataReader reader, string column, T? defaultVal = default)
        {
            object col = reader[column];

            if (col is DBNull || col == null)
            {
                return defaultVal;
            }

            string? val = col.ToString();

            if (string.IsNullOrEmpty(val))
            {
                return defaultVal;
            }

            return ParseObject<T>(val);
        }

        private static T? ParseObject<T>(string value)
        {
            Type type = typeof(T);

            if (type == typeof(int) || type == typeof(bool))
            {
                if (!int.TryParse(value, out int resVal))
                {
                    return default;
                }

                if (type == typeof(bool))
                {
                    return (T)Convert.ChangeType(resVal == 1, type);
                }

                return (T)Convert.ChangeType(resVal, type);
            }
            else if (type == typeof(DateTime))
            {
                if (Regex.IsMatch(value, "^[0-9]+$"))
                {
                    if (!long.TryParse(value, out long ticks))
                    {
                        return default;
                    }
                    var dt = DateTime.FromBinary(ticks);
                    return (T)Convert.ChangeType(dt, typeof(T));
                }

                if (!DateTime.TryParse(value, out var datetime))
                {
                    return default;
                }

                return (T)Convert.ChangeType(datetime, typeof(T));
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}