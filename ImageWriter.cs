using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

public static class ImageWriter
{
    private const int FONT_SIZE = 40;

    private static int GetFooterHeight()
    {
        return FONT_SIZE + 40;
    }

    private static Font LoadFont()
    {
        try
        {
            string fontPath = Path.Combine(
                AppContext.BaseDirectory,
                "Fonts",
                "font.ttf");

            if (!File.Exists(fontPath))
                return new Font("Arial", FONT_SIZE);

            PrivateFontCollection fonts = new PrivateFontCollection();
            fonts.AddFontFile(fontPath);

            return new Font(fonts.Families[0], FONT_SIZE, FontStyle.Regular);
        }
        catch
        {
            return new Font("Arial", FONT_SIZE);
        }
    }

    public static void WritePng(
        string path,
        List<Pixel> pixels,
        int width,
        int height,
        string title,
        long romSizeBytes)
    {
        if (pixels.Count == 0 || width <= 0 || height <= 0)
            throw new Exception("Invalid image data.");

        int footerHeight = GetFooterHeight();
        int finalHeight = height + footerHeight;

        using Bitmap bitmap = new Bitmap(width, finalHeight);

        // ======================
        // Draw tile image
        // ======================

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Pixel p = pixels[y * width + x];
                Color color = Color.FromArgb(p.R, p.G, p.B);
                bitmap.SetPixel(x, y, color);
            }
        }

        using Graphics g = Graphics.FromImage(bitmap);

        // ======================
        // Footer background
        // ======================

        using (Brush footerBrush = new SolidBrush(Color.FromArgb(35, 35, 35)))
        {
            g.FillRectangle(
                footerBrush,
                0,
                height,
                width,
                footerHeight);
        }

        // Divider line
        using (Pen divider = new Pen(Color.FromArgb(200, 200, 200)))
        {
            g.DrawLine(divider, 0, height, width, height);
        }

        // ======================
        // Footer text
        // ======================

        string text = $"{title}  |  {romSizeBytes / 1024} KB";

        using Font font = LoadFont();
        using Brush brush = new SolidBrush(Color.White);

        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        SizeF textSize = g.MeasureString(text, font);

        float textX = (width - textSize.Width) / 2;
        float textY = height + (footerHeight - textSize.Height) / 2;

        g.DrawString(
            text,
            font,
            brush,
            textX,
            textY);

        // ======================
        // Save PNG
        // ======================

        bitmap.Save(path, ImageFormat.Png);
    }

    /*
        Writes a tile image of the corrupted ROM and highlights
        changed pixels in red compared to the original ROM.
        Also shows corruption statistics in the footer.
    */
    public static void WriteCorruptionDiff(
        string path,
        byte[] originalRom,
        byte[] corruptedRom,
        string title,
        long romSizeBytes)
    {
        int tilesPerRow = (int)Math.Sqrt(originalRom.Length / 16);

        int widthA, heightA;
        int widthB, heightB;

        var originalImage = TileDecoder.DecodeAllTiles(
            originalRom,
            tilesPerRow,
            out widthA,
            out heightA);

        var corruptedImage = TileDecoder.DecodeAllTiles(
            corruptedRom,
            tilesPerRow,
            out widthB,
            out heightB);

        int width = widthA;
        int height = heightA;

        int footerHeight = GetFooterHeight();
        int finalHeight = height + footerHeight;

        int pixelsChanged = 0;

        using Bitmap bitmap = new Bitmap(width, finalHeight);

        // ======================
        // Diff pixels
        // ======================

        for (int i = 0; i < originalImage.Count; i++)
        {
            int x = i % width;
            int y = i / width;

            Pixel a = originalImage[i];
            Pixel b = corruptedImage[i];

            Color color;

            if (a.R != b.R || a.G != b.G || a.B != b.B)
            {
                color = Color.Red;
                pixelsChanged++;
            }
            else
            {
                color = Color.FromArgb(b.R, b.G, b.B);
            }

            bitmap.SetPixel(x, y, color);
        }

        // ======================
        // Compute statistics
        // ======================

        int bytesChanged = 0;

        int length = Math.Min(originalRom.Length, corruptedRom.Length);

        for (int i = 0; i < length; i++)
        {
            if (originalRom[i] != corruptedRom[i])
                bytesChanged++;
        }

        // ======================
        // Footer
        // ======================

        using Graphics g = Graphics.FromImage(bitmap);

        g.FillRectangle(
            new SolidBrush(Color.FromArgb(35, 35, 35)),
            0,
            height,
            width,
            footerHeight);

        g.DrawLine(
            new Pen(Color.FromArgb(200, 200, 200)),
            0,
            height,
            width,
            height);

        // ======================
        // Load custom font
        // ======================

        using Font font = LoadFont();

        string text =
            $"{title} - Bytes affected: {bytesChanged}";

        SizeF textSize = g.MeasureString(text, font);

        float textX = (width - textSize.Width) / 2;
        float textY = height + (footerHeight - textSize.Height) / 2;

        g.DrawString(
            text,
            font,
            Brushes.White,
            textX,
            textY);

        bitmap.Save(path, ImageFormat.Png);
    }
}