using BillWise.Models.Entities;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;

namespace BillWise.Models.Services
{
    internal class AppFontResolver : IFontResolver
    {
        public static readonly AppFontResolver Instance = new();
        private static byte[]? _regular;
        private static byte[]? _bold;

        public static async Task InitAsync()
        {
            _regular = await ReadAsync("OpenSans-Regular.ttf");
            _bold = await ReadAsync("OpenSans-Semibold.ttf");
        }

        private static async Task<byte[]> ReadAsync(string name)
        {
            using var s = await FileSystem.OpenAppPackageFileAsync(name);
            using var ms = new MemoryStream();
            await s.CopyToAsync(ms);
            return ms.ToArray();
        }

        public string DefaultFontName => "OpenSans";

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            => new FontResolverInfo(isBold ? "OpenSans#b" : "OpenSans#r");

        public byte[] GetFont(string faceName)
            => faceName == "OpenSans#b" ? _bold! : _regular!;
    }

    public class PdfExportService
    {
        private readonly InvoiceService _invoiceService;
        private static bool _fontReady;

        public PdfExportService(InvoiceService invoiceService) => _invoiceService = invoiceService;

        // ── Colours ──────────────────────────────────────────────────────────
        private static readonly XColor CBlue      = XColor.FromArgb(52, 152, 219);
        private static readonly XColor CDark      = XColor.FromArgb(17, 24, 39);
        private static readonly XColor CGray      = XColor.FromArgb(107, 114, 128);
        private static readonly XColor CLightGray = XColor.FromArgb(229, 231, 235);
        private static readonly XColor CLightBg   = XColor.FromArgb(249, 250, 251);
        private static readonly XColor CRowAlt    = XColor.FromArgb(243, 244, 246);

        // ── Public entry point ───────────────────────────────────────────────
        public async Task<string> GenerateAsync(string userEmail)
        {
            if (!_fontReady)
            {
                await AppFontResolver.InitAsync();
                GlobalFontSettings.FontResolver = AppFontResolver.Instance;
                _fontReady = true;
            }

            var invoices   = await _invoiceService.GetAllInvoicesAsync();
            var monthly    = await _invoiceService.GetMonthlyExpensesAsync();
            var categories = await _invoiceService.GetExpensesByCategoryAsync();

            // ── Aggregate stats ──────────────────────────────────────────────
            decimal totalAmt = invoices.Sum(i => i.Amount);
            decimal paidAmt  = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Amount);
            decimal pendAmt  = invoices.Where(i => i.Status == InvoiceStatus.Pending).Sum(i => i.Amount);
            decimal ovdAmt   = invoices.Where(i => i.Status == InvoiceStatus.Overdue).Sum(i => i.Amount);
            int paidCnt = invoices.Count(i => i.Status == InvoiceStatus.Paid);
            int pendCnt = invoices.Count(i => i.Status == InvoiceStatus.Pending);
            int ovdCnt  = invoices.Count(i => i.Status == InvoiceStatus.Overdue);

            // ── Document setup ───────────────────────────────────────────────
            var doc = new PdfDocument();
            doc.Info.Title  = "BillWise Export";
            doc.Info.Author = userEmail;

            const double margin = 40;
            const double pageW  = 595.28;
            const double pageH  = 841.89;
            double cw = pageW - 2 * margin;

            PdfPage    page = doc.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            XGraphics  gfx  = XGraphics.FromPdfPage(page);
            double     y    = 0;

            // ── Fonts ────────────────────────────────────────────────────────
            var reg8  = new XFont("OpenSans", 8);
            var bold8 = new XFont("OpenSans", 8,  XFontStyle.Bold);
            var reg10 = new XFont("OpenSans", 10);
            var reg11 = new XFont("OpenSans", 11);
            var bold13= new XFont("OpenSans", 13, XFontStyle.Bold);
            var bold22= new XFont("OpenSans", 22, XFontStyle.Bold);

            // ── Local helpers ────────────────────────────────────────────────
            void NewPage()
            {
                page      = doc.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                gfx       = XGraphics.FromPdfPage(page);
                y         = margin;
            }

            void EnsureSpace(double need)
            {
                if (y + need > pageH - margin) NewPage();
            }

            void DrawSectionTitle(string title)
            {
                y += 10;
                gfx.DrawString(title, bold13, new XSolidBrush(CDark), margin, y);
                y += 18;
                gfx.DrawLine(new XPen(CLightGray, 0.5), margin, y, pageW - margin, y);
                y += 10;
            }

            void DrawTable(string[] headers, double[] ratios, IEnumerable<string[]> rows)
            {
                // Header row background
                gfx.DrawRectangle(new XSolidBrush(CRowAlt), margin, y, cw, 22);
                double xh = margin;
                for (int i = 0; i < headers.Length; i++)
                {
                    double w = ratios[i] * cw;
                    gfx.DrawString(headers[i], bold8, new XSolidBrush(CDark),
                        new XRect(xh + 6, y, w - 8, 22), XStringFormats.CenterLeft);
                    xh += w;
                }
                y += 22;

                bool alt = false;
                foreach (var row in rows)
                {
                    EnsureSpace(20);
                    if (alt) gfx.DrawRectangle(new XSolidBrush(CLightBg), margin, y, cw, 20);
                    gfx.DrawLine(new XPen(CLightGray, 0.3), margin, y, pageW - margin, y);
                    double xr = margin;
                    for (int i = 0; i < row.Length && i < ratios.Length; i++)
                    {
                        double w = ratios[i] * cw;
                        gfx.DrawString(row[i], reg8, new XSolidBrush(CDark),
                            new XRect(xr + 6, y, w - 8, 20), XStringFormats.CenterLeft);
                        xr += w;
                    }
                    y += 20;
                    alt = !alt;
                }
                // bottom border
                gfx.DrawLine(new XPen(CLightGray, 0.3), margin, y, pageW - margin, y);
                y += 8;
            }

            // ── HEADER BAR ───────────────────────────────────────────────────
            gfx.DrawRectangle(new XSolidBrush(CBlue), 0, 0, pageW, 68);
            gfx.DrawString("BillWise", bold22, XBrushes.White,
                new XRect(margin, 0, cw, 68), XStringFormats.CenterLeft);
            gfx.DrawString($"Export — {DateTime.Now:dd MMMM yyyy}", reg11,
                new XSolidBrush(XColor.FromArgb(214, 234, 248)),
                new XRect(margin, 0, cw, 68), XStringFormats.CenterRight);
            y = 82;

            // Sub-header line
            gfx.DrawString($"Account: {userEmail}", reg10, new XSolidBrush(CGray), margin, y);
            gfx.DrawString($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}", reg10,
                new XSolidBrush(CGray), pageW - margin - 190, y);
            y += 14;
            gfx.DrawLine(new XPen(CLightGray, 0.5), margin, y, pageW - margin, y);
            y += 16;

            // ── SUMMARY CARDS ────────────────────────────────────────────────
            gfx.DrawString("Summary", bold13, new XSolidBrush(CDark), margin, y);
            y += 20;

            double cardW = (cw - 30) / 4;
            void DrawCard(string label, string count, decimal amount, double cx)
            {
                gfx.DrawRoundedRectangle(
                    new XPen(CLightGray, 0.8), new XSolidBrush(CLightBg),
                    cx, y, cardW, 70, 6, 6);
                gfx.DrawString(label, bold8, new XSolidBrush(CGray),
                    new XRect(cx + 8, y + 8, cardW - 16, 14), XStringFormats.TopLeft);
                gfx.DrawString(count, new XFont("OpenSans", 18, XFontStyle.Bold),
                    new XSolidBrush(CDark),
                    new XRect(cx + 8, y + 22, cardW - 16, 24), XStringFormats.TopLeft);
                gfx.DrawString(CurrencyService.Format(amount), reg8,
                    new XSolidBrush(CGray),
                    new XRect(cx + 8, y + 50, cardW - 16, 14), XStringFormats.TopLeft);
            }

            DrawCard("TOTAL",   invoices.Count.ToString(), totalAmt, margin);
            DrawCard("PAID",    paidCnt.ToString(),         paidAmt,  margin + (cardW + 10));
            DrawCard("PENDING", pendCnt.ToString(),         pendAmt,  margin + (cardW + 10) * 2);
            DrawCard("OVERDUE", ovdCnt.ToString(),          ovdAmt,   margin + (cardW + 10) * 3);
            y += 80;

            // ── MONTHLY EXPENSES ─────────────────────────────────────────────
            if (monthly.Count > 0)
            {
                EnsureSpace(50 + monthly.Count * 20);
                DrawSectionTitle("Monthly Expenses (last 6 months)");
                DrawTable(
                    new[] { "Month", "Amount" },
                    new[] { 0.60, 0.40 },
                    monthly.Select(m => new[]
                    {
                        m.Month.ToString("MMMM yyyy"),
                        CurrencyService.Format(m.Amount)
                    }));
            }

            // ── EXPENSES BY CATEGORY ─────────────────────────────────────────
            if (categories.Count > 0)
            {
                decimal totalCat = categories.Values.Sum();
                EnsureSpace(50 + categories.Count * 20);
                DrawSectionTitle("Expenses by Category");
                DrawTable(
                    new[] { "Category", "Amount", "Share" },
                    new[] { 0.45, 0.30, 0.25 },
                    categories.Select(c => new[]
                    {
                        c.Key.ToString(),
                        CurrencyService.Format(c.Value),
                        totalCat > 0 ? $"{(double)c.Value / (double)totalCat * 100:N1} %" : "0 %"
                    }));
            }

            // ── FULL INVOICE LIST ────────────────────────────────────────────
            EnsureSpace(60);
            DrawSectionTitle($"All Invoices ({invoices.Count})");
            DrawTable(
                new[] { "Name", "Due Date", "Category", "Amount", "Status" },
                new[] { 0.26, 0.16, 0.18, 0.20, 0.20 },
                invoices.Select(i => new[]
                {
                    i.Name,
                    i.DueDate.ToString("dd/MM/yyyy"),
                    i.Category.ToString(),
                    CurrencyService.Format(i.Amount),
                    i.Status.ToString()
                }));

            // ── SAVE ─────────────────────────────────────────────────────────
            var path = Path.Combine(
                FileSystem.CacheDirectory,
                $"BillWise_Export_{DateTime.Now:yyyyMMdd_HHmm}.pdf");

            using var fs = new FileStream(path, FileMode.Create);
            doc.Save(fs);
            return path;
        }
    }
}
