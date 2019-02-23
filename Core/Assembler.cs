using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Core
{
    public struct AssemblyResult
    {
        public byte[] Data;
        public Dictionary<int, byte> SourceMap;
    }

    public class Assembler
    {
        static Dictionary<string, byte> RegisterOffsets = new Dictionary<string, byte>();
        static Dictionary<string, byte> RegisterPairOffsets = new Dictionary<string, byte>();
        static Dictionary<string, byte> StackRegisterOffsets = new Dictionary<string, byte>();

        static byte Parse8BitValue(string value)
        {
            var input = value;
            var style = NumberStyles.None;

            if (input.EndsWith('h'))
            {
                input = value.Substring(0, value.Length - 1);
                style = NumberStyles.HexNumber;
            }

            short result;
            if (!short.TryParse(input, style, CultureInfo.InvariantCulture, out result))
            {
                throw new InvalidDataException($"Failed to parse 16bit address ${input}");
            }

            return (byte)(result & 0xFF);
        }

        static byte[] Parse16BitValue(string value)
        {
            var input = value;
            var style = NumberStyles.None;

            if (input.EndsWith('h'))
            {
                input = value.Substring(0, value.Length - 1);
                style = NumberStyles.HexNumber;
            }

            short result;
            if (!short.TryParse(input, style, CultureInfo.InvariantCulture, out result))
            {
                throw new InvalidDataException($"Failed to parse 16bit address ${input}");
            }

            return new byte[] { (byte)(result & 0xFF), (byte)(result >> 8 & 0xFF) };
        }

        static byte OpcodeWithRegister(string register, byte baseValue, byte multiplier)
        {
            if (!RegisterOffsets.ContainsKey(register))
            {
                throw new InvalidDataException($"Expected valid register name, found {register}");
            }

            return (byte)(baseValue + RegisterOffsets[register] * multiplier);
        }

        private byte[] OpcodeWithRegisterPair(string[] args, byte opcode, bool bdOnly = false)
        {
            ThrowIfInvalidNumberOfOperands(args, 1);

            if (!RegisterPairOffsets.ContainsKey(args[0]))
            {
                throw new InvalidDataException($"Expected register pair B/D/H/SP, found {args[0]}");
            }

            if (bdOnly && args[0] != "b" && args[0] != "d")
            {
                throw new InvalidDataException($"Expected register pair B/D, found {args[0]}");
            }

            return new byte[] { (byte)(opcode + RegisterPairOffsets[args[0]]) };
        }

        private byte[] OpcodeWithStackRegister(string[] args, byte opcode)
        {
            ThrowIfInvalidNumberOfOperands(args, 1);

            if (!StackRegisterOffsets.ContainsKey(args[0]))
            {
                throw new InvalidDataException($"Expected register pair B/D/H/PSW, found {args[0]}");
            }

            return new byte[] { (byte)(opcode + StackRegisterOffsets[args[0]]) };
        }

        private byte[] OpcodeWithIndex(string[] args, byte opcode)
        {
            ThrowIfInvalidNumberOfOperands(args, 1);

            ushort index;
            if (!ushort.TryParse(args[0], out index))
            {
                throw new InvalidDataException($"Expected number, found {args[0]}");
            }

            if (index > 7)
            {
                throw new InvalidDataException($"Expected value between 0-7, found {index}");
            }

            return new byte[] { (byte)(opcode + 8 * index) };
        }

        static void ThrowIfInvalidNumberOfOperands(string[] args, uint expected)
        {
            if (args.Length != expected)
            {
                throw new InvalidDataException($"Expected {expected} operand(s), found {args.Length}");
            }
        }

        static byte[] OpcodeWithRegisterOperand(string[] args, byte baseOpcode, byte multiplier = 1)
        {
            ThrowIfInvalidNumberOfOperands(args, 1);
            return new byte[] { OpcodeWithRegister(args[0], baseOpcode, multiplier) };
        }

        static byte[] OpcodeWith16BitAddress(string[] args, byte opcode)
        {
            ThrowIfInvalidNumberOfOperands(args, 1);
            var address = Parse16BitValue(args[0]);
            return new byte[] { opcode, address[0], address[1] };
        }

        private byte[] OpcodeWith8BitValue(string[] args, byte opcode)
        {
            ThrowIfInvalidNumberOfOperands(args, 1);
            return new byte[] { opcode, Parse8BitValue(args[0]) };
        }

        static Assembler()
        {
            RegisterOffsets.Add("a", 0x07);
            RegisterOffsets.Add("b", 0x00);
            RegisterOffsets.Add("c", 0x01);
            RegisterOffsets.Add("d", 0x02);
            RegisterOffsets.Add("e", 0x03);
            RegisterOffsets.Add("h", 0x04);
            RegisterOffsets.Add("l", 0x05);
            RegisterOffsets.Add("m", 0x06);

            RegisterPairOffsets.Add("b", 0x00);
            RegisterPairOffsets.Add("d", 0x10);
            RegisterPairOffsets.Add("h", 0x20);
            RegisterPairOffsets.Add("sp", 0x30);

            StackRegisterOffsets.Add("b", 0x00);
            StackRegisterOffsets.Add("d", 0x10);
            StackRegisterOffsets.Add("h", 0x20);
            StackRegisterOffsets.Add("psw", 0x30);
        }

        public AssemblyResult Compile(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var data = new List<byte>();
            var sourceMap = new Dictionary<int, byte>();
            var labelDefinitions = new Dictionary<string, short>();
            var labelUsages = new Dictionary<short, string>();

            using (var sr = new StreamReader(stream))
            {
                var line = string.Empty;
                var lineNumber = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    try
                    {
                        var address = (byte)data.Count;
                        line = line.ToLowerInvariant();
                        var commentIndex = line.IndexOf(';');
                        line = commentIndex >= 0 ? line.Substring(0, commentIndex) : line;

                        var label = string.Empty;
                        sourceMap.Add(lineNumber, address);

                        var colonIndex = line.IndexOf(':');
                        if (colonIndex > 0)
                        {
                            label = line.Substring(0, colonIndex);
                            labelDefinitions.Add(label, address);
                            line = line.Substring(colonIndex + 1).Trim();
                        }
                        else if (colonIndex == 0)
                        {
                            throw new InvalidDataException("Label must have at least one character");
                        }
                        else
                        {
                            var opcode = string.Empty;
                            var commaIndex = line.IndexOf(',');
                            if (commaIndex >= 0)
                            {
                                opcode = line.Substring(0, commaIndex);
                                line = line.Substring(commaIndex + 1);
                            }

                            var operands = (from s in line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) select s.Trim()).ToList();

                            if (operands.Count > 0 && opcode.Length == 0)
                            {
                                opcode = operands[0];
                                operands.RemoveAt(0);
                            }

                            data.AddRange(ProcessOpCode(address, opcode, operands.ToArray(), labelUsages));
                        }

                        lineNumber++;
                    }
                    catch (InvalidDataException ex)
                    {
                        throw new InvalidDataException($"Error at line {lineNumber}: {ex.Message}");
                    }
                }

                // Replace label usages with their locations

                foreach (var pair in labelUsages)
                {
                    if (!labelDefinitions.ContainsKey(pair.Value))
                    {
                        throw new InvalidDataException($"Label {pair.Value} was not defined");
                    }

                    var labelDefinition = labelDefinitions[pair.Value];
                    data[pair.Key + 1] = (byte)(labelDefinition & 0xFF);
                    data[pair.Key + 2] = (byte)((labelDefinition << 8) & 0xFF);

                }
            }

            return new AssemblyResult
            {
                Data = data.ToArray(),
                SourceMap = sourceMap
            };
        }

        private byte[] ProcessOpCode(byte address, string opcode, string[] args, Dictionary<short, string> labelUsages)
        {
            switch (opcode)
            {
                // case "aci": 
                case "adc": return OpcodeWithRegisterOperand(args, 0x88);
                case "add": return OpcodeWithRegisterOperand(args, 0x80);
                case "adi": return OpcodeWith8BitValue(args, 0xc6);
                case "ana": return OpcodeWithRegisterOperand(args, 0xA0);
                case "ani": return OpcodeWith8BitValue(args, 0xE6);
                case "call": return OpcodeWithLabel(args, address, labelUsages, 0xCD);
                case "cc": return OpcodeWithLabel(args, address, labelUsages, 0xDC);
                case "cm": return OpcodeWithLabel(args, address, labelUsages, 0xFC);
                case "cma": return new byte[] { 0x2f };
                case "cmc": return new byte[] { 0x3f };
                case "cmp": return OpcodeWithRegisterOperand(args, 0xB8);
                case "cnc": return OpcodeWithLabel(args, address, labelUsages, 0xD4);
                case "cnz": return OpcodeWithLabel(args, address, labelUsages, 0xC4);
                case "cp": return OpcodeWithLabel(args, address, labelUsages, 0xF4);
                case "cpe": return OpcodeWithLabel(args, address, labelUsages, 0xEC);
                case "cpi": return OpcodeWith8BitValue(args, 0xFE);
                case "cpo": return OpcodeWithLabel(args, address, labelUsages, 0xE4);
                case "cz": return OpcodeWithLabel(args, address, labelUsages, 0xCC);
                case "daa": return new byte[] { 0x27 };
                case "dad": return OpcodeWithRegisterPair(args, 0x09);
                case "dcr": return OpcodeWithRegisterOperand(args, 0x05, 8);
                case "dcx": return OpcodeWithRegisterPair(args, 0x0B);
                case "di": return new byte[] { 0xF3 };
                case "ei": return new byte[] { 0xFB };
                case "hlt": return new byte[] { 0x76 };
                case "in": return OpcodeWith8BitValue(args, 0xDB);
                case "inr": return OpcodeWithRegisterOperand(args, 0x04, 8);
                case "inx": return OpcodeWithRegisterPair(args, 0x03);
                case "jc": return OpcodeWithLabel(args, address, labelUsages, 0xDA);
                case "lda": return OpcodeWith16BitAddress(args, 0x3A);
                case "ldax": return OpcodeWithRegisterPair(args, 0x0A, true);
                case "lhld": return OpcodeWith16BitAddress(args, 0x2A);
                case "jm": return OpcodeWithLabel(args, address, labelUsages, 0xFA);
                case "jmp": return OpcodeWithLabel(args, address, labelUsages, 0xC3);
                case "jnc": return OpcodeWithLabel(args, address, labelUsages, 0xD2);
                case "jnz": return OpcodeWithLabel(args, address, labelUsages, 0xC2);
                case "jp": return OpcodeWithLabel(args, address, labelUsages, 0xF2);
                case "jpe": return OpcodeWithLabel(args, address, labelUsages, 0xEA);
                case "jpo": return OpcodeWithLabel(args, address, labelUsages, 0xE2);
                case "jz": return OpcodeWithLabel(args, address, labelUsages, 0xCA);
                case "lxi b": return OpcodeWith16BitAddress(args, 0x01);
                case "lxi d": return OpcodeWith16BitAddress(args, 0x11);
                case "lxi h": return OpcodeWith16BitAddress(args, 0x21);
                case "lxi sp": return OpcodeWith16BitAddress(args, 0x31);
                case "mov a": return OpcodeWithRegisterOperand(args, 0x78);
                case "mov b": return OpcodeWithRegisterOperand(args, 0x40);
                case "mov c": return OpcodeWithRegisterOperand(args, 0x48);
                case "mov d": return OpcodeWithRegisterOperand(args, 0x50);
                case "mov e": return OpcodeWithRegisterOperand(args, 0x58);
                case "mov h": return OpcodeWithRegisterOperand(args, 0x60);
                case "mov l": return OpcodeWithRegisterOperand(args, 0x68);
                case "mov m": return OpcodeWithRegisterOperand(args, 0x70);
                case "mvi a": return OpcodeWith8BitValue(args, 0x3e);
                case "mvi b": return OpcodeWith8BitValue(args, 0x06);
                case "mvi c": return OpcodeWith8BitValue(args, 0x0e);
                case "mvi d": return OpcodeWith8BitValue(args, 0x16);
                case "mvi e": return OpcodeWith8BitValue(args, 0x1e);
                case "mvi h": return OpcodeWith8BitValue(args, 0x26);
                case "mvi l": return OpcodeWith8BitValue(args, 0x2e);
                case "mvi m": return OpcodeWith8BitValue(args, 0x36);
                case "nop": return new byte[] { 0x00 };
                case "ora": return OpcodeWithRegisterOperand(args, 0xB0);
                case "ori": return OpcodeWith8BitValue(args, 0xF6);
                case "out": return OpcodeWith8BitValue(args, 0xD3);
                case "pchl": return new byte[] { 0xE9 };
                case "pop": return OpcodeWithStackRegister(args, 0xC1);
                case "push": return OpcodeWithStackRegister(args, 0xC5);
                case "ral": return new byte[] { 0x17 };
                case "rar": return new byte[] { 0x1F };
                case "ret": return new byte[] { 0xC9 };
                case "rlc": return new byte[] { 0x07 };
                case "rim": return new byte[] { 0x20 };
                case "rnc": return new byte[] { 0xD0 };
                case "rnz": return new byte[] { 0xC0 };
                case "rpe": return new byte[] { 0xE8 };
                case "rpo": return new byte[] { 0xE0 };
                case "rrc": return new byte[] { 0x0F };
                case "rst": return OpcodeWithIndex(args, 0xC7);
                case "rc": return new byte[] { 0xD8 };
                case "rm": return new byte[] { 0xF8 };
                case "rp": return new byte[] { 0xF0 };
                case "rz": return new byte[] { 0xC8 };
                case "sbb": return OpcodeWithRegisterOperand(args, 0x98);
                case "sbi": return OpcodeWith8BitValue(args, 0xDE);
                case "shld": return OpcodeWith16BitAddress(args, 0x22);
                case "sim": return new byte[] { 0x30 };
                case "sphl": return new byte[] { 0xF9 };
                case "sta": return OpcodeWith16BitAddress(args, 0x32);
                case "stc": return new byte[] { 0x37 };
                case "stax": return OpcodeWithRegisterPair(args, 0x02, true);
                case "sub": return OpcodeWithRegisterOperand(args, 0x90);
                case "sui": return OpcodeWith8BitValue(args, 0xD6);

                case "xchg": return new byte[] { 0xEB };
                case "xra": return OpcodeWithRegisterOperand(args, 0xA8);
                case "xri": return OpcodeWith8BitValue(args, 0xEE);
                case "xthl": return new byte[] { 0xE3 };



            }

            throw new InvalidDataException($"Invalid opcode {opcode}");
        }

        private byte[] OpcodeWithLabel(string[] args, byte address, Dictionary<short, string> labelUsages, byte opcode)
        {
            ThrowIfInvalidNumberOfOperands(args, 1);
            labelUsages.Add(address, args[0]);
            return new byte[] { opcode, 0xFF, 0xFF }; // Place temporary address value that will be replaced in label address in last pass
        }
    }
}
