using com.clusterrr.Famicom.Containers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace NROMToMapper474
{
    class Program
    {
        static int Main(string[] args)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddCommandLine(args);
            IConfiguration configuration = configurationBuilder.Build();

            string input = configuration["input"];
            string output = configuration["output"];
            string strSubmapper = configuration["submapper"];

            bool showHelp = false;

            int submapper = 0;
            if (!int.TryParse(strSubmapper, out submapper) || submapper < 0 || submapper > 1)
            {
                System.Console.Error.WriteLine("--submapper should be 0 or 1");
                showHelp = true;
            }

            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(output))
            {
                showHelp = true;
            }

            if (showHelp)
            {
                System.Console.Out.WriteLine("usage NROMToMapper474 --input=myrom.nes --output=mapper474.nes --submapper=0|1");
                return 1;
            }

            var nesInput = NesFile.FromFile(input);
            var nesOutput = new NesFile();

            if (nesInput.Battery)
            {
                System.Console.Error.WriteLine("ROM with battery present is not supported");
                return 1;
            }
            if (nesInput.ChrNvRamSize != 0 || nesInput.ChrRamSize != 0)
            {
                System.Console.Error.WriteLine("ROM with RAM is not supported");
                return 1;
            }
            if (nesInput.Console != NesFile.ConsoleType.Normal)
            {
                System.Console.Error.WriteLine("Only NES or FAMICOM consoles are supported");
                return 1;
            }
            if (nesInput.ExtendedConsole != NesFile.ExtendedConsoleType.RegularNES && nesInput.ExtendedConsole != NesFile.ExtendedConsoleType.UMC_UM6578)
            {
                System.Console.Error.WriteLine("Only NES or FAMICOM consoles are supported");
                return 1;
            }
            if (nesInput.Mapper != 0)
            {
                System.Console.Error.WriteLine("Only NROM Mapper 0 is supported for input");
                return 1;
            }
            if (nesInput.PRG.Length > 32768)
            {
                System.Console.Error.WriteLine("Input PRG size is too large");
                return 1;
            }
            if (nesInput.CHR.Length > 8192)
            {
                System.Console.Error.WriteLine("Input CHR size is too large");
                return 1;
            }

            nesOutput.Mapper = 474;
            nesOutput.Submapper = (byte)submapper;
            nesOutput.CHR = nesInput.CHR;
            byte[] arrayOfZeros = (byte[])Array.CreateInstance(typeof(byte), 1 << 14);
            Array.Fill(arrayOfZeros, (byte)0);

            List<byte> prg = new List<byte>(arrayOfZeros);
            prg.AddRange(nesInput.PRG);
            nesOutput.PRG = prg.ToArray();
            nesOutput.Console = nesInput.Console;
            nesOutput.ExtendedConsole = nesInput.ExtendedConsole;
            nesOutput.Version = NesFile.iNesVersion.NES20;
            nesOutput.Region = nesInput.Region;
            nesOutput.Mirroring = nesInput.Mirroring;
            nesOutput.DefaultExpansionDevice = nesInput.DefaultExpansionDevice;
            nesOutput.Save(output);

            return 0;
        }
    }
}
