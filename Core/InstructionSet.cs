﻿using System;

namespace Core
{
    static class InstructionSet
    {
        [Instruction(0xCE, OperandType.Data8Bit, Description = "Add immediate to accumulator with carry")]
        public static readonly object ACI;

        [Instruction(0x88, OperandType.RegisterOrMemory, Description = "Add register to accumulator with carry")] // Distance = 1
        public static readonly object ADC;

        [Instruction(0x80, OperandType.RegisterOrMemory)]
        public static readonly object ADD;

        [Instruction(0xC6, OperandType.Data8Bit)]
        public static readonly object ADI;

        [Instruction(0xA0, OperandType.RegisterOrMemory)]
        public static readonly object ANA;

        [Instruction(0xE6, OperandType.Data8Bit)]
        public static readonly object ANI;

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

        [Instruction(0x2F, OperandType.None)]
        public static readonly object CMA;

        [Instruction(0x3F, OperandType.None)]
        public static readonly object CMC;

        [Instruction(0xB8, OperandType.RegisterOrMemory)]
        public static readonly object CMP;

        [Instruction(0xFE, OperandType.Data8Bit)]
        public static readonly object CPI;

        [Instruction(0x27, OperandType.None)]
        public static readonly object DAA;

        [Instruction(0x09, OperandType.RegisterPairOrStackPointer)]
        public static readonly object DAD;

        [Instruction(0x05, OperandType.RegisterOrMemory, InstructionSpacing = 0x08)]
        public static readonly object DCR;

        [Instruction(0x0B, OperandType.RegisterPairOrStackPointer)]
        public static readonly object DCX;

        [Instruction(0xF3, OperandType.None)]
        public static readonly object DI;

        [Instruction(0xFB, OperandType.None)]
        public static readonly object EI;

        [Instruction(0x76, OperandType.None)]
        public static readonly object HLT;

        [Instruction(0xDB, OperandType.Data8Bit)]
        public static readonly object IN;

        [Instruction(0x04, OperandType.RegisterOrMemory, InstructionSpacing = 0x08)]
        public static readonly Action<Emulator, Register?> INR = (emulator, reg) =>
        {
            if (reg.HasValue)
            {
                var register = reg.Value;
                emulator.UpdateAuxiliaryCarryFlag(emulator[register], 1, true);

                emulator[register]++;
                emulator.UpdateParityFlag(emulator[register]);
                emulator.UpdateZeroFlag(emulator[register]);
                emulator.UpdateSignFlag(emulator[register]);
            }
            else
            {
                INX(emulator, Register.H);
            }
        };


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

        [Instruction(0xC3, OperandType.LabelAs16BitAddress)]
        public static readonly object JMP;

        [Instruction(0xDA, OperandType.LabelAs16BitAddress)]
        public static readonly object JC;

        [Instruction(0xD2, OperandType.LabelAs16BitAddress)]
        public static readonly object JNC;

        [Instruction(0xF2, OperandType.LabelAs16BitAddress)]
        public static readonly object JP;

        [Instruction(0xFA, OperandType.LabelAs16BitAddress)]
        public static readonly object JM;

        [Instruction(0xEA, OperandType.LabelAs16BitAddress)]
        public static readonly object JPE;

        [Instruction(0xE2, OperandType.LabelAs16BitAddress)]
        public static readonly object JPO;

        [Instruction(0xCA, OperandType.LabelAs16BitAddress)]
        public static readonly object JZ;

        [Instruction(0xC2, OperandType.LabelAs16BitAddress)]
        public static readonly object JNZ;

        [Instruction(0x3A, OperandType.Data16Bit)]
        public static readonly object LDA;

        [Instruction(0x0A, OperandType.RegisterBD)]
        public static readonly object LDAX;

        [Instruction(0x2A, OperandType.Data16Bit)]
        public static readonly object LHLD;

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
        public static readonly object LXI_SP;

        [Instruction(0x78, OperandType.RegisterOrMemory)]
        public static readonly object MOV_A;

        [Instruction(0x40, OperandType.RegisterOrMemory)]
        public static readonly object MOV_B;

        [Instruction(0x48, OperandType.RegisterOrMemory)]
        public static readonly object MOV_C;

        [Instruction(0x50, OperandType.RegisterOrMemory)]
        public static readonly object MOV_D;

        [Instruction(0x58, OperandType.RegisterOrMemory)]
        public static readonly object MOV_E;

        [Instruction(0x60, OperandType.RegisterOrMemory)]
        public static readonly object MOV_H;

        [Instruction(0x68, OperandType.RegisterOrMemory)]
        public static readonly object MOV_L;

        [Instruction(0x70, OperandType.Register)]
        public static readonly object MOV_M;

        [Instruction(0x3E, OperandType.Data8Bit)]
        public static readonly object MVI_A;

        [Instruction(0x06, OperandType.Data8Bit)]
        public static readonly object MVI_B;

        [Instruction(0x0E, OperandType.Data8Bit)]
        public static readonly object MVI_C;

        [Instruction(0x16, OperandType.Data8Bit)]
        public static readonly object MVI_D;

        [Instruction(0x1E, OperandType.Data8Bit)]
        public static readonly object MVI_E;

        [Instruction(0x26, OperandType.Data8Bit)]
        public static readonly object MVI_H;

        [Instruction(0x2E, OperandType.Data8Bit)]
        public static readonly object MVI_L;

        [Instruction(0x36, OperandType.Data8Bit)]
        public static readonly object MVI_M;

        [Instruction(0x00, OperandType.None)]
        public static readonly Action NOP = () => { };

        [Instruction(0xB0, OperandType.RegisterOrMemory)]
        public static readonly object ORA;

        [Instruction(0xF6, OperandType.Data8Bit)]
        public static readonly object ORI;

        [Instruction(0xD3, OperandType.Data8Bit)]
        public static readonly object OUT;

        [Instruction(0xE9, OperandType.None)]
        public static readonly object PCHL;

        [Instruction(0xC1, OperandType.RegisterPairOrProgramStatusWord)]
        public static readonly object POP;

        [Instruction(0xC5, OperandType.RegisterPairOrProgramStatusWord)]
        public static readonly object PUSH;

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

        [Instruction(0x98, OperandType.RegisterOrMemory)]
        public static readonly object SBB;

        [Instruction(0xDE, OperandType.Data8Bit)]
        public static readonly object SBI;

        [Instruction(0x22, OperandType.Data16Bit)]
        public static readonly object SHLD;

        [Instruction(0x30, OperandType.None)]
        public static readonly object SIM;

        [Instruction(0xF9, OperandType.None)]
        public static readonly object SPHL;

        [Instruction(0x32, OperandType.Data16Bit)]
        public static readonly object STA;

        [Instruction(0x02, OperandType.RegisterBD)]
        public static readonly object STAX;

        [Instruction(0x37, OperandType.None)]
        public static readonly object STC;

        [Instruction(0x90, OperandType.RegisterOrMemory)]
        public static readonly object SUB;

        [Instruction(0xD6, OperandType.Data8Bit)]
        public static readonly object SUI;

        [Instruction(0xEB, OperandType.None)]
        public static readonly object XCHG;

        [Instruction(0xA8, OperandType.RegisterOrMemory)]
        public static readonly object XRA;

        [Instruction(0xEE, OperandType.Data8Bit)]
        public static readonly object XRI;

        [Instruction(0xE3, OperandType.None)]
        public static readonly object XTHL;
    }
}