using System.Globalization;
using System.Text;

namespace Sur.Api;

static partial class Fn
{
    const float PageWidth = 595f;
    const float PageHeight = 842f;
    const float MarginLeft = 48f;
    const float MarginRight = 48f;
    const float MarginTop = 58f;
    const float MarginBottom = 48f;
    const float ContentWidth = PageWidth - MarginLeft - MarginRight;

    public static byte[]? WritePdf(IReadOnlyList<SnapshotPart> parts, string snapshotId)
    {
        var regularPath = new[] { @"C:\Windows\Fonts\arial.ttf", @"C:\Windows\Fonts\segoeui.ttf" }.FirstOrDefault(File.Exists);
        var boldPath = new[] { @"C:\Windows\Fonts\arialbd.ttf", @"C:\Windows\Fonts\segoeuib.ttf", regularPath }.FirstOrDefault(path => path != null && File.Exists(path));
        if (regularPath == null || boldPath == null) return null;

        var regular = LoadFace(File.ReadAllBytes(regularPath), "Arial");
        var bold = LoadFace(File.ReadAllBytes(boldPath), "Arial-Bold");
        if (regular == null || bold == null) return null;

        var max = 0;
        void Track(string text) { foreach (var ch in text) if (ch > max) max = ch; }
        foreach (var part in parts) { Track(part.Label); Track(part.Value); }
        Track(snapshotId);
        Track("Audit Snapshot · Страница /");
        if (max < 32) max = 32;

        BuildGlyphMaps(regular, max);
        BuildGlyphMaps(bold, max);

        float Measure(string text, float size, bool useBold)
        {
            var face = useBold ? bold : regular;
            float w = 0;
            foreach (var ch in text)
            {
                face.Cmap.TryGetValue(ch, out var glyph);
                var adv = glyph > 0 && glyph < face.Advances.Length ? face.Advances[glyph] : face.Units / 2;
                w += adv * size / face.Units;
            }
            return w;
        }

        var pages = LayoutPages(parts, snapshotId, Measure);
        var objects = new List<byte[]>
        {
            Pdf("<< /Type /Catalog /Pages 2 0 R >>"),
            Array.Empty<byte>()
        };
        var kids = new StringBuilder();
        var pageIds = new List<int>();
        foreach (var _ in pages)
        {
            pageIds.Add(objects.Count + 1);
            objects.Add(Array.Empty<byte>());
            objects.Add(Array.Empty<byte>());
        }

        var regularFontId = objects.Count + 1;
        AddFontObjects(objects, regular, max, regularFontId);
        var boldFontId = objects.Count + 1;
        AddFontObjects(objects, bold, max, boldFontId);

        for (var i = 0; i < pages.Count; i++)
        {
            var pageId = pageIds[i];
            var contentId = pageId + 1;
            kids.Append(pageId).Append(" 0 R ");
            objects[pageId - 1] = Pdf($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {F(PageWidth)} {F(PageHeight)}] /Contents {contentId} 0 R /Resources << /Font << /F1 {regularFontId} 0 R /F2 {boldFontId} 0 R >> >> >>");
            objects[contentId - 1] = Stream(Encoding.ASCII.GetBytes(RenderPage(pages[i], snapshotId, i + 1, pages.Count)), null);
        }
        objects[1] = Pdf($"<< /Type /Pages /Count {pages.Count} /Kids [{kids}] >>");
        return Assemble(objects);
    }

    static List<List<PdfOp>> LayoutPages(IReadOnlyList<SnapshotPart> parts, string snapshotId, Func<string, float, bool, float> measure)
    {
        var drafts = new List<List<PdfOp>>();
        var page = new List<PdfOp>();
        var y = PageHeight - MarginTop;

        void NewPage()
        {
            drafts.Add(page);
            page = [];
            y = PageHeight - MarginTop - 28;
        }

        void Ensure(float need)
        {
            if (y - need < MarginBottom + 24) NewPage();
        }

        void Put(string text, float size, float x, float yy, bool bold = false, bool gray = false)
        {
            page.Add(new PdfOp.Text(text, size, x, yy, bold, gray));
        }

        void Rule(float yy, float width = 1.2f)
        {
            page.Add(new PdfOp.Rule(MarginLeft, yy, PageWidth - MarginRight, yy, width));
        }

        foreach (var part in parts)
        {
            switch (part.Kind)
            {
                case SnapshotPartKind.Title:
                    Ensure(44);
                    Put(part.Value ?? "", 22, MarginLeft, y, bold: true);
                    y -= 18;
                    Rule(y, 1.6f);
                    y -= 16;
                    break;
                case SnapshotPartKind.Subtitle:
                    Ensure(20);
                    Put(part.Value ?? "", 11, MarginLeft, y, gray: true);
                    y -= 20;
                    break;
                case SnapshotPartKind.Section:
                    Ensure(38);
                    y -= 10;
                    Put((part.Value ?? "").ToUpperInvariant(), 14, MarginLeft, y, bold: true);
                    y -= 8;
                    Rule(y, 1.4f);
                    y -= 18;
                    break;
                case SnapshotPartKind.Field:
                {
                    var label = part.Label.Trim();
                    var value = (part.Value ?? "").Replace("\r", " ").Replace("\n", " ").Replace(';', '·');
                    var emphasis = label is "Название" or "Мера" or "Статус";
                    var valueSize = emphasis ? 11f : 10f;
                    var labelText = label + ":";
                    var labelWidth = Math.Min(measure(labelText + " ", 10, true), 150f);
                    var valueWidth = ContentWidth - labelWidth - 6;
                    var rows = WrapText(value, valueWidth, valueSize, (t, s) => measure(t, s, emphasis));
                    if (rows.Count == 0) rows.Add("");
                    Ensure(15f * rows.Count + 6);
                    Put(labelText, 10, MarginLeft, y, bold: true, gray: true);
                    for (var i = 0; i < rows.Count; i++)
                        Put(rows[i], valueSize, MarginLeft + labelWidth + 6, y - i * 15, bold: emphasis);
                    y -= 15f * rows.Count + 6;
                    break;
                }
                case SnapshotPartKind.Note:
                {
                    var rows = WrapText(part.Value ?? "", ContentWidth, 10, (t, s) => measure(t, s, false));
                    Ensure(14f * Math.Max(rows.Count, 1) + 4);
                    foreach (var row in rows)
                    {
                        Put(row, 10, MarginLeft, y, gray: true);
                        y -= 14;
                    }
                    y -= 4;
                    break;
                }
                case SnapshotPartKind.Blank:
                    y -= 10;
                    break;
            }
        }
        drafts.Add(page);
        return drafts;
    }

    static string RenderPage(List<PdfOp> ops, string snapshotId, int pageNo, int pageCount)
    {
        var body = new StringBuilder();
        body.Append("0.82 0.84 0.87 RG 1 w\n");
        body.Append($"{F(MarginLeft)} {F(PageHeight - 36)} m {F(PageWidth - MarginRight)} {F(PageHeight - 36)} l S\n");
        body.Append("BT\n");
        body.Append(HexText($"Audit Snapshot · {snapshotId}", 9, MarginLeft, PageHeight - 28, false, true));
        body.Append(HexText($"Страница {pageNo} / {pageCount}", 9, PageWidth - MarginRight - 92, PageHeight - 28, false, true));
        body.Append("ET\n");
        foreach (var op in ops)
        {
            switch (op)
            {
                case PdfOp.Rule rule:
                    body.Append("0.70 0.73 0.78 RG ").Append(F(rule.Width)).Append(" w\n");
                    body.Append($"{F(rule.X1)} {F(rule.Y1)} m {F(rule.X2)} {F(rule.Y2)} l S\n");
                    break;
                case PdfOp.Text text:
                    body.Append("BT\n");
                    body.Append(HexText(text.Value, text.Size, text.X, text.Y, text.Bold, text.Gray));
                    body.Append("ET\n");
                    break;
            }
        }
        body.Append("0.82 0.84 0.87 RG 1 w\n");
        body.Append($"{F(MarginLeft)} {F(28)} m {F(PageWidth - MarginRight)} {F(28)} l S\n");
        return body.ToString();
    }

    static List<string> WrapText(string text, float maxWidth, float size, Func<string, float, float> measure)
    {
        var rows = new List<string>();
        if (string.IsNullOrEmpty(text)) return rows;
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return rows;
        var current = words[0];
        for (var i = 1; i < words.Length; i++)
        {
            var trial = current + " " + words[i];
            if (measure(trial, size) <= maxWidth) current = trial;
            else
            {
                rows.Add(current);
                current = words[i];
            }
        }
        rows.Add(current);
        var hard = new List<string>();
        foreach (var row in rows)
        {
            if (measure(row, size) <= maxWidth) { hard.Add(row); continue; }
            var chunk = "";
            foreach (var ch in row)
            {
                var trial = chunk + ch;
                if (chunk.Length > 0 && measure(trial, size) > maxWidth)
                {
                    hard.Add(chunk);
                    chunk = ch.ToString();
                }
                else chunk = trial;
            }
            if (chunk.Length > 0) hard.Add(chunk);
        }
        return hard;
    }

    static string HexText(string text, float size, float x, float y, bool bold, bool gray)
    {
        var color = gray ? "0.40 0.43 0.48 rg" : "0.10 0.12 0.16 rg";
        var font = bold ? "/F2" : "/F1";
        var hex = new StringBuilder();
        foreach (var ch in text) hex.Append(((int)ch).ToString("X4"));
        return $"{color}\n{font} {F(size)} Tf\n1 0 0 1 {F(x)} {F(y)} Tm\n<{hex}> Tj\n";
    }

    static void AddFontObjects(List<byte[]> objects, FontFace face, int max, int fontId)
    {
        var cidId = fontId + 1;
        var descriptorId = fontId + 2;
        var fileId = fontId + 3;
        var unicodeId = fontId + 4;
        var gidId = fontId + 5;
        objects.Add(Pdf($"<< /Type /Font /Subtype /Type0 /BaseFont /{face.Name} /Encoding /Identity-H /DescendantFonts [{cidId} 0 R] /ToUnicode {unicodeId} 0 R >>"));
        objects.Add(Pdf($"<< /Type /Font /Subtype /CIDFontType2 /BaseFont /{face.Name} /CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> /FontDescriptor {descriptorId} 0 R /DW 500 /W [{face.Widths}] /CIDToGIDMap {gidId} 0 R >>"));
        objects.Add(Pdf($"<< /Type /FontDescriptor /FontName /{face.Name} /Flags 32 /FontBBox [-600 -300 1500 1100] /ItalicAngle 0 /Ascent 900 /Descent -200 /CapHeight 700 /StemV {(face.Name.Contains("Bold") ? "140" : "80")} /FontFile2 {fileId} 0 R >>"));
        objects.Add(Stream(face.Bytes, face.Bytes.Length));
        objects.Add(Stream(Encoding.ASCII.GetBytes(ToUnicode(max)), null));
        objects.Add(Stream(face.GidMap!, null));
    }

    static FontFace? LoadFace(byte[] font, string name)
    {
        var cmap = Cmap(font);
        if (cmap.Count == 0) return null;
        return new FontFace(name, font, cmap, Units(font), Advances(font));
    }

    static void BuildGlyphMaps(FontFace face, int max)
    {
        face.GidMap = new byte[(max + 1) * 2];
        var widths = new StringBuilder();
        for (var code = 0; code <= max; code++)
        {
            face.Cmap.TryGetValue(code, out var glyph);
            face.GidMap[code * 2] = (byte)(glyph >> 8);
            face.GidMap[code * 2 + 1] = (byte)glyph;
            if (glyph <= 0 || glyph >= face.Advances.Length) continue;
            var width = face.Advances[glyph] * 1000 / face.Units;
            if (widths.Length > 0) widths.Append(' ');
            widths.Append(code).Append(" [").Append(width).Append(']');
        }
        face.Widths = widths.ToString();
    }

    static string F(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    sealed class FontFace(string name, byte[] bytes, Dictionary<int, int> cmap, int units, int[] advances)
    {
        public string Name { get; } = name;
        public byte[] Bytes { get; } = bytes;
        public Dictionary<int, int> Cmap { get; } = cmap;
        public int Units { get; } = units;
        public int[] Advances { get; } = advances;
        public byte[]? GidMap { get; set; }
        public string Widths { get; set; } = "";
    }

    abstract record PdfOp
    {
        public sealed record Text(string Value, float Size, float X, float Y, bool Bold, bool Gray) : PdfOp;
        public sealed record Rule(float X1, float Y1, float X2, float Y2, float Width) : PdfOp;
    }

    static string ToUnicode(int max)
    {
        var body = new StringBuilder("/CIDInit /ProcSet findresource begin\n12 dict begin\nbegincmap\n/CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def\n/CMapName /Adobe-Identity-UCS def\n/CMapType 2 def\n1 begincodespacerange\n<0000> <FFFF>\nendcodespacerange\n");
        var count = 0;
        for (var code = 0; code <= max; code++)
        {
            if (count % 100 == 0)
            {
                if (count > 0) body.Append("endbfchar\n");
                var take = Math.Min(100, max - code + 1);
                body.Append(take).Append(" beginbfchar\n");
            }
            body.Append('<').Append(code.ToString("X4")).Append("> <").Append(code.ToString("X4")).Append(">\n");
            count++;
        }
        if (count > 0) body.Append("endbfchar\n");
        body.Append("endcmap\nCMapName currentdict /CMap defineresource pop\nend\nend\n");
        return body.ToString();
    }

    static byte[] Assemble(List<byte[]> objects)
    {
        using var stream = new MemoryStream();
        void Raw(string text) => stream.Write(Encoding.ASCII.GetBytes(text));
        Raw("%PDF-1.4\n");
        var offsets = new int[objects.Count + 1];
        for (var i = 0; i < objects.Count; i++)
        {
            offsets[i + 1] = (int)stream.Position;
            Raw($"{i + 1} 0 obj\n");
            stream.Write(objects[i]);
            Raw("\nendobj\n");
        }
        var xref = (int)stream.Position;
        Raw($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        for (var i = 1; i <= objects.Count; i++) Raw(offsets[i].ToString("D10") + " 00000 n \n");
        Raw($"trailer << /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        return stream.ToArray();
    }

    static byte[] Pdf(string body) => Encoding.ASCII.GetBytes(body);

    static byte[] Stream(byte[] data, int? length1)
    {
        var header = length1 == null
            ? $"<< /Length {data.Length} >>\nstream\n"
            : $"<< /Length {data.Length} /Length1 {length1} >>\nstream\n";
        var tail = Encoding.ASCII.GetBytes("\nendstream");
        var head = Encoding.ASCII.GetBytes(header);
        var all = new byte[head.Length + data.Length + tail.Length];
        head.CopyTo(all, 0);
        data.CopyTo(all, head.Length);
        tail.CopyTo(all, head.Length + data.Length);
        return all;
    }

    static Dictionary<int, int> Cmap(byte[] font)
    {
        var map = new Dictionary<int, int>();
        var cmap = Table(font, "cmap");
        if (cmap < 0) return map;
        var tables = U16(font, cmap + 2);
        var chosen = -1;
        for (var i = 0; i < tables; i++)
        {
            var rec = cmap + 4 + i * 8;
            var formatAt = cmap + (int)U32(font, rec + 4);
            if (U16(font, rec) == 3 && U16(font, rec + 2) == 1 && U16(font, formatAt) == 4) chosen = formatAt;
        }
        if (chosen < 0) return map;
        var segments = U16(font, chosen + 6) / 2;
        var endAt = chosen + 14;
        var startAt = endAt + segments * 2 + 2;
        var deltaAt = startAt + segments * 2;
        var rangeAt = deltaAt + segments * 2;
        for (var i = 0; i < segments; i++)
        {
            var end = U16(font, endAt + i * 2);
            var start = U16(font, startAt + i * 2);
            var delta = (short)U16(font, deltaAt + i * 2);
            var range = U16(font, rangeAt + i * 2);
            for (var code = start; code <= end && code != 0xFFFF; code++)
            {
                int glyph;
                if (range == 0) glyph = (code + delta) & 0xFFFF;
                else
                {
                    glyph = U16(font, rangeAt + i * 2 + range + (code - start) * 2);
                    if (glyph != 0) glyph = (glyph + delta) & 0xFFFF;
                }
                if (glyph != 0) map[code] = glyph;
            }
        }
        return map;
    }

    static int Units(byte[] font)
    {
        var head = Table(font, "head");
        return head < 0 ? 2048 : U16(font, head + 18);
    }

    static int[] Advances(byte[] font)
    {
        var hhea = Table(font, "hhea");
        var hmtx = Table(font, "hmtx");
        var count = hhea < 0 ? 0 : U16(font, hhea + 34);
        var maxp = Table(font, "maxp");
        var glyphs = maxp < 0 ? count : U16(font, maxp + 4);
        var advances = new int[Math.Max(glyphs, 1)];
        var last = 500;
        for (var i = 0; i < glyphs; i++)
        {
            var pos = i < count ? hmtx + i * 4 : hmtx + (count - 1) * 4;
            last = pos < 0 || pos + 1 >= font.Length ? last : U16(font, pos);
            advances[i] = last;
        }
        return advances;
    }

    static int Table(byte[] font, string tag)
    {
        var count = U16(font, 4);
        for (var i = 0; i < count; i++)
        {
            var rec = 12 + i * 16;
            if (Encoding.ASCII.GetString(font, rec, 4) == tag) return (int)U32(font, rec + 8);
        }
        return -1;
    }

    static int U16(byte[] font, int at) => (font[at] << 8) | font[at + 1];
    static uint U32(byte[] font, int at) => ((uint)font[at] << 24) | ((uint)font[at + 1] << 16) | ((uint)font[at + 2] << 8) | font[at + 3];
}
