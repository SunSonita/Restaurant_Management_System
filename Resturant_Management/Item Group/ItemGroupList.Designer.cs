namespace Resturant_Management.Item_Group
{
    partial class ItemGroupList
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            label1 = new Label();
            btnCreate = new Guna.UI2.WinForms.Guna2Button();
            txtSearch = new Guna.UI2.WinForms.Guna2TextBox();
            gridGroupData = new Guna.UI2.WinForms.Guna2DataGridView();
            colCategory = new DataGridViewTextBoxColumn();
            colImage = new DataGridViewTextBoxColumn();
            colSubGroups = new DataGridViewTextBoxColumn();
            colVisible = new DataGridViewTextBoxColumn();
            colEdit = new DataGridViewTextBoxColumn();
            colAddChild = new DataGridViewTextBoxColumn();
            label2 = new Label();
            ((System.ComponentModel.ISupportInitialize)gridGroupData).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Navy;
            label1.Location = new Point(42, 16);
            label1.Name = "label1";
            label1.Size = new Size(168, 38);
            label1.TabIndex = 0;
            label1.Text = "Item Group";
            // 
            // btnCreate
            // 
            btnCreate.BorderRadius = 5;
            btnCreate.CustomizableEdges = customizableEdges1;
            btnCreate.DisabledState.BorderColor = Color.DarkGray;
            btnCreate.DisabledState.CustomBorderColor = Color.DarkGray;
            btnCreate.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnCreate.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnCreate.FillColor = Color.Navy;
            btnCreate.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCreate.ForeColor = Color.White;
            btnCreate.Location = new Point(1247, 16);
            btnCreate.Name = "btnCreate";
            btnCreate.ShadowDecoration.CustomizableEdges = customizableEdges2;
            btnCreate.Size = new Size(105, 39);
            btnCreate.TabIndex = 23;
            btnCreate.Text = "Create";
            // 
            // txtSearch
            // 
            txtSearch.BorderRadius = 5;
            txtSearch.CustomizableEdges = customizableEdges3;
            txtSearch.DefaultText = "";
            txtSearch.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtSearch.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtSearch.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtSearch.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtSearch.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtSearch.Font = new Font("Segoe UI", 9F);
            txtSearch.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtSearch.Location = new Point(1147, 86);
            txtSearch.Margin = new Padding(3, 4, 3, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search.................";
            txtSearch.SelectedText = "";
            txtSearch.ShadowDecoration.CustomizableEdges = customizableEdges4;
            txtSearch.Size = new Size(186, 38);
            txtSearch.TabIndex = 24;
            // 
            // gridGroupData
            // 
            gridGroupData.AllowUserToAddRows = false;
            dataGridViewCellStyle1.BackColor = Color.White;
            gridGroupData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            gridGroupData.BorderStyle = BorderStyle.FixedSingle;
            gridGroupData.CellBorderStyle = DataGridViewCellBorderStyle.SingleVertical;
            gridGroupData.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.White;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = Color.Black;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            gridGroupData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            gridGroupData.ColumnHeadersHeight = 45;
            gridGroupData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            gridGroupData.Columns.AddRange(new DataGridViewColumn[] { colCategory, colImage, colSubGroups, colVisible, colEdit, colAddChild });
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            gridGroupData.DefaultCellStyle = dataGridViewCellStyle3;
            gridGroupData.GridColor = Color.FromArgb(231, 229, 255);
            gridGroupData.Location = new Point(42, 142);
            gridGroupData.Name = "gridGroupData";
            gridGroupData.RowHeadersVisible = false;
            gridGroupData.RowHeadersWidth = 51;
            gridGroupData.RowTemplate.Height = 40;
            gridGroupData.Size = new Size(1291, 129);
            gridGroupData.TabIndex = 25;
            gridGroupData.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White;
            gridGroupData.ThemeStyle.HeaderStyle.BackColor = Color.White;
            gridGroupData.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.Single;
            gridGroupData.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            gridGroupData.ThemeStyle.HeaderStyle.ForeColor = Color.Black;
            gridGroupData.ThemeStyle.HeaderStyle.Height = 45;
            gridGroupData.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleVertical;
            gridGroupData.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9F);
            gridGroupData.ThemeStyle.RowsStyle.Height = 40;
            // 
            // colCategory
            // 
            colCategory.HeaderText = "Category";
            colCategory.MinimumWidth = 6;
            colCategory.Name = "colCategory";
            // 
            // colImage
            // 
            colImage.HeaderText = "Image";
            colImage.MinimumWidth = 6;
            colImage.Name = "colImage";
            // 
            // colSubGroups
            // 
            colSubGroups.HeaderText = "Number of Sub-groups";
            colSubGroups.MinimumWidth = 6;
            colSubGroups.Name = "colSubGroups";
            // 
            // colVisible
            // 
            colVisible.HeaderText = "Visible";
            colVisible.MinimumWidth = 6;
            colVisible.Name = "colVisible";
            // 
            // colEdit
            // 
            colEdit.HeaderText = "Edit";
            colEdit.MinimumWidth = 6;
            colEdit.Name = "colEdit";
            // 
            // colAddChild
            // 
            colAddChild.HeaderText = "Add Child";
            colAddChild.MinimumWidth = 6;
            colAddChild.Name = "colAddChild";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.White;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.ForeColor = SystemColors.ControlDark;
            label2.Location = new Point(638, 226);
            label2.Name = "label2";
            label2.Size = new Size(85, 28);
            label2.TabIndex = 26;
            label2.Text = "No Data";
            // 
            // ItemGroupList
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(label2);
            Controls.Add(gridGroupData);
            Controls.Add(txtSearch);
            Controls.Add(btnCreate);
            Controls.Add(label1);
            Name = "ItemGroupList";
            Size = new Size(1385, 790);
            Load += ItemGroup_Load;
            ((System.ComponentModel.ISupportInitialize)gridGroupData).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Guna.UI2.WinForms.Guna2Button btnCreate;
        private Guna.UI2.WinForms.Guna2TextBox txtSearch;
        private Guna.UI2.WinForms.Guna2DataGridView gridGroupData;
        private DataGridViewTextBoxColumn colCategory;
        private DataGridViewTextBoxColumn colImage;
        private DataGridViewTextBoxColumn colSubGroups;
        private DataGridViewTextBoxColumn colVisible;
        private DataGridViewTextBoxColumn colEdit;
        private DataGridViewTextBoxColumn colAddChild;
        private Label label2;
    }
}
