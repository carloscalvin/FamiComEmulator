using System.Text.RegularExpressions;

namespace FamiComEmulator.Tests.TestRoms
{
    public static class NestestParser
    {
        public static List<CpuPpuState> ParseLog(string logFilePath)
        {
            var cpuPpuStates = new List<CpuPpuState>();
            var logLines = File.ReadAllLines(logFilePath);
            var regex = new Regex(@"^(?<Address>[0-9A-F]{4})\s+(?<Opcode>(?:[0-9A-F]{2}\s*){1,3})\s+(?<Instruction>.*?)\s+A:(?<A>[0-9A-F]{2})\s+X:(?<X>[0-9A-F]{2})\s+Y:(?<Y>[0-9A-F]{2})\s+P:(?<P>[0-9A-F]{2})\s+SP:(?<SP>[0-9A-F]{2})\s+PPU:\s*(?<PPUY>\d+),\s*(?<PPUX>\d+)\s+CYC:(?<CYC>\d+)$");

            foreach (var line in logLines)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    var state = new CpuPpuState
                    {
                        Address = ushort.Parse(match.Groups["Address"].Value, System.Globalization.NumberStyles.HexNumber),
                        Opcode = match.Groups["Opcode"].Value.Trim(),
                        Instruction = match.Groups["Instruction"].Value.Trim(),
                        Accumulator = byte.Parse(match.Groups["A"].Value, System.Globalization.NumberStyles.HexNumber),
                        XRegister = byte.Parse(match.Groups["X"].Value, System.Globalization.NumberStyles.HexNumber),
                        YRegister = byte.Parse(match.Groups["Y"].Value, System.Globalization.NumberStyles.HexNumber),
                        Status = byte.Parse(match.Groups["P"].Value, System.Globalization.NumberStyles.HexNumber),
                        StackPointer = byte.Parse(match.Groups["SP"].Value, System.Globalization.NumberStyles.HexNumber),
                        PpuX = int.Parse(match.Groups["PPUX"].Value),
                        PpuY = int.Parse(match.Groups["PPUY"].Value),
                        Cycle = int.Parse(match.Groups["CYC"].Value)
                    };
                    cpuPpuStates.Add(state);
                }
            }

            return cpuPpuStates;
        }
    }
}
