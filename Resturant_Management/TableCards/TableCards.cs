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

        // Polls the database so cards follow changes made elsewhere (new/renamed tables, orders, status)
        private readonly System.Windows.Forms.Timer _refreshTimer = new System.Windows.Forms.Timer { Interval = 3000 };
        private string? _lastSignature;
        private string? _lastError;

        public TableCards()
        {
            InitializeComponent();
            guna2Panel1.AutoScroll = true;
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            _refreshTimer.Tick += (s, e) =>
            {
                if (this.Visible && this.Parent != null && this.IsHandleCreated)
                    LoadTablesFromDatabase();
            };
            this.Disposed += (s, e) =>
            {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
            };

            this.Load += (s, e) => InitRuntime();
            this.VisibleChanged += (s, e) =>
            {
                if (this.Visible && !DesignTimeHelper.IsInDesignMode(this))
                {
                    if (!_isInitialized)
                        InitRuntime();
                    else
                    {
                        RefreshTables();
                        _refreshTimer.Start();
                    }
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
            _refreshTimer.Start();
        }

        /// <summary>Re-reads tables from the database immediately (e.g. after saving a table).</summary>
        public void RefreshTables()
        {
            _lastSignature = null;
            LoadTablesFromDatabase();
        }

        private static string BuildSignature(DataTable dt)
        {
            var sb = new System.Text.StringBuilder();
            foreach (DataRow row in dt.Rows)
            {
                foreach (var value in row.ItemArray)
                    sb.Append(value?.ToString()).Append('\u001f');
                sb.Append('\u001e');
            }
            return sb.ToString();
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
    ISNULL(t.Capacity, 0) AS Capacity,
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

                // Skip redrawing when nothing changed, to avoid flicker on each poll
                string signature = BuildSignature(dt);
                if (signature == _lastSignature) return;
                _lastSignature = signature;

                Control[] designerCards = new Control[] { panelTable, panel, guna2Panel3, guna2Panel6, guna2Panel12 };
                Control[] headers = new Control[] { headertable, guna2Panel11, guna2Panel5, guna2Panel8, guna2Panel14 };
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
                        string tableName = GetTableName(row, $"Table-{i + 1}");
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

                        if (headers[i] != null)
                        {
                            // Use the header's label, creating one if the designer has none
                            Label? lblName = headers[i].Controls.OfType<Label>().FirstOrDefault();
                            if (lblName == null)
                            {
                                lblName = new Label
                                {
                                    Font = new Font("Segoe UI", 10.2F, FontStyle.Bold),
                                    ForeColor = Color.FromArgb(40, 40, 40),
                                    BackColor = Color.Transparent
                                };
                                headers[i].Controls.Add(lblName);
                            }
                            // Fill the header and center so names of any length stay centered
                            lblName.AutoSize = false;
                            lblName.Dock = DockStyle.Fill;
                            lblName.TextAlign = ContentAlignment.MiddleCenter;
                            lblName.Text = tableName;
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
                        string tableName = GetTableName(row, "Table");
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
                // Show each distinct error once, not on every refresh tick
                if (ex.Message != _lastError)
                {
                    _lastError = ex.Message;
                    _lastSignature = null;
                    MessageBox.Show($"Could not load tables from the database:\n\n{ex.Message}",
                        "Table Cards", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            _lastError = null;
        }

        private static string GetTableName(DataRow row, string fallback)
        {
            string? name = row["TableName"] == DBNull.Value ? null : row["TableName"].ToString();
            if (!string.IsNullOrWhiteSpace(name)) return name.Trim();

            string? code = row["TableCode"] == DBNull.Value ? null : row["TableCode"].ToString();
            return string.IsNullOrWhiteSpace(code) ? fallback : code.Trim();
        }

        private void BindCardClick(Control ctrl, int tableId, string tableName)
        {
            // Store the current table on the control so refreshes don't stack click handlers
            bool alreadyBound = ctrl.Tag is (int, string);
            ctrl.Tag = (tableId, tableName);
            ctrl.Cursor = Cursors.Hand;
            if (!alreadyBound)
            {
                ctrl.Click += (s, e) =>
                {
                    if (((Control)s!).Tag is (int id, string name))
                        OpenPOSSale(id, name);
                };
            }
            foreach (Control child in ctrl.Controls)
            {
                BindCardClick(child, tableId, tableName);
            }
        }

        private void OpenPOSSale(int tableId, string tableName)
        {
            Control? parentPanel = this.Parent;
            if (parentPanel == null) return;

            // This view is being replaced, so stop polling the database
            _refreshTimer.Stop();
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

        private void guna2Panel1_Paint(object? sender, PaintEventArgs e) { }
        private void guna2Panel3_Paint(object? sender, PaintEventArgs e) { }
        private void guna2Panel15_Paint(object? sender, PaintEventArgs e) { }
        private void guna2Panel21_Paint(object? sender, PaintEventArgs e) { }

        private void headertable_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lbtableNum_Click(object sender, EventArgs e)
        {

        }

        private void Pictable_Click(object sender, EventArgs e)
        {

        }
    }
}