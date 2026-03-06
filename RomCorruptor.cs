using System;

public enum CorruptionMode
{
    BitFlip,
    ByteRandom,
    ByteAdd,
    ByteXor,
    TileShuffle,
    TileCopy
}

public static class RomCorruptor
{
    private const int SAFE_ZONE_END = 0x10000;
    private const int TILE_SIZE = 16;
    private const int BANK_SIZE = 0x4000;

    /*
    Applies corruption to the ROM data.

    percent     = percentage of tiles to corrupt
    intensity   = probability of modifying each byte inside a tile
    mode        = corruption algorithm
    seed        = optional deterministic RNG seed
    startBank   = optional start bank for corruption range
    endBank     = optional end bank for corruption range
    weighted    = increase corruption probability in later banks
    */
    public static void Corrupt(
        byte[] romData,
        double percent,
        double intensity,
        CorruptionMode mode,
        uint seed = 0,
        int startBank = -1,
        int endBank = -1,
        bool weighted = true)
    {
        if (percent <= 0.0 || percent > 100.0)
            return;

        int startOffset = SAFE_ZONE_END;
        int endOffset = romData.Length;

        // Optional bank range override
        if (startBank >= 0 && endBank >= startBank)
        {
            startOffset = startBank * BANK_SIZE;
            endOffset = (endBank + 1) * BANK_SIZE;

            if (startOffset < SAFE_ZONE_END)
                startOffset = SAFE_ZONE_END;

            if (endOffset > romData.Length)
                endOffset = romData.Length;
        }

        int regionSize = endOffset - startOffset;
        int totalTiles = regionSize / TILE_SIZE;

        if (totalTiles <= 0)
            return;

        int tilesToCorrupt = (int)((percent / 100.0) * totalTiles);

        if (tilesToCorrupt == 0)
            return;

        Random rng = seed != 0 ? new Random((int)seed) : new Random();

        // Create tile index list
        int[] indices = new int[totalTiles];
        for (int i = 0; i < totalTiles; i++)
            indices[i] = i;

        // Shuffle indices (Fisher-Yates)
        for (int i = totalTiles - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            int tmp = indices[i];
            indices[i] = indices[j];
            indices[j] = tmp;
        }

        // Corrupt selected tiles
        for (int n = 0; n < tilesToCorrupt; n++)
        {
            int tileIndex = indices[n];

            int offset = startOffset + tileIndex * TILE_SIZE;

            int bank = offset / BANK_SIZE;

            double weight = 1.0;

            if (weighted)
                weight = Math.Min(1.0, bank / 32.0);

            if (rng.NextDouble() > weight)
                continue;

            switch (mode)
            {
                case CorruptionMode.BitFlip:
                    BitFlip(romData, offset, intensity, rng);
                    break;

                case CorruptionMode.ByteRandom:
                    ByteRandom(romData, offset, intensity, rng);
                    break;

                case CorruptionMode.ByteAdd:
                    ByteAdd(romData, offset, intensity, rng);
                    break;

                case CorruptionMode.ByteXor:
                    ByteXor(romData, offset, intensity, rng);
                    break;

                case CorruptionMode.TileShuffle:
                    TileShuffle(romData, offset, startOffset, totalTiles, rng);
                    break;
                case CorruptionMode.TileCopy:
                    TileCopy(romData, offset, startOffset, totalTiles, rng);
                    break;
            }
        }
    }

    /*
    Flips a random bit in selected bytes.
    Produces subtle graphical glitches.
    */
    private static void BitFlip(byte[] rom, int offset, double intensity, Random rng)
    {
        for (int i = 0; i < TILE_SIZE; i++)
        {
            if (rng.NextDouble() < intensity)
            {
                int bit = rng.Next(8);
                rom[offset + i] ^= (byte)(1 << bit);
            }
        }
    }

    /*
        Replaces selected bytes with completely random values.
        Produces heavy corruption.
    */
    private static void ByteRandom(byte[] rom, int offset, double intensity, Random rng)
    {
        for (int i = 0; i < TILE_SIZE; i++)
        {
            if (rng.NextDouble() < intensity)
                rom[offset + i] = (byte)rng.Next(256);
        }
    }

    /*
        Adds a small random value to selected bytes.
        Produces softer distortions than full randomization.
    */
    private static void ByteAdd(byte[] rom, int offset, double intensity, Random rng)
    {
        for (int i = 0; i < TILE_SIZE; i++)
        {
            if (rng.NextDouble() < intensity)
                rom[offset + i] += (byte)rng.Next(1, 32);
        }
    }

    /*
        XORs selected bytes with a random value.
        Produces chaotic but often stable corruption.
    */
    private static void ByteXor(byte[] rom, int offset, double intensity, Random rng)
    {
        for (int i = 0; i < TILE_SIZE; i++)
        {
            if (rng.NextDouble() < intensity)
                rom[offset + i] ^= (byte)rng.Next(256);
        }
    }

    /*
        Swaps this tile with another random tile.
        Rearranges graphics instead of altering pixel data.
    */
    private static void TileShuffle(byte[] rom, int offset, int startOffset, int totalTiles, Random rng)
    {
        int otherTile = rng.Next(totalTiles);
        int otherOffset = startOffset + otherTile * TILE_SIZE;

        for (int i = 0; i < TILE_SIZE; i++)
        {
            byte tmp = rom[offset + i];
            rom[offset + i] = rom[otherOffset + i];
            rom[otherOffset + i] = tmp;
        }
    }

    /*
    Copies another random tile into this tile.
    Creates sprite swaps and graphical glitches.
    */
    private static void TileCopy(byte[] rom, int offset, int startOffset, int totalTiles, Random rng)
    {
        int otherTile = rng.Next(totalTiles);
        int otherOffset = startOffset + otherTile * TILE_SIZE;

        for (int i = 0; i < TILE_SIZE; i++)
        {
            rom[offset + i] = rom[otherOffset + i];
        }
    }
}


