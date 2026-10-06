using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Resturant_Management.Data;

namespace Resturant_Management.POS
{
    public partial class TableCards : UserControl
    {
        private FlowLayoutPanel? _flowTables;

        private bool _isInitialized = false;

        public TableCards()
        {
            InitializeComponent();
            guna2Panel1.AutoScroll = true;
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            this.Load += (s, e) => InitRuntime();
            this.VisibleChanged += (s, e) =>
            {
                if (this.Visible && !DesignTimeHelper.IsInDesignMode(this))
                {
                    if (!_isInitialized)
                        InitRuntime();
                    else
                        LoadTablesFromDatabase();
                }
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignTimeHelper.IsInDesignMode(this) && !_isInitialized)
            {
                InitRuntime();
            }
        }

        private void InitRuntime()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            LoadTablesFromDatabase();
        }

        private void LoadTablesFromDatabase()
        {
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            try
            {
                string sql = @"
SELECT 
    t.TableID, 
    t.TableCode, 
    t.TableName, 
    ISNULL(t.Status, 'Available') AS TableStatus,
    t.ImagePath,
    ISNULL(tg.GroupName, 'Main Area') AS GroupName,
    o.OrderID AS ActiveOrderID,
    o.OrderNo AS ActiveOrderNo,
    ISNULL(o.GrandTotal, 0) AS ActiveGrandTotal
FROM dbo.DINING_TABLE t
LEFT JOIN dbo.TABLE_GROUP tg ON t.TableGroupID = tg.TableGroupID
OUTER APPLY (
    SELECT TOP 1 OrderID, OrderNo, GrandTotal 
    FROM dbo.SALE_ORDER 
    WHERE TableID = t.TableID AND Status IN ('Open', 'Sent', 'Billed')
    ORDER BY OrderID DESC
) o
WHERE t.IsActive = 1
ORDER BY t.TableID;";

                DataTable dt = DbHelper.ExecuteQuery(sql);

                Control[] designerCards = new Control[] { panelTable, panel, guna2Panel3, guna2Panel6, guna2Panel12 };
                Label[] labels = new Label[] { lbtableNum, label3, label1, label2, label4 };
                PictureBox[] pictures = new PictureBox[] { Pictable, guna2PictureBox1, guna2PictureBox2, guna2PictureBox3, guna2PictureBox4 };
                Control[] footers = new Control[] { footertable, guna2Panel10, guna2Panel4, guna2Panel7, guna2Panel13 };

                // Clean up any dynamically spawned extra cards from previous refresh
                if (_flowTables != null)
                {
                    guna2Panel1.Controls.Remove(_flowTables);
                    _flowTables.Dispose();
                    _flowTables = null;
                }

                // If table count is <= 5, bind directly to the 5 designer cards
                for (int i = 0; i < designerCards.Length; i++)
                {
                    if (designerCards[i] == null) continue;

                    if (i < dt.Rows.Count)
                    {
                        designerCards[i].Visible = true;
                        DataRow row = dt.Rows[i];
                        int tableId = Convert.ToInt32(row["TableID"]);
                        string tableName = row["TableName"]?.ToString() ?? $"Table-{i + 1}";
                        int capacity = Convert.ToInt32(row["Capacity"]);
                        string rawStatus = row["TableStatus"]?.ToString() ?? "Available";
                        bool hasOpen = row["ActiveOrderID"] != DBNull.Value;
                        decimal activeTotal = Convert.ToDecimal(row["ActiveGrandTotal"]);
                        string? activeOrderNo = row["ActiveOrderNo"]?.ToString();

                        string statusText;
                        Color footerBg;
                        Color footerFg;

                        if (hasOpen)
                        {
                            decimal totalUSD = activeTotal / 4000m;
                            statusText = $"Occupied • #{activeOrderNo} (${totalUSD:N2})";
                            footerBg = Color.FromArgb(255, 235, 238);
                            footerFg = Color.FromArgb(198, 40, 40);
                        }
                        else if (rawStatus.Equals("Reserved", StringComparison.OrdinalIgnoreCase))
                        {
                            statusText = "Reserved";
                            footerBg = Color.FromArgb(227, 242, 253);
                            footerFg = Color.FromArgb(21, 101, 192);
                        }
                        else if (rawStatus.Equals("Cleaning", StringComparison.OrdinalIgnoreCase))
                        {
                            statusText = "Cleaning";
                            footerBg = Color.FromArgb(255, 248, 225);
                            footerFg = Color.FromArgb(239, 108, 0);
                        }
                        else
                        {
                            statusText = "Available";
                            footerBg = Color.FromArgb(232, 245, 233);
                            footerFg = Color.FromArgb(46, 125, 50);
                        }

                        if (labels[i] != null)
                        {
                            labels[i].Text = tableName;
                        }

                        string? imgPath = row["ImagePath"]?.ToString();
                        if (!string.IsNullOrEmpty(imgPath) && System.IO.File.Exists(imgPath) && pictures[i] != null)
                        {
                            try { pictures[i].Image = Image.FromFile(imgPath); } catch { }
                        }

                        if (footers[i] != null)
                        {
                            footers[i].BackColor = footerBg;
                            Label? lblStatus = footers[i].Controls.OfType<Label>().FirstOrDefault();
                            if (lblStatus == null)
                            {
                                lblStatus = new Label
                                {
                                    Dock = DockStyle.Fill,
                                    TextAlign = ContentAlignment.MiddleCenter,
                                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
                                };
                                footers[i].Controls.Add(lblStatus);
                            }
                            lblStatus.Text = statusText;
                            lblStatus.ForeColor = footerFg;
                        }

                        BindCardClick(designerCards[i], tableId, tableName);
                    }
                    else
                    {
                        designerCards[i].Visible = false;
                    }
                }

                // If more than 5 tables exist, create dynamic cards in a flow panel
                if (dt.Rows.Count > 5)
                {
                    // Hide static cards and use flow layout for all
                    foreach (var c in designerCards) if (c != null) c.Visible = false;

                    _flowTables = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        AutoScroll = true,
                        BackColor = Color.Transparent,
                        Padding = new Padding(10)
                    };
                    guna2Panel1.Controls.Add(_flowTables);
                    _flowTables.BringToFront();

                    foreach (DataRow row in dt.Rows)
                    {
                        int tableId = Convert.ToInt32(row["TableID"]);
                        string tableName = row["TableName"]?.ToString() ?? "Table";
                        int capacity = Convert.ToInt32(row["Capacity"]);
                        string rawStatus = row["TableStatus"]?.ToString() ?? "Available";
                        bool hasOpen = row["ActiveOrderID"] != DBNull.Value;
                        decimal activeTotal = Convert.ToDecimal(row["ActiveGrandTotal"]);
                        string? activeOrderNo = row["ActiveOrderNo"]?.ToString();
                        string? imgPath = row["ImagePath"]?.ToString();

                        string statusText;
                        Color footerBg;
                        Color footerFg;

                        if (hasOpen)
                        {
                            decimal totalUSD = activeTotal / 4000m;
                            statusText = $"Occupied • #{activeOrderNo} (${totalUSD:N2})";
                            footerBg = Color.FromArgb(255, 235, 238);
                            footerFg = Color.FromArgb(198, 40, 40);
                        }
                        else if (rawStatus.Equals("Reserved", StringComparison.OrdinalIgnoreCase))
                        {
                            statusText = "Reserved";
                            footerBg = Color.FromArgb(227, 242, 253);
                            footerFg = Color.FromArgb(21, 101, 192);
                        }
                        else if (rawStatus.Equals("Cleaning", StringComparison.OrdinalIgnoreCase))
                        {
                            statusText = "Cleaning";
                            footerBg = Color.FromArgb(255, 248, 225);
                            footerFg = Color.FromArgb(239, 108, 0);
                        }
                        else
                        {
                            statusText = "Available";
                            footerBg = Color.FromArgb(232, 245, 233);
                            footerFg = Color.FromArgb(46, 125, 50);
                        }

                        Guna2Panel card = new Guna2Panel
                        {
                            Size = new Size(240, 185),
                            Margin = new Padding(12),
                            BorderRadius = 10,
                            BorderColor = Color.FromArgb(215, 220, 228),
                            BorderThickness = 1,
                            FillColor = Color.White,
                            Cursor = Cursors.Hand
                        };
                        card.ShadowDecoration.Enabled = true;
                        card.ShadowDecoration.Depth = 5;

                        Label lblHead = new Label
                        {
                            Text = tableName,
                            Dock = DockStyle.Top,
                            Height = 35,
                            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                            TextAlign = ContentAlignment.MiddleCenter,
                            ForeColor = Color.FromArgb(40, 40, 40)
                        };

                        PictureBox pb = new PictureBox
                        {
                            Dock = DockStyle.Fill,
                            SizeMode = PictureBoxSizeMode.Zoom
                        };
                        if (!string.IsNullOrEmpty(imgPath) && System.IO.File.Exists(imgPath))
                        {
                            try { pb.Image = Image.FromFile(imgPath); } catch { }
                        }
                        else if (Pictable?.Image != null)
                        {
                            pb.Image = Pictable.Image;
                        }

                        Panel foot = new Panel
                        {
                            Dock = DockStyle.Bottom,
                            Height = 34,
                            BackColor = footerBg
                        };

                        Label lblFoot = new Label
                        {
                            Text = statusText,
                            Dock = DockStyle.Fill,
                            TextAlign = ContentAlignment.MiddleCenter,
                            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                            ForeColor = footerFg
                        };
                        foot.Controls.Add(lblFoot);

                        card.Controls.Add(pb);
                        card.Controls.Add(foot);
                        card.Controls.Add(lblHead);

                        BindCardClick(card, tableId, tableName);
                        _flowTables.Controls.Add(card);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tables: {ex.Message}");
            }
        }

        private void BindCardClick(Control ctrl, int tableId, string tableName)
        {
            ctrl.Cursor = Cursors.Hand;
            ctrl.Click += (s, e) => OpenPOSSale(tableId, tableName);
            foreach (Control child in ctrl.Controls)
            {
                BindCardClick(child, tableId, tableName);
            }
        }

        private void OpenPOSSale(int tableId, string tableName)
        {
            Control? parentPanel = this.Parent;
            if (parentPanel == null) return;

            parentPanel.Controls.Clear();

            TableLayoutPanel posContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };

            posContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            posContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            posContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            POSSale posSaleControl = new POSSale(tableId, tableName) { Dock = DockStyle.Fill };
            ProductList productListControl = new ProductList { Dock = DockStyle.Fill };

            posContainer.Controls.Add(posSaleControl, 0, 0);
            posContainer.Controls.Add(productListControl, 1, 0);

            parentPanel.Controls.Add(posContainer);
            posContainer.BringToFront();
        }

        private void guna2PictureBox1_Click(object? sender, EventArgs e) => OpenPOSSale(1, "Table-1");
        private void label1_Click(object? sender, EventArgs e) => OpenPOSSale(2, "Table-2");
        private void Pictable_Click(object? sender, EventArgs e) => OpenPOSSale(1, "Table-1");
        private void guna2Panel1_Paint(object? sender, PaintEventArgs e) { }
        private void guna2Panel3_Paint(object? sender, PaintEventArgs e) { }
        private void guna2Panel15_Paint(object? sender, PaintEventArgs e) { }
        private void guna2Panel21_Paint(object? sender, PaintEventArgs e) { }
    }
}