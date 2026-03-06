using GbPlayground;
using System;
using System.IO;
using System.Reflection.PortableExecutable;
using static System.Net.WebRequestMethods;

/*     Main entry point for the application. Handles command line arguments and orchestrates
 *     the ROM reading, corruption, and image writing.
*/

class Program
{
    static void Main(string[] args)
    {
        CliArguments cli;

        try
        {
            cli = CliArguments.Parse(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        RomReader reader = new RomReader(cli.RomPath);

        if (!reader.IsValid)
        {
            Console.WriteLine("Invalid ROM");
            return;
        }

        byte[] romData = reader.Data;

        var header = new RomHeader(reader.Data);

        Console.WriteLine($"Title: {header.Title}");
        Console.WriteLine($"Cartridge type: {header.CartridgeType:X}");
        Console.WriteLine($"ROM Size: {header.GetRomSizeBytes() / 1024} KB");
        Console.WriteLine($"Destination: {header.Destination:X}");
        Console.WriteLine($"Checksum valid: {header.ChecksumValid}");
        Console.WriteLine($"Nintendo logo valid: {header.LogoValid}");

        string fontPath = Path.Combine(
            AppContext.BaseDirectory,
            "Fonts",
            "font.ttf"
        );

        // Write full ROM image as tiles
        if (cli.Atlas)
        {
            WriteFullRomImage(
                romData,
                header.Title,
                (int)header.GetRomSizeBytes());
        }

        // Corrupt ROM and write to disk
        if (cli.Corrupt)
        {
            byte[] corrupted = (byte[])romData.Clone();

            RomCorruptor.Corrupt(
                corrupted,
                cli.Percent,
                cli.Intensity,
                cli.Mode,
                cli.Seed,
                cli.StartBank,
                cli.EndBank);

            string outName = header.Title + "_corrupted.gb";

            System.IO.File.WriteAllBytes(outName, corrupted);

            Console.WriteLine("Wrote " + outName);

            // View diff of original vs corrupted ROM as image
            if (cli.Diff)
            {
                ImageWriter.WriteCorruptionDiff(
                    header.Title + "_diff.png",
                    romData,
                    corrupted,
                    header.Title,
                    header.GetRomSizeBytes());
            }
        }
    }

    // Writes an image of all the tiles in the ROM, arranged in a grid. Useful for visualizing the contents of the ROM and how corruption affects it.
    static void WriteFullRomImage(byte[] romData, string title, long romSize)
    {
        int tileCount = romData.Length / 16;

        if (tileCount == 0)
            return;

        // Make atlas as square as possible
        int tilesPerRow = (int)Math.Ceiling(Math.Sqrt(tileCount));

        int width;
        int height;

        var image = TileDecoder.DecodeAllTiles(
            romData,
            tilesPerRow,
            out width,
            out height);

        Console.WriteLine("Writing output image...");

        string outputName = title + "_full.png";

        ImageWriter.WritePng(
            outputName,
            image,
            width,
            height,
            title,
            romSize);

        Console.WriteLine("Wrote " + outputName);
    }
}