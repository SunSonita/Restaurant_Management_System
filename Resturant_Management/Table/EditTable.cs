using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Table
{
    public partial class EditTable : UserControl
    {
        private int _tableId;
        private string _selectedImagePath = "";
        private bool _styled = false;

        private readonly Label lblCode = new Label();
        private readonly Label lblName = new Label();

        private static readonly Color Blue = Color.FromArgb(26, 117, 210);
        private static readonly Color BorderGray = Color.FromArgb(209, 213, 219);
        private static readonly Color TextColor = Color.FromArgb(30, 41, 59);

        public EditTable() : this(1)
        {
        }

        public EditTable(int tableId)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            _tableId = tableId;

            btnUpdate.Click += BtnUpdate_Click;
            btnBack.Click += BtnBack_Click;
            PicItem.Click += PicItem_Click;
            label1.Click += PicItem_Click;
            this.Load += EditTable_Load;
            this.Resize += (s, e) => RelayoutPage();
        }

        // Scale a 96-DPI pixel value to the current monitor DPI
        private int S(int v) => (int)Math.Round(v * DeviceDpi / 96.0);

        private void EditTable_Load(object? sender, EventArgs e)
        {
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            StyleControls();
            RelayoutPage();

            try
            {
                DataTable dtGroups = DbHelper.ExecuteQuery("SELECT TableGroupID, GroupName FROM dbo.TABLE_GROUP ORDER BY GroupName");

                comboTableGroup.Items.Clear();
                comboTableGroup.DisplayMember = "GroupName";
                comboTableGroup.ValueMember = "TableGroupID";
                comboTableGroup.DataSource = dtGroups;
                comboTableGroup.SelectedIndex = -1;

                LoadTableData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------------
        // Styling - identical to the Create Table page
        // ------------------------------------------------------------------
        private void StyleControls()
        {
            if (_styled) return;
            _styled = true;

            this.BackColor = Color.White;

            label2.Text = "Update Table";
            label2.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label2.ForeColor = Color.Navy;

            Font fieldFont = new Font("Segoe UI", 10F);
            Font labelFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            foreach (var lbl in new[] { lblCode, lblName, lbTableGroup })
            {
                lbl.AutoSize = true;
                lbl.Font = labelFont;
                lbl.ForeColor = TextColor;
                lbl.BackColor = Color.Transparent;
            }
            lblCode.Text = "Code";
            lblName.Text = "Name";
            lbTableGroup.Text = "Group Table";
            panelInformationItem.Controls.Add(lblCode);
            panelInformationItem.Controls.Add(lblName);

            foreach (var txt in new[] { txtCode, txtName })
            {
                txt.BorderRadius = 5;
                txt.BorderColor = BorderGray;
                txt.FillColor = Color.White;
                txt.Font = fieldFont;
                txt.ForeColor = TextColor;
                txt.PlaceholderForeColor = Color.FromArgb(148, 163, 184);
                txt.ShadowDecoration.Enabled = false;
            }
            txtCode.PlaceholderText = "Enter table code";
            txtName.PlaceholderText = "Enter table name";

            comboTableGroup.BorderRadius = 5;
            comboTableGroup.BorderColor = BorderGray;
            comboTableGroup.FillColor = Color.White;
            comboTableGroup.Font = fieldFont;
            comboTableGroup.ForeColor = TextColor;
            comboTableGroup.ItemHeight = S(32);
            comboTableGroup.ShadowDecoration.Enabled = false;

            // Image: plain bordered rectangle + blue underlined link underneath
            PicItem.BackColor = Color.White;
            PicItem.BorderRadius = 0;
            PicItem.BorderStyle = BorderStyle.FixedSingle;
            PicItem.SizeMode = PictureBoxSizeMode.Zoom;
            PicItem.Cursor = Cursors.Hand;

            label1.AutoSize = false;
            label1.Text = "Click to choose image";
            label1.Font = new Font("Segoe UI", 11F, FontStyle.Underline);
            label1.ForeColor = Blue;
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.BackColor = Color.Transparent;
            label1.Cursor = Cursors.Hand;

            foreach (var b in new[] { btnUpdate, btnBack })
            {
                b.Size = new Size(S(120), S(44));
                b.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                b.Cursor = Cursors.Hand;
                b.ShadowDecoration.Enabled = false;
            }

            panelInformationItem.FillColor = Color.White;
        }

        // ------------------------------------------------------------------
        // Responsive layout: equal left/right margin, fields fill the width
        // ------------------------------------------------------------------
        private void RelayoutPage()
        {
            if (!_styled) return;

            int width = ClientSize.Width;
            if (width <= 0) return;

            int margin = S(24);     // same on the left and the right
            int pad = S(28);        // card inner padding (also leaves room for the shadow)
            int fieldH = S(44);
            int labelH = S(22);
            int labelGap = S(6);
            int rowGap = S(18);

            label2.Location = new Point(margin, S(20));
            int cardTop = label2.Bottom + S(14);
            int cardW = Math.Max(S(300), width - margin * 2);
            int inner = cardW - pad * 2;

            int imgW = S(320), imgH = S(215), colGap = S(40);
            bool wide = inner >= S(760);
            int fieldsW = wide ? inner - imgW - colGap : inner;

            Control[] labels = { lblCode, lblName, lbTableGroup };
            Control[] fields = { txtCode, txtName, comboTableGroup };

            int y = pad;
            for (int i = 0; i < fields.Length; i++)
            {
                labels[i].Location = new Point(pad, y);
                y += labelH + labelGap;
                fields[i].SetBounds(pad, y, fieldsW, fieldH);
                y += fieldH + rowGap;
            }
            int fieldsBottom = y - rowGap;

            int imgX, imgY;
            if (wide)
            {
                imgX = pad + inner - imgW;
                imgY = pad;
            }
            else
            {
                imgW = Math.Min(imgW, inner);
                imgX = pad;
                imgY = fieldsBottom + S(24);
            }

            PicItem.SetBounds(imgX, imgY, imgW, imgH);
            label1.SetBounds(imgX, PicItem.Bottom + S(10), imgW, S(28));

            int contentBottom = Math.Max(fieldsBottom, label1.Bottom);
            panelInformationItem.SetBounds(margin, cardTop, cardW, contentBottom + pad);

            int btnTop = panelInformationItem.Bottom + S(24);
            btnUpdate.Location = new Point(margin, btnTop);
            btnBack.Location = new Point(btnUpdate.Right + S(16), btnTop);
        }

        private void LoadTableData()
        {
            try
            {
                string sql = "SELECT TableGroupID, TableCode, TableName, ImagePath FROM dbo.DINING_TABLE WHERE TableID = @ID";
                DataTable dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@ID", _tableId));
                if (dt.Rows.Count > 0)
                {
                    txtCode.Text = dt.Rows[0]["TableCode"]?.ToString();
                    txtName.Text = dt.Rows[0]["TableName"]?.ToString();
                    if (dt.Rows[0]["TableGroupID"] != DBNull.Value)
                    {
                        comboTableGroup.SelectedValue = Convert.ToInt32(dt.Rows[0]["TableGroupID"]);
                    }

                    _selectedImagePath = dt.Rows[0]["ImagePath"]?.ToString() ?? "";
                    ShowImage(_selectedImagePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading table: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Loads a copy of the image so the file on disk is not locked
        private void ShowImage(string path)
        {
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) return;
            try
            {
                using (Image src = Image.FromFile(path))
                {
                    Image? old = PicItem.Image;
                    PicItem.Image = new Bitmap(src);
                    old?.Dispose();
                }
            }
            catch { }
        }

        private void PicItem_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _selectedImagePath = ofd.FileName;
                    ShowImage(_selectedImagePath);
                }
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            string name = txtName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a Table Name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (comboTableGroup.SelectedValue == null)
            {
                MessageBox.Show("Please select a Group Table.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboTableGroup.Focus();
                return;
            }

            int groupId = Convert.ToInt32(comboTableGroup.SelectedValue);

            try
            {
                string sql = @"
UPDATE dbo.DINING_TABLE 
SET TableGroupID = @GroupID, TableCode = @Code, TableName = @Name, ImagePath = @Img
WHERE TableID = @ID;";

                int rows = DbHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@GroupID", groupId),
                    new SqlParameter("@Code", code),
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Img", string.IsNullOrEmpty(_selectedImagePath) ? (object)DBNull.Value : _selectedImagePath),
                    new SqlParameter("@ID", _tableId));

                if (rows > 0)
                {
                    MessageBox.Show("Table updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    NavigateBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating table: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                TableList tableListView = new TableList { Dock = DockStyle.Fill };
                parent.Controls.Add(tableListView);
                tableListView.BringToFront();
            }
        }

        private void EditTable_Load_1(object sender, EventArgs e)
        {

        }
    }
}