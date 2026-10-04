using System;
using System.Globalization;
using System.Windows.Forms;

namespace Resturant_Management.POS
{
    public partial class FrmDiscount : Form
    {
        private bool _updating;
        public decimal TotalAmountUSD { get; set; }
        public decimal DiscountUSD { get; private set; }

        public FrmDiscount()
        {
            InitializeComponent();

            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            btnOK.DialogResult = DialogResult.None;      // validated manually in the click handler
            btnCancel.DialogResult = DialogResult.Cancel;
            AcceptButton = btnOK;
            CancelButton = btnCancel;

            txtTotalAmount.ReadOnly = true;
            txtTotalDis.ReadOnly = true;
            txtTotalDisPercent.ReadOnly = true;

            txtDis.TextChanged += TxtDiscountUSD_TextChanged;
            txtDisPercent.TextChanged += TxtDiscountPercent_TextChanged;
            btnOK.Click += BtnOK_Click;

            Load += (s, e) =>
            {
                txtTotalAmount.Text = TotalAmountUSD.ToString("N2");
                RefreshTotals(0m);
                txtDis.Focus();
            };
        }

        private static decimal Parse(string? s) =>
            decimal.TryParse(s?.Replace(",", "").Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

        // typing $ updates %
        private void TxtDiscountUSD_TextChanged(object sender, EventArgs e)
        {
            if (_updating) return;
            _updating = true;

            decimal usd = Math.Min(Parse(txtDis.Text), TotalAmountUSD);
            decimal pct = TotalAmountUSD > 0 ? usd / TotalAmountUSD * 100m : 0m;
            txtDisPercent.Text = pct == 0 ? "" : pct.ToString("0.##");
            RefreshTotals(usd);

            _updating = false;
        }

        // typing % updates $
        private void TxtDiscountPercent_TextChanged(object sender, EventArgs e)
        {
            if (_updating) return;
            _updating = true;

            decimal pct = Math.Min(Parse(txtDisPercent.Text), 100m);
            decimal usd = TotalAmountUSD * pct / 100m;
            txtDis.Text = usd == 0 ? "" : usd.ToString("0.##");
            RefreshTotals(usd);

            _updating = false;
        }

        private void RefreshTotals(decimal usd)
        {
            decimal pct = TotalAmountUSD > 0 ? usd / TotalAmountUSD * 100m : 0m;
            txtTotalDis.Text = usd.ToString("N2");
            txtTotalDisPercent.Text = pct.ToString("0.##");
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            decimal usd = Parse(txtDis.Text);
            if (usd > TotalAmountUSD)
            {
                MessageBox.Show("Discount cannot be greater than the total amount.",
                                "Invalid Discount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DiscountUSD = usd;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnOK_Click_1(object sender, EventArgs e)
        {

        }
    }
}