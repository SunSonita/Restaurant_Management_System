using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.POS
{
    /// <summary>
    /// Builds the 80mm payment receipt for a saved order straight from the database,
    /// so a receipt can be printed right after payment or reprinted later from the receipt list.
    /// </summary>
    public static class ReceiptPrinter
    {
        private const int PaperWidth = 315; // 80mm in hundredths of an inch

        public static string FormatReceiptNo(long paymentId) => $"R-{paymentId:000000}";

        private sealed class ReceiptLine
        {
            public string Name = "";
            public decimal Qty;
            public decimal Price;
            public decimal DiscountPercent;
            public decimal Total;
        }

        private sealed class ReceiptData
        {
            public string CompanyName = "RESTAURANT MANAGEMENT";
            public string CompanyPhone = "";
            public string CompanyAddress = "";
            public string ReceiptNo = "";
            public string OrderNo = "";
            public string TableName = "";
            public string CustomerName = "";
            public string Cashier = "";
            public DateTime PaymentDate;
            public string Note = "";
            public decimal SubTotal;
            public decimal Discount;
            public decimal GrandTotal;
            public decimal Rate = 4000m;
            public decimal ReceivedUsd;
            public decimal ChangeUsd;
            public string PaymentType = "";
            public bool IsPaid;
            public List<ReceiptLine> Lines = new List<ReceiptLine>();
        }

        /// <summary>Opens the 80mm print preview for the order's receipt (the preview has a Print button).</summary>
        public static void ShowPreview(long orderId, IWin32Window? owner = null)
        {
            try
            {
                ReceiptData? data = Load(orderId);
                if (data == null)
                {
                    MessageBox.Show("Receipt not found for this order.", "Receipt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                float contentHeight;
                using (var probe = new PrintDocument())
                {
                    try
                    {
                        using Graphics mg = probe.PrinterSettings.CreateMeasurementGraphics();
                        contentHeight = Render(mg, data);
                    }
                    catch
                    {
                        using var bmp = new Bitmap(1, 1);
                        using Graphics bg = Graphics.FromImage(bmp);
                        contentHeight = Render(bg, data);
                    }
                }

                using var doc = new PrintDocument();
                doc.DocumentName = $"Receipt_{data.ReceiptNo}";
                doc.DefaultPageSettings.PaperSize = new PaperSize("80mm Thermal", PaperWidth, Math.Max(315, (int)Math.Ceiling(contentHeight) + 20));
                doc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                doc.PrintPage += (s, e) =>
                {
                    if (e.Graphics != null) Render(e.Graphics, data);
                };

                using var preview = new PrintPreviewDialog
                {
                    Document = doc,
                    Width = 520,
                    Height = 740,
                    StartPosition = FormStartPosition.CenterParent,
                    Text = $"Print Preview (80mm) - Receipt {data.ReceiptNo}"
                };
                var ppc = preview.Controls.OfType<PrintPreviewControl>().FirstOrDefault();
                if (ppc != null)
                {
                    ppc.AutoZoom = false;
                    ppc.Zoom = 1.25;
                    ppc.Rows = 1;
                    ppc.Columns = 1;
                }

                if (owner != null) preview.ShowDialog(owner); else preview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print preview error: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static ReceiptData? Load(long orderId)
        {
            DbHelper.EnsurePaymentSchema();

            DataTable dtOrder = DbHelper.ExecuteQuery(@"
SELECT o.OrderNo, o.Note, o.SubTotal, o.ItemDiscountTotal, o.DocDiscountAmount, o.GrandTotal, o.Status, o.PostingDate,
       CASE WHEN o.TableID IS NULL THEN 'Takeaway' ELSE ISNULL(t.TableName, '') END AS TableName,
       ISNULL(c.CustomerName, 'General Customer') AS CustomerName,
       ISNULL(cu.FullName, 'System') AS Creator
FROM dbo.SALE_ORDER o
LEFT JOIN dbo.DINING_TABLE t ON o.TableID = t.TableID
LEFT JOIN dbo.CUSTOMER c ON o.CustomerID = c.CustomerID
LEFT JOIN dbo.APP_USER cu ON o.CreatedBy = cu.UserID
WHERE o.OrderID = @OID", new SqlParameter("@OID", orderId));
            if (dtOrder.Rows.Count == 0) return null;
            DataRow o = dtOrder.Rows[0];

            var data = new ReceiptData
            {
                OrderNo = o["OrderNo"]?.ToString() ?? "",
                Note = o["Note"]?.ToString() ?? "",
                SubTotal = Convert.ToDecimal(o["SubTotal"]),
                Discount = Convert.ToDecimal(o["ItemDiscountTotal"]) + Convert.ToDecimal(o["DocDiscountAmount"]),
                GrandTotal = Convert.ToDecimal(o["GrandTotal"]),
                IsPaid = (o["Status"]?.ToString() ?? "") == "Paid",
                PaymentDate = Convert.ToDateTime(o["PostingDate"]),
                TableName = o["TableName"]?.ToString() ?? "",
                CustomerName = o["CustomerName"]?.ToString() ?? "",
                Cashier = o["Creator"]?.ToString() ?? ""
            };

            DataTable dtPay = DbHelper.ExecuteQuery(@"
SELECT TOP 1 p.PaymentID, p.PaymentDate, p.TotalReceived, ISNULL(p.ChangeAmount, p.ChangeGiven) AS ChangeAmount, p.ExchangeRate,
       ISNULL(u.FullName, 'System') AS Cashier,
       ISNULL((SELECT STRING_AGG(x.MethodName, ', ') FROM (
            SELECT DISTINCT m.MethodName FROM dbo.PAYMENT_DETAIL pd
            JOIN dbo.PAYMENT_METHOD m ON pd.MethodID = m.MethodID
            WHERE pd.PaymentID = p.PaymentID) x), '') AS PaymentType
FROM dbo.PAYMENT p
LEFT JOIN dbo.APP_USER u ON p.ReceivedBy = u.UserID
WHERE p.OrderID = @OID
ORDER BY p.PaymentID DESC", new SqlParameter("@OID", orderId));
            if (dtPay.Rows.Count > 0)
            {
                DataRow p = dtPay.Rows[0];
                data.ReceiptNo = FormatReceiptNo(Convert.ToInt64(p["PaymentID"]));
                data.PaymentDate = Convert.ToDateTime(p["PaymentDate"]);
                data.ReceivedUsd = p["TotalReceived"] != DBNull.Value ? Convert.ToDecimal(p["TotalReceived"]) : 0m;
                data.ChangeUsd = p["ChangeAmount"] != DBNull.Value ? Convert.ToDecimal(p["ChangeAmount"]) : 0m;
                if (p["ExchangeRate"] != DBNull.Value && Convert.ToDecimal(p["ExchangeRate"]) > 0) data.Rate = Convert.ToDecimal(p["ExchangeRate"]);
                data.Cashier = p["Cashier"]?.ToString() ?? data.Cashier;
                data.PaymentType = p["PaymentType"]?.ToString() ?? "";
            }
            else
            {
                data.ReceiptNo = data.OrderNo;
            }

            DataTable dtItems = DbHelper.ExecuteQuery(@"
SELECT ItemName, UomName, Qty, UnitPrice, DiscountPercent, TotalAfterDis
FROM dbo.SALE_ORDER_ITEM WHERE OrderID = @OID ORDER BY [LineNo]", new SqlParameter("@OID", orderId));
            foreach (DataRow r in dtItems.Rows)
            {
                string name = r["ItemName"]?.ToString() ?? "";
                string uom = r["UomName"]?.ToString() ?? "";
                data.Lines.Add(new ReceiptLine
                {
                    Name = string.IsNullOrWhiteSpace(uom) || name.Contains($"({uom})") ? name : $"{name} ({uom})",
                    Qty = Convert.ToDecimal(r["Qty"]),
                    Price = Convert.ToDecimal(r["UnitPrice"]),
                    DiscountPercent = Convert.ToDecimal(r["DiscountPercent"]),
                    Total = Convert.ToDecimal(r["TotalAfterDis"])
                });
            }

            try
            {
                DataTable dtComp = DbHelper.ExecuteQuery("SELECT TOP 1 CompanyName, Phone, Address FROM dbo.COMPANY_PROFILE");
                if (dtComp.Rows.Count > 0)
                {
                    string? cName = dtComp.Rows[0]["CompanyName"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(cName)) data.CompanyName = cName;
                    data.CompanyPhone = dtComp.Rows[0]["Phone"]?.ToString() ?? "";
                    data.CompanyAddress = dtComp.Rows[0]["Address"]?.ToString() ?? "";
                }
            }
            catch { }

            return data;
        }

        /// <summary>Draws the receipt and returns the final Y used (for page-length sizing).</summary>
        private static float Render(Graphics g, ReceiptData d)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float width = 299f;
            float startX = 8f;
            float endX = startX + width;
            float y = 12f;

            using var fontTitle = DbHelper.GetKhmerFont(11F, FontStyle.Bold);
            using var fontSubtitle = DbHelper.GetKhmerFont(8F, FontStyle.Regular);
            using var fontHeader = DbHelper.GetKhmerFont(9.5F, FontStyle.Bold);
            using var fontBody = DbHelper.GetKhmerFont(8F, FontStyle.Regular);
            using var fontBodyBold = DbHelper.GetKhmerFont(8F, FontStyle.Bold);
            using var fontTotal = DbHelper.GetKhmerFont(10F, FontStyle.Bold);
            using var fontSmall = DbHelper.GetKhmerFont(7.5F, FontStyle.Regular);

            using var penDash = new Pen(Color.FromArgb(90, 90, 90), 1f) { DashStyle = DashStyle.Dash };
            using var penSolid = new Pen(Color.Black, 1.2f);
            Brush brush = Brushes.Black;

            using var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
            using var sfRight = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            using var sfItem = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.Word };

            // 1. Company
            g.DrawString(d.CompanyName, fontTitle, brush, new RectangleF(startX, y, width, 22), sfCenter);
            y += 22;
            if (!string.IsNullOrWhiteSpace(d.CompanyAddress))
            {
                g.DrawString(d.CompanyAddress, fontSubtitle, brush, new RectangleF(startX, y, width, 16), sfCenter);
                y += 16;
            }
            if (!string.IsNullOrWhiteSpace(d.CompanyPhone))
            {
                g.DrawString($"Tel: {d.CompanyPhone}", fontSubtitle, brush, new RectangleF(startX, y, width, 16), sfCenter);
                y += 16;
            }

            y += 4;
            g.DrawString("RECEIPT / វិក្កយបត្រ", fontHeader, brush, new RectangleF(startX, y, width, 20), sfCenter);
            y += 22;
            g.DrawLine(penDash, startX, y, endX, y);
            y += 6;

            // 2. Meta
            void Meta(string label, string val)
            {
                g.DrawString(label, fontBodyBold, brush, new RectangleF(startX, y, 75, 16), sfLeft);
                g.DrawString(val, fontBody, brush, new RectangleF(startX + 75, y, width - 75, 16), sfLeft);
                y += 17;
            }
            Meta("Receipt No :", d.ReceiptNo);
            Meta("Order No :", d.OrderNo);
            Meta("Table    :", d.TableName);
            Meta("Customer :", d.CustomerName);
            Meta("Date     :", d.PaymentDate.ToString("dd/MM/yyyy HH:mm"));
            Meta("Cashier  :", d.Cashier);
            if (!string.IsNullOrEmpty(d.Note)) Meta("Note     :", d.Note);

            y += 4;
            g.DrawLine(penDash, startX, y, endX, y);
            y += 6;

            // 3. Items (KHR)
            float colItemW = 135f, colQtyW = 32f, colPriceW = 58f, colTotalW = 68f;
            float colItemX = startX;
            float colQtyX = colItemX + colItemW + 2f;
            float colPriceX = colQtyX + colQtyW + 2f;
            float colTotalX = colPriceX + colPriceW + 2f;

            g.DrawString("Item", fontBodyBold, brush, new RectangleF(colItemX, y, colItemW, 18), sfLeft);
            g.DrawString("Qty", fontBodyBold, brush, new RectangleF(colQtyX, y, colQtyW, 18), sfCenter);
            g.DrawString("Price", fontBodyBold, brush, new RectangleF(colPriceX, y, colPriceW, 18), sfRight);
            g.DrawString("Total", fontBodyBold, brush, new RectangleF(colTotalX, y, colTotalW, 18), sfRight);
            y += 20;
            g.DrawLine(penDash, startX, y, endX, y);
            y += 6;

            foreach (var line in d.Lines)
            {
                SizeF nameSize = g.MeasureString(line.Name, fontBody, (int)colItemW, sfItem);
                float rowH = Math.Max(18f, nameSize.Height + 2);
                g.DrawString(line.Name, fontBody, brush, new RectangleF(colItemX, y, colItemW, rowH), sfItem);
                g.DrawString(line.Qty.ToString("0.##"), fontBody, brush, new RectangleF(colQtyX, y, colQtyW, 18), sfCenter);
                g.DrawString(line.Price.ToString("N0"), fontBody, brush, new RectangleF(colPriceX, y, colPriceW, 18), sfRight);
                g.DrawString(line.Total.ToString("N0"), fontBody, brush, new RectangleF(colTotalX, y, colTotalW, 18), sfRight);
                y += rowH;
                if (line.DiscountPercent > 0)
                {
                    g.DrawString($"  (Disc: {line.DiscountPercent:0.##}%)", fontSmall, Brushes.DimGray, new RectangleF(colItemX, y, colItemW, 14), sfLeft);
                    y += 14;
                }
                y += 3;
            }

            g.DrawLine(penDash, startX, y, endX, y);
            y += 6;

            // 4. Totals
            void Sum(string label, string val, Font f)
            {
                g.DrawString(label, f, brush, new RectangleF(startX, y, 120, 18), sfLeft);
                g.DrawString(val, f, brush, new RectangleF(startX + 120, y, width - 120, 18), sfRight);
                y += 19;
            }

            Sum("SubTotal :", $"KHR {d.SubTotal:N0} (${d.SubTotal / d.Rate:N2})", fontBody);
            if (d.Discount > 0) Sum("Discount :", $"- KHR {d.Discount:N0}", fontBody);
            y += 2;
            g.DrawLine(penSolid, startX, y, endX, y);
            y += 6;
            Sum("GRAND TOTAL :", $"KHR {d.GrandTotal:N0}", fontTotal);
            Sum("TOTAL USD   :", $"${d.GrandTotal / d.Rate:N2}", fontTotal);
            y += 2;
            g.DrawLine(penSolid, startX, y, endX, y);
            y += 6;

            // 5. Payment
            if (d.IsPaid && d.ReceivedUsd > 0)
            {
                string method = string.IsNullOrWhiteSpace(d.PaymentType) ? "Cash" : d.PaymentType;
                Sum($"Paid ({method}) :", $"${d.ReceivedUsd:N2} ({d.ReceivedUsd * d.Rate:N0} ៛)", fontBody);
                Sum("Change :", $"${d.ChangeUsd:N2} ({d.ChangeUsd * d.Rate:N0} ៛)", fontBodyBold);
                y += 2;
                g.DrawLine(penDash, startX, y, endX, y);
                y += 6;
            }

            // 6. Footer
            y += 6;
            g.DrawString("Thank you for dining with us!", fontBodyBold, brush, new RectangleF(startX, y, width, 18), sfCenter);
            y += 18;
            g.DrawString("សូមអរគុណ សូមអញ្ជើញមកពិសាម្តងទៀត!", fontBody, brush, new RectangleF(startX, y, width, 18), sfCenter);
            y += 20;
            g.DrawString($"Printed: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fontSmall, Brushes.DimGray, new RectangleF(startX, y, width, 14), sfCenter);
            y += 16;

            return y;
        }
    }
}
