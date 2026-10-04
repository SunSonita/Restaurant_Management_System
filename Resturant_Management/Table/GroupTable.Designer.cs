namespace Resturant_Management.Table
{
    partial class GroupTable
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            dgvGroupTable = new Guna.UI2.WinForms.Guna2DataGridView();
            colAction = new DataGridViewTextBoxColumn();
            colCode = new DataGridViewTextBoxColumn();
            colName = new DataGridViewTextBoxColumn();
            colType = new DataGridViewTextBoxColumn();
            colImage = new DataGridViewTextBoxColumn();
            btnCreate = new Guna.UI2.WinForms.Guna2Button();
            label1 = new Label();
            txtSearch = new Guna.UI2.WinForms.Guna2TextBox();
            ((System.ComponentModel.ISupportInitialize)dgvGroupTable).BeginInit();
            SuspendLayout();
            // 
            // dgvGroupTable
            // 
            dataGridViewCellStyle1.BackColor = Color.White;
            dgvGroupTable.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.White;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = Color.Black;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvGroupTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvGroupTable.ColumnHeadersHeight = 22;
            dgvGroupTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvGroupTable.Columns.AddRange(new DataGridViewColumn[] { colAction, colCode, colName, colType, colImage });
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvGroupTable.DefaultCellStyle = dataGridViewCellStyle3;
            dgvGroupTable.GridColor = Color.FromArgb(231, 229, 255);
            dgvGroupTable.Location = new Point(104, 101);
            dgvGroupTable.Name = "dgvGroupTable";
            dgvGroupTable.RowHeadersVisible = false;
            dgvGroupTable.RowHeadersWidth = 51;
            dgvGroupTable.Size = new Size(1672, 77);
            dgvGroupTable.TabIndex = 44;
            dgvGroupTable.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White;
            dgvGroupTable.ThemeStyle.HeaderStyle.BackColor = Color.White;
            dgvGroupTable.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dgvGroupTable.ThemeStyle.HeaderStyle.ForeColor = Color.Black;
            dgvGroupTable.ThemeStyle.HeaderStyle.Height = 22;
            dgvGroupTable.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9F);
            dgvGroupTable.ThemeStyle.RowsStyle.Height = 29;
            // 
            // colAction
            // 
            colAction.FillWeight = 106.951874F;
            colAction.HeaderText = "Action";
            colAction.MinimumWidth = 6;
            colAction.Name = "colAction";
            // 
            // colCode
            // 
            colCode.HeaderText = "Code";
            colCode.MinimumWidth = 6;
            colCode.Name = "colCode";
            // 
            // colName
            // 
            colName.FillWeight = 97.68271F;
            colName.HeaderText = "Name";
            colName.MinimumWidth = 6;
            colName.Name = "colName";
            // 
            // colType
            // 
            colType.HeaderText = "Type";
            colType.MinimumWidth = 6;
            colType.Name = "colType";
            // 
            // colImage
            // 
            colImage.FillWeight = 97.68271F;
            colImage.HeaderText = "Image";
            colImage.MinimumWidth = 6;
            colImage.Name = "colImage";
            // 
            // btnCreate
            // 
            btnCreate.BorderRadius = 6;
            btnCreate.CustomizableEdges = customizableEdges1;
            btnCreate.DisabledState.BorderColor = Color.DarkGray;
            btnCreate.DisabledState.CustomBorderColor = Color.DarkGray;
            btnCreate.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnCreate.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnCreate.FillColor = Color.RoyalBlue;
            btnCreate.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCreate.ForeColor = Color.White;
            btnCreate.Location = new Point(1684, 16);
            btnCreate.Name = "btnCreate";
            btnCreate.ShadowDecoration.CustomizableEdges = customizableEdges2;
            btnCreate.Size = new Size(92, 41);
            btnCreate.TabIndex = 38;
            btnCreate.Text = "Create";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Navy;
            label1.Location = new Point(104, 19);
            label1.Name = "label1";
            label1.Size = new Size(176, 38);
            label1.TabIndex = 37;
            label1.Text = "Group Table";
            // 
            // txtSearch
            // 
            txtSearch.BorderColor = Color.Silver;
            txtSearch.BorderRadius = 5;
            txtSearch.CustomizableEdges = customizableEdges3;
            txtSearch.DefaultText = "";
            txtSearch.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtSearch.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtSearch.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtSearch.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtSearch.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtSearch.Font = new Font("Segoe UI", 9F);
            txtSearch.ForeColor = Color.Silver;
            txtSearch.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtSearch.Location = new Point(1392, 19);
            txtSearch.Margin = new Padding(3, 4, 3, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderForeColor = Color.Silver;
            txtSearch.PlaceholderText = "Search............................................................";
            txtSearch.SelectedText = "";
            txtSearch.ShadowDecoration.CustomizableEdges = customizableEdges4;
            txtSearch.Size = new Size(257, 42);
            txtSearch.TabIndex = 43;
            // 
            // GroupTable
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(dgvGroupTable);
            Controls.Add(txtSearch);
            Controls.Add(btnCreate);
            Controls.Add(label1);
            Name = "GroupTable";
            Size = new Size(1832, 802);
            ((System.ComponentModel.ISupportInitialize)dgvGroupTable).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Guna.UI2.WinForms.Guna2DataGridView dgvGroupTable;
        private Guna.UI2.WinForms.Guna2Button btnCreate;
        private Label label1;
        private Guna.UI2.WinForms.Guna2TextBox txtSearch;
        private DataGridViewTextBoxColumn colAction;
        private DataGridViewTextBoxColumn colCode;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colType;
        private DataGridViewTextBoxColumn colImage;
    }
}
