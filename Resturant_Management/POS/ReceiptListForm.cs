using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using FontAwesome.Sharp;
using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.POS
{
    /// <summary>List of paid receipts with date filter, search and per-row reprint.</summary>
    public class ReceiptListForm : Form
    {
        private static readonly Color HeaderBar = Color.FromArgb(124, 121, 143);
        private static readonly Color PageBack = Color.FromArgb(240, 244, 248);
        private static readonly Color AccentBlue = Color.FromArgb(99, 179, 237);
        private static readonly Color GridHeader = Color.FromArgb(88, 112, 140);
        private static readonly Color PrintHeader = Color.FromArgb(0, 120, 215);
        private static readonly Color SelectedRow = Color.FromArgb(214, 232, 248);
        private static readonly Color TextDark = Color.FromArgb(40, 48, 60);

        private Guna2DateTimePicker dtFrom = null!;
        private Guna2DateTimePicker dtTo = null!;
        private Guna2TextBox txtSearch = null!;
        private DataGridView grid = null!;
        private Image? printIcon;

        private readonly List<ReceiptRow> allRows = new List<ReceiptRow>();

        private sealed class ReceiptRow
        {
            public long OrderId;
            public string ReceiptNo = "";
            public string Cashier = "";
            public string Name = "";
            public DateTime PaymentDate;
            public string Table = "";
            public decimal Amount;
            public string PaymentType = "";
        }

        public ReceiptListForm()
        {
            Text = "Receipt List";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1180, 760);
            MinimumSize = new Size(900, 560);
            BackColor = PageBack;
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

            BuildUi();
            Load += (s, e) => LoadReceipts();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Fit inside the owner window when it is smaller than the default size
            if (Owner != null)
            {
                Rectangle area = Screen.FromControl(Owner).WorkingArea;
                Width = Math.Min(Width, area.Width - 40);
                Height = Math.Min(Height, area.Height - 40);
                Left = Math.Max(area.Left, Owner.Left + (Owner.Width - Width) / 2);
                Top = Math.Max(area.Top, Owner.Top + (Owner.Height - Height) / 2);
            }
        }

        private void BuildUi()
        {
            // ---------- Title bar ----------
            Panel pnlTitle = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = HeaderBar };
            Label lblTitle = new Label
            {
                Text = "Receipt",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold)
            };
            Label btnClose = new Label
            {
                Text = "✕",
                Dock = DockStyle.Right,
                Width = 56,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => Close();
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(200, 70, 70);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;
            // Back to the POS screen (same as closing)
            Guna2Button btnBack = new Guna2Button
            {
                Text = "◀  Back",
                Dock = DockStyle.Fill,
                BorderRadius = 6,
                FillColor = Color.FromArgb(68, 158, 122),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBack.Click += (s, e) => Close();

            // Equal-width side areas keep the title centered
            Panel pnlLeft = new Panel { Dock = DockStyle.Left, Width = 140, Padding = new Padding(12, 9, 18, 9), BackColor = HeaderBar };
            pnlLeft.Controls.Add(btnBack);
            Panel pnlRight = new Panel { Dock = DockStyle.Right, Width = 140, BackColor = HeaderBar };
            pnlRight.Controls.Add(btnClose);

            pnlTitle.Controls.Add(lblTitle);
            pnlTitle.Controls.Add(pnlLeft);
            pnlTitle.Controls.Add(pnlRight);

            // Drag the borderless window by its title bar
            MouseEventHandler drag = (s, e) =>
            {
                if (e.Button != MouseButtons.Left || !TopLevel) return;
                NativeDrag();
            };
            pnlTitle.MouseDown += drag;
            lblTitle.MouseDown += drag;

            // ---------- Body ----------
            Panel pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 20, 20, 16), BackColor = PageBack };

            // Filter card
            Guna2Panel cardFilter = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 104,
                BorderRadius = 12,
                FillColor = Color.White,
                BorderColor = Color.FromArgb(225, 230, 236),
                BorderThickness = 1,
                BackColor = PageBack
            };

            Label lblFrom = MakeFieldLabel("Date From", new Point(24, 14));
            dtFrom = MakeDatePicker(new Point(20, 42));
            Label lblTo = MakeFieldLabel("Date To", new Point(246, 14));
            dtTo = MakeDatePicker(new Point(242, 42));

            Guna2Button btnFilter = new Guna2Button
            {
                Text = "Filter",
                Location = new Point(470, 42),
                Size = new Size(110, 44),
                BorderRadius = 8,
                FillColor = AccentBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFilter.Click += (s, e) => LoadReceipts();

            txtSearch = new Guna2TextBox
            {
                PlaceholderText = "Search",
                PlaceholderForeColor = Color.FromArgb(150, 160, 175),
                Size = new Size(300, 44),
                BorderRadius = 18,
                FillColor = Color.FromArgb(240, 244, 250),
                BorderColor = Color.FromArgb(225, 230, 236),
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextDark,
                IconLeft = MakeIcon(IconChar.Search, Color.FromArgb(150, 160, 175), 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            txtSearch.TextChanged += (s, e) => ApplySearch();

            cardFilter.Controls.AddRange(new Control[] { lblFrom, dtFrom, lblTo, dtTo, btnFilter, txtSearch });
            cardFilter.Resize += (s, e) => txtSearch.Location = new Point(cardFilter.Width - txtSearch.Width - 20, 42);

            Panel gap = new Panel { Dock = DockStyle.Top, Height = 20, BackColor = PageBack };

            // List card
            Guna2Panel cardList = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 12,
                FillColor = Color.White,
                BorderColor = Color.FromArgb(225, 230, 236),
                BorderThickness = 1,
                BackColor = PageBack,
                Padding = new Padding(16, 74, 16, 16)
            };

            Guna2Button lblList = new Guna2Button
            {
                Text = "List of Receipts",
                Size = new Size(194, 42),
                BorderRadius = 8,
                FillColor = AccentBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Enabled = true,
                Cursor = Cursors.Default
            };
            lblList.DisabledState.FillColor = AccentBlue;
            lblList.DisabledState.ForeColor = Color.White;
            cardList.Resize += (s, e) => lblList.Location = new Point((cardList.Width - lblList.Width) / 2, 16);

            grid = BuildGrid();
            cardList.Controls.Add(grid);
            cardList.Controls.Add(lblList);

            pnlBody.Controls.Add(cardList);
            pnlBody.Controls.Add(gap);
            pnlBody.Controls.Add(cardFilter);

            Controls.Add(pnlBody);
            Controls.Add(pnlTitle);

            // Thin outline so the borderless window stands out from the POS behind it
            Paint += (s, e) =>
            {
                if (!TopLevel) return;
                using var pen = new Pen(Color.FromArgb(180, 180, 190));
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            };
        }

        private DataGridView BuildGrid()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 38,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(225, 230, 236)
            };
            dgv.RowTemplate.Height = 36;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = GridHeader;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = GridHeader;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            dgv.DefaultCellStyle.ForeColor = TextDark;
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = SelectedRow;
            dgv.DefaultCellStyle.SelectionForeColor = TextDark;
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colOrderId", Visible = false });
            AddCol(dgv, "colReceiptNo", "Receipt №", 110);
            AddCol(dgv, "colCashier", "Cashier", 100);
            AddCol(dgv, "colName", "Name", 140);
            AddCol(dgv, "colDate", "Date", 100);
            AddCol(dgv, "colTime", "Time", 80);
            AddCol(dgv, "colTable", "Table", 100);
            var amount = AddCol(dgv, "colAmount", "Amount", 110);
            amount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            amount.DefaultCellStyle.Padding = new Padding(0, 0, 8, 0);
            AddCol(dgv, "colPaymentType", "Payment Type", 110);
            var print = AddCol(dgv, "colPrint", "Print", 70);
            print.HeaderCell.Style.BackColor = PrintHeader;
            print.HeaderCell.Style.SelectionBackColor = PrintHeader;

            printIcon = MakeIcon(IconChar.Print, Color.FromArgb(20, 30, 45), 18);

            // Draw a rounded blue button with a printer icon in the Print column
            dgv.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != dgv.Columns["colPrint"].Index || e.Graphics == null) return;
                e.PaintBackground(e.CellBounds, true);
                Rectangle r = e.CellBounds;
                var btn = new Rectangle(r.X + (r.Width - 50) / 2, r.Y + (r.Height - 26) / 2, 50, 26);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(btn, 6))
                using (var brush = new SolidBrush(AccentBlue))
                {
                    e.Graphics.FillPath(brush, path);
                }
                if (printIcon != null)
                {
                    e.Graphics.DrawImage(printIcon, btn.X + (btn.Width - printIcon.Width) / 2, btn.Y + (btn.Height - printIcon.Height) / 2);
                }
                e.Handled = true;
            };

            dgv.CellMouseMove += (s, e) =>
            {
                dgv.Cursor = e.RowIndex >= 0 && e.ColumnIndex == dgv.Columns["colPrint"].Index ? Cursors.Hand : Cursors.Default;
            };

            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != dgv.Columns["colPrint"].Index) return;
                object? id = dgv.Rows[e.RowIndex].Cells["colOrderId"].Value;
                if (id != null) ReceiptPrinter.ShowPreview(Convert.ToInt64(id), this);
            };

            return dgv;
        }

        private static DataGridViewTextBoxColumn AddCol(DataGridView dgv, string name, string header, int fillWeight)
        {
            var col = new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                FillWeight = fillWeight,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            dgv.Columns.Add(col);
            return col;
        }

        private void LoadReceipts()
        {
            DateTime from = dtFrom.Value.Date;
            DateTime to = dtTo.Value.Date.AddDays(1);
            if (to <= from)
            {
                MessageBox.Show("Date To cannot be earlier than Date From.", "Receipt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            allRows.Clear();
            try
            {
                DbHelper.EnsurePaymentSchema();

                DataTable dt = DbHelper.ExecuteQuery(@"
SELECT p.PaymentID, p.OrderID, p.PaymentDate,
       ISNULL(u.FullName, 'System') AS Cashier,
       ISNULL(c.CustomerName, 'General Customer') AS CustomerName,
       CASE WHEN o.TableID IS NULL THEN 'Takeaway' ELSE ISNULL(t.TableName, '') END AS TableName,
       o.GrandTotal,
       ISNULL(pt.PaymentType, '') AS PaymentType
FROM dbo.PAYMENT p
JOIN dbo.SALE_ORDER o ON p.OrderID = o.OrderID
LEFT JOIN dbo.APP_USER u ON p.ReceivedBy = u.UserID
LEFT JOIN dbo.CUSTOMER c ON o.CustomerID = c.CustomerID
LEFT JOIN dbo.DINING_TABLE t ON o.TableID = t.TableID
OUTER APPLY (
    SELECT STRING_AGG(x.MethodName, ', ') AS PaymentType
    FROM (SELECT DISTINCT m.MethodName
          FROM dbo.PAYMENT_DETAIL pd
          JOIN dbo.PAYMENT_METHOD m ON pd.MethodID = m.MethodID
          WHERE pd.PaymentID = p.PaymentID) x
) pt
WHERE p.PaymentDate >= @From AND p.PaymentDate < @To
ORDER BY p.PaymentID;",
                    new SqlParameter("@From", from),
                    new SqlParameter("@To", to));

                foreach (DataRow r in dt.Rows)
                {
                    allRows.Add(new ReceiptRow
                    {
                        OrderId = Convert.ToInt64(r["OrderID"]),
                        ReceiptNo = ReceiptPrinter.FormatReceiptNo(Convert.ToInt64(r["PaymentID"])),
                        Cashier = r["Cashier"]?.ToString() ?? "",
                        Name = r["CustomerName"]?.ToString() ?? "",
                        PaymentDate = Convert.ToDateTime(r["PaymentDate"]),
                        Table = r["TableName"]?.ToString() ?? "",
                        Amount = Convert.ToDecimal(r["GrandTotal"]),
                        PaymentType = r["PaymentType"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading receipts: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ApplySearch();
        }

        private void ApplySearch()
        {
            string keyword = (txtSearch.Text ?? "").Trim();
            IEnumerable<ReceiptRow> rows = allRows;
            if (keyword.Length > 0)
            {
                rows = rows.Where(r =>
                    r.ReceiptNo.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    r.Cashier.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    r.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    r.Table.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    r.PaymentType.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    r.Amount.ToString("N2").Contains(keyword));
            }

            grid.SuspendLayout();
            grid.Rows.Clear();
            foreach (var r in rows)
            {
                grid.Rows.Add(
                    r.OrderId,
                    r.ReceiptNo,
                    r.Cashier,
                    r.Name,
                    r.PaymentDate.ToString("dd/MM/yyyy"),
                    r.PaymentDate.ToString("HH:mm"),
                    r.Table,
                    r.Amount.ToString("N2"),
                    r.PaymentType,
                    "");
            }
            grid.ClearSelection();
            grid.ResumeLayout();
        }

        private static Label MakeFieldLabel(string text, Point location) => new Label
        {
            Text = text,
            Location = location,
            AutoSize = true,
            ForeColor = TextDark,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        private static Guna2DateTimePicker MakeDatePicker(Point location) => new Guna2DateTimePicker
        {
            Location = location,
            Size = new Size(206, 44),
            BorderRadius = 6,
            FillColor = Color.FromArgb(226, 228, 231),
            BorderColor = Color.FromArgb(190, 195, 200),
            BorderThickness = 1,
            Font = new Font("Segoe UI", 10F),
            ForeColor = TextDark,
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today,
            Checked = true
        };

        private static Image MakeIcon(IconChar ch, Color color, int size)
        {
            using (var pb = new IconPictureBox
            {
                IconChar = ch,
                IconColor = color,
                IconSize = size,
                Size = new Size(size, size),
                BackColor = Color.Transparent
            })
                return new Bitmap(pb.Image);
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private void NativeDrag()
        {
            const int WM_NCLBUTTONDOWN = 0xA1;
            const int HTCAPTION = 0x2;
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) printIcon?.Dispose();
            base.Dispose(disposing);
        }
    }
}
