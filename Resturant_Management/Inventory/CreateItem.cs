using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Inventory
{
    public partial class CreateItem : UserControl
    {
        private string _selectedImagePath = "";

        public CreateItem()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.Margin = new Padding(0);
            this.Padding = new Padding(0);
            this.AutoScroll = false;
            this.HorizontalScroll.Maximum = 0;
            this.HorizontalScroll.Visible = false;
            this.HorizontalScroll.Enabled = false;

            guna2Pan.Dock = DockStyle.Fill;
            guna2Pan.AutoScroll = true;
            guna2Pan.HorizontalScroll.Maximum = 0;
            guna2Pan.HorizontalScroll.Visible = false;
            guna2Pan.HorizontalScroll.Enabled = false;
            guna2Pan.Scroll += (s, e) => SuppressHorizontalScroll();

            btnSave.Click += BtnSave_Click;
            btnBack.Click += BtnBack_Click;
            PicItem.Click += BtnBrowser_Click;
            label1.Click += BtnBrowser_Click;
            label1.Cursor = Cursors.Hand;
            PicItem.Cursor = Cursors.Hand;

            txtStock.KeyPress += (s, e) =>
            {
                string m = comboValautionMethod.SelectedItem?.ToString() ?? "";
                if (string.Equals(m, "Standard", StringComparison.OrdinalIgnoreCase))
                {
                    e.Handled = true;
                }
            };

            comboValautionMethod.SelectedIndexChanged += ComboValautionMethod_SelectedIndexChanged;

            this.Load += CreateItem_Load;
            this.VisibleChanged += (s, e) => { if (this.Visible && !_isRuntimeInitialized) InitRuntime(); };
            this.Resize += (s, e) => { ApplyResponsiveLayout(); SuppressHorizontalScroll(); };
            guna2Pan.Resize += (s, e) => { ApplyResponsiveLayout(); SuppressHorizontalScroll(); };

            if (!DesignTimeHelper.IsInDesignMode(this))
            {
                InitRuntime();
            }
        }

        private bool _isRuntimeInitialized = false;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignTimeHelper.IsInDesignMode(this) && !_isRuntimeInitialized)
            {
                InitRuntime();
            }
        }

        private void CreateItem_Load(object? sender, EventArgs e)
        {
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            InitRuntime();
        }

        public void InitRuntime()
        {
            if (_isRuntimeInitialized) return;
            _isRuntimeInitialized = true;

            this.BringToFront();
            LoadDropdowns();
            ApplyResponsiveLayout();
            SuppressHorizontalScroll();

            // Ensure all 5 dropdowns default blank and let user select
            this.BeginInvoke(new Action(() =>
            {
                ClearDropdownSelections();
                UpdateStockInputState();
                SuppressHorizontalScroll();
            }));
        }

        private void ClearDropdownSelections()
        {
            comboUom.SelectedIndex = -1;
            comboItemGroup.SelectedIndex = -1;
            comboValautionMethod.SelectedIndex = -1;
            comboPrinter1.SelectedIndex = -1;
            comboPrinter2.SelectedIndex = -1;
        }

        private void ComboValautionMethod_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateStockInputState();
        }

        private void UpdateStockInputState()
        {
            string method = comboValautionMethod.SelectedItem?.ToString() ?? "";
            bool isStandard = string.Equals(method, "Standard", StringComparison.OrdinalIgnoreCase);

            if (isStandard)
            {
                txtStock.ReadOnly = true;
                txtStock.Text = "";
                txtStock.PlaceholderText = "";
                txtStock.FillColor = Color.White;
                txtStock.Cursor = Cursors.Default;
                txtStock.TabStop = false;
            }
            else
            {
                txtStock.ReadOnly = false;
                txtStock.PlaceholderText = "QTY";
                txtStock.FillColor = Color.White;
                txtStock.Cursor = Cursors.IBeam;
                txtStock.TabStop = true;
            }
        }

        private void LoadDropdowns()
        {
            try
            {
                // Item Groups
                DataTable dtGroups = DbHelper.ExecuteQuery("SELECT GroupID, GroupName FROM dbo.ITEM_GROUP ORDER BY GroupName");
                comboItemGroup.DataSource = dtGroups;
                comboItemGroup.DisplayMember = "GroupName";
                comboItemGroup.ValueMember = "GroupID";
                comboItemGroup.SelectedIndex = -1;

                // UOM - only one option ( Unit )
                DataTable dtUom = DbHelper.ExecuteQuery("SELECT UomID, UomName FROM dbo.UOM WHERE UomName = 'Unit'");
                if (dtUom.Rows.Count == 0)
                {
                    DbHelper.ExecuteNonQuery("IF NOT EXISTS (SELECT 1 FROM dbo.UOM WHERE UomName = 'Unit') INSERT INTO dbo.UOM (UomName) VALUES ('Unit');");
                    dtUom = DbHelper.ExecuteQuery("SELECT UomID, UomName FROM dbo.UOM WHERE UomName = 'Unit'");
                }
                comboUom.DataSource = dtUom;
                comboUom.DisplayMember = "UomName";
                comboUom.ValueMember = "UomID";
                if (comboUom.Items.Count > 0)
                    comboUom.SelectedIndex = 0;

                // Printers
                DataTable dtPrinters1 = DbHelper.ExecuteQuery("SELECT PrinterID, PrinterName FROM dbo.PRINTER ORDER BY PrinterName");
                comboPrinter1.DataSource = dtPrinters1;
                comboPrinter1.DisplayMember = "PrinterName";
                comboPrinter1.ValueMember = "PrinterID";
                comboPrinter1.SelectedIndex = -1;

                DataTable dtPrinters2 = DbHelper.ExecuteQuery("SELECT PrinterID, PrinterName FROM dbo.PRINTER ORDER BY PrinterName");
                comboPrinter2.DataSource = dtPrinters2;
                comboPrinter2.DisplayMember = "PrinterName";
                comboPrinter2.ValueMember = "PrinterID";
                comboPrinter2.SelectedIndex = -1;

                // Valuation Methods - only two options ( Standard, FIFO )
                comboValautionMethod.Items.Clear();
                comboValautionMethod.Items.AddRange(new object[] { "Standard", "FIFO" });
                comboValautionMethod.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dropdowns: {ex.Message}");
            }
        }

        private void ApplyResponsiveLayout()
        {
            if (guna2Pan == null || panelInformationItem == null || guna2ShadowPanel1 == null)
                return;

            int clientW = guna2Pan.ClientSize.Width;
            if (clientW < 100) return;

            // Preserve vertical scroll without horizontal shifting
            Point scrollPos = guna2Pan.AutoScrollPosition;
            guna2Pan.AutoScrollPosition = new Point(0, 0);

            guna2Pan.SuspendLayout();
            panelInformationItem.SuspendLayout();
            guna2ShadowPanel1.SuspendLayout();

            int margin = 20;
            // Leave a generous right safety margin (35px) so shadows, vertical scrollbars, and borders never cause horizontal overflow
            int cardW = Math.Max(320, clientW - margin - 35);
            int cardLeft = margin;

            lbCodeItem.Location = new Point(cardLeft, 16);
            lbCodeItem.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);

            // Card 1
            panelInformationItem.Location = new Point(cardLeft, lbCodeItem.Bottom + 12);
            panelInformationItem.Width = cardW;

            int padX = 26;
            int padY = 22;
            int availW = cardW - (padX * 2);

            int colGap = 32;
            int imageW = 220;
            int imageH = 165;
            int remainingW = availW - imageW - (colGap * 2);
            int fieldW = Math.Max(180, remainingW / 2);
            int fieldH = 48; // Increased height

            int col1X = padX;
            int col2X = padX + fieldW + colGap;
            int col3X = padX + (fieldW + colGap) * 2;

            // Generous spacing between input rows
            int rowGap = 30;
            int row1Y = padY + 14;
            int row2Y = row1Y + fieldH + rowGap;
            int row3Y = row2Y + fieldH + rowGap;
            int row4Y = row3Y + fieldH + rowGap;

            Font inputFont = new Font("Segoe UI", 10.5F);
            Font labelFont = new Font("Segoe UI", 9.5F);
            Color labelColor = Color.FromArgb(70, 80, 95);

            Action<Control, Label, string, bool> setupField = (input, lbl, title, isRequired) =>
            {
                lbl.Font = labelFont;
                lbl.ForeColor = labelColor;
                lbl.BackColor = Color.White;
                lbl.Text = title;
                lbl.AutoSize = true;
                lbl.Size = lbl.GetPreferredSize(Size.Empty);
                lbl.BringToFront();

                string reqName = lbl.Name + "_req";
                Control? parent = lbl.Parent ?? input.Parent;
                if (parent != null)
                {
                    Control[] found = parent.Controls.Find(reqName, false);
                    Label reqLbl;
                    if (found.Length > 0 && found[0] is Label existing)
                    {
                        reqLbl = existing;
                    }
                    else
                    {
                        reqLbl = new Label
                        {
                            Name = reqName,
                            Text = "*",
                            AutoSize = true,
                            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                            ForeColor = Color.FromArgb(220, 38, 38), // Red color for required *
                            BackColor = Color.White,
                            Margin = Padding.Empty,
                            Padding = Padding.Empty
                        };
                        reqLbl.Click += (s, e) => input.Focus();
                        parent.Controls.Add(reqLbl);
                    }

                    if (isRequired)
                    {
                        reqLbl.Visible = true;
                        reqLbl.Size = reqLbl.GetPreferredSize(Size.Empty);
                        reqLbl.Location = new Point(lbl.Right - 3, lbl.Top);
                        reqLbl.BringToFront();
                    }
                    else
                    {
                        reqLbl.Visible = false;
                    }
                }
            };

            Action<Guna.UI2.WinForms.Guna2ComboBox> setupCombo = (cb) =>
            {
                cb.ItemHeight = 36;
                cb.Font = inputFont;
                cb.BorderRadius = 6;
            };

            // Setup Combobox dimensions
            setupCombo(comboUom);
            setupCombo(comboItemGroup);
            setupCombo(comboValautionMethod);
            setupCombo(comboPrinter1);
            setupCombo(comboPrinter2);

            // Row 1
            txtCode.Location = new Point(col1X, row1Y);
            txtCode.Size = new Size(fieldW, fieldH);
            txtCode.Font = inputFont;
            txtCode.BorderRadius = 6;
            lbCode.Location = new Point(col1X + 16, row1Y - 11);
            setupField(txtCode, lbCode, "Code", true);

            comboUom.Location = new Point(col2X, row1Y);
            comboUom.Size = new Size(fieldW, fieldH);
            lbUom.Location = new Point(col2X + 16, row1Y - 11);
            setupField(comboUom, lbUom, "Group Uom", true);

            PicItem.Location = new Point(col3X + (imageW - 220) / 2, row1Y);
            PicItem.Size = new Size(220, imageH);
            PicItem.SizeMode = PictureBoxSizeMode.Zoom;
            label1.Font = new Font("Segoe UI", 9.5F, FontStyle.Underline);
            label1.ForeColor = Color.FromArgb(21, 119, 214);
            label1.Location = new Point(PicItem.Left + (PicItem.Width - label1.Width) / 2, PicItem.Bottom + 8);

            // Row 2
            txtName.Location = new Point(col1X, row2Y);
            txtName.Size = new Size(fieldW, fieldH);
            txtName.Font = inputFont;
            txtName.BorderRadius = 6;
            lbName.Location = new Point(col1X + 16, row2Y - 11);
            setupField(txtName, lbName, "Name", true);

            comboItemGroup.Location = new Point(col2X, row2Y);
            comboItemGroup.Size = new Size(fieldW, fieldH);
            lbItemGroup.Location = new Point(col2X + 16, row2Y - 11);
            setupField(comboItemGroup, lbItemGroup, "Item Group", true);

            // Row 3
            txtName2.Location = new Point(col1X, row3Y);
            txtName2.Size = new Size(fieldW, fieldH);
            txtName2.Font = inputFont;
            txtName2.BorderRadius = 6;
            lbName2.Location = new Point(col1X + 16, row3Y - 11);
            setupField(txtName2, lbName2, "Name 2", false);

            comboValautionMethod.Location = new Point(col2X, row3Y);
            comboValautionMethod.Size = new Size(fieldW, fieldH);
            lbValautionMethod.Location = new Point(col2X + 16, row3Y - 11);
            setupField(comboValautionMethod, lbValautionMethod, "Valuation Method", true);

            // Row 4
            int stockW = fieldW - 140;
            txtStock.Location = new Point(col2X, row4Y);
            txtStock.Size = new Size(stockW, fieldH);
            txtStock.Font = inputFont;
            txtStock.BorderRadius = 6;
            label3.Location = new Point(col2X + 16, row4Y - 11);
            setupField(txtStock, label3, "Stock", false);

            chkInactive.Location = new Point(txtStock.Right + 20, row4Y + (fieldH - 28) / 2);
            chkInactive.Size = new Size(110, 28);
            chkInactive.Font = new Font("Segoe UI", 10F);

            // Row 5: Description
            int row5Y = row4Y + fieldH + 26;
            guna2TextBox1.Location = new Point(col1X, row5Y);
            guna2TextBox1.Size = new Size(availW, 85);
            guna2TextBox1.Font = inputFont;
            guna2TextBox1.BorderRadius = 6;

            panelInformationItem.Height = guna2TextBox1.Bottom + padY;

            // Card 2: guna2ShadowPanel1
            guna2ShadowPanel1.Location = new Point(cardLeft, panelInformationItem.Bottom + 20);
            guna2ShadowPanel1.Width = cardW;

            // Divide Card 2 into 3 equal fields across the full card width with same gap
            int c2FieldW = (availW - (colGap * 2)) / 3;
            int card2Y = 34;

            txtUnitPrice.Location = new Point(padX, card2Y);
            txtUnitPrice.Size = new Size(c2FieldW, fieldH);
            txtUnitPrice.Font = inputFont;
            txtUnitPrice.BorderRadius = 6;
            label2.Location = new Point(padX + 16, card2Y - 11);
            setupField(txtUnitPrice, label2, "Price", false);

            comboPrinter1.Location = new Point(padX + c2FieldW + colGap, card2Y);
            comboPrinter1.Size = new Size(c2FieldW, fieldH);
            lbPrinter1.Location = new Point(padX + c2FieldW + colGap + 16, card2Y - 11);
            setupField(comboPrinter1, lbPrinter1, "Printer Name", true);

            comboPrinter2.Location = new Point(padX + (c2FieldW + colGap) * 2, card2Y);
            comboPrinter2.Size = new Size(c2FieldW, fieldH);
            lbPrinter2.Location = new Point(padX + (c2FieldW + colGap) * 2 + 16, card2Y - 11);
            setupField(comboPrinter2, lbPrinter2, "Print Name 2", false);

            guna2ShadowPanel1.Height = txtUnitPrice.Bottom + 28;

            // Action Buttons
            btnSave.Location = new Point(cardLeft, guna2ShadowPanel1.Bottom + 24);
            btnSave.Size = new Size(120, 44);
            btnSave.FillColor = Color.FromArgb(21, 119, 214);
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSave.BorderRadius = 5;

            btnBack.Location = new Point(btnSave.Right + 16, guna2ShadowPanel1.Bottom + 24);
            btnBack.Size = new Size(120, 44);
            btnBack.FillColor = Color.FromArgb(245, 158, 11);
            btnBack.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnBack.BorderRadius = 5;

            panelInformationItem.ResumeLayout(true);
            guna2ShadowPanel1.ResumeLayout(true);
            guna2Pan.ResumeLayout(true);

            if (scrollPos.Y != 0)
            {
                guna2Pan.AutoScrollPosition = new Point(0, Math.Abs(scrollPos.Y));
            }

            SuppressHorizontalScroll();
        }

        private void BtnBrowser_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _selectedImagePath = ofd.FileName;
                    PicItem.Image = Image.FromFile(_selectedImagePath);
                }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            string name = txtName.Text.Trim();
            string name2 = txtName2.Text.Trim();

            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Please enter an Item Code.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCode.Focus();
                return;
            }

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter an Item Name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (comboUom.SelectedIndex < 0 || comboUom.SelectedValue == null)
            {
                MessageBox.Show("Please select a Group UoM.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboUom.Focus();
                return;
            }

            if (comboItemGroup.SelectedIndex < 0 || comboItemGroup.SelectedValue == null)
            {
                MessageBox.Show("Please select an Item Group.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboItemGroup.Focus();
                return;
            }

            if (comboValautionMethod.SelectedIndex < 0 || string.IsNullOrEmpty(comboValautionMethod.SelectedItem?.ToString()))
            {
                MessageBox.Show("Please select a Valuation Method.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboValautionMethod.Focus();
                return;
            }

            decimal.TryParse(txtUnitPrice.Text.Trim(), out decimal price);
            decimal stock = 0;
            if (!txtStock.ReadOnly && !string.IsNullOrWhiteSpace(txtStock.Text))
            {
                decimal.TryParse(txtStock.Text.Trim(), out stock);
            }

            int groupId = Convert.ToInt32(comboItemGroup.SelectedValue);
            int uomId = Convert.ToInt32(comboUom.SelectedValue);
            int? printer1Id = comboPrinter1.SelectedIndex >= 0 && comboPrinter1.SelectedValue != null ? Convert.ToInt32(comboPrinter1.SelectedValue) : (int?)null;
            int? printer2Id = comboPrinter2.SelectedIndex >= 0 && comboPrinter2.SelectedValue != null ? Convert.ToInt32(comboPrinter2.SelectedValue) : (int?)null;
            string valuationMethod = comboValautionMethod.SelectedItem?.ToString() ?? "Standard";
            bool isInactive = chkInactive.Checked;
            bool isStockItem = stock > 0;

            try
            {
                // Check if code exists
                object? exists = DbHelper.ExecuteScalar("SELECT COUNT(1) FROM dbo.ITEM WHERE ItemCode = @Code", new SqlParameter("@Code", code));
                if (exists != null && Convert.ToInt32(exists) > 0)
                {
                    MessageBox.Show($"Item code '{code}' already exists.", "Duplicate Code", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string insertSql = @"
INSERT INTO dbo.ITEM 
(ItemCode, ItemName, ItemName2, [Description], GroupID, UomID, ImagePath, IsStockItem, ValuationMethod, PrinterID, Printer2ID, IsInactive, CreatedAt)
VALUES 
(@Code, @Name, @Name2, @Description, @GroupID, @UomID, @ImagePath, @IsStockItem, @ValuationMethod, @PrinterID, @Printer2ID, @IsInactive, SYSDATETIME());
SELECT SCOPE_IDENTITY();";

                using var conn = DbHelper.GetConnection();
                conn.Open();
                using var trans = conn.BeginTransaction();

                int newItemId;
                using (var cmd = new SqlCommand(insertSql, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@Code", code);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Name2", string.IsNullOrEmpty(name2) ? (object)DBNull.Value : name2);
                    string desc = guna2TextBox1.Text.Trim();
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(desc) ? (object)DBNull.Value : desc);
                    cmd.Parameters.AddWithValue("@GroupID", groupId);
                    cmd.Parameters.AddWithValue("@UomID", uomId);
                    cmd.Parameters.AddWithValue("@ImagePath", string.IsNullOrEmpty(_selectedImagePath) ? (object)DBNull.Value : _selectedImagePath);
                    cmd.Parameters.AddWithValue("@IsStockItem", isStockItem);
                    cmd.Parameters.AddWithValue("@ValuationMethod", valuationMethod);
                    cmd.Parameters.AddWithValue("@PrinterID", (object?)printer1Id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Printer2ID", (object?)printer2Id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsInactive", isInactive);

                    newItemId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Insert Price in KHR; Trigger TR_ITEM_PRICE_AutoConvert automatically converts & inserts USD price
                string priceSql = @"
INSERT INTO dbo.ITEM_PRICE (ItemID, CurrencyCode, Price)
VALUES (@ItemID, 'KHR', @Price);";
                using (var cmdPrice = new SqlCommand(priceSql, conn, trans))
                {
                    cmdPrice.Parameters.AddWithValue("@ItemID", newItemId);
                    cmdPrice.Parameters.AddWithValue("@Price", price);
                    cmdPrice.ExecuteNonQuery();
                }

                if (stock > 0)
                {
                    string stockSql = @"
INSERT INTO dbo.STOCK_MOVEMENT (ItemID, MovementType, Qty, UnitCost, Remark, CreatedBy, CreatedAt)
VALUES (@ItemID, 'Opening', @Qty, @Cost, 'Initial Stock', @User, SYSDATETIME());";
                    using var cmdStock = new SqlCommand(stockSql, conn, trans);
                    cmdStock.Parameters.AddWithValue("@ItemID", newItemId);
                    cmdStock.Parameters.AddWithValue("@Qty", stock);
                    cmdStock.Parameters.AddWithValue("@Cost", price);
                    cmdStock.Parameters.AddWithValue("@User", UserSession.UserID > 0 ? UserSession.UserID : 1);
                    cmdStock.ExecuteNonQuery();
                }

                trans.Commit();

                MessageBox.Show("Item created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NavigateBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving item: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBack_Click(object? sender, EventArgs e)
        {
            NavigateBack();
        }

        private void NavigateBack()
        {
            Control? parentContainer = this.Parent;
            if (parentContainer != null)
            {
                parentContainer.Controls.Clear();
                Itemlist itemlistScreen = new Itemlist { Dock = DockStyle.Fill };
                parentContainer.Controls.Add(itemlistScreen);
                itemlistScreen.BringToFront();
            }
        }

        private void btnBack_Click(object sender, EventArgs e) => NavigateBack();
        private void btnbrowser_Click(object? sender, EventArgs e) => BtnBrowser_Click(sender, e);
        private void guna2Pan_Paint(object? sender, PaintEventArgs e) { }
        private void guna2Pan_Paint_1(object? sender, PaintEventArgs e) { }
        private void pnlMainContent_Paint(object? sender, PaintEventArgs e) { }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        private const int SB_HORZ = 0;

        private void SuppressHorizontalScroll()
        {
            try
            {
                this.AutoScroll = false;
                this.HorizontalScroll.Maximum = 0;
                this.HorizontalScroll.Visible = false;
                this.HorizontalScroll.Enabled = false;

                guna2Pan.HorizontalScroll.Maximum = 0;
                guna2Pan.HorizontalScroll.Visible = false;
                guna2Pan.HorizontalScroll.Enabled = false;

                if (guna2Pan.IsHandleCreated)
                {
                    ShowScrollBar(guna2Pan.Handle, SB_HORZ, false);
                }
                if (this.IsHandleCreated)
                {
                    ShowScrollBar(this.Handle, SB_HORZ, false);
                }
            }
            catch { }
        }
    }
}
