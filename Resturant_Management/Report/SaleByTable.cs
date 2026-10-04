using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Report
{
    public partial class SaleByTable : UserControl
    {
        private bool _isInitialized = false;
        private Guna2Button btnExport = null!;
        private Guna2Button btnPrint = null!;
        private Guna2Button btnReset = null!;

        public SaleByTable()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            txtSearch.Text = "";
            txtSearch.PlaceholderText = "Search table, group, order...";

            CreateActionButtons();

            this.Resize += (s, e) => ApplyResponsiveLayout();
            SetupFilters();
            _isInitialized = true;
            this.Load += SaleByTable_Load;

            btnFilter.Click += (s, e) => LoadReportData(true);
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LoadReportData(false); } };
            comboCreator.SelectedIndexChanged += (s, e) => { if (_isInitialized) LoadReportData(false); };
            cmbTypeReport.SelectedIndexChanged += (s, e) => { if (_isInitialized) { ConfigureGridColumns(); LoadReportData(false); } };
        }

        private void CreateActionButtons()
        {
            btnReset = new Guna2Button
            {
                Text = "Reset",
                Size = new Size(80, 48),
                BorderRadius = 6,
                FillColor = Color.FromArgb(140, 150, 165),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReset.Click += (s, e) => ResetFilters();

            btnExport = new Guna2Button
            {
                Text = "Export CSV",
                Size = new Size(105, 48),
                BorderRadius = 6,
                FillColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExport.Click += (s, e) => ReportExporter.ExportToCsv(gridDataitem, "SaleByTableReport");

            btnPrint = new Guna2Button
            {
                Text = "Print",
                Size = new Size(85, 48),
                BorderRadius = 6,
                FillColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPrint.Click += (s, e) =>
            {
                string subtitle = $"Period: {DateFrom.Value:yyyy-MM-dd} {TimeFrom.Value:HH:mm} to {DateTo.Value:yyyy-MM-dd} {TimeTo.Value:HH:mm} | Group: {comboCreator.SelectedItem} | Type: {cmbTypeReport.SelectedItem}";
                ReportExporter.PrintReport(gridDataitem, "Sale By Table Report", subtitle);
            };

            pnlInfoReportSale.Controls.Add(btnReset);
            pnlInfoReportSale.Controls.Add(btnExport);
            pnlInfoReportSale.Controls.Add(btnPrint);
        }

        private void SaleByTable_Load(object? sender, EventArgs e)
        {
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            ApplyResponsiveLayout();
            ConfigureGridColumns();
            SetupFilters();
            _isInitialized = true;
            LoadReportData(false);
        }

        private void ResetFilters()
        {
            DateFrom.Value = DateTime.Today.AddDays(-7);
            DateTo.Value = DateTime.Today;
            TimeFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0);
            TimeTo.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 23, 59, 59);
            txtSearch.Text = "";
            if (comboCreator.Items.Count > 0) comboCreator.SelectedIndex = 0;
            if (cmbTypeReport.Items.Count > 0) cmbTypeReport.SelectedIndex = 0;
            ConfigureGridColumns();
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

                // Use comboCreator as Dining Area / Table Group Filter
                comboCreator.Items.Clear();
                comboCreator.Items.Add("All Dining Areas");
                DataTable dtGroups = DbHelper.ExecuteQuery("SELECT GroupName FROM dbo.TABLE_GROUP ORDER BY GroupName");
                foreach (DataRow r in dtGroups.Rows)
                {
                    string? name = r["GroupName"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                        comboCreator.Items.Add(name);
                }
                comboCreator.Items.Add("Take Away / Direct");
                comboCreator.SelectedIndex = 0;

                // Populate Report Type ComboBox
                cmbTypeReport.Items.Clear();
                cmbTypeReport.Items.Add("Group by Table");
                cmbTypeReport.Items.Add("Detailed Orders by Table");
                cmbTypeReport.Items.Add("Group by Dining Area");
                cmbTypeReport.Items.Add("Take Away & Delivery Orders");
                cmbTypeReport.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing filters: {ex.Message}");
            }
        }

        private void ConfigureGridColumns()
        {
            gridDataitem.Columns.Clear();
            gridDataitem.AutoGenerateColumns = false;
            string reportType = cmbTypeReport.SelectedItem?.ToString() ?? "Group by Table";

            if (reportType == "Group by Table")
            {
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Table / Area", Width = 160 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Dining Group", Width = 140 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Orders", Width = 70 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Before Dis (KHR)", Width = 150 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Discount (KHR)", Width = 130 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Net Sales (KHR)", Width = 140 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Paid Amount (KHR)", Width = 140 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Net Sales (USD)", Width = 120 });
            }
            else if (reportType == "Group by Dining Area")
            {
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Dining Area / Group", Width = 200 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Orders", Width = 100 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Before Dis (KHR)", Width = 170 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Discount (KHR)", Width = 140 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Net Sales (KHR)", Width = 160 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Paid (KHR)", Width = 140 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Net (USD)", Width = 120 });
            }
            else // Detailed Orders by Table or Takeaway
            {
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Invoice No", Width = 130 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Order No", Width = 100 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Posting Date", Width = 140 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Table", Width = 120 });
                gridDataitem.Columns.Add(new HeaderTextColumn("Dining Area", 120));
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Creator", Width = 110 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Before Dis", Width = 130 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Discount", Width = 110 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Grand Total", Width = 130 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Paid Amount", Width = 120 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", Width = 80 });
            }
        }

        private class HeaderTextColumn : DataGridViewTextBoxColumn
        {
            public HeaderTextColumn(string title, int width)
            {
                HeaderText = title;
                Width = width;
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

            string selectedArea = comboCreator.SelectedItem?.ToString() ?? "All Dining Areas";
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
    GroupTable,
    COUNT(OrderID) AS OrderCount,
    SUM(TotalBeforeDis) AS TotalBeforeDis,
    SUM(DiscountItem) AS DiscountItem,
    SUM(TotalAfterDis) AS TotalAfterDis,
    SUM(Paid) AS Paid
FROM dbo.vw_SaleByTable
WHERE (PostingDate >= @Start AND PostingDate <= @End)
  AND (@Area = 'All Dining Areas' OR GroupTable = @Area)
  AND (@Search = '' OR TableName LIKE @Pattern OR GroupTable LIKE @Pattern)
GROUP BY TableName, GroupTable
ORDER BY GroupTable, TableName ASC;";

                    dt = DbHelper.ExecuteQuery(sql,
                        new SqlParameter("@Start", startDateTime),
                        new SqlParameter("@End", endDateTime),
                        new SqlParameter("@Area", selectedArea),
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
                            r["GroupTable"]?.ToString(),
                            count,
                            bef.ToString("N2"),
                            dis.ToString("N2"),
                            aft.ToString("N2"),
                            paid.ToString("N2"),
                            (aft / 4000m).ToString("N2")
                        );
                    }
                }
                else if (reportType == "Group by Dining Area")
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
  AND (@Area = 'All Dining Areas' OR GroupTable = @Area)
  AND (@Search = '' OR GroupTable LIKE @Pattern)
GROUP BY GroupTable
ORDER BY GroupTable ASC;";

                    dt = DbHelper.ExecuteQuery(sql,
                        new SqlParameter("@Start", startDateTime),
                        new SqlParameter("@End", endDateTime),
                        new SqlParameter("@Area", selectedArea),
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
                            count,
                            bef.ToString("N2"),
                            dis.ToString("N2"),
                            aft.ToString("N2"),
                            paid.ToString("N2"),
                            (aft / 4000m).ToString("N2")
                        );
                    }
                }
                else if (reportType == "Take Away & Delivery Orders")
                {
                    string sql = @"
SELECT 
    InvoiceNo,
    OrderNo,
    PostingDate,
    TableName,
    GroupTable,
    Creator,
    TotalBeforeDis,
    DiscountItem,
    TotalAfterDis,
    Paid,
    Status
FROM dbo.vw_SaleByTable
WHERE (PostingDate >= @Start AND PostingDate <= @End)
  AND (TableID IS NULL OR TableName LIKE '%Take%' OR TableName LIKE '%Delivery%')
  AND (@Search = '' OR InvoiceNo LIKE @Pattern OR OrderNo LIKE @Pattern OR TableName LIKE @Pattern OR Creator LIKE @Pattern)
ORDER BY PostingDate DESC;";

                    dt = DbHelper.ExecuteQuery(sql,
                        new SqlParameter("@Start", startDateTime),
                        new SqlParameter("@End", endDateTime),
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

                        string invNo = r["InvoiceNo"]?.ToString() ?? "";
                        if (string.IsNullOrEmpty(invNo)) invNo = r["OrderNo"]?.ToString() ?? "";

                        gridDataitem.Rows.Add(
                            invNo,
                            r["OrderNo"]?.ToString(),
                            Convert.ToDateTime(r["PostingDate"]).ToString("yyyy-MM-dd HH:mm"),
                            r["TableName"]?.ToString(),
                            r["GroupTable"]?.ToString(),
                            r["Creator"]?.ToString(),
                            bef.ToString("N2"),
                            dis.ToString("N2"),
                            aft.ToString("N2"),
                            paid.ToString("N2"),
                            r["Status"]?.ToString()
                        );
                    }
                }
                else // Detailed Orders by Table
                {
                    string sql = @"
SELECT 
    OrderID,
    InvoiceNo,
    OrderNo,
    PostingDate,
    Creator,
    GroupTable,
    TableName,
    TotalBeforeDis,
    DiscountItem,
    TotalAfterDis,
    Paid,
    Status
FROM dbo.vw_SaleByTable
WHERE (PostingDate >= @Start AND PostingDate <= @End)
  AND (@Area = 'All Dining Areas' OR GroupTable = @Area)
  AND (@Search = '' OR InvoiceNo LIKE @Pattern OR OrderNo LIKE @Pattern OR TableName LIKE @Pattern OR GroupTable LIKE @Pattern OR Creator LIKE @Pattern)
ORDER BY PostingDate DESC;";

                    dt = DbHelper.ExecuteQuery(sql,
                        new SqlParameter("@Start", startDateTime),
                        new SqlParameter("@End", endDateTime),
                        new SqlParameter("@Area", selectedArea),
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

                        string invNo = r["InvoiceNo"]?.ToString() ?? "";
                        if (string.IsNullOrEmpty(invNo)) invNo = r["OrderNo"]?.ToString() ?? "";

                        gridDataitem.Rows.Add(
                            invNo,
                            r["OrderNo"]?.ToString(),
                            Convert.ToDateTime(r["PostingDate"]).ToString("yyyy-MM-dd HH:mm"),
                            r["TableName"]?.ToString(),
                            r["GroupTable"]?.ToString(),
                            r["Creator"]?.ToString(),
                            bef.ToString("N2"),
                            dis.ToString("N2"),
                            aft.ToString("N2"),
                            paid.ToString("N2"),
                            r["Status"]?.ToString()
                        );
                    }
                }

                if (dt.Rows.Count == 0 && isExplicitFilter)
                {
                    MessageBox.Show("No table sales records found matching the selected filter criteria.",
                                    "Sale by Table", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sale by table data: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (lbTotalBeforeDis != null) lbTotalBeforeDis.Text = $"Total Before Discount : {sumBeforeDis:N2} KHR ({(sumBeforeDis / 4000m):N2} USD)";
            if (lbTotalAfterDis != null) lbTotalAfterDis.Text = $"Discount Item : {sumDisItem:N2} KHR ({(sumDisItem / 4000m):N2} USD)";
            if (lbTotalAfterDisc != null) lbTotalAfterDisc.Text = $"Net Sales : {sumAfterDis:N2} KHR ({(sumAfterDis / 4000m):N2} USD)";
            if (lbGrandTotal != null) lbGrandTotal.Text = $"Paid Amount : {sumPaid:N2} KHR ({(sumPaid / 4000m):N2} USD)";
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

            // Right-align search box & action buttons
            int rightX = pnlInfoReportSale.Width - 25;

            if (btnFilter != null)
            {
                btnFilter.Location = new Point(rightX - btnFilter.Width, 31);
                rightX -= (btnFilter.Width + 8);
            }

            if (btnReset != null)
            {
                btnReset.Location = new Point(rightX - btnReset.Width, 31);
                rightX -= (btnReset.Width + 8);
            }

            if (btnPrint != null)
            {
                btnPrint.Location = new Point(rightX - btnPrint.Width, 31);
                rightX -= (btnPrint.Width + 8);
            }

            if (btnExport != null)
            {
                btnExport.Location = new Point(rightX - btnExport.Width, 31);
            }

            if (txtSearch != null)
            {
                txtSearch.Location = new Point(pnlInfoReportSale.Width - txtSearch.Width - 25, 168);
            }

            // Summary Totals: neatly aligned at bottom right
            AlignSummaryLabels();

            // DataGridView fills from Y = 218 down to summary section
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
