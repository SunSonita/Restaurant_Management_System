using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Table
{
    public partial class CreateTable : UserControl
    {
        private string _selectedImagePath = "";

        public CreateTable()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            btnSave.Click += BtnSave_Click;
            btnBack.Click += BtnBack_Click;
            PicItem.Click += PicItem_Click;
            this.Load += CreateTable_Load;
        }

        private void CreateTable_Load(object? sender, EventArgs e)
        {
            try
            {
                DataTable dt = DbHelper.ExecuteQuery("SELECT TableGroupID, GroupName FROM dbo.TABLE_GROUP ORDER BY GroupName");
                comboTableGroup.DataSource = dt;
                comboTableGroup.DisplayMember = "GroupName";
                comboTableGroup.ValueMember = "TableGroupID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading table groups: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            string name = txtName.Text.Trim();

            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Please enter a Table Code.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCode.Focus();
                return;
            }

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
INSERT INTO dbo.DINING_TABLE (TableGroupID, TableCode, TableName, ImagePath, IsActive)
VALUES (@GroupID, @Code, @Name, @Img, 1);";

                int rows = DbHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@GroupID", groupId),
                    new SqlParameter("@Code", code),
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Img", string.IsNullOrEmpty(_selectedImagePath) ? (object)DBNull.Value : _selectedImagePath));

                if (rows > 0)
                {
                    MessageBox.Show("Table created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    NavigateBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating table: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
