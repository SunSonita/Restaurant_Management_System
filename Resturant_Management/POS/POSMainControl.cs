using System;
using System.Drawing;
using System.Windows.Forms;

namespace Resturant_Management.POS
{
    public class POSMainControl : UserControl
    {
        // product was selected
        public event EventHandler<ProductEventArgs>? OnProductSelected;

        private TableLayoutPanel splitLayout = null!;
        public POSSale cartView = null!;
        public ProductList productView = null!;

        public POSMainControl()
        {
            InitializeComponentByCode();
            BindEvents();
        }

        private void InitializeComponentByCode()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(240, 242, 245);

            splitLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            splitLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            splitLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            splitLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            cartView = new POSSale { Dock = DockStyle.Fill };
            productView = new ProductList { Dock = DockStyle.Fill };

            splitLayout.Controls.Add(cartView, 0, 0);
            splitLayout.Controls.Add(productView, 1, 0);

            this.Controls.Add(splitLayout);
        }

        private void BindEvents()
        {
            productView.OnProductSelected += (sender, e) =>
            {
                cartView.AddProductToCart(e.Code, e.Name, e.Price);
                OnProductSelected?.Invoke(this, e);
            };
        }
    }
}
