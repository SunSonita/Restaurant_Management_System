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

        public TableList()
        {
            InitializeComponent();
            SetupComboBox();
            SetupDataGridView();
            LoadTableData();
            this.Load += CreateTable_Load;
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadTableData(); };
            btnCreate.Click += BtnCreate_Click;
            btnCreateTableList.Click += BtnCreateTableList_Click;
            btnSetImageAll.Click += BtnSetImageAll_Click;
            btnDeletebySelect.Click += BtnDeletebySelect_Click;
            txtSearch.TextChanged += (s, e) => LoadTableData();
        }

        private void dgvDataTableList_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
        }

        private void CreateTable_Load(object? sender, EventArgs e)
        {
            SetupComboBox();
            SetupDataGridView();
            LoadTableData();
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

            dgvDataTableList.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            dgvDataTableList.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(33, 33, 33);
            dgvDataTableList.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvDataTableList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvDataTableList.ColumnHeadersHeight = 45;
            dgvDataTableList.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            dgvDataTableList.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvDataTableList.GridColor = Color.FromArgb(235, 235, 235);
            dgvDataTableList.RowTemplate.Height = 55;
            dgvDataTableList.AllowUserToAddRows = false;
            dgvDataTableList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

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

                Rectangle btnChoose = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y + 12, 85, 30);
                DrawActionButton(e.Graphics, btnChoose, "Choose File");

                Rectangle btnEdit = new Rectangle(e.CellBounds.X + 105, e.CellBounds.Y + 12, 55, 30);
                DrawActionButton(e.Graphics, btnEdit, "Edit");

                Rectangle btnDelete = new Rectangle(e.CellBounds.X + 170, e.CellBounds.Y + 12, 60, 30);
                DrawActionButton(e.Graphics, btnDelete, "Delete");

                Rectangle chkBox = new Rectangle(e.CellBounds.X + 245, e.CellBounds.Y + 18, 16, 16);
                int tid = dgvDataTableList.Rows[e.RowIndex].Tag != null ? Convert.ToInt32(dgvDataTableList.Rows[e.RowIndex].Tag) : 0;
                bool isChecked = _selectedTableIds.Contains(tid);
                ControlPaint.DrawCheckBox(e.Graphics, chkBox, isChecked ? ButtonState.Checked : ButtonState.Flat);

                e.Handled = true;
            }
        }

        private void DrawActionButton(Graphics g, Rectangle bounds, string text)
        {
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(25, 118, 210))) 
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

                if (clickPoint.X >= 10 && clickPoint.X <= 95)
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
                else if (clickPoint.X >= 105 && clickPoint.X <= 160)
                {
                    if (tableId > 0)
                    {
                        OpenEditTable(tableId);
                    }
                }
                else if (clickPoint.X >= 170 && clickPoint.X <= 230)
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
                else if (clickPoint.X >= 240 && clickPoint.X <= 270)
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