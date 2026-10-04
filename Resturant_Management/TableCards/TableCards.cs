using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Resturant_Management.Data;

namespace Resturant_Management.POS
{
    public partial class TableCards : UserControl
    {
        public TableCards()
        {
            InitializeComponent();
            LoadTablesFromDatabase();
            this.VisibleChanged += (s, e) => { if (this.Visible) LoadTablesFromDatabase(); };
        }

        private void LoadTablesFromDatabase()
        {
            try
            {
                string sql = @"
SELECT t.TableID, t.TableCode, t.TableName, t.ImagePath,
       CASE WHEN EXISTS (SELECT 1 FROM dbo.SALE_ORDER o WHERE o.TableID = t.TableID AND o.Status IN ('Open', 'Sent', 'Billed')) THEN 1 ELSE 0 END AS HasOpenOrder
FROM dbo.DINING_TABLE t
WHERE t.IsActive = 1
ORDER BY t.TableID;";
                DataTable dt = DbHelper.ExecuteQuery(sql);

                // Cards in visual order from left to right:
                // Card 1: panelTable (x=3)
                // Card 2: panel (x=283)
                // Card 3: guna2Panel3 (x=562)
                // Card 4: guna2Panel6 (x=846)
                // Card 5: guna2Panel12 (x=1118)
                Control[] cards = new Control[] { panelTable, panel, guna2Panel3, guna2Panel6, guna2Panel12 };
                Label[] labels = new Label[] { lbtableNum, label3, label1, label2, label4 };
                PictureBox[] pictures = new PictureBox[] { Pictable, guna2PictureBox1, guna2PictureBox2, guna2PictureBox3, guna2PictureBox4 };
                Control[] footers = new Control[] { footertable, guna2Panel10, guna2Panel4, guna2Panel7, guna2Panel13 };

                for (int i = 0; i < cards.Length; i++)
                {
                    if (cards[i] == null) continue;

                    if (i < dt.Rows.Count)
                    {
                        cards[i].Visible = true;
                        int tableId = Convert.ToInt32(dt.Rows[i]["TableID"]);
                        string tableName = dt.Rows[i]["TableName"]?.ToString() ?? $"Table-{i + 1}";
                        bool hasOpen = Convert.ToInt32(dt.Rows[i]["HasOpenOrder"]) == 1;

                        if (labels[i] != null)
                        {
                            labels[i].Text = tableName;
                        }

                        // Load custom image if set
                        string? imgPath = dt.Rows[i]["ImagePath"]?.ToString();
                        if (!string.IsNullOrEmpty(imgPath) && System.IO.File.Exists(imgPath) && pictures[i] != null)
                        {
                            try { pictures[i].Image = Image.FromFile(imgPath); } catch { }
                        }

                        // Update footer status color & text
                        if (footers[i] != null)
                        {
                            footers[i].BackColor = hasOpen ? Color.FromArgb(255, 204, 204) : Color.FromArgb(255, 224, 192);
                            // Set or update status label
                            Label? lblStatus = footers[i].Controls.OfType<Label>().FirstOrDefault();
                            if (lblStatus == null)
                            {
                                lblStatus = new Label
                                {
                                    Dock = DockStyle.Fill,
                                    TextAlign = ContentAlignment.MiddleCenter,
                                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                                };
                                footers[i].Controls.Add(lblStatus);
                            }
                            lblStatus.Text = hasOpen ? "Occupied" : "Available";
                            lblStatus.ForeColor = hasOpen ? Color.FromArgb(180, 40, 40) : Color.FromArgb(120, 80, 40);
                        }

                        BindCardClick(cards[i], tableId, tableName);
                    }
                    else
                    {
                        cards[i].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tables: {ex.Message}");
                AttachTableClickEvents();
            }
        }

        private void BindCardClick(Control ctrl, int tableId, string tableName)
        {
            ctrl.Cursor = Cursors.Hand;
            ctrl.Click += (s, e) => OpenPOSSale(tableId, tableName);
            foreach (Control child in ctrl.Controls)
            {
                BindCardClick(child, tableId, tableName);
            }
        }

        private void AttachTableClickEvents()
        {
            RegisterClickRecursive(this);
        }

        private void RegisterClickRecursive(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                ctrl.Click += Table_Click;

                if (ctrl.HasChildren)
                {
                    RegisterClickRecursive(ctrl);
                }
            }
        }

        private void Table_Click(object? sender, EventArgs e)
        {
            OpenPOSSale(1, "Table-1");
        }

        private void OpenPOSSale(int tableId = 1, string tableName = "Table-1")
        {
            Control? parentPanel = this.Parent;
            if (parentPanel == null) return;

            parentPanel.Controls.Clear();

            TableLayoutPanel posContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };

            posContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            posContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            posContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            POSSale posSaleControl = new POSSale(tableId, tableName) { Dock = DockStyle.Fill };
            ProductList productListControl = new ProductList { Dock = DockStyle.Fill };

            posContainer.Controls.Add(posSaleControl, 0, 0);
            posContainer.Controls.Add(productListControl, 1, 0);

            parentPanel.Controls.Add(posContainer);
            posContainer.BringToFront();
        }

        // Empty event handlers kept to prevent designer breaking
        private void guna2PictureBox1_Click(object? sender, EventArgs e) => OpenPOSSale(1, "Table-1");
        private void label1_Click(object? sender, EventArgs e) => OpenPOSSale(2, "Table-2");
        private void Pictable_Click(object? sender, EventArgs e) => OpenPOSSale(1, "Table-1");
        private void guna2Panel1_Paint(object? sender, PaintEventArgs e) { }
        private void guna2Panel3_Paint(object? sender, PaintEventArgs e) { }
        private void guna2Panel15_Paint(object? sender, PaintEventArgs e) { }
        private void guna2Panel21_Paint(object? sender, PaintEventArgs e) { }
    }
}