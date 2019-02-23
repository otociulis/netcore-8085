using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class EmulatorTests
    {
        [TestMethod]
        public void AddingTwo8BitNumbers()
        {
            var program = new byte[] { 0x21, 0x05, 0x30, 0x7E, 0x23, 0x86, 0x23, 0x77, 0x76 };
            var emulator = new Emulator();
            var halted = false;

            emulator.Halted += (sender, args) => { halted = true; };
            emulator.SetMemory(0x400, program);
            emulator.SetMemory(0x3005, 0x14, 0x89);

            emulator.ProgramCounter = 0x400;

            emulator.Step();

            Assert.AreEqual(0x403, emulator.ProgramCounter);
            Assert.AreEqual(0x30, emulator[Register.H]);
            Assert.AreEqual(0x05, emulator[Register.L]);

            emulator.Step();

            Assert.AreEqual(0x404, emulator.ProgramCounter);
            Assert.AreEqual(0x14, emulator[Register.A]);
            Assert.IsFalse(emulator[Flag.P]);

            emulator.Step();

            Assert.AreEqual(0x405, emulator.ProgramCounter);
            Assert.AreEqual(0x30, emulator[Register.H]);
            Assert.AreEqual(0x06, emulator[Register.L]);

            emulator.Step();

            Assert.AreEqual(0x406, emulator.ProgramCounter);
            Assert.AreEqual(0x9D, emulator[Register.A]);
            Assert.IsTrue(emulator[Flag.P]);

            emulator.Step();

            Assert.AreEqual(0x407, emulator.ProgramCounter);
            Assert.AreEqual(0x30, emulator[Register.H]);
            Assert.AreEqual(0x07, emulator[Register.L]);

            emulator.Step();

            Assert.AreEqual(0x408, emulator.ProgramCounter);
            Assert.AreEqual(0x9D, emulator[0x3007]);

            emulator.Step();

            Assert.AreEqual(0x409, emulator.ProgramCounter);
            Assert.IsTrue(halted);
        }

        [TestMethod]
        public void ExchangingMemoryLocations()
        {
            var program = new byte[] {
                0x3A, 0x00, 0x50, 0x47, 0x3A, 0x00, 0x60, 0x32,
                0x00, 0x50, 0x78, 0x32, 0x00, 0x60, 0x76
            };
            var emulator = new Emulator();

            emulator.SetMemory(0x400, program);
            emulator[0x5000] = 0x22;
            emulator[0x6000] = 0x44;

            emulator.ProgramCounter = 0x400;

            emulator.Run();

            Assert.AreEqual(0x44, emulator[0x5000]);
            Assert.AreEqual(0x22, emulator[0x6000]);
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
            var emulator = new Emulator();

            emulator.SetMemory(0x400, program);
            emulator.SetMemory(0x3000, 0x12, 0x01, 0x05, 0xAD, 0x03, 0x56, 0x1A, 0xD2, 0x00, 0x44);
            emulator.ProgramCounter = 0x400;

            emulator.Run();

            Assert.AreEqual(0x00, emulator[0x3000]);
            Assert.AreEqual(0x01, emulator[0x3001]);
        }
    }
}
