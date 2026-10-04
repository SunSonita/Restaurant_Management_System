using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Table
{
    public partial class UpdatGroupTable : UserControl
    {
        private int _groupId;
        private string _selectedImagePath = "";

        public UpdatGroupTable() : this(1)
        {
        }

        public UpdatGroupTable(int groupId)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            _groupId = groupId;

            btnUpdate.Click += BtnUpdate_Click;
            btnBack.Click += BtnBack_Click;
            PicItem.Click += PicItem_Click;
            this.Load += UpdatGroupTable_Load;
        }

        private void UpdatGroupTable_Load(object? sender, EventArgs e)
        {
            comboTableGroup.Items.Clear();
            comboTableGroup.Items.Add("MainTable");
            comboTableGroup.Items.Add("Delivery");
            comboTableGroup.Items.Add("TakeOut");
            comboTableGroup.SelectedIndex = 0;

            LoadGroupData();
        }

        private void LoadGroupData()
        {
            try
            {
                string sql = "SELECT GroupCode, GroupName, GroupType, ImagePath FROM dbo.TABLE_GROUP WHERE TableGroupID = @ID";
                DataTable dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@ID", _groupId));
                if (dt.Rows.Count > 0)
                {
                    txtCode.Text = dt.Rows[0]["GroupCode"]?.ToString();
                    txtName.Text = dt.Rows[0]["GroupName"]?.ToString();
                    string? type = dt.Rows[0]["GroupType"]?.ToString();
                    if (!string.IsNullOrEmpty(type) && comboTableGroup.Items.Contains(type))
                        comboTableGroup.SelectedItem = type;

                    _selectedImagePath = dt.Rows[0]["ImagePath"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(_selectedImagePath) && System.IO.File.Exists(_selectedImagePath))
                    {
                        try { PicItem.Image = Image.FromFile(_selectedImagePath); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading group: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            string name = txtName.Text.Trim();
            string type = comboTableGroup.SelectedItem?.ToString() ?? "MainTable";

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a Group Name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            try
            {
                string sql = @"
UPDATE dbo.TABLE_GROUP 
SET GroupCode = @Code, GroupName = @Name, GroupType = @Type, ImagePath = @Img
WHERE TableGroupID = @ID;";

                int rows = DbHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@Code", code),
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Type", type),
                    new SqlParameter("@Img", string.IsNullOrEmpty(_selectedImagePath) ? (object)DBNull.Value : _selectedImagePath),
                    new SqlParameter("@ID", _groupId));

                if (rows > 0)
                {
                    MessageBox.Show("Table Group updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    NavigateBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating table group: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
