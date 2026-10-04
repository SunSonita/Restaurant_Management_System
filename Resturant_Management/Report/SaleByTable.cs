using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Report
{
    public partial class SaleByTable : UserControl
    {
        private bool _isInitialized = false;

        public SaleByTable()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            txtSearch.Text = "";
            txtSearch.PlaceholderText = "Search...";
            this.Resize += (s, e) => ApplyResponsiveLayout();
            SetupFilters();
            _isInitialized = true;
            LoadReportData(false);
            this.Load += SaleByTable_Load;

            btnFilter.Click += (s, e) => LoadReportData(true);
            txtSearch.TextChanged += (s, e) => LoadReportData(false);
            comboCreator.SelectedIndexChanged += (s, e) => { if (_isInitialized) LoadReportData(false); };
            cmbTypeReport.SelectedIndexChanged += (s, e) => { if (_isInitialized) LoadReportData(false); };
        }

        private void SaleByTable_Load(object? sender, EventArgs e)
        {
            ApplyResponsiveLayout();
            SetupFilters();
            _isInitialized = true;
            LoadReportData(false);
        }

        private void SetupFilters()
        {
            try
            {
                DateFrom.Value = DateTime.Today.AddDays(-7);
                DateTo.Value = DateTime.Today;
                TimeFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0);
                TimeTo.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 23, 59, 59);

                // Populate Creator ComboBox
                comboCreator.Items.Clear();
                comboCreator.Items.Add("All Creators");
                DataTable dtUsers = DbHelper.ExecuteQuery("SELECT DISTINCT FullName FROM dbo.APP_USER ORDER BY FullName");
                foreach (DataRow r in dtUsers.Rows)
                {
                    string? name = r["FullName"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                        comboCreator.Items.Add(name);
                }
                comboCreator.SelectedIndex = 0;

                // Populate Report Type ComboBox
                cmbTypeReport.Items.Clear();
                cmbTypeReport.Items.Add("Group by Table");
                cmbTypeReport.Items.Add("Detailed Orders by Table");
                cmbTypeReport.Items.Add("Group by Table Group");
                cmbTypeReport.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing filters: {ex.Message}");
            }
        }

        private void LoadReportData(bool isExplicitFilter = false)
        {
            gridDataitem.Rows.Clear();

            DateTime startDateTime = DateFrom.Value.Date + TimeFrom.Value.TimeOfDay;
            DateTime endDateTime = DateTo.Value.Date + TimeTo.Value.TimeOfDay;

            if (endDateTime < startDateTime)
            {
                if (isExplicitFilter)
                {
                    MessageBox.Show("End Date & Time cannot be earlier than Start Date & Time.",
                                    "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }

            string creator = comboCreator.SelectedItem?.ToString() ?? "All Creators";
            string reportType = cmbTypeReport.SelectedItem?.ToString() ?? "Group by Table";
            string search = txtSearch.Text.Trim();
            if (search.StartsWith("Search", StringComparison.OrdinalIgnoreCase) && search.Contains("."))
            {
                search = "";
            }

            decimal sumBeforeDis = 0m;
            decimal sumDisItem = 0m;
            decimal sumAfterDis = 0m;
            decimal sumPaid = 0m;

            try
            {
                DataTable dt;

                if (reportType == "Group by Table")
                {
                    string sql = @"
SELECT 
    TableName,
    COUNT(OrderID) AS OrderCount,
    SUM(TotalBeforeDis) AS TotalBeforeDis,
    SUM(DiscountItem) AS DiscountItem,
    SUM(TotalAfterDis) AS TotalAfterDis,
    SUM(Paid) AS Paid
FROM dbo.vw_SaleByTable
WHERE (PostingDate >= @Start AND PostingDate <= @End)
  AND (@Creator = 'All Creators' OR Creator = @Creator)
  AND (@Search = '' OR TableName LIKE @Pattern OR GroupTable LIKE @Pattern)
GROUP BY TableName
ORDER BY TableName ASC;";

                    dt = DbHelper.ExecuteQuery(sql,
                        new SqlParameter("@Start", startDateTime),
                        new SqlParameter("@End", endDateTime),
                        new SqlParameter("@Creator", creator),
                        new SqlParameter("@Search", search),
                        new SqlParameter("@Pattern", $"%{search}%"));

                    foreach (DataRow r in dt.Rows)
                    {
                        decimal bef = r["TotalBeforeDis"] != DBNull.Value ? Convert.ToDecimal(r["TotalBeforeDis"]) : 0m;
                        decimal dis = r["DiscountItem"] != DBNull.Value ? Convert.ToDecimal(r["DiscountItem"]) : 0m;
                        decimal aft = r["TotalAfterDis"] != DBNull.Value ? Convert.ToDecimal(r["TotalAfterDis"]) : 0m;
                        decimal paid = r["Paid"] != DBNull.Value ? Convert.ToDecimal(r["Paid"]) : 0m;
                        int count = r["OrderCount"] != DBNull.Value ? Convert.ToInt32(r["OrderCount"]) : 0;

                        sumBeforeDis += bef;
                        sumDisItem += dis;
                        sumAfterDis += aft;
                        sumPaid += paid;

                        gridDataitem.Rows.Add(
                            r["TableName"]?.ToString(),
                            $"{count} Order{(count == 1 ? "" : "s")}",
                            bef.ToString("N2"),
                            dis.ToString("N2"),
                            aft.ToString("N2"),
                            paid.ToString("N2")
                        );
                    }
                }
                else if (reportType == "Group by Table Group")
                {
                    string sql = @"
SELECT 
    GroupTable,
    COUNT(OrderID) AS OrderCount,
    SUM(TotalBeforeDis) AS TotalBeforeDis,
    SUM(DiscountItem) AS DiscountItem,
    SUM(TotalAfterDis) AS TotalAfterDis,
    SUM(Paid) AS Paid
FROM dbo.vw_SaleByTable
WHERE (PostingDate >= @Start AND PostingDate <= @End)
  AND (@Creator = 'All Creators' OR Creator = @Creator)
  AND (@Search = '' OR GroupTable LIKE @Pattern)
GROUP BY GroupTable
ORDER BY GroupTable ASC;";

                    dt = DbHelper.ExecuteQuery(sql,
                        new SqlParameter("@Start", startDateTime),
                        new SqlParameter("@End", endDateTime),
                        new SqlParameter("@Creator", creator),
                        new SqlParameter("@Search", search),
                        new SqlParameter("@Pattern", $"%{search}%"));

                    foreach (DataRow r in dt.Rows)
                    {
                        decimal bef = r["TotalBeforeDis"] != DBNull.Value ? Convert.ToDecimal(r["TotalBeforeDis"]) : 0m;
                        decimal dis = r["DiscountItem"] != DBNull.Value ? Convert.ToDecimal(r["DiscountItem"]) : 0m;
                        decimal aft = r["TotalAfterDis"] != DBNull.Value ? Convert.ToDecimal(r["TotalAfterDis"]) : 0m;
                        decimal paid = r["Paid"] != DBNull.Value ? Convert.ToDecimal(r["Paid"]) : 0m;
                        int count = r["OrderCount"] != DBNull.Value ? Convert.ToInt32(r["OrderCount"]) : 0;

                        sumBeforeDis += bef;
                        sumDisItem += dis;
                        sumAfterDis += aft;
                        sumPaid += paid;

                        gridDataitem.Rows.Add(
                            r["GroupTable"]?.ToString(),
                            $"{count} Order{(count == 1 ? "" : "s")}",
                            bef.ToString("N2"),
                            dis.ToString("N2"),
                            aft.ToString("N2"),
                            paid.ToString("N2")
                        );
                    }
                }
                else // Detailed Orders by Table
                {
                    string sql = @"
SELECT 
    OrderID,
    OrderNo,
    PostingDate,
    Creator,
    GroupTable,
    TableName,
    TotalBeforeDis,
    DiscountItem,
    TotalAfterDis,
    Paid
FROM dbo.vw_SaleByTable
WHERE (PostingDate >= @Start AND PostingDate <= @End)
  AND (@Creator = 'All Creators' OR Creator = @Creator)
  AND (@Search = '' OR OrderNo LIKE @Pattern OR TableName LIKE @Pattern OR GroupTable LIKE @Pattern)
ORDER BY PostingDate DESC;";

                    dt = DbHelper.ExecuteQuery(sql,
                        new SqlParameter("@Start", startDateTime),
                        new SqlParameter("@End", endDateTime),
                        new SqlParameter("@Creator", creator),
                        new SqlParameter("@Search", search),
                        new SqlParameter("@Pattern", $"%{search}%"));

                    foreach (DataRow r in dt.Rows)
                    {
                        decimal bef = r["TotalBeforeDis"] != DBNull.Value ? Convert.ToDecimal(r["TotalBeforeDis"]) : 0m;
                        decimal dis = r["DiscountItem"] != DBNull.Value ? Convert.ToDecimal(r["DiscountItem"]) : 0m;
                        decimal aft = r["TotalAfterDis"] != DBNull.Value ? Convert.ToDecimal(r["TotalAfterDis"]) : 0m;
                        decimal paid = r["Paid"] != DBNull.Value ? Convert.ToDecimal(r["Paid"]) : 0m;

                        sumBeforeDis += bef;
                        sumDisItem += dis;
                        sumAfterDis += aft;
                        sumPaid += paid;

                        string displayTitle = $"{r["OrderNo"]} ({r["TableName"]})";

                        gridDataitem.Rows.Add(
                            displayTitle,
                            Convert.ToDateTime(r["PostingDate"]).ToString("yyyy-MM-dd HH:mm"),
                            bef.ToString("N2"),
                            dis.ToString("N2"),
                            aft.ToString("N2"),
                            paid.ToString("N2")
                        );
                    }
                }

                if (dt.Rows.Count == 0 && isExplicitFilter)
                {
                    MessageBox.Show("No table sale records found matching the selected filter criteria.",
                                    "Sale by Table", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sale by table data: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (lbTotalBeforeDis != null) lbTotalBeforeDis.Text = $"Total Before Discount : {sumBeforeDis:N2} KHR";
            if (lbTotalAfterDis != null) lbTotalAfterDis.Text = $"Discount Item : {sumDisItem:N2} KHR";
            if (lbTotalAfterDisc != null) lbTotalAfterDisc.Text = $"Total After Discount : {sumAfterDis:N2} KHR";
            if (lbGrandTotal != null) lbGrandTotal.Text = $"Grand Total : {sumAfterDis:N2} KHR";
            AlignSummaryLabels();
        }

        private void ApplyResponsiveLayout()
        {
            if (pnlInfoReportSale == null || gridDataitem == null) return;

            this.SuspendLayout();
            pnlInfoReportSale.SuspendLayout();

            int marginX = 25;
            int marginTop = 70;
            int marginBottom = 20;

            pnlInfoReportSale.Location = new Point(marginX, marginTop);
            pnlInfoReportSale.Size = new Size(
                Math.Max(800, this.ClientSize.Width - (marginX * 2)),
                Math.Max(450, this.ClientSize.Height - marginTop - marginBottom)
            );

            // Right-align search box above the grid
            int padRight = 25;
            if (txtSearch != null)
            {
                txtSearch.Location = new Point(pnlInfoReportSale.Width - txtSearch.Width - padRight, 168);
            }

            // Summary Totals: neatly aligned at the bottom right
            AlignSummaryLabels();

            // DataGridView: Fills from Y = 218 down to the summary section
            int gridTop = 218;
            int summaryReserved = 135;
            int gridHeight = Math.Max(150, pnlInfoReportSale.Height - gridTop - summaryReserved);
            int gridWidth = Math.Max(500, pnlInfoReportSale.Width - 50);

            gridDataitem.Location = new Point(25, gridTop);
            gridDataitem.Size = new Size(gridWidth, gridHeight);
            gridDataitem.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            pnlInfoReportSale.ResumeLayout(true);
            this.ResumeLayout(true);
        }

        private void AlignSummaryLabels()
        {
            if (pnlInfoReportSale == null) return;

            int padRight = 30;
            int bottomY = pnlInfoReportSale.Height - 15;

            if (lbGrandTotal != null)
                lbGrandTotal.Location = new Point(pnlInfoReportSale.Width - lbGrandTotal.PreferredWidth - padRight, bottomY - 24);

            if (lbTotalAfterDisc != null)
                lbTotalAfterDisc.Location = new Point(pnlInfoReportSale.Width - lbTotalAfterDisc.PreferredWidth - padRight, bottomY - 49);

            if (lbTotalAfterDis != null)
                lbTotalAfterDis.Location = new Point(pnlInfoReportSale.Width - lbTotalAfterDis.PreferredWidth - padRight, bottomY - 74);

            if (lbTotalBeforeDis != null)
                lbTotalBeforeDis.Location = new Point(pnlInfoReportSale.Width - lbTotalBeforeDis.PreferredWidth - padRight, bottomY - 99);
        }

        private void label2_Click(object? sender, EventArgs e) { }
    }
}
