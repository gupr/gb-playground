using System;

public class CliArguments
{
    public string RomPath = "";

    public bool Atlas = false;
    public bool Corrupt = false;
    public bool Diff = false;

    public CorruptionMode Mode = CorruptionMode.BitFlip;

    public double Percent = 5;
    public double Intensity = 0.3;

    public uint Seed = 0;

    public int StartBank = -1;
    public int EndBank = -1;

    /*
        Parses command line arguments into a usable structure.
    */
    public static CliArguments Parse(string[] args)
    {
        CliArguments cli = new CliArguments();

        if (args.Length == 0)
            throw new Exception("No ROM specified.");
            

        cli.RomPath = args[0];

        for (int i = 1; i < args.Length; i++)
        {
            string arg = args[i].ToLower();

            switch (arg)
            {
                case "--atlas":
                    cli.Atlas = true;
                    break;

                case "--corrupt":
                    cli.Corrupt = true;
                    break;

                case "--diff":
                    cli.Diff = true;
                    break;

                case "--percent":
                    cli.Percent = double.Parse(args[++i]);
                    break;

                case "--intensity":
                    cli.Intensity = double.Parse(args[++i]);
                    break;

                case "--seed":
                    cli.Seed = uint.Parse(args[++i]);
                    break;

                case "--mode":
                    cli.Mode = ParseMode(args[++i]);
                    break;

                case "--banks":
                    ParseBankRange(args[++i], cli);
                    break;
            }
        }

        return cli;
    }

    /*
        Converts string into corruption mode enum.
    */
    private static CorruptionMode ParseMode(string mode)
    {
        mode = mode.ToLower();

        return mode switch
        {
            "bitflip" => CorruptionMode.BitFlip,
            "random" => CorruptionMode.ByteRandom,
            "add" => CorruptionMode.ByteAdd,
            "xor" => CorruptionMode.ByteXor,
            "shuffle" => CorruptionMode.TileShuffle,
            _ => CorruptionMode.BitFlip
        };
    }

    /*
        Parses bank range string like "20-40".
    */
    private static void ParseBankRange(string text, CliArguments cli)
    {
        string[] parts = text.Split('-');

        if (parts.Length != 2)
            return;

        cli.StartBank = int.Parse(parts[0]);
        cli.EndBank = int.Parse(parts[1]);
    }
}