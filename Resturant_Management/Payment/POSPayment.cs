using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Resturant_Management.Payment
{
    [System.ComponentModel.DesignerCategory("Code")]
    public partial class POSPayment : UserControl
    {
        // UI Controls - Left Side Summary
        public Label lblTotalUSD = null!;
        public Label lblTotalKHR = null!;
        public Label lblReceivedUSD = null!;
        public Label lblReceivedKHR = null!;
        public Label lblChangedUSD = null!;
        public Label lblChangedKHR = null!;

        // UI Controls - Right Side Inputs
        public Guna2TextBox txtABAUsd = null!;
        public Guna2TextBox txtCashUsd = null!;
        public Guna2TextBox txtCashKhr = null!;
        public Guna2TextBox txtABAKhr = null!;
        public Guna2DateTimePicker dtpPaymentDate = null!;

        public Guna2Button btnBack = null!;
        public Guna2Button btnPay = null!;

        // Layout helpers
        private Panel pnlHeader = null!;
        private Panel pnlBody = null!;
        private Panel pnlFooter = null!;
        private Guna2Panel mainPanel = null!;
        private Guna2Button btnCheckoutHeader = null!;

        private Guna2Panel[] cards = new Guna2Panel[3];
        private Guna2Button[] cardBadges = new Guna2Button[3];
        private Guna2Panel[] inputRows = new Guna2Panel[4];

        private static readonly Color HeaderPurple = Color.FromArgb(160, 85, 135);
        private static readonly Color Green = Color.FromArgb(80, 160, 120);
        private static readonly Color Orange = Color.FromArgb(235, 130, 40);
        private static readonly Color SkyBlue = Color.FromArgb(100, 175, 230);
        private static readonly Color LineBlue = Color.FromArgb(160, 210, 240);

        public POSPayment()
        {
            InitializeComponentCustom();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime
        || DesignMode)
            {
                return;
            }
        }

        private void InitializeComponentCustom()
        {
            this.Size = new Size(1000, 600);
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = HeaderPurple
            };

            btnBack = new Guna2Button
            {
                Text = "◀ Back",
                Size = new Size(80, 32),
                Location = new Point(6, 7),
                FillColor = HeaderPurple,
                ForeColor = Color.White,
                BorderColor = Color.FromArgb(215, 170, 200),
                BorderThickness = 1,
                BorderRadius = 4,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            btnBack.Click += (s, e) =>
            {
                Form? parentForm = this.FindForm();
                if (parentForm != null) parentForm.DialogResult = DialogResult.Cancel;
            };
            pnlHeader.Controls.Add(btnBack);

            // ================= Footer (line + PAY) =================
            pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 84,
                BackColor = Color.White
            };

            Panel separator = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(150, 150, 150)
            };

            btnPay = new Guna2Button
            {
                Text = "PAY",
                Height = 56,
                FillColor = SkyBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                BorderRadius = 28,
                Cursor = Cursors.Hand
            };
            btnPay.Click += (s, e) =>
            {
                Form? parentForm = this.FindForm();
                if (parentForm != null)
                {
                    if (TotalReceivedUSD < TotalDueUSD)
                    {
                        MessageBox.Show($"Received amount ({TotalReceivedUSD:N2} USD) is less than the total due ({TotalDueUSD:N2} USD).\nPlease enter sufficient payment.",
                            "Insufficient Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    parentForm.DialogResult = DialogResult.OK;
                }
            };

            pnlFooter.Controls.Add(btnPay);
            pnlFooter.Controls.Add(separator);

            // ================= Body =================
            pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20, 24, 20, 12)
            };

            mainPanel = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderColor = Color.FromArgb(205, 205, 205),
                BorderThickness = 1,
                BorderRadius = 8,
                FillColor = Color.White
            };

            // "Checkout" Header Button - Dynamically Sized
            Font checkoutFont = new Font("Segoe UI", 12.5F, FontStyle.Bold);
            int checkoutWidth = TextRenderer.MeasureText("Checkout", checkoutFont).Width + 40;
            btnCheckoutHeader = new Guna2Button
            {
                Text = "Checkout",
                Size = new Size(checkoutWidth, 38),
                Location = new Point(26, 6),
                FillColor = SkyBlue,
                ForeColor = Color.White,
                Font = checkoutFont,
                BorderRadius = 8,
                Cursor = Cursors.Default
            };

            // ---- Left summary cards ----
            // Total Amount at 15F; Received and Changed reduced to 12.5F
            cards[0] = CreateSummaryCard(15F, out lblTotalUSD, out lblTotalKHR);
            cards[1] = CreateSummaryCard(12.5F, out lblReceivedUSD, out lblReceivedKHR);
            cards[2] = CreateSummaryCard(12.5F, out lblChangedUSD, out lblChangedKHR);

            // Dynamic width badge controls so no text ever gets cut off
            cardBadges[0] = CreateCardBadge("Total Amount");
            cardBadges[1] = CreateCardBadge("Received Amount");
            cardBadges[2] = CreateCardBadge("Changed Amount");

            // ---- Right side: date ----
            dtpPaymentDate = new Guna2DateTimePicker
            {
                Size = new Size(210, 45),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now,
                BorderRadius = 18,
                BorderColor = LineBlue,
                BorderThickness = 1,
                Padding = new Padding(10, 0, 0, 0),
                Margin = new Padding(0),
                FillColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 11F)
            };

            // ---- Right side: input rows ----
            txtABAUsd = CreateInputRow("USD", "ABA ($)", 0);
            txtCashUsd = CreateInputRow("USD", "Cash-$", 1);
            txtCashKhr = CreateInputRow("KHR", "Cash-៛", 2);
            txtABAKhr = CreateInputRow("KHR", "ABA (៛)", 3);

            txtABAUsd.TextChanged += CalculatePaymentTotals;
            txtCashUsd.TextChanged += CalculatePaymentTotals;
            txtCashKhr.TextChanged += CalculatePaymentTotals;
            txtABAKhr.TextChanged += CalculatePaymentTotals;

            foreach (var c in cards) mainPanel.Controls.Add(c);
            mainPanel.Controls.Add(dtpPaymentDate);
            foreach (var r in inputRows) mainPanel.Controls.Add(r);
            foreach (var b in cardBadges) mainPanel.Controls.Add(b);

            pnlBody.Controls.Add(mainPanel);
            pnlBody.Controls.Add(btnCheckoutHeader);
            btnCheckoutHeader.BringToFront();

            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlHeader);

            mainPanel.Resize += (s, e) => LayoutControls();
            this.Load += (s, e) => LayoutControls();
            LayoutControls();
        }

        private void LayoutControls()
        {
            if (mainPanel == null) return;

            int w = mainPanel.ClientSize.Width;
            const int pad = 22;
            const int gap = 30;

            int colW = (w - 2 * pad - gap) / 2;
            if (colW < 300) colW = 300;

            int leftX = pad;
            int rightX = pad + colW + gap;

            // Left cards
            int[] cardY = { 40, 140, 240 };
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].SetBounds(leftX, cardY[i], colW, 92);
                cardBadges[i].Location = new Point(leftX + 12, cardY[i] - 12);
                cardBadges[i].BringToFront();
            }

            // Right: date
            dtpPaymentDate.SetBounds(rightX, 40, 215, 46);

            // Right: input rows
            for (int i = 0; i < inputRows.Length; i++)
                inputRows[i].SetBounds(rightX, 102 + i * 58, colW, 46);

            // PAY button aligned with right column
            btnPay.SetBounds(pnlBody.Padding.Left + rightX, 18, colW, 56);
        }

        private Guna2Panel CreateSummaryCard(float fontSize, out Label usdLabel, out Label khrLabel)
        {
            Guna2Panel card = new Guna2Panel
            {
                BorderColor = LineBlue,
                BorderThickness = 1,
                BorderRadius = 34,
                FillColor = Color.White,
                Padding = new Padding(10, 10, 22, 6)
            };

            TableLayoutPanel tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            usdLabel = CreateAmountLabel("0.00 USD", fontSize);
            khrLabel = CreateAmountLabel("0.00 KHR", fontSize);

            tbl.Controls.Add(usdLabel, 0, 0);
            tbl.Controls.Add(khrLabel, 0, 1);
            card.Controls.Add(tbl);

            return card;
        }

        private Label CreateAmountLabel(string text, float fontSize)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", fontSize, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                BackColor = Color.Transparent,
                Text = text
            };
        }

        private Guna2Button CreateCardBadge(string text)
        {
            Font badgeFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            int calculatedWidth = TextRenderer.MeasureText(text, badgeFont).Width + 28;

            return new Guna2Button
            {
                Text = text,
                Size = new Size(calculatedWidth, 26),
                FillColor = Green,
                ForeColor = Color.White,
                Font = badgeFont,
                BorderRadius = 12,
                Cursor = Cursors.Default
            };
        }

        private Guna2TextBox CreateInputRow(string currency, string methodTitle, int index)
        {
            Guna2Panel row = new Guna2Panel
            {
                BorderColor = LineBlue,
                BorderThickness = 1,
                BorderRadius = 22,
                FillColor = Color.White,
                Padding = new Padding(3)
            };

            Guna2TextBox txtInput = new Guna2TextBox
            {
                Dock = DockStyle.Fill,
                BorderThickness = 0,
                FillColor = Color.White,
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.Black,
                Margin = new Padding(0),
                Text = "0"
            };

            Guna2Button btnMethod = new Guna2Button
            {
                Text = methodTitle,
                Dock = DockStyle.Right,
                Width = 110,
                FillColor = Orange,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BorderRadius = 18,
                Cursor = Cursors.Hand
            };
            btnMethod.Click += (s, e) =>
            {
                decimal totalUSD = ParseDec(lblTotalUSD?.Text);
                if (currency == "USD")
                {
                    txtInput.Text = totalUSD.ToString("N2");
                }
                else
                {
                    txtInput.Text = (totalUSD * 4000m).ToString("N0");
                }
                CalculatePaymentTotals(null, EventArgs.Empty);
            };

            Guna2Button btnCurrency = new Guna2Button
            {
                Text = currency + "  ˅",
                Dock = DockStyle.Left,
                Width = 114,
                FillColor = Green,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BorderRadius = 18,
                Cursor = Cursors.Default
            };

            row.Controls.Add(txtInput);
            row.Controls.Add(btnMethod);
            row.Controls.Add(btnCurrency);

            inputRows[index] = row;
            return txtInput;
        }

        private static decimal ParseDec(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0m;
            string s = text.Replace(",", "").Replace("USD", "").Replace("KHR", "").Replace("$", "").Replace("៛", "").Trim();
            return decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal v) ? v : 0m;
        }

        public void CalculatePaymentTotals(object? sender, EventArgs e)
        {
            if (lblTotalUSD == null || txtABAUsd == null || txtCashUsd == null ||
                txtCashKhr == null || txtABAKhr == null || lblReceivedUSD == null ||
                lblReceivedKHR == null || lblChangedUSD == null || lblChangedKHR == null)
            {
                return;
            }
            if (DesignMode || System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            decimal totalUSD = ParseDec(lblTotalUSD.Text);
            decimal abaUSD = ParseDec(txtABAUsd.Text);
            decimal cashUSD = ParseDec(txtCashUsd.Text);
            decimal cashKHR = ParseDec(txtCashKhr.Text);
            decimal abaKHR = ParseDec(txtABAKhr.Text);

            decimal totalReceivedUSD = abaUSD + cashUSD + ((cashKHR + abaKHR) / 4000m);
            decimal totalReceivedKHR = totalReceivedUSD * 4000m;

            decimal changeUSD = totalReceivedUSD - totalUSD;
            decimal changeKHR = changeUSD * 4000m;

            lblReceivedUSD.Text = $"{totalReceivedUSD:N2} USD";
            lblReceivedKHR.Text = $"{totalReceivedKHR:N2} KHR";

            lblChangedUSD.Text = $"{changeUSD:N2} USD";
            lblChangedKHR.Text = $"{changeKHR:N2} KHR";

            Color statusColor = changeUSD >= 0 ? Color.FromArgb(78, 163, 129) : Color.FromArgb(230, 60, 60);
            lblReceivedUSD.ForeColor = Color.FromArgb(40, 40, 40);
            lblReceivedKHR.ForeColor = Color.FromArgb(40, 40, 40);
            lblChangedUSD.ForeColor = statusColor;
            lblChangedKHR.ForeColor = statusColor;
        }

        public decimal AbaUSD => ParseDec(txtABAUsd?.Text);
        public decimal CashUSD => ParseDec(txtCashUsd?.Text);
        public decimal CashKHR => ParseDec(txtCashKhr?.Text);
        public decimal AbaKHR => ParseDec(txtABAKhr?.Text);
        public decimal TotalDueUSD => ParseDec(lblTotalUSD?.Text);
        public decimal TotalReceivedUSD => ParseDec(lblReceivedUSD?.Text);
        public decimal ChangeAmountUSD => ParseDec(lblChangedUSD?.Text);
        public DateTime PaymentDate => dtpPaymentDate?.Value ?? DateTime.Now;
    }
}