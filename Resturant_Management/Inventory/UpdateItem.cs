using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Inventory
{
    public partial class UpdateItem : UserControl
    {
        private string _itemCode = "";
        private string _selectedImagePath = "";
        private bool _isRuntimeInitialized = false;

        private object? _lastLoadedGroupId;
        private object? _lastLoadedUomId;
        private object? _lastLoadedPrinterId;
        private object? _lastLoadedPrinter2Id;
        private string? _lastLoadedValuationMethod;

        public UpdateItem() : this("")
        {
        }

        public UpdateItem(string itemCode)
        {
            InitializeComponent();
            this.BindingContext = new BindingContext();
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

            _itemCode = itemCode?.Trim() ?? "";

            btnUpdate.Click += BtnUpdate_Click;
            PicItem.Click += BtnBrowser_Click;
            label1.Click += BtnBrowser_Click;
            label1.Cursor = Cursors.Hand;
            PicItem.Cursor = Cursors.Hand;
            PicItem.SizeMode = PictureBoxSizeMode.Zoom;
            btnBack.Click += BtnBack_Click;

            txtStock.KeyPress += (s, e) =>
            {
                string m = comboValautionMethod.SelectedItem?.ToString() ?? "";
                if (string.Equals(m, "Standard", StringComparison.OrdinalIgnoreCase))
                {
                    e.Handled = true;
                }
            };

            comboValautionMethod.SelectedIndexChanged += ComboValautionMethod_SelectedIndexChanged;

            this.Load += (s, e) => { if (!_isRuntimeInitialized) InitRuntime(); };
            this.VisibleChanged += (s, e) => { if (this.Visible && !_isRuntimeInitialized) InitRuntime(); };
            this.Resize += (s, e) => { ApplyResponsiveLayout(); SuppressHorizontalScroll(); };
            guna2Pan.Resize += (s, e) => { ApplyResponsiveLayout(); SuppressHorizontalScroll(); };

            if (!string.IsNullOrEmpty(_itemCode) && !DesignTimeHelper.IsInDesignMode(this))
            {
                InitRuntime();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignTimeHelper.IsInDesignMode(this))
            {
                if (!_isRuntimeInitialized)
                {
                    InitRuntime();
                }
                else if (!string.IsNullOrEmpty(_itemCode))
                {
                    ReassertSelections();
                }
            }
        }

        public void LoadItem(string code)
        {
            _itemCode = code?.Trim() ?? "";
            InitRuntime();
        }

        public void InitRuntime()
        {
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            this.BringToFront();
            LoadDropdowns();
            ApplyResponsiveLayout();

            if (!string.IsNullOrEmpty(_itemCode))
            {
                LoadItemData(_itemCode);
            }
            else
            {
                UpdateStockInputState(preserveCurrentText: false);
            }

            SuppressHorizontalScroll();
            _isRuntimeInitialized = true;
        }

        private void ReassertSelections()
        {
            if (_lastLoadedGroupId != null) SetComboSelectedValue(comboItemGroup, _lastLoadedGroupId);
            // Group UoM has only one option (Unit) - always select it
            if (comboUom.Items.Count > 0) comboUom.SelectedIndex = 0;
            if (_lastLoadedPrinterId != null) SetComboSelectedValue(comboPrinter1, _lastLoadedPrinterId);
            if (_lastLoadedPrinter2Id != null) SetComboSelectedValue(comboPrinter2, _lastLoadedPrinter2Id);
            if (!string.IsNullOrEmpty(_lastLoadedValuationMethod)) SetValuationMethodSelection(_lastLoadedValuationMethod);
            UpdateStockInputState(preserveCurrentText: true);
        }

        private void ComboValautionMethod_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateStockInputState(preserveCurrentText: true);
        }

        private void UpdateStockInputState(bool preserveCurrentText = true)
        {
            string method = comboValautionMethod.SelectedItem?.ToString() ?? "";
            bool isStandard = string.Equals(method, "Standard", StringComparison.OrdinalIgnoreCase);

            if (isStandard)
            {
                txtStock.ReadOnly = true;
                if (!preserveCurrentText)
                {
                    txtStock.Text = "";
                }
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
                DataTable dtGroups = DbHelper.ExecuteQuery("SELECT GroupID, GroupName FROM dbo.ITEM_GROUP ORDER BY GroupName");
                comboItemGroup.DataSource = dtGroups;
                comboItemGroup.DisplayMember = "GroupName";
                comboItemGroup.ValueMember = "GroupID";

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

                DataTable dtPrinters1 = DbHelper.ExecuteQuery("SELECT PrinterID, PrinterName FROM dbo.PRINTER ORDER BY PrinterName");
                comboPrinter1.DataSource = dtPrinters1;
                comboPrinter1.DisplayMember = "PrinterName";
                comboPrinter1.ValueMember = "PrinterID";

                DataTable dtPrinters2 = DbHelper.ExecuteQuery("SELECT PrinterID, PrinterName FROM dbo.PRINTER ORDER BY PrinterName");
                comboPrinter2.DataSource = dtPrinters2;
                comboPrinter2.DisplayMember = "PrinterName";
                comboPrinter2.ValueMember = "PrinterID";

                comboValautionMethod.Items.Clear();
                comboValautionMethod.Items.AddRange(new object[] { "Standard", "FIFO" });
                comboValautionMethod.SelectedIndex = -1;
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
            int fieldH = 48; // Responsive 48px height matching CreateItem

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
            txtCode.ReadOnly = true;
            txtCode.FillColor = Color.White;
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
            btnUpdate.Location = new Point(cardLeft, guna2ShadowPanel1.Bottom + 24);
            btnUpdate.Size = new Size(120, 44);
            btnUpdate.FillColor = Color.FromArgb(21, 119, 214);
            btnUpdate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnUpdate.BorderRadius = 5;

            btnBack.Location = new Point(btnUpdate.Right + 16, guna2ShadowPanel1.Bottom + 24);
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

        private void LoadItemData(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return;
            string cleanCode = code.Trim();

            try
            {
                string sql = @"
SELECT 
    i.ItemID, i.ItemCode, i.ItemName, i.ItemName2, i.Description, i.GroupID, i.UomID,
    COALESCE(
        (SELECT TOP 1 Price FROM dbo.ITEM_PRICE WHERE ItemID = i.ItemID AND CurrencyCode = 'KHR'),
        (SELECT TOP 1 Price * 4000 FROM dbo.ITEM_PRICE WHERE ItemID = i.ItemID AND CurrencyCode = 'USD'),
        ip.PriceKHR, 
        0
    ) AS Price,
    i.ImagePath, i.IsStockItem, i.ValuationMethod, i.PrinterID, i.Printer2ID, i.IsInactive,
    COALESCE(
        (SELECT SUM(CASE WHEN sm.MovementType IN ('Purchase', 'StockIn', 'AdjustmentIn', 'Opening') THEN sm.Qty 
                         WHEN sm.MovementType IN ('Sale', 'StockOut', 'AdjustmentOut', 'Waste') THEN -sm.Qty 
                         ELSE 0 END) FROM dbo.STOCK_MOVEMENT sm WHERE sm.ItemID = i.ItemID),
        s.QtyOnHand,
        0
    ) AS StockQty
FROM dbo.ITEM i
LEFT JOIN dbo.vw_ItemPrice ip ON i.ItemID = ip.ItemID
LEFT JOIN dbo.vw_ItemStock s ON i.ItemID = s.ItemID
WHERE RTRIM(LTRIM(i.ItemCode)) = @Code;";

                DataTable dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@Code", cleanCode));
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtCode.Text = r["ItemCode"]?.ToString() ?? cleanCode;
                    txtCode.ReadOnly = true;
                    txtCode.FillColor = Color.White;
                    txtName.Text = r["ItemName"]?.ToString() ?? "";
                    txtName2.Text = r["ItemName2"]?.ToString() ?? "";
                    guna2TextBox1.Text = r["Description"]?.ToString() ?? "";

                    if (r["Price"] != DBNull.Value)
                    {
                        decimal p = Convert.ToDecimal(r["Price"]);
                        txtUnitPrice.Text = p.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        txtUnitPrice.Text = "0.00";
                    }

                    if (r["StockQty"] != DBNull.Value)
                    {
                        decimal sq = Convert.ToDecimal(r["StockQty"]);
                        txtStock.Text = sq.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        txtStock.Text = "0";
                    }

                    chkInactive.Checked = r["IsInactive"] != DBNull.Value && Convert.ToBoolean(r["IsInactive"]);

                    _lastLoadedGroupId = r["GroupID"];
                    _lastLoadedUomId = r["UomID"];
                    _lastLoadedPrinterId = r["PrinterID"];
                    _lastLoadedPrinter2Id = r["Printer2ID"];
                    _lastLoadedValuationMethod = r["ValuationMethod"]?.ToString() ?? "Standard";

                    ReassertSelections();

                    string? imgPath = r["ImagePath"]?.ToString();
                    if (!string.IsNullOrEmpty(imgPath) && System.IO.File.Exists(imgPath))
                    {
                        try
                        {
                            _selectedImagePath = imgPath;
                            byte[] bytes = System.IO.File.ReadAllBytes(imgPath);
                            using var ms = new System.IO.MemoryStream(bytes);
                            PicItem.Image = new Bitmap(ms);
                        }
                        catch { }
                    }

                    // Reinforce selections once controls are laid out and drawn
                    if (this.IsHandleCreated)
                    {
                        this.BeginInvoke(new Action(ReassertSelections));
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Item with code '{cleanCode}' not found.");
                    MessageBox.Show($"Item code '{cleanCode}' was not found in the database.", "Item Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading item details: {ex.Message}");
                MessageBox.Show($"Error loading item details: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetComboSelectedValue(Guna.UI2.WinForms.Guna2ComboBox combo, object? val)
        {
            if (val == null || val == DBNull.Value)
            {
                combo.SelectedIndex = -1;
                return;
            }

            string target = val.ToString()?.Trim() ?? "";
            if (string.IsNullOrEmpty(target))
            {
                combo.SelectedIndex = -1;
                return;
            }

            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is DataRowView drv)
                {
                    string colVal = drv[combo.ValueMember]?.ToString()?.Trim() ?? "";
                    if (string.Equals(colVal, target, StringComparison.OrdinalIgnoreCase))
                    {
                        combo.SelectedIndex = i;
                        return;
                    }
                }
                else if (string.Equals(combo.Items[i]?.ToString()?.Trim(), target, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }

            if (int.TryParse(target, out int targetInt))
            {
                for (int i = 0; i < combo.Items.Count; i++)
                {
                    if (combo.Items[i] is DataRowView drv)
                    {
                        if (int.TryParse(drv[combo.ValueMember]?.ToString(), out int rowInt) && rowInt == targetInt)
                        {
                            combo.SelectedIndex = i;
                            return;
                        }
                    }
                }
            }

            try { combo.SelectedValue = val; } catch { }
        }

        private void SetValuationMethodSelection(string? val)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                comboValautionMethod.SelectedIndex = -1;
                return;
            }

            string target = val.Trim();
            for (int i = 0; i < comboValautionMethod.Items.Count; i++)
            {
                if (string.Equals(comboValautionMethod.Items[i]?.ToString()?.Trim(), target, StringComparison.OrdinalIgnoreCase))
                {
                    comboValautionMethod.SelectedIndex = i;
                    return;
                }
            }
            comboValautionMethod.SelectedIndex = -1;
        }

        private int? GetComboSelectedId(Guna.UI2.WinForms.Guna2ComboBox combo)
        {
            if (combo.SelectedIndex < 0) return null;
            if (combo.SelectedValue != null && int.TryParse(combo.SelectedValue.ToString(), out int idFromVal))
                return idFromVal;
            if (combo.SelectedItem is DataRowView drv && int.TryParse(drv[combo.ValueMember]?.ToString(), out int idFromRow))
                return idFromRow;
            return null;
        }

        private void BtnBrowser_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _selectedImagePath = ofd.FileName;
                    byte[] bytes = System.IO.File.ReadAllBytes(_selectedImagePath);
                    using var ms = new System.IO.MemoryStream(bytes);
                    PicItem.Image = new Bitmap(ms);
                }
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            string name = txtName.Text.Trim();
            string name2 = txtName2.Text.Trim();
            string desc = guna2TextBox1.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter an Item Name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            int? uomId = GetComboSelectedId(comboUom);
            if (uomId == null)
            {
                MessageBox.Show("Please select a Group UoM.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboUom.Focus();
                return;
            }

            int? groupId = GetComboSelectedId(comboItemGroup);
            if (groupId == null)
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

            decimal.TryParse(txtUnitPrice.Text.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price);
            decimal stock = 0;
            if (!string.IsNullOrWhiteSpace(txtStock.Text))
            {
                decimal.TryParse(txtStock.Text.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out stock);
            }

            int? printer1Id = GetComboSelectedId(comboPrinter1);
            int? printer2Id = GetComboSelectedId(comboPrinter2);
            string valuationMethod = comboValautionMethod.SelectedItem?.ToString() ?? "Standard";
            bool isInactive = chkInactive.Checked;
            bool isStockItem = stock > 0;

            try
            {
                string updateSql = @"
UPDATE dbo.ITEM SET 
    ItemName = @Name,
    ItemName2 = @Name2,
    [Description] = @Description,
    GroupID = @GroupID,
    UomID = @UomID,
    IsStockItem = @IsStockItem,
    ValuationMethod = @ValuationMethod,
    PrinterID = @PrinterID,
    Printer2ID = @Printer2ID,
    IsInactive = @IsInactive,
    ImagePath = CASE WHEN @ImagePath IS NOT NULL THEN @ImagePath ELSE ImagePath END
WHERE ItemCode = @Code;

DECLARE @ItemID int = (SELECT ItemID FROM dbo.ITEM WHERE ItemCode = @Code);

IF EXISTS (SELECT 1 FROM dbo.ITEM_PRICE WHERE ItemID = @ItemID AND CurrencyCode = 'KHR')
    UPDATE dbo.ITEM_PRICE SET Price = @Price WHERE ItemID = @ItemID AND CurrencyCode = 'KHR';
ELSE
    INSERT INTO dbo.ITEM_PRICE (ItemID, CurrencyCode, Price) VALUES (@ItemID, 'KHR', @Price);
";

                int rows = DbHelper.ExecuteNonQuery(updateSql,
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Name2", string.IsNullOrEmpty(name2) ? (object)DBNull.Value : name2),
                    new SqlParameter("@Description", string.IsNullOrEmpty(desc) ? (object)DBNull.Value : desc),
                    new SqlParameter("@GroupID", groupId.Value),
                    new SqlParameter("@UomID", uomId.Value),
                    new SqlParameter("@IsStockItem", isStockItem),
                    new SqlParameter("@Price", price),
                    new SqlParameter("@ValuationMethod", valuationMethod),
                    new SqlParameter("@PrinterID", (object?)printer1Id ?? DBNull.Value),
                    new SqlParameter("@Printer2ID", (object?)printer2Id ?? DBNull.Value),
                    new SqlParameter("@IsInactive", isInactive),
                    new SqlParameter("@ImagePath", string.IsNullOrEmpty(_selectedImagePath) ? (object)DBNull.Value : _selectedImagePath),
                    new SqlParameter("@Code", code)
                );

                if (rows > 0)
                {
                    MessageBox.Show("Item updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    NavigateBack();
                }
                else
                {
                    MessageBox.Show("No item was updated.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating item: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
