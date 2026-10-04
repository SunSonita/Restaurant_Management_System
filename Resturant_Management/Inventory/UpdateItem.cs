using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Inventory
{
    public partial class UpdateItem : UserControl
    {
        private string _itemCode = "";
        private string _selectedImagePath = "";

        public UpdateItem() : this("")
        {
        }

        public UpdateItem(string itemCode)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            _itemCode = itemCode;

            btnUpdate.Click += BtnUpdate_Click;
            PicItem.Click += BtnBrowser_Click;
            btnBack.Click += BtnBack_Click;
            this.Load += UpdateItem_Load;
        }

        private void UpdateItem_Load(object? sender, EventArgs e)
        {
            this.BringToFront();
            LoadDropdowns();
            if (!string.IsNullOrEmpty(_itemCode))
            {
                LoadItemData(_itemCode);
            }
        }

        private void LoadDropdowns()
        {
            try
            {
                DataTable dtGroups = DbHelper.ExecuteQuery("SELECT GroupID, GroupName FROM dbo.ITEM_GROUP ORDER BY GroupName");
                comboItemGroup.DataSource = dtGroups;
                comboItemGroup.DisplayMember = "GroupName";
                comboItemGroup.ValueMember = "GroupID";

                DataTable dtUom = DbHelper.ExecuteQuery("SELECT UomID, UomName FROM dbo.UOM ORDER BY UomName");
                comboUom.DataSource = dtUom;
                comboUom.DisplayMember = "UomName";
                comboUom.ValueMember = "UomID";

                DataTable dtPrinters1 = DbHelper.ExecuteQuery("SELECT PrinterID, PrinterName FROM dbo.PRINTER ORDER BY PrinterName");
                comboPrinter1.DataSource = dtPrinters1;
                comboPrinter1.DisplayMember = "PrinterName";
                comboPrinter1.ValueMember = "PrinterID";

                DataTable dtPrinters2 = DbHelper.ExecuteQuery("SELECT PrinterID, PrinterName FROM dbo.PRINTER ORDER BY PrinterName");
                comboPrinter2.DataSource = dtPrinters2;
                comboPrinter2.DisplayMember = "PrinterName";
                comboPrinter2.ValueMember = "PrinterID";

                comboValautionMethod.Items.Clear();
                comboValautionMethod.Items.AddRange(new object[] { "Standard", "FIFO", "MovingAverage" });
                comboValautionMethod.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dropdowns: {ex.Message}");
            }
        }

        private void LoadItemData(string code)
        {
            try
            {
                string sql = @"
SELECT 
    i.ItemID, i.ItemCode, i.ItemName, i.ItemName2, i.GroupID, i.UomID,
    COALESCE(ip.PriceKHR, ip.PriceUSD * 4000, 0) AS Price,
    i.ImagePath, i.IsStockItem, i.ValuationMethod, i.PrinterID, i.Printer2ID, i.IsInactive,
    ISNULL(s.QtyOnHand, 0) AS StockQty
FROM dbo.ITEM i
LEFT JOIN dbo.vw_ItemPrice ip ON i.ItemID = ip.ItemID
LEFT JOIN dbo.vw_ItemStock s ON i.ItemID = s.ItemID
WHERE i.ItemCode = @Code;";

                DataTable dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@Code", code));
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtCode.Text = r["ItemCode"]?.ToString() ?? "";
                    txtCode.ReadOnly = true;
                    txtName.Text = r["ItemName"]?.ToString() ?? "";
                    txtName2.Text = r["ItemName2"]?.ToString() ?? "";
                    txtUnitPrice.Text = r["Price"] != DBNull.Value ? Convert.ToDecimal(r["Price"]).ToString("N2") : "0.00";
                    txtStock.Text = r["StockQty"] != DBNull.Value ? Convert.ToDecimal(r["StockQty"]).ToString("N0") : "0";

                    if (r["GroupID"] != DBNull.Value) comboItemGroup.SelectedValue = Convert.ToInt32(r["GroupID"]);
                    if (r["UomID"] != DBNull.Value) comboUom.SelectedValue = Convert.ToInt32(r["UomID"]);
                    if (r["PrinterID"] != DBNull.Value) comboPrinter1.SelectedValue = Convert.ToInt32(r["PrinterID"]);
                    if (r["Printer2ID"] != DBNull.Value) comboPrinter2.SelectedValue = Convert.ToInt32(r["Printer2ID"]);

                    string vm = r["ValuationMethod"]?.ToString() ?? "Standard";
                    int idx = comboValautionMethod.Items.IndexOf(vm);
                    if (idx >= 0) comboValautionMethod.SelectedIndex = idx;

                    chkInactive.Checked = r["IsInactive"] != DBNull.Value && Convert.ToBoolean(r["IsInactive"]);

                    string? imgPath = r["ImagePath"]?.ToString();
                    if (!string.IsNullOrEmpty(imgPath) && System.IO.File.Exists(imgPath))
                    {
                        try
                        {
                            _selectedImagePath = imgPath;
                            PicItem.Image = Image.FromFile(imgPath);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading item details: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            string name = txtName.Text.Trim();
            string name2 = txtName2.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter an Item Name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            decimal.TryParse(txtUnitPrice.Text.Trim(), out decimal price);
            int groupId = comboItemGroup.SelectedValue != null ? Convert.ToInt32(comboItemGroup.SelectedValue) : 1;
            int uomId = comboUom.SelectedValue != null ? Convert.ToInt32(comboUom.SelectedValue) : 1;
            int? printer1Id = comboPrinter1.SelectedValue != null ? Convert.ToInt32(comboPrinter1.SelectedValue) : null;
            int? printer2Id = comboPrinter2.SelectedValue != null ? Convert.ToInt32(comboPrinter2.SelectedValue) : null;
            string valuationMethod = comboValautionMethod.SelectedItem?.ToString() ?? "Standard";
            bool isInactive = chkInactive.Checked;

            try
            {
                string updateSql = @"
UPDATE dbo.ITEM SET 
    ItemName = @Name,
    ItemName2 = @Name2,
    GroupID = @GroupID,
    UomID = @UomID,
    ValuationMethod = @ValuationMethod,
    PrinterID = @PrinterID,
    Printer2ID = @Printer2ID,
    IsInactive = @IsInactive,
    ImagePath = CASE WHEN @ImagePath IS NOT NULL THEN @ImagePath ELSE ImagePath END
WHERE ItemCode = @Code;

DECLARE @ItemID int = (SELECT ItemID FROM dbo.ITEM WHERE ItemCode = @Code);

IF EXISTS (SELECT 1 FROM dbo.ITEM_PRICE WHERE ItemID = @ItemID AND CurrencyCode = 'KHR')
    UPDATE dbo.ITEM_PRICE SET Price = @Price WHERE ItemID = @ItemID AND CurrencyCode = 'KHR';
ELSE
    INSERT INTO dbo.ITEM_PRICE (ItemID, CurrencyCode, Price) VALUES (@ItemID, 'KHR', @Price);
";

                int rows = DbHelper.ExecuteNonQuery(updateSql,
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Name2", string.IsNullOrEmpty(name2) ? (object)DBNull.Value : name2),
                    new SqlParameter("@GroupID", groupId),
                    new SqlParameter("@UomID", uomId),
                    new SqlParameter("@Price", price),
                    new SqlParameter("@ValuationMethod", valuationMethod),
                    new SqlParameter("@PrinterID", (object?)printer1Id ?? DBNull.Value),
                    new SqlParameter("@Printer2ID", (object?)printer2Id ?? DBNull.Value),
                    new SqlParameter("@IsInactive", isInactive),
                    new SqlParameter("@ImagePath", string.IsNullOrEmpty(_selectedImagePath) ? (object)DBNull.Value : _selectedImagePath),
                    new SqlParameter("@Code", code)
                );

                if (rows > 0)
                {
                    MessageBox.Show("Item updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    NavigateBack();
                }
                else
                {
                    MessageBox.Show("No item was updated.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating item: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
