using System.Linq;
using System.Collections.Generic;
using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    [TestClass]
    public class _emulatorTests
    {
        Emulator _emulator;

        [TestInitialize]
        public void Initialize()
        {
            _emulator = new Emulator();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _emulator = null;
        }

        class ProgramOptions
        {
            public ProgramOptions() { }

            public ProgramOptions(int instructionSize, Register[] affectedRegisters, Flag[] affectedFlags)
            {
                InstructionSize = instructionSize;
                AffectedRegisters = new SortedSet<Register>(affectedRegisters);
                AffectedFlags = new SortedSet<Flag>(affectedFlags);
            }

            public ProgramOptions(int instructionSize, params Register[] affectedRegisters)
                : this(instructionSize, affectedRegisters, new Flag[] { })
            { }

            public int InstructionSize = 1;
            public SortedSet<Register> AffectedRegisters = new SortedSet<Register>();
            public SortedSet<Flag> AffectedFlags = new SortedSet<Flag>();
        }

        private void SetProgramAndStep(ProgramOptions options, params byte[] program)
        {
            var changedFlags = new SortedSet<Flag>();
            var changedRegisters = new SortedSet<Register>();

            _emulator.FlagChanged += (_, args) => { changedFlags.Add(args.Flag); };
            _emulator.RegisterChanged += (_, args) => { changedRegisters.Add(args.Register); };

            _emulator.SetMemory(0x400, program);
            _emulator.ProgramCounter = 0x400;
            _emulator.Step();

            Assert.AreEqual(0x400 + options.InstructionSize, _emulator.ProgramCounter);

            CollectionAssert.AreEqual(options.AffectedFlags, changedFlags);
            CollectionAssert.AreEqual(options.AffectedRegisters, changedRegisters);
        }

        // ------------------ 0x00 - 0x0F

        [TestMethod] public void NOP() { SetProgramAndStep(new ProgramOptions(), 0x00); }
        [TestMethod] public void LXI_B() { LXI(0x01, Register.B, Register.C); }
        [TestMethod] public void STAX_B() { STAX(0x02, Register.B, Register.C); }
        [TestMethod] public void INX_B() { INX(0x03, Register.B, Register.C); }
        [TestMethod] public void INR_B() { INR(0x04, Register.B); }
        [TestMethod] public void DCR_B() { DCR(0x05, Register.B); }
        [TestMethod] public void MVI_B() { MVI(0x06, Register.B); }
        [TestMethod]
        public void RLC()
        {
            _emulator[Register.A] = 0xA7;
            SetProgramAndStep(new ProgramOptions(1, new Register[] { Register.A }, new Flag[] { Flag.C }), 0x07);
            Assert.AreEqual(0x4F, _emulator[Register.A]);
        }
        [TestMethod, ExpectedException(typeof(InvalidOperationException))] public void Invalid_0x08() { SetProgramAndStep(new ProgramOptions(), 0x08); }
        [TestMethod] public void DAD_B() { DAD(0x09, Register.B, Register.C); }
        [TestMethod] public void LDAX_B() { LDAX(0x0A, Register.B, Register.C); }
        [TestMethod] public void DCX_B() { DCX(0x0B, Register.B, Register.C); }
        [TestMethod] public void INR_C() { INR(0x0C, Register.C); }
        [TestMethod] public void DCR_C() { DCR(0x0D, Register.C); }
        [TestMethod] public void MVI_C() { MVI(0x0E, Register.C); }
        [TestMethod]
        public void RRC()
        {
            _emulator[Register.A] = 0xA7;
            SetProgramAndStep(new ProgramOptions(1, new Register[] { Register.A }, new Flag[] { Flag.C }), 0x0F);
            Assert.AreEqual(0xD3, _emulator[Register.A]);
        }

        // ------------------ 0x10 - 0x1F

        [TestMethod, ExpectedException(typeof(InvalidOperationException))] public void Invalid_0x10() { SetProgramAndStep(new ProgramOptions(), 0x10); }
        [TestMethod] public void LXI_D() { LXI(0x11, Register.D, Register.E); }
        [TestMethod] public void STAX_D() { STAX(0x12, Register.D, Register.E); }
        [TestMethod] public void INX_D() { INX(0x13, Register.D, Register.E); }
        [TestMethod] public void INR_D() { INR(0x14, Register.D); }
        [TestMethod] public void DCR_D() { DCR(0x15, Register.D); }
        [TestMethod] public void MVI_D() { MVI(0x16, Register.D); }
        [TestMethod]
        public void RAL()
        {
            _emulator[Register.A] = 0xA7;
            SetProgramAndStep(new ProgramOptions(1, new Register[] { Register.A }, new Flag[] { Flag.C }), 0x17);
            Assert.AreEqual(0x4E, _emulator[Register.A]);
        }
        [TestMethod, ExpectedException(typeof(InvalidOperationException))] public void Invalid_0x18() { SetProgramAndStep(new ProgramOptions(), 0x18); }
        [TestMethod] public void DAD_D() { DAD(0x19, Register.D, Register.E); }
        [TestMethod] public void LDAX_D() { LDAX(0x1A, Register.D, Register.E); }
        [TestMethod] public void DCX_D() { DCX(0x1B, Register.D, Register.E); }
        [TestMethod] public void INR_E() { INR(0x1C, Register.E); }
        [TestMethod] public void DCR_E() { DCR(0x1D, Register.E); }
        [TestMethod] public void MVI_E() { MVI(0x1E, Register.E); }
        [TestMethod]
        public void RAR()
        {
            _emulator[Register.A] = 0xA7;
            SetProgramAndStep(new ProgramOptions(1, new Register[] { Register.A }, new Flag[] { Flag.C }), 0x1F);
            Assert.AreEqual(0x53, _emulator[Register.A]);
        }

        // ------------------ 0x20 - 0x2F

        [TestMethod]
        public void RIM()
        {
            _emulator.InterruptMask = 0x24;
            SetProgramAndStep(new ProgramOptions(1, Register.A), 0x20);
            Assert.AreEqual(0x24, _emulator[Register.A]);
        }
        [TestMethod] public void LXI_H() { LXI(0x21, Register.H, Register.L); }
        [TestMethod]
        public void SHLD()
        {
            _emulator[Register.H] = 0x01;
            _emulator[Register.L] = 0x02;

            SetProgramAndStep(new ProgramOptions(3), 0x22, 0x50, 0x20);

            Assert.AreEqual(0x02, _emulator[0x2050]);
            Assert.AreEqual(0x01, _emulator[0x2051]);
        }
        [TestMethod] public void INX_H() { INX(0x23, Register.H, Register.L); }
        [TestMethod] public void INR_H() { INR(0x24, Register.H); }
        [TestMethod] public void DCR_H() { DCR(0x25, Register.H); }
        [TestMethod] public void MVI_H() { MVI(0x26, Register.H); }
        [TestMethod] public void DAA() { throw new NotImplementedException(); }
        [TestMethod, ExpectedException(typeof(InvalidOperationException))] public void Invalid_0x28() { SetProgramAndStep(new ProgramOptions(), 0x28); }
        [TestMethod] public void DAD_H() { DAD(0x29, Register.H, Register.L); }
        [TestMethod]
        public void LHLD()
        {
            _emulator[0x2050] = 0x01;
            _emulator[0x2051] = 0x02;

            SetProgramAndStep(new ProgramOptions(3, Register.H, Register.L), 0x2A, 0x50, 0x20);

            Assert.AreEqual(0x02, _emulator[Register.H]);
            Assert.AreEqual(0x01, _emulator[Register.L]);
        }
        [TestMethod] public void DCX_H() { DCX(0x2B, Register.H, Register.L); }
        [TestMethod] public void INR_L() { INR(0x2C, Register.L); }
        [TestMethod] public void DCR_L() { DCR(0x2D, Register.L); }
        [TestMethod] public void MVI_L() { MVI(0x2E, Register.L); }
        [TestMethod]
        public void CMA()
        {
            _emulator[Register.A] = 0x89;

            SetProgramAndStep(new ProgramOptions(1, Register.A), 0x2F);

            Assert.AreEqual(0x76, _emulator[Register.A]);
        }

        // ------------------ 0x30 - 0x3F

        [TestMethod]
        public void SIM()
        {
            _emulator[Register.A] = 0x24;
            SetProgramAndStep(new ProgramOptions(1), 0x30);
            Assert.AreEqual(0x24, _emulator.InterruptMask);
        }

        [TestMethod]
        public void LXI_SP()
        {
            SetProgramAndStep(new ProgramOptions(3), 0x31, 0x05, 0x20);
            Assert.AreEqual(0x2005, _emulator.StackPointer);
        }

        [TestMethod]
        public void STA()
        {
            _emulator[Register.A] = 0x9F;
            SetProgramAndStep(new ProgramOptions(3), 0x32, 0x50, 0x20);
            Assert.AreEqual(0x9F, _emulator[0x2050]);
        }

        [TestMethod]
        public void INX_SP()
        {
            _emulator.StackPointer = 0x2050;
            SetProgramAndStep(new ProgramOptions(1), 0x33);
            Assert.AreEqual(0x2051, _emulator.StackPointer);
        }
        [TestMethod]
        public void INR_M()
        {
            _emulator[Register.H] = 0x20;
            _emulator[Register.L] = 0x50;
            _emulator[0x2050] = 0xFF;
            _emulator[Flag.S] = true;
            SetProgramAndStep(new ProgramOptions(1, new Register[] { }, new Flag[] { Flag.S, Flag.Z, Flag.P, Flag.AC }), 0x34);
            Assert.AreEqual(0x00, _emulator[0x2050]);
        }

        [TestMethod]
        public void DCR_M()
        {
            _emulator[Register.H] = 0x20;
            _emulator[Register.L] = 0x50;
            _emulator[0x2050] = 0x00;
            _emulator[Flag.Z] = true;
            _emulator[Flag.AC] = true;
            SetProgramAndStep(new ProgramOptions(1, new Register[] { }, new Flag[] { Flag.S, Flag.Z, Flag.P, Flag.AC }), 0x35);
            Assert.AreEqual(0xFF, _emulator[0x2050]);
        }

        [TestMethod]
        public void MVI_M()
        {
            _emulator[Register.H] = 0x20;
            _emulator[Register.L] = 0x50;
            SetProgramAndStep(new ProgramOptions(2), 0x36, 0x92);
            Assert.AreEqual(0x92, _emulator[0x2050]);
        }

        [TestMethod]
        public void STC()
        {
            SetProgramAndStep(new ProgramOptions(1, new Register[] { }, new Flag[] { Flag.C }), 0x37);
            Assert.IsTrue(_emulator[Flag.C]);
        }
        [TestMethod, ExpectedException(typeof(InvalidOperationException))] public void Invalid_0x38() { SetProgramAndStep(new ProgramOptions(), 0x38); }

        [TestMethod]
        public void DAD_SP()
        {
            _emulator[Register.H] = 0x00;
            _emulator[Register.L] = 0x01;
            _emulator.StackPointer = 0xFFFF;

            SetProgramAndStep(new ProgramOptions(1, new Register[] { Register.L }, new Flag[] { Flag.C }), 0x39);
            Assert.IsTrue(_emulator[Flag.C]);
            Assert.AreEqual(0x00, _emulator[Register.H]);
            Assert.AreEqual(0x00, _emulator[Register.L]);
        }

        [TestMethod]
        public void LDA()
        {
            _emulator[0x2050] = 0xF8;

            SetProgramAndStep(new ProgramOptions(3, Register.A), 0x3A, 0x50, 0x20);

            Assert.AreEqual(0xF8, _emulator[Register.A]);
        }

        [TestMethod]
        public void DCX_SP()
        {
            _emulator.StackPointer = 0x0000;

            SetProgramAndStep(new ProgramOptions(1), 0x3B);
            Assert.AreEqual(0xFFFF, _emulator.StackPointer);
        }

        [TestMethod] public void INR_A() { INR(0x3C, Register.A); }
        [TestMethod] public void DCR_A() { DCR(0x3D, Register.A); }
        [TestMethod] public void MVI_A() { MVI(0x3E, Register.A); }
        [TestMethod]
        public void CMC()
        {
            _emulator[Flag.C] = true;

            SetProgramAndStep(new ProgramOptions(1, new Register[] { }, new Flag[] { Flag.C }), 0x3F);

            Assert.IsFalse(_emulator[Flag.C]);
        }

        // ------------------ 0x40 - 0x4F

        [TestMethod] public void MOV_B_B() => MOV(0x40, Register.B, Register.B);
        [TestMethod] public void MOV_B_C() => MOV(0x41, Register.B, Register.C);
        [TestMethod] public void MOV_B_D() => MOV(0x42, Register.B, Register.D);
        [TestMethod] public void MOV_B_E() => MOV(0x43, Register.B, Register.E);
        [TestMethod] public void MOV_B_H() => MOV(0x44, Register.B, Register.H);
        [TestMethod] public void MOV_B_L() => MOV(0x45, Register.B, Register.L);
        [TestMethod] public void MOV_B_M() => MOV(0x46, Register.B, null);
        [TestMethod] public void MOV_B_A() => MOV(0x47, Register.B, Register.A);
        [TestMethod] public void MOV_C_B() => MOV(0x48, Register.C, Register.B);
        [TestMethod] public void MOV_C_C() => MOV(0x49, Register.C, Register.C);
        [TestMethod] public void MOV_C_D() => MOV(0x4A, Register.C, Register.D);
        [TestMethod] public void MOV_C_E() => MOV(0x4B, Register.C, Register.E);
        [TestMethod] public void MOV_C_H() => MOV(0x4C, Register.C, Register.H);
        [TestMethod] public void MOV_C_L() => MOV(0x4D, Register.C, Register.L);
        [TestMethod] public void MOV_C_M() => MOV(0x4E, Register.C, null);
        [TestMethod] public void MOV_C_A() => MOV(0x4F, Register.C, Register.A);

        // ------------------ 0x50 - 0x5F

        [TestMethod] public void MOV_D_B() => MOV(0x50, Register.D, Register.B);
        [TestMethod] public void MOV_D_C() => MOV(0x51, Register.D, Register.C);
        [TestMethod] public void MOV_D_D() => MOV(0x52, Register.D, Register.D);
        [TestMethod] public void MOV_D_E() => MOV(0x53, Register.D, Register.E);
        [TestMethod] public void MOV_D_H() => MOV(0x54, Register.D, Register.H);
        [TestMethod] public void MOV_D_L() => MOV(0x55, Register.D, Register.L);
        [TestMethod] public void MOV_D_M() => MOV(0x56, Register.D, null);
        [TestMethod] public void MOV_D_A() => MOV(0x57, Register.D, Register.A);
        [TestMethod] public void MOV_E_B() => MOV(0x58, Register.E, Register.B);
        [TestMethod] public void MOV_E_C() => MOV(0x59, Register.E, Register.C);
        [TestMethod] public void MOV_E_D() => MOV(0x5A, Register.E, Register.D);
        [TestMethod] public void MOV_E_E() => MOV(0x5B, Register.E, Register.E);
        [TestMethod] public void MOV_E_H() => MOV(0x5C, Register.E, Register.H);
        [TestMethod] public void MOV_E_L() => MOV(0x5D, Register.E, Register.L);
        [TestMethod] public void MOV_E_M() => MOV(0x5E, Register.E, null);
        [TestMethod] public void MOV_E_A() => MOV(0x5F, Register.E, Register.A);

        // ------------------ 0x60 - 0x6F

        [TestMethod] public void MOV_H_B() => MOV(0x60, Register.H, Register.B);
        [TestMethod] public void MOV_H_C() => MOV(0x61, Register.H, Register.C);
        [TestMethod] public void MOV_H_D() => MOV(0x62, Register.H, Register.D);
        [TestMethod] public void MOV_H_E() => MOV(0x63, Register.H, Register.E);
        [TestMethod] public void MOV_H_H() => MOV(0x64, Register.H, Register.H);
        [TestMethod] public void MOV_H_L() => MOV(0x65, Register.H, Register.L);
        [TestMethod] public void MOV_H_M() => MOV(0x66, Register.H, null);
        [TestMethod] public void MOV_H_A() => MOV(0x67, Register.H, Register.A);
        [TestMethod] public void MOV_L_B() => MOV(0x68, Register.L, Register.B);
        [TestMethod] public void MOV_L_C() => MOV(0x69, Register.L, Register.C);
        [TestMethod] public void MOV_L_D() => MOV(0x6A, Register.L, Register.D);
        [TestMethod] public void MOV_L_E() => MOV(0x6B, Register.L, Register.E);
        [TestMethod] public void MOV_L_H() => MOV(0x6C, Register.L, Register.H);
        [TestMethod] public void MOV_L_L() => MOV(0x6D, Register.L, Register.L);
        [TestMethod] public void MOV_L_M() => MOV(0x6E, Register.L, null);
        [TestMethod] public void MOV_L_A() => MOV(0x6F, Register.L, Register.A);

        // ------------------ 0x70 - 0x7F

        [TestMethod] public void MOV_M_B() => MOV(0x70, Register.B);
        [TestMethod] public void MOV_M_C() => MOV(0x71, Register.C);
        [TestMethod] public void MOV_M_D() => MOV(0x72, Register.D);
        [TestMethod] public void MOV_M_E() => MOV(0x73, Register.E);
        [TestMethod] public void MOV_M_H() => MOV(0x74, Register.H);
        [TestMethod] public void MOV_M_L() => MOV(0x75, Register.L);
        [TestMethod]
        public void HLT()
        {
            _emulator.SetMemory(0x400, 0x76);
            _emulator.ProgramCounter = 0x400;
            _emulator.Run();

            Assert.AreEqual(0x400, _emulator.ProgramCounter);
        }
        [TestMethod] public void MOV_M_A() => MOV(0x77, Register.A);
        [TestMethod] public void MOV_A_B() => MOV(0x78, Register.A, Register.B);
        [TestMethod] public void MOV_A_C() => MOV(0x79, Register.A, Register.C);
        [TestMethod] public void MOV_A_D() => MOV(0x7A, Register.A, Register.D);
        [TestMethod] public void MOV_A_E() => MOV(0x7B, Register.A, Register.E);
        [TestMethod] public void MOV_A_H() => MOV(0x7C, Register.A, Register.H);
        [TestMethod] public void MOV_A_L() => MOV(0x7D, Register.A, Register.L);
        [TestMethod] public void MOV_A_M() => MOV(0x7E, Register.A, null);
        [TestMethod] public void MOV_A_A() => MOV(0x7F, Register.A, Register.A);

        // ------------------ 0x80 - 0x8F

        [TestMethod] public void ADD_B() => ADD(0x80, Register.B);
        [TestMethod] public void ADD_C() => ADD(0x81, Register.C);
        [TestMethod] public void ADD_D() => ADD(0x82, Register.D);
        [TestMethod] public void ADD_E() => ADD(0x83, Register.E);
        [TestMethod] public void ADD_H() => ADD(0x84, Register.H);
        [TestMethod] public void ADD_L() => ADD(0x85, Register.L);
        [TestMethod] public void ADD_M() => ADD(0x86, null);
        [TestMethod] public void ADD_A() => ADD(0x87, Register.A);
        [TestMethod] public void ADC_B() => ADC(0x88, Register.B);
        [TestMethod] public void ADC_C() => ADC(0x89, Register.C);
        [TestMethod] public void ADC_D() => ADC(0x8A, Register.D);
        [TestMethod] public void ADC_E() => ADC(0x8B, Register.E);
        [TestMethod] public void ADC_H() => ADC(0x8C, Register.H);
        [TestMethod] public void ADC_L() => ADC(0x8D, Register.L);
        [TestMethod] public void ADC_M() => ADC(0x8E, null);
        [TestMethod] public void ADC_A() => ADC(0x8F, Register.A);

        // ------------------ 0x90 - 0x9F

        [TestMethod] public void SUB_B() => SUB(0x90, Register.B);
        [TestMethod] public void SUB_C() => SUB(0x91, Register.C);
        [TestMethod] public void SUB_D() => SUB(0x92, Register.D);
        [TestMethod] public void SUB_E() => SUB(0x93, Register.E);
        [TestMethod] public void SUB_H() => SUB(0x94, Register.H);
        [TestMethod] public void SUB_L() => SUB(0x95, Register.L);
        [TestMethod] public void SUB_M() => SUB(0x96, null);
        [TestMethod] public void SUB_A() => SUB(0x97, Register.A);
        [TestMethod] public void SBB_B() => SBB(0x98, Register.B);
        [TestMethod] public void SBB_C() => SBB(0x99, Register.C);
        [TestMethod] public void SBB_D() => SBB(0x9A, Register.D);
        [TestMethod] public void SBB_E() => SBB(0x9B, Register.E);
        [TestMethod] public void SBB_H() => SBB(0x9C, Register.H);
        [TestMethod] public void SBB_L() => SBB(0x9D, Register.L);
        [TestMethod] public void SBB_M() => SBB(0x9E, null);
        [TestMethod] public void SBB_A() => SBB(0x9F, Register.A);

        // ------------------ 0xA0 - 0xAF

        [TestMethod] public void ANA_B() => ANA(0xA0, Register.B);
        [TestMethod] public void ANA_C() => ANA(0xA1, Register.C);
        [TestMethod] public void ANA_D() => ANA(0xA2, Register.D);
        [TestMethod] public void ANA_E() => ANA(0xA3, Register.E);
        [TestMethod] public void ANA_H() => ANA(0xA4, Register.H);
        [TestMethod] public void ANA_L() => ANA(0xA5, Register.L);
        [TestMethod] public void ANA_M() => ANA(0xA6, null);
        [TestMethod] public void ANA_A() => ANA(0xA7, Register.A);
        [TestMethod] public void XRA_B() => XRA(0xA8, Register.B);
        [TestMethod] public void XRA_C() => XRA(0xA9, Register.C);
        [TestMethod] public void XRA_D() => XRA(0xAA, Register.D);
        [TestMethod] public void XRA_E() => XRA(0xAB, Register.E);
        [TestMethod] public void XRA_H() => XRA(0xAC, Register.H);
        [TestMethod] public void XRA_L() => XRA(0xAD, Register.L);
        [TestMethod] public void XRA_M() => XRA(0xAE, null);
        [TestMethod] public void XRA_A() => XRA(0xAF, Register.A);

        // ------------------ 0xB0 - 0xBF

        [TestMethod] public void ORA_B() => ORA(0xB0, Register.B);
        [TestMethod] public void ORA_C() => ORA(0xB1, Register.C);
        [TestMethod] public void ORA_D() => ORA(0xB2, Register.D);
        [TestMethod] public void ORA_E() => ORA(0xB3, Register.E);
        [TestMethod] public void ORA_H() => ORA(0xB4, Register.H);
        [TestMethod] public void ORA_L() => ORA(0xB5, Register.L);
        [TestMethod] public void ORA_M() => ORA(0xB6, null);
        [TestMethod] public void ORA_A() => ORA(0xB7, Register.A);
        [TestMethod] public void CMP_B() => CMP(0xB8, Register.B);
        [TestMethod] public void CMP_C() => CMP(0xB9, Register.C);
        [TestMethod] public void CMP_D() => CMP(0xBA, Register.D);
        [TestMethod] public void CMP_E() => CMP(0xBB, Register.E);
        [TestMethod] public void CMP_H() => CMP(0xBC, Register.H);
        [TestMethod] public void CMP_L() => CMP(0xBD, Register.L);
        [TestMethod] public void CMP_M() => CMP(0xBE, null);
        [TestMethod] public void CMP_A() => CMP(0xBF, Register.A);

        // ------------------ 0xC0 - 0xCF

        [TestMethod] public void RNZ() => new NotImplementedException();
        [TestMethod] public void POP_B() => POP(0xC1, Register.B);
        [TestMethod] public void JNZ() => JMP(0xC2, Flag.Z, false);
        [TestMethod] public void JMP() => JMP(0xC3, null, null);
        [TestMethod] public void CNZ() => new NotImplementedException();
        [TestMethod] public void PUSH_B() => PUSH(0xC5, Register.B);
        [TestMethod]
        public void ADI()
        {
            _emulator[Register.A] = 0x4A;

            SetProgramAndStep(new ProgramOptions(2, new Register[] { Register.A }, new Flag[] { Flag.S, Flag.P, Flag.AC }), 0xC6, 0x59);

            Assert.AreEqual(0xA3, _emulator[Register.A]);
        }
        [TestMethod] public void RST_0() => new NotImplementedException();
        [TestMethod] public void RZ() => new NotImplementedException();
        [TestMethod] public void RET() => new NotImplementedException();
        [TestMethod] public void JZ() => JMP(0xCA, Flag.Z, true);
        [TestMethod, ExpectedException(typeof(InvalidOperationException))] public void Invalid_0xCB() { SetProgramAndStep(new ProgramOptions(), 0xCB); }
        [TestMethod] public void CZ() => new NotImplementedException();
        [TestMethod] public void CALL() => new NotImplementedException();
        [TestMethod]
        public void ACI()
        {
            _emulator[Register.A] = 0x26;
            _emulator[Flag.C] = true;

            SetProgramAndStep(new ProgramOptions(2, new Register[] { Register.A }, new Flag[] { Flag.P, Flag.C }), 0xCE, 0x57);
            Assert.AreEqual(0x7E, _emulator[Register.A]);
        }
        [TestMethod] public void RST_1() => new NotImplementedException();


        // ------------------ 0xD0 - 0xDF

        [TestMethod] public void RNC() => new NotImplementedException();
        [TestMethod] public void POP_D() => POP(0xD1, Register.D);
        [TestMethod] public void JNC() => JMP(0xC2, Flag.C, false);
        [TestMethod] public void OUT() => new NotImplementedException();
        [TestMethod] public void CNC() => new NotImplementedException();
        [TestMethod] public void PUSH_D() => PUSH(0xD5, Register.D);
        [TestMethod]
        public void SUI()
        {
            _emulator[Register.A] = 0x40;

            SetProgramAndStep(new ProgramOptions(2, new Register[] { Register.A }, new Flag[] { Flag.P }), 0xD6, 0x37);
            Assert.AreEqual(0x09, _emulator[Register.A]);
        }

        [TestMethod] public void RST_2() => new NotImplementedException();
        [TestMethod] public void RC() => new NotImplementedException();
        [TestMethod, ExpectedException(typeof(InvalidOperationException))] public void Invalid_0xD9() { SetProgramAndStep(new ProgramOptions(), 0xD9); }
        [TestMethod] public void JC() => JMP(0xDA, Flag.C, true);
        [TestMethod] public void IN() => new NotImplementedException();
        [TestMethod] public void CC() => new NotImplementedException();
        [TestMethod, ExpectedException(typeof(InvalidOperationException))] public void Invalid_0xDD() { SetProgramAndStep(new ProgramOptions(), 0xDD); }
        [TestMethod] public void SBI()
        {
            _emulator[Register.A] = 0x37;
            _emulator[Flag.C] = true;

            SetProgramAndStep(new ProgramOptions(2, new Register[] { Register.A }, new Flag[] { Flag.P, Flag.C }), 0xDE, 0x25);
            Assert.AreEqual(0x11, _emulator[Register.A]);
        }

        [TestMethod] public void RST_3() => new NotImplementedException();

        // ------------------ 0xE0 - 0xEF

        [TestMethod] public void RPO() => new NotImplementedException();
        [TestMethod] public void POP_H() => POP(0xE1, Register.H);
        [TestMethod] public void JPO() => JMP(0xE2, Flag.P, false);
        [TestMethod] public void XTHL()
        {
            _emulator[Register.H] = 0xA2;
            _emulator[Register.L] = 0x57;
            _emulator[0x2095] = 0x38;
            _emulator[0x2096] = 0x67;
            _emulator.StackPointer = 0x2095;

            SetProgramAndStep(new ProgramOptions(1, Register.H, Register.L), 0xE3);

            Assert.AreEqual(0x67, _emulator[Register.H]);
            Assert.AreEqual(0x38, _emulator[Register.L]);
            Assert.AreEqual(0x57, _emulator[0x2095]);
            Assert.AreEqual(0xA2, _emulator[0x2096]);
        }
        [TestMethod] public void CPO() => new NotImplementedException();
        [TestMethod] public void PUSH_H() => PUSH(0xE5, Register.H);
        [TestMethod] public void ANI()
        {
            _emulator[Register.A] = 0xA3;
            SetProgramAndStep(new ProgramOptions(2, new Register[] { Register.A }, new Flag[] { Flag.S, Flag.AC }), 0xE6, 0x97);

            Assert.AreEqual(0x83, _emulator[Register.A]);
        }
        [TestMethod] public void RST_4() => new NotImplementedException();
        [TestMethod] public void RPE() => new NotImplementedException();
        //[TestMethod] public void PCHL() => CompileAndCompare(@"PCHL", 0xE9);
        [TestMethod] public void JPE() => JMP(0xEA, Flag.P, true);
        //[TestMethod] public void XCHG() => CompileAndCompare(@"XCHG", 0xEB);
        [TestMethod] public void CPE() => new NotImplementedException();
        [TestMethod, ExpectedException(typeof(InvalidOperationException))] public void Invalid_0xED() { SetProgramAndStep(new ProgramOptions(), 0xED); }
        //[TestMethod] public void XRI() => CompileAndCompare(@"XRI 92H", 0xEE, 0x92);
        [TestMethod] public void RST_5() => new NotImplementedException();

        private void POP(byte opcode, Register? registerPair)
        {
            _emulator.StackPointer = 0x2090;
            _emulator[0x2090] = 0xF5;
            _emulator[0x2091] = 0x01;

            ProgramOptions options;

            if (registerPair.HasValue)
            {
                options = new ProgramOptions(1, registerPair.Value, registerPair.Value + 1);
            }
            else
            {
                // PSW will be modified
                options = new ProgramOptions(1, registerPair.Value, registerPair.Value + 1);
            }

            SetProgramAndStep(options, opcode);
            Assert.AreEqual(0x2092, _emulator.StackPointer);

            if (registerPair.HasValue)
            {
                Assert.AreEqual(0x01, _emulator[registerPair.Value]);
                Assert.AreEqual(0xF5, _emulator[registerPair.Value + 1]);
            }
        }

        private void PUSH(byte opcode, Register? registerPair)
        {
            _emulator.StackPointer = 0x2099;


            ProgramOptions options;

            if (registerPair.HasValue)
            {
                _emulator[registerPair.Value] = 0x32;
                _emulator[registerPair.Value + 1] = 0x57;
                options = new ProgramOptions(1);
            }
            else
            {
                // PSW will be modified
                options = new ProgramOptions(1, registerPair.Value, registerPair.Value + 1);
            }

            SetProgramAndStep(options, opcode);
            Assert.AreEqual(0x2097, _emulator.StackPointer);

            if (registerPair.HasValue)
            {
                Assert.AreEqual(0x57, _emulator[0x2097]);
                Assert.AreEqual(0x32, _emulator[0x2098]);
            }
        }

        private void JMP(byte opcode, Flag? flag, bool? val)
        {
            if (flag.HasValue)
            {
                _emulator[flag.Value] = val.Value;
            }
            SetProgramAndStep(new ProgramOptions(0x2050 - 0x400), opcode, 0x50, 0x20);

            Assert.AreEqual(0x2050, _emulator.ProgramCounter);
        }

        private void MOV(byte opcode, Register destination, Register? source)
        {
            if (source.HasValue)
            {
                _emulator[source.Value] = 0x34;
            }
            else
            {
                _emulator[Register.H] = 0x20;
                _emulator[Register.L] = 0x50;
                _emulator[0x2050] = 0x34;
            }

            var options = new ProgramOptions(1, source == destination ? new Register[] { } : new Register[] { destination });

            SetProgramAndStep(options, opcode);
            Assert.AreEqual(0x34, _emulator[destination]);
        }

        private void ANA(byte opcode, Register? register)
        {
            if (register.HasValue)
            {
                _emulator[register.Value] = 0x82;
            }
            else
            {
                _emulator[Register.H] = 0x20;
                _emulator[Register.L] = 0x50;
                _emulator[0x2050] = 0x82;
            }
            _emulator[Register.A] = 0x54;
            SetProgramAndStep(new ProgramOptions(1,
                register == Register.A ? new Register[] { } : new Register[] { Register.A },
                register == Register.A ? new Flag[] { Flag.AC } : new Flag[] { Flag.Z, Flag.AC, Flag.P }), opcode);
            Assert.AreEqual(register == Register.A ? 0x54 : 0x00, _emulator[Register.A]);
        }

        private void ORA(byte opcode, Register? register)
        {
            if (register.HasValue)
            {
                _emulator[register.Value] = 0x81;
            }
            else
            {
                _emulator[Register.H] = 0x20;
                _emulator[Register.L] = 0x50;
                _emulator[0x2050] = 0x81;
            }
            _emulator[Register.A] = 0x03;
            SetProgramAndStep(new ProgramOptions(1,
                register == Register.A ? new Register[] { } : new Register[] { Register.A },
                register == Register.A ? new Flag[] { Flag.P } : new Flag[] { Flag.S }), opcode);
            Assert.AreEqual(register == Register.A ? 0x03 : 0x83, _emulator[Register.A]);
        }

        private void CMP(byte opcode, Register? register)
        {
            if (register.HasValue)
            {
                _emulator[register.Value] = 0x62;
            }
            else
            {
                _emulator[Register.H] = 0x20;
                _emulator[Register.L] = 0x50;
                _emulator[0x2050] = 0x62;
            }
            _emulator[Register.A] = 0x57;
            SetProgramAndStep(new ProgramOptions(1,
                new Register[] { },
                register == Register.A ? new Flag[] { Flag.Z, Flag.P } : new Flag[] { Flag.S, Flag.P, Flag.C }), opcode);
        }

        private void XRA(byte opcode, Register? register)
        {
            if (register.HasValue)
            {
                _emulator[register.Value] = 0x56;
            }
            else
            {
                _emulator[Register.H] = 0x20;
                _emulator[Register.L] = 0x50;
                _emulator[0x2050] = 0x56;
            }
            _emulator[Register.A] = 0x77;
            SetProgramAndStep(new ProgramOptions(1,
                new Register[] { Register.A },
                register == Register.A ? new Flag[] { Flag.Z, Flag.P } : new Flag[] { Flag.P }), opcode);
            Assert.AreEqual(register == Register.A ? 0x00 : 0x21, _emulator[Register.A]);
        }

        private void ADD(byte opcode, Register? register)
        {
            if (register.HasValue)
            {
                _emulator[register.Value] = 0x51;
            }
            else
            {
                _emulator[Register.H] = 0x20;
                _emulator[Register.L] = 0x50;
                _emulator[0x2050] = 0x51;
            }
            _emulator[Register.A] = 0x47;
            SetProgramAndStep(new ProgramOptions(1, new Register[] { Register.A }, register == Register.A ? new Flag[] { Flag.S, Flag.P } : new Flag[] { Flag.S }), opcode);
            Assert.AreEqual(register == Register.A ? 0x8E : 0x98, _emulator[Register.A]);
            Assert.IsTrue(_emulator[Flag.S]);
        }

        private void SUB(byte opcode, Register? register)
        {
            if (register.HasValue)
            {
                _emulator[register.Value] = 0x40;
            }
            else
            {
                _emulator[Register.H] = 0x20;
                _emulator[Register.L] = 0x50;
                _emulator[0x2050] = 0x40;
            }
            _emulator[Register.A] = 0x37;
            SetProgramAndStep(new ProgramOptions(1, new Register[] { Register.A }, register == Register.A ? new Flag[] { Flag.Z, Flag.P } : new Flag[] { Flag.S }), opcode);
            Assert.AreEqual(register == Register.A ? 0x00 : 0xF7, _emulator[Register.A]);
        }

        private void SBB(byte opcode, Register? register)
        {
            _emulator[Flag.C] = true;
            if (register.HasValue)
            {
                _emulator[register.Value] = 0x3F;
            }
            else
            {
                _emulator[Register.H] = 0x20;
                _emulator[Register.L] = 0x50;
                _emulator[0x2050] = 0x3F;
            }
            _emulator[Register.A] = 0x37;
            SetProgramAndStep(new ProgramOptions(1, new Register[] { Register.A }, register == Register.A ? new Flag[] { Flag.C, Flag.S, Flag.P } : new Flag[] { Flag.S, Flag.C }), opcode);
            Assert.AreEqual(register == Register.A ? 0xFF : 0xF7, _emulator[Register.A]);
        }

        private void ADC(byte opcode, Register? register)
        {
            if (register.HasValue)
            {
                _emulator[register.Value] = 0xA1;
            }
            else
            {
                _emulator[Register.H] = 0x20;
                _emulator[Register.L] = 0x50;
                _emulator[0x2050] = 0xA1;
            }
            _emulator[Register.A] = 0x98;
            _emulator[Flag.C] = true;
            SetProgramAndStep(new ProgramOptions(1, new Register[] { Register.A }, register == Register.A ? new Flag[] { Flag.C } : new Flag[] { Flag.C, Flag.P }), opcode);
            Assert.AreEqual(register == Register.A ? 0x31 : 0x3A, _emulator[Register.A]);
            Assert.IsFalse(_emulator[Flag.C]);
        }

        private void MOV(byte opcode, Register source)
        {
            _emulator[source] = 0x34;
            _emulator[Register.H] = 0x20;
            _emulator[Register.L] = 0x50;

            SetProgramAndStep(new ProgramOptions(1), opcode);
            Assert.AreEqual(source == Register.H ? 0x20 : (source == Register.L ? 0x50 : 0x34), _emulator[0x2050]);
        }

        void LXI(byte opcode, Register upper, Register lower)
        {
            SetProgramAndStep(new ProgramOptions(3, lower, upper), opcode, 0x05, 0x20);
            Assert.AreEqual(0x20, _emulator[upper]);
            Assert.AreEqual(0x05, _emulator[lower]);
        }

        void STAX(byte opcode, Register upper, Register lower)
        {
            _emulator[Register.A] = 0xF9;
            _emulator[upper] = 0x20;
            _emulator[lower] = 0x50;
            SetProgramAndStep(new ProgramOptions(1), opcode);
            Assert.AreEqual(0xF9, _emulator[0x2050]);
        }

        void INX(byte opcode, Register upper, Register lower)
        {
            _emulator[upper] = 0x9F;
            _emulator[lower] = 0xFF;
            SetProgramAndStep(new ProgramOptions(1, lower, upper), opcode);
            Assert.AreEqual(0xA0, _emulator[upper]);
            Assert.AreEqual(0x00, _emulator[lower]);
        }

        void INR(byte opcode, Register register)
        {
            // Test that Carry flag is not set
            _emulator[register] = 0xFF;
            _emulator[Flag.S] = true;
            SetProgramAndStep(new ProgramOptions(1, new Register[] { register }, new Flag[] { Flag.S, Flag.Z, Flag.P, Flag.AC }), opcode);
            Assert.AreEqual(0x00, _emulator[register]);
            Assert.IsFalse(_emulator[Flag.S]);
            Assert.IsTrue(_emulator[Flag.Z]);
            Assert.IsTrue(_emulator[Flag.P]);
            Assert.IsTrue(_emulator[Flag.AC]);
        }

        void DCR(byte opcode, Register register)
        {
            // Test that Carry flag is not set
            _emulator[register] = 0x00;
            _emulator[Flag.Z] = true;
            _emulator[Flag.AC] = true;
            SetProgramAndStep(new ProgramOptions(1, new Register[] { register }, new Flag[] { Flag.S, Flag.Z, Flag.P, Flag.AC }), opcode);
            Assert.AreEqual(0xFF, _emulator[register]);
            Assert.IsTrue(_emulator[Flag.S]);
            Assert.IsFalse(_emulator[Flag.Z]);
            Assert.IsFalse(_emulator[Flag.AC]);
            Assert.IsTrue(_emulator[Flag.P]);
        }

        void DCX(byte opcode, Register upper, Register lower)
        {
            // Test that Carry flag is not set
            _emulator[upper] = 0xA2;
            _emulator[lower] = 0x34;
            SetProgramAndStep(new ProgramOptions(1, lower), opcode);
            Assert.AreEqual(0x33, _emulator[lower]);
        }

        private void DAD(byte opcode, Register upper, Register lower)
        {
            _emulator[Register.H] = 0xFF;
            _emulator[Register.L] = 0xFF;
            _emulator[lower] = 0x01;

            SetProgramAndStep(new ProgramOptions(1, new Register[] { Register.H, Register.L }, new Flag[] { Flag.C }), opcode);
            Assert.IsTrue(_emulator[Flag.C]);
            // In case we are adding HL to HL we are calculating 0xFF01 + 0xFF01 = 0xFE02
            Assert.AreEqual(upper == Register.H ? 0xFE : 0x00, _emulator[Register.H]);
            Assert.AreEqual(lower == Register.L ? 0x02 : 0x00, _emulator[Register.L]);
        }

        private void LDAX(byte opcode, Register upper, Register lower)
        {
            _emulator.SetMemory(0x2050, 0x9F);
            _emulator[upper] = 0x20;
            _emulator[lower] = 0x50;

            SetProgramAndStep(new ProgramOptions(1, Register.A), opcode);
            Assert.AreEqual(0x9F, _emulator[Register.A]);
        }

        private void MVI(byte opcode, Register register)
        {
            SetProgramAndStep(new ProgramOptions(2, register), opcode, 0x92);
            Assert.AreEqual(0x92, _emulator[register]);
        }

        [TestMethod]
        public void AddingTwo8BitNumbers()
        {
            var program = new byte[] {
                0x21, 0x05, 0x30,
                0x7E,
                0x23,
                0x86,
                0x23,
                0x77,
                0x76  // HLT
            };
            var halted = false;

            _emulator.Halted += (sender, args) => { halted = true; };
            _emulator.SetMemory(0x400, program);
            _emulator.SetMemory(0x3005, 0x14, 0x89);

            _emulator.ProgramCounter = 0x400;

            _emulator.Step(); // LXI H, 3005h

            Assert.AreEqual(0x403, _emulator.ProgramCounter);
            Assert.AreEqual(0x30, _emulator[Register.H]);
            Assert.AreEqual(0x05, _emulator[Register.L]);

            _emulator.Step(); // MOV A, M

            Assert.AreEqual(0x404, _emulator.ProgramCounter);
            Assert.AreEqual(0x14, _emulator[Register.A]);
            Assert.IsFalse(_emulator[Flag.P]);

            _emulator.Step(); // INX H

            Assert.AreEqual(0x405, _emulator.ProgramCounter);
            Assert.AreEqual(0x30, _emulator[Register.H]);
            Assert.AreEqual(0x06, _emulator[Register.L]);

            _emulator.Step(); // ADD M

            Assert.AreEqual(0x406, _emulator.ProgramCounter);
            Assert.AreEqual(0x9D, _emulator[Register.A]);
            Assert.IsFalse(_emulator[Flag.P]);

            _emulator.Step(); // INX H

            Assert.AreEqual(0x407, _emulator.ProgramCounter);
            Assert.AreEqual(0x30, _emulator[Register.H]);
            Assert.AreEqual(0x07, _emulator[Register.L]);

            _emulator.Step(); // MOV M, A

            Assert.AreEqual(0x408, _emulator.ProgramCounter);
            Assert.AreEqual(0x9D, _emulator[0x3007]);

            _emulator.Step();

            Assert.AreEqual(0x409, _emulator.ProgramCounter);
            Assert.IsTrue(halted);
        }

        [TestMethod]
        public void ExchangingMemoryLocations()
        {
            var program = new byte[] {
                0x3A, 0x00, 0x50, 0x47, 0x3A, 0x00, 0x60, 0x32,
                0x00, 0x50, 0x78, 0x32, 0x00, 0x60, 0x76
            };

            _emulator.SetMemory(0x400, program);
            _emulator[0x5000] = 0x22;
            _emulator[0x6000] = 0x44;

            _emulator.ProgramCounter = 0x400;

            _emulator.Run();

            Assert.AreEqual(0x44, _emulator[0x5000]);
            Assert.AreEqual(0x22, _emulator[0x6000]);
        }

        [TestMethod]
        public void SortNumbers()
        {
            var program = new byte[] {
                0x06, 0x09, // MVI B, 09
                // START: 
                0x21, 0x00, 0x30, // LXI H, 3000H
                0x0E, 0x09, // MVI C, 09H
                // BACK: 
                0x7E, // MOV A, M
                0x23, // INX H
                0xBE, // CMP M
                0xDA, 0x15, 0x00, // JC SKIP
                0xCA, 0x15, 0x00, // JZ SKIP
                0x56, // MOV D, M
                0x77, // MOV M, A
                0x2B, // DCX H
                0x72, // MOV M, D
                0x23, // INX H
                // SKIP:
                0x0D, // DCR C
                0xC2, 0x07,0x00, // JNZ BACK
                0x05, // DCR B
                0xC2, 0x02, 0x00, // JNZ START
                0x76
            };

            _emulator.SetMemory(0x00, program);
            var numbers = new byte[] { 0x12, 0x01, 0x05, 0xAD, 0x03, 0x56, 0x1A, 0xD2, 0x00, 0x44 };
            _emulator.SetMemory(0x3000, numbers);
            _emulator.ProgramCounter = 0x00;

            _emulator.Run();

            var list = new List<byte>(numbers);
            list.Sort();

            for(var i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(list[i], _emulator[(ushort)(0x3000 + i)]);
            }
        }
    }
}
