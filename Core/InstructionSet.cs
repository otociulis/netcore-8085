using System;

namespace Core
{
    static class InstructionSet
    {
        [Instruction(0xCE, OperandType.Data8Bit, Description = "Add immediate to accumulator with carry")]
        public static readonly Action<Emulator, byte> ACI = (emulator, data) =>
        {
            var current = emulator[Register.A] + (emulator[Flag.C] ? 1 : 0);

            emulator[Flag.AC] = ((byte)current).AuxiliaryCarryFlag(data, true);
            emulator[Flag.C] = (current + data) > 0xFF;
            emulator[Register.A] = (byte)(current + data);
            emulator[Flag.P] = emulator[Register.A].ParityFlag();
            emulator[Flag.Z] = emulator[Register.A] == 0;
            emulator[Flag.S] = emulator[Register.A].SignFlag();
        };

        [Instruction(0x88, OperandType.RegisterOrMemory, Description = "Add register to accumulator with carry")] // Distance = 1
        public static readonly Action<Emulator, Register?> ADC = (emulator, register) =>
        {
            var increment = register.HasValue ? emulator[register.Value] : emulator.GetHLMemoryValue();
            increment += (byte)(emulator[Flag.C] ? 1 : 0);
            IncrementSourceBy(emulator, Register.A, increment);
            emulator[Flag.C] = false;
        };

        [Instruction(0x80, OperandType.RegisterOrMemory, Description = "Add register to accumulator ")]
        public static readonly Action<Emulator, Register?> ADD = (emulator, register) =>
        {
            IncrementSourceBy(emulator, Register.A, register.HasValue ? emulator[register.Value] : emulator.GetHLMemoryValue());
        };

        [Instruction(0xC6, OperandType.Data8Bit, Description = "Add immediate to accumulator")]
        public static readonly Action<Emulator, byte> ADI = (emulator, data) =>
        {
            var current = emulator[Register.A];

            emulator[Flag.AC] = current.AuxiliaryCarryFlag(data, true);
            emulator[Flag.C] = (current + data) > 0xFF;
            current += data;
            emulator[Flag.P] = current.ParityFlag();
            emulator[Flag.Z] = current == 0;
            emulator[Flag.S] = current.SignFlag();

            emulator[Register.A] = current;
        };

        [Instruction(0xA0, OperandType.RegisterOrMemory, Description = "Logical AND with accumulator")]
        public static readonly Action<Emulator, Register?> ANA = (emulator, register) =>
        {
            var current = register.HasValue ? emulator[register.Value] : emulator.GetHLMemoryValue();
            current &= emulator[Register.A];

            emulator[Flag.AC] = true;
            emulator[Flag.C] = false;
            emulator[Flag.P] = current.ParityFlag();
            emulator[Flag.Z] = current == 0;
            emulator[Flag.S] = current.SignFlag();

            emulator[Register.A] = current;
        };

        [Instruction(0xE6, OperandType.Data8Bit, Description = "AND immediate with accumulator")]
        public static readonly Action<Emulator, byte> ANI = (emulator, data) =>
        {
            data &= emulator[Register.A];

            emulator[Flag.AC] = true;
            emulator[Flag.C] = false;
            emulator[Flag.P] = data.ParityFlag();
            emulator[Flag.Z] = data == 0;
            emulator[Flag.S] = data.SignFlag();

            emulator[Register.A] = data;
        };

        [Instruction(0xCD, OperandType.LabelAs16BitAddress)]
        public static readonly object CALL;

        [Instruction(0xDC, OperandType.LabelAs16BitAddress)]
        public static readonly object CC;

        [Instruction(0xD4, OperandType.LabelAs16BitAddress)]
        public static readonly object CNC;

        [Instruction(0xF4, OperandType.LabelAs16BitAddress)]
        public static readonly object CP;

        [Instruction(0xFC, OperandType.LabelAs16BitAddress)]
        public static readonly object CM;

        [Instruction(0xEC, OperandType.LabelAs16BitAddress)]
        public static readonly object CPE;

        [Instruction(0xE4, OperandType.LabelAs16BitAddress)]
        public static readonly object CPO;

        [Instruction(0xCC, OperandType.LabelAs16BitAddress)]
        public static readonly object CZ;

        [Instruction(0xC4, OperandType.LabelAs16BitAddress)]
        public static readonly object CNZ;

        [Instruction(0x2F, OperandType.None, Description = "Complement accumulator")]
        public static readonly Action<Emulator> CMA = emulator => emulator[Register.A] = (byte)(0xFF - emulator[Register.A]);

        [Instruction(0x3F, OperandType.None, Description = "Complement carry")]
        public static readonly Action<Emulator> CMC = emulator => emulator[Flag.C] = !emulator[Flag.C];

        [Instruction(0xB8, OperandType.RegisterOrMemory, Description = "Compare with accumulator")]
        public static readonly Action<Emulator, Register?> CMP = (emulator, register) =>
        {
            var value = register.HasValue ? emulator[register.Value] : emulator.GetHLMemoryValue();
            var accumulator = emulator[Register.A];
            // Comparison is performed by subtracting contents of the operand from acculumator but registers/memory is not modified
            var current = (byte)(emulator[Register.A] - value);

            emulator[Flag.AC] = accumulator.AuxiliaryCarryFlag(value, false);
            emulator[Flag.C] = accumulator < value ? true : (accumulator == value ? emulator[Flag.C] : false);
            emulator[Flag.Z] = current == 0;
            emulator[Flag.P] = current.ParityFlag();
            emulator[Flag.S] = current.SignFlag();
        };

        [Instruction(0xFE, OperandType.Data8Bit)]
        public static readonly object CPI;

        [Instruction(0x27, OperandType.None)]
        public static readonly object DAA;

        [Instruction(0x09, OperandType.RegisterPairOrStackPointer, Description = "Add register pair to H and L registers")]
        public static readonly Action<Emulator, Register?> DAD = (emulator, register) =>
        {
            var hlValue = emulator.Get16BitValue(Register.H, Register.L);
            ushort value = 0;
            if (register.HasValue)
            {
                value = emulator.Get16BitValue(register.Value, register.Value + 1);
            }
            else
            {
                value = emulator.StackPointer;
            }

            value += hlValue;
            emulator[Flag.C] = value < hlValue;
            emulator[Register.H] = (byte)(value >> 8);
            emulator[Register.L] = (byte)(value & 0xFF);
        };

        [Instruction(0x05, OperandType.RegisterOrMemory, InstructionSpacing = 0x08)]
        public static readonly Action<Emulator, Register?> DCR = (emulator, register) => IncrementSourceBy(emulator, register, -1);

        [Instruction(0x0B, OperandType.RegisterPairOrStackPointer, Description = "Decrement register pair by 1")]
        public static readonly Action<Emulator, Register?> DCX = (emulator, register) =>
        {
            if (register.HasValue)
            {
                var upper = register.Value;
                var lower = upper + 1;
                emulator[lower]--;
                emulator[upper] -= (byte)((emulator[lower] == 0xFF) ? 1 : 0);
            }
            else
            {
                emulator.StackPointer--;
            }
        };

        [Instruction(0xF3, OperandType.None)]
        public static readonly object DI;

        [Instruction(0xFB, OperandType.None)]
        public static readonly object EI;

        [Instruction(0x76, OperandType.None)]
        public static readonly Action<Emulator> HLT = emulator => { emulator.OnHalted(); };

        [Instruction(0xDB, OperandType.Data8Bit)]
        public static readonly object IN;

        [Instruction(0x04, OperandType.RegisterOrMemory, InstructionSpacing = 0x08)]
        public static readonly Action<Emulator, Register?> INR = (emulator, register) => IncrementSourceBy(emulator, register, 1);

        [Instruction(0x03, OperandType.RegisterPairOrStackPointer)]
        public static readonly Action<Emulator, Register?> INX = (emulator, register) =>
        {
            if (register.HasValue)
            {
                var upper = register.Value;
                var lower = upper + 1;
                emulator[lower]++;
                emulator[upper] += (byte)((emulator[lower] == 0) ? 1 : 0);
            }
            else
            {
                emulator.StackPointer++;
            }
        };

        [Instruction(0xC3, OperandType.LabelAs16BitAddress, Description = "Jump unconditionally")]
        public static readonly Action<Emulator, byte, byte> JMP = (emulator, lower, upper) => JumpTo(emulator, lower, upper, true);

        [Instruction(0xDA, OperandType.LabelAs16BitAddress, Description = "Jump on Carry")]
        public static readonly Action<Emulator, byte, byte> JC = (emulator, lower, upper) => JumpTo(emulator, lower, upper, emulator[Flag.C]);

        [Instruction(0xD2, OperandType.LabelAs16BitAddress, Description = "Jump with no Carry")]
        public static readonly Action<Emulator, byte, byte> JNC = (emulator, lower, upper) => JumpTo(emulator, lower, upper, !emulator[Flag.C]);

        [Instruction(0xF2, OperandType.LabelAs16BitAddress, Description = "Jump on positive")]
        public static readonly Action<Emulator, byte, byte> JP = (emulator, lower, upper) => JumpTo(emulator, lower, upper, emulator[Flag.S]);

        [Instruction(0xFA, OperandType.LabelAs16BitAddress, Description = "Jump on minus")]
        public static readonly Action<Emulator, byte, byte> JM = (emulator, lower, upper) => JumpTo(emulator, lower, upper, !emulator[Flag.S]);

        [Instruction(0xEA, OperandType.LabelAs16BitAddress, Description = "Jump on Parity Even")]
        public static readonly Action<Emulator, byte, byte> JPE = (emulator, lower, upper) => JumpTo(emulator, lower, upper, emulator[Flag.P]);

        [Instruction(0xE2, OperandType.LabelAs16BitAddress, Description = "Jump on Parity Odd")]
        public static readonly Action<Emulator, byte, byte> JPO = (emulator, lower, upper) => JumpTo(emulator, lower, upper, !emulator[Flag.P]);

        [Instruction(0xCA, OperandType.LabelAs16BitAddress, Description = "Jump on Zero")]
        public static readonly Action<Emulator, byte, byte> JZ = (emulator, lower, upper) => JumpTo(emulator, lower, upper, emulator[Flag.Z]);

        [Instruction(0xC2, OperandType.LabelAs16BitAddress, Description = "Jump on No Zero")]
        public static readonly Action<Emulator, byte, byte> JNZ = (emulator, lower, upper) => JumpTo(emulator, lower, upper, !emulator[Flag.Z]);

        [Instruction(0x3A, OperandType.Data16Bit, Description = "Load accumulator direct")]
        public static readonly Action<Emulator, byte, byte> LDA = (emulator, lower, upper) =>
        {
            var address = (upper << 8) + lower;
            emulator[Register.A] = emulator[(ushort)address];
        };

        [Instruction(0x0A, OperandType.RegisterBD, Description = "Load accumulator indirect")]
        public static readonly Action<Emulator, Register> LDAX = (emulator, register) =>
        {
            var address = emulator.Get16BitValue(register, register + 1);
            emulator[Register.A] = emulator[address];
        };

        [Instruction(0x2A, OperandType.Data16Bit)]
        public static readonly Action<Emulator, byte, byte> LHLD = (emulator, lower, upper) =>
        {
            var address = (upper << 8) + lower;
            emulator[Register.L] = emulator[(ushort)address];
            emulator[Register.H] = emulator[(ushort)(address + 1)];
        };

        [Instruction(0x01, OperandType.Data16Bit)]
        public static readonly Action<Emulator, byte, byte> LXI_B = (emulator, lower, upper) =>
        {
            emulator[Register.C] = lower;
            emulator[Register.B] = upper;
        };

        [Instruction(0x11, OperandType.Data16Bit)]
        public static readonly Action<Emulator, byte, byte> LXI_D = (emulator, lower, upper) =>
        {
            emulator[Register.E] = lower;
            emulator[Register.D] = upper;
        };

        [Instruction(0x21, OperandType.Data16Bit)]
        public static readonly Action<Emulator, byte, byte> LXI_H = (emulator, lower, upper) =>
        {
            emulator[Register.L] = lower;
            emulator[Register.H] = upper;
        };

        [Instruction(0x31, OperandType.Data16Bit)]
        public static readonly Action<Emulator, byte, byte> LXI_SP = (emulator, lower, upper) =>
         {
             emulator.StackPointer = (ushort)((upper << 8) + lower);
         };

        [Instruction(0x78, OperandType.RegisterOrMemory, Description = "Copy from source to register A")]
        public static readonly Action<Emulator, Register?> MOV_A = (emulator, register) => CopyFromSourceToDestination(emulator, register, Register.A);

        [Instruction(0x40, OperandType.RegisterOrMemory, Description = "Copy from source to register B")]
        public static readonly Action<Emulator, Register?> MOV_B = (emulator, register) => CopyFromSourceToDestination(emulator, register, Register.B);

        [Instruction(0x48, OperandType.RegisterOrMemory, Description = "Copy from source to register C")]
        public static readonly Action<Emulator, Register?> MOV_C = (emulator, register) => CopyFromSourceToDestination(emulator, register, Register.C);

        [Instruction(0x50, OperandType.RegisterOrMemory, Description = "Copy from source to register D")]
        public static readonly Action<Emulator, Register?> MOV_D = (emulator, register) => CopyFromSourceToDestination(emulator, register, Register.D);

        [Instruction(0x58, OperandType.RegisterOrMemory, Description = "Copy from source to register E")]
        public static readonly Action<Emulator, Register?> MOV_E = (emulator, register) => CopyFromSourceToDestination(emulator, register, Register.E);

        [Instruction(0x60, OperandType.RegisterOrMemory, Description = "Copy from source to register H")]
        public static readonly Action<Emulator, Register?> MOV_H = (emulator, register) => CopyFromSourceToDestination(emulator, register, Register.H);

        [Instruction(0x68, OperandType.RegisterOrMemory, Description = "Copy from source to register L")]
        public static readonly Action<Emulator, Register?> MOV_L = (emulator, register) => CopyFromSourceToDestination(emulator, register, Register.L);

        [Instruction(0x70, OperandType.Register, Description = "Copy from source to memory")]
        public static readonly Action<Emulator, Register> MOV_M = (emulator, register) =>
        {
            var address = emulator.Get16BitValue(Register.H, Register.L);
            emulator[address] = emulator[register];
        };

        [Instruction(0x3E, OperandType.Data8Bit)]
        public static readonly Action<Emulator, byte> MVI_A = (emulator, data) => MoveImmediate(emulator, Register.A, data);

        [Instruction(0x06, OperandType.Data8Bit)]
        public static readonly Action<Emulator, byte> MVI_B = (emulator, data) => MoveImmediate(emulator, Register.B, data);

        [Instruction(0x0E, OperandType.Data8Bit)]
        public static readonly Action<Emulator, byte> MVI_C = (emulator, data) => MoveImmediate(emulator, Register.C, data);

        [Instruction(0x16, OperandType.Data8Bit)]
        public static readonly Action<Emulator, byte> MVI_D = (emulator, data) => MoveImmediate(emulator, Register.D, data);

        [Instruction(0x1E, OperandType.Data8Bit)]
        public static readonly Action<Emulator, byte> MVI_E = (emulator, data) => MoveImmediate(emulator, Register.E, data);

        [Instruction(0x26, OperandType.Data8Bit)]
        public static readonly Action<Emulator, byte> MVI_H = (emulator, data) => MoveImmediate(emulator, Register.H, data);

        [Instruction(0x2E, OperandType.Data8Bit)]
        public static readonly Action<Emulator, byte> MVI_L = (emulator, data) => MoveImmediate(emulator, Register.L, data);

        [Instruction(0x36, OperandType.Data8Bit)]
        public static readonly Action<Emulator, byte> MVI_M = (emulator, data) => MoveImmediate(emulator, null, data);

        [Instruction(0x00, OperandType.None)]
        public static readonly Action NOP = () => { };

        [Instruction(0xB0, OperandType.RegisterOrMemory, Description = "Logical OR with accumulator")]
        public static readonly Action<Emulator, Register?> ORA = (emulator, register) =>
        {
            var current = register.HasValue ? emulator[register.Value] : emulator.GetHLMemoryValue();
            current |= emulator[Register.A];

            emulator[Flag.AC] = false;
            emulator[Flag.C] = false;
            emulator[Flag.P] = current.ParityFlag();
            emulator[Flag.Z] = current == 0;
            emulator[Flag.S] = current.SignFlag();

            emulator[Register.A] = current;
        };

        [Instruction(0xF6, OperandType.Data8Bit)]
        public static readonly object ORI;

        [Instruction(0xD3, OperandType.Data8Bit)]
        public static readonly object OUT;

        [Instruction(0xE9, OperandType.None)]
        public static readonly object PCHL;

        [Instruction(0xC1, OperandType.RegisterPairOrProgramStatusWord, Description = "Pop off stack to register pair")]
        public static readonly Action<Emulator, Register?> POP = (emulator, register) =>
        {
            var value = emulator[emulator.StackPointer];

            if (register.HasValue)
            {
                emulator[register.Value + 1] = value;
            }

            emulator.StackPointer++;
            value = emulator[emulator.StackPointer];

            if (register.HasValue)
            {
                emulator[register.Value] = value;
            }

            emulator.StackPointer++;
        };

        [Instruction(0xC5, OperandType.RegisterPairOrProgramStatusWord, Description = "Push register pair onto stack")]
        public static readonly Action<Emulator, Register?> PUSH = (emulator, register) =>
        {
            emulator.StackPointer--;

            if (register.HasValue)
            {
                emulator[emulator.StackPointer] = emulator[register.Value];
            }

            emulator.StackPointer--;

            if (register.HasValue)
            {
                emulator[emulator.StackPointer] = emulator[register.Value + 1];
            }
        };

        [Instruction(0x17, OperandType.None)]
        public static readonly object RAL;

        [Instruction(0x1F, OperandType.None)]
        public static readonly object RAR;

        [Instruction(0x07, OperandType.None)]
        public static readonly object RLC;

        [Instruction(0x0F, OperandType.None)]
        public static readonly object RRC;

        [Instruction(0xC9, OperandType.None)]
        public static readonly object RET;

        [Instruction(0xD8, OperandType.None)]
        public static readonly object RC;

        [Instruction(0xD0, OperandType.None)]
        public static readonly object RNC;

        [Instruction(0xF0, OperandType.None)]
        public static readonly object RP;

        [Instruction(0xF8, OperandType.None)]
        public static readonly object RM;

        [Instruction(0xE8, OperandType.None)]
        public static readonly object RPE;

        [Instruction(0xE0, OperandType.None)]
        public static readonly object RPO;

        [Instruction(0xC8, OperandType.None)]
        public static readonly object RZ;

        [Instruction(0xC0, OperandType.None)]
        public static readonly object RNZ;

        [Instruction(0x20, OperandType.None)]
        public static readonly object RIM;

        [Instruction(0xC7, OperandType.Index)]
        public static readonly object RST;

        [Instruction(0x98, OperandType.RegisterOrMemory, Description = "Subtract source and Borrow from accumulator")]
        public static readonly Action<Emulator, Register?> SBB = (emulator, register) =>
        {
            var decrement = register.HasValue ? emulator[register.Value] : emulator.GetHLMemoryValue();
            decrement += (byte)(emulator[Flag.C] ? 1 : 0);
            IncrementSourceBy(emulator, Register.A, -decrement);
            emulator[Flag.C] = false;
        };

        [Instruction(0xDE, OperandType.Data8Bit, Description = "Subtract immediate with borrow")]
        public static readonly Action<Emulator, byte> SBI = (emulator, value) =>
        {
            var decrement = (byte)(value + (emulator[Flag.C] ? 1 : 0));
            IncrementSourceBy(emulator, Register.A, -decrement);
            emulator[Flag.C] = false;
        };

        [Instruction(0x22, OperandType.Data16Bit)]
        public static readonly Action<Emulator, byte, byte> SHLD = (emulator, lower, upper) =>
        {
            var address = (upper << 8) + lower;
            emulator[(ushort)(address)] = emulator[Register.L];
            emulator[(ushort)(address + 1)] = emulator[Register.H];
        };

        [Instruction(0x30, OperandType.None)]
        public static readonly object SIM;

        [Instruction(0xF9, OperandType.None)]
        public static readonly object SPHL;

        [Instruction(0x32, OperandType.Data16Bit, Description = "Store accumulator direct")]
        public static readonly Action<Emulator, byte, byte> STA = (emulator, lower, upper) =>
        {
            var address = (upper << 8) + lower;
            emulator[(ushort)(address)] = emulator[Register.A];
        };

        [Instruction(0x02, OperandType.RegisterBD, Description = "Store accumulator indirect")]
        public static readonly Action<Emulator, Register> STAX = (emulator, register) =>
        {
            var address = (emulator[register] << 8) + emulator[register + 1];
            emulator[(ushort)address] = emulator[Register.A];
        };

        [Instruction(0x37, OperandType.None)]
        public static readonly Action<Emulator> STC = emulator => emulator[Flag.C] = true;

        [Instruction(0x90, OperandType.RegisterOrMemory, Description = "Subtract register or memory from accumulator")]
        public static readonly Action<Emulator, Register?> SUB = (emulator, register) =>
        {
            IncrementSourceBy(emulator, Register.A, -(register.HasValue ? emulator[register.Value] : emulator.GetHLMemoryValue()));
        };

        [Instruction(0xD6, OperandType.Data8Bit, Description = "Subtract immediate from accumulator")]
        public static readonly Action<Emulator, byte> SUI = (emulator, value) =>
        {
            IncrementSourceBy(emulator, Register.A, -value);
        };

        [Instruction(0xEB, OperandType.None)]
        public static readonly object XCHG;

        [Instruction(0xA8, OperandType.RegisterOrMemory, Description = "Exclusive OR with accumulator")]
        public static readonly Action<Emulator, Register?> XRA = (emulator, register) =>
        {
            var current = register.HasValue ? emulator[register.Value] : emulator.GetHLMemoryValue();
            current ^= emulator[Register.A];

            emulator[Flag.AC] = false;
            emulator[Flag.C] = false;
            emulator[Flag.P] = current.ParityFlag();
            emulator[Flag.Z] = current == 0;
            emulator[Flag.S] = current.SignFlag();

            emulator[Register.A] = current;
        };

        [Instruction(0xEE, OperandType.Data8Bit)]
        public static readonly object XRI;

        [Instruction(0xE3, OperandType.None)]
        public static readonly Action<Emulator> XTHL = emulator =>
        {
            var stackPointer = emulator[emulator.StackPointer];
            emulator[emulator.StackPointer] = emulator[Register.L];
            emulator[Register.L] = stackPointer;

            stackPointer = emulator[(ushort)(emulator.StackPointer + 1)];
            emulator[(ushort)(emulator.StackPointer + 1)] = emulator[Register.H];
            emulator[Register.H] = stackPointer;
        };

        private static void CopyFromSourceToDestination(Emulator emulator, Register? source, Register destination)
        {
            emulator[destination] = source.HasValue ? emulator[source.Value] : emulator.GetHLMemoryValue();
        }

        private static void JumpTo(Emulator emulator, byte lower, byte upper, bool condition)
        {
            ushort address = (ushort)((upper << 8) + lower);
            emulator.ProgramCounter = condition ? address : emulator.ProgramCounter;
        }

        private static void IncrementSourceBy(Emulator emulator, Register? register, int increment)
        {
            var memoryAddress = emulator.Get16BitValue(Register.H, Register.L);
            var current = register.HasValue ? emulator[register.Value] : emulator[memoryAddress];

            emulator[Flag.AC] = current.AuxiliaryCarryFlag((byte)(increment < 0 ? (-increment) : increment), increment > 0);
            current = (byte)(current + increment);
            emulator[Flag.P] = current.ParityFlag();
            emulator[Flag.Z] = current == 0;
            emulator[Flag.S] = current.SignFlag();

            if (register.HasValue)
            {
                emulator[register.Value] = current;
            }
            else
            {
                emulator[memoryAddress] = current;
            }
        }

        private static void MoveImmediate(Emulator emulator, Register? register, byte value)
        {
            if (register.HasValue)
            {
                emulator[register.Value] = value;
            }
            else
            {
                emulator[emulator.Get16BitValue(Register.H, Register.L)] = value;
            }
        }
    }
}
