using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;

namespace Resturant_Management.Data
{
    public static class DbHelper
    {
        private static string _connectionString = "";

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                {
                    try
                    {
                        var settings = ConfigurationManager.ConnectionStrings["RestaurantDB"];
                        if (settings != null && !string.IsNullOrWhiteSpace(settings.ConnectionString))
                        {
                            _connectionString = settings.ConnectionString;
                        }
                    }
                    catch
                    {
                        // Fallback below
                    }

                    if (string.IsNullOrWhiteSpace(_connectionString))
                    {
                        _connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=RestaurantDB;Trusted_Connection=True;TrustServerCertificate=True;";
                    }
                }
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static bool TestConnection(out string errorMessage)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                errorMessage = "";
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public static DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using var conn = GetConnection();
            using var cmd = new SqlCommand(query, conn);
            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
            return dt;
        }

        public static int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(query, conn);
            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public static object? ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(query, conn);
            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }
            conn.Open();
            return cmd.ExecuteScalar();
        }

        private static string? _resolvedKhmerFontName = null;

        public static Font GetKhmerFont(float size, FontStyle style = FontStyle.Regular)
        {
            if (_resolvedKhmerFontName == null)
            {
                string[] candidates = { "Khmer OS Battambang", "Khmer OS", "Kantumruy Pro", "Leelawadee UI", "Khmer UI", "Segoe UI" };
                foreach (var fontName in candidates)
                {
                    try
                    {
                        using (var test = new Font(fontName, 10F))
                        {
                            if (test.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase))
                            {
                                _resolvedKhmerFontName = fontName;
                                break;
                            }
                        }
                    }
                    catch { }
                }
                _resolvedKhmerFontName ??= "Segoe UI";
            }

            try
            {
                return new Font(_resolvedKhmerFontName, size, style);
            }
            catch
            {
                return new Font("Segoe UI", size, style);
            }
        }
    }
}
