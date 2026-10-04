using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Table
{
    public partial class TableList : UserControl
    {
        private readonly System.Collections.Generic.HashSet<int> _selectedTableIds = new();
        private bool _isInitialized = false;

        public TableList()
        {
            InitializeComponent();
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            this.Load += CreateTable_Load;
            this.VisibleChanged += (s, e) =>
            {
                if (this.Visible && !DesignTimeHelper.IsInDesignMode(this))
                {
                    if (!_isInitialized)
                        InitRuntime();
                    else
                        LoadTableData();
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

        private void CreateTable_Load(object? sender, EventArgs e)
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
            SetupComboBox();
            SetupDataGridView();
            LoadTableData();

            btnCreate.Click += BtnCreate_Click;
            btnCreateTableList.Click += BtnCreateTableList_Click;
            btnSetImageAll.Click += BtnSetImageAll_Click;
            btnDeletebySelect.Click += BtnDeletebySelect_Click;
            txtSearch.TextChanged += (s, e) => LoadTableData();
        }

        private void BuildResponsiveLayout()
        {
            this.SuspendLayout();
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // 1. Top Title Bar (Dock = Top, Height = 58)
            Panel pnlTitle = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            label1.Text = "Table List";
            label1.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(21, 119, 214);
            label1.AutoSize = true;
            label1.Location = new Point(15, 12);

            btnCreate.Text = "+ Create Table";
            btnCreate.Size = new Size(130, 38);
            btnCreate.BorderRadius = 6;
            btnCreate.FillColor = Color.FromArgb(21, 119, 214);
            btnCreate.ForeColor = Color.White;
            btnCreate.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnCreate.Cursor = Cursors.Hand;

            pnlTitle.Controls.Add(label1);
            pnlTitle.Controls.Add(btnCreate);

            void PositionTitle()
            {
                btnCreate.Location = new Point(pnlTitle.Width - btnCreate.Width - 20, 10);
            }
            pnlTitle.Resize += (s, ev) => PositionTitle();
            PositionTitle();

            // 2. Action / Filter Toolbar (Dock = Top, Height = 54)
            Panel pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = Color.White
            };

            btnCreateTableList.Text = "Batch Create";
            btnCreateTableList.Size = new Size(130, 38);
            btnCreateTableList.BorderRadius = 6;
            btnCreateTableList.FillColor = Color.FromArgb(21, 119, 214);
            btnCreateTableList.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCreateTableList.Cursor = Cursors.Hand;
            btnCreateTableList.Margin = new Padding(0, 0, 8, 0);

            btnSetImageAll.Text = "Set Image for Selected";
            btnSetImageAll.Size = new Size(185, 38);
            btnSetImageAll.BorderRadius = 6;
            btnSetImageAll.FillColor = Color.FromArgb(21, 119, 214);
            btnSetImageAll.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSetImageAll.Cursor = Cursors.Hand;
            btnSetImageAll.Margin = new Padding(0, 0, 8, 0);

            comboGroupTable.Size = new Size(180, 38);
            comboGroupTable.BorderRadius = 6;
            comboGroupTable.Font = new Font("Segoe UI", 9.5F);
            comboGroupTable.Margin = new Padding(0, 0, 8, 0);

            btnDeletebySelect.Text = "Delete Selected";
            btnDeletebySelect.Size = new Size(140, 38);
            btnDeletebySelect.BorderRadius = 6;
            btnDeletebySelect.FillColor = Color.FromArgb(239, 83, 80);
            btnDeletebySelect.HoverState.FillColor = Color.FromArgb(211, 47, 47);
            btnDeletebySelect.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnDeletebySelect.Cursor = Cursors.Hand;
            btnDeletebySelect.Margin = new Padding(0, 0, 8, 0);

            txtSearch.Size = new Size(200, 38);
            txtSearch.PlaceholderText = "Search table...";
            txtSearch.BorderRadius = 6;
            txtSearch.Font = new Font("Segoe UI", 9F);

            FlowLayoutPanel flowLeft = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(20, 8, 0, 6)
            };
            flowLeft.Controls.Add(btnCreateTableList);
            flowLeft.Controls.Add(btnSetImageAll);
            flowLeft.Controls.Add(comboGroupTable);
            flowLeft.Controls.Add(btnDeletebySelect);

            pnlToolbar.Controls.Add(flowLeft);
            pnlToolbar.Controls.Add(txtSearch);

            void RepositionSearch()
            {
                int minX = flowLeft.Right + 10;
                int targetX = pnlToolbar.Width - txtSearch.Width - 20;
                txtSearch.Location = new Point(Math.Max(minX, targetX), 8);
            }
            pnlToolbar.Resize += (s, ev) => RepositionSearch();
            flowLeft.Resize += (s, ev) => RepositionSearch();
            RepositionSearch();

            // 3. DataGridView Container (Dock = Fill)
            Panel pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20, 6, 20, 20)
            };

            dgvDataTableList.Dock = DockStyle.Fill;
            pnlGrid.Controls.Add(dgvDataTableList);

            this.Controls.Clear();
            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlToolbar);
            this.Controls.Add(pnlTitle);

            this.ResumeLayout(true);
        }

        private void dgvDataTableList_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
        }

        private void SetupComboBox()
        {
            if (comboGroupTable != null)
            {
                comboGroupTable.SelectedIndexChanged -= ComboGroupTable_SelectedIndexChanged;
                comboGroupTable.Items.Clear();
                comboGroupTable.Items.Add("All Group Table");

                try
                {
                    DataTable dt = DbHelper.ExecuteQuery("SELECT GroupName FROM dbo.TABLE_GROUP ORDER BY GroupName");
                    foreach (DataRow r in dt.Rows)
                    {
                        string? name = r["GroupName"]?.ToString();
                        if (!string.IsNullOrEmpty(name)) comboGroupTable.Items.Add(name);
                    }
                }
                catch { }

                comboGroupTable.SelectedIndex = 0;
                comboGroupTable.SelectedIndexChanged += ComboGroupTable_SelectedIndexChanged;
            }
        }

        private void ComboGroupTable_SelectedIndexChanged(object? sender, EventArgs e)
        {
            LoadTableData();
        }

        private void SetupDataGridView()
        {
            dgvDataTableList.EnableHeadersVisualStyles = false;
            dgvDataTableList.BackgroundColor = Color.White;
            dgvDataTableList.BorderStyle = BorderStyle.None;

            dgvDataTableList.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
            dgvDataTableList.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(50, 60, 75);
            dgvDataTableList.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvDataTableList.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 247, 250);
            dgvDataTableList.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(50, 60, 75);
            dgvDataTableList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvDataTableList.ColumnHeadersHeight = 44;
            dgvDataTableList.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            dgvDataTableList.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvDataTableList.GridColor = Color.FromArgb(235, 238, 242);
            dgvDataTableList.RowTemplate.Height = 54;
            dgvDataTableList.AllowUserToAddRows = false;
            dgvDataTableList.AllowUserToDeleteRows = false;
            dgvDataTableList.AllowUserToResizeRows = false;
            dgvDataTableList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDataTableList.MultiSelect = false;
            dgvDataTableList.RowHeadersVisible = false;

            dgvDataTableList.DefaultCellStyle.BackColor = Color.White;
            dgvDataTableList.DefaultCellStyle.ForeColor = Color.FromArgb(40, 40, 40);
            dgvDataTableList.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 243, 254);
            dgvDataTableList.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 20, 20);
            dgvDataTableList.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            if (dgvDataTableList.Columns.Contains("colImage") && !(dgvDataTableList.Columns["colImage"] is DataGridViewImageColumn))
            {
                int imgIdx = dgvDataTableList.Columns["colImage"].Index;
                dgvDataTableList.Columns.Remove("colImage");
                var imgCol = new DataGridViewImageColumn
                {
                    Name = "colImage",
                    HeaderText = "Image",
                    ImageLayout = DataGridViewImageCellLayout.Zoom,
                    Width = 90
                };
                dgvDataTableList.Columns.Insert(imgIdx, imgCol);
            }

            dgvDataTableList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            if (dgvDataTableList.Columns.Contains("colTableName"))
            {
                dgvDataTableList.Columns["colTableName"].HeaderText = "Table Name";
                dgvDataTableList.Columns["colTableName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvDataTableList.Columns["colTableName"].FillWeight = 40;
                dgvDataTableList.Columns["colTableName"].MinimumWidth = 140;
            }

            if (dgvDataTableList.Columns.Contains("colGroupTableName"))
            {
                dgvDataTableList.Columns["colGroupTableName"].HeaderText = "Group Table Name";
                dgvDataTableList.Columns["colGroupTableName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvDataTableList.Columns["colGroupTableName"].FillWeight = 40;
                dgvDataTableList.Columns["colGroupTableName"].MinimumWidth = 140;
            }

            if (dgvDataTableList.Columns.Contains("colImage"))
            {
                dgvDataTableList.Columns["colImage"].HeaderText = "Image";
                dgvDataTableList.Columns["colImage"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvDataTableList.Columns["colImage"].Width = 90;
                dgvDataTableList.Columns["colImage"].Resizable = DataGridViewTriState.False;
            }

            if (dgvDataTableList.Columns.Contains("colActions"))
            {
                dgvDataTableList.Columns["colActions"].HeaderText = "Actions";
                dgvDataTableList.Columns["colActions"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvDataTableList.Columns["colActions"].Width = 290;
                dgvDataTableList.Columns["colActions"].Resizable = DataGridViewTriState.False;
            }

            dgvDataTableList.CellPainting -= dgvDataTableList_CellPainting;
            dgvDataTableList.CellPainting += dgvDataTableList_CellPainting;

            dgvDataTableList.CellMouseClick -= dgvDataTableList_CellMouseClick;
            dgvDataTableList.CellMouseClick += dgvDataTableList_CellMouseClick;
        }

        private void LoadTableData()
        {
            dgvDataTableList.Rows.Clear();
            string selectedGroup = comboGroupTable?.SelectedItem?.ToString() ?? "All Group Table";
            string search = txtSearch?.Text?.Trim() ?? "";

            try
            {
                string sql = @"
SELECT 
    t.TableID,
    t.TableCode,
    t.TableName,
    g.GroupName,
    t.ImagePath,
    t.IsActive
FROM dbo.DINING_TABLE t
JOIN dbo.TABLE_GROUP g ON t.TableGroupID = g.TableGroupID
WHERE (@Group = 'All Group Table' OR g.GroupName = @Group)
  AND (@Search = '' OR t.TableName LIKE @Pattern OR t.TableCode LIKE @Pattern)
ORDER BY t.TableID;";

                DataTable dt = DbHelper.ExecuteQuery(sql,
                    new SqlParameter("@Group", selectedGroup),
                    new SqlParameter("@Search", search),
                    new SqlParameter("@Pattern", $"%{search}%"));

                foreach (DataRow r in dt.Rows)
                {
                    Image? tableImg = null;
                    string? imgPath = r["ImagePath"]?.ToString();
                    if (!string.IsNullOrEmpty(imgPath) && System.IO.File.Exists(imgPath))
                    {
                        try { tableImg = Image.FromFile(imgPath); } catch { }
                    }

                    if (tableImg == null)
                    {
                        // Draw a clean placeholder bitmap
                        Bitmap bmp = new Bitmap(45, 45);
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.Clear(Color.FromArgb(245, 245, 245));
                            using (Pen p = new Pen(Color.FromArgb(200, 200, 200), 1))
                                g.DrawEllipse(p, 4, 4, 36, 36);
                        }
                        tableImg = bmp;
                    }

                    int rowIndex = dgvDataTableList.Rows.Add(
                        r["TableName"]?.ToString(),
                        r["GroupName"]?.ToString(),
                        tableImg,
                        ""
                    );
                    dgvDataTableList.Rows[rowIndex].Tag = Convert.ToInt32(r["TableID"]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tables: {ex.Message}");
            }
        }

        private void dgvDataTableList_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 3)
            {
                e.PaintBackground(e.CellBounds, true);

                Rectangle btnChoose = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + 12, 85, 30);
                DrawActionButton(e.Graphics, btnChoose, "Choose File", Color.FromArgb(30, 136, 229));

                Rectangle btnEdit = new Rectangle(e.CellBounds.X + 100, e.CellBounds.Y + 12, 55, 30);
                DrawActionButton(e.Graphics, btnEdit, "Edit", Color.FromArgb(46, 125, 50));

                Rectangle btnDelete = new Rectangle(e.CellBounds.X + 162, e.CellBounds.Y + 12, 60, 30);
                DrawActionButton(e.Graphics, btnDelete, "Delete", Color.FromArgb(229, 57, 53));

                Rectangle chkBox = new Rectangle(e.CellBounds.X + 235, e.CellBounds.Y + 18, 16, 16);
                int tid = dgvDataTableList.Rows[e.RowIndex].Tag != null ? Convert.ToInt32(dgvDataTableList.Rows[e.RowIndex].Tag) : 0;
                bool isChecked = _selectedTableIds.Contains(tid);
                ControlPaint.DrawCheckBox(e.Graphics, chkBox, isChecked ? ButtonState.Checked : ButtonState.Flat);

                e.Handled = true;
            }
        }

        private void DrawActionButton(Graphics g, Rectangle bounds, string text, Color bgColor)
        {
            using (SolidBrush brush = new SolidBrush(bgColor))
            using (Font font = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            {
                g.FillRectangle(brush, bounds);
                TextRenderer.DrawText(g, text, font, bounds, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void dgvDataTableList_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 3)
            {
                Point clickPoint = e.Location;
                int tableId = dgvDataTableList.Rows[e.RowIndex].Tag != null ? Convert.ToInt32(dgvDataTableList.Rows[e.RowIndex].Tag) : 0;

                if (clickPoint.X >= 8 && clickPoint.X <= 93)
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            dgvDataTableList.Rows[e.RowIndex].Cells[2].Value = Image.FromFile(ofd.FileName);
                            if (tableId > 0)
                            {
                                DbHelper.ExecuteNonQuery("UPDATE dbo.DINING_TABLE SET ImagePath = @Img WHERE TableID = @ID",
                                    new SqlParameter("@Img", ofd.FileName),
                                    new SqlParameter("@ID", tableId));
                            }
                        }
                    }
                }
                else if (clickPoint.X >= 100 && clickPoint.X <= 155)
                {
                    if (tableId > 0)
                    {
                        OpenEditTable(tableId);
                    }
                }
                else if (clickPoint.X >= 162 && clickPoint.X <= 222)
                {
                    if (MessageBox.Show("Are you sure you want to delete this table?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            DbHelper.ExecuteNonQuery("DELETE FROM dbo.DINING_TABLE WHERE TableID = @ID", new SqlParameter("@ID", tableId));
                            _selectedTableIds.Remove(tableId);
                            LoadTableData();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Could not delete table (it may have active orders): {ex.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else if (clickPoint.X >= 230 && clickPoint.X <= 260)
                {
                    if (_selectedTableIds.Contains(tableId)) _selectedTableIds.Remove(tableId);
                    else _selectedTableIds.Add(tableId);
                    dgvDataTableList.InvalidateCell(e.ColumnIndex, e.RowIndex);
                }
            }
        }

        private void OpenEditTable(int tableId)
        {
            Control? parent = this.Parent;
            if (parent != null)
            {
                parent.Controls.Clear();
                EditTable editTable = new EditTable(tableId) { Dock = DockStyle.Fill };
                parent.Controls.Add(editTable);
                editTable.BringToFront();
            }
        }

        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            Control? parent = this.Parent;
            if (parent != null)
            {
                parent.Controls.Clear();
                CreateTable createTable = new CreateTable { Dock = DockStyle.Fill };
                parent.Controls.Add(createTable);
                createTable.BringToFront();
            }
        }

        private void BtnSetImageAll_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (_selectedTableIds.Count > 0)
                        {
                            foreach (int tid in _selectedTableIds)
                            {
                                DbHelper.ExecuteNonQuery("UPDATE dbo.DINING_TABLE SET ImagePath = @Img WHERE TableID = @ID",
                                    new SqlParameter("@Img", ofd.FileName),
                                    new SqlParameter("@ID", tid));
                            }
                            MessageBox.Show($"Image updated for {_selectedTableIds.Count} selected table(s).", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            string selectedGroup = comboGroupTable?.SelectedItem?.ToString() ?? "All Group Table";
                            if (selectedGroup == "All Group Table")
                            {
                                DbHelper.ExecuteNonQuery("UPDATE dbo.DINING_TABLE SET ImagePath = @Img",
                                    new SqlParameter("@Img", ofd.FileName));
                            }
                            else
                            {
                                DbHelper.ExecuteNonQuery(@"
UPDATE t SET t.ImagePath = @Img
FROM dbo.DINING_TABLE t
JOIN dbo.TABLE_GROUP g ON t.TableGroupID = g.TableGroupID
WHERE g.GroupName = @Group;",
                                    new SqlParameter("@Img", ofd.FileName),
                                    new SqlParameter("@Group", selectedGroup));
                            }
                            MessageBox.Show("Images updated successfully for all matching tables.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        LoadTableData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error setting images: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnDeletebySelect_Click(object? sender, EventArgs e)
        {
            var targetIds = new System.Collections.Generic.List<int>(_selectedTableIds);
            if (targetIds.Count == 0)
            {
                foreach (DataGridViewRow row in dgvDataTableList.SelectedRows)
                {
                    int tid = row.Tag != null ? Convert.ToInt32(row.Tag) : 0;
                    if (tid > 0) targetIds.Add(tid);
                }
            }

            if (targetIds.Count == 0)
            {
                MessageBox.Show("Please select or check one or more tables to delete.", "Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to delete {targetIds.Count} selected table(s)?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int deleted = 0;
                foreach (int tableId in targetIds)
                {
                    try
                    {
                        DbHelper.ExecuteNonQuery("DELETE FROM dbo.DINING_TABLE WHERE TableID = @ID", new SqlParameter("@ID", tableId));
                        _selectedTableIds.Remove(tableId);
                        deleted++;
                    }
                    catch { }
                }
                MessageBox.Show($"Successfully deleted {deleted} table(s).", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTableData();
            }
        }

        private void BtnCreateTableList_Click(object? sender, EventArgs e)
        {
            using (CreateTableList createListForm = new CreateTableList())
            {
                if (createListForm.ShowDialog() == DialogResult.OK)
                {
                    LoadTableData();
                }
            }
        }
    }
}