using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Table
{
    public partial class GroupTable : UserControl
    {
        private bool _isInitialized = false;

        public GroupTable()
        {
            InitializeComponent();
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            this.Load += GroupTable_Load;
            this.VisibleChanged += (s, e) =>
            {
                if (this.Visible && !DesignTimeHelper.IsInDesignMode(this))
                {
                    if (!_isInitialized)
                        InitRuntime();
                    else
                        LoadGroupTableData();
                }
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignTimeHelper.IsInDesignMode(this) && !_isInitialized)
            {
                InitRuntime();
            }
        }

        private void GroupTable_Load(object? sender, EventArgs e)
        {
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            InitRuntime();
        }

        private void InitRuntime()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            BuildResponsiveLayout();
            LoadGroupTableData();

            btnCreate.Click += BtnCreate_Click;
            txtSearch.TextChanged += (s, e) => LoadGroupTableData();
            dgvGroupTable.CellClick += DgvGroupTable_CellClick;
        }

        private void BuildResponsiveLayout()
        {
            this.SuspendLayout();
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            label1.Text = "Group Table";
            label1.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(21, 119, 214);
            label1.AutoSize = true;
            label1.Location = new Point(15, 12);

            btnCreate.Text = "+ Create Group";
            btnCreate.Size = new Size(130, 38);
            btnCreate.BorderRadius = 6;
            btnCreate.FillColor = Color.FromArgb(21, 119, 214);
            btnCreate.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnCreate.Cursor = Cursors.Hand;

            txtSearch.Size = new Size(220, 38);
            txtSearch.PlaceholderText = "Search group...";
            txtSearch.BorderRadius = 6;

            pnlHeader.Controls.Add(label1);
            pnlHeader.Controls.Add(txtSearch);
            pnlHeader.Controls.Add(btnCreate);

            void PositionHeader()
            {
                btnCreate.Location = new Point(pnlHeader.Width - btnCreate.Width - 20, 10);
                txtSearch.Location = new Point(pnlHeader.Width - btnCreate.Width - txtSearch.Width - 30, 10);
            }
            pnlHeader.Resize += (s, ev) => PositionHeader();
            PositionHeader();

            Panel pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20, 6, 20, 20)
            };

            dgvGroupTable.Dock = DockStyle.Fill;
            dgvGroupTable.BackgroundColor = Color.White;
            dgvGroupTable.BorderStyle = BorderStyle.None;
            dgvGroupTable.EnableHeadersVisualStyles = false;
            dgvGroupTable.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
            dgvGroupTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(50, 60, 75);
            dgvGroupTable.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvGroupTable.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 247, 250);
            dgvGroupTable.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(50, 60, 75);
            dgvGroupTable.ColumnHeadersHeight = 44;
            dgvGroupTable.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvGroupTable.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvGroupTable.GridColor = Color.FromArgb(235, 238, 242);
            dgvGroupTable.RowTemplate.Height = 55;
            dgvGroupTable.AllowUserToAddRows = false;
            dgvGroupTable.AllowUserToDeleteRows = false;
            dgvGroupTable.AllowUserToResizeRows = false;
            dgvGroupTable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGroupTable.MultiSelect = false;
            dgvGroupTable.RowHeadersVisible = false;
            dgvGroupTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            pnlGrid.Controls.Add(dgvGroupTable);

            this.Controls.Clear();
            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(true);
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
                    string imagePath = r["ImagePath"]?.ToString() ?? "";
                    Image? groupImage = null;

                    if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                    {
                        try
                        {
                            using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                            {
                                using (var tempImg = Image.FromStream(fs))
                                {
                                    groupImage = new Bitmap(tempImg, new Size(40, 40));
                                }
                            }
                        }
                        catch
                        {
                            groupImage = null;
                        }
                    }

                    int rowIndex = dgvGroupTable.Rows.Add(
                        "➔",
                        groupImage,
                        r["GroupCode"]?.ToString(),
                        r["GroupName"]?.ToString(),
                        r["GroupType"]?.ToString()
                    );

                    // Align left for Edit column
                    dgvGroupTable.Rows[rowIndex].Cells["colAction"].Style.ForeColor = Color.FromArgb(21, 119, 214);
                    dgvGroupTable.Rows[rowIndex].Cells["colAction"].Style.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                    dgvGroupTable.Rows[rowIndex].Cells["colAction"].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

                    // Align left for Image column
                    dgvGroupTable.Rows[rowIndex].Cells["colImage"].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

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

        private void GroupTable_Load_1(object sender, EventArgs e)
        {
        }
    }
}