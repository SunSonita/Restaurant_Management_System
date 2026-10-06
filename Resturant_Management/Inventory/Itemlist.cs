using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Inventory
{
    public partial class Itemlist : UserControl
    {
        private readonly Color Blue = Color.FromArgb(21, 119, 214);
        private readonly Color BorderGray = Color.FromArgb(220, 224, 230);

        private const int ToolbarHeight = 56;

        // Only controls that are NOT already in the designer
        private Panel pnlHeader = null!, pnlToolbar = null!, pnlFooter = null!, pnlGrid = null!;
        private Panel pnlSearchBox = null!;
        private Label lblPageInfo = null!;

        private bool _isInitialized = false;

        public Itemlist()
        {
            InitializeComponent();
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            this.Load += Itemlist_Load;
            this.VisibleChanged += (s, e) =>
            {
                if (this.Visible && !DesignTimeHelper.IsInDesignMode(this))
                {
                    if (!_isInitialized)
                        InitRuntime();
                    else
                        LoadItemData();
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

        private void Itemlist_Load(object? sender, EventArgs e)
        {
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            InitRuntime();
        }

        private void InitRuntime()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            BuildLayout();
            ApplyUIStyle();
            SetupColumns();

            btnCreate.Click -= btnCreate_Click;
            btnCreate.Click += btnCreate_Click;

            dgvItemMasterData.CellClick -= dgvItemMasterData_CellClick;
            dgvItemMasterData.CellClick += dgvItemMasterData_CellClick;

            chkInactive.CheckedChanged -= ChkInactive_CheckedChanged;
            chkInactive.CheckedChanged += ChkInactive_CheckedChanged;

            txtSearch.TextChanged -= TxtSearch_TextChanged;
            txtSearch.TextChanged += TxtSearch_TextChanged;

            LoadItemData();
        }

        private void ChkInactive_CheckedChanged(object? sender, EventArgs e) => LoadItemData();
        private void TxtSearch_TextChanged(object? sender, EventArgs e) => LoadItemData();

        private void BuildLayout()
        {
            SuspendLayout();
            BackColor = Color.White;

            // ---------- Header (title + Create) ----------
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.White };
            pnlHeader.Paint += (s, ev) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 200, 200), 2))
                    ev.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            // designer controls: reuse them, just restyle and move them
            lblTitle.Text = "Item Master Data";
            lblTitle.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
            lblTitle.ForeColor = Blue;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Location = new Point(10, 14);

            // Guna2Button: use FillColor / BorderRadius (no FlatStyle)
            btnCreate.Text = "Create";
            btnCreate.Size = new Size(120, 40);
            btnCreate.FillColor = Blue;
            btnCreate.ForeColor = Color.White;
            btnCreate.BorderRadius = 4;
            btnCreate.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnCreate.TextAlign = HorizontalAlignment.Center;
            btnCreate.TextOffset = new Point(0, 0);
            btnCreate.Padding = new Padding(0);
            btnCreate.Cursor = Cursors.Hand;

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(btnCreate);

            void RepositionHeaderButtons()
            {
                btnCreate.Location = new Point(pnlHeader.Width - btnCreate.Width - 15, 9);
            }
            pnlHeader.Resize += (s, ev) => RepositionHeaderButtons();
            RepositionHeaderButtons();

            // ---------- Toolbar (Inactive + search) ----------
            pnlToolbar = new Panel { Dock = DockStyle.Top, Height = ToolbarHeight, BackColor = Color.White };

            chkInactive.Text = "Inactive";
            chkInactive.Font = new Font("Segoe UI", 9.5f);
            chkInactive.AutoSize = true;

            pnlSearchBox = new Panel
            {
                Size = new Size(240, 36),
                BackColor = Color.White
            };
            pnlSearchBox.Paint += (s, ev) =>
            {
                using var pen = new Pen(BorderGray, 1);
                ev.Graphics.DrawRectangle(pen, 0, 0, pnlSearchBox.Width - 1, pnlSearchBox.Height - 1);
            };
            pnlSearchBox.Click += (s, e) => txtSearch.Focus();

            txtSearch.Multiline = false;
            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.Font = new Font("Segoe UI", 10f);
            txtSearch.ForeColor = Color.FromArgb(50, 50, 50);
            txtSearch.Text = "";
            txtSearch.PlaceholderText = "Search...";
            txtSearch.Width = pnlSearchBox.Width - 20;
            txtSearch.Location = new Point(10, (pnlSearchBox.Height - txtSearch.PreferredHeight) / 2);

            pnlSearchBox.Controls.Add(txtSearch);
            pnlToolbar.Controls.Add(chkInactive);
            pnlToolbar.Controls.Add(pnlSearchBox);
            pnlToolbar.Resize += (s, ev) => PositionToolbar();
            PositionToolbar();

            // ---------- Footer (total items count) ----------
            pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.White };
            pnlFooter.Paint += (s, ev) =>
            {
                using var pen = new Pen(BorderGray, 1);
                ev.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
            };

            lblPageInfo = new Label
            {
                Text = "Total: 0 items",
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI", 9.5f),
                AutoSize = true
            };
            pnlFooter.Controls.Add(lblPageInfo);
            void RepositionFooter()
            {
                lblPageInfo.Location = new Point(pnlFooter.Width - lblPageInfo.Width - 20, (pnlFooter.Height - lblPageInfo.Height) / 2);
            }
            pnlFooter.Resize += (s, ev) => RepositionFooter();
            RepositionFooter();

            // ---------- Grid ----------
            pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 0), BackColor = Color.White };
            dgvItemMasterData.Margin = Padding.Empty;
            dgvItemMasterData.Dock = DockStyle.Fill;
            pnlGrid.Controls.Add(dgvItemMasterData);

            Controls.Add(pnlGrid);
            Controls.Add(pnlFooter);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlHeader);

            // Hide any leftover designer control that is still sitting directly on the
            // UserControl (an empty docked panel there is what creates a big blank gap).
            foreach (Control c in Controls.Cast<Control>().ToList())
            {
                if (c != pnlHeader && c != pnlToolbar && c != pnlFooter && c != pnlGrid)
                    c.Visible = false;
            }

            // Dock order: header, toolbar and footer are docked first, the grid fills what is left
            Controls.SetChildIndex(pnlGrid, 0);
            Controls.SetChildIndex(pnlFooter, 1);
            Controls.SetChildIndex(pnlToolbar, 2);
            Controls.SetChildIndex(pnlHeader, 3);

            ResumeLayout(true);
        }

        private void PositionToolbar()
        {
            if (pnlToolbar == null) return;

            // vertically centre both items inside the toolbar with clean outside margins
            chkInactive.Location = new Point(20, (pnlToolbar.Height - chkInactive.Height) / 2);
            if (pnlSearchBox != null)
            {
                pnlSearchBox.Location = new Point(
                    pnlToolbar.Width - pnlSearchBox.Width - 20,
                    (pnlToolbar.Height - pnlSearchBox.Height) / 2);
            }
        }

        private void ApplyUIStyle()
        {
            dgvItemMasterData.BackgroundColor = Color.White;
            dgvItemMasterData.BorderStyle = BorderStyle.FixedSingle;
            dgvItemMasterData.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvItemMasterData.GridColor = Color.FromArgb(235, 238, 242);
            dgvItemMasterData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItemMasterData.AllowUserToAddRows = false;
            dgvItemMasterData.AllowUserToDeleteRows = false;
            dgvItemMasterData.AllowUserToResizeRows = false;
            dgvItemMasterData.ReadOnly = true;
            dgvItemMasterData.RowHeadersVisible = false;
            dgvItemMasterData.RowTemplate.Height = 62;
            dgvItemMasterData.EnableHeadersVisualStyles = false;
            dgvItemMasterData.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgvItemMasterData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvItemMasterData.ColumnHeadersHeight = 42;

            dgvItemMasterData.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(64, 64, 64),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                SelectionBackColor = Color.White,
                SelectionForeColor = Color.FromArgb(64, 64, 64),
                WrapMode = DataGridViewTriState.False
            };

            dgvItemMasterData.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(50, 50, 50),
                Font = new Font("Segoe UI", 10f),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                SelectionBackColor = Color.FromArgb(240, 245, 255),
                SelectionForeColor = Color.Black,
                WrapMode = DataGridViewTriState.False
            };
        }

        private void SetupColumns()
        {
            dgvItemMasterData.Columns.Clear();

            var colEdit = new DataGridViewTextBoxColumn
            {
                Name = "colEdit",
                HeaderText = "Edit",
                Width = 60,
                Resizable = DataGridViewTriState.False
            };
            colEdit.DefaultCellStyle.ForeColor = Color.FromArgb(0, 114, 206);
            colEdit.DefaultCellStyle.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            colEdit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colEdit.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvItemMasterData.Columns.Add(colEdit);

            dgvItemMasterData.Columns.Add(new DataGridViewImageColumn
            {
                Name = "colImage",
                HeaderText = "Image",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                Width = 75,
                Resizable = DataGridViewTriState.False
            });

            dgvItemMasterData.Columns.Add("colCode", "Item Code");
            dgvItemMasterData.Columns.Add("colName", "Item Name");
            dgvItemMasterData.Columns.Add("colName2", "Item Name 2");
            dgvItemMasterData.Columns.Add("colUom", "Group UoM");
            dgvItemMasterData.Columns.Add("colGroup", "Item Group");
            dgvItemMasterData.Columns.Add("colProcess", "Process");
            dgvItemMasterData.Columns.Add("colCreated", "Created");

            dgvItemMasterData.Columns["colCode"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvItemMasterData.Columns["colCode"].MinimumWidth = 110;

            dgvItemMasterData.Columns["colName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvItemMasterData.Columns["colName"].MinimumWidth = 160;
            dgvItemMasterData.Columns["colName"].FillWeight = 50;
            dgvItemMasterData.Columns["colName"].DefaultCellStyle.Font = DbHelper.GetKhmerFont(10F, FontStyle.Regular);

            dgvItemMasterData.Columns["colName2"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvItemMasterData.Columns["colName2"].MinimumWidth = 140;
            dgvItemMasterData.Columns["colName2"].FillWeight = 50;
            dgvItemMasterData.Columns["colName2"].DefaultCellStyle.Font = DbHelper.GetKhmerFont(10F, FontStyle.Regular);

            dgvItemMasterData.Columns["colUom"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvItemMasterData.Columns["colUom"].MinimumWidth = 120;

            dgvItemMasterData.Columns["colGroup"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvItemMasterData.Columns["colGroup"].MinimumWidth = 130;

            dgvItemMasterData.Columns["colProcess"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvItemMasterData.Columns["colProcess"].MinimumWidth = 110;

            dgvItemMasterData.Columns["colCreated"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvItemMasterData.Columns["colCreated"].MinimumWidth = 100;

            // CellContentClick doesn't fire for plain text columns, so use CellClick
            dgvItemMasterData.CellClick -= dgvItemMasterData_CellClick;
            dgvItemMasterData.CellClick += dgvItemMasterData_CellClick;
            dgvItemMasterData.CellDoubleClick -= dgvItemMasterData_CellDoubleClick;
            dgvItemMasterData.CellDoubleClick += dgvItemMasterData_CellDoubleClick;
            dgvItemMasterData.CellMouseEnter -= dgvItemMasterData_CellMouseEnter;
            dgvItemMasterData.CellMouseEnter += dgvItemMasterData_CellMouseEnter;
        }

        private Image CreatePlaceholderImage()
        {
            Bitmap bmp = new Bitmap(46, 46);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var bg = new SolidBrush(Color.FromArgb(235, 235, 235)))
                    g.FillRectangle(bg, 0, 0, 46, 46);
                g.DrawIcon(SystemIcons.Application, new Rectangle(11, 11, 24, 24));
            }
            return bmp;
        }

        private void LoadItemData()
        {
            dgvItemMasterData.Rows.Clear();
            Image defaultImg = CreatePlaceholderImage();

            try
            {
                string search = txtSearch?.Text?.Trim() ?? "";
                if (search.StartsWith("Search", StringComparison.OrdinalIgnoreCase) && search.Contains("."))
                {
                    search = "";
                }
                bool showInactive = chkInactive?.Checked ?? false;

                string sql = @"
SELECT 
    i.ItemID,
    i.ItemCode,
    i.ItemName,
    ISNULL(i.ItemName2, '') AS ItemName2,
    ISNULL(u.UomName, 'Unit') AS UomName,
    ISNULL(g.GroupName, '') AS GroupName,
    'Item' AS [Type],
    ISNULL(i.ValuationMethod, 'Standard') AS Process,
    FORMAT(i.CreatedAt, 'MM-dd') AS CreatedDate,
    i.ImagePath,
    i.IsInactive
FROM dbo.ITEM i
LEFT JOIN dbo.ITEM_GROUP g ON i.GroupID = g.GroupID
LEFT JOIN dbo.UOM u ON i.UomID = u.UomID
WHERE ISNULL(i.IsInactive, 0) = @ShowInactive
  AND (@Search = '' OR i.ItemCode LIKE @Pattern OR i.ItemName LIKE @Pattern)
ORDER BY i.ItemID;";

                DataTable dt = DbHelper.ExecuteQuery(sql,
                    new SqlParameter("@ShowInactive", showInactive ? 1 : 0),
                    new SqlParameter("@Search", search),
                    new SqlParameter("@Pattern", $"%{search}%"));

                foreach (DataRow r in dt.Rows)
                {
                    Image rowImg = defaultImg;
                    string? imgPath = r["ImagePath"]?.ToString();
                    if (!string.IsNullOrEmpty(imgPath) && System.IO.File.Exists(imgPath))
                    {
                        try { rowImg = Image.FromFile(imgPath); } catch { }
                    }

                    dgvItemMasterData.Rows.Add(
                        "➔",
                        rowImg,
                        r["ItemCode"]?.ToString(),
                        r["ItemName"]?.ToString(),
                        r["ItemName2"]?.ToString(),
                        r["UomName"]?.ToString(),
                        r["GroupName"]?.ToString(),
                        r["Process"]?.ToString(),
                        r["CreatedDate"]?.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading items: {ex.Message}");
                MessageBox.Show($"Error loading items: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Auto size columns to ensure headers and content are fully visible
            dgvItemMasterData.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            if (dgvItemMasterData.Columns["colName"] != null)
                dgvItemMasterData.Columns["colName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            if (dgvItemMasterData.Columns["colName2"] != null)
                dgvItemMasterData.Columns["colName2"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            lblPageInfo.Text = $"Total: {dgvItemMasterData.Rows.Count} items";
            if (pnlFooter != null)
                lblPageInfo.Location = new Point(pnlFooter.Width - lblPageInfo.Width - 20, (pnlFooter.Height - lblPageInfo.Height) / 2);
        }

        private void dgvItemMasterData_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvItemMasterData.Columns["colEdit"].Index)
            {
                OpenUpdateItemForRow(e.RowIndex);
            }
        }

        private void dgvItemMasterData_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                OpenUpdateItemForRow(e.RowIndex);
            }
        }

        private void dgvItemMasterData_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvItemMasterData.Columns["colEdit"].Index)
            {
                dgvItemMasterData.Cursor = Cursors.Hand;
            }
            else
            {
                dgvItemMasterData.Cursor = Cursors.Default;
            }
        }

        private void OpenUpdateItemForRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvItemMasterData.Rows.Count) return;
            string? itemCode = dgvItemMasterData.Rows[rowIndex].Cells["colCode"].Value?.ToString();
            if (string.IsNullOrEmpty(itemCode) && dgvItemMasterData.Rows[rowIndex].Cells.Count > 2)
            {
                itemCode = dgvItemMasterData.Rows[rowIndex].Cells[2].Value?.ToString();
            }

            if (!string.IsNullOrEmpty(itemCode))
            {
                Control? parentContainer = this.Parent;
                if (parentContainer != null)
                {
                    parentContainer.Controls.Clear();
                    UpdateItem updateScreen = new UpdateItem(itemCode) { Dock = DockStyle.Fill };
                    parentContainer.Controls.Add(updateScreen);
                    updateScreen.BringToFront();
                    updateScreen.LoadItem(itemCode);
                }
            }
        }

        private void btnCreate_Click(object? sender, EventArgs e)
        {
            Control? parentContainer = this.Parent;
            if (parentContainer != null)
            {
                parentContainer.Controls.Clear();
                CreateItem createScreen = new CreateItem { Dock = DockStyle.Fill };
                parentContainer.Controls.Add(createScreen);
                createScreen.BringToFront();
            }
        }

        private void pnlTop_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dgvItemMasterData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
