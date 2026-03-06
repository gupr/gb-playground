using System;
using System.Collections.Generic;

public struct Pixel
{
    public byte R;
    public byte G;
    public byte B;

    public Pixel(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }
}

public static class TileDecoder
{
    public static List<Pixel> DecodeAllTiles(
        byte[] rom,
        int tilesPerRow,
        out int outWidth,
        out int outHeight)
    {
        if (tilesPerRow <= 0)
            throw new Exception("tilesPerRow must be > 0");

        const int bytesPerTile = 16;
        const int tileSize = 8;

        int tileCount = rom.Length / bytesPerTile;

        if (tileCount == 0)
            throw new Exception("ROM too small to contain tiles.");

        int tileRows = (tileCount + tilesPerRow - 1) / tilesPerRow;

        outWidth = tilesPerRow * tileSize;
        outHeight = tileRows * tileSize;

        var image = new List<Pixel>(outWidth * outHeight);

        // Initialize buffer
        for (int i = 0; i < outWidth * outHeight; i++)
            image.Add(new Pixel(255, 255, 255));

        for (int tileIndex = 0; tileIndex < tileCount; tileIndex++)
        {
            int baseIndex = tileIndex * bytesPerTile;

            int tileX = tileIndex % tilesPerRow;
            int tileY = tileIndex / tilesPerRow;

            for (int row = 0; row < 8; row++)
            {
                byte byte1 = rom[baseIndex + row * 2];
                byte byte2 = rom[baseIndex + row * 2 + 1];

                for (int bit = 7; bit >= 0; bit--)
                {
                    byte bit0 = (byte)((byte1 >> bit) & 1);
                    byte bit1 = (byte)((byte2 >> bit) & 1);

                    byte colorIndex = (byte)((bit1 << 1) | bit0);

                    byte shade;

                    switch (colorIndex)
                    {
                        case 0: shade = 255; break;
                        case 1: shade = 170; break;
                        case 2: shade = 85; break;
                        case 3: shade = 0; break;
                        default: shade = 255; break;
                    }

                    int pixelX = tileX * 8 + (7 - bit);
                    int pixelY = tileY * 8 + row;

                    int pixelIndex = pixelY * outWidth + pixelX;

                    if (pixelIndex < image.Count)
                    {
                        image[pixelIndex] = new Pixel(shade, shade, shade);
                    }
                }
            }
        }

        return image;
    }
}