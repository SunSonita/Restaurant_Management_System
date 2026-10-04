using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Table
{
    public partial class GroupTable : UserControl
    {
        public GroupTable()
        {
            InitializeComponent();
            LoadGroupTableData();
            btnCreate.Click += BtnCreate_Click;
            txtSearch.TextChanged += (s, e) => LoadGroupTableData();
            dgvGroupTable.CellClick += DgvGroupTable_CellClick;
            this.Load += GroupTable_Load;
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadGroupTableData(); };
        }

        private void DgvGroupTable_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 0)
            {
                int groupId = dgvGroupTable.Rows[e.RowIndex].Tag != null ? Convert.ToInt32(dgvGroupTable.Rows[e.RowIndex].Tag) : 0;
                if (groupId > 0)
                {
                    Control? parent = this.Parent;
                    if (parent != null)
                    {
                        parent.Controls.Clear();
                        UpdatGroupTable updateView = new UpdatGroupTable(groupId) { Dock = DockStyle.Fill };
                        parent.Controls.Add(updateView);
                        updateView.BringToFront();
                    }
                }
            }
        }

        private void GroupTable_Load(object? sender, EventArgs e)
        {
            LoadGroupTableData();
        }

        private void LoadGroupTableData()
        {
            dgvGroupTable.Rows.Clear();
            string search = txtSearch?.Text?.Trim() ?? "";

            try
            {
                string sql = @"
SELECT TableGroupID, GroupCode, GroupName, GroupType, ImagePath 
FROM dbo.TABLE_GROUP 
WHERE (@Search = '' OR GroupName LIKE @Pattern OR GroupCode LIKE @Pattern)
ORDER BY TableGroupID;";

                DataTable dt = DbHelper.ExecuteQuery(sql,
                    new SqlParameter("@Search", search),
                    new SqlParameter("@Pattern", $"%{search}%"));

                foreach (DataRow r in dt.Rows)
                {
                    int rowIndex = dgvGroupTable.Rows.Add(
                        "Edit",
                        r["GroupCode"]?.ToString(),
                        r["GroupName"]?.ToString(),
                        r["GroupType"]?.ToString(),
                        r["ImagePath"]?.ToString()
                    );
                    dgvGroupTable.Rows[rowIndex].Tag = Convert.ToInt32(r["TableGroupID"]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading group tables: {ex.Message}");
            }
        }

        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            Control? parent = this.Parent;
            if (parent != null)
            {
                parent.Controls.Clear();
                CreateGroupTable createView = new CreateGroupTable { Dock = DockStyle.Fill };
                parent.Controls.Add(createView);
                createView.BringToFront();
            }
        }
    }
}
