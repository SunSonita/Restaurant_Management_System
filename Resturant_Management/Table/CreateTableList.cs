using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Table
{
    public partial class CreateTableList : UserControl
    {
        private string? _imagePath;
        private bool _configured = false;

        private static readonly Color PlaceholderColor = Color.FromArgb(170, 180, 196);
        private static readonly Color LinkBlue = Color.FromArgb(26, 115, 232);

        public CreateTableList()
        {
            InitializeComponent();
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            this.Load += CreateTableList_Load;
            this.Resize += (s, e) => ApplyLayout();

            btnSave.Click += BtnSave_Click;

            PicItem.Cursor = Cursors.Hand;
            label3.Cursor = Cursors.Hand;
            PicItem.Click += ChooseImage_Click;
            label3.Click += ChooseImage_Click;
        }

        // Scale a 96-DPI pixel value to the current monitor DPI
        private int S(int v) => (int)Math.Round(v * DeviceDpi / 96.0);

        // ------------------------------------------------------------------
        // Look & feel (run once, after the handle exists so DPI is correct)
        // ------------------------------------------------------------------
        private void ConfigureControls()
        {
            if (_configured) return;
            _configured = true;

            int fieldH = S(46);

            this.BackColor = Color.White;

            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 16F, FontStyle.Bold);

            // Text inputs
            foreach (var t in new[] { txtPrefixName, txtNumberfrom, txtNumberTo })
            {
                t.Font = new Font("Segoe UI", 10F);
                t.BorderRadius = 5;
                t.Size = new Size(t.Width, fieldH);
            }

            // Combos (blank by default, same height as the text inputs).
            // For an owner-drawn Guna combo: control height = ItemHeight + 6
            foreach (var c in new[] { cmbTableGroup, comboTableGroup })
            {
                c.Font = new Font("Segoe UI", 10F);
                c.BorderRadius = 5;
                c.ItemHeight = fieldH - 6;
                c.Height = fieldH;
            }

            // The old "Group Table" / "Type" labels become in-field placeholders
            SetupPlaceholder(label4, cmbTableGroup, "Group Table");
            SetupPlaceholder(lbTableGroup, comboTableGroup, "Type");

            // Image box + link, same style as the reference
            PicItem.BorderRadius = 0;
            PicItem.BorderStyle = BorderStyle.FixedSingle;
            PicItem.BackColor = Color.White;
            PicItem.SizeMode = PictureBoxSizeMode.Zoom;

            label3.AutoSize = false;
            label3.AutoEllipsis = true;
            label3.TextAlign = ContentAlignment.MiddleCenter;
            label3.Font = new Font("Segoe UI", 10.5F, FontStyle.Underline);
            label3.ForeColor = LinkBlue;
            label3.Text = "Click to choose image";
        }

        private void SetupPlaceholder(Label lbl, Guna.UI2.WinForms.Guna2ComboBox cbo, string text)
        {
            lbl.AutoSize = false;
            lbl.Text = text;
            lbl.BackColor = Color.White;
            lbl.ForeColor = PlaceholderColor;
            lbl.Font = new Font("Segoe UI", 10F);
            lbl.TextAlign = ContentAlignment.MiddleLeft;
            lbl.Cursor = Cursors.Hand;

            lbl.Click += (s, e) =>
            {
                cbo.Focus();
                cbo.DroppedDown = true;
            };
            cbo.SelectedIndexChanged += (s, e) => UpdatePlaceholders();
        }

        private void UpdatePlaceholders()
        {
            label4.Visible = cmbTableGroup.SelectedIndex < 0;
            lbTableGroup.Visible = comboTableGroup.SelectedIndex < 0;
        }

        // ------------------------------------------------------------------
        // Responsive layout: equal left/right margin, equal-size fields
        // [ prefix / from / to ]  [ group / type ]  [ image ]
        // ------------------------------------------------------------------
        private void ApplyLayout()
        {
            if (!_configured || ClientSize.Width <= 0) return;

            SuspendLayout();

            int m = S(24);          // page margin (left == right)
            int pad = S(24);        // card inner padding
            int fieldH = S(46);
            int rowGap = S(16);
            int colGap = S(24);
            int picW = S(220);
            int linkH = S(26);
            int linkGap = S(10);

            // Title
            label2.Location = new Point(m, S(20));

            // Card
            int innerH = fieldH * 3 + rowGap * 2;
            int cardW = ClientSize.Width - m * 2;
            int cardTop = label2.Bottom + S(14);
            panelInformationItem.SetBounds(m, cardTop, cardW, innerH + pad * 2);

            // Columns
            int fieldW = Math.Max(S(120), (cardW - pad * 2 - picW - colGap * 2) / 2);
            int x1 = pad;
            int x2 = x1 + fieldW + colGap;
            int x3 = cardW - pad - picW;

            int y0 = pad;
            int y1 = y0 + fieldH + rowGap;
            int y2 = y1 + fieldH + rowGap;

            // Column 1
            txtPrefixName.SetBounds(x1, y0, fieldW, fieldH);
            txtNumberfrom.SetBounds(x1, y1, fieldW, fieldH);
            txtNumberTo.SetBounds(x1, y2, fieldW, fieldH);

            // Column 2
            cmbTableGroup.SetBounds(x2, y0, fieldW, fieldH);
            comboTableGroup.SetBounds(x2, y1, fieldW, fieldH);

            // Placeholders sit inside the combos (leave the border and arrow free)
            label4.SetBounds(cmbTableGroup.Left + S(10), cmbTableGroup.Top + S(6),
                             cmbTableGroup.Width - S(48), cmbTableGroup.Height - S(12));
            lbTableGroup.SetBounds(comboTableGroup.Left + S(10), comboTableGroup.Top + S(6),
                                   comboTableGroup.Width - S(48), comboTableGroup.Height - S(12));
            label4.BringToFront();
            lbTableGroup.BringToFront();

            // Column 3: image + link
            int picH = innerH - linkH - linkGap;
            PicItem.SetBounds(x3, y0, picW, picH);
            label3.SetBounds(x3, y0 + picH + linkGap, picW, linkH);

            // Save button, aligned with the left margin
            btnSave.Size = new Size(S(108), S(44));
            btnSave.Location = new Point(m, panelInformationItem.Bottom + S(20));

            ResumeLayout(true);
        }


        // NOTE: in the designer, cmbTableGroup = "Group Table" combo, comboTableGroup = "Type" combo
        private void CreateTableList_Load(object? sender, EventArgs e)
        {
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            ConfigureControls();

            try
            {
                DataTable dt = DbHelper.ExecuteQuery("SELECT TableGroupID, GroupName FROM dbo.TABLE_GROUP ORDER BY GroupName");
                cmbTableGroup.Items.Clear();
                cmbTableGroup.DataSource = dt;
                cmbTableGroup.DisplayMember = "GroupName";
                cmbTableGroup.ValueMember = "TableGroupID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading groups: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Start blank: the user clicks to open the dropdown and choose
            cmbTableGroup.SelectedIndex = -1;
            comboTableGroup.SelectedIndex = -1;
            UpdatePlaceholders();

            ApplyLayout();
        }

        private void ChooseImage_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp;*.bmp";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                // Load via a copy so the file is not locked
                using Image src = Image.FromFile(ofd.FileName);
                PicItem.Image?.Dispose();
                PicItem.Image = new Bitmap(src);
                _imagePath = ofd.FileName;
                label3.Text = Path.GetFileName(ofd.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load image: {ex.Message}", "Image Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string prefix = txtPrefixName.Text.Trim();
            if (string.IsNullOrEmpty(prefix)) prefix = "Table";

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

            if (cmbTableGroup.SelectedValue == null)
            {
                MessageBox.Show("Please select a group table.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int groupId = Convert.ToInt32(cmbTableGroup.SelectedValue);

            int createdCount = 0;
            try
            {
                using var conn = DbHelper.GetConnection();
                conn.Open();
                using var trans = conn.BeginTransaction();

                for (int i = fromNum; i <= toNum; i++)
                {
                    string code = $"T{i}";
                    string name = prefix.EndsWith("-") || prefix.EndsWith(" ") ? $"{prefix}{i}" : $"{prefix}-{i}";

                    string insertSql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.DINING_TABLE WHERE TableCode = @Code)
BEGIN
    INSERT INTO dbo.DINING_TABLE (TableGroupID, TableCode, TableName, Capacity, Status, IsActive, ImagePath)
    VALUES (@GroupID, @Code, @Name, 4, 'Available', 1, @Img);
END";
                    using var cmd = new SqlCommand(insertSql, conn, trans);
                    cmd.Parameters.AddWithValue("@GroupID", groupId);
                    cmd.Parameters.AddWithValue("@Code", code);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Img", (object?)_imagePath ?? DBNull.Value);
                    int r = cmd.ExecuteNonQuery();
                    if (r > 0) createdCount++;
                }

                trans.Commit();

                MessageBox.Show($"Successfully created {createdCount} table(s).", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GoBackToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating tables: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GoBackToList()
        {
            Control? parent = this.Parent;
            if (parent == null) return;

            parent.Controls.Clear();
            TableList list = new TableList { Dock = DockStyle.Fill };
            parent.Controls.Add(list);
            list.BringToFront();
        }
    }
}