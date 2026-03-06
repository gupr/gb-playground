using System;
using System.Collections.Generic;

public static class TileEntropy
{
    const int TILE_SIZE = 16;

    /*
        Generates an entropy value for every tile (16 bytes).
        Lower entropy = more structured (likely graphics).
        Higher entropy = more random (likely code/data).
    */
    public static List<float> Calculate(byte[] rom)
    {
        int tileCount = rom.Length / TILE_SIZE;

        var entropy = new List<float>(tileCount);

        for (int t = 0; t < tileCount; t++)
        {
            int offset = t * TILE_SIZE;

            int[] histogram = new int[256];

            for (int i = 0; i < TILE_SIZE; i++)
            {
                histogram[rom[offset + i]]++;
            }

            float e = 0f;

            for (int i = 0; i < 256; i++)
            {
                if (histogram[i] == 0)
                    continue;

                float p = histogram[i] / (float)TILE_SIZE;

                e -= p * (float)Math.Log(p, 2);
            }

            entropy.Add(e);
        }

        return entropy;
    }
}