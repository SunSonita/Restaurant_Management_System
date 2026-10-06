using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Table
{
    public partial class TableList : UserControl
    {
        private readonly HashSet<int> _selectedTableIds = new();
        private bool _isInitialized = false;

        // Dynamic responsive controls
        private readonly Guna.UI2.WinForms.Guna2Button btnMoveGroup = new Guna.UI2.WinForms.Guna2Button();
        private Panel pnlTitle = null!;
        private Panel pnlToolbar = null!;
        private Panel pnlGrid = null!;

        // DPI-aware metrics for the "actions" column (measured from real text, never hard-coded)
        private Font _actionFont = null!;
        private Font _headerFont = null!;
        private int _pad, _gap, _chk, _thumb;
        private int _wChoose, _wEdit, _wDelete;

        private static readonly Color Blue = Color.FromArgb(26, 117, 210);
        private static readonly Color LineColor = Color.FromArgb(232, 236, 241);
        private static readonly Color TextColor = Color.FromArgb(30, 41, 59);

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

        // Scale a 96-DPI pixel value to the current monitor DPI
        private int S(int v) => (int)Math.Round(v * DeviceDpi / 96.0);

        private void InitMetrics()
        {
            _actionFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            _headerFont = new Font("Segoe UI", 10F, FontStyle.Bold);
            _pad = S(16);
            _gap = S(24);
            _chk = S(18);
            _thumb = S(36);
            _wChoose = TextRenderer.MeasureText("Choose File", _actionFont).Width + S(26);
            _wEdit = TextRenderer.MeasureText("Edit", _actionFont).Width + S(26);
            _wDelete = TextRenderer.MeasureText("Delete", _actionFont).Width + S(26);
        }

        private int ActionsColumnWidth =>
            _pad + _wChoose + _gap + _wEdit + _gap + _wDelete + _gap + _chk + _pad;

        private void InitRuntime()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            InitMetrics();
            BuildResponsiveLayout();
            SetupComboBox();
            FitCombo();
            RepositionToolbar();
            SetupDataGridView();
            LoadTableData();

            btnCreate.Click += BtnCreate_Click;
            btnCreateTableList.Click += BtnCreateTableList_Click;
            btnSetImageAll.Click += BtnSetImageAll_Click;
            btnMoveGroup.Click += BtnMoveGroup_Click;
            btnDeletebySelect.Click += BtnDeletebySelect_Click;
            txtSearch.TextChanged += (s, e) => LoadTableData();
        }

        // Size a button to its real text so nothing is ever truncated (any DPI / font scaling)
        private void FitButton(Guna.UI2.WinForms.Guna2Button b)
        {
            int w = TextRenderer.MeasureText(b.Text, b.Font).Width + S(44);
            b.Size = new Size(Math.Max(w, S(90)), S(38));
        }

        private void FitCombo()
        {
            int max = 0;
            foreach (var item in comboGroupTable.Items)
                max = Math.Max(max, TextRenderer.MeasureText(item?.ToString() ?? "", comboGroupTable.Font).Width);

            comboGroupTable.Size = new Size(Math.Max(max + S(64), S(190)), S(38));
            comboGroupTable.DropDownWidth = comboGroupTable.Width + S(20);
        }

        private void StyleButton(Guna.UI2.WinForms.Guna2Button b, string text)
        {
            b.Text = text;
            b.BorderRadius = 4;
            b.FillColor = Blue;
            b.HoverState.FillColor = Color.FromArgb(21, 101, 192);
            b.ForeColor = Color.White;
            b.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            b.TextAlign = HorizontalAlignment.Center;
            b.TextOffset = new Point(0, 0);
            b.Cursor = Cursors.Hand;
            b.ShadowDecoration.Enabled = false;
            FitButton(b);
        }

        private void BuildResponsiveLayout()
        {
            this.SuspendLayout();
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // 1. Title bar
            pnlTitle = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(58),
                BackColor = Color.White
            };

            label1.Text = "Table list";
            label1.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            label1.ForeColor = Blue;
            label1.AutoSize = true;

            StyleButton(btnCreate, "Create");

            pnlTitle.Controls.Add(label1);
            pnlTitle.Controls.Add(btnCreate);

            void PositionTitle()
            {
                label1.Location = new Point(S(18), (pnlTitle.Height - label1.Height) / 2);
                btnCreate.Location = new Point(pnlTitle.Width - btnCreate.Width - S(20), (pnlTitle.Height - btnCreate.Height) / 2);
            }
            pnlTitle.Resize += (s, ev) => PositionTitle();
            PositionTitle();

            // 2. Toolbar (positions itself in RepositionToolbar)
            pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(110),
                BackColor = Color.White
            };

            StyleButton(btnCreateTableList, "Create Table List");
            StyleButton(btnSetImageAll, "Set Image for selected items");
            StyleButton(btnMoveGroup, "Move Group Table By Selected");
            StyleButton(btnDeletebySelect, "Delete By Selected");

            comboGroupTable.BorderRadius = 4;
            comboGroupTable.BorderColor = Color.FromArgb(209, 213, 219);
            comboGroupTable.FillColor = Color.White;
            comboGroupTable.ForeColor = TextColor;
            comboGroupTable.Font = new Font("Segoe UI", 9.5F);
            comboGroupTable.ItemHeight = S(28);
            comboGroupTable.ShadowDecoration.Enabled = false;

            txtSearch.Height = S(38);
            txtSearch.BorderRadius = 4;
            txtSearch.BorderColor = Color.FromArgb(209, 213, 219);
            txtSearch.PlaceholderText = "Search table name or code...";
            txtSearch.PlaceholderForeColor = Color.FromArgb(148, 163, 184);
            txtSearch.Font = new Font("Segoe UI", 9.5F);
            txtSearch.ForeColor = TextColor;
            txtSearch.ShadowDecoration.Enabled = false;

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
            txtSearch.IconLeftSize = new Size(S(14), S(14));
            txtSearch.IconLeftOffset = new Point(S(6), 0);

            pnlToolbar.Controls.Add(btnCreateTableList);
            pnlToolbar.Controls.Add(btnSetImageAll);
            pnlToolbar.Controls.Add(comboGroupTable);
            pnlToolbar.Controls.Add(btnMoveGroup);
            pnlToolbar.Controls.Add(btnDeletebySelect);
            pnlToolbar.Controls.Add(txtSearch);

            pnlToolbar.Resize += (s, ev) => RepositionToolbar();

            // 3. Grid container
            pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(S(20), S(4), S(20), S(20))
            };

            dgvDataTableList.Dock = DockStyle.Fill;
            pnlGrid.Controls.Add(dgvDataTableList);

            this.Controls.Clear();
            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlToolbar);
            this.Controls.Add(pnlTitle);

            this.ResumeLayout(true);
        }

        // Wide: [left group ........ right group] + search on the 2nd row (right aligned)
        // Narrow: every control wraps onto as many rows as needed - nothing is clipped
        private void RepositionToolbar()
        {
            if (pnlToolbar == null || pnlToolbar.ClientSize.Width <= 0) return;

            int margin = S(20), gap = S(10), rowH = S(38), rowGap = S(10), top = S(10);
            int width = pnlToolbar.ClientSize.Width;

            btnCreateTableList.Height = btnSetImageAll.Height = btnMoveGroup.Height =
                btnDeletebySelect.Height = comboGroupTable.Height = txtSearch.Height = rowH;

            txtSearch.Width = Math.Min(S(280), Math.Max(S(160), width - margin * 2));

            Control[] left = { btnCreateTableList, btnSetImageAll, comboGroupTable };
            Control[] right = { btnMoveGroup, btnDeletebySelect };

            int leftW = 0, rightW = 0;
            foreach (var c in left) leftW += c.Width + gap;
            foreach (var c in right) rightW += c.Width + gap;
            int needed = margin * 2 + leftW + rightW;

            int bottom;
            if (width >= needed)
            {
                int x = margin;
                foreach (var c in left)
                {
                    c.Location = new Point(x, top);
                    x += c.Width + gap;
                }

                x = width - margin;
                for (int i = right.Length - 1; i >= 0; i--)
                {
                    x -= right[i].Width;
                    right[i].Location = new Point(x, top);
                    x -= gap;
                }

                txtSearch.Location = new Point(width - margin - txtSearch.Width, top + rowH + rowGap);
                bottom = top + rowH + rowGap + rowH;
            }
            else
            {
                Control[] all = { btnCreateTableList, btnSetImageAll, comboGroupTable, btnMoveGroup, btnDeletebySelect, txtSearch };
                int x = margin, y = top;
                foreach (var c in all)
                {
                    if (x > margin && x + c.Width > width - margin)
                    {
                        x = margin;
                        y += rowH + rowGap;
                    }
                    c.Location = new Point(x, y);
                    x += c.Width + gap;
                }
                bottom = y + rowH;
            }

            int newHeight = bottom + rowGap;
            if (pnlToolbar.Height != newHeight)
                pnlToolbar.Height = newHeight;
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
            // Double buffering removes the flicker / leftover lines while resizing
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, dgvDataTableList, new object[] { true });

            dgvDataTableList.EnableHeadersVisualStyles = false;
            dgvDataTableList.BackgroundColor = Color.White;
            dgvDataTableList.BorderStyle = BorderStyle.None;
            dgvDataTableList.ReadOnly = true;

            // ---- Borders: Guna's theme must be set too, otherwise it re-applies its own lines ----
            dgvDataTableList.ThemeStyle.BackColor = Color.White;
            dgvDataTableList.ThemeStyle.GridColor = LineColor;
            dgvDataTableList.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.None;
            dgvDataTableList.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvDataTableList.GridColor = LineColor;
            dgvDataTableList.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgvDataTableList.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvDataTableList.RowTemplate.DividerHeight = 0;

            // Header: flat white
            dgvDataTableList.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            dgvDataTableList.ColumnHeadersDefaultCellStyle.ForeColor = TextColor;
            dgvDataTableList.ColumnHeadersDefaultCellStyle.Font = _headerFont;
            dgvDataTableList.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            dgvDataTableList.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextColor;
            dgvDataTableList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvDataTableList.ColumnHeadersHeight = S(48);

            dgvDataTableList.RowTemplate.Height = S(58);
            dgvDataTableList.AllowUserToAddRows = false;
            dgvDataTableList.AllowUserToDeleteRows = false;
            dgvDataTableList.AllowUserToResizeRows = false;
            dgvDataTableList.AllowUserToResizeColumns = false;
            dgvDataTableList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDataTableList.MultiSelect = false;
            dgvDataTableList.RowHeadersVisible = false;

            dgvDataTableList.DefaultCellStyle.BackColor = Color.White;
            dgvDataTableList.DefaultCellStyle.ForeColor = TextColor;
            dgvDataTableList.DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 247, 255);
            dgvDataTableList.DefaultCellStyle.SelectionForeColor = TextColor;
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
                    Width = S(140)
                };
                dgvDataTableList.Columns.Insert(imgIdx, imgCol);
            }

            dgvDataTableList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            if (dgvDataTableList.Columns.Contains("colTableName"))
            {
                var c = dgvDataTableList.Columns["colTableName"];
                c.HeaderText = "Table Name";
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                c.FillWeight = 26;
                c.MinimumWidth = S(140);
                c.DefaultCellStyle.Padding = new Padding(S(16), 0, 0, 0);
                c.HeaderCell.Style.Padding = new Padding(S(16), 0, 0, 0);
            }

            if (dgvDataTableList.Columns.Contains("colGroupTableName"))
            {
                var c = dgvDataTableList.Columns["colGroupTableName"];
                c.HeaderText = "Group Table Name";
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                c.FillWeight = 26;
                c.MinimumWidth = S(150);
                c.DefaultCellStyle.Padding = new Padding(S(16), 0, 0, 0);
                c.HeaderCell.Style.Padding = new Padding(S(16), 0, 0, 0);
            }

            if (dgvDataTableList.Columns.Contains("colImage"))
            {
                var c = dgvDataTableList.Columns["colImage"];
                c.HeaderText = "Image Name";
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                c.Width = S(140);
                c.MinimumWidth = S(120);
                c.Resizable = DataGridViewTriState.False;
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            if (dgvDataTableList.Columns.Contains("colActions"))
            {
                var c = dgvDataTableList.Columns["colActions"];
                c.HeaderText = "Select All";
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                c.Width = ActionsColumnWidth;       // computed from real text widths
                c.MinimumWidth = ActionsColumnWidth;
                c.Resizable = DataGridViewTriState.False;
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            ApplyFlatBorders();

            // Single set of handlers (the old MouseUp / ColumnHeaderMouseClick duplicates toggled checkboxes 2-3 times)
            dgvDataTableList.CellPainting -= dgvDataTableList_CellPainting;
            dgvDataTableList.CellPainting += dgvDataTableList_CellPainting;

            dgvDataTableList.RowPostPaint -= dgvDataTableList_RowPostPaint;
            dgvDataTableList.RowPostPaint += dgvDataTableList_RowPostPaint;

            dgvDataTableList.CellMouseClick -= dgvDataTableList_CellMouseClick;
            dgvDataTableList.CellMouseClick += dgvDataTableList_CellMouseClick;

            dgvDataTableList.MouseMove -= DgvDataTableList_MouseMove;
            dgvDataTableList.MouseMove += DgvDataTableList_MouseMove;

            dgvDataTableList.Resize -= DgvDataTableList_Resize;
            dgvDataTableList.Resize += DgvDataTableList_Resize;
        }

        // Removes every border the grid itself would draw (outer frame, vertical and horizontal cell lines).
        // The only line left is the one we paint under each row in CellPainting.
        private void ApplyFlatBorders()
        {
            dgvDataTableList.BorderStyle = BorderStyle.None;
            dgvDataTableList.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgvDataTableList.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvDataTableList.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvDataTableList.GridColor = Color.White;
            dgvDataTableList.ThemeStyle.GridColor = Color.White;
            dgvDataTableList.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.None;
            dgvDataTableList.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            try
            {
                dgvDataTableList.AdvancedCellBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;
                dgvDataTableList.AdvancedColumnHeadersBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;
            }
            catch { }
        }

        // Runs after the grid has painted a row. Whatever border the grid/Guna theme drew on top of our
        // cells (left edge, column dividers, dark horizontal lines) is painted over with the row colour,
        // then the single light bottom line is drawn.
        private void dgvDataTableList_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
            Color bg = selected ? Color.FromArgb(240, 247, 255) : Color.White;
            Graphics g = e.Graphics;

            using (SolidBrush bgBrush = new SolidBrush(bg))
            {
                foreach (DataGridViewColumn col in dgvDataTableList.Columns)
                {
                    if (!col.Visible) continue;
                    Rectangle r = dgvDataTableList.GetCellDisplayRectangle(col.Index, e.RowIndex, false);
                    if (r.Width <= 0 || r.Height <= 0) continue;

                    g.FillRectangle(bgBrush, r.Left - 1, r.Top, 3, r.Height - 1);   // left edge / divider
                    g.FillRectangle(bgBrush, r.Right - 2, r.Top, 3, r.Height - 1);  // right edge / divider
                    g.FillRectangle(bgBrush, r.Left, r.Top, r.Width, 1);            // top line
                }
            }

            using (Pen line = new Pen(LineColor, 1f))
            {
                g.DrawLine(line, e.RowBounds.Left, e.RowBounds.Bottom - 1, e.RowBounds.Right, e.RowBounds.Bottom - 1);
            }
        }

        private void DgvDataTableList_Resize(object? sender, EventArgs e)
        {
            dgvDataTableList.Invalidate();
        }

        private void LoadTableData()
        {
            // Release old thumbnails
            if (dgvDataTableList.Columns.Contains("colImage"))
            {
                foreach (DataGridViewRow row in dgvDataTableList.Rows)
                    if (row.Cells["colImage"].Value is Image old) old.Dispose();
            }
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
                    Image tableImg = CreateRoundedThumbnail(imgPath, _thumb);

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

            // Guna can re-apply its theme borders after data changes, so reset them every load
            ApplyFlatBorders();
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

                        // Picture placeholder (scaled with the thumbnail)
                        int m = Math.Max(4, size / 6);
                        Rectangle iconRect = new Rectangle(m, m, size - m * 2, size - m * 2);
                        float k = iconRect.Width / 20f;
                        using (GraphicsPath iconPath = GetRoundedPath(iconRect, 2))
                        {
                            using (SolidBrush skyBrush = new SolidBrush(Color.FromArgb(224, 242, 254)))
                            {
                                g.FillPath(skyBrush, iconPath);
                            }

                            g.SetClip(iconPath);
                            using (SolidBrush sunBrush = new SolidBrush(Color.FromArgb(250, 204, 21)))
                            {
                                g.FillEllipse(sunBrush, iconRect.Right - 7 * k, iconRect.Y + 2 * k, 4 * k, 4 * k);
                            }
                            using (SolidBrush mtn1 = new SolidBrush(Color.FromArgb(74, 222, 128)))
                            {
                                g.FillPolygon(mtn1, new PointF[]
                                {
                                    new PointF(iconRect.Left - 2 * k, iconRect.Bottom),
                                    new PointF(iconRect.Left + 5 * k, iconRect.Y + 6 * k),
                                    new PointF(iconRect.Left + 11 * k, iconRect.Bottom)
                                });
                            }
                            using (SolidBrush mtn2 = new SolidBrush(Color.FromArgb(34, 197, 94)))
                            {
                                g.FillPolygon(mtn2, new PointF[]
                                {
                                    new PointF(iconRect.Left + 4 * k, iconRect.Bottom),
                                    new PointF(iconRect.Left + 10 * k, iconRect.Y + 4 * k),
                                    new PointF(iconRect.Right + 2 * k, iconRect.Bottom)
                                });
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

        // ------------------------------------------------------------------
        // Actions column geometry - the ONE place used by painting, click and hover
        // ------------------------------------------------------------------
        private (Rectangle choose, Rectangle edit, Rectangle delete, Rectangle check) GetActionRects(Rectangle cell)
        {
            int btnH = S(30);
            int cy = cell.Y + (cell.Height - btnH) / 2;
            int x = cell.X + _pad;

            var choose = new Rectangle(x, cy, _wChoose, btnH);
            x = choose.Right + _gap;
            var edit = new Rectangle(x, cy, _wEdit, btnH);
            x = edit.Right + _gap;
            var del = new Rectangle(x, cy, _wDelete, btnH);
            x = del.Right + _gap;
            var chk = new Rectangle(x, cell.Y + (cell.Height - _chk) / 2, _chk, _chk);

            return (choose, edit, del, chk);
        }

        private bool IsAllSelected()
        {
            if (dgvDataTableList.Rows.Count == 0) return false;
            foreach (DataGridViewRow row in dgvDataTableList.Rows)
            {
                if (row.Tag != null && !_selectedTableIds.Contains(Convert.ToInt32(row.Tag)))
                    return false;
            }
            return true;
        }

        private void DrawCheckBox(Graphics g, Rectangle box, bool isChecked)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath p = GetRoundedPath(box, 2))
            using (SolidBrush bg = new SolidBrush(isChecked ? Blue : Color.White))
            using (Pen border = new Pen(isChecked ? Blue : Color.FromArgb(148, 163, 184), 1.5f))
            {
                g.FillPath(bg, p);
                g.DrawPath(border, p);
                if (isChecked)
                {
                    float k = box.Width / 18f;
                    using (Pen tick = new Pen(Color.White, 2f))
                    {
                        g.DrawLines(tick, new PointF[]
                        {
                            new PointF(box.X + 3 * k, box.Y + 9 * k),
                            new PointF(box.X + 7 * k, box.Y + 13 * k),
                            new PointF(box.X + 14 * k, box.Y + 4 * k)
                        });
                    }
                }
            }
        }

        private void dgvDataTableList_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.Graphics == null) return;

            // If Guna's theme put any grid border back, strip it again before drawing
            if (dgvDataTableList.CellBorderStyle != DataGridViewCellBorderStyle.None ||
                dgvDataTableList.BorderStyle != BorderStyle.None)
            {
                ApplyFlatBorders();
            }

            int actionColIdx = dgvDataTableList.Columns.Contains("colActions") ? dgvDataTableList.Columns["colActions"].Index : 3;
            int imageColIdx = dgvDataTableList.Columns.Contains("colImage") ? dgvDataTableList.Columns["colImage"].Index : 2;

            // Background + a single bottom line. We handle the whole cell, so the grid draws NO borders of its own.
            bool isHeader = e.RowIndex == -1;
            bool isSelected = !isHeader && (e.State & DataGridViewElementStates.Selected) != 0;

            using (SolidBrush bg = new SolidBrush(isSelected ? Color.FromArgb(240, 247, 255) : Color.White))
                e.Graphics.FillRectangle(bg, e.CellBounds);

            using (Pen line = new Pen(LineColor, 1f))
                e.Graphics.DrawLine(line, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);

            // ---- Header ----
            if (isHeader)
            {
                if (e.ColumnIndex == actionColIdx)
                {
                    var rects = GetActionRects(e.CellBounds);
                    TextRenderer.DrawText(e.Graphics, "Select All", _headerFont,
                        new Rectangle(e.CellBounds.X + _pad, e.CellBounds.Y, rects.check.X - e.CellBounds.X - _pad, e.CellBounds.Height),
                        TextColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
                    DrawCheckBox(e.Graphics, rects.check, IsAllSelected());
                }
                else
                {
                    e.Paint(e.ClipBounds, DataGridViewPaintParts.ContentForeground);
                }
                e.Handled = true;
                return;
            }

            // ---- Data rows ----
            if (e.ColumnIndex == actionColIdx)
            {
                var r = GetActionRects(e.CellBounds);

                DrawRoundedActionButton(e.Graphics, r.choose, "Choose File", Blue, 4);
                DrawSeparatorDiamond(e.Graphics, r.choose.Right + _gap / 2, r.choose.Y + r.choose.Height / 2);

                DrawRoundedActionButton(e.Graphics, r.edit, "Edit", Blue, 4);
                DrawSeparatorDiamond(e.Graphics, r.edit.Right + _gap / 2, r.edit.Y + r.edit.Height / 2);

                DrawRoundedActionButton(e.Graphics, r.delete, "Delete", Blue, 4);
                DrawSeparatorDiamond(e.Graphics, r.delete.Right + _gap / 2, r.delete.Y + r.delete.Height / 2);

                int tid = dgvDataTableList.Rows[e.RowIndex].Tag != null ? Convert.ToInt32(dgvDataTableList.Rows[e.RowIndex].Tag) : 0;
                DrawCheckBox(e.Graphics, r.check, _selectedTableIds.Contains(tid));

                e.Handled = true;
                return;
            }

            if (e.ColumnIndex == imageColIdx)
            {
                if (e.Value is Image img)
                {
                    int imgX = e.CellBounds.X + (e.CellBounds.Width - _thumb) / 2;
                    int imgY = e.CellBounds.Y + (e.CellBounds.Height - _thumb) / 2;
                    e.Graphics.DrawImage(img, new Rectangle(imgX, imgY, _thumb, _thumb));
                }
                e.Handled = true;
                return;
            }

            // Text columns
            e.Paint(e.ClipBounds, DataGridViewPaintParts.ContentForeground);
            e.Handled = true;
        }

        private void DrawSeparatorDiamond(Graphics g, int cx, int cy)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush brush = new SolidBrush(TextColor))
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
            {
                g.FillPath(brush, path);
                TextRenderer.DrawText(g, text, _actionFont, bounds, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
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
            if (e.ColumnIndex != actionColIdx) return;

            // Header (Select All)
            if (e.RowIndex == -1)
            {
                ToggleSelectAll();
                return;
            }
            if (e.RowIndex < 0) return;

            var row = dgvDataTableList.Rows[e.RowIndex];
            int tableId = row.Tag != null ? Convert.ToInt32(row.Tag) : 0;

            // e.Location is relative to the cell, so build the rects on a cell at (0,0)
            var cell = new Rectangle(0, 0, dgvDataTableList.Columns[e.ColumnIndex].Width, row.Height);
            var r = GetActionRects(cell);
            Point p = e.Location;

            Rectangle chkHit = r.check;
            chkHit.Inflate(S(8), S(12));

            if (r.choose.Contains(p))
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp;*.bmp";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        row.Cells["colImage"].Value = CreateRoundedThumbnail(ofd.FileName, _thumb);
                        if (tableId > 0)
                        {
                            DbHelper.ExecuteNonQuery("UPDATE dbo.DINING_TABLE SET ImagePath = @Img WHERE TableID = @ID",
                                new SqlParameter("@Img", ofd.FileName),
                                new SqlParameter("@ID", tableId));
                        }
                    }
                }
            }
            else if (r.edit.Contains(p))
            {
                if (tableId > 0) OpenEditTable(tableId);
            }
            else if (r.delete.Contains(p))
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
            else if (chkHit.Contains(p))
            {
                if (tableId > 0)
                {
                    if (!_selectedTableIds.Remove(tableId)) _selectedTableIds.Add(tableId);
                    dgvDataTableList.Invalidate();
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
                var r = GetActionRects(cellRect);
                Rectangle chkHit = r.check;
                chkHit.Inflate(S(8), S(12));

                bool onButton = r.choose.Contains(e.Location) || r.edit.Contains(e.Location)
                             || r.delete.Contains(e.Location) || chkHit.Contains(e.Location);
                dgvDataTableList.Cursor = onButton ? Cursors.Hand : Cursors.Default;
            }
            else
            {
                dgvDataTableList.Cursor = Cursors.Default;
            }
        }

        private void ToggleSelectAll()
        {
            if (dgvDataTableList.Rows.Count == 0) return;

            if (IsAllSelected())
            {
                _selectedTableIds.Clear();
            }
            else
            {
                foreach (DataGridViewRow row in dgvDataTableList.Rows)
                {
                    if (row.Tag != null)
                        _selectedTableIds.Add(Convert.ToInt32(row.Tag));
                }
            }

            dgvDataTableList.Invalidate();
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
                prompt.AutoScaleMode = AutoScaleMode.Dpi;
                prompt.ClientSize = new Size(S(364), S(160));
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "Move Tables to Group";
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;
                prompt.BackColor = Color.White;

                Label lbl = new Label
                {
                    Left = S(24),
                    Top = S(20),
                    Text = $"Move {targetIds.Count} selected table(s) to:",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = TextColor,
                    AutoSize = true
                };

                ComboBox cbo = new ComboBox
                {
                    Left = S(24),
                    Top = S(54),
                    Width = S(316),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 10F),
                    DataSource = dtGroups,
                    DisplayMember = "GroupName",
                    ValueMember = "TableGroupID"
                };

                Button btnOk = new Button
                {
                    Text = "Move",
                    Left = S(140),
                    Top = S(104),
                    Width = S(95),
                    Height = S(34),
                    BackColor = Blue,
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
                    Left = S(245),
                    Top = S(104),
                    Width = S(95),
                    Height = S(34),
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

        // Same pattern as BtnCreate_Click: replace the current view inside the parent.
        // CreateTableList is a Form, so it is embedded as a non-top-level child control.
        // When it closes (after Save / Cancel), we navigate back to a fresh TableList.
        private void BtnCreateTableList_Click(object? sender, EventArgs e)
        {
            Control? parent = this.Parent;
            if (parent != null)
            {
                parent.Controls.Clear();
                CreateTableList createTableList = new CreateTableList { Dock = DockStyle.Fill };
                parent.Controls.Add(createTableList);
                createTableList.BringToFront();
            }
        }

        private void TableList_Load(object sender, EventArgs e)
        {

        }
    }
}