using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Resturant_Management.POS
{
    public class FrmReceiptPreview : Form
    {
        private readonly PrintDocument _printDoc;
        private readonly Action<Graphics, float, float>? _fallbackRenderer;
        private readonly List<Image> _pages = new List<Image>();
        private int _currentPage = 0;
        private double _zoom = 1.25; // 125% default
        private float _paperWidthHundredths = 315f;
        private float _paperHeightHundredths = 600f;

        private ToolStrip _toolStrip = null!;
        private ToolStripSplitButton _btnPrint = null!;
        private ToolStripButton _btnZoomIn = null!;
        private ToolStripButton _btnZoomOut = null!;
        private ToolStripButton _btnFitWidth = null!;
        private ToolStripComboBox _cmbZoom = null!;
        private ToolStripButton _btnClose = null!;
        private ToolStripButton _btnPrevPage = null!;
        private ToolStripButton _btnNextPage = null!;
        private ToolStripLabel _lblPage = null!;

        private ReceiptCanvas _canvas = null!;

        public FrmReceiptPreview(
            PrintDocument printDoc,
            string title = "Print Preview (80x80)",
            Action<Graphics, float, float>? fallbackRenderer = null)
        {
            _printDoc = printDoc ?? throw new ArgumentNullException(nameof(printDoc));
            _fallbackRenderer = fallbackRenderer;

            if (_printDoc.DefaultPageSettings?.PaperSize != null)
            {
                _paperWidthHundredths = _printDoc.DefaultPageSettings.PaperSize.Width;
                _paperHeightHundredths = _printDoc.DefaultPageSettings.PaperSize.Height;
            }

            InitializeUi(title);
            GeneratePreview();
        }

        private void InitializeUi(string title)
        {
            this.Text = title;
            this.Size = new Size(540, 760);
            this.MinimumSize = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowIcon = false;
            this.BackColor = Color.FromArgb(160, 160, 160);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.KeyPreview = true;

            // Top ToolStrip
            _toolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden,
                BackColor = SystemColors.Control,
                RenderMode = ToolStripRenderMode.System,
                Padding = new Padding(6, 4, 6, 4),
                AutoSize = true
            };

            // Print button (split button)
            _btnPrint = new ToolStripSplitButton("Print", CreatePrinterIcon(), (s, e) => DoPrint(false))
            {
                ToolTipText = "Print directly to default printer",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };
            var itemDirect = new ToolStripMenuItem("Print Directly (Default Printer)", null, (s, e) => DoPrint(false));
            var itemDialog = new ToolStripMenuItem("Select Printer...", null, (s, e) => DoPrint(true));
            _btnPrint.DropDownItems.Add(itemDirect);
            _btnPrint.DropDownItems.Add(itemDialog);
            _toolStrip.Items.Add(_btnPrint);

            _toolStrip.Items.Add(new ToolStripSeparator());

            // Zoom buttons
            _btnZoomIn = new ToolStripButton(string.Empty, CreateZoomInIcon(), (s, e) => ZoomStep(0.15))
            {
                ToolTipText = "Zoom In (Ctrl + Wheel Up)"
            };
            _toolStrip.Items.Add(_btnZoomIn);

            _cmbZoom = new ToolStripComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 75,
                ToolTipText = "Zoom Level"
            };
            _cmbZoom.Items.AddRange(new object[] { "50%", "75%", "100%", "125%", "150%", "200%", "Fit Width" });
            _cmbZoom.SelectedItem = "125%";
            _cmbZoom.SelectedIndexChanged += CmbZoom_SelectedIndexChanged;
            _toolStrip.Items.Add(_cmbZoom);

            _btnZoomOut = new ToolStripButton(string.Empty, CreateZoomOutIcon(), (s, e) => ZoomStep(-0.15))
            {
                ToolTipText = "Zoom Out (Ctrl + Wheel Down)"
            };
            _toolStrip.Items.Add(_btnZoomOut);

            _btnFitWidth = new ToolStripButton("Fit", CreateFitIcon(), (s, e) => FitWidth())
            {
                ToolTipText = "Fit Width",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };
            _toolStrip.Items.Add(_btnFitWidth);

            _toolStrip.Items.Add(new ToolStripSeparator());

            // Close button
            _btnClose = new ToolStripButton("Close", CreateCloseIcon(), (s, e) => this.Close())
            {
                ToolTipText = "Close Preview (Esc)",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };
            _toolStrip.Items.Add(_btnClose);

            _toolStrip.Items.Add(new ToolStripSeparator());

            // Page navigation
            _btnPrevPage = new ToolStripButton("◀", null, (s, e) => ChangePage(_currentPage - 1))
            {
                ToolTipText = "Previous Page",
                Enabled = false
            };
            _btnNextPage = new ToolStripButton("▶", null, (s, e) => ChangePage(_currentPage + 1))
            {
                ToolTipText = "Next Page",
                Enabled = false
            };
            _lblPage = new ToolStripLabel("Page 1 of 1")
            {
                Alignment = ToolStripItemAlignment.Right
            };

            _toolStrip.Items.Add(_btnPrevPage);
            _toolStrip.Items.Add(_btnNextPage);
            _toolStrip.Items.Add(_lblPage);

            // Viewer Canvas
            _canvas = new ReceiptCanvas
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(160, 160, 160)
            };
            _canvas.ZoomChanged += (zoom) =>
            {
                _zoom = zoom;
                _cmbZoom.Text = $"{(int)Math.Round(_zoom * 100)}%";
            };

            this.Controls.Add(_canvas);
            this.Controls.Add(_toolStrip);

            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
                else if (e.Control && e.KeyCode == Keys.P)
                {
                    DoPrint(false);
                }
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CenterFormToScreen();
            _canvas.SetPage(_pages.Count > 0 ? _pages[_currentPage] : null, _paperWidthHundredths, _paperHeightHundredths, _zoom);
        }

        private void CenterFormToScreen()
        {
            try
            {
                Screen screen = this.Owner != null ? Screen.FromControl(this.Owner) : Screen.FromPoint(Cursor.Position);
                Rectangle bounds = screen.WorkingArea;
                this.Location = new Point(
                    bounds.Left + Math.Max(0, (bounds.Width - this.Width) / 2),
                    bounds.Top + Math.Max(0, (bounds.Height - this.Height) / 2)
                );
            }
            catch
            {
                this.CenterToScreen();
            }
        }

        private void GeneratePreview()
        {
            _pages.Clear();
            _currentPage = 0;

            try
            {
                // Silent preview generation with PreviewPrintController: ZERO "Generating preview..." popup!
                var previewController = new PreviewPrintController { UseAntiAlias = true };
                _printDoc.PrintController = previewController;
                _printDoc.Print();

                var pageInfos = previewController.GetPreviewPageInfo();
                if (pageInfos != null)
                {
                    foreach (var info in pageInfos)
                    {
                        if (info?.Image != null)
                        {
                            _pages.Add(info.Image);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PreviewPrintController error: " + ex.Message);

                // Fallback: If no printer installed or driver error, render directly using fallbackRenderer
                if (_fallbackRenderer != null)
                {
                    try
                    {
                        int w = (int)Math.Ceiling(_paperWidthHundredths / 100f * 96f);
                        int h = (int)Math.Ceiling(_paperHeightHundredths / 100f * 96f);
                        var bmp = new Bitmap(Math.Max(100, w), Math.Max(100, h));
                        using (var g = Graphics.FromImage(bmp))
                        {
                            g.Clear(Color.White);
                            _fallbackRenderer(g, _paperWidthHundredths, _paperHeightHundredths);
                        }
                        _pages.Add(bmp);
                    }
                    catch (Exception ex2)
                    {
                        System.Diagnostics.Debug.WriteLine("Fallback render error: " + ex2.Message);
                    }
                }
            }

            UpdatePageControls();
        }

        private void UpdatePageControls()
        {
            int total = Math.Max(1, _pages.Count);
            _lblPage.Text = $"Page {_currentPage + 1} of {total}";
            _btnPrevPage.Enabled = _currentPage > 0;
            _btnNextPage.Enabled = _currentPage < _pages.Count - 1;

            if (_pages.Count > 0 && _currentPage >= 0 && _currentPage < _pages.Count)
            {
                _canvas.SetPage(_pages[_currentPage], _paperWidthHundredths, _paperHeightHundredths, _zoom);
            }
            else
            {
                _canvas.SetPage(null, _paperWidthHundredths, _paperHeightHundredths, _zoom);
            }
        }

        private void ChangePage(int newPage)
        {
            if (newPage >= 0 && newPage < _pages.Count)
            {
                _currentPage = newPage;
                UpdatePageControls();
            }
        }

        private void ZoomStep(double delta)
        {
            double newZoom = Math.Max(0.25, Math.Min(4.0, _zoom + delta));
            _zoom = newZoom;
            _cmbZoom.Text = $"{(int)Math.Round(_zoom * 100)}%";
            _canvas.SetZoom(_zoom);
        }

        private void FitWidth()
        {
            int clientW = _canvas.ClientSize.Width - 48;
            if (clientW <= 0) return;
            double baseW = _paperWidthHundredths / 100.0 * 96.0;
            _zoom = Math.Max(0.25, Math.Min(3.0, clientW / baseW));
            _cmbZoom.Text = $"{(int)Math.Round(_zoom * 100)}%";
            _canvas.SetZoom(_zoom);
        }

        private void CmbZoom_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string txt = _cmbZoom.SelectedItem?.ToString() ?? "";
            if (txt == "Fit Width")
            {
                FitWidth();
            }
            else if (txt.EndsWith("%") && int.TryParse(txt.TrimEnd('%'), out int pct))
            {
                _zoom = Math.Max(0.25, Math.Min(4.0, pct / 100.0));
                _canvas.SetZoom(_zoom);
            }
        }

        public void DoPrint(bool promptDialog)
        {
            try
            {
                if (promptDialog)
                {
                    using var pd = new PrintDialog
                    {
                        Document = _printDoc,
                        UseEXDialog = true
                    };
                    if (pd.ShowDialog(this) != DialogResult.OK) return;
                }

                // Suppress printing status dialog as well
                _printDoc.PrintController = new StandardPrintController();
                _printDoc.Print();
                this.Close();
            }
            catch (Exception ex)
            {
                if (!promptDialog)
                {
                    // If direct print fails, let user pick printer
                    DoPrint(true);
                }
                else
                {
                    MessageBox.Show(this, "Failed to print: " + ex.Message, "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ---------- Icons ----------
        private static Bitmap CreatePrinterIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush b = new SolidBrush(Color.FromArgb(40, 45, 55)))
                using (Pen p = new Pen(Color.FromArgb(40, 45, 55), 1.2f))
                {
                    g.DrawRectangle(p, 3, 1, 9, 4);
                    g.FillRectangle(b, 1, 5, 14, 6);
                    g.FillRectangle(Brushes.White, 3, 8, 10, 6);
                    g.DrawRectangle(p, 3, 8, 10, 6);
                }
            }
            return bmp;
        }

        private static Bitmap CreateZoomInIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen p = new Pen(Color.FromArgb(50, 55, 65), 1.6f))
                {
                    g.DrawEllipse(p, 2, 2, 8, 8);
                    g.DrawLine(p, 8, 8, 13, 13);
                    g.DrawLine(p, 4, 6, 8, 6);
                    g.DrawLine(p, 6, 4, 6, 8);
                }
            }
            return bmp;
        }

        private static Bitmap CreateZoomOutIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen p = new Pen(Color.FromArgb(50, 55, 65), 1.6f))
                {
                    g.DrawEllipse(p, 2, 2, 8, 8);
                    g.DrawLine(p, 8, 8, 13, 13);
                    g.DrawLine(p, 4, 6, 8, 6);
                }
            }
            return bmp;
        }

        private static Bitmap CreateFitIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen p = new Pen(Color.FromArgb(50, 55, 65), 1.3f))
                {
                    g.DrawRectangle(p, 2, 2, 11, 11);
                    g.DrawLine(p, 5, 2, 5, 13);
                    g.DrawLine(p, 10, 2, 10, 13);
                }
            }
            return bmp;
        }

        private static Bitmap CreateCloseIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen p = new Pen(Color.FromArgb(190, 60, 60), 2f))
                {
                    g.DrawLine(p, 3, 3, 12, 12);
                    g.DrawLine(p, 12, 3, 3, 12);
                }
            }
            return bmp;
        }

        // ---------- Custom Canvas ----------
        private class ReceiptCanvas : Panel
        {
            private Image? _image;
            private float _paperW = 315f;
            private float _paperH = 600f;
            private double _zoom = 1.25;

            public event Action<double>? ZoomChanged;

            public ReceiptCanvas()
            {
                this.DoubleBuffered = true;
                this.AutoScroll = true;
                this.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            }

            public void SetPage(Image? image, float paperWidth, float paperHeight, double zoom)
            {
                _image = image;
                _paperW = paperWidth;
                _paperH = paperHeight;
                _zoom = zoom;
                UpdateScrollSize();
                this.Invalidate();
            }

            public void SetZoom(double zoom)
            {
                _zoom = zoom;
                UpdateScrollSize();
                this.Invalidate();
            }

            private void UpdateScrollSize()
            {
                int pixelW = (int)Math.Round(_paperW / 100.0 * 96.0 * _zoom);
                int pixelH = (int)Math.Round(_paperH / 100.0 * 96.0 * _zoom);
                this.AutoScrollMinSize = new Size(pixelW + 40, pixelH + 40);
            }

            protected override void OnResize(EventArgs eventargs)
            {
                base.OnResize(eventargs);
                this.Invalidate();
            }

            protected override void OnMouseWheel(MouseEventArgs e)
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    double delta = e.Delta > 0 ? 0.15 : -0.15;
                    _zoom = Math.Max(0.25, Math.Min(4.0, _zoom + delta));
                    UpdateScrollSize();
                    this.Invalidate();
                    ZoomChanged?.Invoke(_zoom);
                    return;
                }
                base.OnMouseWheel(e);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;

                if (_image == null)
                {
                    using var font = new Font("Segoe UI", 11F, FontStyle.Regular);
                    using var brush = new SolidBrush(Color.FromArgb(70, 70, 70));
                    using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("No preview available", font, brush, this.ClientRectangle, sf);
                    return;
                }

                int paperPixelW = (int)Math.Round(_paperW / 100.0 * 96.0 * _zoom);
                int paperPixelH = (int)Math.Round(_paperH / 100.0 * 96.0 * _zoom);

                int scrollX = this.AutoScrollPosition.X;
                int scrollY = this.AutoScrollPosition.Y;

                int posX = scrollX + (paperPixelW < this.ClientSize.Width ? (this.ClientSize.Width - paperPixelW) / 2 : 20);
                int posY = scrollY + 20;

                Rectangle paperRect = new Rectangle(posX, posY, paperPixelW, paperPixelH);

                // Drop shadow
                Rectangle shadowRect = new Rectangle(paperRect.X + 4, paperRect.Y + 4, paperRect.Width, paperRect.Height);
                using (var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                {
                    g.FillRectangle(shadowBrush, shadowRect);
                }

                // White paper background
                g.FillRectangle(Brushes.White, paperRect);

                // Render vector receipt image
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(_image, paperRect);

                // Thin paper border
                using (var borderPen = new Pen(Color.FromArgb(130, 130, 130), 1f))
                {
                    g.DrawRectangle(borderPen, paperRect);
                }
            }
        }
    }
}
