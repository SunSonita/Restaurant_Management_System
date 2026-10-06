namespace Resturant_Management.Table
{
    partial class GroupTable
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();

            dgvGroupTable = new Guna.UI2.WinForms.Guna2DataGridView();
            colAction = new DataGridViewTextBoxColumn();
            colImage = new DataGridViewImageColumn();
            colCode = new DataGridViewTextBoxColumn();
            colName = new DataGridViewTextBoxColumn();
            colType = new DataGridViewTextBoxColumn();
            btnCreate = new Guna.UI2.WinForms.Guna2Button();
            label1 = new Label();
            txtSearch = new Guna.UI2.WinForms.Guna2TextBox();

            ((System.ComponentModel.ISupportInitialize)dgvGroupTable).BeginInit();
            SuspendLayout();

            // dgvGroupTable
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
            dgvGroupTable.ColumnHeadersHeight = 44;
            dgvGroupTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvGroupTable.Columns.AddRange(new DataGridViewColumn[] { colAction, colImage, colCode, colName, colType });

            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvGroupTable.DefaultCellStyle = dataGridViewCellStyle3;
            dgvGroupTable.GridColor = Color.FromArgb(231, 229, 255);
            dgvGroupTable.Location = new Point(30, 90);
            dgvGroupTable.Name = "dgvGroupTable";
            dgvGroupTable.RowHeadersVisible = false;
            dgvGroupTable.RowTemplate.Height = 55;
            dgvGroupTable.Size = new Size(1200, 500);
            dgvGroupTable.TabIndex = 44;

            // colAction (Left Aligned)
            colAction.HeaderText = "Edit";
            colAction.MinimumWidth = 60;
            colAction.FillWeight = 60F;
            colAction.Name = "colAction";
            colAction.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colAction.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // colImage (Left Aligned)
            colImage.HeaderText = "Image";
            colImage.MinimumWidth = 80;
            colImage.FillWeight = 80F;
            colImage.Name = "colImage";
            colImage.ImageLayout = DataGridViewImageCellLayout.Zoom;
            colImage.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colImage.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // colCode
            colCode.HeaderText = "Code";
            colCode.MinimumWidth = 100;
            colCode.Name = "colCode";

            // colName
            colName.HeaderText = "Name";
            colName.MinimumWidth = 150;
            colName.Name = "colName";

            // colType
            colType.HeaderText = "Type";
            colType.MinimumWidth = 120;
            colType.Name = "colType";

            // btnCreate
            btnCreate.BorderRadius = 6;
            btnCreate.CustomizableEdges = customizableEdges1;
            btnCreate.FillColor = Color.RoyalBlue;
            btnCreate.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            btnCreate.ForeColor = Color.White;
            btnCreate.Location = new Point(1115, 20);
            btnCreate.Name = "btnCreate";
            btnCreate.ShadowDecoration.CustomizableEdges = customizableEdges2;
            btnCreate.Size = new Size(115, 45);
            btnCreate.TabIndex = 38;
            btnCreate.Text = "+ Create";

            // label1
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold);
            label1.ForeColor = Color.Navy;
            label1.Location = new Point(30, 24);
            label1.Name = "label1";
            label1.Size = new Size(206, 45);
            label1.TabIndex = 37;
            label1.Text = "Group Table";

            // txtSearch
            txtSearch.BorderColor = Color.Silver;
            txtSearch.BorderRadius = 5;
            txtSearch.CustomizableEdges = customizableEdges3;
            txtSearch.DefaultText = "";
            txtSearch.Location = new Point(780, 20);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search group...";
            txtSearch.Size = new Size(321, 45);
            txtSearch.TabIndex = 43;

            // GroupTable
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(dgvGroupTable);
            Controls.Add(txtSearch);
            Controls.Add(btnCreate);
            Controls.Add(label1);
            Name = "GroupTable";
            Size = new Size(1260, 650);
            Load += GroupTable_Load_1;
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
        private DataGridViewImageColumn colImage;
        private DataGridViewTextBoxColumn colCode;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colType;
    }
}