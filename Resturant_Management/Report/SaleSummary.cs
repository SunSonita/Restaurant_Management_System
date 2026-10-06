using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Report
{
    public partial class SaleSummary : UserControl
    {
        private bool _isInitialized = false;
        private Guna2Button btnExport = null!;
        private Guna2Button btnPrint = null!;
        private Guna2Button btnReset = null!;

        public SaleSummary()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            txtSearch.Text = "";
            txtSearch.PlaceholderText = "Search invoice, creator, table...";

            CreateActionButtons();

            this.Resize += (s, e) => ApplyResponsiveLayout();
            SetupFilters();
            _isInitialized = true;
            this.Load += SaleSummary_Load;

            btnFilter.Click += (s, e) => LoadReportData(true);
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LoadReportData(false); } };
            comboCreator.SelectedIndexChanged += (s, e) => { if (_isInitialized) LoadReportData(false); };
            cmbTypeReport.SelectedIndexChanged += (s, e) => { if (_isInitialized) { ConfigureGridColumns(); LoadReportData(false); } };
        }

        private void CreateActionButtons()
        {
            // Reset Button
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

            // Export CSV Button
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
            btnExport.Click += (s, e) => ReportExporter.ExportToCsv(gridDataitem, "SaleSummaryReport");

            // Print Report Button
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
                string subtitle = $"Period: {DateFrom.Value:yyyy-MM-dd} {TimeFrom.Value:HH:mm} to {DateTo.Value:yyyy-MM-dd} {TimeTo.Value:HH:mm} | Type: {cmbTypeReport.SelectedItem} | Creator: {comboCreator.SelectedItem}";
                ReportExporter.PrintReport(gridDataitem, "Sale Summary Report", subtitle);
            };

            pnlInfoReportSale.Controls.Add(btnReset);
            pnlInfoReportSale.Controls.Add(btnExport);
            pnlInfoReportSale.Controls.Add(btnPrint);
        }

        private void SaleSummary_Load(object? sender, EventArgs e)
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
                cmbTypeReport.Items.Add("Summary by Order");
                cmbTypeReport.Items.Add("Daily Summary");
                cmbTypeReport.Items.Add("Summary by Creator");
                cmbTypeReport.Items.Add("Summary by Payment Method");
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
            string reportType = cmbTypeReport.SelectedItem?.ToString() ?? "Summary by Order";

            if (reportType == "Daily Summary")
            {
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "No", Width = 50 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sale Date", Width = 130 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Orders", Width = 80 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Before Dis (KHR)", Width = 170 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Discount Item (KHR)", Width = 150 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total After Dis (KHR)", Width = 160 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Paid (KHR)", Width = 140 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total (USD)", Width = 120 });
            }
            else if (reportType == "Summary by Creator")
            {
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "No", Width = 50 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Creator / Cashier", Width = 180 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Orders", Width = 80 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Before Dis (KHR)", Width = 170 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Discount Item (KHR)", Width = 150 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total After Dis (KHR)", Width = 160 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Paid (KHR)", Width = 140 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total (USD)", Width = 120 });
            }
            else if (reportType == "Summary by Payment Method")
            {
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "No", Width = 50 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Payment Method", Width = 180 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Transactions", Width = 100 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Received", Width = 160 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total (USD Approx)", Width = 150 });
            }
            else // Summary by Order
            {
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "No", Width = 45 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Posting Date/Time", Width = 140 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Invoice No", Width = 110 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Creator", Width = 110 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Table", Width = 100 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Before Dis", Width = 120 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Discount Item", Width = 110 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total After Dis", Width = 120 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Grand Total", Width = 120 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Paid Amount", Width = 110 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Change", Width = 90 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Method", Width = 85 });
                gridDataitem.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", Width = 80 });
            }

            ApplyGridStyle();
        }

        private void LoadReportData(bool isExplicitFilter)
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

            string selectedCreator = comboCreator.SelectedItem?.ToString() ?? "All Creators";
            string reportType = cmbTypeReport.SelectedItem?.ToString() ?? "Summary by Order";
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
                DbHelper.EnsureReportViews();

                DataTable dt;

                if (reportType == "Daily Summary")
                {
                    string sql = @"
SELECT 
    CONVERT(varchar(10), PostingDate, 120) AS PostDate,
    COUNT(OrderID) AS OrderCount,
    SUM(TotalBeforeDis) AS TotalBeforeDis,
    SUM(DiscountItem) AS DiscountItem,
    SUM(TotalAfterDis) AS TotalAfterDis,
    SUM(Paid) AS Paid
FROM dbo.vw_SaleSummary
WHERE (PostingDate >= @Start AND PostingDate <= @End)
  AND (@Creator = 'All Creators' OR Creator = @Creator)
  AND (@Search = '' OR CONVERT(varchar(10), PostingDate, 120) LIKE @Pattern)
GROUP BY CONVERT(varchar(10), PostingDate, 120)
ORDER BY PostDate DESC;";

                    dt = DbHelper.ExecuteQuery(sql,
                        new SqlParameter("@Start", startDateTime),
                        new SqlParameter("@End", endDateTime),
                        new SqlParameter("@Creator", selectedCreator),
                        new SqlParameter("@Search", search),
                        new SqlParameter("@Pattern", $"%{search}%"));

                    int rowNo = 1;
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

                        gridDataitem.Rows.Add(
                            rowNo++,
                            r["PostDate"]?.ToString(),
                            r["OrderCount"]?.ToString(),
                            bef.ToString("N2"),
                            dis.ToString("N2"),
                            aft.ToString("N2"),
                            paid.ToString("N2"),
                            (aft / 4000m).ToString("N2")
                        );
                    }
                }
                else if (reportType == "Summary by Creator")
                {
                    string sql = @"
SELECT 
    Creator,
    COUNT(OrderID) AS OrderCount,
    SUM(TotalBeforeDis) AS TotalBeforeDis,
    SUM(DiscountItem) AS DiscountItem,
    SUM(TotalAfterDis) AS TotalAfterDis,
    SUM(Paid) AS Paid
FROM dbo.vw_SaleSummary
WHERE (PostingDate >= @Start AND PostingDate <= @End)
  AND (@Creator = 'All Creators' OR Creator = @Creator)
  AND (@Search = '' OR Creator LIKE @Pattern)
GROUP BY Creator
ORDER BY Creator ASC;";

                    dt = DbHelper.ExecuteQuery(sql,
                        new SqlParameter("@Start", startDateTime),
                        new SqlParameter("@End", endDateTime),
                        new SqlParameter("@Creator", selectedCreator),
                        new SqlParameter("@Search", search),
                        new SqlParameter("@Pattern", $"%{search}%"));

                    int rowNo = 1;
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

                        gridDataitem.Rows.Add(
                            rowNo++,
                            r["Creator"]?.ToString(),
                            r["OrderCount"]?.ToString(),
                            bef.ToString("N2"),
                            dis.ToString("N2"),
                            aft.ToString("N2"),
                            paid.ToString("N2"),
                            (aft / 4000m).ToString("N2")
                        );
                    }
                }
                else if (reportType == "Summary by Payment Method")
                {
                    string sql = @"
SELECT 
    PaymentMethod,
    COUNT(OrderID) AS OrderCount,
    SUM(Paid) AS TotalPaid
FROM dbo.vw_SaleSummary
WHERE (PostingDate >= @Start AND PostingDate <= @End)
  AND (@Creator = 'All Creators' OR Creator = @Creator)
  AND (@Search = '' OR PaymentMethod LIKE @Pattern)
GROUP BY PaymentMethod
ORDER BY PaymentMethod ASC;";

                    dt = DbHelper.ExecuteQuery(sql,
                        new SqlParameter("@Start", startDateTime),
                        new SqlParameter("@End", endDateTime),
                        new SqlParameter("@Creator", selectedCreator),
                        new SqlParameter("@Search", search),
                        new SqlParameter("@Pattern", $"%{search}%"));

                    int rowNo = 1;
                    foreach (DataRow r in dt.Rows)
                    {
                        decimal paid = r["TotalPaid"] != DBNull.Value ? Convert.ToDecimal(r["TotalPaid"]) : 0m;
                        sumPaid += paid;
                        sumAfterDis += paid;

                        gridDataitem.Rows.Add(
                            rowNo++,
                            r["PaymentMethod"]?.ToString(),
                            r["OrderCount"]?.ToString(),
                            paid.ToString("N2") + " KHR",
                            (paid / 4000m).ToString("N2") + " USD"
                        );
                    }
                }
                else // Summary by Order
                {
                    string sql = @"
SELECT 
    OrderID,
    InvoiceNo,
    OrderNo,
    PostingDate,
    Creator,
    TableName,
    TotalBeforeDis,
    DiscountItem,
    TotalAfterDis,
    GrandTotal,
    Paid,
    ChangeAmount,
    PaymentMethod,
    PaymentStatus
FROM dbo.vw_SaleSummary
WHERE (PostingDate >= @Start AND PostingDate <= @End)
  AND (@Creator = 'All Creators' OR Creator = @Creator)
  AND (@Search = '' OR InvoiceNo LIKE @Pattern OR OrderNo LIKE @Pattern OR Creator LIKE @Pattern OR TableName LIKE @Pattern OR PaymentMethod LIKE @Pattern)
ORDER BY PostingDate DESC;";

                    dt = DbHelper.ExecuteQuery(sql,
                        new SqlParameter("@Start", startDateTime),
                        new SqlParameter("@End", endDateTime),
                        new SqlParameter("@Creator", selectedCreator),
                        new SqlParameter("@Search", search),
                        new SqlParameter("@Pattern", $"%{search}%"));

                    int rowNo = 1;
                    foreach (DataRow r in dt.Rows)
                    {
                        decimal bef = r["TotalBeforeDis"] != DBNull.Value ? Convert.ToDecimal(r["TotalBeforeDis"]) : 0m;
                        decimal dis = r["DiscountItem"] != DBNull.Value ? Convert.ToDecimal(r["DiscountItem"]) : 0m;
                        decimal aft = r["TotalAfterDis"] != DBNull.Value ? Convert.ToDecimal(r["TotalAfterDis"]) : 0m;
                        decimal grand = r["GrandTotal"] != DBNull.Value ? Convert.ToDecimal(r["GrandTotal"]) : aft;
                        decimal paid = r["Paid"] != DBNull.Value ? Convert.ToDecimal(r["Paid"]) : 0m;
                        decimal chg = r["ChangeAmount"] != DBNull.Value ? Convert.ToDecimal(r["ChangeAmount"]) : 0m;

                        sumBeforeDis += bef;
                        sumDisItem += dis;
                        sumAfterDis += aft;
                        sumPaid += paid;

                        string invNo = r["InvoiceNo"]?.ToString() ?? "";
                        if (string.IsNullOrEmpty(invNo)) invNo = r["OrderNo"]?.ToString() ?? "";

                        gridDataitem.Rows.Add(
                            rowNo++,
                            Convert.ToDateTime(r["PostingDate"]).ToString("yyyy-MM-dd HH:mm"),
                            invNo,
                            r["Creator"]?.ToString(),
                            r["TableName"]?.ToString(),
                            bef.ToString("N2"),
                            dis.ToString("N2"),
                            aft.ToString("N2"),
                            grand.ToString("N2"),
                            paid.ToString("N2"),
                            chg.ToString("N2"),
                            r["PaymentMethod"]?.ToString(),
                            r["PaymentStatus"]?.ToString()
                        );
                    }
                }

                if (dt.Rows.Count == 0 && isExplicitFilter)
                {
                    MessageBox.Show("No sales records found matching the selected filter criteria.",
                                    "Sale Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sale summary data: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            gridDataitem.ClearSelection();

            // Update summary labels
            if (lbTotalBeforeDis != null) lbTotalBeforeDis.Text = $"Total Before Discount : {sumBeforeDis:N2} KHR ({(sumBeforeDis / 4000m):N2} USD)";
            if (lbTotalAfterDis != null) lbTotalAfterDis.Text = $"Discount : {sumDisItem:N2} KHR ({(sumDisItem / 4000m):N2} USD)";
            if (lbTotalAfterDisc != null) lbTotalAfterDisc.Text = $"Total After Discount : {sumAfterDis:N2} KHR ({(sumAfterDis / 4000m):N2} USD)";
            if (lbGrandTotal != null) lbGrandTotal.Text = $"Grand Total : {sumAfterDis:N2} KHR ({(sumAfterDis / 4000m):N2} USD)";
            AlignSummaryLabels();
        }

        /// <summary>
        /// Shared look for the report grid: flat navy header matching the title and Filter button,
        /// light row separators, amounts right-aligned. Runs after the columns for a report type are built.
        /// </summary>
        private void ApplyGridStyle()
        {
            var amountHeader = new System.Text.RegularExpressions.Regex("KHR|USD|Total Before|Discount|Paid|Net|Grand", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            foreach (DataGridViewColumn col in gridDataitem.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                bool isAmount = amountHeader.IsMatch(col.HeaderText ?? "") && !(col.HeaderText ?? "").Contains("Orders");
                var align = isAmount ? DataGridViewContentAlignment.MiddleRight : DataGridViewContentAlignment.MiddleLeft;
                var pad = isAmount ? new Padding(0, 0, 12, 0) : new Padding(10, 0, 0, 0);
                col.DefaultCellStyle.Alignment = align;
                col.DefaultCellStyle.Padding = pad;
                col.HeaderCell.Style.Alignment = align;
                col.HeaderCell.Style.Padding = pad;
            }

            gridDataitem.AllowUserToAddRows = false;
            gridDataitem.ReadOnly = true;

            Color navy = Color.FromArgb(10, 20, 110);
            gridDataitem.EnableHeadersVisualStyles = false;
            gridDataitem.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            gridDataitem.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            gridDataitem.ColumnHeadersHeight = 40;
            gridDataitem.ColumnHeadersDefaultCellStyle.BackColor = navy;
            gridDataitem.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridDataitem.ColumnHeadersDefaultCellStyle.SelectionBackColor = navy;
            gridDataitem.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            gridDataitem.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            gridDataitem.ThemeStyle.HeaderStyle.BackColor = navy;
            gridDataitem.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            gridDataitem.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            gridDataitem.ThemeStyle.HeaderStyle.Height = 40;

            gridDataitem.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            gridDataitem.GridColor = Color.FromArgb(228, 230, 240);
            gridDataitem.ThemeStyle.GridColor = Color.FromArgb(228, 230, 240);
            gridDataitem.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 248, 252);
            gridDataitem.ThemeStyle.AlternatingRowsStyle.BackColor = Color.FromArgb(247, 248, 252);
            gridDataitem.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 230, 250);
            gridDataitem.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 25, 60);
            gridDataitem.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 230, 250);
            gridDataitem.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 25, 60);
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

            // DataGridView fills from Y = 218 down to the summary section
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

        private void panelInformationItem_Paint(object sender, PaintEventArgs e) { }
        private void comboUom_SelectedIndexChanged(object sender, EventArgs e) { }
        private void pnlInfoReportSale_Paint(object sender, PaintEventArgs e) { }
    }
}
