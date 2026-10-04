using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Resturant_Management.Report
{
    public static class ReportExporter
    {
        /// <summary>
        /// Exports the content of a DataGridView to a CSV file.
        /// </summary>
        public static void ExportToCsv(DataGridView grid, string defaultFileName)
        {
            if (grid == null || grid.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                sfd.FileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();

                        // Header row
                        bool firstCol = true;
                        for (int i = 0; i < grid.Columns.Count; i++)
                        {
                            if (!grid.Columns[i].Visible) continue;
                            if (!firstCol) sb.Append(",");
                            sb.Append(EscapeCsv(grid.Columns[i].HeaderText));
                            firstCol = false;
                        }
                        sb.AppendLine();

                        // Data rows
                        foreach (DataGridViewRow row in grid.Rows)
                        {
                            if (row.IsNewRow) continue;
                            firstCol = true;
                            for (int i = 0; i < grid.Columns.Count; i++)
                            {
                                if (!grid.Columns[i].Visible) continue;
                                if (!firstCol) sb.Append(",");
                                string val = row.Cells[i].Value?.ToString() ?? "";
                                sb.Append(EscapeCsv(val));
                                firstCol = false;
                            }
                            sb.AppendLine();
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Report exported successfully to CSV!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to export report: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private static string EscapeCsv(string str)
        {
            if (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"))
            {
                return $"\"{str.Replace("\"", "\"\"")}\"";
            }
            return str;
        }

        /// <summary>
        /// Prints or shows print preview for a DataGridView report.
        /// </summary>
        public static void PrintReport(DataGridView grid, string title, string subtitle)
        {
            if (grid == null || grid.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to print.", "Print Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int currentRowIndex = 0;
            int pageNumber = 1;

            PrintDocument doc = new PrintDocument();
            doc.DefaultPageSettings.Landscape = true;
            doc.DefaultPageSettings.Margins = new Margins(40, 40, 40, 40);

            doc.PrintPage += (s, ev) =>
            {
                Graphics g = ev.Graphics!;
                Font titleFont = new Font("Segoe UI", 14, FontStyle.Bold);
                Font subFont = new Font("Segoe UI", 9, FontStyle.Regular);
                Font headerFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                Font cellFont = new Font("Segoe UI", 8, FontStyle.Regular);

                int y = ev.MarginBounds.Top;
                int left = ev.MarginBounds.Left;
                int right = ev.MarginBounds.Right;
                int width = ev.MarginBounds.Width;

                // Title
                g.DrawString(title, titleFont, Brushes.Black, left, y);
                y += 28;

                // Subtitle / Filters
                g.DrawString(subtitle, subFont, Brushes.DarkSlateGray, left, y);
                y += 24;

                // Divider line
                g.DrawLine(Pens.Gray, left, y, right, y);
                y += 8;

                // Columns layout
                int visibleColCount = 0;
                for (int i = 0; i < grid.Columns.Count; i++)
                    if (grid.Columns[i].Visible) visibleColCount++;

                int colWidth = Math.Max(60, width / (visibleColCount > 0 ? visibleColCount : 1));
                int rowHeight = 22;

                // Print Table Headers
                int x = left;
                for (int i = 0; i < grid.Columns.Count; i++)
                {
                    if (!grid.Columns[i].Visible) continue;
                    Rectangle rect = new Rectangle(x, y, colWidth, rowHeight);
                    g.FillRectangle(Brushes.LightGray, rect);
                    g.DrawRectangle(Pens.Gray, rect);
                    g.DrawString(grid.Columns[i].HeaderText, headerFont, Brushes.Black, rect);
                    x += colWidth;
                }
                y += rowHeight;

                // Print Data Rows
                while (currentRowIndex < grid.Rows.Count)
                {
                    DataGridViewRow row = grid.Rows[currentRowIndex];
                    if (!row.IsNewRow)
                    {
                        if (y + rowHeight > ev.MarginBounds.Bottom - 30)
                        {
                            ev.HasMorePages = true;
                            pageNumber++;
                            return;
                        }

                        x = left;
                        for (int i = 0; i < grid.Columns.Count; i++)
                        {
                            if (!grid.Columns[i].Visible) continue;
                            Rectangle rect = new Rectangle(x, y, colWidth, rowHeight);
                            g.DrawRectangle(Pens.LightGray, rect);
                            string cellText = row.Cells[i].Value?.ToString() ?? "";
                            g.DrawString(cellText, cellFont, Brushes.Black, rect);
                            x += colWidth;
                        }
                        y += rowHeight;
                    }
                    currentRowIndex++;
                }

                // Footer
                string footer = $"Printed on {DateTime.Now:yyyy-MM-dd HH:mm} | Page {pageNumber}";
                g.DrawString(footer, subFont, Brushes.Gray, left, ev.MarginBounds.Bottom + 5);

                ev.HasMorePages = false;
                currentRowIndex = 0;
            };

            using (PrintPreviewDialog preview = new PrintPreviewDialog())
            {
                preview.Document = doc;
                preview.Width = 1000;
                preview.Height = 700;
                preview.StartPosition = FormStartPosition.CenterParent;
                preview.ShowDialog();
            }
        }
    }
}
