namespace Resturant_Management.Inventory
{
    partial class Itemlist
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            pnlTop = new Panel();
            btnCreate = new Guna.UI2.WinForms.Guna2Button();
            lblTitle = new Label();
            chkInactive = new CheckBox();
            txtSearch = new TextBox();
            dgvItemMasterData = new DataGridView();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvItemMasterData).BeginInit();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(btnCreate);
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(chkInactive);
            pnlTop.Controls.Add(txtSearch);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Margin = new Padding(4, 4, 4, 4);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1109, 81);
            pnlTop.TabIndex = 0;
            pnlTop.Paint += pnlTop_Paint;
            // 
            // btnCreate
            // 
            btnCreate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCreate.Animated = true;
            btnCreate.BorderRadius = 5;
            btnCreate.CustomizableEdges = customizableEdges1;
            btnCreate.DisabledState.BorderColor = Color.DarkGray;
            btnCreate.DisabledState.CustomBorderColor = Color.DarkGray;
            btnCreate.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnCreate.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnCreate.FillColor = Color.FromArgb(10, 10, 200);
            btnCreate.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnCreate.ForeColor = Color.White;
            btnCreate.Location = new Point(864, 11);
            btnCreate.Margin = new Padding(3, 2, 3, 2);
            btnCreate.Name = "btnCreate";
            btnCreate.ShadowDecoration.CustomizableEdges = customizableEdges2;
            btnCreate.Size = new Size(75, 26);
            btnCreate.TabIndex = 2;
            btnCreate.Text = "Create";
            btnCreate.Click += btnCreate_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Navy;
            lblTitle.Location = new Point(18, 11);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(164, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Item Master Data";
            // 
            // chkInactive
            // 
            chkInactive.AutoSize = true;
            chkInactive.Font = new Font("Segoe UI", 9F);
            chkInactive.ForeColor = Color.FromArgb(80, 80, 80);
            chkInactive.Location = new Point(24, 49);
            chkInactive.Margin = new Padding(4, 4, 4, 4);
            chkInactive.Name = "chkInactive";
            chkInactive.Size = new Size(67, 19);
            chkInactive.TabIndex = 1;
            chkInactive.Text = "Inactive";
            chkInactive.UseVisualStyleBackColor = true;
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSearch.Font = new Font("Segoe UI", 9F);
            txtSearch.ForeColor = Color.FromArgb(64, 64, 64);
            txtSearch.Location = new Point(864, 44);
            txtSearch.Margin = new Padding(4, 4, 4, 4);
            txtSearch.Multiline = true;
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search...";
            txtSearch.Size = new Size(227, 31);
            txtSearch.TabIndex = 2;
            // 
            // dgvItemMasterData
            // 
            dgvItemMasterData.ColumnHeadersHeight = 29;
            dgvItemMasterData.Dock = DockStyle.Fill;
            dgvItemMasterData.Location = new Point(0, 81);
            dgvItemMasterData.Margin = new Padding(4, 4, 4, 4);
            dgvItemMasterData.Name = "dgvItemMasterData";
            dgvItemMasterData.RowHeadersWidth = 51;
            dgvItemMasterData.Size = new Size(1109, 485);
            dgvItemMasterData.TabIndex = 1;
            // 
            // Itemlist
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(dgvItemMasterData);
            Controls.Add(pnlTop);
            Margin = new Padding(4, 4, 4, 4);
            Name = "Itemlist";
            Size = new Size(1109, 566);
            Load += Itemlist_Load;
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvItemMasterData).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.CheckBox chkInactive;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.DataGridView dgvItemMasterData;
        private Guna.UI2.WinForms.Guna2Button btnCreate;
    }
}