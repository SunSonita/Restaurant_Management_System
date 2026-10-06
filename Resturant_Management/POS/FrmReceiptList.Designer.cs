namespace Resturant_Management.POS
{
    partial class FrmReceiptList
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle amountStyle = new System.Windows.Forms.DataGridViewCellStyle();

            this.elipseForm = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.pnlTitle = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblClose = new System.Windows.Forms.Label();

            this.cardFilter = new Guna.UI2.WinForms.Guna2Panel();
            this.lblDateFrom = new System.Windows.Forms.Label();
            this.dtpDateFrom = new Guna.UI2.WinForms.Guna2DateTimePicker();
            this.lblDateTo = new System.Windows.Forms.Label();
            this.dtpDateTo = new Guna.UI2.WinForms.Guna2DateTimePicker();
            this.btnFilter = new Guna.UI2.WinForms.Guna2Button();
            this.txtSearch = new Guna.UI2.WinForms.Guna2TextBox();

            this.cardList = new Guna.UI2.WinForms.Guna2Panel();
            this.pnlListTitle = new Guna.UI2.WinForms.Guna2Panel();
            this.lblListTitle = new System.Windows.Forms.Label();
            this.dgvReceiptList = new Guna.UI2.WinForms.Guna2DataGridView();
            this.colReceiptNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCashier = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPaymentType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPrint = new System.Windows.Forms.DataGridViewTextBoxColumn();

            this.pnlTitle.SuspendLayout();
            this.cardFilter.SuspendLayout();
            this.cardList.SuspendLayout();
            this.pnlListTitle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReceiptList)).BeginInit();
            this.SuspendLayout();

            // ---------------- Rounded form corners ----------------
            this.elipseForm.BorderRadius = 14;
            this.elipseForm.TargetControl = this;

            // ---------------- Title bar ----------------
            this.pnlTitle.BackColor = System.Drawing.Color.FromArgb(123, 122, 142);
            this.pnlTitle.Controls.Add(this.lblTitle);
            this.pnlTitle.Controls.Add(this.lblClose);
            this.pnlTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitle.Name = "pnlTitle";
            this.pnlTitle.Size = new System.Drawing.Size(1200, 48);

            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12.5F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "Receipt";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblClose.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblClose.ForeColor = System.Drawing.Color.White;
            this.lblClose.Name = "lblClose";
            this.lblClose.Size = new System.Drawing.Size(60, 48);
            this.lblClose.Text = "×";
            this.lblClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblClose.Click += new System.EventHandler(this.lblClose_Click);
            this.lblClose.MouseEnter += new System.EventHandler(this.lblClose_MouseEnter);
            this.lblClose.MouseLeave += new System.EventHandler(this.lblClose_MouseLeave);

            // ---------------- Filter card ----------------
            this.cardFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cardFilter.BackColor = System.Drawing.Color.Transparent;
            this.cardFilter.BorderColor = System.Drawing.Color.FromArgb(225, 230, 238);
            this.cardFilter.BorderRadius = 12;
            this.cardFilter.BorderThickness = 1;
            this.cardFilter.FillColor = System.Drawing.Color.White;
            this.cardFilter.Location = new System.Drawing.Point(20, 66);
            this.cardFilter.Name = "cardFilter";
            this.cardFilter.Size = new System.Drawing.Size(1160, 88);
            this.cardFilter.Controls.Add(this.lblDateFrom);
            this.cardFilter.Controls.Add(this.dtpDateFrom);
            this.cardFilter.Controls.Add(this.lblDateTo);
            this.cardFilter.Controls.Add(this.dtpDateTo);
            this.cardFilter.Controls.Add(this.btnFilter);
            this.cardFilter.Controls.Add(this.txtSearch);

            // Date From
            this.lblDateFrom.AutoSize = true;
            this.lblDateFrom.BackColor = System.Drawing.Color.Transparent;
            this.lblDateFrom.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDateFrom.ForeColor = System.Drawing.Color.FromArgb(70, 80, 95);
            this.lblDateFrom.Location = new System.Drawing.Point(20, 10);
            this.lblDateFrom.Name = "lblDateFrom";
            this.lblDateFrom.Text = "Date From";

            this.dtpDateFrom.BorderColor = System.Drawing.Color.FromArgb(180, 215, 240);
            this.dtpDateFrom.BorderRadius = 8;
            this.dtpDateFrom.BorderThickness = 1;
            this.dtpDateFrom.Checked = true;
            this.dtpDateFrom.FillColor = System.Drawing.Color.White;
            this.dtpDateFrom.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpDateFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDateFrom.Location = new System.Drawing.Point(20, 36);
            this.dtpDateFrom.Name = "dtpDateFrom";
            this.dtpDateFrom.Size = new System.Drawing.Size(200, 36);

            // Date To
            this.lblDateTo.AutoSize = true;
            this.lblDateTo.BackColor = System.Drawing.Color.Transparent;
            this.lblDateTo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDateTo.ForeColor = System.Drawing.Color.FromArgb(70, 80, 95);
            this.lblDateTo.Location = new System.Drawing.Point(240, 10);
            this.lblDateTo.Name = "lblDateTo";
            this.lblDateTo.Text = "Date To";

            this.dtpDateTo.BorderColor = System.Drawing.Color.FromArgb(180, 215, 240);
            this.dtpDateTo.BorderRadius = 8;
            this.dtpDateTo.BorderThickness = 1;
            this.dtpDateTo.Checked = true;
            this.dtpDateTo.FillColor = System.Drawing.Color.White;
            this.dtpDateTo.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpDateTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDateTo.Location = new System.Drawing.Point(240, 36);
            this.dtpDateTo.Name = "dtpDateTo";
            this.dtpDateTo.Size = new System.Drawing.Size(200, 36);

            // Filter button (blue, rounded)
            this.btnFilter.Animated = true;
            this.btnFilter.BorderRadius = 8;
            this.btnFilter.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilter.FillColor = System.Drawing.Color.FromArgb(99, 179, 232);
            this.btnFilter.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnFilter.ForeColor = System.Drawing.Color.White;
            this.btnFilter.HoverState.FillColor = System.Drawing.Color.FromArgb(70, 160, 220);
            this.btnFilter.Location = new System.Drawing.Point(460, 36);
            this.btnFilter.Name = "btnFilter";
            this.btnFilter.Size = new System.Drawing.Size(105, 36);
            this.btnFilter.Text = "Filter";
            this.btnFilter.Click += new System.EventHandler(this.btnFilter_Click);

            // Search box (pill shape)
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.BorderColor = System.Drawing.Color.FromArgb(225, 230, 238);
            this.txtSearch.BorderRadius = 18;
            this.txtSearch.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtSearch.DefaultText = "";
            this.txtSearch.FillColor = System.Drawing.Color.FromArgb(244, 247, 251);
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSearch.Location = new System.Drawing.Point(850, 36);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "🔍 Search";
            this.txtSearch.SelectedText = "";
            this.txtSearch.Size = new System.Drawing.Size(290, 36);
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);

            // ---------------- List card ----------------
            this.cardList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cardList.BackColor = System.Drawing.Color.Transparent;
            this.cardList.BorderColor = System.Drawing.Color.FromArgb(225, 230, 238);
            this.cardList.BorderRadius = 12;
            this.cardList.BorderThickness = 1;
            this.cardList.FillColor = System.Drawing.Color.White;
            this.cardList.Location = new System.Drawing.Point(20, 170);
            this.cardList.Name = "cardList";
            this.cardList.Size = new System.Drawing.Size(1160, 510);
            this.cardList.Controls.Add(this.pnlListTitle);
            this.cardList.Controls.Add(this.dgvReceiptList);

            // "List of Receipts" pill
            this.pnlListTitle.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.pnlListTitle.BackColor = System.Drawing.Color.Transparent;
            this.pnlListTitle.BorderRadius = 10;
            this.pnlListTitle.Controls.Add(this.lblListTitle);
            this.pnlListTitle.FillColor = System.Drawing.Color.FromArgb(99, 179, 232);
            this.pnlListTitle.Location = new System.Drawing.Point(485, 14);
            this.pnlListTitle.Name = "pnlListTitle";
            this.pnlListTitle.Size = new System.Drawing.Size(190, 36);

            this.lblListTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblListTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblListTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblListTitle.ForeColor = System.Drawing.Color.White;
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Text = "List of Receipts";
            this.lblListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ---------------- Grid ----------------
            this.dgvReceiptList.AllowUserToAddRows = false;
            this.dgvReceiptList.AllowUserToDeleteRows = false;
            this.dgvReceiptList.AllowUserToResizeRows = false;
            this.dgvReceiptList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvReceiptList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvReceiptList.BackgroundColor = System.Drawing.Color.White;
            this.dgvReceiptList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvReceiptList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvReceiptList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvReceiptList.ColumnHeadersHeight = 42;
            this.dgvReceiptList.GridColor = System.Drawing.Color.FromArgb(232, 236, 242);
            this.dgvReceiptList.Location = new System.Drawing.Point(16, 62);
            this.dgvReceiptList.MultiSelect = false;
            this.dgvReceiptList.Name = "dgvReceiptList";
            this.dgvReceiptList.ReadOnly = true;
            this.dgvReceiptList.RowHeadersVisible = false;
            this.dgvReceiptList.RowTemplate.Height = 40;
            this.dgvReceiptList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvReceiptList.Size = new System.Drawing.Size(1128, 432);
            this.dgvReceiptList.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvReceiptList_CellPainting);
            this.dgvReceiptList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvReceiptList_CellClick);
            this.dgvReceiptList.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvReceiptList_CellMouseEnter);
            this.dgvReceiptList.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvReceiptList_CellMouseLeave);

            this.dgvReceiptList.ThemeStyle.BackColor = System.Drawing.Color.White;
            this.dgvReceiptList.ThemeStyle.GridColor = System.Drawing.Color.FromArgb(232, 236, 242);
            this.dgvReceiptList.ThemeStyle.HeaderStyle.BackColor = System.Drawing.Color.FromArgb(95, 119, 145);
            this.dgvReceiptList.ThemeStyle.HeaderStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.dgvReceiptList.ThemeStyle.HeaderStyle.ForeColor = System.Drawing.Color.White;
            this.dgvReceiptList.ThemeStyle.HeaderStyle.Height = 42;
            this.dgvReceiptList.ThemeStyle.RowsStyle.BackColor = System.Drawing.Color.White;
            this.dgvReceiptList.ThemeStyle.RowsStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgvReceiptList.ThemeStyle.RowsStyle.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.dgvReceiptList.ThemeStyle.RowsStyle.Height = 40;
            this.dgvReceiptList.ThemeStyle.RowsStyle.SelectionBackColor = System.Drawing.Color.FromArgb(214, 234, 250);
            this.dgvReceiptList.ThemeStyle.RowsStyle.SelectionForeColor = System.Drawing.Color.Black;

            amountStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            amountStyle.Format = "N2";

            this.dgvReceiptList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colReceiptNo, this.colCashier, this.colName,
                this.colDate, this.colTime, this.colTable, this.colAmount, this.colPaymentType, this.colPrint });

            this.colReceiptNo.HeaderText = "Receipt №"; this.colReceiptNo.Name = "colReceiptNo";
            this.colCashier.HeaderText = "Cashier"; this.colCashier.Name = "colCashier";
            this.colName.HeaderText = "Name"; this.colName.Name = "colName";
            this.colDate.HeaderText = "Date"; this.colDate.Name = "colDate";
            this.colTime.HeaderText = "Time"; this.colTime.Name = "colTime";
            this.colTable.HeaderText = "Table"; this.colTable.Name = "colTable";
            this.colAmount.HeaderText = "Amount"; this.colAmount.Name = "colAmount";
            this.colAmount.DefaultCellStyle = amountStyle;
            this.colPaymentType.HeaderText = "Payment Type"; this.colPaymentType.Name = "colPaymentType";
            this.colPrint.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colPrint.HeaderText = "Print";
            this.colPrint.Name = "colPrint";
            this.colPrint.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colPrint.Width = 90;

            // ---------------- Form ----------------
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(242, 245, 249);
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.cardList);
            this.Controls.Add(this.cardFilter);
            this.Controls.Add(this.pnlTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmReceiptList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Receipt";

            this.pnlTitle.ResumeLayout(false);
            this.cardFilter.ResumeLayout(false);
            this.cardFilter.PerformLayout();
            this.cardList.ResumeLayout(false);
            this.pnlListTitle.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvReceiptList)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private Guna.UI2.WinForms.Guna2Elipse elipseForm;
        private System.Windows.Forms.Panel pnlTitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblClose;
        private Guna.UI2.WinForms.Guna2Panel cardFilter;
        private System.Windows.Forms.Label lblDateFrom;
        private Guna.UI2.WinForms.Guna2DateTimePicker dtpDateFrom;
        private System.Windows.Forms.Label lblDateTo;
        private Guna.UI2.WinForms.Guna2DateTimePicker dtpDateTo;
        private Guna.UI2.WinForms.Guna2Button btnFilter;
        private Guna.UI2.WinForms.Guna2TextBox txtSearch;
        private Guna.UI2.WinForms.Guna2Panel cardList;
        private Guna.UI2.WinForms.Guna2Panel pnlListTitle;
        private System.Windows.Forms.Label lblListTitle;
        private Guna.UI2.WinForms.Guna2DataGridView dgvReceiptList;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReceiptNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCashier;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAmount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPaymentType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPrint;
    }
}
