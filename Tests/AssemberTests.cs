using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace Tests
{
    [TestClass]
    public class AssemberTests
    {
        [TestMethod]
        public void AddingTwo8BitNumbers()
        {
            var input = @"LXI H, 3005h
MOV A, M
INX H
ADD M
INX H
MOV M, A
HLT";
            var assembler = new Assembler();
            var expected = new byte[] { 0x21, 0x05, 0x30, 0x7E, 0x23, 0x86, 0x23, 0x77, 0x76 };
            using (var ms = new MemoryStream(Encoding.Default.GetBytes(input)))
            {
                var result = assembler.Compile(ms);

                CollectionAssert.AreEqual(expected, result.Data);
            }
        }

        [TestMethod]
        public void ExchangingMemoryLocations()
        {
            var input = @"LDA 5000h
MOV B, A    
LDA 6000h   
STA 5000h   
MOV A, B    
STA 6000h
HLT";
            var assembler = new Assembler();
            var expected = new byte[] { 0x3a, 0x00, 0x50, 0x47, 0x3A, 0x00, 0x60, 0x32, 0x00, 0x50, 0x78, 0x32, 0x00, 0x60, 0x76 };
            using (var ms = new MemoryStream(Encoding.Default.GetBytes(input)))
            {
                var result = assembler.Compile(ms).Data;

                CollectionAssert.AreEqual(expected, result);
            }
        }

        [TestMethod]
        public void SortNumbers()
        {
            var input = @"MVI B, 09
START: 
LXI H, 3000H
MVI C, 09H
BACK: 
MOV A, M
INX H
CMP M
JC SKIP
JZ SKIP
MOV D, M
MOV M, A
DCX H
MOV M, D
INX H
SKIP: 
DCR C
JNZ BACK
DCR B
JNZ START
HLT";
            var assembler = new Assembler();
            var expected = new byte[] {
                0x06, 0x09, 0x21, 0x00, 0x30, 0x0E, 0x09, 0x7E,
                0x23, 0xBE, 0xDA, 0x15, 0x00, 0xCA, 0x15, 0x00,
                0x56, 0x77, 0x2B, 0x72, 0x23, 0x0D, 0xC2, 0x07,
                0x00, 0x05, 0xC2, 0x02, 0x00, 0x76
            };
            using (var ms = new MemoryStream(Encoding.Default.GetBytes(input)))
            {
                var result = assembler.Compile(ms);

                CollectionAssert.AreEqual(expected, result.Data);
            }
        }
    }
}
