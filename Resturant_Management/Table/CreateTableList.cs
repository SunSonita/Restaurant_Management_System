using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Table
{
    public partial class CreateTableList : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private readonly CheckBox chkTablePriceList = new CheckBox();
        private readonly ComboBox comboPriceList = new ComboBox();
        private readonly ComboBox comboType = new ComboBox();

        public CreateTableList()
        {
            InitializeComponent();
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            BuildModernLayout();

            btnSave.Click += BtnSave_Click;
            label1.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Load += CreateTableList_Load;
        }

        private void BuildModernLayout()
        {
            this.SuspendLayout();
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(760, 390);
            this.BackColor = Color.White;

            this.Controls.Clear();

            // 1. Top Gray Header Bar with Draggable support and ✕ Close button
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(241, 245, 249)
            };
            pnlTop.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            label1.Text = "✕";
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(71, 85, 105);
            label1.AutoSize = true;
            label1.Location = new Point(pnlTop.Width - 28, 6);
            label1.Cursor = Cursors.Hand;
            pnlTop.Resize += (s, e) => { label1.Location = new Point(pnlTop.Width - 28, 6); };
            pnlTop.Controls.Add(label1);

            // 2. Title "Create Table List"
            Label lblTitle = new Label
            {
                Text = "Create Table List",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Location = new Point(36, 48)
            };

            // 3. Configure Input Controls
            // txtPrefixName
            txtPrefixName.BorderThickness = 0;
            txtPrefixName.Font = new Font("Segoe UI", 9.5F);
            txtPrefixName.ForeColor = Color.FromArgb(30, 41, 59);

            // txtNumberfrom
            txtNumberfrom.BorderThickness = 0;
            txtNumberfrom.Font = new Font("Segoe UI", 9.5F);
            txtNumberfrom.ForeColor = Color.FromArgb(30, 41, 59);

            // txtNumberTo
            txtNumberTo.BorderThickness = 0;
            txtNumberTo.Font = new Font("Segoe UI", 9.5F);
            txtNumberTo.ForeColor = Color.FromArgb(30, 41, 59);

            // comboTableGroup
            comboTableGroup.DropDownStyle = ComboBoxStyle.DropDownList;
            comboTableGroup.Font = new Font("Segoe UI", 9.5F);
            comboTableGroup.BorderThickness = 0;
            comboTableGroup.ForeColor = Color.FromArgb(30, 41, 59);

            // comboPriceList
            comboPriceList.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPriceList.Font = new Font("Segoe UI", 9.5F);
            comboPriceList.FlatStyle = FlatStyle.Flat;
            comboPriceList.ForeColor = Color.FromArgb(30, 41, 59);
            comboPriceList.Items.AddRange(new object[] { "Standard Dine-in", "VIP Menu", "Happy Hour" });
            comboPriceList.SelectedIndex = 0;

            // comboType
            comboType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboType.Font = new Font("Segoe UI", 9.5F);
            comboType.FlatStyle = FlatStyle.Flat;
            comboType.ForeColor = Color.FromArgb(30, 41, 59);
            comboType.Items.AddRange(new object[] { "Normal", "VIP", "Outdoor", "Delivery", "Take Out" });
            comboType.SelectedIndex = 0;

            // Checkbox
            chkTablePriceList.Text = "Table Price List";
            chkTablePriceList.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            chkTablePriceList.ForeColor = Color.FromArgb(51, 65, 85);
            chkTablePriceList.AutoSize = true;
            chkTablePriceList.Location = new Point(36, 256);
            chkTablePriceList.Cursor = Cursors.Hand;

            // Outlined Field Panels matching second image reference
            Panel fldPrefix = CreateOutlinedField("Prefix Name", txtPrefixName, 36, 82, 330, 42);
            Panel fldFrom = CreateOutlinedField("Number From", txtNumberfrom, 36, 138, 330, 42);
            Panel fldTo = CreateOutlinedField("Number To", txtNumberTo, 36, 194, 330, 42, isFocused: true, isSpinner: true);

            Panel fldGroup = CreateOutlinedField("Group Table", comboTableGroup, 394, 82, 330, 42);
            Panel fldPrice = CreateOutlinedField("Price List :", comboPriceList, 394, 138, 330, 42);
            Panel fldType = CreateOutlinedField("Type :", comboType, 394, 194, 330, 42);

            // 4. Save Button
            btnSave.Text = "Save";
            btnSave.Size = new Size(80, 34);
            btnSave.Location = new Point(this.Width - 116, 330);
            btnSave.BorderRadius = 4;
            btnSave.FillColor = Color.FromArgb(26, 117, 210);
            btnSave.HoverState.FillColor = Color.FromArgb(21, 101, 192);
            btnSave.ForeColor = Color.White;
            btnSave.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSave.Cursor = Cursors.Hand;
            btnSave.ShadowDecoration.Enabled = false;

            // Add all controls
            this.Controls.Add(pnlTop);
            this.Controls.Add(lblTitle);
            this.Controls.Add(fldPrefix);
            this.Controls.Add(fldFrom);
            this.Controls.Add(fldTo);
            this.Controls.Add(chkTablePriceList);
            this.Controls.Add(fldGroup);
            this.Controls.Add(fldPrice);
            this.Controls.Add(fldType);
            this.Controls.Add(btnSave);

            // 5. Draw 1px subtle outer border around frameless modal dialog
            this.Paint += (s, e) =>
            {
                using (Pen p = new Pen(Color.FromArgb(203, 213, 225), 1f))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, this.Width - 1, this.Height - 1);
                }
            };

            this.ResumeLayout(true);
        }

        private Panel CreateOutlinedField(string labelText, Control inputControl, int x, int y, int width, int height, bool isFocused = false, bool isSpinner = false)
        {
            Panel pnl = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White
            };

            Color borderColor = isFocused ? Color.FromArgb(26, 117, 210) : Color.FromArgb(209, 213, 219);

            Label lbl = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = isFocused ? Color.FromArgb(26, 117, 210) : Color.FromArgb(100, 116, 139),
                BackColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 0)
            };

            pnl.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(borderColor, 1.2f))
                {
                    Rectangle rect = new Rectangle(0, 6, width - 1, height - 7);
                    using (GraphicsPath path = GetRoundedPath(rect, 4))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }

                if (isSpinner)
                {
                    using (SolidBrush arrowBrush = new SolidBrush(Color.FromArgb(100, 116, 139)))
                    {
                        // Up arrow
                        Point[] up = { new Point(width - 20, 16), new Point(width - 14, 16), new Point(width - 17, 12) };
                        e.Graphics.FillPolygon(arrowBrush, up);
                        // Down arrow
                        Point[] down = { new Point(width - 20, 24), new Point(width - 14, 24), new Point(width - 17, 28) };
                        e.Graphics.FillPolygon(arrowBrush, down);
                    }
                }
            };

            inputControl.Location = new Point(12, 13);
            inputControl.Width = isSpinner ? width - 38 : width - 24;
            inputControl.Height = 22;

            pnl.Controls.Add(lbl);
            pnl.Controls.Add(inputControl);
            lbl.BringToFront();

            return pnl;
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

        private void CreateTableList_Load(object? sender, EventArgs e)
        {
            if (DesignTimeHelper.IsInDesignMode(this))
                return;

            try
            {
                DataTable dt = DbHelper.ExecuteQuery("SELECT TableGroupID, GroupName FROM dbo.TABLE_GROUP ORDER BY GroupName");
                comboTableGroup.DataSource = dt;
                comboTableGroup.DisplayMember = "GroupName";
                comboTableGroup.ValueMember = "TableGroupID";

                txtPrefixName.Text = "Table";
                txtNumberfrom.Text = "11";
                txtNumberTo.Text = "20";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading groups: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            int groupId = comboTableGroup.SelectedValue != null ? Convert.ToInt32(comboTableGroup.SelectedValue) : 1;

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
    INSERT INTO dbo.DINING_TABLE (TableGroupID, TableCode, TableName, Capacity, Status, IsActive)
    VALUES (@GroupID, @Code, @Name, 4, 'Available', 1);
END";
                    using var cmd = new SqlCommand(insertSql, conn, trans);
                    cmd.Parameters.AddWithValue("@GroupID", groupId);
                    cmd.Parameters.AddWithValue("@Code", code);
                    cmd.Parameters.AddWithValue("@Name", name);
                    int r = cmd.ExecuteNonQuery();
                    if (r > 0) createdCount++;
                }

                trans.Commit();

                MessageBox.Show($"Successfully created {createdCount} table(s).", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating tables: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
