﻿using System.Linq;
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

        // ------------------ 0x21 - 0x2F

        [TestMethod] public void LXI_H() { LXI(0x21, Register.H, Register.L); }


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
            Assert.AreEqual(0x00, _emulator[Register.H]);
            Assert.AreEqual(0x00, _emulator[Register.L]);
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
            var program = new byte[] { 0x21, 0x05, 0x30, 0x7E, 0x23, 0x86, 0x23, 0x77, 0x76 };
            var halted = false;

            _emulator.Halted += (sender, args) => { halted = true; };
            _emulator.SetMemory(0x400, program);
            _emulator.SetMemory(0x3005, 0x14, 0x89);

            _emulator.ProgramCounter = 0x400;

            _emulator.Step();

            Assert.AreEqual(0x403, _emulator.ProgramCounter);
            Assert.AreEqual(0x30, _emulator[Register.H]);
            Assert.AreEqual(0x05, _emulator[Register.L]);

            _emulator.Step();

            Assert.AreEqual(0x404, _emulator.ProgramCounter);
            Assert.AreEqual(0x14, _emulator[Register.A]);
            Assert.IsFalse(_emulator[Flag.P]);

            _emulator.Step();

            Assert.AreEqual(0x405, _emulator.ProgramCounter);
            Assert.AreEqual(0x30, _emulator[Register.H]);
            Assert.AreEqual(0x06, _emulator[Register.L]);

            _emulator.Step();

            Assert.AreEqual(0x406, _emulator.ProgramCounter);
            Assert.AreEqual(0x9D, _emulator[Register.A]);
            Assert.IsTrue(_emulator[Flag.P]);

            _emulator.Step();

            Assert.AreEqual(0x407, _emulator.ProgramCounter);
            Assert.AreEqual(0x30, _emulator[Register.H]);
            Assert.AreEqual(0x07, _emulator[Register.L]);

            _emulator.Step();

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
