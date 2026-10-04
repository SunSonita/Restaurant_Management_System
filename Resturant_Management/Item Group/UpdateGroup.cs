using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Item_Group
{
    public partial class UpdateGroup : UserControl
    {
        private int _groupId;
        private string _selectedImagePath = "";

        public UpdateGroup() : this(1)
        {
        }

        public UpdateGroup(int groupId)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            _groupId = groupId;

            btnUpdate.Click += BtnUpdate_Click;
            btnBack.Click += BtnBack_Click;
            PicItem.Click += PicItem_Click;
            this.Load += UpdateGroup_Load;
        }

        private void UpdateGroup_Load(object? sender, EventArgs e)
        {
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            LoadGroupData();
        }

        private void LoadGroupData()
        {
            try
            {
                DataTable dt = DbHelper.ExecuteQuery(
                    "SELECT GroupID, GroupCode, GroupName, ImagePath, IsVisible FROM dbo.ITEM_GROUP WHERE GroupID = @ID",
                    new SqlParameter("@ID", _groupId));

                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtCode.Text = r["GroupCode"]?.ToString() ?? "";
                    txtCode.ReadOnly = true;
                    txtName.Text = r["GroupName"]?.ToString() ?? "";
                    checkVisible.Checked = r["IsVisible"] != DBNull.Value && Convert.ToBoolean(r["IsVisible"]);

                    string? img = r["ImagePath"]?.ToString();
                    if (!string.IsNullOrEmpty(img) && System.IO.File.Exists(img))
                    {
                        try
                        {
                            _selectedImagePath = img;
                            PicItem.Image = Image.FromFile(img);
                        }
                        catch { }
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
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _selectedImagePath = ofd.FileName;
                    PicItem.Image = Image.FromFile(_selectedImagePath);
                }
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            bool isVisible = checkVisible.Checked;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a Group Name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            try
            {
                string updateSql = @"
UPDATE dbo.ITEM_GROUP SET 
    GroupName = @Name,
    IsVisible = @Visible,
    ImagePath = CASE WHEN @Image IS NOT NULL THEN @Image ELSE ImagePath END
WHERE GroupID = @ID;";

                DbHelper.ExecuteNonQuery(updateSql,
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Visible", isVisible),
                    new SqlParameter("@Image", string.IsNullOrEmpty(_selectedImagePath) ? (object)DBNull.Value : _selectedImagePath),
                    new SqlParameter("@ID", _groupId)
                );

                MessageBox.Show("Item Group updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NavigateBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating group: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
