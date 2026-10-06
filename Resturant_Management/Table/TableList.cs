using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Table
{
    public partial class TableList : UserControl
    {
        private readonly HashSet<int> _selectedTableIds = new();
        private bool _isInitialized = false;

        // Dynamic Responsive Controls
        private readonly Guna.UI2.WinForms.Guna2Button btnMoveGroup = new Guna.UI2.WinForms.Guna2Button();
        private readonly Panel pnlSearch = new Panel();
        private readonly Label lblSearch = new Label();
        private Panel pnlTitle = null!;
        private Panel pnlToolbar = null!;
        private Panel pnlGrid = null!;

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
            btnMoveGroup.Click += BtnMoveGroup_Click;
            btnDeletebySelect.Click += BtnDeletebySelect_Click;
            txtSearch.TextChanged += (s, e) => LoadTableData();
        }

        private void BuildResponsiveLayout()
        {
            this.SuspendLayout();
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // 1. Top Title Bar (Dock = Top, Height = 58) - Flat, NO shadow
            pnlTitle = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            label1.Text = "Table list";
            label1.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(26, 117, 210);
            label1.AutoSize = true;
            label1.Location = new Point(18, 12);

            btnCreate.Text = "Create";
            btnCreate.Size = new Size(120, 38);
            btnCreate.BorderRadius = 4;
            btnCreate.FillColor = Color.FromArgb(26, 117, 210);
            btnCreate.ForeColor = Color.White;
            btnCreate.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnCreate.TextAlign = HorizontalAlignment.Center;
            btnCreate.TextOffset = new Point(0, 0);
            btnCreate.Cursor = Cursors.Hand;
            btnCreate.ShadowDecoration.Enabled = false;

            pnlTitle.Controls.Add(label1);
            pnlTitle.Controls.Add(btnCreate);

            void PositionTitle()
            {
                btnCreate.Location = new Point(pnlTitle.Width - btnCreate.Width - 20, 10);
            }
            pnlTitle.Resize += (s, ev) => PositionTitle();
            PositionTitle();

            // 2. Action / Filter Toolbar (Dock = Top) - Fully Responsive
            pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 105,
                BackColor = Color.White
            };

            // Configure generous dimensions to prevent any text truncation across all DPIs
            btnCreateTableList.Text = "Create Table List";
            btnCreateTableList.Size = new Size(160, 38);
            btnCreateTableList.BorderRadius = 4;
            btnCreateTableList.FillColor = Color.FromArgb(26, 117, 210);
            btnCreateTableList.ForeColor = Color.White;
            btnCreateTableList.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnCreateTableList.Cursor = Cursors.Hand;
            btnCreateTableList.ShadowDecoration.Enabled = false;

            btnSetImageAll.Text = "Set Image for selected items";
            btnSetImageAll.Size = new Size(235, 38);
            btnSetImageAll.BorderRadius = 4;
            btnSetImageAll.FillColor = Color.FromArgb(26, 117, 210);
            btnSetImageAll.ForeColor = Color.White;
            btnSetImageAll.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnSetImageAll.Cursor = Cursors.Hand;
            btnSetImageAll.ShadowDecoration.Enabled = false;

            // Generous width for combo so "All Group Table" NEVER truncates to "All Gr... Table"
            comboGroupTable.Size = new Size(210, 38);
            comboGroupTable.DropDownWidth = 230;
            comboGroupTable.BorderRadius = 4;
            comboGroupTable.BorderColor = Color.FromArgb(209, 213, 219);
            comboGroupTable.FillColor = Color.White;
            comboGroupTable.ForeColor = Color.FromArgb(30, 41, 59);
            comboGroupTable.Font = new Font("Segoe UI", 9.5F);
            comboGroupTable.ItemHeight = 30;
            comboGroupTable.ShadowDecoration.Enabled = false;

            btnMoveGroup.Text = "Move Group Table By Selected";
            btnMoveGroup.Size = new Size(245, 38);
            btnMoveGroup.BorderRadius = 4;
            btnMoveGroup.FillColor = Color.FromArgb(26, 117, 210);
            btnMoveGroup.ForeColor = Color.White;
            btnMoveGroup.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnMoveGroup.Cursor = Cursors.Hand;
            btnMoveGroup.ShadowDecoration.Enabled = false;

            btnDeletebySelect.Text = "Deleted By Selected";
            btnDeletebySelect.Size = new Size(170, 38);
            btnDeletebySelect.BorderRadius = 4;
            btnDeletebySelect.FillColor = Color.FromArgb(26, 117, 210);
            btnDeletebySelect.HoverState.FillColor = Color.FromArgb(21, 101, 192);
            btnDeletebySelect.ForeColor = Color.White;
            btnDeletebySelect.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnDeletebySelect.Cursor = Cursors.Hand;
            btnDeletebySelect.ShadowDecoration.Enabled = false;

            // Search Container
            pnlSearch.Size = new Size(270, 52);
            pnlSearch.BackColor = Color.Transparent;

            lblSearch.Text = "Search";
            lblSearch.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            lblSearch.ForeColor = Color.FromArgb(100, 116, 139);
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(2, 0);

            txtSearch.Size = new Size(270, 32);
            txtSearch.Location = new Point(0, 17);
            txtSearch.BorderRadius = 4;
            txtSearch.BorderColor = Color.FromArgb(209, 213, 219);
            txtSearch.PlaceholderText = "";
            txtSearch.Font = new Font("Segoe UI", 9.5F);
            txtSearch.ForeColor = Color.FromArgb(30, 41, 59);
            txtSearch.ShadowDecoration.Enabled = false;

            // Search Icon
            Bitmap searchIcon = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(searchIcon))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen p = new Pen(Color.FromArgb(100, 116, 139), 1.6f))
                {
                    g.DrawEllipse(p, 2, 2, 7, 7);
                    g.DrawLine(p, 7, 7, 12, 12);
                }
            }
            txtSearch.IconLeft = searchIcon;
            txtSearch.IconLeftSize = new Size(14, 14);
            txtSearch.IconLeftOffset = new Point(6, 0);

            pnlSearch.Controls.Clear();
            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(txtSearch);

            pnlToolbar.Controls.Add(btnCreateTableList);
            pnlToolbar.Controls.Add(btnSetImageAll);
            pnlToolbar.Controls.Add(comboGroupTable);
            pnlToolbar.Controls.Add(btnMoveGroup);
            pnlToolbar.Controls.Add(btnDeletebySelect);
            pnlToolbar.Controls.Add(pnlSearch);

            void RepositionToolbar()
            {
                int leftGroupWidth = btnCreateTableList.Width + btnSetImageAll.Width + comboGroupTable.Width + 24;
                int rightGroupWidth = btnMoveGroup.Width + btnDeletebySelect.Width + 10;
                int totalNeeded = leftGroupWidth + rightGroupWidth + 40;
                int margin = 20;

                if (pnlToolbar.Width >= totalNeeded)
                {
                    // Wide Desktop: single row for buttons, search box under right group
                    pnlToolbar.Height = 105;

                    btnCreateTableList.Location = new Point(margin, 10);
                    btnSetImageAll.Location = new Point(btnCreateTableList.Right + 10, 10);
                    comboGroupTable.Location = new Point(btnSetImageAll.Right + 10, 10);

                    btnDeletebySelect.Location = new Point(pnlToolbar.Width - btnDeletebySelect.Width - margin, 10);
                    btnMoveGroup.Location = new Point(btnDeletebySelect.Left - btnMoveGroup.Width - 10, 10);

                    pnlSearch.Location = new Point(pnlToolbar.Width - pnlSearch.Width - margin, 52);
                }
                else
                {
                    // Responsive / Compact View: Row 1 = Left controls, Row 2 = Right controls & search
                    pnlToolbar.Height = 112;

                    btnCreateTableList.Location = new Point(margin, 10);
                    btnSetImageAll.Location = new Point(btnCreateTableList.Right + 8, 10);
                    comboGroupTable.Location = new Point(btnSetImageAll.Right + 8, 10);

                    btnMoveGroup.Location = new Point(margin, 56);
                    btnDeletebySelect.Location = new Point(btnMoveGroup.Right + 8, 56);

                    int searchX = Math.Max(btnDeletebySelect.Right + 12, pnlToolbar.Width - pnlSearch.Width - margin);
                    pnlSearch.Location = new Point(searchX, 42);
                }
            }
            pnlToolbar.Resize += (s, ev) => RepositionToolbar();
            RepositionToolbar();

            // 3. DataGridView Container (Dock = Fill) - Expands to full remaining window
            pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20, 4, 20, 20)
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
            dgvDataTableList.ReadOnly = true;

            // REMOVE BACK BORDERS & VERTICAL BORDERS - ONLY SINGLE BOTTOM BORDER
            dgvDataTableList.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgvDataTableList.GridColor = Color.FromArgb(241, 245, 249);
            dgvDataTableList.AdvancedCellBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;
            dgvDataTableList.AdvancedColumnHeadersBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;
            dgvDataTableList.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.None;
            dgvDataTableList.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;

            // Header Style: Flat, solid white, NO shadow, comfortable 48px height
            dgvDataTableList.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            dgvDataTableList.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            dgvDataTableList.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvDataTableList.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            dgvDataTableList.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            dgvDataTableList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvDataTableList.ColumnHeadersHeight = 48;
            dgvDataTableList.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            dgvDataTableList.RowTemplate.Height = 58;
            dgvDataTableList.AllowUserToAddRows = false;
            dgvDataTableList.AllowUserToDeleteRows = false;
            dgvDataTableList.AllowUserToResizeRows = false;
            dgvDataTableList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDataTableList.MultiSelect = false;
            dgvDataTableList.RowHeadersVisible = false;

            dgvDataTableList.DefaultCellStyle.BackColor = Color.White;
            dgvDataTableList.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            dgvDataTableList.DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 247, 255);
            dgvDataTableList.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            dgvDataTableList.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            if (dgvDataTableList.Columns.Contains("colImage") && !(dgvDataTableList.Columns["colImage"] is DataGridViewImageColumn))
            {
                int imgIdx = dgvDataTableList.Columns["colImage"].Index;
                dgvDataTableList.Columns.Remove("colImage");
                var imgCol = new DataGridViewImageColumn
                {
                    Name = "colImage",
                    HeaderText = "Image Name",
                    ImageLayout = DataGridViewImageCellLayout.Zoom,
                    Width = 140
                };
                dgvDataTableList.Columns.Insert(imgIdx, imgCol);
            }

            dgvDataTableList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            if (dgvDataTableList.Columns.Contains("colTableName"))
            {
                dgvDataTableList.Columns["colTableName"].HeaderText = "Table Name";
                dgvDataTableList.Columns["colTableName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvDataTableList.Columns["colTableName"].FillWeight = 26;
                dgvDataTableList.Columns["colTableName"].MinimumWidth = 140;
                dgvDataTableList.Columns["colTableName"].DefaultCellStyle.Padding = new Padding(16, 0, 0, 0);
            }

            if (dgvDataTableList.Columns.Contains("colGroupTableName"))
            {
                dgvDataTableList.Columns["colGroupTableName"].HeaderText = "Group Table Name";
                dgvDataTableList.Columns["colGroupTableName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvDataTableList.Columns["colGroupTableName"].FillWeight = 26;
                dgvDataTableList.Columns["colGroupTableName"].MinimumWidth = 150;
            }

            if (dgvDataTableList.Columns.Contains("colImage"))
            {
                dgvDataTableList.Columns["colImage"].HeaderText = "Image Name";
                dgvDataTableList.Columns["colImage"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvDataTableList.Columns["colImage"].Width = 140;
                dgvDataTableList.Columns["colImage"].MinimumWidth = 130;
                dgvDataTableList.Columns["colImage"].Resizable = DataGridViewTriState.False;
                dgvDataTableList.Columns["colImage"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            if (dgvDataTableList.Columns.Contains("colActions"))
            {
                dgvDataTableList.Columns["colActions"].HeaderText = "Select All";
                dgvDataTableList.Columns["colActions"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvDataTableList.Columns["colActions"].Width = 380;
                dgvDataTableList.Columns["colActions"].MinimumWidth = 360;
                dgvDataTableList.Columns["colActions"].Resizable = DataGridViewTriState.False;
                dgvDataTableList.Columns["colActions"].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            if (dgvDataTableList.Columns.Contains("colImage"))
            {
                dgvDataTableList.Columns["colImage"].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dgvDataTableList.CellPainting -= dgvDataTableList_CellPainting;
            dgvDataTableList.CellPainting += dgvDataTableList_CellPainting;

            dgvDataTableList.CellMouseClick -= dgvDataTableList_CellMouseClick;
            dgvDataTableList.CellMouseClick += dgvDataTableList_CellMouseClick;

            dgvDataTableList.ColumnHeaderMouseClick -= dgvDataTableList_ColumnHeaderMouseClick;
            dgvDataTableList.ColumnHeaderMouseClick += dgvDataTableList_ColumnHeaderMouseClick;

            dgvDataTableList.MouseUp -= DgvDataTableList_MouseUp;
            dgvDataTableList.MouseUp += DgvDataTableList_MouseUp;

            dgvDataTableList.MouseMove -= DgvDataTableList_MouseMove;
            dgvDataTableList.MouseMove += DgvDataTableList_MouseMove;
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
WHERE (@Group = 'All Group Table' OR @Group = 'All Group' OR g.GroupName = @Group)
  AND (@Search = '' OR t.TableName LIKE @Pattern OR t.TableCode LIKE @Pattern)
ORDER BY t.TableID;";

                DataTable dt = DbHelper.ExecuteQuery(sql,
                    new SqlParameter("@Group", selectedGroup),
                    new SqlParameter("@Search", search),
                    new SqlParameter("@Pattern", $"%{search}%"));

                foreach (DataRow r in dt.Rows)
                {
                    string? imgPath = r["ImagePath"]?.ToString();
                    // Smaller 30px thumbnail with generous padding inside column
                    Image tableImg = CreateRoundedThumbnail(imgPath, 30);

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

        private Image CreateRoundedThumbnail(string? filePath, int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);

                Rectangle rect = new Rectangle(1, 1, size - 2, size - 2);
                using (GraphicsPath path = GetRoundedPath(rect, 3))
                {
                    bool imageLoaded = false;
                    if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            using (Image src = Image.FromFile(filePath))
                            {
                                g.SetClip(path);
                                g.DrawImage(src, rect);
                                g.ResetClip();
                                imageLoaded = true;
                            }
                        }
                        catch
                        {
                            imageLoaded = false;
                        }
                    }

                    if (!imageLoaded)
                    {
                        using (SolidBrush bgBrush = new SolidBrush(Color.White))
                        {
                            g.FillPath(bgBrush, path);
                        }

                        // Picture placeholder card
                        Rectangle iconRect = new Rectangle(5, 5, size - 10, size - 10);
                        using (GraphicsPath iconPath = GetRoundedPath(iconRect, 2))
                        {
                            using (SolidBrush skyBrush = new SolidBrush(Color.FromArgb(224, 242, 254)))
                            {
                                g.FillPath(skyBrush, iconPath);
                            }

                            g.SetClip(iconPath);
                            // Sun
                            using (SolidBrush sunBrush = new SolidBrush(Color.FromArgb(250, 204, 21)))
                            {
                                g.FillEllipse(sunBrush, iconRect.Right - 7, iconRect.Y + 2, 4, 4);
                            }
                            // Hills
                            using (SolidBrush mtn1 = new SolidBrush(Color.FromArgb(74, 222, 128)))
                            {
                                Point[] pts1 = new Point[]
                                {
                                    new Point(iconRect.Left - 2, iconRect.Bottom),
                                    new Point(iconRect.Left + 5, iconRect.Y + 6),
                                    new Point(iconRect.Left + 11, iconRect.Bottom)
                                };
                                g.FillPolygon(mtn1, pts1);
                            }
                            using (SolidBrush mtn2 = new SolidBrush(Color.FromArgb(34, 197, 94)))
                            {
                                Point[] pts2 = new Point[]
                                {
                                    new Point(iconRect.Left + 4, iconRect.Bottom),
                                    new Point(iconRect.Left + 10, iconRect.Y + 4),
                                    new Point(iconRect.Right + 2, iconRect.Bottom)
                                };
                                g.FillPolygon(mtn2, pts2);
                            }
                            g.ResetClip();

                            using (Pen iconBorder = new Pen(Color.FromArgb(148, 163, 184), 1f))
                            {
                                g.DrawPath(iconBorder, iconPath);
                            }
                        }
                    }

                    using (Pen borderPen = new Pen(Color.FromArgb(209, 213, 219), 1f))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }
            }
            return bmp;
        }

        private void dgvDataTableList_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.Graphics == null) return;

            int actionColIdx = dgvDataTableList.Columns.Contains("colActions") ? dgvDataTableList.Columns["colActions"].Index : 3;
            int imageColIdx = dgvDataTableList.Columns.Contains("colImage") ? dgvDataTableList.Columns["colImage"].Index : 2;

            // 1. Paint Header Cells - Flat white, NO vertical borders, ONLY single bottom line
            if (e.RowIndex == -1)
            {
                using (SolidBrush bgBrush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillRectangle(bgBrush, e.CellBounds);
                }

                // Single bottom line under header
                using (Pen gridPen = new Pen(Color.FromArgb(235, 238, 242), 1f))
                {
                    e.Graphics.DrawLine(gridPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                }

                if (e.ColumnIndex == actionColIdx)
                {
                    bool isAllSelected = dgvDataTableList.Rows.Count > 0;
                    foreach (DataGridViewRow row in dgvDataTableList.Rows)
                    {
                        if (row.Tag != null)
                        {
                            int tid = Convert.ToInt32(row.Tag);
                            if (!_selectedTableIds.Contains(tid))
                            {
                                isAllSelected = false;
                                break;
                            }
                        }
                    }

                    int startX = e.CellBounds.X + 16;
                    using (Font font = new Font("Segoe UI", 10F, FontStyle.Bold))
                    using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
                    {
                        e.Graphics.DrawString("Select All", font, textBrush, startX + 4, e.CellBounds.Y + (e.CellBounds.Height - 20) / 2);
                    }

                    // Checkbox in Header at X = startX + 296 (lines up with row checkbox)
                    int chkY = e.CellBounds.Y + (e.CellBounds.Height - 18) / 2;
                    Rectangle chkBox = new Rectangle(startX + 296, chkY, 18, 18);
                    using (GraphicsPath chkPath = GetRoundedPath(chkBox, 2))
                    using (SolidBrush chkBg = new SolidBrush(isAllSelected ? Color.FromArgb(26, 117, 210) : Color.White))
                    using (Pen chkBorder = new Pen(isAllSelected ? Color.FromArgb(26, 117, 210) : Color.FromArgb(148, 163, 184), 1.5f))
                    {
                        e.Graphics.FillPath(chkBg, chkPath);
                        e.Graphics.DrawPath(chkBorder, chkPath);
                        if (isAllSelected)
                        {
                            using (Pen checkPen = new Pen(Color.White, 2f))
                            {
                                e.Graphics.DrawLines(checkPen, new Point[]
                                {
                                    new Point(chkBox.X + 3, chkBox.Y + 9),
                                    new Point(chkBox.X + 7, chkBox.Y + 13),
                                    new Point(chkBox.X + 14, chkBox.Y + 4)
                                });
                            }
                        }
                    }

                    e.Handled = true;
                    return;
                }
                else
                {
                    // Paint regular column header text cleanly without any vertical borders
                    e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);
                    e.Handled = true;
                    return;
                }
            }

            // 2. Paint Data Row Cells - ONLY single bottom border, NO vertical borders, NO back borders
            if (e.RowIndex >= 0)
            {
                bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;
                Color bgColor = isSelected ? Color.FromArgb(240, 247, 255) : Color.White;
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    e.Graphics.FillRectangle(bgBrush, e.CellBounds);
                }

                // ONLY ONE SINGLE BOTTOM BORDER FOR ROWS
                using (Pen gridPen = new Pen(Color.FromArgb(241, 245, 249), 1f))
                {
                    e.Graphics.DrawLine(gridPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                }

                // Action Column
                if (e.ColumnIndex == actionColIdx)
                {
                    int cy = e.CellBounds.Y + (e.CellBounds.Height - 30) / 2;
                    int startX = e.CellBounds.X + 16;
                    Color btnBlue = Color.FromArgb(26, 117, 210);

                    // 1. Choose File button with generous padding: 108x30
                    Rectangle btnChoose = new Rectangle(startX, cy, 108, 30);
                    DrawRoundedActionButton(e.Graphics, btnChoose, "Choose File", btnBlue, 4);

                    // Separator 1 (✦)
                    DrawSeparatorDiamond(e.Graphics, startX + 108 + 12, cy + 15);

                    // 2. Edit button (52x30)
                    Rectangle btnEdit = new Rectangle(startX + 132, cy, 52, 30);
                    DrawRoundedActionButton(e.Graphics, btnEdit, "Edit", btnBlue, 4);

                    // Separator 2 (✦)
                    DrawSeparatorDiamond(e.Graphics, startX + 132 + 52 + 12, cy + 15);

                    // 3. Delete button (66x30)
                    Rectangle btnDelete = new Rectangle(startX + 208, cy, 66, 30);
                    DrawRoundedActionButton(e.Graphics, btnDelete, "Delete", btnBlue, 4);

                    // Separator 3 (✦)
                    DrawSeparatorDiamond(e.Graphics, startX + 208 + 66 + 12, cy + 15);

                    // 4. Checkbox (18x18) at startX + 296
                    int chkY = e.CellBounds.Y + (e.CellBounds.Height - 18) / 2;
                    Rectangle chkBox = new Rectangle(startX + 296, chkY, 18, 18);
                    int tid = dgvDataTableList.Rows[e.RowIndex].Tag != null ? Convert.ToInt32(dgvDataTableList.Rows[e.RowIndex].Tag) : 0;
                    bool isChecked = _selectedTableIds.Contains(tid);

                    using (GraphicsPath chkPath = GetRoundedPath(chkBox, 2))
                    using (SolidBrush chkBg = new SolidBrush(isChecked ? btnBlue : Color.White))
                    using (Pen chkBorder = new Pen(isChecked ? btnBlue : Color.FromArgb(148, 163, 184), 1.5f))
                    {
                        e.Graphics.FillPath(chkBg, chkPath);
                        e.Graphics.DrawPath(chkBorder, chkPath);
                        if (isChecked)
                        {
                            using (Pen checkPen = new Pen(Color.White, 2f))
                            {
                                e.Graphics.DrawLines(checkPen, new Point[]
                                {
                                    new Point(chkBox.X + 3, chkBox.Y + 9),
                                    new Point(chkBox.X + 7, chkBox.Y + 13),
                                    new Point(chkBox.X + 14, chkBox.Y + 4)
                                });
                            }
                        }
                    }

                    e.Handled = true;
                    return;
                }

                // Image Column: Centered smaller thumbnail with padding inside the column
                if (e.ColumnIndex == imageColIdx)
                {
                    if (e.Value is Image img)
                    {
                        int imgSize = 30;
                        int imgX = e.CellBounds.X + (e.CellBounds.Width - imgSize) / 2;
                        int imgY = e.CellBounds.Y + (e.CellBounds.Height - imgSize) / 2;
                        e.Graphics.DrawImage(img, new Rectangle(imgX, imgY, imgSize, imgSize));
                    }
                    e.Handled = true;
                    return;
                }

                // Other Columns (Table Name, Group Table Name)
                e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);
                e.Handled = true;
            }
        }

        private void DrawSeparatorDiamond(Graphics g, int cx, int cy)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(30, 41, 59)))
            {
                Point[] pts = new Point[]
                {
                    new Point(cx, cy - 3),
                    new Point(cx + 3, cy),
                    new Point(cx, cy + 3),
                    new Point(cx - 3, cy)
                };
                g.FillPolygon(brush, pts);
            }
        }

        private void DrawRoundedActionButton(Graphics g, Rectangle bounds, string text, Color bgColor, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = GetRoundedPath(bounds, radius))
            using (SolidBrush brush = new SolidBrush(bgColor))
            using (Font font = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                g.FillPath(brush, path);
                TextRenderer.DrawText(g, text, font, bounds, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void dgvDataTableList_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            int actionColIdx = dgvDataTableList.Columns.Contains("colActions") ? dgvDataTableList.Columns["colActions"].Index : 3;

            // Clicking in Header row (Select All / Header Checkbox)
            if (e.RowIndex == -1 && e.ColumnIndex == actionColIdx)
            {
                ToggleSelectAll();
                return;
            }

            // Clicking in Data row
            if (e.RowIndex >= 0 && e.ColumnIndex == actionColIdx)
            {
                int startX = 16;
                int clickX = e.X;
                int tableId = dgvDataTableList.Rows[e.RowIndex].Tag != null ? Convert.ToInt32(dgvDataTableList.Rows[e.RowIndex].Tag) : 0;

                // 1. Choose File [startX..startX+108] => [16..124]
                if (clickX >= startX - 2 && clickX <= startX + 112)
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp;*.bmp";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            dgvDataTableList.Rows[e.RowIndex].Cells["colImage"].Value = CreateRoundedThumbnail(ofd.FileName, 30);
                            if (tableId > 0)
                            {
                                DbHelper.ExecuteNonQuery("UPDATE dbo.DINING_TABLE SET ImagePath = @Img WHERE TableID = @ID",
                                    new SqlParameter("@Img", ofd.FileName),
                                    new SqlParameter("@ID", tableId));
                            }
                        }
                    }
                }
                // 2. Edit [startX+132..startX+132+52] => [148..200]
                else if (clickX >= startX + 128 && clickX <= startX + 188)
                {
                    if (tableId > 0)
                    {
                        OpenEditTable(tableId);
                    }
                }
                // 3. Delete [startX+208..startX+208+66] => [224..290]
                else if (clickX >= startX + 204 && clickX <= startX + 276)
                {
                    if (MessageBox.Show("Are you sure you want to delete this table?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
                // 4. Checkbox [startX+296..startX+296+18] => [312..330]
                else if (clickX >= startX + 290 && clickX <= startX + 345)
                {
                    if (_selectedTableIds.Contains(tableId)) _selectedTableIds.Remove(tableId);
                    else _selectedTableIds.Add(tableId);

                    dgvDataTableList.Refresh();
                }
            }
        }

        private void DgvDataTableList_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var hit = dgvDataTableList.HitTest(e.X, e.Y);
            int actionColIdx = dgvDataTableList.Columns.Contains("colActions") ? dgvDataTableList.Columns["colActions"].Index : 3;

            // Header of Action Column (Select All / Header Checkbox)
            if (hit.Type == DataGridViewHitTestType.ColumnHeader && hit.ColumnIndex == actionColIdx)
            {
                ToggleSelectAll();
                return;
            }

            // Data Row Checkbox via MouseUp
            if (hit.Type == DataGridViewHitTestType.Cell && hit.ColumnIndex == actionColIdx && hit.RowIndex >= 0)
            {
                Rectangle cellRect = dgvDataTableList.GetCellDisplayRectangle(hit.ColumnIndex, hit.RowIndex, false);
                int clickX = e.X - cellRect.X;
                if (clickX >= 290 && clickX <= 350)
                {
                    int tableId = dgvDataTableList.Rows[hit.RowIndex].Tag != null ? Convert.ToInt32(dgvDataTableList.Rows[hit.RowIndex].Tag) : 0;
                    if (tableId > 0)
                    {
                        if (_selectedTableIds.Contains(tableId)) _selectedTableIds.Remove(tableId);
                        else _selectedTableIds.Add(tableId);

                        dgvDataTableList.Refresh();
                    }
                }
            }
        }

        private void DgvDataTableList_MouseMove(object? sender, MouseEventArgs e)
        {
            var hit = dgvDataTableList.HitTest(e.X, e.Y);
            int actionColIdx = dgvDataTableList.Columns.Contains("colActions") ? dgvDataTableList.Columns["colActions"].Index : 3;

            if (hit.Type == DataGridViewHitTestType.ColumnHeader && hit.ColumnIndex == actionColIdx)
            {
                dgvDataTableList.Cursor = Cursors.Hand;
            }
            else if (hit.Type == DataGridViewHitTestType.Cell && hit.ColumnIndex == actionColIdx && hit.RowIndex >= 0)
            {
                Rectangle cellRect = dgvDataTableList.GetCellDisplayRectangle(hit.ColumnIndex, hit.RowIndex, false);
                int clickX = e.X - cellRect.X;
                if ((clickX >= 14 && clickX <= 126) ||  // Choose File
                    (clickX >= 144 && clickX <= 204) || // Edit
                    (clickX >= 220 && clickX <= 292) || // Delete
                    (clickX >= 296 && clickX <= 345))   // Checkbox
                {
                    dgvDataTableList.Cursor = Cursors.Hand;
                }
                else
                {
                    dgvDataTableList.Cursor = Cursors.Default;
                }
            }
            else
            {
                dgvDataTableList.Cursor = Cursors.Default;
            }
        }

        private void dgvDataTableList_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            int actionColIdx = dgvDataTableList.Columns.Contains("colActions") ? dgvDataTableList.Columns["colActions"].Index : 3;
            if (e.ColumnIndex == actionColIdx)
            {
                ToggleSelectAll();
            }
        }

        private void ToggleSelectAll()
        {
            if (dgvDataTableList.Rows.Count == 0) return;

            bool allSelected = true;
            foreach (DataGridViewRow row in dgvDataTableList.Rows)
            {
                if (row.Tag != null)
                {
                    int tid = Convert.ToInt32(row.Tag);
                    if (!_selectedTableIds.Contains(tid))
                    {
                        allSelected = false;
                        break;
                    }
                }
            }

            if (allSelected)
            {
                _selectedTableIds.Clear();
            }
            else
            {
                foreach (DataGridViewRow row in dgvDataTableList.Rows)
                {
                    if (row.Tag != null)
                    {
                        _selectedTableIds.Add(Convert.ToInt32(row.Tag));
                    }
                }
            }

            dgvDataTableList.Refresh();
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

        private void BtnMoveGroup_Click(object? sender, EventArgs e)
        {
            var targetIds = new List<int>(_selectedTableIds);
            if (targetIds.Count == 0)
            {
                foreach (DataGridViewRow row in dgvDataTableList.SelectedRows)
                {
                    int tid = row.Tag != null ? Convert.ToInt32(row.Tag) : 0;
                    if (tid > 0 && !targetIds.Contains(tid)) targetIds.Add(tid);
                }
            }

            if (targetIds.Count == 0)
            {
                MessageBox.Show("Please select or check one or more tables to move.", "Move Group", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataTable dtGroups;
            try
            {
                dtGroups = DbHelper.ExecuteQuery("SELECT TableGroupID, GroupName FROM dbo.TABLE_GROUP ORDER BY GroupName");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading groups: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dtGroups.Rows.Count == 0)
            {
                MessageBox.Show("No table groups found in database.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (Form prompt = new Form())
            {
                prompt.Width = 380;
                prompt.Height = 210;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "Move Tables to Group";
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;
                prompt.BackColor = Color.White;

                Label lbl = new Label
                {
                    Left = 24,
                    Top = 20,
                    Text = $"Move {targetIds.Count} selected table(s) to:",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    AutoSize = true
                };

                ComboBox cbo = new ComboBox
                {
                    Left = 24,
                    Top = 54,
                    Width = 316,
                    Height = 32,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 10F),
                    DataSource = dtGroups,
                    DisplayMember = "GroupName",
                    ValueMember = "TableGroupID"
                };

                Button btnOk = new Button
                {
                    Text = "Move",
                    Left = 140,
                    Top = 110,
                    Width = 95,
                    Height = 34,
                    BackColor = Color.FromArgb(26, 117, 210),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.OK
                };
                btnOk.FlatAppearance.BorderSize = 0;

                Button btnCancel = new Button
                {
                    Text = "Cancel",
                    Left = 245,
                    Top = 110,
                    Width = 95,
                    Height = 34,
                    BackColor = Color.FromArgb(241, 245, 249),
                    ForeColor = Color.FromArgb(71, 85, 105),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.Cancel
                };
                btnCancel.FlatAppearance.BorderSize = 0;

                prompt.Controls.Add(lbl);
                prompt.Controls.Add(cbo);
                prompt.Controls.Add(btnOk);
                prompt.Controls.Add(btnCancel);
                prompt.AcceptButton = btnOk;
                prompt.CancelButton = btnCancel;

                if (prompt.ShowDialog(this) == DialogResult.OK && cbo.SelectedValue != null)
                {
                    int targetGroupId = Convert.ToInt32(cbo.SelectedValue);
                    string groupName = cbo.Text;
                    try
                    {
                        string idList = string.Join(",", targetIds);
                        DbHelper.ExecuteNonQuery($"UPDATE dbo.DINING_TABLE SET TableGroupID = @GroupID WHERE TableID IN ({idList})",
                            new SqlParameter("@GroupID", targetGroupId));

                        MessageBox.Show($"Successfully moved {targetIds.Count} table(s) to group '{groupName}'.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadTableData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error moving tables: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnSetImageAll_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp;*.bmp";
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
                            if (selectedGroup == "All Group Table" || selectedGroup == "All Group")
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
            var targetIds = new List<int>(_selectedTableIds);
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
                if (createListForm.ShowDialog(this) == DialogResult.OK)
                {
                    LoadTableData();
                }
            }
        }
    }
}