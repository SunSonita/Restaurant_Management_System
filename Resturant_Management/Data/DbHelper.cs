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

        private static bool _paymentSchemaChecked = false;

        /// <summary>
        /// Adds payment columns that older databases are missing (e.g. PAYMENT.ExchangeRate, PAYMENT.TotalDue).
        /// Mirrors the migration block in Database/RestaurantDB_Schema.sql so POS payment works without re-running the script.
        /// </summary>
        public static void EnsurePaymentSchema()
        {
            if (_paymentSchemaChecked) return;

            const string sql = @"
IF COL_LENGTH('dbo.PAYMENT', 'InvoiceNo') IS NULL
    ALTER TABLE dbo.PAYMENT ADD [InvoiceNo] varchar(50) NULL;
IF COL_LENGTH('dbo.PAYMENT', 'TotalDue') IS NULL
    ALTER TABLE dbo.PAYMENT ADD [TotalDue] decimal(18,2) NULL;
IF COL_LENGTH('dbo.PAYMENT', 'ChangeAmount') IS NULL
    ALTER TABLE dbo.PAYMENT ADD [ChangeAmount] decimal(18,2) NULL;
IF COL_LENGTH('dbo.PAYMENT', 'ExchangeRate') IS NULL
    ALTER TABLE dbo.PAYMENT ADD [ExchangeRate] decimal(18,4) NOT NULL CONSTRAINT DF_PAYMENT_ExchangeRate DEFAULT 4000;
IF COL_LENGTH('dbo.SALE_ORDER', 'ExchangeRate') IS NULL
    ALTER TABLE dbo.SALE_ORDER ADD [ExchangeRate] decimal(18,4) NULL;";

            ExecuteNonQuery(sql);
            _paymentSchemaChecked = true;
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
