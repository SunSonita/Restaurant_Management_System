using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Table
{
    public partial class CreateTableList : Form
    {
        public CreateTableList()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;

            btnSave.Click += BtnSave_Click;
            label1.Click += (s, e) => this.Close();
            this.Load += CreateTableList_Load;
        }

        private void CreateTableList_Load(object? sender, EventArgs e)
        {
            try
            {
                DataTable dt = DbHelper.ExecuteQuery("SELECT TableGroupID, GroupName FROM dbo.TABLE_GROUP ORDER BY GroupName");
                comboTableGroup.DataSource = dt;
                comboTableGroup.DisplayMember = "GroupName";
                comboTableGroup.ValueMember = "TableGroupID";

                txtPrefixName.Text = "Table-";
                txtNumberfrom.Text = "1";
                txtNumberTo.Text = "4";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading groups: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string prefix = txtPrefixName.Text.Trim();
            if (!int.TryParse(txtNumberfrom.Text.Trim(), out int fromNum) ||
                !int.TryParse(txtNumberTo.Text.Trim(), out int toNum))
            {
                MessageBox.Show("Please enter valid start and end numbers.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (fromNum > toNum)
            {
                MessageBox.Show("From number cannot be greater than To number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int groupId = comboTableGroup.SelectedValue != null ? Convert.ToInt32(comboTableGroup.SelectedValue) : 1;

            int createdCount = 0;
            try
            {
                using var conn = DbHelper.GetConnection();
                conn.Open();
                using var trans = conn.BeginTransaction();

                for (int i = fromNum; i <= toNum; i++)
                {
                    string code = $"T{i}";
                    string name = $"{prefix}{i}";

                    string insertSql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.DINING_TABLE WHERE TableCode = @Code)
BEGIN
    INSERT INTO dbo.DINING_TABLE (TableGroupID, TableCode, TableName, IsActive)
    VALUES (@GroupID, @Code, @Name, 1);
END";
                    using var cmd = new SqlCommand(insertSql, conn, trans);
                    cmd.Parameters.AddWithValue("@GroupID", groupId);
                    cmd.Parameters.AddWithValue("@Code", code);
                    cmd.Parameters.AddWithValue("@Name", name);
                    int r = cmd.ExecuteNonQuery();
                    if (r > 0) createdCount++;
                }

                trans.Commit();

                MessageBox.Show($"Successfully created {createdCount} tables.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating tables: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
