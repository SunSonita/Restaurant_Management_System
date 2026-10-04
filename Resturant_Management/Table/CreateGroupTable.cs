using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Table
{
    public partial class CreateGroupTable : UserControl
    {
        private string _selectedImagePath = "";

        public CreateGroupTable()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            btnSave.Click += BtnSave_Click;
            btnBack.Click += BtnBack_Click;
            PicItem.Click += PicItem_Click;
            this.Load += CreateGroupTable_Load;
        }

        private void CreateGroupTable_Load(object? sender, EventArgs e)
        {
            comboTableGroup.Items.Clear();
            comboTableGroup.Items.Add("MainTable");
            comboTableGroup.Items.Add("Delivery");
            comboTableGroup.Items.Add("TakeOut");
            comboTableGroup.SelectedIndex = 0;
        }

        private void PicItem_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _selectedImagePath = ofd.FileName;
                    try { PicItem.Image = Image.FromFile(_selectedImagePath); } catch { }
                }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            string name = txtName.Text.Trim();
            string type = comboTableGroup.SelectedItem?.ToString() ?? "MainTable";

            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Please enter a Group Code.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCode.Focus();
                return;
            }

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a Group Name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            try
            {
                string sql = @"
INSERT INTO dbo.TABLE_GROUP (GroupCode, GroupName, GroupType, ImagePath)
VALUES (@Code, @Name, @Type, @Img);";

                int rows = DbHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@Code", code),
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Type", type),
                    new SqlParameter("@Img", string.IsNullOrEmpty(_selectedImagePath) ? (object)DBNull.Value : _selectedImagePath));

                if (rows > 0)
                {
                    MessageBox.Show("Table Group created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    NavigateBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating table group: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBack_Click(object? sender, EventArgs e)
        {
            NavigateBack();
        }

        private void NavigateBack()
        {
            Control? parent = this.Parent;
            if (parent != null)
            {
                parent.Controls.Clear();
                GroupTable groupTableView = new GroupTable { Dock = DockStyle.Fill };
                parent.Controls.Add(groupTableView);
                groupTableView.BringToFront();
            }
        }

        private void comboTableGroup_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}
