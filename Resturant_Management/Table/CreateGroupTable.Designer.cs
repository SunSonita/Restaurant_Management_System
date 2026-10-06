namespace Resturant_Management.Table
{
    partial class CreateGroupTable
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();

            panelInformationItem = new Guna.UI2.WinForms.Guna2ShadowPanel();
            lbTableGroup = new Label();
            comboTableGroup = new Guna.UI2.WinForms.Guna2ComboBox();
            label1 = new Label();
            PicItem = new Guna.UI2.WinForms.Guna2PictureBox();
            txtName = new Guna.UI2.WinForms.Guna2TextBox();
            txtCode = new Guna.UI2.WinForms.Guna2TextBox();
            btnBack = new Guna.UI2.WinForms.Guna2Button();
            btnSave = new Guna.UI2.WinForms.Guna2Button();
            label2 = new Label();

            panelInformationItem.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PicItem).BeginInit();
            SuspendLayout();

            // panelInformationItem
            panelInformationItem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelInformationItem.BackColor = Color.Transparent;
            panelInformationItem.Controls.Add(lbTableGroup);
            panelInformationItem.Controls.Add(comboTableGroup);
            panelInformationItem.Controls.Add(label1);
            panelInformationItem.Controls.Add(PicItem);
            panelInformationItem.Controls.Add(txtName);
            panelInformationItem.Controls.Add(txtCode);
            panelInformationItem.FillColor = Color.White;
            panelInformationItem.Location = new Point(30, 85);
            panelInformationItem.Name = "panelInformationItem";
            panelInformationItem.Radius = 10;
            panelInformationItem.ShadowColor = Color.Gray;
            panelInformationItem.Size = new Size(1200, 340);
            panelInformationItem.TabIndex = 29;

            // lbTableGroup
            lbTableGroup.AutoSize = true;
            lbTableGroup.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            lbTableGroup.ForeColor = Color.FromArgb(68, 88, 112);
            lbTableGroup.Location = new Point(45, 202);
            lbTableGroup.Name = "lbTableGroup";
            lbTableGroup.Size = new Size(53, 28);
            lbTableGroup.TabIndex = 37;
            lbTableGroup.Text = "Type";

            // comboTableGroup
            comboTableGroup.BackColor = Color.Transparent;
            comboTableGroup.BorderRadius = 6;
            comboTableGroup.CustomizableEdges = customizableEdges1;
            comboTableGroup.DrawMode = DrawMode.OwnerDrawFixed;
            comboTableGroup.DropDownStyle = ComboBoxStyle.DropDownList;
            comboTableGroup.FocusedColor = Color.FromArgb(21, 119, 214);
            comboTableGroup.FocusedState.BorderColor = Color.FromArgb(21, 119, 214);
            comboTableGroup.Font = new Font("Segoe UI", 11F);
            comboTableGroup.ForeColor = Color.FromArgb(68, 88, 112);
            comboTableGroup.ItemHeight = 35;
            comboTableGroup.Items.AddRange(new object[] { "Main Table", "Delivery", "Take Out" });
            comboTableGroup.Location = new Point(40, 215);
            comboTableGroup.Name = "comboTableGroup";
            comboTableGroup.ShadowDecoration.CustomizableEdges = customizableEdges2;
            comboTableGroup.Size = new Size(600, 55);
            comboTableGroup.StartIndex = -1;
            comboTableGroup.TabIndex = 36;
            comboTableGroup.SelectedIndexChanged += comboTableGroup_SelectedIndexChanged;

            // label1
            label1.AutoSize = true;
            label1.Cursor = Cursors.Hand;
            label1.Font = new Font("Segoe UI", 11F, FontStyle.Underline);
            label1.ForeColor = Color.FromArgb(21, 119, 214);
            label1.Location = new Point(780, 265);
            label1.Name = "label1";
            label1.Size = new Size(205, 30);
            label1.TabIndex = 30;
            label1.Text = "Click to choose image";

            // PicItem
            PicItem.BackColor = Color.White;
            PicItem.BorderRadius = 6;
            PicItem.BorderStyle = BorderStyle.FixedSingle;
            PicItem.Cursor = Cursors.Hand;
            PicItem.CustomizableEdges = customizableEdges3;
            PicItem.ImageRotate = 0F;
            PicItem.Location = new Point(720, 35);
            PicItem.Name = "PicItem";
            PicItem.ShadowDecoration.CustomizableEdges = customizableEdges4;
            PicItem.Size = new Size(320, 215);
            PicItem.SizeMode = PictureBoxSizeMode.StretchImage;
            PicItem.TabIndex = 29;
            PicItem.TabStop = false;

            // txtName
            txtName.BorderRadius = 6;
            txtName.CustomizableEdges = customizableEdges5;
            txtName.DefaultText = "";
            txtName.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtName.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtName.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtName.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtName.FocusedState.BorderColor = Color.FromArgb(21, 119, 214);
            txtName.Font = new Font("Segoe UI", 11F);
            txtName.HoverState.BorderColor = Color.FromArgb(21, 119, 214);
            txtName.Location = new Point(40, 125);
            txtName.Margin = new Padding(4, 5, 4, 5);
            txtName.Name = "txtName";
            txtName.PlaceholderText = "Name";
            txtName.SelectedText = "";
            txtName.ShadowDecoration.CustomizableEdges = customizableEdges6;
            txtName.Size = new Size(600, 55);
            txtName.TabIndex = 24;

            // txtCode
            txtCode.BorderRadius = 6;
            txtCode.CustomizableEdges = customizableEdges7;
            txtCode.DefaultText = "";
            txtCode.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtCode.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtCode.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtCode.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtCode.FocusedState.BorderColor = Color.FromArgb(21, 119, 214);
            txtCode.Font = new Font("Segoe UI", 11F);
            txtCode.HoverState.BorderColor = Color.FromArgb(21, 119, 214);
            txtCode.Location = new Point(40, 35);
            txtCode.Margin = new Padding(4, 5, 4, 5);
            txtCode.Name = "txtCode";
            txtCode.PlaceholderText = "Code";
            txtCode.SelectedText = "";
            txtCode.ShadowDecoration.CustomizableEdges = customizableEdges8;
            txtCode.Size = new Size(600, 55);
            txtCode.TabIndex = 22;

            // btnBack
            btnBack.BorderRadius = 6;
            btnBack.DisabledState.BorderColor = Color.DarkGray;
            btnBack.DisabledState.CustomBorderColor = Color.DarkGray;
            btnBack.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnBack.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnBack.FillColor = Color.FromArgb(240, 140, 0);
            btnBack.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnBack.ForeColor = Color.White;
            btnBack.Location = new Point(160, 445);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(115, 45);
            btnBack.TabIndex = 31;
            btnBack.Text = "Back";

            // btnSave
            btnSave.BorderRadius = 6;
            btnSave.DisabledState.BorderColor = Color.DarkGray;
            btnSave.DisabledState.CustomBorderColor = Color.DarkGray;
            btnSave.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnSave.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnSave.FillColor = Color.FromArgb(21, 119, 214);
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(30, 445);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(115, 45);
            btnSave.TabIndex = 30;
            btnSave.Text = "Save";

            // label2
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            label2.ForeColor = Color.FromArgb(21, 119, 214);
            label2.Location = new Point(30, 20);
            label2.Name = "label2";
            label2.Size = new Size(313, 45);
            label2.TabIndex = 39;
            label2.Text = "Create Group Table";

            // CreateGroupTable
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(label2);
            Controls.Add(btnBack);
            Controls.Add(btnSave);
            Controls.Add(panelInformationItem);
            Name = "CreateGroupTable";
            Size = new Size(1260, 650);
            Load += CreateGroupTable_Load_1;
            panelInformationItem.ResumeLayout(false);
            panelInformationItem.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PicItem).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Guna.UI2.WinForms.Guna2ShadowPanel panelInformationItem;
        private Label label1;
        private Guna.UI2.WinForms.Guna2PictureBox PicItem;
        private Guna.UI2.WinForms.Guna2TextBox txtName;
        private Guna.UI2.WinForms.Guna2TextBox txtCode;
        private Label lbTableGroup;
        private Guna.UI2.WinForms.Guna2ComboBox comboTableGroup;
        private Guna.UI2.WinForms.Guna2Button btnBack;
        private Guna.UI2.WinForms.Guna2Button btnSave;
        private Label label2;
    }
}