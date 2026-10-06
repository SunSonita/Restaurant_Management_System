using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Report
{
    public partial class SaleSummary : UserControl
    {
        private bool _isInitialized = false;

        public SaleSummary()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            txtSearch.Text = "";
            txtSearch.PlaceholderText = "Search...";
            this.Resize += (s, e) => ApplyResponsiveLayout();
            SetupFilters();
            _isInitialized = true;
            LoadReportData(false);
            this.Load += SaleSummary_Load;

            btnFilter.Click += (s, e) => LoadReportData(true);
            txtSearch.TextChanged += (s, e) => LoadReportData(false);
            comboCreator.SelectedIndexChanged += (s, e) => { if (_isInitialized) LoadReportData(false); };
            cmbTypeReport.SelectedIndexChanged += (s, e) => { if (_isInitialized) LoadReportData(false); };
        }

        private void SaleSummary_Load(object? sender, EventArgs e)
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
                cmbTypeReport.Items.Add("Summary by Order");
                cmbTypeReport.Items.Add("Daily Summary");
                cmbTypeReport.Items.Add("Summary by Creator");
                cmbTypeReport.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing filters: {ex.Message}");
            }
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
            ConfigureGridColumns(reportType);
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
                            bef.ToString("N2"),
                            dis.ToString("N2"),
                            aft.ToString("N2"),
                            paid.ToString("N2")
                        );
                    }
                }
                else if (reportType == "Summary by Creator")
                {
                    string sql = @"
SELECT 
    Creator,
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
                            bef.ToString("N2"),
                            dis.ToString("N2"),
                            aft.ToString("N2"),
                            paid.ToString("N2")
                        );
                    }
                }
                else // Summary by Order
                {
                    string sql = @"
SELECT 
    OrderID,
    OrderNo,
    PostingDate,
    Creator,
    TotalBeforeDis,
    DiscountItem,
    TotalAfterDis,
    Paid
FROM dbo.vw_SaleSummary
WHERE (PostingDate >= @Start AND PostingDate <= @End)
  AND (@Creator = 'All Creators' OR Creator = @Creator)
  AND (@Search = '' OR OrderNo LIKE @Pattern OR Creator LIKE @Pattern)
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
                        decimal paid = r["Paid"] != DBNull.Value ? Convert.ToDecimal(r["Paid"]) : 0m;

                        sumBeforeDis += bef;
                        sumDisItem += dis;
                        sumAfterDis += aft;
                        sumPaid += paid;

                        gridDataitem.Rows.Add(
                            r["OrderNo"]?.ToString() ?? rowNo.ToString(),
                            Convert.ToDateTime(r["PostingDate"]).ToString("yyyy-MM-dd HH:mm"),
                            bef.ToString("N2"),
                            dis.ToString("N2"),
                            aft.ToString("N2"),
                            paid.ToString("N2")
                        );
                        rowNo++;
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

            // Update summary labels
            if (lbTotalBeforeDis != null) lbTotalBeforeDis.Text = $"Total Before Discount : {sumBeforeDis:N2} KHR";
            if (lbTotalAfterDis != null) lbTotalAfterDis.Text = $"Discount : {sumDisItem:N2} KHR";
            if (lbTotalAfterDisc != null) lbTotalAfterDisc.Text = $"Total After Discount : {sumAfterDis:N2} KHR";
            if (lbGrandTotal != null) lbGrandTotal.Text = $"Grand Total : {sumAfterDis:N2} KHR";
            AlignSummaryLabels();
        }

        /// <summary>The first two grid columns hold different data per report type; label and size them to match.</summary>
        private void ConfigureGridColumns(string reportType)
        {
            (string first, string second, float firstWeight, float secondWeight) = reportType switch
            {
                "Daily Summary" => ("No", "Date", 40f, 110f),
                "Summary by Creator" => ("No", "Creator", 40f, 150f),
                _ => ("Order No", "Posting Date", 90f, 130f)
            };
            ApplyGridColumnLayout(first, second, firstWeight, secondWeight);
        }
        private void ApplyGridColumnLayout(string first, string second, float firstWeight, float secondWeight)
        {
            colNo.HeaderText = first;
            colNo.FillWeight = firstWeight;
            colPostingDate.HeaderText = second;
            colPostingDate.FillWeight = secondWeight;
            colDisItem.HeaderText = "Discount";

            foreach (DataGridViewColumn col in new DataGridViewColumn[] { colTotalbeforeDis, colDisItem, colTotalAtferDis, colPaid })
            {
                col.FillWeight = 110f;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                col.DefaultCellStyle.Padding = new Padding(0, 0, 10, 0);
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            colNo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPostingDate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
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

        private void panelInformationItem_Paint(object sender, PaintEventArgs e) { }
        private void comboUom_SelectedIndexChanged(object sender, EventArgs e) { }
        private void pnlInfoReportSale_Paint(object sender, PaintEventArgs e) { }
    }
}
