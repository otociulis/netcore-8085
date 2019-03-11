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
                        case OperandType.Register:
                            foreach (Register key in Enum.GetValues(typeof(Register)))
                            {
                                var offset = (byte)(attribute.Code + (int)key * attribute.InstructionSpacing);
                                InstructionSet.Add(offset, new InstructionMetadata(metadata, key));
                            }
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

        internal void OnHalted()
        {
            Halted?.Invoke(this, EventArgs.Empty);
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
