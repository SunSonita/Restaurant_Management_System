using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;
using Resturant_Management.Report;

namespace Resturant_Management.Inventory
{
    public class InventoryManagement : UserControl
    {
        private Panel pnlHeader = null!;
        private Panel pnlTabs = null!;
        private Panel pnlContainer = null!;

        private Guna2Button btnTabStock = null!;
        private Guna2Button btnTabMovements = null!;
        private Guna2Button btnTabAdjustment = null!;
        private Guna2Button btnTabSuppliers = null!;
        private Guna2Button btnBack = null!;

        // View panels
        private Panel pnlStockOverview = null!;
        private Panel pnlMovements = null!;
        private Panel pnlAdjustment = null!;
        private Panel pnlSuppliers = null!;

        // Grid controls
        private DataGridView dgvStock = null!;
        private DataGridView dgvMovements = null!;
        private DataGridView dgvSuppliers = null!;

        // Filter controls
        private Guna2TextBox txtStockSearch = null!;
        private Guna2TextBox txtMovementSearch = null!;
        private Guna2DateTimePicker dtpFrom = null!;
        private Guna2DateTimePicker dtpTo = null!;

        // Adjustment inputs
        private Guna2ComboBox cmbAdjItem = null!;
        private Guna2ComboBox cmbAdjType = null!;
        private Guna2TextBox txtAdjQty = null!;
        private Guna2TextBox txtAdjCost = null!;
        private Guna2TextBox txtAdjRemark = null!;
        private Label lblCurrentStockNotice = null!;

        public InventoryManagement()
        {
            InitializeComponentByCode();
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            LoadStockOverview();
        }

        private void InitializeComponentByCode()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(240, 244, 248);

            // 1. Top Header
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            Label lblTitle = new Label
            {
                Text = "Inventory & Stock Management",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(21, 119, 214),
                Dock = DockStyle.Left,
                AutoSize = true
            };

            btnBack = new Guna2Button
            {
                Text = "◀ Back to Items",
                Size = new Size(130, 40),
                Dock = DockStyle.Right,
                BorderRadius = 6,
                FillColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBack.Click += (s, e) =>
            {
                Control? parent = this.Parent;
                if (parent != null)
                {
                    parent.Controls.Clear();
                    Itemlist list = new Itemlist { Dock = DockStyle.Fill };
                    parent.Controls.Add(list);
                    list.BringToFront();
                }
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(btnBack);

            // 2. Tab Navigation Bar
            pnlTabs = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.FromArgb(230, 236, 242),
                Padding = new Padding(20, 4, 20, 4)
            };

            btnTabStock = CreateTabButton("📦 Current Stock & Alerts", 0);
            btnTabMovements = CreateTabButton("📋 Movement History", 1);
            btnTabAdjustment = CreateTabButton("⚡ Stock Adjustment", 2);
            btnTabSuppliers = CreateTabButton("🏢 Suppliers & Purchases", 3);

            pnlTabs.Controls.Add(btnTabSuppliers);
            pnlTabs.Controls.Add(btnTabAdjustment);
            pnlTabs.Controls.Add(btnTabMovements);
            pnlTabs.Controls.Add(btnTabStock);

            // 3. Central Content Container
            pnlContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            BuildStockOverviewPanel();
            BuildMovementsPanel();
            BuildAdjustmentPanel();
            BuildSuppliersPanel();

            this.Controls.Add(pnlContainer);
            this.Controls.Add(pnlTabs);
            this.Controls.Add(pnlHeader);

            SwitchTab(0);
        }

        private Guna2Button CreateTabButton(string text, int index)
        {
            var btn = new Guna2Button
            {
                Text = text,
                Height = 40,
                Width = 200,
                BorderRadius = 6,
                FillColor = Color.Transparent,
                ForeColor = Color.FromArgb(70, 80, 95),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Dock = DockStyle.Left,
                Margin = new Padding(0, 0, 8, 0)
            };
            btn.Click += (s, e) => SwitchTab(index);
            return btn;
        }

        private void SwitchTab(int index)
        {
            Guna2Button[] tabs = { btnTabStock, btnTabMovements, btnTabAdjustment, btnTabSuppliers };
            for (int i = 0; i < tabs.Length; i++)
            {
                bool active = (i == index);
                tabs[i].FillColor = active ? Color.FromArgb(21, 119, 214) : Color.Transparent;
                tabs[i].ForeColor = active ? Color.White : Color.FromArgb(70, 80, 95);
            }

            pnlContainer.Controls.Clear();
            if (index == 0)
            {
                pnlContainer.Controls.Add(pnlStockOverview);
                pnlStockOverview.BringToFront();
                LoadStockOverview();
            }
            else if (index == 1)
            {
                pnlContainer.Controls.Add(pnlMovements);
                pnlMovements.BringToFront();
                LoadMovementHistory();
            }
            else if (index == 2)
            {
                pnlContainer.Controls.Add(pnlAdjustment);
                pnlAdjustment.BringToFront();
                LoadAdjustmentDropdown();
            }
            else
            {
                pnlContainer.Controls.Add(pnlSuppliers);
                pnlSuppliers.BringToFront();
                LoadSuppliers();
            }
        }

        // ================= Tab 1: Stock Overview =================
        private void BuildStockOverviewPanel()
        {
            pnlStockOverview = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
            txtStockSearch = new Guna2TextBox
            {
                PlaceholderText = "Search item code, name, category...",
                Size = new Size(280, 38),
                Location = new Point(0, 5),
                BorderRadius = 6
            };
            txtStockSearch.TextChanged += (s, e) => LoadStockOverview();

            Guna2Button btnExportStock = new Guna2Button
            {
                Text = "Export CSV",
                Size = new Size(110, 38),
                BorderRadius = 6,
                FillColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExportStock.Click += (s, e) => ReportExporter.ExportToCsv(dgvStock, "InventoryStockBalance");

            void RepositionTopBar()
            {
                btnExportStock.Location = new Point(topBar.Width - btnExportStock.Width - 10, 5);
            }
            topBar.Resize += (s, e) => RepositionTopBar();
            RepositionTopBar();

            topBar.Controls.Add(txtStockSearch);
            topBar.Controls.Add(btnExportStock);

            dgvStock = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 42 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvStock.Columns.Add("colCode", "Item Code");
            dgvStock.Columns.Add("colName", "Item Name");
            dgvStock.Columns.Add("colCategory", "Category");
            dgvStock.Columns.Add("colUom", "UoM");
            dgvStock.Columns.Add("colQtyOnHand", "Qty On Hand");
            dgvStock.Columns.Add("colTotalIn", "Total In");
            dgvStock.Columns.Add("colTotalOut", "Total Out");
            dgvStock.Columns.Add("colStatus", "Status");
            dgvStock.Columns.Add("colPriceUSD", "Price (USD)");

            pnlStockOverview.Controls.Add(dgvStock);
            pnlStockOverview.Controls.Add(topBar);
        }

        private void LoadStockOverview()
        {
            dgvStock.Rows.Clear();
            string search = txtStockSearch.Text.Trim();

            try
            {
                string sql = @"
SELECT 
    i.ItemID,
    i.ItemCode,
    i.ItemName,
    g.GroupName AS Category,
    u.UomName,
    ISNULL(s.QtyOnHand, 0) AS QtyOnHand,
    ISNULL(s.TotalIn, 0) AS TotalIn,
    ISNULL(s.TotalOut, 0) AS TotalOut,
    ISNULL(ip.PriceUSD, 0) AS PriceUSD
FROM dbo.ITEM i
LEFT JOIN dbo.ITEM_GROUP g ON i.GroupID = g.GroupID
LEFT JOIN dbo.UOM u ON i.UomID = u.UomID
LEFT JOIN dbo.vw_ItemStock s ON i.ItemID = s.ItemID
LEFT JOIN dbo.vw_ItemPrice ip ON i.ItemID = ip.ItemID
WHERE (@Search = '' OR i.ItemCode LIKE @Pattern OR i.ItemName LIKE @Pattern OR g.GroupName LIKE @Pattern)
ORDER BY s.QtyOnHand ASC, i.ItemCode ASC;";

                DataTable dt = DbHelper.ExecuteQuery(sql,
                    new SqlParameter("@Search", search),
                    new SqlParameter("@Pattern", $"%{search}%"));

                foreach (DataRow r in dt.Rows)
                {
                    decimal qty = Convert.ToDecimal(r["QtyOnHand"]);
                    string status = qty <= 0 ? "Out of Stock" : (qty <= 5 ? "Low Stock" : "In Stock");

                    int idx = dgvStock.Rows.Add(
                        r["ItemCode"]?.ToString(),
                        r["ItemName"]?.ToString(),
                        r["Category"]?.ToString(),
                        r["UomName"]?.ToString(),
                        qty.ToString("N2"),
                        Convert.ToDecimal(r["TotalIn"]).ToString("N2"),
                        Convert.ToDecimal(r["TotalOut"]).ToString("N2"),
                        status,
                        Convert.ToDecimal(r["PriceUSD"]).ToString("N2")
                    );

                    if (qty <= 0)
                        dgvStock.Rows[idx].DefaultCellStyle.ForeColor = Color.FromArgb(210, 40, 40);
                    else if (qty <= 5)
                        dgvStock.Rows[idx].DefaultCellStyle.ForeColor = Color.FromArgb(200, 120, 20);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading stock overview: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================= Tab 2: Movement History =================
        private void BuildMovementsPanel()
        {
            pnlMovements = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
            txtMovementSearch = new Guna2TextBox
            {
                PlaceholderText = "Search item, type, remark...",
                Size = new Size(250, 38),
                Location = new Point(0, 5),
                BorderRadius = 6
            };
            txtMovementSearch.TextChanged += (s, e) => LoadMovementHistory();

            dtpFrom = new Guna2DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(-30),
                Size = new Size(130, 38),
                Location = new Point(265, 5),
                BorderRadius = 6
            };
            dtpFrom.ValueChanged += (s, e) => LoadMovementHistory();

            dtpTo = new Guna2DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                Size = new Size(130, 38),
                Location = new Point(410, 5),
                BorderRadius = 6
            };
            dtpTo.ValueChanged += (s, e) => LoadMovementHistory();

            Guna2Button btnExportMovements = new Guna2Button
            {
                Text = "Export CSV",
                Size = new Size(110, 38),
                Location = new Point(555, 5),
                BorderRadius = 6,
                FillColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExportMovements.Click += (s, e) => ReportExporter.ExportToCsv(dgvMovements, "StockMovementHistory");

            topBar.Controls.Add(txtMovementSearch);
            topBar.Controls.Add(dtpFrom);
            topBar.Controls.Add(dtpTo);
            topBar.Controls.Add(btnExportMovements);

            dgvMovements = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 40 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvMovements.Columns.Add("colID", "ID");
            dgvMovements.Columns.Add("colDate", "Date & Time");
            dgvMovements.Columns.Add("colItemCode", "Item Code");
            dgvMovements.Columns.Add("colItemName", "Item Name");
            dgvMovements.Columns.Add("colType", "Movement Type");
            dgvMovements.Columns.Add("colQty", "Quantity");
            dgvMovements.Columns.Add("colCost", "Unit Cost");
            dgvMovements.Columns.Add("colRemark", "Remark / Reason");
            dgvMovements.Columns.Add("colUser", "Created By");

            pnlMovements.Controls.Add(dgvMovements);
            pnlMovements.Controls.Add(topBar);
        }

        private void LoadMovementHistory()
        {
            dgvMovements.Rows.Clear();
            string search = txtMovementSearch.Text.Trim();
            DateTime start = dtpFrom.Value.Date;
            DateTime end = dtpTo.Value.Date.AddDays(1).AddSeconds(-1);

            try
            {
                string sql = @"
SELECT 
    sm.MovementID,
    sm.CreatedAt,
    i.ItemCode,
    i.ItemName,
    sm.MovementType,
    sm.Qty,
    ISNULL(sm.UnitCost, 0) AS UnitCost,
    ISNULL(sm.Remark, '') AS Remark,
    ISNULL(u.FullName, u.Username) AS Creator
FROM dbo.STOCK_MOVEMENT sm
JOIN dbo.ITEM i ON sm.ItemID = i.ItemID
LEFT JOIN dbo.APP_USER u ON sm.CreatedBy = u.UserID
WHERE sm.CreatedAt >= @Start AND sm.CreatedAt <= @End
  AND (@Search = '' OR i.ItemCode LIKE @Pattern OR i.ItemName LIKE @Pattern OR sm.MovementType LIKE @Pattern OR sm.Remark LIKE @Pattern)
ORDER BY sm.MovementID DESC;";

                DataTable dt = DbHelper.ExecuteQuery(sql,
                    new SqlParameter("@Start", start),
                    new SqlParameter("@End", end),
                    new SqlParameter("@Search", search),
                    new SqlParameter("@Pattern", $"%{search}%"));

                foreach (DataRow r in dt.Rows)
                {
                    dgvMovements.Rows.Add(
                        r["MovementID"]?.ToString(),
                        Convert.ToDateTime(r["CreatedAt"]).ToString("yyyy-MM-dd HH:mm"),
                        r["ItemCode"]?.ToString(),
                        r["ItemName"]?.ToString(),
                        r["MovementType"]?.ToString(),
                        Convert.ToDecimal(r["Qty"]).ToString("N2"),
                        Convert.ToDecimal(r["UnitCost"]).ToString("N2"),
                        r["Remark"]?.ToString(),
                        r["Creator"]?.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading movements: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================= Tab 3: Stock Adjustment =================
        private void BuildAdjustmentPanel()
        {
            pnlAdjustment = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(30) };

            TableLayoutPanel formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 360,
                Width = 600,
                ColumnCount = 2,
                RowCount = 6
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            void AddRow(string labelText, Control inputControl, int row)
            {
                Label lbl = new Label
                {
                    Text = labelText,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                formLayout.Controls.Add(lbl, 0, row);
                formLayout.Controls.Add(inputControl, 1, row);
            }

            cmbAdjItem = new Guna2ComboBox { Dock = DockStyle.Fill, BorderRadius = 6 };
            cmbAdjItem.SelectedIndexChanged += (s, e) => UpdateCurrentStockNotice();

            cmbAdjType = new Guna2ComboBox { Dock = DockStyle.Fill, BorderRadius = 6 };
            cmbAdjType.Items.AddRange(new object[] { "Adjustment_In (Stock In)", "Adjustment_Out (Stock Out)", "Purchase" });
            cmbAdjType.SelectedIndex = 0;

            txtAdjQty = new Guna2TextBox { Dock = DockStyle.Fill, BorderRadius = 6, PlaceholderText = "Enter quantity (e.g. 10)" };
            txtAdjCost = new Guna2TextBox { Dock = DockStyle.Fill, BorderRadius = 6, PlaceholderText = "Unit cost in KHR (e.g. 5000)" };
            txtAdjRemark = new Guna2TextBox { Dock = DockStyle.Fill, BorderRadius = 6, PlaceholderText = "Reason (e.g. Damaged goods, Restock, Inventory count)" };

            lblCurrentStockNotice = new Label
            {
                Text = "Current Stock: 0.00",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(21, 119, 214),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            AddRow("Select Item:", cmbAdjItem, 0);
            AddRow("Current Stock Balance:", lblCurrentStockNotice, 1);
            AddRow("Adjustment Type:", cmbAdjType, 2);
            AddRow("Quantity:", txtAdjQty, 3);
            AddRow("Unit Cost (KHR):", txtAdjCost, 4);
            AddRow("Reason / Remark:", txtAdjRemark, 5);

            Guna2Button btnSaveAdjustment = new Guna2Button
            {
                Text = "✔ Record Adjustment",
                Size = new Size(200, 48),
                Location = new Point(210, 380),
                BorderRadius = 6,
                FillColor = Color.FromArgb(21, 119, 214),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSaveAdjustment.Click += BtnSaveAdjustment_Click;

            pnlAdjustment.Controls.Add(btnSaveAdjustment);
            pnlAdjustment.Controls.Add(formLayout);
        }

        private void LoadAdjustmentDropdown()
        {
            try
            {
                DataTable dt = DbHelper.ExecuteQuery("SELECT ItemID, ItemCode, ItemName FROM dbo.ITEM WHERE IsInactive = 0 ORDER BY ItemName");
                cmbAdjItem.DisplayMember = "DisplayName";
                cmbAdjItem.ValueMember = "ItemID";

                dt.Columns.Add("DisplayName", typeof(string), "ItemCode + ' - ' + ItemName");
                cmbAdjItem.DataSource = dt;
                UpdateCurrentStockNotice();
            }
            catch { }
        }

        private void UpdateCurrentStockNotice()
        {
            if (cmbAdjItem.SelectedValue != null && int.TryParse(cmbAdjItem.SelectedValue.ToString(), out int itemId))
            {
                object? res = DbHelper.ExecuteScalar("SELECT QtyOnHand FROM dbo.vw_ItemStock WHERE ItemID = @ID", new SqlParameter("@ID", itemId));
                decimal qty = (res != null && res != DBNull.Value) ? Convert.ToDecimal(res) : 0m;
                lblCurrentStockNotice.Text = $"Current Stock on Hand: {qty:N2}";
            }
        }

        private void BtnSaveAdjustment_Click(object? sender, EventArgs e)
        {
            if (cmbAdjItem.SelectedValue == null || !int.TryParse(cmbAdjItem.SelectedValue.ToString(), out int itemId))
            {
                MessageBox.Show("Please select an item.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtAdjQty.Text.Trim(), out decimal qty) || qty <= 0)
            {
                MessageBox.Show("Please enter a valid positive quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAdjQty.Focus();
                return;
            }

            decimal.TryParse(txtAdjCost.Text.Trim(), out decimal unitCost);
            string remark = txtAdjRemark.Text.Trim();
            if (string.IsNullOrEmpty(remark))
            {
                MessageBox.Show("Please enter a reason or remark for the adjustment.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAdjRemark.Focus();
                return;
            }

            string rawType = cmbAdjType.SelectedItem?.ToString() ?? "Adjustment_In";
            string movementType = rawType.Contains("Adjustment_Out") ? "Adjustment_Out" :
                                 (rawType.Contains("Purchase") ? "Purchase" : "Adjustment_In");

            // Prevent negative stock on Adjustment_Out
            if (movementType == "Adjustment_Out")
            {
                object? res = DbHelper.ExecuteScalar("SELECT QtyOnHand FROM dbo.vw_ItemStock WHERE ItemID = @ID", new SqlParameter("@ID", itemId));
                decimal currentStock = (res != null && res != DBNull.Value) ? Convert.ToDecimal(res) : 0m;
                if (currentStock < qty)
                {
                    DialogResult dr = MessageBox.Show($"Current stock is only {currentStock:N2}. Adjusting out {qty:N2} will result in negative stock balance.\nDo you want to proceed anyway?",
                        "Confirm Negative Stock", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dr != DialogResult.Yes) return;
                }
            }

            try
            {
                string sql = @"
INSERT INTO dbo.STOCK_MOVEMENT (ItemID, MovementType, Qty, UnitCost, Remark, CreatedBy, CreatedAt)
VALUES (@ItemID, @Type, @Qty, @Cost, @Remark, @User, SYSDATETIME());";

                int rows = DbHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@ItemID", itemId),
                    new SqlParameter("@Type", movementType),
                    new SqlParameter("@Qty", qty),
                    new SqlParameter("@Cost", unitCost),
                    new SqlParameter("@Remark", remark),
                    new SqlParameter("@User", UserSession.UserID > 0 ? UserSession.UserID : 1));

                if (rows > 0)
                {
                    MessageBox.Show("Stock movement recorded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtAdjQty.Clear();
                    txtAdjCost.Clear();
                    txtAdjRemark.Clear();
                    UpdateCurrentStockNotice();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error recording adjustment: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================= Tab 4: Suppliers =================
        private void BuildSuppliersPanel()
        {
            pnlSuppliers = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
            Guna2Button btnAddSupplier = new Guna2Button
            {
                Text = "➕ New Supplier",
                Size = new Size(140, 38),
                Location = new Point(0, 5),
                BorderRadius = 6,
                FillColor = Color.FromArgb(21, 119, 214),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddSupplier.Click += BtnAddSupplier_Click;
            topBar.Controls.Add(btnAddSupplier);

            dgvSuppliers = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 40 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvSuppliers.Columns.Add("colID", "ID");
            dgvSuppliers.Columns.Add("colCode", "Supplier Code");
            dgvSuppliers.Columns.Add("colName", "Supplier Name");
            dgvSuppliers.Columns.Add("colContact", "Contact Name");
            dgvSuppliers.Columns.Add("colPhone", "Phone");
            dgvSuppliers.Columns.Add("colEmail", "Email");
            dgvSuppliers.Columns.Add("colAddress", "Address");

            pnlSuppliers.Controls.Add(dgvSuppliers);
            pnlSuppliers.Controls.Add(topBar);
        }

        private void LoadSuppliers()
        {
            dgvSuppliers.Rows.Clear();
            try
            {
                DataTable dt = DbHelper.ExecuteQuery("SELECT SupplierID, SupplierCode, SupplierName, ContactName, Phone, Email, Address FROM dbo.SUPPLIER ORDER BY SupplierName");
                foreach (DataRow r in dt.Rows)
                {
                    dgvSuppliers.Rows.Add(
                        r["SupplierID"]?.ToString(),
                        r["SupplierCode"]?.ToString(),
                        r["SupplierName"]?.ToString(),
                        r["ContactName"]?.ToString(),
                        r["Phone"]?.ToString(),
                        r["Email"]?.ToString(),
                        r["Address"]?.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddSupplier_Click(object? sender, EventArgs e)
        {
            using (Form modal = new Form())
            {
                modal.Text = "Add New Supplier";
                modal.Size = new Size(420, 360);
                modal.StartPosition = FormStartPosition.CenterParent;
                modal.FormBorderStyle = FormBorderStyle.FixedDialog;
                modal.MaximizeBox = false;

                TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(15) };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

                Guna2TextBox txtCode = new Guna2TextBox { Dock = DockStyle.Fill, Text = $"SUP-{DateTime.Now:mmss}" };
                Guna2TextBox txtName = new Guna2TextBox { Dock = DockStyle.Fill, PlaceholderText = "Supplier name" };
                Guna2TextBox txtContact = new Guna2TextBox { Dock = DockStyle.Fill, PlaceholderText = "Contact person" };
                Guna2TextBox txtPhone = new Guna2TextBox { Dock = DockStyle.Fill, PlaceholderText = "Phone number" };
                Guna2TextBox txtAddress = new Guna2TextBox { Dock = DockStyle.Fill, PlaceholderText = "Address" };

                layout.Controls.Add(new Label { Text = "Code:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
                layout.Controls.Add(txtCode, 1, 0);
                layout.Controls.Add(new Label { Text = "Name:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
                layout.Controls.Add(txtName, 1, 1);
                layout.Controls.Add(new Label { Text = "Contact:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
                layout.Controls.Add(txtContact, 1, 2);
                layout.Controls.Add(new Label { Text = "Phone:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
                layout.Controls.Add(txtPhone, 1, 3);
                layout.Controls.Add(new Label { Text = "Address:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
                layout.Controls.Add(txtAddress, 1, 4);

                Guna2Button btnSave = new Guna2Button
                {
                    Text = "Save Supplier",
                    Dock = DockStyle.Bottom,
                    Height = 44,
                    FillColor = Color.FromArgb(21, 119, 214),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnSave.Click += (s2, e2) =>
                {
                    if (string.IsNullOrWhiteSpace(txtName.Text))
                    {
                        MessageBox.Show("Supplier Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    try
                    {
                        string sql = @"
INSERT INTO dbo.SUPPLIER (SupplierCode, SupplierName, ContactName, Phone, Address, IsActive)
VALUES (@Code, @Name, @Contact, @Phone, @Address, 1);";
                        DbHelper.ExecuteNonQuery(sql,
                            new SqlParameter("@Code", txtCode.Text.Trim()),
                            new SqlParameter("@Name", txtName.Text.Trim()),
                            new SqlParameter("@Contact", txtContact.Text.Trim()),
                            new SqlParameter("@Phone", txtPhone.Text.Trim()),
                            new SqlParameter("@Address", txtAddress.Text.Trim()));

                        modal.DialogResult = DialogResult.OK;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                modal.Controls.Add(layout);
                modal.Controls.Add(btnSave);

                if (modal.ShowDialog() == DialogResult.OK)
                {
                    LoadSuppliers();
                }
            }
        }
    }
}
