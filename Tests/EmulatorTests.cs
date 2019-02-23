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

            public ProgramOptions(int instructionSize, HashSet<Register> affectedRegisters, HashSet<Flag> affectedFlags)
            {
                InstructionSize = instructionSize;
                AffectedRegisters = affectedRegisters;
                AffectedFlags = affectedFlags;
            }

            public ProgramOptions(int instructionSize, params Register[] affectedRegisters)
                : this(instructionSize, new HashSet<Register>(affectedRegisters), new HashSet<Flag>())
            { }

            public int InstructionSize = 1;
            public HashSet<Register> AffectedRegisters = new HashSet<Register>();
            public HashSet<Flag> AffectedFlags = new HashSet<Flag>();
        }

        private void SetProgramAndStep(ProgramOptions options, params byte[] program)
        {
            var changedFlags = new HashSet<Flag>();
            var changedRegisters = new HashSet<Register>();

            _emulator.FlagChanged += (_, args) => { changedFlags.Add(args.Flag); };
            _emulator.RegisterChanged += (_, args) => { changedRegisters.Add(args.Register); };

            _emulator.SetMemory(0x400, program);
            _emulator.ProgramCounter = 0x400;
            _emulator.Step();

            Assert.AreEqual(0x400 + options.InstructionSize, _emulator.ProgramCounter);

            CollectionAssert.AreEqual(options.AffectedFlags.OrderBy(x => x).ToArray(), changedFlags.OrderBy(x => x).ToArray());
            CollectionAssert.AreEqual(options.AffectedRegisters.OrderBy(x => x).ToArray(), changedRegisters.OrderBy(x => x).ToArray());
        }

        // ------------------ 0x00 - 0x0F

        [TestMethod] public void NOP() { SetProgramAndStep(new ProgramOptions(), 0x00); }
        [TestMethod] public void LXI_B() { LXI(0x01, Register.B, Register.C); }
        [TestMethod] public void STAX_B() { STAX(0x02, Register.B, Register.C); }
        [TestMethod] public void INX_B() { INX(0x03, Register.B, Register.C); }

        // ------------------ 0x10 - 0x1F

        [TestMethod, ExpectedException(typeof(InvalidOperationException))] public void x10_Invalid() { SetProgramAndStep(new ProgramOptions(), 0x10); }
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
