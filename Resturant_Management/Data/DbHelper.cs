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

        private static bool _reportViewsChecked = false;

        // Per-order report views, all amounts in KHR. Kept in sync with Database/RestaurantDB_Schema.sql.
        private const string SaleSummaryViewSql = @"
CREATE OR ALTER VIEW dbo.vw_SaleSummary
AS
SELECT 
    o.OrderID,
    o.PostingDate,
    ISNULL(o.InvoiceNo, o.OrderNo) AS InvoiceNo,
    o.OrderNo,
    ISNULL(u.FullName, 'System') AS Creator,
    o.SubTotal AS TotalBeforeDiscount,
    o.SubTotal AS TotalBeforeDis,
    o.ItemDiscountTotal + o.DocDiscountAmount AS DiscountItem,
    o.DocDiscountAmount AS DiscountOrder,
    o.GrandTotal AS TotalAfterDiscount,
    o.GrandTotal AS TotalAfterDis,
    0 AS Tax,
    0 AS ServiceCharge,
    o.GrandTotal,
    CASE WHEN o.Status = 'Paid' THEN o.GrandTotal ELSE 0 END AS PaidAmount,
    CASE WHEN o.Status = 'Paid' THEN o.GrandTotal ELSE 0 END AS Paid,
    ISNULL(p.ChangeGiven * p.ExchangeRate, 0) AS Change,
    ISNULL(p.ChangeGiven * p.ExchangeRate, 0) AS ChangeAmount,
    ISNULL(pm.MethodName, 'Cash') AS PaymentMethod,
    o.Status AS PaymentStatus,
    t.TableName,
    c.CustomerName
FROM dbo.SALE_ORDER o
LEFT JOIN dbo.APP_USER u ON o.CreatedBy = u.UserID
LEFT JOIN dbo.DINING_TABLE t ON o.TableID = t.TableID
LEFT JOIN dbo.CUSTOMER c ON o.CustomerID = c.CustomerID
OUTER APPLY (
    SELECT TOP 1 PaymentID, ChangeGiven, ExchangeRate
    FROM dbo.PAYMENT WHERE OrderID = o.OrderID
    ORDER BY PaymentID DESC
) p
OUTER APPLY (
    SELECT TOP 1 m.MethodName
    FROM dbo.PAYMENT_DETAIL pd
    JOIN dbo.PAYMENT_METHOD m ON pd.MethodID = m.MethodID
    WHERE pd.PaymentID = p.PaymentID
) pm
WHERE o.Status IN ('Sent', 'Billed', 'Paid');";

        private const string SaleByTableViewSql = @"
CREATE OR ALTER VIEW dbo.vw_SaleByTable
AS
SELECT 
    o.OrderID,
    ISNULL(o.InvoiceNo, o.OrderNo) AS InvoiceNo,
    o.OrderNo,
    o.PostingDate,
    o.TableID,
    CASE 
        WHEN o.TableID IS NULL THEN 'Takeaway / Delivery' 
        ELSE ISNULL(t.TableName, 'Table ' + CAST(o.TableID AS varchar(10))) 
    END AS TableName,
    ISNULL(tg.GroupName, 'Main Dining Hall') AS GroupTable,
    ISNULL(u.FullName, 'System') AS Creator,
    o.SubTotal AS TotalBeforeDis,
    o.ItemDiscountTotal + o.DocDiscountAmount AS DiscountItem,
    o.GrandTotal AS TotalAfterDis,
    CASE WHEN o.Status = 'Paid' THEN o.GrandTotal ELSE 0 END AS Paid,
    o.Status
FROM dbo.SALE_ORDER o
LEFT JOIN dbo.DINING_TABLE t ON o.TableID = t.TableID
LEFT JOIN dbo.TABLE_GROUP tg ON t.TableGroupID = tg.TableGroupID
LEFT JOIN dbo.APP_USER u ON o.CreatedBy = u.UserID
WHERE o.Status IN ('Sent', 'Billed', 'Paid');";

        // Orders saved before the POS fix subtracted the document discount twice (it is already inside the line discounts).
        // Recompute with the POS rule: item discount = line discounts - doc discount; grand = subtotal - item - doc.
        // Idempotent: correctly saved orders already match and are left unchanged.
        private const string RepairDocDiscountSql = @"
UPDATE o
SET o.ItemDiscountTotal = x.NewItem,
    o.GrandTotal = CASE WHEN o.SubTotal - x.NewItem - o.DocDiscountAmount > 0 
                        THEN o.SubTotal - x.NewItem - o.DocDiscountAmount ELSE 0 END
FROM dbo.SALE_ORDER o
CROSS APPLY (
    SELECT CASE WHEN SUM(CASE WHEN i.TotalBeforeDis > i.TotalAfterDis THEN i.TotalBeforeDis - i.TotalAfterDis ELSE 0 END) > o.DocDiscountAmount
                THEN SUM(CASE WHEN i.TotalBeforeDis > i.TotalAfterDis THEN i.TotalBeforeDis - i.TotalAfterDis ELSE 0 END) - o.DocDiscountAmount
                ELSE 0 END AS NewItem,
           COUNT(*) AS LineCount
    FROM dbo.SALE_ORDER_ITEM i
    WHERE i.OrderID = o.OrderID
) x
WHERE o.DocDiscountAmount > 0
  AND x.LineCount > 0
  AND (o.ItemDiscountTotal <> x.NewItem
       OR o.GrandTotal <> CASE WHEN o.SubTotal - x.NewItem - o.DocDiscountAmount > 0
                               THEN o.SubTotal - x.NewItem - o.DocDiscountAmount ELSE 0 END);";

        /// <summary>
        /// Brings the report views up to date (per-order, KHR, Paid in KHR) and repairs orders whose
        /// document discount was double-counted. Runs once per app session; safe to run repeatedly.
        /// </summary>
        public static void EnsureReportViews()
        {
            if (_reportViewsChecked) return;

            ExecuteNonQuery(SaleSummaryViewSql);
            ExecuteNonQuery(SaleByTableViewSql);
            ExecuteNonQuery(RepairDocDiscountSql);

            _reportViewsChecked = true;
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
