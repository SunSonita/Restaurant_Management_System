using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Item_Group
{
    public partial class CreateGroup : UserControl
    {
        private int? _parentGroupId;
        private string _selectedImagePath = "";

        public CreateGroup() : this(null)
        {
        }

        public CreateGroup(int? parentGroupId)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            _parentGroupId = parentGroupId;

            btnSave.Click += BtnSave_Click;
            btnBack.Click += BtnBack_Click;
            PicItem.Click += PicItem_Click;
            checkVisible.Checked = true;
        }

        private void CreateGroup_Load(object? sender, EventArgs e)
        {
        }

        private void PicItem_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _selectedImagePath = ofd.FileName;
                    PicItem.Image = Image.FromFile(_selectedImagePath);
                }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            string name = txtName.Text.Trim();
            bool isVisible = checkVisible.Checked;

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
                object? exists = DbHelper.ExecuteScalar("SELECT COUNT(1) FROM dbo.ITEM_GROUP WHERE GroupCode = @Code", new SqlParameter("@Code", code));
                if (exists != null && Convert.ToInt32(exists) > 0)
                {
                    MessageBox.Show($"Group code '{code}' already exists.", "Duplicate Code", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string insertSql = @"
INSERT INTO dbo.ITEM_GROUP (ParentGroupID, GroupCode, GroupName, ImagePath, IsVisible)
VALUES (@Parent, @Code, @Name, @Image, @Visible);";

                DbHelper.ExecuteNonQuery(insertSql,
                    new SqlParameter("@Parent", (object?)_parentGroupId ?? DBNull.Value),
                    new SqlParameter("@Code", code),
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Image", string.IsNullOrEmpty(_selectedImagePath) ? (object)DBNull.Value : _selectedImagePath),
                    new SqlParameter("@Visible", isVisible)
                );

                MessageBox.Show("Item Group created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NavigateBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving group: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                ItemGroupList list = new ItemGroupList { Dock = DockStyle.Fill };
                parent.Controls.Add(list);
                list.BringToFront();
            }
        }
    }
}
