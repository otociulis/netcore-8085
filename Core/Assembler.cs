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
        static Dictionary<string, InstructionAttribute> InstructionSet = new Dictionary<string, InstructionAttribute>();

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

        static Assembler()
        {
            RegisterOffsets.Add("a", 0x07);
            RegisterOffsets.Add("b", 0x00);
            RegisterOffsets.Add("c", 0x01);
            RegisterOffsets.Add("d", 0x02);
            RegisterOffsets.Add("e", 0x03);
            RegisterOffsets.Add("h", 0x04);
            RegisterOffsets.Add("l", 0x05);

            RegisterPairOffsets.Add("b", 0x00);
            RegisterPairOffsets.Add("d", 0x10);
            RegisterPairOffsets.Add("h", 0x20);

            foreach (var fieldInfo in typeof(InstructionSet).GetFields())
            {
                var attribute = fieldInfo.GetCustomAttributes(false).Cast<InstructionAttribute>().FirstOrDefault();
                if (attribute != null)
                {
                    var instructionName = string.Join(' ', fieldInfo.Name.Split('_'));
                    InstructionSet.Add(instructionName.ToLowerInvariant(), attribute);
                }
            }
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

        byte InstructionRegisterOffset(string operand, bool memoryAllowed)
        {
            if (RegisterOffsets.ContainsKey(operand))
            {
                return RegisterOffsets[operand];
            }
            else if (memoryAllowed && operand == "m")
            {
                return 0x6;
            }

            if (memoryAllowed)
            {
                throw new InvalidDataException($"Expected valid register name or memory indicator, found {operand}");
            }

            throw new InvalidDataException($"Expected valid register name, found {operand}");
        }

        byte InstructionRegisterPairOffset(string operand, string additionalRegister)
        {
            if (RegisterPairOffsets.ContainsKey(operand))
            {
                return RegisterPairOffsets[operand];
            }
            else if (operand == additionalRegister)
            {
                return 0x30;
            }

            throw new InvalidDataException($"Expected register pair B/D/H/{additionalRegister}, found {operand}");
        }

        private byte[] ProcessOpCode(byte address, string opcode, string[] args, Dictionary<short, string> labelUsages)
        {
            if (!InstructionSet.ContainsKey(opcode))
            {
                throw new InvalidDataException($"Invalid opcode {opcode}");
            }

            var instruction = InstructionSet[opcode];
            var operand = string.Empty;

            if (instruction.OperandType != OperandType.None)
            {
                if (args.Length == 1)
                {
                    operand = args[0];
                }
                else
                {
                    throw new InvalidDataException($"Expected operand but none was found");
                }
            }

            switch (instruction.OperandType)
            {
                case OperandType.None:
                    return new byte[] { instruction.Code };
                case OperandType.Data8Bit:
                    return new byte[] { instruction.Code, Parse8BitValue(args[0]) };
                case OperandType.Data16Bit:
                    var operandAsAddress = Parse16BitValue(operand);
                    return new byte[] { instruction.Code, operandAsAddress[0], operandAsAddress[1] };
                case OperandType.LabelAs16BitAddress:
                    labelUsages.Add(address, args[0]);
                    return new byte[] { instruction.Code, 0xFF, 0xFF }; // Place temporary address value that will be replaced in label address in last pass
                case OperandType.RegisterOrMemory:
                    return new byte[] { (byte)(instruction.Code + InstructionRegisterOffset(operand, true) * instruction.InstructionSpacing) };
                case OperandType.Register:
                    return new byte[] { (byte)(instruction.Code + InstructionRegisterOffset(operand, false) * instruction.InstructionSpacing) };
                case OperandType.RegisterPairOrStackPointer:
                    return new byte[] { (byte)(instruction.Code + InstructionRegisterPairOffset(args[0], "sp")) };
                case OperandType.RegisterPairOrProgramStatusWord:
                    return new byte[] { (byte)(instruction.Code + InstructionRegisterPairOffset(args[0], "psw")) };
                case OperandType.RegisterBD:
                    if (operand != "b" && operand != "d")
                    {
                        throw new InvalidDataException($"Expected register pair B/D, found {args[0]}");
                    }

                    return new byte[] { (byte)(instruction.Code + RegisterPairOffsets[operand]) };
                case OperandType.Index:
                    ushort index;
                    if (!ushort.TryParse(args[0], out index))
                    {
                        throw new InvalidDataException($"Expected number, found {args[0]}");
                    }

                    if (index > 7)
                    {
                        throw new InvalidDataException($"Expected value between 0-7, found {index}");
                    }

                    return new byte[] { (byte)(instruction.Code + 8 * index) };
                default:
                    throw new InvalidDataException($"Invalid operand type {instruction.OperandType}");
            }
        }
    }
}
