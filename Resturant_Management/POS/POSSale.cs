using Guna.UI2.WinForms;
using Resturant_Management.Payment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.POS
{
    public class POSSale : UserControl
    {
        private Panel pnlToolbar = null!;
        public Guna2Button btnReceipt = null!;
        private Panel pnlGreenHeader = null!;
        public Guna2Button btnBackTable = null!;
        public Label lblTableOrder = null!;
        public Label lblCountRows = null!;
        public Label lblCountQtys = null!;
        public Guna2Button btnNote = null!;
        public Guna2Button btnCustomerTag = null!;
        public Guna2Button btnReceiptList = null!;
        private Panel pnlCustomerBar = null!;
        public Label lblCustomerName = null!;
        public Guna2TextBox txtSearchCustomer = null!;
        public Guna2TextBox txtReadBarcode = null!;
        public Guna2DataGridView dgvCart = null!;

        // ===== Bottom Controls =====
        private Panel pnlBottom = null!;
        private TableLayoutPanel tblSummary = null!;
        private TableLayoutPanel tblButtons = null!;

        public Label lbSubTotalUSD = null!;
        public Label lbDisItemUSD = null!;
        public Label lbDisDocUSD = null!;
        public Label lbGrandTotalUSD = null!;

        public Guna2Button btnDisDoc = null!;
        public Guna2Button btnSend = null!;
        public Guna2Button btnBill = null!;
        public Guna2Button btnPay = null!;

        // Action Column Layout Constants
        private const int ActionStartX = 6;
        private const int ActionSlotWidth = 24;
        private const int ActionSlotCount = 5; // 0: Note, 1: Minus, 2: Plus, 3: Calc, 4: Delete
        private const decimal KhrPerUsd = 4000m;

        // State fields
        private int _tableId = 1;
        private string _tableName = "Table-1";
        private int _currentCustomerId = 1;
        private long _currentOrderId = 0;
        private string _currentOrderNo = "";
        private string _invoiceNo = "";
        private string _orderNote = "";

        // Custom Document Discount State
        private decimal currentDocDiscountKHR = 0.00m;

        // Theme Colors
        private static readonly Color HeaderGreen = Color.FromArgb(80, 168, 125);
        private static readonly Color BadgeColor = Color.FromArgb(70, 148, 138);
        private static readonly Color BadgeBorder = Color.FromArgb(140, 205, 175);
        private static readonly Color BarBlueGray = Color.FromArgb(170, 184, 200);
        private static readonly Color GridHeader = Color.FromArgb(92, 118, 141);

        public POSSale() : this(1, "Table-1")
        {
        }

        public POSSale(int tableId, string tableName)
        {
            _tableId = tableId;
            _tableName = tableName;

            InitializeComponentByCode();
            SetupCartGridColumns();

            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            _currentOrderNo = GenerateOrderNo();
            _invoiceNo = GenerateInvoiceNo();

            lblTableOrder.Text = $"{_tableName} > {_invoiceNo} (#{_currentOrderNo})";

            LoadDefaultCustomer();
            LoadActiveOrderForTable();

            btnPay.Click += BtnPay_Click;
            btnDisDoc.Click += BtnDisDoc_Click;
            btnSend.Click += BtnSend_Click;
            btnBill.Click += BtnBill_Click;
            btnBackTable.Click += BtnBackTable_Click;
            btnReceipt.Click += BtnReceipt_Click;
            btnReceiptList.Click += BtnReceiptList_Click;
            btnNote.Click += BtnNote_Click;
            btnCustomerTag.Click += BtnCustomerTag_Click;
            txtReadBarcode.KeyDown += TxtReadBarcode_KeyDown;
            txtSearchCustomer.KeyDown += TxtSearchCustomer_KeyDown;

            // Auto-connect: whenever a product card is clicked anywhere, add it to this cart
            ProductList.ProductClicked += OnProductClicked;
        }

        private void LoadActiveOrderForTable()
        {
            try
            {
                string sql = @"
SELECT TOP 1 o.OrderID, o.OrderNo, o.InvoiceNo, o.CustomerID, c.CustomerName, o.Note, o.DocDiscountAmount, o.Status
FROM dbo.SALE_ORDER o
LEFT JOIN dbo.CUSTOMER c ON o.CustomerID = c.CustomerID
WHERE o.TableID = @TableID AND o.Status IN ('Open', 'Sent', 'Billed')
ORDER BY o.OrderID DESC;";

                DataTable dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@TableID", _tableId));
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    _currentOrderId = Convert.ToInt64(r["OrderID"]);
                    _currentOrderNo = r["OrderNo"]?.ToString() ?? _currentOrderNo;
                    _invoiceNo = (r["InvoiceNo"] != DBNull.Value && !string.IsNullOrWhiteSpace(r["InvoiceNo"].ToString()))
                        ? r["InvoiceNo"].ToString()!
                        : GenerateInvoiceNo();
                    _currentCustomerId = r["CustomerID"] != DBNull.Value ? Convert.ToInt32(r["CustomerID"]) : 1;
                    if (r["CustomerName"] != DBNull.Value)
                        lblCustomerName.Text = r["CustomerName"].ToString()!;
                    _orderNote = r["Note"]?.ToString() ?? "";
                    btnNote.Text = string.IsNullOrEmpty(_orderNote) ? "Note" : "Note *";
                    currentDocDiscountKHR = r["DocDiscountAmount"] != DBNull.Value ? Convert.ToDecimal(r["DocDiscountAmount"]) : 0m;
                    string status = r["Status"]?.ToString() ?? "Open";

                    lblTableOrder.Text = $"{_tableName} > {_invoiceNo} (#{_currentOrderNo}) ({status})";

                    string itemsSql = @"
SELECT oi.[LineNo], i.ItemCode, oi.ItemName, oi.Qty, oi.UomName, oi.UnitPrice, oi.TotalBeforeDis, oi.DiscountPercent, oi.TotalAfterDis
FROM dbo.SALE_ORDER_ITEM oi
JOIN dbo.ITEM i ON oi.ItemID = i.ItemID
WHERE oi.OrderID = @OID
ORDER BY oi.[LineNo] ASC;";

                    DataTable itemsDt = DbHelper.ExecuteQuery(itemsSql, new SqlParameter("@OID", _currentOrderId));
                    dgvCart.Rows.Clear();
                    foreach (DataRow itemRow in itemsDt.Rows)
                    {
                        dgvCart.Rows.Add(
                            itemRow["LineNo"],
                            itemRow["ItemCode"]?.ToString() ?? "",
                            itemRow["ItemName"]?.ToString() ?? "",
                            Convert.ToDecimal(itemRow["Qty"]).ToString("N2"),
                            itemRow["UomName"]?.ToString() ?? "Unit",
                            Convert.ToDecimal(itemRow["UnitPrice"]).ToString("N2"),
                            Convert.ToDecimal(itemRow["TotalBeforeDis"]).ToString("N2"),
                            Convert.ToDecimal(itemRow["DiscountPercent"]).ToString("0.##"),
                            Convert.ToDecimal(itemRow["TotalAfterDis"]).ToString("N2"),
                            ""
                        );
                    }
                    RecalculateTotals();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading active order: {ex.Message}");
            }
        }

        private string GenerateInvoiceNo()
        {
            try
            {
                string todayPrefix = $"INV-{DateTime.Now:yyyyMMdd}-";
                object? maxObj = DbHelper.ExecuteScalar(
                    "SELECT TOP 1 InvoiceNo FROM dbo.SALE_ORDER WHERE InvoiceNo LIKE @Prefix + '%' ORDER BY InvoiceNo DESC",
                    new SqlParameter("@Prefix", todayPrefix));

                if (maxObj != null && maxObj != DBNull.Value)
                {
                    string maxStr = maxObj.ToString()!;
                    if (maxStr.Length > todayPrefix.Length)
                    {
                        string suffix = maxStr.Substring(todayPrefix.Length);
                        if (int.TryParse(suffix, out int currentSeq))
                        {
                            return $"{todayPrefix}{(currentSeq + 1):D4}";
                        }
                    }
                }

                object? countObj = DbHelper.ExecuteScalar("SELECT COUNT(1) FROM dbo.SALE_ORDER WHERE CAST(PostingDate AS date) = CAST(GETDATE() AS date)");
                int seq = (countObj != null && countObj != DBNull.Value) ? Convert.ToInt32(countObj) + 1 : 1;
                return $"{todayPrefix}{seq:D4}";
            }
            catch
            {
                return $"INV-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";
            }
        }

        private string GenerateOrderNo()
        {
            try
            {
                object? count = DbHelper.ExecuteScalar("SELECT COUNT(1) FROM dbo.SALE_ORDER");
                int next = (count != null && count != DBNull.Value) ? Convert.ToInt32(count) + 1 : 1;
                return $"Order-{next}";
            }
            catch
            {
                return $"Order-{DateTime.Now:HHmmss}";
            }
        }

        private void LoadDefaultCustomer()
        {
            try
            {
                DataTable dt = DbHelper.ExecuteQuery("SELECT TOP 1 CustomerID, CustomerCode, CustomerName FROM dbo.CUSTOMER WHERE IsDefault = 1");
                if (dt.Rows.Count > 0)
                {
                    _currentCustomerId = Convert.ToInt32(dt.Rows[0]["CustomerID"]);
                    lblCustomerName.Text = dt.Rows[0]["CustomerName"]?.ToString() ?? "General Customer";
                }
            }
            catch { }
        }

        private void TxtSearchCustomer_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string query = txtSearchCustomer.Text.Trim();
                if (string.IsNullOrEmpty(query)) return;

                try
                {
                    DataTable dt = DbHelper.ExecuteQuery(
                        "SELECT TOP 1 CustomerID, CustomerCode, CustomerName FROM dbo.CUSTOMER WHERE CustomerCode = @q OR CustomerName LIKE @pattern",
                        new SqlParameter("@q", query),
                        new SqlParameter("@pattern", $"%{query}%"));

                    if (dt.Rows.Count > 0)
                    {
                        _currentCustomerId = Convert.ToInt32(dt.Rows[0]["CustomerID"]);
                        lblCustomerName.Text = dt.Rows[0]["CustomerName"]?.ToString() ?? query;
                    }
                    else
                    {
                        MessageBox.Show($"Customer '{query}' not found.", "Customer Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnBackTable_Click(object? sender, EventArgs e)
        {
            Control? host = this.Parent?.Parent;
            if (host != null)
            {
                host.Controls.Clear();
                TableCards tableCards = new TableCards { Dock = DockStyle.Fill };
                host.Controls.Add(tableCards);
                tableCards.BringToFront();
            }
        }

        private void OnProductClicked(object? sender, ProductEventArgs e)
        {
            if (IsDisposed || !IsHandleCreated) return;
            AddProductToCart(e.Code, e.Name, e.Price);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ProductList.ProductClicked -= OnProductClicked;
            base.Dispose(disposing);
        }

        private void TxtReadBarcode_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string barcode = txtReadBarcode.Text.Trim();
                if (!string.IsNullOrEmpty(barcode))
                {
                    try
                    {
                        string sql = @"
SELECT i.ItemCode, i.ItemName, COALESCE(ip.PriceKHR, ip.PriceUSD * 4000, 0) AS Price 
FROM dbo.ITEM i 
LEFT JOIN dbo.vw_ItemPrice ip ON i.ItemID = ip.ItemID 
WHERE i.ItemCode = @code AND i.IsInactive = 0";
                        DataTable dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@code", barcode));
                        if (dt.Rows.Count > 0)
                        {
                            string code = dt.Rows[0]["ItemCode"]?.ToString() ?? barcode;
                            string name = dt.Rows[0]["ItemName"]?.ToString() ?? barcode;
                            decimal price = Convert.ToDecimal(dt.Rows[0]["Price"]);
                            AddProductToCart(code, name, price);
                        }
                        else
                        {
                            MessageBox.Show($"Item code '{barcode}' not found.", "Barcode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    txtReadBarcode.Clear();
                }
            }
        }

        private void BtnCustomerTag_Click(object? sender, EventArgs e)
        {
            using (Form modal = new Form())
            {
                modal.Text = "Select Customer";
                modal.Size = new Size(500, 400);
                modal.StartPosition = FormStartPosition.CenterParent;
                modal.FormBorderStyle = FormBorderStyle.FixedDialog;
                modal.MaximizeBox = false;

                TextBox txtSearchCust = new TextBox { Dock = DockStyle.Top, Font = new Font("Segoe UI", 10F), PlaceholderText = "Search customer..." };
                DataGridView dgvCust = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    RowHeadersVisible = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };

                Button btnSelect = new Button { Text = "Select", Dock = DockStyle.Bottom, Height = 40, BackColor = HeaderGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };

                void LoadCustomers(string q = "")
                {
                    try
                    {
                        string sql = "SELECT CustomerID, CustomerCode, CustomerName, Phone FROM dbo.CUSTOMER WHERE @q = '' OR CustomerCode LIKE @p OR CustomerName LIKE @p OR Phone LIKE @p ORDER BY CustomerName";
                        DataTable dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@q", q), new SqlParameter("@p", $"%{q}%"));
                        dgvCust.DataSource = dt;
                    }
                    catch { }
                }

                txtSearchCust.TextChanged += (s, args) => LoadCustomers(txtSearchCust.Text.Trim());
                dgvCust.DoubleClick += (s, args) => { btnSelect.PerformClick(); };

                btnSelect.Click += (s, args) =>
                {
                    if (dgvCust.CurrentRow != null)
                    {
                        _currentCustomerId = Convert.ToInt32(dgvCust.CurrentRow.Cells["CustomerID"].Value);
                        lblCustomerName.Text = dgvCust.CurrentRow.Cells["CustomerName"].Value?.ToString() ?? "Customer";
                        modal.DialogResult = DialogResult.OK;
                    }
                };

                modal.Controls.Add(dgvCust);
                modal.Controls.Add(txtSearchCust);
                modal.Controls.Add(btnSelect);

                LoadCustomers();
                modal.ShowDialog();
            }
        }

        private void BtnNote_Click(object? sender, EventArgs e)
        {
            using (Form modal = new Form())
            {
                modal.Text = "Order Note";
                modal.Size = new Size(400, 250);
                modal.StartPosition = FormStartPosition.CenterParent;
                modal.FormBorderStyle = FormBorderStyle.FixedDialog;
                modal.MaximizeBox = false;

                TextBox txtNoteInput = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    Font = new Font("Segoe UI", 10F),
                    Text = _orderNote,
                    ScrollBars = ScrollBars.Vertical
                };

                Button btnSaveNote = new Button
                {
                    Text = "Save Note",
                    Dock = DockStyle.Bottom,
                    Height = 40,
                    BackColor = HeaderGreen,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };

                btnSaveNote.Click += (s, args) =>
                {
                    _orderNote = txtNoteInput.Text.Trim();
                    btnNote.Text = string.IsNullOrEmpty(_orderNote) ? "Note" : "Note *";
                    modal.DialogResult = DialogResult.OK;
                };

                modal.Controls.Add(txtNoteInput);
                modal.Controls.Add(btnSaveNote);
                modal.ShowDialog();
            }
        }

        private void BtnReceipt_Click(object? sender, EventArgs e)
        {
            OpenReceiptList();
        }

        public void OpenReceiptList()
        {
            using (var frm = new FrmReceiptList())
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                Form? parentForm = this.FindForm();
                if (parentForm != null)
                    frm.ShowDialog(parentForm);
                else
                    frm.ShowDialog(this);
            }
        }

        private void Show80mmReceiptPreview()
        {
            try
            {
                int calculatedHeight = Calculate80mmReceiptHeight();

                var printDoc = new System.Drawing.Printing.PrintDocument();
                printDoc.DocumentName = $"Receipt_{_invoiceNo}_{_currentOrderNo}";
                // Suppress "Generating preview..." / progress popup
                printDoc.PrintController = new System.Drawing.Printing.StandardPrintController();

                // 80mm width in hundredths of an inch is 315 (80mm / 25.4 * 100)
                printDoc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("80mm Thermal", 315, calculatedHeight);
                printDoc.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);

                printDoc.PrintPage += (ps, pe) =>
                {
                    if (pe.Graphics != null)
                    {
                        Render80mmReceipt(pe.Graphics, 315, calculatedHeight);
                    }
                };

                using var preview = new FrmReceiptPreview(
                    printDoc,
                    $"Print Preview (80x80) - {_invoiceNo} (#{_currentOrderNo})",
                    (g, w, h) => Render80mmReceipt(g, w, h));

                preview.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print preview error: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int Calculate80mmReceiptHeight()
        {
            float h = 20f;  // Top margin
            h += 24f;       // Company title
            h += 34f;       // Address / Phone
            h += 24f;       // Receipt title
            h += 14f;       // Line & gap
            h += 108f;      // Order meta (including Invoice No)
            if (!string.IsNullOrEmpty(_orderNote)) h += 18f;
            h += 14f;       // Line & gap
            h += 26f;       // Header & line

            using (Bitmap dummyBmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(dummyBmp))
            using (Font itemFont = DbHelper.GetKhmerFont(8F, FontStyle.Regular))
            {
                foreach (DataGridViewRow row in dgvCart.Rows)
                {
                    string name = row.Cells["colName"].Value?.ToString() ?? "";
                    SizeF sz = g.MeasureString(name, itemFont, 135);
                    float rowH = Math.Max(18f, sz.Height + 2);
                    h += rowH + 4f;

                    decimal discPct = ParseDecimal(row.Cells["colDisc"].Value);
                    if (discPct > 0) h += 14f;
                }
            }

            h += 14f;       // Line & gap
            h += 20f;       // Subtotal
            if (currentDocDiscountKHR > 0) h += 20f;
            h += 52f;       // Grand totals KHR & USD
            h += 60f;       // Payment & change lines (if present)
            h += 52f;       // Footer greetings
            h += 40f;       // Bottom padding

            return Math.Max(350, (int)Math.Ceiling(h));
        }

        private void Render80mmReceipt(Graphics g, float paperWidth, float paperHeight)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float printableWidth = 299f;
            float startX = 8f;
            float endX = startX + printableWidth;
            float y = 12f;

            using var fontTitle = DbHelper.GetKhmerFont(11F, FontStyle.Bold);
            using var fontSubtitle = DbHelper.GetKhmerFont(8F, FontStyle.Regular);
            using var fontHeader = DbHelper.GetKhmerFont(9.5F, FontStyle.Bold);
            using var fontBody = DbHelper.GetKhmerFont(8F, FontStyle.Regular);
            using var fontBodyBold = DbHelper.GetKhmerFont(8F, FontStyle.Bold);
            using var fontTotal = DbHelper.GetKhmerFont(10F, FontStyle.Bold);
            using var fontSmall = DbHelper.GetKhmerFont(7.5F, FontStyle.Regular);

            using var penDash = new Pen(Color.FromArgb(90, 90, 90), 1f) { DashStyle = DashStyle.Dash };
            using var penSolid = new Pen(Color.Black, 1.2f);
            using var brushText = new SolidBrush(Color.Black);

            using var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
            using var sfRight = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            using var sfItem = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.Word };

            // 1. Company Profile
            string compName = "RESTAURANT MANAGEMENT";
            string compPhone = "";
            string compAddress = "";
            try
            {
                DataTable dtComp = DbHelper.ExecuteQuery("SELECT TOP 1 CompanyName, Phone, Address FROM dbo.COMPANY_PROFILE");
                if (dtComp.Rows.Count > 0)
                {
                    string? cName = dtComp.Rows[0]["CompanyName"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(cName)) compName = cName;
                    compPhone = dtComp.Rows[0]["Phone"]?.ToString() ?? "";
                    compAddress = dtComp.Rows[0]["Address"]?.ToString() ?? "";
                }
            }
            catch { }

            g.DrawString(compName, fontTitle, brushText, new RectangleF(startX, y, printableWidth, 22), sfCenter);
            y += 22;

            if (!string.IsNullOrWhiteSpace(compAddress))
            {
                g.DrawString(compAddress, fontSubtitle, brushText, new RectangleF(startX, y, printableWidth, 16), sfCenter);
                y += 16;
            }
            if (!string.IsNullOrWhiteSpace(compPhone))
            {
                g.DrawString($"Tel: {compPhone}", fontSubtitle, brushText, new RectangleF(startX, y, printableWidth, 16), sfCenter);
                y += 16;
            }

            y += 4;
            g.DrawString("RECEIPT / វិក្កយបត្រ", fontHeader, brushText, new RectangleF(startX, y, printableWidth, 20), sfCenter);
            y += 22;

            g.DrawLine(penDash, startX, y, endX, y);
            y += 6;

            // 2. Order Metadata
            void DrawMetaRow(string label, string val)
            {
                g.DrawString(label, fontBodyBold, brushText, new RectangleF(startX, y, 75, 16), sfLeft);
                g.DrawString(val, fontBody, brushText, new RectangleF(startX + 75, y, printableWidth - 75, 16), sfLeft);
                y += 17;
            }

            DrawMetaRow("Invoice No :", string.IsNullOrEmpty(_invoiceNo) ? $"#{_currentOrderNo}" : _invoiceNo);
            DrawMetaRow("Order No   :", $"#{_currentOrderNo}");
            DrawMetaRow("Table      :", _tableName);
            DrawMetaRow("Customer   :", lblCustomerName.Text);
            DrawMetaRow("Date       :", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DrawMetaRow("Cashier    :", UserSession.FullName);
            if (!string.IsNullOrEmpty(_orderNote))
            {
                DrawMetaRow("Note       :", _orderNote);
            }

            y += 4;
            g.DrawLine(penDash, startX, y, endX, y);
            y += 6;

            // 3. Item Table Header
            float colItemW = 135f;
            float colQtyW = 32f;
            float colPriceW = 58f;
            float colTotalW = 68f;

            float colItemX = startX;
            float colQtyX = colItemX + colItemW + 2f;
            float colPriceX = colQtyX + colQtyW + 2f;
            float colTotalX = colPriceX + colPriceW + 2f;

            g.DrawString("Item", fontBodyBold, brushText, new RectangleF(colItemX, y, colItemW, 18), sfLeft);
            g.DrawString("Qty", fontBodyBold, brushText, new RectangleF(colQtyX, y, colQtyW, 18), sfCenter);
            g.DrawString("Price", fontBodyBold, brushText, new RectangleF(colPriceX, y, colPriceW, 18), sfRight);
            g.DrawString("Total", fontBodyBold, brushText, new RectangleF(colTotalX, y, colTotalW, 18), sfRight);
            y += 20;

            g.DrawLine(penDash, startX, y, endX, y);
            y += 6;

            // 4. Cart Items
            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                string name = row.Cells["colName"].Value?.ToString() ?? "";
                decimal qty = ParseDecimal(row.Cells["colQty"].Value);
                decimal price = ParseDecimal(row.Cells["colPrice"].Value);
                decimal aftDis = ParseDecimal(row.Cells["colAftDis"].Value);
                decimal discPct = ParseDecimal(row.Cells["colDisc"].Value);

                SizeF nameSize = g.MeasureString(name, fontBody, (int)colItemW, sfItem);
                float rowH = Math.Max(18f, nameSize.Height + 2);

                g.DrawString(name, fontBody, brushText, new RectangleF(colItemX, y, colItemW, rowH), sfItem);
                g.DrawString(qty.ToString("0.##"), fontBody, brushText, new RectangleF(colQtyX, y, colQtyW, 18), sfCenter);
                g.DrawString(price.ToString("N0"), fontBody, brushText, new RectangleF(colPriceX, y, colPriceW, 18), sfRight);
                g.DrawString(aftDis.ToString("N0"), fontBody, brushText, new RectangleF(colTotalX, y, colTotalW, 18), sfRight);
                y += rowH;

                if (discPct > 0)
                {
                    g.DrawString($"  (Disc: {discPct:0.##}%)", fontSmall, Brushes.DimGray, new RectangleF(colItemX, y, colItemW, 14), sfLeft);
                    y += 14;
                }
                y += 3;
            }

            g.DrawLine(penDash, startX, y, endX, y);
            y += 6;

            // 5. Totals
            decimal subTotalKHR = GetCartSubTotal();
            decimal subTotalUSD = subTotalKHR >= 100m ? (subTotalKHR / KhrPerUsd) : subTotalKHR;
            decimal docDiscountKHR = currentDocDiscountKHR;
            decimal itemDiscountKHR = 0;
            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                decimal bef = ParseDecimal(row.Cells["colBefDis"].Value);
                decimal aft = ParseDecimal(row.Cells["colAftDis"].Value);
                if (bef > aft) itemDiscountKHR += (bef - aft);
            }
            decimal totalDiscountKHR = itemDiscountKHR + docDiscountKHR;
            decimal grandTotalKHR = Math.Max(0m, subTotalKHR - totalDiscountKHR);
            decimal grandTotalUSD = grandTotalKHR >= 100m ? (grandTotalKHR / KhrPerUsd) : grandTotalKHR;

            void DrawSummaryRow(string label, string val, Font f)
            {
                g.DrawString(label, f, brushText, new RectangleF(startX, y, 120, 18), sfLeft);
                g.DrawString(val, f, brushText, new RectangleF(startX + 120, y, printableWidth - 120, 18), sfRight);
                y += 19;
            }

            DrawSummaryRow("SubTotal :", $"KHR {subTotalKHR:N0} (${subTotalUSD:N2})", fontBody);
            if (totalDiscountKHR > 0)
            {
                DrawSummaryRow("Discount :", $"- KHR {totalDiscountKHR:N0}", fontBody);
            }

            y += 2;
            g.DrawLine(penSolid, startX, y, endX, y);
            y += 6;

            DrawSummaryRow("GRAND TOTAL :", $"KHR {grandTotalKHR:N0}", fontTotal);
            DrawSummaryRow("TOTAL USD   :", $"${grandTotalUSD:N2}", fontTotal);

            y += 2;
            g.DrawLine(penSolid, startX, y, endX, y);
            y += 6;

            // 6. Payment Information (if paid)
            try
            {
                if (_currentOrderId > 0)
                {
                    DataTable dtPay = DbHelper.ExecuteQuery(
                        @"SELECT TOP 1 p.TotalDue, p.TotalReceived, p.ChangeAmount, p.ExchangeRate, pm.MethodName
                          FROM dbo.PAYMENT p
                          LEFT JOIN dbo.PAYMENT_LINE pl ON p.PaymentID = pl.PaymentID
                          LEFT JOIN dbo.PAYMENT_METHOD pm ON pl.MethodID = pm.MethodID
                          WHERE p.OrderID = @OID
                          ORDER BY p.PaymentID DESC",
                        new SqlParameter("@OID", _currentOrderId));

                    if (dtPay.Rows.Count > 0)
                    {
                        decimal totalRec = Convert.ToDecimal(dtPay.Rows[0]["TotalReceived"]);
                        decimal change = Convert.ToDecimal(dtPay.Rows[0]["ChangeAmount"]);
                        decimal rate = Convert.ToDecimal(dtPay.Rows[0]["ExchangeRate"]);
                        string method = dtPay.Rows[0]["MethodName"]?.ToString() ?? "Cash";

                        DrawSummaryRow($"Paid ({method}) :", $"${totalRec:N2} ({(totalRec * rate):N0} ៛)", fontBody);
                        DrawSummaryRow("Change :", $"${change:N2} ({(change * rate):N0} ៛)", fontBodyBold);
                        y += 2;
                        g.DrawLine(penDash, startX, y, endX, y);
                        y += 6;
                    }
                }
            }
            catch { }

            // 7. Footer
            y += 6;
            g.DrawString("Thank you for dining with us!", fontBodyBold, brushText, new RectangleF(startX, y, printableWidth, 18), sfCenter);
            y += 18;
            g.DrawString("សូមអរគុណ សូមអញ្ជើញមកពិសាម្តងទៀត!", fontBody, brushText, new RectangleF(startX, y, printableWidth, 18), sfCenter);
            y += 20;

            g.DrawString($"Printed: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", fontSmall, Brushes.DimGray, new RectangleF(startX, y, printableWidth, 14), sfCenter);
            y += 16;
        }

        private void BtnReceiptList_Click(object? sender, EventArgs e)
        {
            OpenReceiptList();
        }

        private static decimal ParseDecimal(object? val)
        {
            if (val == null) return 0m;
            string s = val.ToString() ?? "";
            s = s.Replace(",", "").Replace("KHR", "").Replace("USD", "").Replace("$", "").Replace("៛", "").Trim();
            if (string.IsNullOrEmpty(s)) return 0m;
            return decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal result) ? result : 0m;
        }

        private void BtnDisDoc_Click(object? sender, EventArgs e)
        {
            if (dgvCart.Rows.Count == 0)
            {
                MessageBox.Show("Please add items to the cart before applying a document discount.",
                                "Cart Empty", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal subTotal = GetCartSubTotal();
            decimal subTotalUSD = subTotal >= 100m ? (subTotal / KhrPerUsd) : subTotal;

            using (FrmDiscount disModal = new FrmDiscount())
            {
                disModal.TotalAmountUSD = subTotalUSD;

                if (disModal.ShowDialog() == DialogResult.OK)
                {
                    decimal discUSD = disModal.DiscountUSD;
                    decimal pct = (disModal.TotalAmountUSD > 0) ? (discUSD / disModal.TotalAmountUSD * 100m) : 0m;

                    // Immediately refresh and update data in cart table rows
                    foreach (DataGridViewRow row in dgvCart.Rows)
                    {
                        decimal befDis = ParseDecimal(row.Cells["colBefDis"].Value);
                        decimal aftDis = Math.Round(befDis * (1m - (pct / 100m)), 2);
                        if (aftDis < 0) aftDis = 0;
                        row.Cells["colDisc"].Value = pct.ToString("0.##");
                        row.Cells["colAftDis"].Value = aftDis.ToString("N2");
                    }
                    dgvCart.Refresh();

                    currentDocDiscountKHR = subTotal >= 100m ? (discUSD * KhrPerUsd) : discUSD;
                    RecalculateTotals();

                    if (_currentOrderId > 0)
                    {
                        try { SaveOrderToDatabase("Open"); } catch { }
                    }
                }
            }
        }

        private long SaveOrderToDatabase(string status)
        {
            if (dgvCart.Rows.Count == 0) return 0;

            decimal subTotalKHR = GetCartSubTotal();
            decimal docDiscountKHR = currentDocDiscountKHR;
            decimal itemDiscountKHR = 0;
            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                decimal bef = ParseDecimal(row.Cells["colBefDis"].Value);
                decimal aft = ParseDecimal(row.Cells["colAftDis"].Value);
                if (bef > aft) itemDiscountKHR += (bef - aft);
            }
            decimal grandTotalKHR = subTotalKHR - itemDiscountKHR - docDiscountKHR;
            if (grandTotalKHR < 0) grandTotalKHR = 0;

            decimal docDiscountPct = (subTotalKHR > 0) ? (docDiscountKHR / subTotalKHR * 100m) : 0m;

            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                long orderId = _currentOrderId;
                if (orderId == 0)
                {
                    string insertOrderSql = @"
INSERT INTO dbo.SALE_ORDER 
(OrderNo, InvoiceNo, TableID, CustomerID, CreatedBy, PostingDate, Status, Note, SubTotal, ItemDiscountTotal, DocDiscountPercent, DocDiscountAmount, GrandTotal, ExchangeRate)
VALUES 
(@OrderNo, @InvoiceNo, @TableID, @CustomerID, @CreatedBy, SYSDATETIME(), @Status, @Note, @SubTotal, @ItemDiscountTotal, @DocDiscountPercent, @DocDiscountAmount, @GrandTotal, @ExchangeRate);
SELECT SCOPE_IDENTITY();";

                    using var cmdOrder = new SqlCommand(insertOrderSql, conn, trans);
                    cmdOrder.Parameters.AddWithValue("@OrderNo", _currentOrderNo);
                    cmdOrder.Parameters.AddWithValue("@InvoiceNo", string.IsNullOrEmpty(_invoiceNo) ? (object)DBNull.Value : _invoiceNo);
                    cmdOrder.Parameters.AddWithValue("@TableID", (object?)_tableId ?? DBNull.Value);
                    cmdOrder.Parameters.AddWithValue("@CustomerID", _currentCustomerId);
                    cmdOrder.Parameters.AddWithValue("@CreatedBy", UserSession.UserID > 0 ? UserSession.UserID : 1);
                    cmdOrder.Parameters.AddWithValue("@Status", status);
                    cmdOrder.Parameters.AddWithValue("@Note", string.IsNullOrEmpty(_orderNote) ? (object)DBNull.Value : _orderNote);
                    cmdOrder.Parameters.AddWithValue("@SubTotal", subTotalKHR);
                    cmdOrder.Parameters.AddWithValue("@ItemDiscountTotal", itemDiscountKHR);
                    cmdOrder.Parameters.AddWithValue("@DocDiscountPercent", docDiscountPct);
                    cmdOrder.Parameters.AddWithValue("@DocDiscountAmount", docDiscountKHR);
                    cmdOrder.Parameters.AddWithValue("@GrandTotal", grandTotalKHR);
                    cmdOrder.Parameters.AddWithValue("@ExchangeRate", KhrPerUsd);

                    orderId = Convert.ToInt64(cmdOrder.ExecuteScalar());
                    _currentOrderId = orderId;
                }
                else
                {
                    string updateOrderSql = @"
UPDATE dbo.SALE_ORDER SET 
    InvoiceNo = COALESCE(InvoiceNo, @InvoiceNo),
    TableID = @TableID, 
    CustomerID = @CustomerID, 
    Status = @Status, 
    Note = @Note,
    SubTotal = @SubTotal, 
    ItemDiscountTotal = @ItemDiscountTotal, 
    DocDiscountPercent = @DocDiscountPercent, 
    DocDiscountAmount = @DocDiscountAmount, 
    GrandTotal = @GrandTotal 
WHERE OrderID = @OrderID;";

                    using var cmdUpdate = new SqlCommand(updateOrderSql, conn, trans);
                    cmdUpdate.Parameters.AddWithValue("@InvoiceNo", string.IsNullOrEmpty(_invoiceNo) ? (object)DBNull.Value : _invoiceNo);
                    cmdUpdate.Parameters.AddWithValue("@TableID", (object?)_tableId ?? DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("@CustomerID", _currentCustomerId);
                    cmdUpdate.Parameters.AddWithValue("@Status", status);
                    cmdUpdate.Parameters.AddWithValue("@Note", string.IsNullOrEmpty(_orderNote) ? (object)DBNull.Value : _orderNote);
                    cmdUpdate.Parameters.AddWithValue("@SubTotal", subTotalKHR);
                    cmdUpdate.Parameters.AddWithValue("@ItemDiscountTotal", itemDiscountKHR);
                    cmdUpdate.Parameters.AddWithValue("@DocDiscountPercent", docDiscountPct);
                    cmdUpdate.Parameters.AddWithValue("@DocDiscountAmount", docDiscountKHR);
                    cmdUpdate.Parameters.AddWithValue("@GrandTotal", grandTotalKHR);
                    cmdUpdate.Parameters.AddWithValue("@OrderID", orderId);
                    cmdUpdate.ExecuteNonQuery();

                    using var cmdDel = new SqlCommand("DELETE FROM dbo.SALE_ORDER_ITEM WHERE OrderID = @OrderID", conn, trans);
                    cmdDel.Parameters.AddWithValue("@OrderID", orderId);
                    cmdDel.ExecuteNonQuery();
                }

                if (_tableId > 0 && status != "Paid" && status != "Void")
                {
                    using var cmdTableOcc = new SqlCommand("UPDATE dbo.DINING_TABLE SET Status = 'Occupied' WHERE TableID = @TID", conn, trans);
                    cmdTableOcc.Parameters.AddWithValue("@TID", _tableId);
                    cmdTableOcc.ExecuteNonQuery();
                }

                int lineNo = 1;
                foreach (DataGridViewRow row in dgvCart.Rows)
                {
                    string code = row.Cells["colCode"].Value?.ToString() ?? "";
                    string name = row.Cells["colName"].Value?.ToString() ?? "";
                    string uom = row.Cells["colUom"].Value?.ToString() ?? "Unit";
                    decimal qty = ParseDecimal(row.Cells["colQty"].Value);
                    decimal price = ParseDecimal(row.Cells["colPrice"].Value);
                    decimal befDis = ParseDecimal(row.Cells["colBefDis"].Value);
                    decimal discPct = ParseDecimal(row.Cells["colDisc"].Value);
                    decimal aftDis = ParseDecimal(row.Cells["colAftDis"].Value);
                    decimal discAmt = befDis - aftDis;

                    int itemId = 1;
                    using (var cmdFindItem = new SqlCommand("SELECT ItemID FROM dbo.ITEM WHERE ItemCode = @c", conn, trans))
                    {
                        cmdFindItem.Parameters.AddWithValue("@c", code);
                        var itmVal = cmdFindItem.ExecuteScalar();
                        if (itmVal != null && itmVal != DBNull.Value) itemId = Convert.ToInt32(itmVal);
                    }

                    string insertItemSql = @"
INSERT INTO dbo.SALE_ORDER_ITEM 
(OrderID, [LineNo], ItemID, ItemName, UomName, Qty, UnitPrice, TotalBeforeDis, DiscountPercent, DiscountAmount, TotalAfterDis, Note, SentToKitchenAt)
VALUES 
(@OrderID, @LineNo, @ItemID, @ItemName, @UomName, @Qty, @UnitPrice, @TotalBeforeDis, @DiscountPercent, @DiscountAmount, @TotalAfterDis, @Note, @Sent);";

                    using var cmdItem = new SqlCommand(insertItemSql, conn, trans);
                    cmdItem.Parameters.AddWithValue("@OrderID", orderId);
                    cmdItem.Parameters.AddWithValue("@LineNo", lineNo++);
                    cmdItem.Parameters.AddWithValue("@ItemID", itemId);
                    cmdItem.Parameters.AddWithValue("@ItemName", name);
                    cmdItem.Parameters.AddWithValue("@UomName", uom);
                    cmdItem.Parameters.AddWithValue("@Qty", qty);
                    cmdItem.Parameters.AddWithValue("@UnitPrice", price);
                    cmdItem.Parameters.AddWithValue("@TotalBeforeDis", befDis);
                    cmdItem.Parameters.AddWithValue("@DiscountPercent", discPct);
                    cmdItem.Parameters.AddWithValue("@DiscountAmount", discAmt);
                    cmdItem.Parameters.AddWithValue("@TotalAfterDis", aftDis);
                    cmdItem.Parameters.AddWithValue("@Note", DBNull.Value);
                    cmdItem.Parameters.AddWithValue("@Sent", status == "Sent" ? (object)DateTime.Now : DBNull.Value);

                    cmdItem.ExecuteNonQuery();
                }

                trans.Commit();
                return orderId;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        private void BtnSend_Click(object? sender, EventArgs e)
        {
            if (dgvCart.Rows.Count == 0)
            {
                MessageBox.Show("Cart is empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                SaveOrderToDatabase("Sent");
                lblTableOrder.Text = $"{_tableName} > {_invoiceNo} (#{_currentOrderNo}) (Sent)";
                MessageBox.Show($"Order {_invoiceNo} (#{_currentOrderNo}) for {_tableName} sent to kitchen successfully!",
                                "Kitchen Order Ticket", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving order: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBill_Click(object? sender, EventArgs e)
        {
            if (dgvCart.Rows.Count == 0)
            {
                MessageBox.Show("Cart is empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                SaveOrderToDatabase("Billed");
                lblTableOrder.Text = $"{_tableName} > {_invoiceNo} (#{_currentOrderNo}) (Billed)";
                // Open Receipt preview for printing/showing the guest bill
                Show80mmReceiptPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating bill: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPay_Click(object? sender, EventArgs e)
        {
            if (dgvCart.Rows.Count == 0)
            {
                MessageBox.Show("Please add items to the cart before proceeding to payment.",
                                "Cart Empty", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            long orderId;
            try
            {
                string statusToSave = "Open";
                if (_currentOrderId > 0)
                {
                    object? s = DbHelper.ExecuteScalar("SELECT Status FROM dbo.SALE_ORDER WHERE OrderID = @OID", new SqlParameter("@OID", _currentOrderId));
                    if (s != null && s != DBNull.Value && (s.ToString() == "Sent" || s.ToString() == "Billed"))
                    {
                        statusToSave = s.ToString()!;
                    }
                }
                orderId = SaveOrderToDatabase(statusToSave);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not prepare order: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (Form payModal = new Form())
            {
                payModal.Text = "Payment Processing";
                payModal.Size = new Size(1020, 560);
                payModal.StartPosition = FormStartPosition.CenterParent;
                payModal.FormBorderStyle = FormBorderStyle.FixedDialog;
                payModal.MaximizeBox = false;
                payModal.MinimizeBox = false;

                POSPayment paymentControl = new POSPayment
                {
                    Dock = DockStyle.Fill,
                    InvoiceNo = _invoiceNo
                };

                decimal subTotal = GetCartSubTotal();
                decimal totalGridDiscount = 0;
                foreach (DataGridViewRow row in dgvCart.Rows)
                {
                    decimal bef = ParseDecimal(row.Cells["colBefDis"].Value);
                    decimal aft = ParseDecimal(row.Cells["colAftDis"].Value);
                    if (bef > aft) totalGridDiscount += (bef - aft);
                }
                decimal grandTotal = subTotal - totalGridDiscount;
                if (grandTotal < 0) grandTotal = 0;

                decimal totalUSD = grandTotal >= 100m ? (grandTotal / KhrPerUsd) : grandTotal;
                decimal totalKHR = grandTotal < 100m ? (grandTotal * KhrPerUsd) : grandTotal;

                paymentControl.lblTotalUSD.Text = $"{totalUSD:N2} USD";
                paymentControl.lblTotalKHR.Text = $"{totalKHR:N2} KHR";

                paymentControl.CalculatePaymentTotals(null, EventArgs.Empty);
                payModal.Controls.Add(paymentControl);

                if (payModal.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using var conn = DbHelper.GetConnection();
                        conn.Open();
                        using var trans = conn.BeginTransaction();

                        decimal totalDueUSD = paymentControl.TotalDueUSD;
                        decimal totalReceivedUSD = paymentControl.TotalReceivedUSD;
                        decimal changeUSD = paymentControl.ChangeAmountUSD;

                        string insertPaymentSql = @"
INSERT INTO dbo.PAYMENT (OrderID, InvoiceNo, PaymentDate, TotalDue, TotalReceived, ChangeAmount, ChangeGiven, ExchangeRate, ReceivedBy)
VALUES (@OrderID, @InvoiceNo, @Date, @Due, @Rec, @Chg, @Chg, @Rate, @User);
SELECT SCOPE_IDENTITY();";

                        long paymentId;
                        using (var cmd = new SqlCommand(insertPaymentSql, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            cmd.Parameters.AddWithValue("@InvoiceNo", string.IsNullOrEmpty(_invoiceNo) ? (object)DBNull.Value : _invoiceNo);
                            cmd.Parameters.AddWithValue("@Date", paymentControl.PaymentDate);
                            cmd.Parameters.AddWithValue("@Due", totalDueUSD);
                            cmd.Parameters.AddWithValue("@Rec", totalReceivedUSD);
                            cmd.Parameters.AddWithValue("@Chg", changeUSD);
                            cmd.Parameters.AddWithValue("@Rate", KhrPerUsd);
                            cmd.Parameters.AddWithValue("@User", UserSession.UserID > 0 ? UserSession.UserID : 1);
                            paymentId = Convert.ToInt64(cmd.ExecuteScalar());
                        }

                        void AddPaymentLine(byte methodId, string currency, decimal amount)
                        {
                            if (amount <= 0) return;
                            decimal rate = currency == "USD" ? 4000m : 1m;
                            string lineSql = @"
INSERT INTO dbo.PAYMENT_DETAIL (PaymentID, MethodID, CurrencyCode, Amount, ExchangeRate)
VALUES (@PID, @MID, @Cur, @Amt, @Rate);";
                            using var cmd = new SqlCommand(lineSql, conn, trans);
                            cmd.Parameters.AddWithValue("@PID", paymentId);
                            cmd.Parameters.AddWithValue("@MID", methodId);
                            cmd.Parameters.AddWithValue("@Cur", currency);
                            cmd.Parameters.AddWithValue("@Amt", amount);
                            cmd.Parameters.AddWithValue("@Rate", rate);
                            cmd.ExecuteNonQuery();
                        }

                        AddPaymentLine(1, "USD", paymentControl.CashUSD);
                        AddPaymentLine(1, "KHR", paymentControl.CashKHR);
                        AddPaymentLine(2, "USD", paymentControl.AbaUSD);
                        AddPaymentLine(2, "KHR", paymentControl.AbaKHR);

                        using (var cmdPaid = new SqlCommand("UPDATE dbo.SALE_ORDER SET Status = 'Paid', InvoiceNo = COALESCE(InvoiceNo, @Inv) WHERE OrderID = @OID", conn, trans))
                        {
                            cmdPaid.Parameters.AddWithValue("@Inv", string.IsNullOrEmpty(_invoiceNo) ? (object)DBNull.Value : _invoiceNo);
                            cmdPaid.Parameters.AddWithValue("@OID", orderId);
                            cmdPaid.ExecuteNonQuery();
                        }

                        if (_tableId > 0)
                        {
                            using (var cmdTable = new SqlCommand("UPDATE dbo.DINING_TABLE SET Status = 'Available' WHERE TableID = @TID", conn, trans))
                            {
                                cmdTable.Parameters.AddWithValue("@TID", _tableId);
                                cmdTable.ExecuteNonQuery();
                            }
                        }

                        string stockItemsSql = @"
SELECT oi.OrderItemID, oi.ItemID, oi.Qty, i.IsStockItem, oi.UnitPrice
FROM dbo.SALE_ORDER_ITEM oi
JOIN dbo.ITEM i ON oi.ItemID = i.ItemID
WHERE oi.OrderID = @OID AND i.IsStockItem = 1;";

                        using (var cmdGetItems = new SqlCommand(stockItemsSql, conn, trans))
                        {
                            cmdGetItems.Parameters.AddWithValue("@OID", orderId);
                            using var rdr = cmdGetItems.ExecuteReader();
                            var stockMoves = new List<(int orderItemId, int itemId, decimal qty, decimal unitPrice)>();
                            while (rdr.Read())
                            {
                                stockMoves.Add((
                                    Convert.ToInt32(rdr[0]),
                                    Convert.ToInt32(rdr[1]),
                                    Convert.ToDecimal(rdr[2]),
                                    Convert.ToDecimal(rdr[4])
                                ));
                            }
                            rdr.Close();

                            foreach (var sm in stockMoves)
                            {
                                string insStock = @"
INSERT INTO dbo.STOCK_MOVEMENT (ItemID, MovementType, Qty, UnitCost, OrderItemID, Remark, CreatedBy, CreatedAt)
VALUES (@ItemID, 'Sale', @Qty, @Cost, @OrderItemID, 'POS Sale', @CreatedBy, SYSDATETIME());";
                                using var cmdSm = new SqlCommand(insStock, conn, trans);
                                cmdSm.Parameters.AddWithValue("@ItemID", sm.itemId);
                                cmdSm.Parameters.AddWithValue("@Qty", sm.qty);
                                cmdSm.Parameters.AddWithValue("@Cost", sm.unitPrice);
                                cmdSm.Parameters.AddWithValue("@OrderItemID", sm.orderItemId);
                                cmdSm.Parameters.AddWithValue("@CreatedBy", UserSession.UserID > 0 ? UserSession.UserID : 1);
                                cmdSm.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();

                        MessageBox.Show($"Payment for Invoice {_invoiceNo} (#{_currentOrderNo}) completed successfully!\nChange: {changeUSD:N2} USD ({(changeUSD * KhrPerUsd):N0} KHR)",
                                        "Payment Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Show receipt preview for paid order
                        Show80mmReceiptPreview();

                        dgvCart.Rows.Clear();
                        currentDocDiscountKHR = 0.00m;
                        _orderNote = "";
                        btnNote.Text = "Note";
                        _currentOrderId = 0;
                        _currentOrderNo = GenerateOrderNo();
                        _invoiceNo = GenerateInvoiceNo();
                        lblTableOrder.Text = $"{_tableName} > {_invoiceNo} (#{_currentOrderNo})";
                        RecalculateTotals();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error recording payment: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void InitializeComponentByCode()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(238, 238, 238);

            // ---------------- Top Toolbar ----------------
            pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.FromArgb(243, 246, 250),
                Padding = new Padding(10, 6, 10, 0)
            };

            Panel receiptHost = new Panel { Dock = DockStyle.Left, Width = 80 };
            btnReceipt = new Guna2Button
            {
                Text = "🖨",
                Size = new Size(40, 40),
                Location = new Point(20, 2),
                FillColor = Color.FromArgb(224, 0, 160),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 14F),
                BorderRadius = 8,
                Cursor = Cursors.Hand
            };
            Label lblReceipt = new Label
            {
                Text = "Receipt",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Bottom,
                Height = 22,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            lblReceipt.Click += (s, e) => OpenReceiptList();
            receiptHost.Cursor = Cursors.Hand;
            receiptHost.Click += (s, e) => OpenReceiptList();
            receiptHost.Controls.Add(btnReceipt);
            receiptHost.Controls.Add(lblReceipt);
            pnlToolbar.Controls.Add(receiptHost);

            // ---------------- Green Header ----------------
            pnlGreenHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = HeaderGreen
            };

            TableLayoutPanel tblHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            btnBackTable = CreateHeaderButton("◀ Table", 72, new Font("Segoe UI", 10.5F));
            btnBackTable.Margin = new Padding(3, 5, 3, 5);
            tblHeader.Controls.Add(btnBackTable, 0, 0);

            FlowLayoutPanel flowCenter = new FlowLayoutPanel
            {
                AutoSize = true,
                Anchor = AnchorStyles.None,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            lblTableOrder = CreateHeaderBadge("Table-1 > #Order-1");
            lblCountRows = CreateHeaderBadge("Count Rows :0");
            lblCountQtys = CreateHeaderBadge("Count Qtys :0");
            flowCenter.Controls.Add(lblTableOrder);
            flowCenter.Controls.Add(lblCountRows);
            flowCenter.Controls.Add(lblCountQtys);
            tblHeader.Controls.Add(flowCenter, 1, 0);

            FlowLayoutPanel flowRight = new FlowLayoutPanel
            {
                AutoSize = true,
                Anchor = AnchorStyles.None,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            btnNote = CreateHeaderButton("📄", 30, new Font("Segoe UI Emoji", 11F));
            btnCustomerTag = CreateHeaderButton("👤", 40, new Font("Segoe UI Emoji", 11F));
            btnReceiptList = CreateHeaderButton("🧾", 30, new Font("Segoe UI Emoji", 11F));
            flowRight.Controls.Add(btnNote);
            flowRight.Controls.Add(btnCustomerTag);
            flowRight.Controls.Add(btnReceiptList);
            tblHeader.Controls.Add(flowRight, 2, 0);

            pnlGreenHeader.Controls.Add(tblHeader);

            // ---------------- Customer Bar ----------------
            pnlCustomerBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = BarBlueGray
            };

            TableLayoutPanel tblCustomer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            tblCustomer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230F));
            tblCustomer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblCustomer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblCustomer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Panel customerInfo = new Panel { Dock = DockStyle.Fill };
            Guna2Button avatar = new Guna2Button
            {
                Text = "👤",
                Size = new Size(38, 38),
                Location = new Point(2, 4),
                FillColor = Color.FromArgb(205, 215, 228),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 14F),
                BorderRadius = 4,
                Enabled = false,
                DisabledState = { FillColor = Color.FromArgb(205, 215, 228), ForeColor = Color.White }
            };
            lblCustomerName = new Label
            {
                Text = "General Customer",
                Font = new Font("Segoe UI", 10.5F),
                AutoSize = true,
                Location = new Point(46, 13),
                BackColor = Color.Transparent
            };
            customerInfo.Controls.Add(avatar);
            customerInfo.Controls.Add(lblCustomerName);
            tblCustomer.Controls.Add(customerInfo, 0, 0);

            txtSearchCustomer = new Guna2TextBox
            {
                PlaceholderText = "🔍 Search customer....",
                Dock = DockStyle.Fill,
                Margin = new Padding(6),
                BorderRadius = 16,
                BorderThickness = 0,
                FillColor = Color.White,
                Font = new Font("Segoe UI", 9.5F)
            };

            txtReadBarcode = new Guna2TextBox
            {
                PlaceholderText = "⌨ Read barcode ...",
                Dock = DockStyle.Fill,
                Margin = new Padding(6),
                BorderRadius = 16,
                BorderThickness = 0,
                FillColor = Color.White,
                Font = new Font("Segoe UI", 9.5F)
            };

            tblCustomer.Controls.Add(txtSearchCustomer, 1, 0);
            tblCustomer.Controls.Add(txtReadBarcode, 2, 0);
            pnlCustomerBar.Controls.Add(tblCustomer);

            // ---------------- Cart Grid ----------------
            dgvCart = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(238, 238, 238),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                AllowUserToAddRows = false,
                AllowUserToResizeColumns = true,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                RowTemplate = { Height = 44 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                GridColor = Color.FromArgb(210, 210, 210),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 42
            };

            dgvCart.ThemeStyle.BackColor = Color.FromArgb(238, 238, 238);
            dgvCart.ThemeStyle.GridColor = Color.FromArgb(210, 210, 210);
            dgvCart.ThemeStyle.HeaderStyle.BackColor = GridHeader;
            dgvCart.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvCart.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvCart.ThemeStyle.HeaderStyle.Height = 42;

            dgvCart.ThemeStyle.RowsStyle.BackColor = Color.FromArgb(240, 240, 240);
            dgvCart.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            dgvCart.ThemeStyle.RowsStyle.ForeColor = Color.FromArgb(40, 40, 40);
            dgvCart.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(225, 233, 242);
            dgvCart.ThemeStyle.RowsStyle.SelectionForeColor = Color.Black;
            dgvCart.ThemeStyle.RowsStyle.Height = 44;

            dgvCart.CellValueChanged += DgvCart_CellValueChanged;
            dgvCart.CellPainting += DgvCart_CellPainting;
            dgvCart.CellMouseClick += DgvCart_CellMouseClick;

            // ---------------- Bottom Summary & Action Buttons ----------------
            pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 170,
                BackColor = Color.FromArgb(238, 238, 238)
            };

            tblSummary = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 92,
                ColumnCount = 4,
                RowCount = 2,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            for (int i = 0; i < 4; i++)
                tblSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            tblSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            tblSummary.Controls.Add(CreateHeaderLabel("Subtotal"), 0, 0);
            tblSummary.Controls.Add(CreateHeaderLabel("Dis. Item"), 1, 0);
            tblSummary.Controls.Add(CreateHeaderLabel("Dis. Doc"), 2, 0);
            tblSummary.Controls.Add(CreateHeaderLabel("Grand Total"), 3, 0);

            lbSubTotalUSD = CreateValueLabel("KHR 0.00");
            lbDisItemUSD = CreateValueLabel("KHR 0.00");
            lbDisDocUSD = CreateValueLabel("KHR 0.00");
            lbGrandTotalUSD = CreateValueLabel("KHR 0.00");

            tblSummary.Controls.Add(lbSubTotalUSD, 0, 1);
            tblSummary.Controls.Add(lbDisItemUSD, 1, 1);
            tblSummary.Controls.Add(lbDisDocUSD, 2, 1);
            tblSummary.Controls.Add(lbGrandTotalUSD, 3, 1);

            tblButtons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(2)
            };

            for (int i = 0; i < 4; i++)
                tblButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tblButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            btnDisDoc = CreateActionButton(null, "%", "Discount", Color.FromArgb(239, 64, 64), out Panel hostDisDoc);
            btnSend = CreateActionButton(CreateSendIcon(28), "✈", "Send", Color.FromArgb(100, 175, 230), out Panel hostSend);
            btnBill = CreateActionButton(CreateBillIcon(28), "🖨", "Bill", Color.FromArgb(80, 160, 125), out Panel hostBill);
            btnPay = CreateActionButton(CreatePayIcon(28), "$", "Pay", Color.FromArgb(160, 80, 130), out Panel hostPay);

            tblButtons.Controls.Add(hostDisDoc, 0, 0);
            tblButtons.Controls.Add(hostSend, 1, 0);
            tblButtons.Controls.Add(hostBill, 2, 0);
            tblButtons.Controls.Add(hostPay, 3, 0);

            // Fill control first, then the Top docked one
            pnlBottom.Controls.Add(tblButtons);
            pnlBottom.Controls.Add(tblSummary);

            // Fill control (grid) first, then the edge-docked panels
            this.Controls.Add(dgvCart);
            this.Controls.Add(pnlCustomerBar);
            this.Controls.Add(pnlGreenHeader);
            this.Controls.Add(pnlToolbar);
            this.Controls.Add(pnlBottom);
        }

        private void SetupCartGridColumns()
        {
            dgvCart.Columns.Clear();

            var center = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter };

            var colNo = new DataGridViewTextBoxColumn { Name = "colNo", HeaderText = "No", Width = 45 };
            var colCode = new DataGridViewTextBoxColumn { Name = "colCode", HeaderText = "Code", Width = 70 };

            var colName = new DataGridViewTextBoxColumn
            {
                Name = "colName",
                HeaderText = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 140
            };
            colName.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = DbHelper.GetKhmerFont(10F, FontStyle.Bold)
            };

            var colQty = new DataGridViewTextBoxColumn { Name = "colQty", HeaderText = "Qty", Width = 70 };
            colQty.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Consolas", 10.5F, FontStyle.Bold)
            };

            DataGridViewComboBoxColumn comboUom = new DataGridViewComboBoxColumn
            {
                Name = "colUom",
                HeaderText = "UoM",
                FlatStyle = FlatStyle.Flat,
                Width = 110,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
            };
            comboUom.Items.AddRange("Unit", "Cup", "Glass", "Pack");

            var colPrice = new DataGridViewTextBoxColumn { Name = "colPrice", HeaderText = "Price", Width = 100 };
            var colBefDis = new DataGridViewTextBoxColumn { Name = "colBefDis", HeaderText = "Before Dis.", Width = 110 };
            var colDisc = new DataGridViewTextBoxColumn { Name = "colDisc", HeaderText = "Disc.(%)", Width = 90 };
            var colAftDis = new DataGridViewTextBoxColumn { Name = "colAftDis", HeaderText = "After Disc.", Width = 110 };

            colPrice.DefaultCellStyle = center;
            colBefDis.DefaultCellStyle = center;
            colDisc.DefaultCellStyle = center;
            colAftDis.DefaultCellStyle = center;

            DataGridViewTextBoxColumn colActions = new DataGridViewTextBoxColumn
            {
                Name = "colActions",
                HeaderText = "",
                ReadOnly = true,
                Width = ActionStartX * 2 + ActionSlotWidth * ActionSlotCount
            };

            dgvCart.Columns.AddRange(new DataGridViewColumn[] {
                colNo, colCode, colName, colQty, comboUom, colPrice, colBefDis, colDisc, colAftDis, colActions
            });

            foreach (DataGridViewColumn c in dgvCart.Columns)
                c.SortMode = DataGridViewColumnSortMode.NotSortable;

            dgvCart.CellClick -= DgvCart_CellClick;
            dgvCart.CellClick += DgvCart_CellClick;
        }

        private void DgvCart_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string colName = dgvCart.Columns[e.ColumnIndex].Name;
            if (colName == "colDisc" || colName == "colAftDis")
            {
                var row = dgvCart.Rows[e.RowIndex];
                decimal befDis = ParseDecimal(row.Cells["colBefDis"].Value);
                if (befDis <= 0) return;

                decimal befDisUSD = befDis >= 100m ? (befDis / KhrPerUsd) : befDis;

                using (FrmDiscount disModal = new FrmDiscount())
                {
                    disModal.Text = "Discount Item";
                    disModal.TotalAmountUSD = befDisUSD;

                    if (disModal.ShowDialog() == DialogResult.OK)
                    {
                        decimal discUSD = disModal.DiscountUSD;
                        decimal pct = (disModal.TotalAmountUSD > 0) ? (discUSD / disModal.TotalAmountUSD * 100m) : 0m;
                        decimal aftDis = Math.Round(befDis * (1m - (pct / 100m)), 2);
                        if (aftDis < 0) aftDis = 0;

                        row.Cells["colDisc"].Value = pct.ToString("0.##");
                        row.Cells["colAftDis"].Value = aftDis.ToString("N2");

                        dgvCart.Refresh();
                        RecalculateTotals();

                        if (_currentOrderId > 0)
                        {
                            try { SaveOrderToDatabase("Open"); } catch { }
                        }
                    }
                }
            }
        }

        public void AddProductToCart(string code, string name, decimal price)
        {
            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                if (row.Cells["colCode"].Value?.ToString() == code)
                {
                    decimal currentQty = ParseDecimal(row.Cells["colQty"].Value);
                    currentQty += 1.00m;
                    row.Cells["colQty"].Value = currentQty.ToString("N2");

                    decimal befDis = currentQty * price;
                    row.Cells["colBefDis"].Value = befDis.ToString("N2");
                    row.Cells["colAftDis"].Value = befDis.ToString("N2");

                    RecalculateTotals();
                    return;
                }
            }

            int no = dgvCart.Rows.Count + 1;
            dgvCart.Rows.Add(
                no,
                code,
                name,
                "1.00",
                "Unit",
                price.ToString("N2"),
                price.ToString("N2"),
                "0.00",
                price.ToString("N2"),
                ""
            );

            RecalculateTotals();
        }

        private void DgvCart_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 ||
                dgvCart.Columns[e.ColumnIndex].Name != "colActions")
                return;

            e.PaintBackground(e.CellBounds, true);

            Graphics g = e.Graphics!;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Font iconFont = new Font("Segoe UI Symbol", 12F, FontStyle.Regular))
            {
                for (int slot = 0; slot < ActionSlotCount; slot++)
                {
                    Rectangle r = new Rectangle(
                        e.CellBounds.Left + ActionStartX + slot * ActionSlotWidth,
                        e.CellBounds.Top,
                        ActionSlotWidth,
                        e.CellBounds.Height);

                    switch (slot)
                    {
                        case 0: // Note / Comment
                            DrawIcon(g, "💬", iconFont, r, Color.FromArgb(40, 50, 180));
                            break;

                        case 1: // Minus
                            DrawCircleButton(g, r, Color.FromArgb(52, 152, 219), false);
                            break;

                        case 2: // Plus
                            DrawCircleButton(g, r, Color.FromArgb(242, 133, 0), true);
                            break;

                        case 3: // Keypad / Numpad
                            DrawIcon(g, "▦", iconFont, r, Color.FromArgb(80, 150, 220));
                            break;

                        case 4: // Delete
                            DrawIcon(g, "🗑", iconFont, r, Color.FromArgb(230, 50, 50));
                            break;
                    }
                }
            }

            e.Handled = true;
        }

        private static void DrawCircleButton(Graphics g, Rectangle r, Color color, bool plus)
        {
            int d = 20;
            int cx = r.Left + r.Width / 2;
            int cy = r.Top + r.Height / 2;

            using (var brush = new SolidBrush(color))
                g.FillEllipse(brush, cx - d / 2, cy - d / 2, d, d);

            using (var pen = new Pen(Color.White, 2.5f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, cx - 5, cy, cx + 5, cy);
                if (plus)
                    g.DrawLine(pen, cx, cy - 5, cx, cy + 5);
            }
        }

        private static void DrawIcon(Graphics g, string text, Font font, Rectangle bounds, Color color)
        {
            TextRenderer.DrawText(g, text, font, bounds, color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }

        private void DgvCart_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 ||
                dgvCart.Columns[e.ColumnIndex].Name != "colActions")
                return;

            if (e.X < ActionStartX) return;
            int slot = (e.X - ActionStartX) / ActionSlotWidth;

            var row = dgvCart.Rows[e.RowIndex];

            switch (slot)
            {
                case 0: // Custom Note/Instructions
                    using (Form noteForm = new Form())
                    {
                        noteForm.Text = "Item Remarks/Note";
                        noteForm.Size = new Size(350, 200);
                        noteForm.StartPosition = FormStartPosition.CenterParent;
                        TextBox txtNote = new TextBox { Dock = DockStyle.Fill, Multiline = true };
                        Button btnSave = new Button { Text = "Save", Dock = DockStyle.Bottom };
                        btnSave.Click += (s, args) => { noteForm.DialogResult = DialogResult.OK; };
                        noteForm.Controls.Add(txtNote);
                        noteForm.Controls.Add(btnSave);
                        if (noteForm.ShowDialog() == DialogResult.OK)
                        {
                            row.Cells["colName"].Value = $"{row.Cells["colName"].Value}\n ({txtNote.Text})";
                        }
                    }
                    break;

                case 1: // -1 Qty
                case 2: // +1 Qty
                    {
                        decimal currentQty = ParseDecimal(row.Cells["colQty"].Value);
                        decimal price = ParseDecimal(row.Cells["colPrice"].Value);
                        decimal discPct = ParseDecimal(row.Cells["colDisc"].Value);
                        currentQty += (slot == 2) ? 1.00m : -1.00m;
                        if (currentQty < 1.00m) currentQty = 1.00m;

                        row.Cells["colQty"].Value = currentQty.ToString("N2");

                        decimal befDis = currentQty * price;
                        decimal aftDis = Math.Round(befDis * (1m - (discPct / 100m)), 2);
                        row.Cells["colBefDis"].Value = befDis.ToString("N2");
                        row.Cells["colAftDis"].Value = aftDis.ToString("N2");

                        RecalculateTotals();
                    }
                    break;

                case 3: // Custom Numpad Keypad for Manual Qty Input
                    using (Form qtyModal = new Form())
                    {
                        qtyModal.Text = "Enter Quantity";
                        qtyModal.Size = new Size(250, 150);
                        qtyModal.StartPosition = FormStartPosition.CenterParent;
                        TextBox txtQtyInput = new TextBox { Dock = DockStyle.Top, Text = row.Cells["colQty"].Value?.ToString() };
                        Button btnConfirm = new Button { Text = "OK", Dock = DockStyle.Bottom };
                        btnConfirm.Click += (s, args) => { qtyModal.DialogResult = DialogResult.OK; };
                        qtyModal.Controls.Add(txtQtyInput);
                        qtyModal.Controls.Add(btnConfirm);

                        if (qtyModal.ShowDialog() == DialogResult.OK)
                        {
                            decimal newQty = ParseDecimal(txtQtyInput.Text);
                            if (newQty > 0)
                            {
                                row.Cells["colQty"].Value = newQty.ToString("N2");
                                decimal unitPrice = ParseDecimal(row.Cells["colPrice"].Value);
                                decimal discPct = ParseDecimal(row.Cells["colDisc"].Value);
                                decimal befDis = newQty * unitPrice;
                                decimal aftDis = Math.Round(befDis * (1m - (discPct / 100m)), 2);
                                row.Cells["colBefDis"].Value = befDis.ToString("N2");
                                row.Cells["colAftDis"].Value = aftDis.ToString("N2");
                                RecalculateTotals();
                            }
                        }
                    }
                    break;

                case 4: // Delete Item
                    dgvCart.Rows.RemoveAt(e.RowIndex);
                    ReorderRowNumbers();
                    RecalculateTotals();
                    break;
            }
        }

        private void DgvCart_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvCart.Columns[e.ColumnIndex].Name == "colQty")
            {
                var row = dgvCart.Rows[e.RowIndex];
                decimal qty = ParseDecimal(row.Cells["colQty"].Value);
                decimal price = ParseDecimal(row.Cells["colPrice"].Value);
                decimal discPct = ParseDecimal(row.Cells["colDisc"].Value);
                if (qty > 0)
                {
                    decimal befDis = qty * price;
                    decimal aftDis = Math.Round(befDis * (1m - (discPct / 100m)), 2);
                    row.Cells["colBefDis"].Value = befDis.ToString("N2");
                    row.Cells["colAftDis"].Value = aftDis.ToString("N2");
                    RecalculateTotals();
                }
            }
        }

        private void ReorderRowNumbers()
        {
            for (int i = 0; i < dgvCart.Rows.Count; i++)
            {
                dgvCart.Rows[i].Cells["colNo"].Value = i + 1;
            }
        }

        private decimal GetCartSubTotal()
        {
            decimal subTotal = 0;
            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                subTotal += ParseDecimal(row.Cells["colBefDis"].Value);
            }
            return subTotal;
        }

        private void RecalculateTotals()
        {
            int totalRows = dgvCart.Rows.Count;
            decimal totalQty = 0;
            decimal subTotal = 0;
            decimal totalGridDiscount = 0;

            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                decimal q = ParseDecimal(row.Cells["colQty"].Value);
                decimal bef = ParseDecimal(row.Cells["colBefDis"].Value);
                decimal aft = ParseDecimal(row.Cells["colAftDis"].Value);

                totalQty += q;
                subTotal += bef;

                if (bef > aft)
                    totalGridDiscount += (bef - aft);
            }

            decimal docDiscount = currentDocDiscountKHR;
            decimal itemDiscount = 0m;

            if (docDiscount > 0)
            {
                itemDiscount = Math.Max(0m, totalGridDiscount - docDiscount);
            }
            else
            {
                itemDiscount = totalGridDiscount;
            }

            decimal grandTotal = subTotal - itemDiscount - docDiscount;
            if (grandTotal < 0) grandTotal = 0;

            lblCountRows.Text = $"Count Rows :{totalRows}";
            lblCountQtys.Text = $"Count Qtys :{totalQty:N0}";

            lbSubTotalUSD.Text = $"KHR {subTotal:N2}";
            lbDisItemUSD.Text = $"KHR {itemDiscount:N2}";
            lbDisDocUSD.Text = $"KHR {docDiscount:N2}";
            lbGrandTotalUSD.Text = $"KHR {grandTotal:N2}";
        }

        private Guna2Button CreateHeaderButton(string text, int width, Font font)
        {
            return new Guna2Button
            {
                Text = text,
                Size = new Size(width, 34),
                FillColor = BadgeColor,
                ForeColor = Color.White,
                Font = font,
                BorderColor = BadgeBorder,
                BorderThickness = 1,
                BorderRadius = 4,
                Margin = new Padding(3, 0, 3, 0),
                Cursor = Cursors.Hand
            };
        }

        private Label CreateHeaderBadge(string text)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = BadgeColor,
                AutoSize = true,
                Padding = new Padding(6),
                Margin = new Padding(3, 0, 3, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lbl.Paint += (s, e) =>
            {
                using (var pen = new Pen(BadgeBorder, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, lbl.Width - 1, lbl.Height - 1);
            };

            return lbl;
        }

        private Label CreateHeaderLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                BackColor = BarBlueGray,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private Label CreateValueLabel(string defaultText)
        {
            return new Label
            {
                Text = defaultText,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                BackColor = Color.FromArgb(240, 240, 240),
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private static Image CreateSendIcon(int size = 28)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(Color.White);
                PointF[] pts = new PointF[]
                {
                    new PointF(size * 0.12f, size * 0.15f),
                    new PointF(size * 0.90f, size * 0.50f),
                    new PointF(size * 0.12f, size * 0.85f),
                    new PointF(size * 0.38f, size * 0.50f)
                };
                g.FillPolygon(brush, pts);
            }
            return bmp;
        }

        private static Image CreateBillIcon(int size = 28)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(Color.White);
                using var bgBrush = new SolidBrush(Color.FromArgb(80, 160, 125));

                // Printer top paper
                g.FillRectangle(brush, size * 0.28f, size * 0.08f, size * 0.44f, size * 0.24f);
                // Printer main body
                g.FillRectangle(brush, size * 0.10f, size * 0.30f, size * 0.80f, size * 0.38f);
                // Paper slot
                g.FillRectangle(bgBrush, size * 0.18f, size * 0.52f, size * 0.64f, size * 0.08f);
                // Bottom paper coming out
                g.FillRectangle(brush, size * 0.24f, size * 0.58f, size * 0.52f, size * 0.34f);
                // Lines on bottom paper
                using var pen = new Pen(bgBrush, 1.5f);
                g.DrawLine(pen, size * 0.32f, size * 0.70f, size * 0.68f, size * 0.70f);
                g.DrawLine(pen, size * 0.32f, size * 0.80f, size * 0.68f, size * 0.80f);
            }
            return bmp;
        }

        private static Image CreatePayIcon(int size = 28)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(Color.White, 2f);
                using var brush = new SolidBrush(Color.White);

                // Card body outline
                g.DrawRectangle(pen, size * 0.08f, size * 0.22f, size * 0.84f, size * 0.56f);
                // Magnetic strip
                g.FillRectangle(brush, size * 0.08f, size * 0.32f, size * 0.84f, size * 0.14f);
                // Chip
                g.FillRectangle(brush, size * 0.20f, size * 0.54f, size * 0.22f, size * 0.16f);
            }
            return bmp;
        }

        private Guna2Button CreateActionButton(Image? iconImg, string textFallback, string caption, Color color, out Panel host)
        {
            host = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                BackColor = Color.FromArgb(200, 200, 200),
                Cursor = Cursors.Hand
            };

            var btn = new Guna2Button
            {
                Dock = DockStyle.Fill,
                FillColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                BorderRadius = 4,
                Cursor = Cursors.Hand
            };

            if (iconImg != null)
            {
                btn.Image = iconImg;
                btn.ImageSize = new Size(26, 26);
                btn.Text = "";
            }
            else
            {
                btn.Text = textFallback;
            }

            var lbl = new Label
            {
                Text = caption,
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.FromArgb(40, 40, 40),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };

            // Clicking on the label or host panel triggers the button click
            lbl.Click += (s, e) => btn.PerformClick();
            host.Click += (s, e) => btn.PerformClick();

            // Fill control first, then the Bottom docked label
            host.Controls.Add(btn);
            host.Controls.Add(lbl);

            return btn;
        }
    }
}
