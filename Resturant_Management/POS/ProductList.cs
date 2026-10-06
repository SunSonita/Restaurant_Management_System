using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Resturant_Management.Data;

namespace Resturant_Management.POS
{
    public class ProductList : UserControl
    {
        public event EventHandler<ProductEventArgs>? OnProductSelected;

        // Static event: any POSSale on screen listens to this, so no manual wiring is needed
        public static event EventHandler<ProductEventArgs>? ProductClicked;

        private Panel pnlTopBar = null!;
        public Guna2TextBox txtSearchProduct = null!;
        public FlowLayoutPanel flpCategories = null!;
        public FlowLayoutPanel flpProducts = null!;

        private List<ProductModel> masterProducts = new List<ProductModel>();
        private string activeCategory = "All Items";

        // Shared placeholder bitmap (drawn once, reused by every card)
        private static Bitmap? _noImageCache;

        public ProductList()
        {
            InitializeComponentByCode();
            LoadCategories();
            LoadProducts();
            BindFilterEvents();
        }

        private void InitializeComponentByCode()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(240, 242, 245);

            // 1. Search Bar Panel
            pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.White,
                Padding = new Padding(6)
            };

            txtSearchProduct = new Guna2TextBox
            {
                PlaceholderText = "Search product name or code...",
                Dock = DockStyle.Fill,
                BorderRadius = 15,
                FillColor = Color.FromArgb(245, 246, 248),
                Font = new Font("Segoe UI", 9F)
            };

            pnlTopBar.Controls.Add(txtSearchProduct);

            // 2. Category Flow Panel
            flpCategories = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 55,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.FromArgb(230, 235, 240),
                Padding = new Padding(6, 8, 6, 8)
            };

            // 3. Product Grid Flow Panel
            flpProducts = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(6),
                BackColor = Color.FromArgb(240, 242, 245)
            };

            // Fill control first, then the Top docked ones
            this.Controls.Add(flpProducts);
            this.Controls.Add(flpCategories);
            this.Controls.Add(pnlTopBar);
        }

        public void ReloadData()
        {
            LoadCategories();
            LoadProducts();
        }

        private void LoadCategories()
        {
            flpCategories.Controls.Clear();
            List<string> categories = new List<string> { "All Items" };

            try
            {
                string sql = "SELECT GroupName FROM dbo.ITEM_GROUP WHERE IsVisible = 1 ORDER BY GroupID";
                DataTable dt = DbHelper.ExecuteQuery(sql);
                foreach (DataRow r in dt.Rows)
                {
                    string? name = r["GroupName"]?.ToString();
                    if (!string.IsNullOrEmpty(name) && !categories.Contains(name))
                    {
                        categories.Add(name);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
                if (!categories.Contains("Drinks")) categories.Add("Drinks");
                if (!categories.Contains("Foods")) categories.Add("Foods");
            }

            foreach (var cat in categories)
            {
                var btn = new Guna2Button
                {
                    Text = cat,
                    Height = 36,
                    AutoSize = true,
                    AutoRoundedCorners = true,
                    FillColor = cat == activeCategory ? Color.FromArgb(78, 163, 129) : Color.White,
                    ForeColor = cat == activeCategory ? Color.White : Color.Black,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Margin = new Padding(3, 0, 3, 0),
                    Padding = new Padding(12, 2, 12, 2),
                    Cursor = Cursors.Hand
                };

                btn.Click += (s, e) =>
                {
                    activeCategory = cat;
                    foreach (Control c in flpCategories.Controls)
                    {
                        if (c is Guna2Button b)
                        {
                            b.FillColor = Color.White;
                            b.ForeColor = Color.Black;
                        }
                    }
                    btn.FillColor = Color.FromArgb(78, 163, 129);
                    btn.ForeColor = Color.White;

                    ApplyFilter();
                };

                flpCategories.Controls.Add(btn);
            }
        }

        private void LoadProducts()
        {
            masterProducts.Clear();
            try
            {
                string sql = @"
SELECT 
    i.ItemID,
    i.ItemCode,
    i.ItemName,
    u.UomName,
    COALESCE(ip.PriceKHR, ip.PriceUSD * 4000, 0) AS Price,
    g.GroupName AS Category,
    ISNULL(s.QtyOnHand, 0) AS StockQty,
    i.IsStockItem,
    i.ImagePath
FROM dbo.ITEM i
INNER JOIN dbo.ITEM_GROUP g ON i.GroupID = g.GroupID
INNER JOIN dbo.UOM u ON i.UomID = u.UomID
LEFT JOIN dbo.vw_ItemPrice ip ON i.ItemID = ip.ItemID
LEFT JOIN dbo.vw_ItemStock s ON i.ItemID = s.ItemID
WHERE i.IsInactive = 0
ORDER BY i.ItemID";

                DataTable dt = DbHelper.ExecuteQuery(sql);
                foreach (DataRow r in dt.Rows)
                {
                    string code = r["ItemCode"]?.ToString() ?? "";
                    string name = r["ItemName"]?.ToString() ?? "";
                    string uom = r["UomName"]?.ToString() ?? "";
                    decimal price = r["Price"] != DBNull.Value ? Convert.ToDecimal(r["Price"]) : 0m;
                    string cat = r["Category"]?.ToString() ?? "General";
                    int stock = r["StockQty"] != DBNull.Value ? Convert.ToInt32(r["StockQty"]) : 0;
                    bool isStock = r["IsStockItem"] != DBNull.Value && Convert.ToBoolean(r["IsStockItem"]);
                    string? imgPath = r["ImagePath"]?.ToString();

                    Image? img = null;
                    if (!string.IsNullOrEmpty(imgPath) && System.IO.File.Exists(imgPath))
                    {
                        try { img = Image.FromFile(imgPath); } catch { }
                    }

                    string displayName = string.IsNullOrEmpty(uom) ? name : $"{name} ({uom})";
                    masterProducts.Add(new ProductModel(code, displayName, price, cat, stock, isStock, img));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
            }

            ApplyFilter();
        }

        private void BindFilterEvents()
        {
            txtSearchProduct.TextChanged += (s, e) => ApplyFilter();
        }

        private void ApplyFilter()
        {
            // Dispose old cards to avoid leaking GDI handles
            var old = flpProducts.Controls.Cast<Control>().ToList();
            flpProducts.SuspendLayout();
            flpProducts.Controls.Clear();
            foreach (var c in old) c.Dispose();

            string keyword = txtSearchProduct.Text.Trim().ToLower();

            var filtered = masterProducts.Where(p =>
                (activeCategory == "All Items" || p.Category.Equals(activeCategory, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(keyword) || p.Name.ToLower().Contains(keyword) || p.Code.ToLower().Contains(keyword))
            );

            foreach (var prod in filtered)
            {
                RenderProductCard(prod);
            }

            flpProducts.ResumeLayout();
        }

        private void RenderProductCard(ProductModel prod)
        {
            Guna2Panel card = new Guna2Panel
            {
                Size = new Size(130, 165),
                BorderRadius = 18,
                FillColor = Color.White,
                Margin = new Padding(5),
                ShadowDecoration = { Enabled = true, Depth = 4 },
                Cursor = Cursors.Hand
            };

            // --- TOP TITLE ---
            Panel pnlTopTitle = new Panel
            {
                Size = new Size(130, 42),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(172, 163, 128),
                Padding = new Padding(4, 2, 4, 2)
            };

            Label lblName = new Label
            {
                Text = prod.Name,
                Font = DbHelper.GetKhmerFont(8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 20),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = false
            };
            pnlTopTitle.Controls.Add(lblName);

            // --- IMAGE / MIDDLE AREA ---
            PictureBox pbImage = new PictureBox
            {
                Size = new Size(130, 95),
                Location = new Point(0, 42),
                BackColor = Color.FromArgb(235, 235, 235),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = prod.Image ?? GetNoImageBitmap()
            };

            // STOCK strip
            if (prod.IsStockProduct)
            {
                Label lblStock = new Label
                {
                    Text = prod.StockQty > 0 ? $"Stock: {prod.StockQty}" : "Out of stock",
                    Font = new Font("Segoe UI", 7.5F, FontStyle.Regular),
                    ForeColor = prod.StockQty > 0 ? Color.FromArgb(46, 125, 50) : Color.Red,
                    BackColor = Color.White,
                    Size = new Size(130, 16),
                    Location = new Point(0, 0),
                    TextAlign = ContentAlignment.MiddleRight,
                    Padding = new Padding(0, 0, 4, 0)
                };
                pbImage.Controls.Add(lblStock);
            }

            // PRICE BADGE (N0 + wider label so the full price is visible)
            Label lblPrice = new Label
            {
                Text = $"KHR {prod.Price:N0}",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(68, 158, 122),
                Size = new Size(100, 20),
                Location = new Point(30, 75),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pbImage.Controls.Add(lblPrice);

            if (IsOutOfStock(prod))
            {
                lblPrice.BackColor = Color.Gray;
            }

            // --- BOTTOM FOOTER BANNER ---
            Panel pnlBottom = new Panel
            {
                Size = new Size(130, 28),
                Location = new Point(0, 137),
                BackColor = IsOutOfStock(prod) ? Color.Gray : Color.FromArgb(128, 98, 108)
            };

            Label lblCode = new Label
            {
                Text = prod.Code,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(4, 4),
                AutoSize = true
            };

            Label lblCalcIcon = new Label
            {
                Text = "🧮",
                Font = new Font("Segoe UI Emoji", 9F, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(106, 3),
                AutoSize = true
            };

            pnlBottom.Controls.Add(lblCode);
            pnlBottom.Controls.Add(lblCalcIcon);

            // Add all panels to the main card (ONCE)
            card.Controls.Add(pnlTopTitle);
            card.Controls.Add(pbImage);
            card.Controls.Add(pnlBottom);

            // Bind click events AFTER all children are added (ONCE)
            BindClickEvent(card, prod);

            flpProducts.Controls.Add(card);
        }

        private static bool IsOutOfStock(ProductModel prod) => prod.IsStockProduct && prod.StockQty <= 0;

        private void BindClickEvent(Control ctrl, ProductModel prod)
        {
            ctrl.Cursor = IsOutOfStock(prod) ? Cursors.No : Cursors.Hand;
            ctrl.Click += (s, e) =>
            {
                if (IsOutOfStock(prod))
                {
                    MessageBox.Show($"\"{prod.Name}\" is out of stock and cannot be ordered.",
                                    "Out of Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var args = new ProductEventArgs(prod.Code, prod.Name, prod.Price);
                OnProductSelected?.Invoke(this, args);
                ProductClicked?.Invoke(this, args);
            };

            // Recursively attach to all inner child controls (Labels, PictureBoxes, Panels)
            foreach (Control child in ctrl.Controls)
            {
                BindClickEvent(child, prod);
            }
        }

        private static Bitmap GetNoImageBitmap()
        {
            if (_noImageCache != null) return _noImageCache;

            int w = 130, h = 95;
            var bmp = new Bitmap(w, h);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.Clear(Color.FromArgb(235, 235, 235));

                var gray = Color.FromArgb(165, 165, 165);
                using var grayBrush = new SolidBrush(gray);
                using var lightBrush = new SolidBrush(Color.FromArgb(232, 232, 232));

                // Tilted "picture" icon
                float s = 46;
                g.TranslateTransform(w / 2f, 12 + s / 2f);
                g.RotateTransform(-12);

                var frame = new RectangleF(-s / 2, -s / 2, s, s);
                using (var path = RoundedRect(frame, 7))
                    g.FillPath(grayBrush, path);

                var inner = new RectangleF(frame.X + 5, frame.Y + 5, s - 10, s - 10);
                g.FillRectangle(lightBrush, inner);

                // Sun
                g.FillEllipse(grayBrush, inner.X + 6, inner.Y + 5, 8, 8);

                // Mountains
                g.FillPolygon(grayBrush, new[]
                {
                    new PointF(inner.X,       inner.Bottom),
                    new PointF(inner.X + 12,  inner.Y + 20),
                    new PointF(inner.X + 20,  inner.Y + 28),
                    new PointF(inner.X + 28,  inner.Y + 16),
                    new PointF(inner.Right,   inner.Bottom)
                });
                g.ResetTransform();

                // Text under the icon
                using var f = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                using var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("NO IMAGE", f, grayBrush, new RectangleF(0, 58, w, 16), sf);
                g.DrawString("AVAILABLE", f, grayBrush, new RectangleF(0, 72, w, 16), sf);
            }

            _noImageCache = bmp;
            return bmp;
        }

        private static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            float d = radius * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    public class ProductModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public int StockQty { get; set; }
        public bool IsStockProduct { get; set; }
        public Image? Image { get; set; }

        public ProductModel(string code, string name, decimal price, string category, int stockQty = 0, bool isStockProduct = false, Image? image = null)
        {
            Code = code;
            Name = name;
            Price = price;
            Category = category;
            StockQty = stockQty;
            IsStockProduct = isStockProduct;
            Image = image;
        }
    }

    public class ProductEventArgs : EventArgs
    {
        public string Code { get; }
        public string Name { get; }
        public decimal Price { get; }

        public ProductEventArgs(string code, string name, decimal price)
        {
            Code = code;
            Name = name;
            Price = price;
        }
    }
}
