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
        /*[TestMethod] public void ADC_B() => CompileAndCompare(@"ADC B", 0x88);
        [TestMethod] public void ADC_C() => CompileAndCompare(@"ADC C", 0x89);
        [TestMethod] public void ADC_D() => CompileAndCompare(@"ADC D", 0x8A);
        [TestMethod] public void ADC_E() => CompileAndCompare(@"ADC E", 0x8B);
        [TestMethod] public void ADC_H() => CompileAndCompare(@"ADC H", 0x8C);
        [TestMethod] public void ADC_L() => CompileAndCompare(@"ADC L", 0x8D);
        [TestMethod] public void ADC_M() => CompileAndCompare(@"ADC M", 0x8E);
        [TestMethod] public void ADC_A() => CompileAndCompare(@"ADC A", 0x8F);*/

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
            Assert.IsTrue(_emulator[Flag.P]);

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
                0x06, 0x09, 0x21, 0x00, 0x30, 0x0E, 0x09, 0x7E,
                0x23, 0xBE, 0xDA, 0x15, 0x00, 0xCA, 0x15, 0x00,
                0x56, 0x77, 0x2B, 0x72, 0x23, 0x0D, 0xC2, 0x07,
                0x00, 0x05, 0xC2, 0x02, 0x00, 0x76
            };

            _emulator.SetMemory(0x400, program);
            _emulator.SetMemory(0x3000, 0x12, 0x01, 0x05, 0xAD, 0x03, 0x56, 0x1A, 0xD2, 0x00, 0x44);
            _emulator.ProgramCounter = 0x400;

            _emulator.Run();

            Assert.AreEqual(0x00, _emulator[0x3000]);
            Assert.AreEqual(0x01, _emulator[0x3001]);
        }
    }
}
