using System;
using System.IO;
using System.Text;

namespace GbPlayground
{

    public class RomReader
    {
        private const int MIN_ROM_SIZE = 32768;

        private byte[] romData = Array.Empty<byte>();

        public bool IsValid { get; private set; } = false;

        public byte[] Data => romData;

        public RomReader(string filePath)
        {
            LoadFile(filePath);
        }

        private void LoadFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    IsValid = false;
                    return;
                }

                byte[] fileBytes = File.ReadAllBytes(filePath);

                if (fileBytes.Length < MIN_ROM_SIZE)
                {
                    IsValid = false;
                    return;
                }

                romData = fileBytes;
                IsValid = true;
            }
            catch
            {
                IsValid = false;
            }
        }
    }

    public class RomHeader
    {
        // Offsets
        private const int TITLE_START = 0x134;
        private const int TITLE_END = 0x143;
        private const int CARTRIDGE_TYPE_OFFSET = 0x147;
        private const int ROM_SIZE_OFFSET = 0x148;
        private const int DESTINATION_OFFSET = 0x14A;

        private const int HEADER_CHECKSUM_OFFSET = 0x14D;
        private const int CHECKSUM_START = 0x134;
        private const int CHECKSUM_END = 0x14C;

        private const int LOGO_OFFSET = 0x0104;

        private static readonly byte[] NintendoLogo =
        {
            0xCE,0xED,0x66,0x66,0xCC,0x0D,0x00,0x0B,
            0x03,0x73,0x00,0x83,0x00,0x0C,0x00,0x0D,
            0x00,0x08,0x11,0x1F,0x88,0x89,0x00,0x0E,
            0xDC,0xCC,0x6E,0xE6,0xDD,0xDD,0xD9,0x99,
            0xBB,0xBB,0x67,0x63,0x6E,0x0E,0xEC,0xCC,
            0xDD,0xDC,0x99,0x9F,0xBB,0xB9,0x33,0x3E
        };

        public string Title { get; private set; } = "";
        public byte CartridgeType { get; private set; }
        public byte RomSizeCode { get; private set; }
        public byte Destination { get; private set; }

        public bool ChecksumValid { get; private set; }
        public bool LogoValid { get; private set; }

        public RomHeader(byte[] romData)
        {
            if (romData.Length < 0x150)
                throw new Exception("ROM too small for header");

            ParseHeader(romData);
        }

        private void ParseHeader(byte[] romData)
        {
            // Title
            StringBuilder sb = new StringBuilder();

            for (int i = TITLE_START; i <= TITLE_END; i++)
            {
                byte b = romData[i];

                if (b == 0)
                    break;

                sb.Append((char)b);
            }

            Title = sb.ToString();

            CartridgeType = romData[CARTRIDGE_TYPE_OFFSET];
            RomSizeCode = romData[ROM_SIZE_OFFSET];
            Destination = romData[DESTINATION_OFFSET];

            ChecksumValid = ValidateChecksum(romData);
            LogoValid = ValidateNintendoLogo(romData);
        }

        private bool ValidateChecksum(byte[] romData)
        {
            byte checksum = 0;

            for (int i = CHECKSUM_START; i <= CHECKSUM_END; i++)
            {
                checksum = (byte)(checksum - romData[i] - 1);
            }

            return checksum == romData[HEADER_CHECKSUM_OFFSET];
        }

        private bool ValidateNintendoLogo(byte[] romData)
        {
            for (int i = 0; i < NintendoLogo.Length; i++)
            {
                if (romData[LOGO_OFFSET + i] != NintendoLogo[i])
                    return false;
            }

            return true;
        }

        public long GetRomSizeBytes()
        {
            if (RomSizeCode <= 7)
                return 32L * 1024 << RomSizeCode;

            return 0;
        }
    }
}