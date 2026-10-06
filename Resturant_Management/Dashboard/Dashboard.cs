using FontAwesome.Sharp;
using Guna.UI2.WinForms;
using Resturant_Management.Item_Group;
using Resturant_Management.POS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management
{
    public partial class dashboard : Form
    {
        private Panel mainContentPanel = null!;   // host panel (lives for the whole form)
        private Panel dashboardView = null!;      // dashboard content (built once, reused)
        private Label lblKpiSalesToday = null!;
        private Label lblKpiAvgSaleAmount = null!;
        private Label lblKpiAvgSaleQty = null!;
        private Chart pieChart = null!;
        private Chart barChart = null!;
        private bool isInventoryExpanded = false;
        private bool isTableExpanded = false;
        private const int RowH = 52;
        private const int HeaderH = 80;
        private const int SidebarExpandedW = 233;
        private const int SidebarCollapsedW = 60;

        private bool sidebarCollapsed = false;
        private IconPictureBox collapseBtn = null!;
        private Panel pnlSearch = null!;
        private Image? logoImage;
        private IconPictureBox? inventoryChevron;
        private IconPictureBox? tableChevron;

        private Label lblUserName = null!;
        private Label lblUserRole = null!;
        private Guna2Button btnLogout = null!;
        private Guna2Button btnExitApp = null!;

        private class ButtonBackup
        {
            public string Text = "";
            public Image? Image;
            public Size ImageSize;
        }

        private readonly Dictionary<Guna2Button, ButtonBackup> btnBackups = new Dictionary<Guna2Button, ButtonBackup>();
        private readonly Dictionary<Guna2Button, Image> tileCache = new Dictionary<Guna2Button, Image>();

        public dashboard()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }

        // ------------------------------------------------------------------
        //  View switching
        // ------------------------------------------------------------------

        /// <summary>
        /// Removes everything from the host panel. Loaded views (Report, POS, ...)
        /// are disposed; the dashboard panel is only detached so it can be reused.
        /// </summary>
        private void ClearHost()
        {
            foreach (Control c in mainContentPanel.Controls.Cast<Control>().ToList())
            {
                mainContentPanel.Controls.Remove(c);
                if (c != dashboardView)
                    c.Dispose();
            }
        }

        private void LoadView(Control view)
        {
            ClearHost();
            view.Dock = DockStyle.Fill;

            if (view is Form form)
            {
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Show();
            }

            mainContentPanel.Controls.Add(view);
            view.BringToFront();
        }

        private void ShowDashboard()
        {
            ClearHost();
            dashboardView.Dock = DockStyle.Fill;
            mainContentPanel.Controls.Add(dashboardView);
            dashboardView.BringToFront();

            RefreshDashboardData();
        }

        // ------------------------------------------------------------------
        //  Form load
        // ------------------------------------------------------------------

        private void dashboard_Load_1(object sender, EventArgs e)
        {
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            try
            {
                sidebar.AutoScroll = true;

                // The Designer never hooks this up, so do it here
                // (remove this line if you add it in dashboard.Designer.cs instead)
                btndashboard.Click -= btndashboard_Click;
                btndashboard.Click += btndashboard_Click;

                if (PictureLogo != null)
                {
                    PictureLogo.SizeMode = PictureBoxSizeMode.Zoom;
                    PictureLogo.Height = HeaderH;
                    PictureLogo.Dock = DockStyle.Top;
                    PictureLogo.BackColor = Color.FromArgb(191, 234, 255);
                }

                btndashboard.Dock = DockStyle.Top;
                btnPOS.Dock = DockStyle.Top;
                Inventory.Dock = DockStyle.Top;
                btnReport.Dock = DockStyle.Top;

                if (PictureLogo != null) PictureLogo.BringToFront();
                btndashboard.BringToFront();
                btnPOS.BringToFront();
                Inventory.BringToFront();
                btnReport.BringToFront();

                Guna2Button[] allSubButtons = { btninventory, btnItemGroup, btnItemMasterData, btnTable, btnTableGroup, btnTableName };
                foreach (var btn in allSubButtons)
                {
                    if (btn != null && !Inventory.Controls.Contains(btn))
                    {
                        Inventory.Controls.Add(btn);
                    }
                }

                btninventory.Dock = DockStyle.Top;
                btninventory.Height = RowH;

                Guna2Button[] level1SubButtons = { btnItemGroup, btnItemMasterData, btnTable };
                foreach (Guna2Button btn in level1SubButtons)
                {
                    if (btn != null)
                    {
                        btn.Dock = DockStyle.Top;
                        btn.Height = RowH;
                        btn.Margin = new Padding(0);
                        btn.TextAlign = HorizontalAlignment.Left;
                        btn.ImageAlign = HorizontalAlignment.Left;
                        btn.ImageOffset = new Point(15, 0);
                        btn.TextOffset = new Point(48, 0);
                    }
                }

                Guna2Button[] level2SubButtons = { btnTableGroup, btnTableName };
                foreach (Guna2Button btn in level2SubButtons)
                {
                    if (btn != null)
                    {
                        btn.Dock = DockStyle.Top;
                        btn.Height = RowH;
                        btn.Margin = new Padding(0);
                        btn.TextAlign = HorizontalAlignment.Left;
                        btn.ImageAlign = HorizontalAlignment.Left;
                        btn.ImageOffset = new Point(35, 0);
                        btn.TextOffset = new Point(68, 0);
                        btn.Visible = false;
                    }
                }

                btninventory.BringToFront();
                btnItemGroup.BringToFront();
                btnItemMasterData.BringToFront();
                btnTable.BringToFront();
                btnTableGroup.BringToFront();
                btnTableName.BringToFront();

                Inventory.Height = RowH;

                if (!UserSession.CanManageTables)
                {
                    btnTable.Visible = false;
                    btnTableGroup.Visible = false;
                    btnTableName.Visible = false;
                }

                SetupHeaderUserProfile();
                StyleSidebar();
                InitializeDashboardView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Layout error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------------
        //  Sidebar styling / collapsing
        // ------------------------------------------------------------------

        private void StyleSidebar()
        {
            Color text = Color.FromArgb(30, 30, 30);
            Color hover = Color.FromArgb(238, 243, 248);

            sidebar.BackColor = Color.White;
            logoImage = PictureLogo.Image;

            //  Header button ----
            PictureLogo.Height = HeaderH;
            PictureLogo.BackColor = Color.FromArgb(191, 234, 255);

            int btnY = (HeaderH - 26) / 2;
            collapseBtn = new IconPictureBox
            {
                IconChar = IconChar.ChevronLeft,
                IconColor = Color.White,
                IconSize = 14,
                Size = new Size(26, 26),
                BackColor = Color.FromArgb(84, 100, 112),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            collapseBtn.Location = new Point(SidebarExpandedW - 40, btnY);
            using (var gp = new GraphicsPath())
            {
                gp.AddEllipse(0, 0, 26, 26);
                collapseBtn.Region = new Region(gp);
            }
            collapseBtn.Click += PictureLogo_Click;
            PictureLogo.Controls.Add(collapseBtn);

            // Search box
            var searchIcon = new IconPictureBox
            {
                IconChar = IconChar.Search,
                IconColor = Color.FromArgb(90, 90, 90),
                IconSize = 18
            };

            pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                Padding = new Padding(10, 10, 10, 8),
                BackColor = Color.White
            };
            pnlSearch.Controls.Add(new Guna2TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Tap to search menu",
                BorderRadius = 4,
                BorderColor = Color.FromArgb(170, 170, 170),
                FillColor = Color.White,
                Font = new Font("Segoe UI", 10.5F),
                IconLeft = searchIcon.Image,
                IconLeftSize = new Size(18, 18)
            });
            sidebar.Controls.Add(pnlSearch);

            void StyleBtn(Guna2Button b, float fontSize)
            {
                b.FillColor = Color.White;
                b.ForeColor = text;
                b.BorderRadius = 0;
                b.Font = new Font("Segoe UI", fontSize);
                b.TextAlign = HorizontalAlignment.Left;
                b.ImageAlign = HorizontalAlignment.Left;
                b.HoverState.FillColor = hover;
            }

            IconPictureBox AddChevron(Guna2Button b, int x)
            {
                foreach (var old in b.Controls.OfType<IconPictureBox>().ToList())
                    b.Controls.Remove(old);

                var ch = new IconPictureBox
                {
                    IconChar = IconChar.ChevronRight,
                    IconColor = text,
                    IconSize = 12,
                    Size = new Size(12, 12),
                    BackColor = Color.Transparent,
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    Cursor = Cursors.Hand,
                    Location = new Point(x, (RowH - 12) / 2)
                };
                ch.Click += (s, e) => b.PerformClick();
                b.Controls.Add(ch);
                return ch;
            }

            foreach (var b in new[] { btndashboard, btnPOS, btninventory, btnReport,
                                      btnItemGroup, btnItemMasterData, btnTable,
                                      btnTableGroup, btnTableName })
                b.Height = RowH;
            Inventory.Height = RowH;

            StyleBtn(btndashboard, 10.5F);
            foreach (var b in new[] { btnPOS, btninventory, btnReport })
                StyleBtn(b, 10.5F);

            inventoryChevron = AddChevron(btninventory, 14);      // only Inventory has a dropdown

            Inventory.FillColor = Color.White;

            // Sub rows under Inventory
            foreach (var b in new[] { btnItemGroup, btnItemMasterData, btnTable, btnTableGroup, btnTableName })
                StyleBtn(b, 10F);
            tableChevron = AddChevron(btnTable, 28);          // Table also has a dropdown placed to the left of "Table"

            // Remember the original text/icon so we can switch back after collapsing
            foreach (var b in new[] { btndashboard, btnPOS, btninventory, btnReport })
            {
                btnBackups[b] = new ButtonBackup
                {
                    Text = b.Text,
                    Image = b.Image,
                    ImageSize = b.ImageSize
                };
            }

            //  Stack order (top -> bottom) ----
            PictureLogo.BringToFront();
            pnlSearch.BringToFront();
            btndashboard.BringToFront();
            btnPOS.BringToFront();
            Inventory.BringToFront();
            btnReport.BringToFront();

            SetSidebarCollapsed(false);
        }

        private void SetSidebarCollapsed(bool collapse)
        {
            sidebarCollapsed = collapse;
            sidebar.SuspendLayout();

            sidebar.Width = collapse ? SidebarCollapsedW : SidebarExpandedW;

            // header
            PictureLogo.Image = collapse ? null : logoImage;
            collapseBtn.IconChar = collapse ? IconChar.ChevronRight : IconChar.ChevronLeft;
            int btnY = (HeaderH - collapseBtn.Height) / 2;
            collapseBtn.Location = collapse
                ? new Point((SidebarCollapsedW - collapseBtn.Width) / 2, btnY)
                : new Point(SidebarExpandedW - 40, btnY);

            // search box
            pnlSearch.Visible = !collapse;

            // dropdown arrows
            if (inventoryChevron != null)
            {
                inventoryChevron.Visible = !collapse;
                inventoryChevron.IconChar = IconChar.ChevronRight;
            }
            if (tableChevron != null)
            {
                tableChevron.Visible = !collapse;
                tableChevron.IconChar = IconChar.ChevronRight;
            }

            // Inventory dropdown always starts closed after a switch
            isInventoryExpanded = false;
            isTableExpanded = false;
            Inventory.Height = RowH;

            btnTableGroup.Visible = false;
            btnTableName.Visible = false;
            btnItemGroup.Visible = !collapse;
            btnItemMasterData.Visible = !collapse;
            btnTable.Visible = !collapse && UserSession.CanManageTables;

            // top-level buttons
            Guna2Button[] topButtons = { btndashboard, btnPOS, btninventory, btnReport };
            foreach (var b in topButtons)
            {
                var bk = btnBackups[b];

                if (collapse)
                {
                    b.Text = "";
                    b.Image = GetTile(b, bk.Image);
                    b.ImageSize = new Size(SidebarCollapsedW, RowH);
                    b.ImageAlign = HorizontalAlignment.Left;
                    b.ImageOffset = new Point(0, 0);
                    b.TextOffset = new Point(0, 0);
                }
                else
                {
                    b.Text = bk.Text;
                    b.Image = bk.Image;
                    b.ImageSize = bk.ImageSize;
                    b.ImageAlign = HorizontalAlignment.Left;
                    b.TextAlign = HorizontalAlignment.Left;

                    bool isDash = b == btndashboard;
                    b.ImageOffset = new Point(isDash ? 14 : 34, 0);
                    b.TextOffset = new Point(isDash ? 24 : 44, 0);
                }
            }

            // sub buttons keep their indent
            foreach (var b in new[] { btnItemGroup, btnItemMasterData, btnTable })
            {
                b.ImageOffset = new Point(15, 0);
                b.TextOffset = new Point(48, 0);
            }
            foreach (var b in new[] { btnTableGroup, btnTableName })
            {
                b.ImageOffset = new Point(35, 0);
                b.TextOffset = new Point(68, 0);
            }

            sidebar.ResumeLayout();
        }

        // Rounded-square "tile" with the button icon inside
        private Image GetTile(Guna2Button b, Image? icon)
        {
            if (!tileCache.TryGetValue(b, out var tile))
            {
                tile = MakeTile(icon);
                tileCache[b] = tile;
            }
            return tile;
        }

        private static Bitmap MakeTile(Image? icon)
        {
            const int T = 40;
            var bmp = new Bitmap(SidebarCollapsedW, RowH);

            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                float ox = (SidebarCollapsedW - T) / 2f;
                float oy = (RowH - T) / 2f;
                var r = new RectangleF(ox + 1.5f, oy + 1.5f, T - 4, T - 4);

                // soft shadow
                using (var shadow = new SolidBrush(Color.FromArgb(25, 0, 0, 0)))
                using (var sp = TileRoundRect(new RectangleF(r.X, r.Y + 1.5f, r.Width, r.Height), 7))
                    g.FillPath(shadow, sp);

                // tile body + border
                using (var p = TileRoundRect(r, 7))
                using (var fill = new SolidBrush(Color.FromArgb(250, 250, 251)))
                using (var pen = new Pen(Color.FromArgb(205, 210, 215)))
                {
                    g.FillPath(fill, p);
                    g.DrawPath(pen, p);
                }

                if (icon != null)
                    g.DrawImage(icon, new Rectangle((int)ox + 8, (int)oy + 8, 24, 24));
            }

            return bmp;
        }

        private static GraphicsPath TileRoundRect(RectangleF r, float radius)
        {
            float d = radius * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        // ------------------------------------------------------------------
        //  Sidebar click handlers
        // ------------------------------------------------------------------

        private void btninventory_Click(object sender, EventArgs e)
        {
            if (sidebarCollapsed)
            {
                SetSidebarCollapsed(false);
                isInventoryExpanded = false;
            }

            if (isInventoryExpanded)
            {
                Inventory.Height = RowH;
                isInventoryExpanded = false;
                if (inventoryChevron != null) inventoryChevron.IconChar = IconChar.ChevronRight;

                isTableExpanded = false;
                if (tableChevron != null) tableChevron.IconChar = IconChar.ChevronRight;
                if (btnTableGroup != null) btnTableGroup.Visible = false;
                if (btnTableName != null) btnTableName.Visible = false;
            }
            else
            {
                int subItemCount = UserSession.CanManageTables ? (isTableExpanded ? 5 : 3) : 2;
                Inventory.Height = RowH * (1 + subItemCount);
                isInventoryExpanded = true;
                if (inventoryChevron != null) inventoryChevron.IconChar = IconChar.ChevronDown;
            }

            if (PicProfile != null)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, PicProfile.Width, PicProfile.Height);
                PicProfile.Region = new Region(path);
            }
        }

        private void btnTable_Click(object sender, EventArgs e)
        {
            if (isTableExpanded)
            {
                if (btnTableGroup != null) btnTableGroup.Visible = false;
                if (btnTableName != null) btnTableName.Visible = false;
                isTableExpanded = false;
                if (tableChevron != null) tableChevron.IconChar = IconChar.ChevronRight;

                Inventory.Height = RowH * 4;
            }
            else
            {
                if (btnTableGroup != null) btnTableGroup.Visible = true;
                if (btnTableName != null) btnTableName.Visible = true;
                isTableExpanded = true;
                if (tableChevron != null) tableChevron.IconChar = IconChar.ChevronDown;

                Inventory.Height = RowH * 6;      // header + 3 subs + 2 table subs
            }
        }

        private void PictureLogo_Click(object sender, EventArgs e)
        {
            SetSidebarCollapsed(!sidebarCollapsed);
        }

        private void btndashboard_Click(object sender, EventArgs e)
        {
            ShowDashboard();
        }

        // ------------------------------------------------------------------
        //  Dashboard UI (built once)
        // ------------------------------------------------------------------

        private void InitializeDashboardView()
        {
            // Host panel: stays for the whole life of the form
            mainContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 244, 248)
            };
            this.Controls.Add(mainContentPanel);
            mainContentPanel.BringToFront();

            // Dashboard content panel: kept in memory and re-added when needed
            dashboardView = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 244, 248),
                AutoScroll = true
            };

            TableLayoutPanel mainGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(15)
            };
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            dashboardView.Controls.Add(mainGrid);

            Label lblTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Fill
            };
            mainGrid.Controls.Add(lblTitle, 0, 0);

            // KPI Grid (Initial 0.00 State)
            TableLayoutPanel kpiGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 10)
            };
            for (int i = 0; i < 3; i++)
                kpiGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));

            kpiGrid.Controls.Add(CreateKpiCard("0.00 USD", "SALES TODAY", Color.FromArgb(0, 122, 204), out lblKpiSalesToday), 0, 0);
            kpiGrid.Controls.Add(CreateKpiCard("0.00 USD", "AVERAGE SALES AMOUNT", Color.FromArgb(0, 122, 204), out lblKpiAvgSaleAmount), 1, 0);
            kpiGrid.Controls.Add(CreateKpiCard("0.00", "AVERAGE SALES QTY", Color.FromArgb(0, 122, 204), out lblKpiAvgSaleQty), 2, 0);

            mainGrid.Controls.Add(kpiGrid, 0, 1);

            // Charts Grid
            TableLayoutPanel chartsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            chartsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            chartsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            chartsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            pieChart = CreateEmptyPieChart();
            barChart = CreateEmptyBarChart();
            chartsGrid.Controls.Add(pieChart, 0, 0);
            chartsGrid.Controls.Add(barChart, 1, 0);

            mainGrid.Controls.Add(chartsGrid, 0, 2);

            // Finally show it
            ShowDashboard();
        }

        private void RefreshDashboardData()
        {
            try
            {
                // 1. Sales Today
                string sqlSalesToday = @"
SELECT ISNULL(SUM(GrandTotal), 0) 
FROM dbo.SALE_ORDER 
WHERE Status = 'Paid' AND CAST(PostingDate AS date) = CAST(SYSDATETIME() AS date);";
                object? resSales = DbHelper.ExecuteScalar(sqlSalesToday);
                decimal salesKHR = resSales != null && resSales != DBNull.Value ? Convert.ToDecimal(resSales) : 0m;
                decimal salesUSD = salesKHR / 4000m;
                if (lblKpiSalesToday != null) lblKpiSalesToday.Text = $"{salesUSD:N2} USD";

                // 2. Average Sales Amount
                string sqlAvgAmount = @"
SELECT ISNULL(AVG(GrandTotal), 0) 
FROM dbo.SALE_ORDER 
WHERE Status = 'Paid' AND CAST(PostingDate AS date) = CAST(SYSDATETIME() AS date);";
                object? resAvgAmt = DbHelper.ExecuteScalar(sqlAvgAmount);
                decimal avgAmt = resAvgAmt != null && resAvgAmt != DBNull.Value ? Convert.ToDecimal(resAvgAmt) : 0m;
                decimal avgAmtUSD = avgAmt / 4000m;
                if (lblKpiAvgSaleAmount != null) lblKpiAvgSaleAmount.Text = $"{avgAmtUSD:N2} USD";

                // 3. Average Sales Qty
                string sqlAvgQty = @"
SELECT ISNULL(AVG(oi.Qty), 0)
FROM dbo.SALE_ORDER_ITEM oi
JOIN dbo.SALE_ORDER o ON oi.OrderID = o.OrderID
WHERE o.Status = 'Paid' AND CAST(o.PostingDate AS date) = CAST(SYSDATETIME() AS date);";
                object? resAvgQty = DbHelper.ExecuteScalar(sqlAvgQty);
                decimal avgQty = resAvgQty != null && resAvgQty != DBNull.Value ? Convert.ToDecimal(resAvgQty) : 0m;
                if (lblKpiAvgSaleQty != null) lblKpiAvgSaleQty.Text = $"{avgQty:N2}";

                // 4. Pie Chart: vw_SaleByGroup
                if (pieChart != null && pieChart.Series.Count > 0)
                {
                    var series = pieChart.Series[0];
                    series.Points.Clear();

                    DataTable dtPie = DbHelper.ExecuteQuery("SELECT GroupName, Amount FROM dbo.vw_SaleByGroup;");
                    if (dtPie.Rows.Count > 0)
                    {
                        series.IsValueShownAsLabel = true;
                        foreach (DataRow r in dtPie.Rows)
                        {
                            string gName = r["GroupName"]?.ToString() ?? "";
                            decimal amt = r["Amount"] != DBNull.Value ? Convert.ToDecimal(r["Amount"]) : 0m;
                            decimal amtUSD = amt / 4000m;
                            series.Points.AddXY(gName, amtUSD);
                        }
                    }
                    else
                    {
                        series.IsValueShownAsLabel = false;
                        int idx = series.Points.AddXY("No Data Available", 100);
                        series.Points[idx].Color = Color.LightGray;
                    }
                }

                // 5. Bar Chart: Monthly sales performance for current year
                if (barChart != null && barChart.Series.Count > 0)
                {
                    var series = barChart.Series[0];
                    series.Points.Clear();

                    string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                    decimal[] monthValues = new decimal[12];

                    string sqlMonthly = @"
SELECT MONTH(PostingDate) AS [Month], ISNULL(SUM(GrandTotal), 0) AS TotalAmount
FROM dbo.SALE_ORDER
WHERE Status = 'Paid' AND YEAR(PostingDate) = YEAR(GETDATE())
GROUP BY MONTH(PostingDate);";

                    DataTable dtMonthly = DbHelper.ExecuteQuery(sqlMonthly);
                    foreach (DataRow r in dtMonthly.Rows)
                    {
                        int m = Convert.ToInt32(r["Month"]);
                        if (m >= 1 && m <= 12)
                        {
                            decimal tot = Convert.ToDecimal(r["TotalAmount"]);
                            monthValues[m - 1] = tot / 4000m;
                        }
                    }

                    for (int i = 0; i < months.Length; i++)
                    {
                        int pIndex = series.Points.AddXY(months[i], monthValues[i]);
                        series.Points[pIndex].AxisLabel = months[i];
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing dashboard: {ex.Message}");
            }
        }

        private Guna2Panel CreateKpiCard(string value, string title, Color valueColor, out Label outLblVal)
        {
            Guna2Panel card = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                FillColor = Color.White,
                BorderRadius = 6,
                Margin = new Padding(5)
            };
            card.ShadowDecoration.Enabled = true;
            card.ShadowDecoration.Depth = 5;

            Label lblVal = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = valueColor,
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.BottomCenter
            };

            Label lblSub = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.Gray,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter
            };

            card.Controls.Add(lblSub);
            card.Controls.Add(lblVal);
            outLblVal = lblVal;
            return card;
        }

        private Chart CreateEmptyPieChart()
        {
            Chart chart = CreateBaseChart("SALE BY PRODUCT GROUP (USD)");

            Series series = new Series
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = false // Hide values on slices when empty
            };

            // Single placeholder slice to show a clean grey "No Data Available" pie visual
            int pIndex = series.Points.AddXY("No Data Available", 100);
            series.Points[pIndex].Color = Color.LightGray;

            chart.Series.Add(series);
            return chart;
        }

        private Chart CreateEmptyBarChart()
        {
            Chart chart = CreateBaseChart("MONTHLY SALES PERFORMANCE (USD)");
            chart.Legends.Clear();

            Series series = new Series
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(0, 150, 236),
                IsValueShownAsLabel = false // Hide zero labels over empty bars
            };

            string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            // Render empty month ticks cleanly across X-axis without label clutter
            for (int i = 0; i < months.Length; i++)
            {
                int pIndex = series.Points.AddXY(months[i], 0);
                series.Points[pIndex].AxisLabel = months[i];
            }

            chart.Series.Add(series);

            ChartArea area = chart.ChartAreas[0];
            area.AxisX.Interval = 1;
            area.AxisX.MajorGrid.Enabled = false;

            // Set Y-Axis range for clean empty grid appearance
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 1000;

            return chart;
        }

        private Chart CreateBaseChart(string titleText)
        {
            Chart chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(5)
            };

            Title title = new Title
            {
                Text = titleText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DimGray
            };
            chart.Titles.Add(title);

            ChartArea area = new ChartArea { BackColor = Color.White };
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas.Add(area);

            Legend legend = new Legend
            {
                Docking = Docking.Right,
                Font = new Font("Segoe UI", 8)
            };
            chart.Legends.Add(legend);

            return chart;
        }

        private void SetupHeaderUserProfile()
        {
            if (header == null) return;

            header.Controls.Clear();
            header.Height = HeaderH;

            label1 = new Label
            {
                Text = "Restaurant Management System",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            header.Controls.Add(label1);

            FlowLayoutPanel pnlUser = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, (HeaderH - 52) / 2, 20, 0)
            };

            int nameWidth = TextRenderer.MeasureText(
                string.IsNullOrEmpty(UserSession.FullName) ? UserSession.Username : UserSession.FullName,
                new Font("Segoe UI", 10.5F, FontStyle.Bold)).Width;
            int textWidth = Math.Max(200, nameWidth + 24);

            Panel pnlText = new Panel
            {
                Width = textWidth,
                Height = 52,
                BackColor = Color.Transparent
            };

            lblUserName = new Label
            {
                Text = string.IsNullOrEmpty(UserSession.FullName) ? UserSession.Username : UserSession.FullName,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.MiddleRight
            };

            lblUserRole = new Label
            {
                Text = $"Role: {UserSession.Role.ToUpper()}",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(225, 245, 255),
                Dock = DockStyle.Bottom,
                Height = 22,
                TextAlign = ContentAlignment.TopRight
            };

            pnlText.Controls.Add(lblUserRole);
            pnlText.Controls.Add(lblUserName);

            if (PicProfile == null)
            {
                PicProfile = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            }
            PicProfile.Size = new Size(46, 46);
            PicProfile.Margin = new Padding(8, 3, 12, 0);
            PicProfile.Cursor = Cursors.Hand;
            PicProfile.SizeMode = PictureBoxSizeMode.Zoom;
            PicProfile.BackColor = Color.Transparent;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, PicProfile.Width, PicProfile.Height);
                PicProfile.Region = new Region(path);
            }

            // Load and display user profile avatar
            string? profileImgPath = UserSession.ProfileImagePath;
            if (string.IsNullOrEmpty(profileImgPath) && UserSession.UserID > 0)
            {
                try
                {
                    DataTable dtUser = DbHelper.ExecuteQuery(
                        "SELECT ProfileImagePath FROM dbo.APP_USER WHERE UserID = @UID",
                        new SqlParameter("@UID", UserSession.UserID));
                    if (dtUser.Rows.Count > 0)
                    {
                        profileImgPath = dtUser.Rows[0]["ProfileImagePath"]?.ToString();
                        UserSession.ProfileImagePath = profileImgPath;
                    }
                }
                catch { }
            }

            var (avatarImg, originalImg) = LoadUserAvatar(profileImgPath, 128);
            if (avatarImg != null)
            {
                PicProfile.Image = avatarImg;
                PicProfile.Tag = originalImg;
            }
            else
            {
                PicProfile.Image = CreateDefaultAvatar(46);
            }

            btnLogout = new Guna2Button
            {
                Text = "Logout",
                Size = new Size(100, 36),
                BorderRadius = 6,
                FillColor = Color.FromArgb(239, 83, 80),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center,
                Margin = new Padding(6, 8, 8, 0),
                Cursor = Cursors.Hand
            };
            btnLogout.HoverState.FillColor = Color.FromArgb(211, 47, 47);
            btnLogout.Click += (s, e) =>
            {
                var confirm = MessageBox.Show("Are you sure you want to log out?", "Logout Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    UserSession.Logout();
                    this.Close();
                }
            };

            btnExitApp = new Guna2Button
            {
                Text = "✕",
                Size = new Size(40, 36),
                BorderRadius = 6,
                FillColor = Color.FromArgb(55, 71, 79),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Margin = new Padding(4, 8, 8, 0),
                Cursor = Cursors.Hand
            };
            btnExitApp.HoverState.FillColor = Color.FromArgb(198, 40, 40);
            btnExitApp.Click += (s, e) =>
            {
                var confirm = MessageBox.Show("Are you sure you want to exit the system?", "Exit Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    Application.Exit();
                }
            };

            pnlUser.Controls.Add(pnlText);
            pnlUser.Controls.Add(PicProfile);
            pnlUser.Controls.Add(btnLogout);
            pnlUser.Controls.Add(btnExitApp);

            header.Controls.Add(pnlUser);

            void CenterTitle()
            {
                if (label1 == null || header == null) return;
                int titleW = label1.PreferredWidth;
                int idealX = (header.ClientSize.Width - titleW) / 2;
                int idealY = (header.ClientSize.Height - label1.PreferredHeight) / 2;

                if (pnlUser != null && pnlUser.Visible && pnlUser.Left > 0 && idealX + titleW > pnlUser.Left - 10)
                {
                    idealX = Math.Max(10, pnlUser.Left - titleW - 10);
                }

                label1.Location = new Point(Math.Max(10, idealX), Math.Max(0, idealY));
                label1.BringToFront();
            }

            header.Resize += (s, e) => CenterTitle();
            pnlUser.Resize += (s, e) => CenterTitle();
            CenterTitle();
        }

        // ------------------------------------------------------------------
        //  Misc handlers
        // ------------------------------------------------------------------

        private void sidebar_Paint(object sender, PaintEventArgs e) { }
        private void Inventory_Paint(object sender, PaintEventArgs e) { }
        private void iconButton1_Click(object sender, EventArgs e) { }

        private void btnItemGroup_Click(object sender, EventArgs e)
        {
            LoadView(new ItemGroupList());
        }

        private void btnTableName_Click(object sender, EventArgs e)
        {
            LoadView(new Resturant_Management.Table.TableList());
        }

        private void btnTableGroup_Click(object sender, EventArgs e)
        {
            LoadView(new Resturant_Management.Table.GroupTable());
        }

        private void PicProfile_Click(object sender, EventArgs e)
        {
            Image? imgToDisplay = PicProfile.Tag as Image ?? PicProfile.Image;
            if (imgToDisplay != null)
            {
                Form imageViewer = new Form
                {
                    Text = $"{UserSession.FullName} - Profile Picture",
                    Size = new Size(460, 560),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    BackColor = Color.White
                };

                PictureBox pb = new PictureBox
                {
                    Image = imgToDisplay,
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Padding = new Padding(10)
                };

                imageViewer.Controls.Add(pb);
                imageViewer.ShowDialog(this);
            }
        }

        private (Image? avatar, Image? original) LoadUserAvatar(string? path, int size)
        {
            if (string.IsNullOrWhiteSpace(path)) return (null, null);

            try
            {
                string resolvedPath = "";
                if (System.IO.File.Exists(path))
                {
                    resolvedPath = path;
                }
                else
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string combined = System.IO.Path.Combine(baseDir, path);
                    if (System.IO.File.Exists(combined))
                    {
                        resolvedPath = combined;
                    }
                    else
                    {
                        string fileName = System.IO.Path.GetFileName(path);
                        string resPath = System.IO.Path.Combine(baseDir, "Resources", fileName);
                        if (System.IO.File.Exists(resPath))
                        {
                            resolvedPath = resPath;
                        }
                        else
                        {
                            string projectRes = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, @"..\..\..\Resources", fileName));
                            if (System.IO.File.Exists(projectRes))
                            {
                                resolvedPath = projectRes;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(resolvedPath) && System.IO.File.Exists(resolvedPath))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(resolvedPath);
                    using var ms = new System.IO.MemoryStream(bytes);
                    using Image original = Image.FromStream(ms);
                    Bitmap originalCopy = new Bitmap(original);
                    Bitmap avatar = CropToCircleAvatar(originalCopy, size);
                    return (avatar, originalCopy);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load profile avatar: {ex.Message}");
            }

            return (null, null);
        }

        private static Bitmap CropToCircleAvatar(Image src, int size)
        {
            Bitmap result = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                int srcDim = Math.Min(src.Width, src.Height);
                int srcX = (src.Width - srcDim) / 2;
                int srcY = 0;
                if (src.Height > src.Width)
                {
                    // Framing the face nicely in the upper portion
                    srcY = Math.Max(0, (src.Height - srcDim) / 5);
                }
                else
                {
                    srcY = (src.Height - srcDim) / 2;
                }

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, size, size);
                    g.SetClip(path);
                    g.DrawImage(src, new Rectangle(0, 0, size, size), srcX, srcY, srcDim, srcDim, GraphicsUnit.Pixel);
                }
            }
            return result;
        }

        private static Bitmap CreateDefaultAvatar(int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(220, 230, 242)))
                    g.FillEllipse(brush, 0, 0, size, size);

                string initial = !string.IsNullOrEmpty(UserSession.FullName)
                    ? UserSession.FullName.Substring(0, 1).ToUpper()
                    : (!string.IsNullOrEmpty(UserSession.Username) ? UserSession.Username.Substring(0, 1).ToUpper() : "U");

                using var font = new Font("Segoe UI", size * 0.42f, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.FromArgb(41, 128, 185));
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(initial, font, textBrush, new RectangleF(0, 0, size, size), sf);
            }
            return bmp;
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            LoadView(new Resturant_Management.Report.ReportContainer());
        }

        private void btnItemMasterData_Click(object sender, EventArgs e)
        {
            LoadView(new Resturant_Management.Inventory.Itemlist());
        }

        private void btnPOS_Click(object sender, EventArgs e)
        {
            LoadView(new TableCards());
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
