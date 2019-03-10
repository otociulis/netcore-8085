using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core
{
    public class Emulator
    {
        class InstructionMetadata
        {
            public Register? Register { get; set; }
            public InstructionAttribute Attribute { get; private set; }
            public FieldInfo FieldInfo { get; private set; }

            public InstructionMetadata(InstructionAttribute attribute, FieldInfo fieldInfo)
            {
                FieldInfo = fieldInfo;
                Attribute = attribute;
            }

            public InstructionMetadata(InstructionMetadata metadata, Register register) : this(metadata.Attribute, metadata.FieldInfo)
            {
                Register = register;
            }
        }

        private const int HaltOpCode = 0x76;
        private byte[] _memory;
        private readonly Dictionary<Register, byte> _registers = new Dictionary<Register, byte>();
        private readonly Dictionary<Flag, bool> _flags = new Dictionary<Flag, bool>();
        static Dictionary<byte, InstructionMetadata> InstructionSet = new Dictionary<byte, InstructionMetadata>();

        #region ProgramCounter
        private ushort _programCounter;
        public ushort ProgramCounter
        {
            get => _programCounter;
            internal set
            {
                if (_programCounter != value)
                {
                    _programCounter = value;
                    ProgramCounterChanged?.Invoke(this, _programCounter);
                }
            }
        }
        #endregion
        #region StackPointer
        private ushort _stackPointer;
        public ushort StackPointer
        {
            get => _stackPointer;
            internal set
            {
                if (_stackPointer != value)
                {
                    _stackPointer = value;
                    StackPointerChanged?.Invoke(this, _stackPointer);
                }
            }
        }
        #endregion
        #region InterruptMask
        private byte _interruptMask;
        public byte InterruptMask
        {
            get => _interruptMask;
            internal set
            {
                if (_interruptMask != value)
                {
                    _interruptMask = value;
                    InterruptMaskChanged?.Invoke(this, _interruptMask);
                }
            }
        }
        #endregion
        #region this[Register register]
        public byte this[Register register]
        {
            get
            {
                return _registers[register];
            }
            internal set
            {
                if (_registers[register] != value)
                {
                    _registers[register] = value;
                    RegisterChanged?.Invoke(this, new RegisterChangedEventArgs(register, value));
                }
            }
        }

        #endregion
        #region this[Flag flag]
        public bool this[Flag flag]
        {
            get
            {
                return _flags[flag];
            }
            internal set
            {
                if (_flags[flag] != value)
                {
                    _flags[flag] = value;
                    FlagChanged?.Invoke(this, new FlagChangedEventArgs(flag, value));
                }
            }
        }
        #endregion
        #region this[ushort address]
        public byte this[ushort address]
        {
            get
            {
                return _memory[address];
            }
            internal set
            {
                _memory[address] = value;
            }
        }
        #endregion

        public event EventHandler Halted;
        public event EventHandler<ushort> ProgramCounterChanged;
        public event EventHandler<ushort> StackPointerChanged;
        public event EventHandler<RegisterChangedEventArgs> RegisterChanged;
        public event EventHandler<FlagChangedEventArgs> FlagChanged;
        public event EventHandler<byte> InterruptMaskChanged;

        static Emulator()
        {
            foreach (var fieldInfo in typeof(InstructionSet).GetFields())
            {
                var attribute = fieldInfo.GetCustomAttributes(false).Cast<InstructionAttribute>().FirstOrDefault();
                if (attribute != null)
                {
                    var metadata = new InstructionMetadata(attribute, fieldInfo);
                    switch (attribute.OperandType)
                    {
                        case OperandType.None:
                        case OperandType.Data8Bit:
                            InstructionSet.Add(attribute.Code, metadata);
                            break;
                        case OperandType.Data16Bit:
                            InstructionSet.Add(attribute.Code, metadata);
                            break;
                        case OperandType.RegisterBD:
                            InstructionSet.Add(attribute.Code, new InstructionMetadata(metadata, Register.B));
                            InstructionSet.Add((byte)(attribute.Code + 0x10), new InstructionMetadata(metadata, Register.D));
                            break;
                        case OperandType.RegisterOrMemory:
                            InstructionSet.Add((byte)(attribute.Code + 0x06 * attribute.InstructionSpacing), metadata);
                            foreach (Register key in Enum.GetValues(typeof(Register)))
                            {
                                var offset = (byte)(attribute.Code + (int)key * attribute.InstructionSpacing);
                                InstructionSet.Add(offset, new InstructionMetadata(metadata, key));
                            }
                            break;
                        case OperandType.RegisterPairOrStackPointer:
                            InstructionSet.Add(attribute.Code, new InstructionMetadata(metadata, Register.B));
                            InstructionSet.Add((byte)(attribute.Code + 0x10), new InstructionMetadata(metadata, Register.D));
                            InstructionSet.Add((byte)(attribute.Code + 0x20), new InstructionMetadata(metadata, Register.H));
                            InstructionSet.Add((byte)(attribute.Code + 0x30), metadata);
                            break;
                    }
                }
            }
        }

        public Emulator(int memorySize = 64 * 1024)
        {
            _memory = new byte[memorySize];
            foreach (Register key in Enum.GetValues(typeof(Register)))
            {
                _registers.Add(key, 0);
            }

            foreach (Flag key in Enum.GetValues(typeof(Flag)))
            {
                _flags.Add(key, false);
            }
        }

        public void SetMemory(ushort address, params byte[] program)
        {
            Array.Copy(program, 0, _memory, address, program.Length);
        }

        internal ushort Get16BitValue(Register upper, Register lower)
        {
            return (ushort)((this[upper] << 8) + this[lower]);
        }

        void Set16BitValue(Register upper, Register lower, ushort value)
        {
            this[upper] = (byte)(value >> 8);
            this[lower] = (byte)(value & 0xFF);
        }

        ushort GetMemoryAddressAtNextAddress()
        {
            return (ushort)(_memory[ProgramCounter++] + _memory[ProgramCounter++] << 8);
        }

        void ExecuteActionOnRegister(byte opcode, byte bRegisterOpcode, Action<Register> action)
        {
            var offset = (opcode - bRegisterOpcode) / 8;
            action(offset == 7 ? Register.A : (Register)(1 + offset));
        }

        void ExecuteActionOnRegisterPair(byte opcode, byte bRegisterOpcode, byte divisor, Action<Register, Register> action)
        {
            var lower = (Register)(1 + (opcode - bRegisterOpcode) / divisor);
            action(lower, lower + 1);
        }

        public void Step()
        {
            var opcode = _memory[ProgramCounter++];
            var operand = new byte[0];

            if (!InstructionSet.ContainsKey(opcode))
            {
                throw new InvalidOperationException($"Unknown opcode 0x{opcode.ToString("X2")} at address 0x{ProgramCounter.ToString("X4")}");
            }
            else { 
                var metadata = InstructionSet[opcode];

                switch (metadata.Attribute.OperandType)
                {
                    case OperandType.Index:
                    case OperandType.None:
                    case OperandType.Register:
                    case OperandType.RegisterBD:
                    case OperandType.RegisterOrMemory:
                    case OperandType.RegisterPairOrProgramStatusWord:
                    case OperandType.RegisterPairOrStackPointer:
                        break;
                    case OperandType.Data16Bit:
                    case OperandType.LabelAs16BitAddress:
                        operand = new byte[] { _memory[ProgramCounter++], _memory[ProgramCounter++] };
                        break;
                    case OperandType.Data8Bit:
                        operand = new byte[] { _memory[ProgramCounter++] };
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown operand type {metadata.Attribute.OperandType} at address 0x{ProgramCounter.ToString("X4")}");
                }

                var executor = metadata.FieldInfo.GetValue(null);
                (executor as Action)?.Invoke();
                (executor as Action<Emulator>)?.Invoke(this);
                (executor as Action<Emulator, Register?>)?.Invoke(this, metadata.Register);

                if (metadata.Register.HasValue)
                {
                    (executor as Action<Emulator, Register>)?.Invoke(this, metadata.Register.Value);
                }

                if (operand.Length == 2)
                {
                    (executor as Action<Emulator, byte, byte>)?.Invoke(this, operand[0], operand[1]);
                }

                if (operand.Length == 1)
                {
                    (executor as Action<Emulator, byte>)?.Invoke(this, operand[0]);
                }

            }

            /*switch (opcode)
            {
                default:
                    throw new InvalidOperationException($"Unknown opcode 0x{opcode.ToString("X2")} at address 0x{ProgramCounter.ToString("X4")}");
                case 0x00: // NOP 
                    break;
                case 0x01: // LXI B
                case 0x11: // LXI D
                case 0x21: // LXI H
                    ExecuteActionOnRegisterPair(opcode, 0x01, 0x08, (lower, upper) =>
                    {
                        this[upper] = _memory[ProgramCounter++];
                        this[lower] = _memory[ProgramCounter++];
                    });
                    break;
                case 0x02: // STAX B
                case 0x12: // STAX D
                    ExecuteActionOnRegisterPair(opcode, 0x02, 0x08, (lower, upper) =>
                    {
                        _memory[Get16BitValue(lower, upper)] = this[Register.A];
                    });
                    break;
                case 0x03: // INX B
                case 0x13: // INX D
                case 0x23: // INX H
                    ExecuteActionOnRegisterPair(opcode, 0x03, 0x08, (lower, upper) =>
                    {
                        IncrementPair(lower);
                    });
                    break;
                case 0x04: // INR B
                case 0x0C: // INR C
                case 0x14: // INR D
                case 0x1C: // INR E
                case 0x24: // INR H
                case 0x2C: // INR L
                case 0x3C: // INR A
                    ExecuteActionOnRegister(opcode, 0x04, IncrementRegister);
                    break;
                case 0x05: // DCR B
                case 0x0D: // DCR C
                case 0x15: // DCR D
                case 0x1D: // DCR E
                case 0x25: // DCR H
                case 0x2D: // DCR L
                case 0x3D: // DCR A
                    ExecuteActionOnRegister(opcode, 0x05, DecrementRegister);
                    break;
                case 0x06: // MVI B
                case 0x0E: // MVI C
                case 0x16: // MVI D
                case 0x1E: // MVI E
                case 0x26: // MVI H
                case 0x2E: // MVI L
                case 0x3E: // MVI A
                    ExecuteActionOnRegister(opcode, 0x06, r => { this[r] = _memory[ProgramCounter++]; });
                    break;
                case 0x07: // RLC
                    this[Flag.C] = (this[Register.A] & 0x80) == 0x80;
                    this[Register.A] = (byte)((byte)(this[Register.A] << 1) + (byte)(this[Flag.C] ? 1 : 0));
                    break;
                case 0x09: // DAD B
                case 0x19: // DAD D
                case 0x29: // DAD H
                    ExecuteActionOnRegisterPair(opcode, 0x09, 0x08, AddRegisterPairToHLRegisters);
                    break;
                case 0x0A: // LDAX B
                case 0x1A: // LDAX D
                    ExecuteActionOnRegisterPair(opcode, 0x0A, 0x08, (lower, upper) =>
                    {
                        this[Register.A] = _memory[Get16BitValue(lower, upper)];
                    });
                    break;
                case 0x0B: // DCX B
                case 0x1B: // DCX D
                case 0x2B: // DCX H
                    ExecuteActionOnRegisterPair(opcode, 0x09, 0x08, (lower, upper) =>
                    {
                        Set16BitValue(lower, upper, (ushort)(Get16BitValue(lower, upper) - 1));
                    });
                    break;
                case 0x0F: // RRC
                    this[Flag.C] = (this[Register.A] & 0x01) == 0x01;
                    this[Register.A] = (byte)((byte)(this[Register.A] >> 1) + (byte)(this[Flag.C] ? 0x80 : 0));
                    break;
                case 0x17: // RAL
                    {
                        var carry = (byte)(this[Flag.C] ? 1 : 0);
                        this[Flag.C] = (this[Register.A] & 0x80) == 0x80;
                        this[Register.A] = (byte)((this[Register.A] << 1) + carry);
                        break;
                    }
                case 0x1F: // RAR
                    {
                        var carry = (byte)(this[Flag.C] ? 0x80 : 0);
                        this[Flag.C] = (this[Register.A] & 0x01) == 0x01;
                        this[Register.A] = (byte)((this[Register.A] >> 1) + carry);
                        break;
                    }
                case 0x32: // STA
                    _memory[GetMemoryAddressAtNextAddress()] = this[Register.A];
                    break;
                case 0x3A: // LDA
                    this[Register.A] = _memory[GetMemoryAddressAtNextAddress()];
                    break;
                case 0x47: // MOV B, A
                    this[Register.B] = this[Register.A];
                    break;
                case 0x78: // MOV B, A
                    this[Register.A] = this[Register.B];
                    break;
                case HaltOpCode: // HLT
                    Halted?.Invoke(this, EventArgs.Empty);
                    break;

                case 0x77: // MOV M, A
                    _memory[Get16BitValue(Register.H, Register.L)] = this[Register.A];
                    break;

                case 0x7E: // MOV A, M
                    this[Register.A] = _memory[Get16BitValue(Register.H, Register.L)];
                    break;

                case 0x86: // ADD M
                    this[Register.A] += _memory[Get16BitValue(Register.H, Register.L)];
                    break;

                case 0xBE: // CMP M
                    {
                        var val = _memory[Get16BitValue(Register.H, Register.L)];

                        if (this[Register.A] < val)
                        {
                            this[Flag.C] = true;
                        }
                        else if (this[Register.A] == val)
                        {
                            this[Flag.Z] = true;
                        }
                        else
                        {
                            this[Flag.C] = false;
                            this[Flag.Z] = false;
                        }
                        break;
                    }
            }*/
        }

        private void AddRegisterPairToHLRegisters(Register upper, Register lower)
        {
            var result = Get16BitValue(Register.H, Register.L) + Get16BitValue(upper, lower);
            this[Flag.C] = (result & 0x10000) == 0x10000;
            Set16BitValue(Register.H, Register.L, (ushort)result);
        }

        void IncrementRegister(Register register)
        {
            UpdateAuxiliaryCarryFlag(this[register], 1, true);

            this[register]++;
            UpdateParityFlag(this[register]);
            UpdateZeroFlag(this[register]);
            UpdateSignFlag(this[register]);
        }

        void DecrementRegister(Register register)
        {
            UpdateAuxiliaryCarryFlag(this[register], 1, false);

            this[register]--;
            UpdateParityFlag(this[register]);
            UpdateZeroFlag(this[register]);
            UpdateSignFlag(this[register]);
        }

        void IncrementPair(Register upper)
        {
            var lower = upper + 1;
            this[lower]++;
            this[upper] += (byte)((this[lower] == 0) ? 1 : 0);
        }

        internal void UpdateZeroFlag(byte value)
        {
            this[Flag.Z] = value == 0;
        }

        internal void UpdateSignFlag(byte value)
        {
            this[Flag.S] = (value & 0x80) == 0x80;
        }

        internal void UpdateAuxiliaryCarryFlag(byte a, byte b, bool increment)
        {
            var valueLower = a & 0xF;
            var previousLower = b & 0xF;

            this[Flag.AC] = increment ? (valueLower + previousLower > 0xF) : (valueLower - previousLower > valueLower);
        }

        internal void UpdateParityFlag(byte value)
        {
            var onesCount = 0;
            onesCount += (value & 0x01) == 0x00 ? 0 : 1;
            onesCount += (value & 0x02) == 0x00 ? 0 : 1;
            onesCount += (value & 0x04) == 0x00 ? 0 : 1;
            onesCount += (value & 0x08) == 0x00 ? 0 : 1;
            onesCount += (value & 0x10) == 0x00 ? 0 : 1;
            onesCount += (value & 0x20) == 0x00 ? 0 : 1;
            onesCount += (value & 0x40) == 0x00 ? 0 : 1;
            onesCount += (value & 0x80) == 0x00 ? 0 : 1;

            this[Flag.P] = onesCount % 2 == 0;
        }

        public void Run()
        {
            while (_memory[ProgramCounter] != HaltOpCode && ProgramCounter < _memory.Length)
            {
                Step();
            }
        }
    }
}
