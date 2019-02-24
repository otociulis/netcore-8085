﻿using System;
using System.Collections.Generic;

namespace Core
{
    public class Emulator
    {
        private const int HaltOpCode = 0x76;
        private byte[] _memory;
        private readonly Dictionary<Register, byte> _registers = new Dictionary<Register, byte>();
        private readonly Dictionary<Flag, bool> _flags = new Dictionary<Flag, bool>();

        #region ProgramCounter
        private ushort _programCounter;
        public ushort ProgramCounter
        {
            get => _programCounter;
            set
            {
                if (_programCounter != value)
                {
                    _programCounter = value;
                    ProgramCounterChanged?.Invoke(this, _programCounter);
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
            set
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
            set
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
            set
            {
                _memory[address] = value;
            }
        }
        #endregion

        public event EventHandler Halted;
        public event EventHandler<ushort> ProgramCounterChanged;
        public event EventHandler<RegisterChangedEventArgs> RegisterChanged;
        public event EventHandler<FlagChangedEventArgs> FlagChanged;

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

        private ushort Get16BitValue(Register upper, Register lower)
        {
            return (ushort)((this[upper] << 8) + this[lower]);
        }

        private void Set16BitValue(Register upper, Register lower, ushort value)
        {
            this[upper] = (byte)(value >> 8);
            this[lower] = (byte)(value & 0xFF);
        }

        private ushort GetMemoryAddressAtNextAddress()
        {
            return (ushort)(_memory[ProgramCounter++] + _memory[ProgramCounter++] << 8);
        }

        public void Step()
        {
            var opcode = _memory[ProgramCounter++];

            switch (opcode)
            {
                default:
                    throw new InvalidOperationException($"Unknown opcode 0x{opcode.ToString("X2")} at address 0x{ProgramCounter.ToString("X4")}");
                case 0x00: // NOP 
                    break;
                case 0x01: // LXI B
                    this[Register.C] = _memory[ProgramCounter++];
                    this[Register.B] = _memory[ProgramCounter++];
                    break;
                case 0x02: // STAX B
                    _memory[Get16BitValue(Register.B, Register.C)] = this[Register.A];
                    break;
                case 0x03: // INX B
                    IncrementPair(Register.B, Register.C);
                    break;
                case 0x04: // INR B
                    IncrementRegister(Register.B);
                    break;
                case 0x05: // DCR B
                    DecrementRegister(Register.B);
                    break;
                case 0x06: // MVI B
                    this[Register.B] = _memory[ProgramCounter++];
                    break;
                case 0x07: // RLC
                    this[Flag.C] = (this[Register.A] & 0x80) == 0x80;
                    this[Register.A] = (byte)((byte)(this[Register.A] << 1) + (byte)(this[Flag.C] ? 1 : 0));
                    break;
                case 0x09: //DAD B
                    AddRegisterPairToHLRegisters(Register.B, Register.C);
                    break;
                case 0x0A: // LDAX B
                    this[Register.A] = _memory[Get16BitValue(Register.B, Register.C)];
                    break;
                case 0x0B: // DCX B
                    Set16BitValue(Register.B, Register.C, (ushort)(Get16BitValue(Register.B, Register.C) - 1));
                    break;
                case 0x0C: // INR C
                    IncrementRegister(Register.C);
                    break;
                case 0x0D: // DCR C
                    DecrementRegister(Register.C);
                    break;
                case 0x0E: // MVI C
                    this[Register.C] = _memory[ProgramCounter++];
                    break;
                case 0x0F: // RRC
                    this[Flag.C] = (this[Register.A] & 0x01) == 0x01;
                    this[Register.A] = (byte)((byte)(this[Register.A] >> 1) + (byte)(this[Flag.C] ? 0x80 : 0));
                    break;
                case 0x11: // LXI D
                    this[Register.E] = _memory[ProgramCounter++];
                    this[Register.D] = _memory[ProgramCounter++];
                    break;
                case 0x12: // STAX D
                    _memory[Get16BitValue(Register.D, Register.E)] = this[Register.A];
                    break;
                case 0x13: // INX D
                    IncrementPair(Register.D, Register.E);
                    break;
                case 0x16: // MVI D
                    this[Register.D] = _memory[ProgramCounter++];
                    break;
                case 0x1E: // MVI E
                    this[Register.E] = _memory[ProgramCounter++];
                    break;
                case 0x21: // LXI H
                    this[Register.L] = _memory[ProgramCounter++];
                    this[Register.H] = _memory[ProgramCounter++];
                    break;

                case 0x23: // INX H
                    IncrementPair(Register.H, Register.L);
                    break;
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

        void IncrementPair(Register upper, Register lower)
        {
            this[lower]++;
            this[upper] += (byte)((this[lower] == 0) ? 1 : 0);
        }

        void UpdateZeroFlag(byte value)
        {
            this[Flag.Z] = value == 0;
        }

        void UpdateSignFlag(byte value)
        {
            this[Flag.S] = (value & 0x80) == 0x80;
        }

        void UpdateAuxiliaryCarryFlag(byte a, byte b, bool increment)
        {
            var valueLower = a & 0xF;
            var previousLower = b & 0xF;

            this[Flag.AC] = increment ? (valueLower + previousLower > 0xF) : (valueLower - previousLower > valueLower);
        }

        void UpdateParityFlag(byte value)
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
