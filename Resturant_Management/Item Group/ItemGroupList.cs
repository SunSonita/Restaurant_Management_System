using FontAwesome.Sharp;
using Guna.UI2.WinForms;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Item_Group
{
    public partial class ItemGroupList : UserControl
    {
        private static readonly Color Blue = Color.FromArgb(25, 118, 210);

        private DataGridView grid = null!;
        private Guna2TextBox txtSearchBox = null!;

        private bool _isInitialized = false;

        public ItemGroupList()
        {
            InitializeComponent();
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            this.Load += ItemGroup_Load;
            this.VisibleChanged += (s, e) =>
            {
                if (this.Visible && !DesignTimeHelper.IsInDesignMode(this))
                {
                    if (!_isInitialized)
                        InitRuntime();
                    else
                        LoadGroupData();
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

        private void ItemGroup_Load(object? sender, EventArgs e)
        {
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            InitRuntime();
        }

        private void InitRuntime()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            BuildUi();
            LoadGroupData();
        }

        private void BuildUi()
        {
            Controls.Clear();
            BackColor = Color.White;
            Dock = DockStyle.Fill;

            // ----- Grid -----
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                GridColor = Color.FromArgb(221, 221, 221),
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 62 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 60),
                SelectionBackColor = Color.White,
                SelectionForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 30, 30),
                SelectionBackColor = Color.FromArgb(238, 243, 248),
                SelectionForeColor = Color.FromArgb(30, 30, 30),
                Font = new Font("Segoe UI", 10F),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            var colCategory = new DataGridViewTextBoxColumn { HeaderText = "Category", FillWeight = 36 };
            colCategory.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colCategory.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colCategory.DefaultCellStyle.Padding = new Padding(15, 0, 0, 0);

            var colImage = new DataGridViewImageColumn
            {
                HeaderText = "Image",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 80
            };
            colImage.DefaultCellStyle.Padding = new Padding(14, 8, 14, 8);

            var colEdit = new DataGridViewImageColumn { Name = "colEdit", HeaderText = "Edit", FillWeight = 9, ImageLayout = DataGridViewImageCellLayout.Normal };
            var colAddChild = new DataGridViewImageColumn { Name = "colAddChild", HeaderText = "Add Child", FillWeight = 11, ImageLayout = DataGridViewImageCellLayout.Normal };
            var colId = new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "ID", Visible = false };

            grid.Columns.AddRange(
                colCategory,
                colImage,
                new DataGridViewTextBoxColumn { HeaderText = "Number of Sub-groups", FillWeight = 36 },
                new DataGridViewTextBoxColumn { HeaderText = "Visible", FillWeight = 13 },
                new DataGridViewTextBoxColumn { HeaderText = "Level", FillWeight = 12 },
                colEdit,
                colAddChild,
                colId
            );

            grid.CellClick += Grid_CellClick;

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30, 0, 30, 0) };
            gridHost.Controls.Add(grid);

            // ----- Search -----
            txtSearchBox = new Guna2TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Search ...",
                BorderRadius = 4,
                BorderColor = Color.FromArgb(220, 220, 220),
                FillColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                IconLeft = MakeIcon(IconChar.Search, Color.FromArgb(90, 90, 90), 18),
                IconLeftSize = new Size(18, 18)
            };
            txtSearchBox.TextChanged += (s, e) => LoadGroupData();

            var searchHost = new Panel { Dock = DockStyle.Right, Width = 215 };
            searchHost.Controls.Add(txtSearchBox);

            var pnlSearchBar = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(0, 18, 30, 14) };
            pnlSearchBar.Controls.Add(searchHost);

            // ----- Header -----
            var lblTitle = new Label
            {
                Text = "Item Group",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Blue,
                AutoSize = true,
                Location = new Point(20, 17)
            };

            var btnCreate = HeaderButton("Create", 88);
            btnCreate.Click += BtnCreate_Click;

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0, 8, 8, 0),
                BackColor = Color.White
            };
            buttons.Controls.Add(btnCreate);

            var header = new Guna2Panel { Dock = DockStyle.Top, Height = 64, FillColor = Color.White };
            header.ShadowDecoration.Enabled = true;
            header.ShadowDecoration.Depth = 6;
            header.Controls.Add(buttons);
            header.Controls.Add(lblTitle);

            var topStrip = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = Color.FromArgb(191, 234, 255) };

            Controls.Add(gridHost);
            Controls.Add(pnlSearchBar);
            Controls.Add(header);
            Controls.Add(topStrip);
        }

        private void LoadGroupData()
        {
            grid.Rows.Clear();
            Image editIcon = MakeIcon(IconChar.Edit, Blue, 20);
            Image addIcon = MakeIcon(IconChar.PlusCircle, Blue, 20);
            Image noImg = MakePlaceholder();

            try
            {
                string search = txtSearchBox?.Text?.Trim() ?? "";
                string sql = @"
SELECT 
    g.GroupID,
    g.GroupName,
    g.GroupCode,
    g.ImagePath,
    (SELECT COUNT(1) FROM dbo.ITEM_GROUP s WHERE s.ParentGroupID = g.GroupID) AS SubGroupCount,
    CASE WHEN g.IsVisible = 1 THEN 'Yes' ELSE 'No' END AS VisibleStr,
    CASE WHEN g.ParentGroupID IS NULL THEN '0' ELSE '1' END AS [Level]
FROM dbo.ITEM_GROUP g
WHERE (@Search = '' OR g.GroupName LIKE @Pattern OR g.GroupCode LIKE @Pattern)
ORDER BY g.GroupID;";

                DataTable dt = DbHelper.ExecuteQuery(sql,
                    new SqlParameter("@Search", search),
                    new SqlParameter("@Pattern", $"%{search}%"));

                foreach (DataRow r in dt.Rows)
                {
                    Image rowImg = noImg;
                    string? imgPath = r["ImagePath"]?.ToString();
                    if (!string.IsNullOrEmpty(imgPath) && System.IO.File.Exists(imgPath))
                    {
                        try { rowImg = Image.FromFile(imgPath); } catch { }
                    }

                    string subCount = r["SubGroupCount"]?.ToString() ?? "0";
                    grid.Rows.Add(
                        r["GroupName"]?.ToString(),
                        rowImg,
                        subCount == "0" ? "" : subCount,
                        r["VisibleStr"]?.ToString(),
                        r["Level"]?.ToString(),
                        editIcon,
                        addIcon,
                        Convert.ToInt32(r["GroupID"])
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading groups: {ex.Message}");
            }
            grid.ClearSelection();
        }

        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int idIndex = grid.Columns["colId"].Index;
            int editIndex = grid.Columns["colEdit"].Index;
            int addIndex = grid.Columns["colAddChild"].Index;

            if (e.ColumnIndex == editIndex)
            {
                int groupId = Convert.ToInt32(grid.Rows[e.RowIndex].Cells[idIndex].Value);
                Control? parent = this.Parent;
                if (parent != null)
                {
                    parent.Controls.Clear();
                    UpdateGroup editView = new UpdateGroup(groupId) { Dock = DockStyle.Fill };
                    parent.Controls.Add(editView);
                    editView.BringToFront();
                }
            }
            else if (e.ColumnIndex == addIndex)
            {
                int parentId = Convert.ToInt32(grid.Rows[e.RowIndex].Cells[idIndex].Value);
                Control? parent = this.Parent;
                if (parent != null)
                {
                    parent.Controls.Clear();
                    CreateGroup createChild = new CreateGroup(parentId) { Dock = DockStyle.Fill };
                    parent.Controls.Add(createChild);
                    createChild.BringToFront();
                }
            }
        }

        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            Control? parent = this.Parent;
            if (parent != null)
            {
                parent.Controls.Clear();
                CreateGroup createView = new CreateGroup() { Dock = DockStyle.Fill };
                parent.Controls.Add(createView);
                createView.BringToFront();
            }
        }

        private static Guna2Button HeaderButton(string text, int width) => new Guna2Button
        {
            Text = text,
            Size = new Size(width, 40),
            FillColor = Blue,
            ForeColor = Color.White,
            BorderRadius = 4,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(4, 0, 4, 0),
            Cursor = Cursors.Hand
        };

        private static Image MakeIcon(IconChar ch, Color color, int size)
        {
            using (var pb = new IconPictureBox
            {
                IconChar = ch,
                IconColor = color,
                IconSize = size,
                Size = new Size(size, size),
                BackColor = Color.Transparent
            })
                return new Bitmap(pb.Image);
        }

        private static Image MakePlaceholder()
        {
            var bmp = new Bitmap(50, 44);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var fill = new SolidBrush(Color.FromArgb(235, 235, 235)))
                    g.FillRectangle(fill, 0, 0, bmp.Width, bmp.Height);
                using (var glyph = MakeIcon(IconChar.Image, Color.FromArgb(150, 150, 150), 20))
                    g.DrawImage(glyph, 15, 12, 20, 20);
            }
            return bmp;
        }
    }
}
