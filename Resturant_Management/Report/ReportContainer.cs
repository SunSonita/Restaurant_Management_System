using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Resturant_Management.Report
{
    public class ReportContainer : UserControl
    {
        private Panel pnlTabBar = null!;
        private Panel pnlContent = null!;
        private Guna2Button btnSaleSummary = null!;
        private Guna2Button btnSaleByTable = null!;

        private SaleSummary saleSummaryView = null!;
        private SaleByTable saleByTableView = null!;

        public ReportContainer()
        {
            InitializeComponentByCode();
        }

        private void InitializeComponentByCode()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(240, 244, 248);

            // Tab bar at top
            pnlTabBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.White,
                Padding = new Padding(25, 8, 25, 8)
            };

            btnSaleSummary = new Guna2Button
            {
                Text = "📊 Sale Summary Report",
                Height = 40,
                Width = 220,
                BorderRadius = 6,
                FillColor = Color.FromArgb(10, 20, 110),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Dock = DockStyle.Left,
                Margin = new Padding(0, 0, 10, 0)
            };

            btnSaleByTable = new Guna2Button
            {
                Text = "🪑 Sale By Table Report",
                Height = 40,
                Width = 220,
                BorderRadius = 6,
                FillColor = Color.FromArgb(235, 240, 245),
                ForeColor = Color.FromArgb(60, 70, 80),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Dock = DockStyle.Left
            };

            pnlTabBar.Controls.Add(btnSaleByTable);
            pnlTabBar.Controls.Add(btnSaleSummary);

            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 244, 248)
            };

            saleSummaryView = new SaleSummary { Dock = DockStyle.Fill };
            saleByTableView = new SaleByTable { Dock = DockStyle.Fill };

            pnlContent.Controls.Add(saleSummaryView);

            btnSaleSummary.Click += (s, e) =>
            {
                btnSaleSummary.FillColor = Color.FromArgb(10, 20, 110);
                btnSaleSummary.ForeColor = Color.White;
                btnSaleByTable.FillColor = Color.FromArgb(235, 240, 245);
                btnSaleByTable.ForeColor = Color.FromArgb(60, 70, 80);

                pnlContent.Controls.Clear();
                pnlContent.Controls.Add(saleSummaryView);
                saleSummaryView.BringToFront();
            };

            btnSaleByTable.Click += (s, e) =>
            {
                btnSaleByTable.FillColor = Color.FromArgb(10, 20, 110);
                btnSaleByTable.ForeColor = Color.White;
                btnSaleSummary.FillColor = Color.FromArgb(235, 240, 245);
                btnSaleSummary.ForeColor = Color.FromArgb(60, 70, 80);

                pnlContent.Controls.Clear();
                pnlContent.Controls.Add(saleByTableView);
                saleByTableView.BringToFront();
            };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlTabBar);
        }
    }
}
