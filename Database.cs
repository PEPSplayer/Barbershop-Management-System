using Npgsql;
using System;
using System.Data;
using System.Windows;

namespace HairSalonIS.Data
{
    public static class Database
    {
        private static readonly string connectionString =
            "Host=localhost;Port=5432;Database=hair_salon_db;Username=postgres;Password=12345";

        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(connectionString);
        }

        public static DataTable ExecuteQuery(string query, params NpgsqlParameter[] parameters)
        {
            DataTable table = new DataTable();

            try
            {
                using (NpgsqlConnection connection = GetConnection())
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        if (parameters != null && parameters.Length > 0)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(table);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при выполнении запроса к базе данных:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            return table;
        }

        public static bool ExecuteNonQuery(string query, params NpgsqlParameter[] parameters)
        {
            try
            {
                using (NpgsqlConnection connection = GetConnection())
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        if (parameters != null && parameters.Length > 0)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при изменении данных:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return false;
            }
        }
    }
}