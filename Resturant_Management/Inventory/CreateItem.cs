using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Inventory
{
    public partial class CreateItem : UserControl
    {
        private string _selectedImagePath = "";

        public CreateItem()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.Margin = new Padding(0);
            this.Padding = new Padding(0);

            btnSave.Click += BtnSave_Click;
            btnBack.Click += BtnBack_Click;
            PicItem.Click += BtnBrowser_Click;
            this.Load += CreateItem_Load;
        }

        private void CreateItem_Load(object? sender, EventArgs e)
        {
            this.BringToFront();
            LoadDropdowns();
        }

        private void LoadDropdowns()
        {
            try
            {
                // Item Groups
                DataTable dtGroups = DbHelper.ExecuteQuery("SELECT GroupID, GroupName FROM dbo.ITEM_GROUP ORDER BY GroupName");
                comboItemGroup.DataSource = dtGroups;
                comboItemGroup.DisplayMember = "GroupName";
                comboItemGroup.ValueMember = "GroupID";

                // UOM
                DataTable dtUom = DbHelper.ExecuteQuery("SELECT UomID, UomName FROM dbo.UOM ORDER BY UomName");
                comboUom.DataSource = dtUom;
                comboUom.DisplayMember = "UomName";
                comboUom.ValueMember = "UomID";

                // Printers
                DataTable dtPrinters1 = DbHelper.ExecuteQuery("SELECT PrinterID, PrinterName FROM dbo.PRINTER ORDER BY PrinterName");
                comboPrinter1.DataSource = dtPrinters1;
                comboPrinter1.DisplayMember = "PrinterName";
                comboPrinter1.ValueMember = "PrinterID";

                DataTable dtPrinters2 = DbHelper.ExecuteQuery("SELECT PrinterID, PrinterName FROM dbo.PRINTER ORDER BY PrinterName");
                comboPrinter2.DataSource = dtPrinters2;
                comboPrinter2.DisplayMember = "PrinterName";
                comboPrinter2.ValueMember = "PrinterID";

                // Valuation Methods
                comboValautionMethod.Items.Clear();
                comboValautionMethod.Items.AddRange(new object[] { "Standard", "FIFO", "MovingAverage" });
                comboValautionMethod.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dropdowns: {ex.Message}");
            }
        }

        private void BtnBrowser_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _selectedImagePath = ofd.FileName;
                    PicItem.Image = Image.FromFile(_selectedImagePath);
                }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            string name = txtName.Text.Trim();
            string name2 = txtName2.Text.Trim();

            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Please enter an Item Code.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCode.Focus();
                return;
            }

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter an Item Name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            decimal.TryParse(txtUnitPrice.Text.Trim(), out decimal price);
            decimal.TryParse(txtStock.Text.Trim(), out decimal stock);

            int groupId = comboItemGroup.SelectedValue != null ? Convert.ToInt32(comboItemGroup.SelectedValue) : 1;
            int uomId = comboUom.SelectedValue != null ? Convert.ToInt32(comboUom.SelectedValue) : 1;
            int? printer1Id = comboPrinter1.SelectedValue != null ? Convert.ToInt32(comboPrinter1.SelectedValue) : null;
            int? printer2Id = comboPrinter2.SelectedValue != null ? Convert.ToInt32(comboPrinter2.SelectedValue) : null;
            string valuationMethod = comboValautionMethod.SelectedItem?.ToString() ?? "Standard";
            bool isInactive = chkInactive.Checked;
            bool isStockItem = stock > 0;

            try
            {
                // Check if code exists
                object? exists = DbHelper.ExecuteScalar("SELECT COUNT(1) FROM dbo.ITEM WHERE ItemCode = @Code", new SqlParameter("@Code", code));
                if (exists != null && Convert.ToInt32(exists) > 0)
                {
                    MessageBox.Show($"Item code '{code}' already exists.", "Duplicate Code", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string insertSql = @"
INSERT INTO dbo.ITEM 
(ItemCode, ItemName, ItemName2, GroupID, UomID, ImagePath, IsStockItem, ValuationMethod, PrinterID, Printer2ID, IsInactive, CreatedAt)
VALUES 
(@Code, @Name, @Name2, @GroupID, @UomID, @ImagePath, @IsStockItem, @ValuationMethod, @PrinterID, @Printer2ID, @IsInactive, SYSDATETIME());
SELECT SCOPE_IDENTITY();";

                using var conn = DbHelper.GetConnection();
                conn.Open();
                using var trans = conn.BeginTransaction();

                int newItemId;
                using (var cmd = new SqlCommand(insertSql, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@Code", code);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Name2", string.IsNullOrEmpty(name2) ? (object)DBNull.Value : name2);
                    cmd.Parameters.AddWithValue("@GroupID", groupId);
                    cmd.Parameters.AddWithValue("@UomID", uomId);
                    cmd.Parameters.AddWithValue("@ImagePath", string.IsNullOrEmpty(_selectedImagePath) ? (object)DBNull.Value : _selectedImagePath);
                    cmd.Parameters.AddWithValue("@IsStockItem", isStockItem);
                    cmd.Parameters.AddWithValue("@ValuationMethod", valuationMethod);
                    cmd.Parameters.AddWithValue("@PrinterID", (object?)printer1Id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Printer2ID", (object?)printer2Id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsInactive", isInactive);

                    newItemId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Insert Price in KHR; Trigger TR_ITEM_PRICE_AutoConvert automatically converts & inserts USD price
                string priceSql = @"
INSERT INTO dbo.ITEM_PRICE (ItemID, CurrencyCode, Price)
VALUES (@ItemID, 'KHR', @Price);";
                using (var cmdPrice = new SqlCommand(priceSql, conn, trans))
                {
                    cmdPrice.Parameters.AddWithValue("@ItemID", newItemId);
                    cmdPrice.Parameters.AddWithValue("@Price", price);
                    cmdPrice.ExecuteNonQuery();
                }

                if (stock > 0)
                {
                    string stockSql = @"
INSERT INTO dbo.STOCK_MOVEMENT (ItemID, MovementType, Qty, UnitCost, Remark, CreatedBy, CreatedAt)
VALUES (@ItemID, 'Opening', @Qty, @Cost, 'Initial Stock', @User, SYSDATETIME());";
                    using var cmdStock = new SqlCommand(stockSql, conn, trans);
                    cmdStock.Parameters.AddWithValue("@ItemID", newItemId);
                    cmdStock.Parameters.AddWithValue("@Qty", stock);
                    cmdStock.Parameters.AddWithValue("@Cost", price);
                    cmdStock.Parameters.AddWithValue("@User", UserSession.UserID > 0 ? UserSession.UserID : 1);
                    cmdStock.ExecuteNonQuery();
                }

                trans.Commit();

                MessageBox.Show("Item created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NavigateBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving item: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBack_Click(object? sender, EventArgs e)
        {
            NavigateBack();
        }

        private void NavigateBack()
        {
            Control? parentContainer = this.Parent;
            if (parentContainer != null)
            {
                parentContainer.Controls.Clear();
                Itemlist itemlistScreen = new Itemlist { Dock = DockStyle.Fill };
                parentContainer.Controls.Add(itemlistScreen);
                itemlistScreen.BringToFront();
            }
        }

        private void btnBack_Click(object sender, EventArgs e) => NavigateBack();
        private void btnbrowser_Click(object? sender, EventArgs e) => BtnBrowser_Click(sender, e);
        private void guna2Pan_Paint(object? sender, PaintEventArgs e) { }
        private void guna2Pan_Paint_1(object? sender, PaintEventArgs e) { }
        private void pnlMainContent_Paint(object? sender, PaintEventArgs e) { }
    }
}
