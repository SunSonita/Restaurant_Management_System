using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Table
{
    public partial class EditTable : UserControl
    {
        private int _tableId;
        private string _selectedImagePath = "";

        public EditTable() : this(1)
        {
        }

        public EditTable(int tableId)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            _tableId = tableId;

            btnUpdate.Click += BtnUpdate_Click;
            btnBack.Click += BtnBack_Click;
            PicItem.Click += PicItem_Click;
            this.Load += EditTable_Load;
        }

        private void EditTable_Load(object? sender, EventArgs e)
        {
            try
            {
                DataTable dtGroups = DbHelper.ExecuteQuery("SELECT TableGroupID, GroupName FROM dbo.TABLE_GROUP ORDER BY GroupName");
                comboTableGroup.DataSource = dtGroups;
                comboTableGroup.DisplayMember = "GroupName";
                comboTableGroup.ValueMember = "TableGroupID";

                LoadTableData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTableData()
        {
            try
            {
                string sql = "SELECT TableGroupID, TableCode, TableName, ImagePath FROM dbo.DINING_TABLE WHERE TableID = @ID";
                DataTable dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@ID", _tableId));
                if (dt.Rows.Count > 0)
                {
                    txtCode.Text = dt.Rows[0]["TableCode"]?.ToString();
                    txtName.Text = dt.Rows[0]["TableName"]?.ToString();
                    if (dt.Rows[0]["TableGroupID"] != DBNull.Value)
                    {
                        comboTableGroup.SelectedValue = Convert.ToInt32(dt.Rows[0]["TableGroupID"]);
                    }

                    _selectedImagePath = dt.Rows[0]["ImagePath"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(_selectedImagePath) && System.IO.File.Exists(_selectedImagePath))
                    {
                        try { PicItem.Image = Image.FromFile(_selectedImagePath); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading table: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a Table Name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            int groupId = comboTableGroup.SelectedValue != null ? Convert.ToInt32(comboTableGroup.SelectedValue) : 1;

            try
            {
                string sql = @"
UPDATE dbo.DINING_TABLE 
SET TableGroupID = @GroupID, TableCode = @Code, TableName = @Name, ImagePath = @Img
WHERE TableID = @ID;";

                int rows = DbHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@GroupID", groupId),
                    new SqlParameter("@Code", code),
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Img", string.IsNullOrEmpty(_selectedImagePath) ? (object)DBNull.Value : _selectedImagePath),
                    new SqlParameter("@ID", _tableId));

                if (rows > 0)
                {
                    MessageBox.Show("Table updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    NavigateBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating table: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                TableList tableListView = new TableList { Dock = DockStyle.Fill };
                parent.Controls.Add(tableListView);
                tableListView.BringToFront();
            }
        }
    }
}
