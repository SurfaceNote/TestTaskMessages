namespace MessageService.Data.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Npgsql;
    using Serilog;

    public class PostgresDatabase : IDatabase
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        public PostgresDatabase(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public void ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }
            }
            cmd.ExecuteNonQuery();
        }

        public DataTable ExecuteQuery(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }
            }
            using var reader = cmd.ExecuteReader();
            var dataTable = new DataTable();
            dataTable.Load(reader);
            return dataTable;
        }

        public void InitTables()
        {
            // Необходима проверка на случай, если мы первый раз запустили проект и у нас еще нет всех таблиц
            _logger.Information("Проверяем наличие необходимой таблицы в базе данных");
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var command = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS Messages (
                        Id SERIAL PRIMARY KEY,
                        Text VARCHAR(128) NOT NULL,
                        Timestamp TIMESTAMP NOT NULL,
                        SequenceNumber INTEGER NOT NULL
                    );
                ", connection);

                command.ExecuteNonQuery();
            }
        }
    }
}