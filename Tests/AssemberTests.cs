using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class AssemberTests
    {
        Assembler _assembler;

        [TestInitialize]
        public void Initialize()
        {
            _assembler = new Assembler();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _assembler = null;
        }

        void CompileAndCompare(string input, params byte[] expected)
        {
            using (var ms = new MemoryStream(Encoding.Default.GetBytes(input)))
            {
                var result = _assembler.Compile(ms);
                CollectionAssert.AreEqual(expected, result.Data);
            }
        }

        // ------------------ 0x00 - 0x0F

        [TestMethod] public void NOP() => CompileAndCompare(@"NOP", 0x00);
        [TestMethod] public void LXI_B() => CompileAndCompare(@"LXI B, 1245H", 0x01, 0x45, 0x12);
        [TestMethod] public void STAX_B() => CompileAndCompare(@"STAX B", 0x02);
        [TestMethod] public void INX_B() => CompileAndCompare(@"INX B", 0x03);
        [TestMethod] public void INR_B() => CompileAndCompare(@"INR B", 0x04);
        [TestMethod] public void DCR_B() => CompileAndCompare(@"DCR B", 0x05);
        [TestMethod] public void MVI_B() => CompileAndCompare(@"MVI B, 92H", 0x06, 0x92);
        [TestMethod] public void RLC() => CompileAndCompare(@"RLC", 0x07);
        // 0x08 is not an opcode
        [TestMethod] public void DAD_B() => CompileAndCompare(@"DAD B", 0x09);
        [TestMethod] public void LDAX_B() => CompileAndCompare(@"LDAX B", 0x0A);
        [TestMethod] public void DCX_B() => CompileAndCompare(@"DCX B", 0x0B);
        [TestMethod] public void INR_C() => CompileAndCompare(@"INR C", 0x0C);
        [TestMethod] public void DCR_C() => CompileAndCompare(@"DCR C", 0x0D);
        [TestMethod] public void MVI_C() => CompileAndCompare(@"MVI C, 92H", 0x0E, 0x92);
        [TestMethod] public void RRC() => CompileAndCompare(@"RRC", 0x0F);

        // ------------------ 0x10 - 0x1F

        // 0x10 is not an opcode
        [TestMethod] public void LXI_D() => CompileAndCompare(@"LXI D, 1245H", 0x11, 0x45, 0x12);
        [TestMethod] public void STAX_D() => CompileAndCompare(@"STAX D", 0x12);
        [TestMethod] public void INX_D() => CompileAndCompare(@"INX D", 0x13);
        [TestMethod] public void INR_D() => CompileAndCompare(@"INR D", 0x14);
        [TestMethod] public void DCR_D() => CompileAndCompare(@"DCR D", 0x15);
        [TestMethod] public void MVI_D() => CompileAndCompare(@"MVI D, 92H", 0x16, 0x92);
        [TestMethod] public void RAL() => CompileAndCompare(@"RAL", 0x17);
        // 0x18 is not an opcode
        [TestMethod] public void DAD_D() => CompileAndCompare(@"DAD D", 0x19);
        [TestMethod] public void LDAX_D() => CompileAndCompare(@"LDAX D", 0x1A);
        [TestMethod] public void DCX_D() => CompileAndCompare(@"DCX D", 0x1B);
        [TestMethod] public void INR_E() => CompileAndCompare(@"INR E", 0x1C);
        [TestMethod] public void DCR_E() => CompileAndCompare(@"DCR E", 0x1D);
        [TestMethod] public void MVI_E() => CompileAndCompare(@"MVI E, 92H", 0x1E, 0x92);
        [TestMethod] public void RAR() => CompileAndCompare(@"RAR", 0x1F);

        // ------------------ 0x20 - 0x2F

        [TestMethod] public void RIM() => CompileAndCompare(@"RIM", 0x20);
        [TestMethod] public void LXI_H() => CompileAndCompare(@"LXI H, 1245H", 0x21, 0x45, 0x12);
        [TestMethod] public void SHLD() => CompileAndCompare(@"SHLD 1234H", 0x22, 0x34, 0x12);
        [TestMethod] public void INX_H() => CompileAndCompare(@"INX H", 0x23);
        [TestMethod] public void INR_H() => CompileAndCompare(@"INR H", 0x24);
        [TestMethod] public void DCR_H() => CompileAndCompare(@"DCR H", 0x25);
        [TestMethod] public void MVI_H() => CompileAndCompare(@"MVI H, 92H", 0x26, 0x92);
        [TestMethod] public void DAA() => CompileAndCompare(@"DAA", 0x27);
        // 0x28 is not an opcode
        [TestMethod] public void DAD_H() => CompileAndCompare(@"DAD H", 0x29);
        [TestMethod] public void LHLD() => CompileAndCompare(@"LHLD 1234H", 0x2A, 0x34, 0x12);
        [TestMethod] public void DCX_H() => CompileAndCompare(@"DCX H", 0x2B);
        [TestMethod] public void INR_L() => CompileAndCompare(@"INR L", 0x2C);
        [TestMethod] public void DCR_L() => CompileAndCompare(@"DCR L", 0x2D);
        [TestMethod] public void MVI_L() => CompileAndCompare(@"MVI L, 92H", 0x2E, 0x92);
        [TestMethod] public void CMA() => CompileAndCompare(@"CMA", 0x2F);

        // ------------------ 0x30 - 0x3F

        [TestMethod] public void SIM() => CompileAndCompare(@"SIM", 0x30);
        [TestMethod] public void LXI_SP() => CompileAndCompare(@"LXI SP, 1245H", 0x31, 0x45, 0x12);
        [TestMethod] public void STA() => CompileAndCompare(@"STA 1234H", 0x32, 0x34, 0x12);
        [TestMethod] public void INX_SP() => CompileAndCompare(@"INX SP", 0x33);
        [TestMethod] public void INR_M() => CompileAndCompare(@"INR M", 0x34);
        [TestMethod] public void DCR_M() => CompileAndCompare(@"DCR M", 0x35);
        [TestMethod] public void MVI_M() => CompileAndCompare(@"MVI M, 92H", 0x36, 0x92);
        [TestMethod] public void STC() => CompileAndCompare(@"STC", 0x37);
        // 0x38 is not an opcode
        [TestMethod] public void DAD_SP() => CompileAndCompare(@"DAD SP", 0x39);
        [TestMethod] public void LDA() => CompileAndCompare(@"LDA 1234H", 0x3A, 0x34, 0x12);
        [TestMethod] public void DCX_SP() => CompileAndCompare(@"DCX SP", 0x3B);
        [TestMethod] public void INR_A() => CompileAndCompare(@"INR A", 0x3C);
        [TestMethod] public void DCR_A() => CompileAndCompare(@"DCR A", 0x3D);
        [TestMethod] public void MVI_A() => CompileAndCompare(@"MVI A, 92H", 0x3E, 0x92);
        [TestMethod] public void CMC() => CompileAndCompare(@"CMC", 0x3F);

        // ------------------ 0x40 - 0x4F

        [TestMethod] public void MOV_B_B() => CompileAndCompare(@"MOV B,B", 0x40);
        [TestMethod] public void MOV_B_C() => CompileAndCompare(@"MOV B,C", 0x41);
        [TestMethod] public void MOV_B_D() => CompileAndCompare(@"MOV B,D", 0x42);
        [TestMethod] public void MOV_B_E() => CompileAndCompare(@"MOV B,E", 0x43);
        [TestMethod] public void MOV_B_H() => CompileAndCompare(@"MOV B,H", 0x44);
        [TestMethod] public void MOV_B_L() => CompileAndCompare(@"MOV B,L", 0x45);
        [TestMethod] public void MOV_B_M() => CompileAndCompare(@"MOV B,M", 0x46);
        [TestMethod] public void MOV_B_A() => CompileAndCompare(@"MOV B,A", 0x47);
        [TestMethod] public void MOV_C_B() => CompileAndCompare(@"MOV C,B", 0x48);
        [TestMethod] public void MOV_C_C() => CompileAndCompare(@"MOV C,C", 0x49);
        [TestMethod] public void MOV_C_D() => CompileAndCompare(@"MOV C,D", 0x4A);
        [TestMethod] public void MOV_C_E() => CompileAndCompare(@"MOV C,E", 0x4B);
        [TestMethod] public void MOV_C_H() => CompileAndCompare(@"MOV C,H", 0x4C);
        [TestMethod] public void MOV_C_L() => CompileAndCompare(@"MOV C,L", 0x4D);
        [TestMethod] public void MOV_C_M() => CompileAndCompare(@"MOV C,M", 0x4E);
        [TestMethod] public void MOV_C_A() => CompileAndCompare(@"MOV C,A", 0x4F);

        // ------------------ 0x50 - 0x5F

        [TestMethod] public void MOV_D_B() => CompileAndCompare(@"MOV D,B", 0x50);
        [TestMethod] public void MOV_D_C() => CompileAndCompare(@"MOV D,C", 0x51);
        [TestMethod] public void MOV_D_D() => CompileAndCompare(@"MOV D,D", 0x52);
        [TestMethod] public void MOV_D_E() => CompileAndCompare(@"MOV D,E", 0x53);
        [TestMethod] public void MOV_D_H() => CompileAndCompare(@"MOV D,H", 0x54);
        [TestMethod] public void MOV_D_L() => CompileAndCompare(@"MOV D,L", 0x55);
        [TestMethod] public void MOV_D_M() => CompileAndCompare(@"MOV D,M", 0x56);
        [TestMethod] public void MOV_D_A() => CompileAndCompare(@"MOV D,A", 0x57);
        [TestMethod] public void MOV_E_B() => CompileAndCompare(@"MOV E,B", 0x58);
        [TestMethod] public void MOV_E_C() => CompileAndCompare(@"MOV E,C", 0x59);
        [TestMethod] public void MOV_E_D() => CompileAndCompare(@"MOV E,D", 0x5A);
        [TestMethod] public void MOV_E_E() => CompileAndCompare(@"MOV E,E", 0x5B);
        [TestMethod] public void MOV_E_H() => CompileAndCompare(@"MOV E,H", 0x5C);
        [TestMethod] public void MOV_E_L() => CompileAndCompare(@"MOV E,L", 0x5D);
        [TestMethod] public void MOV_E_M() => CompileAndCompare(@"MOV E,M", 0x5E);
        [TestMethod] public void MOV_E_A() => CompileAndCompare(@"MOV E,A", 0x5F);

        // ------------------ 0x60 - 0x6F

        [TestMethod] public void MOV_H_B() => CompileAndCompare(@"MOV H,B", 0x60);
        [TestMethod] public void MOV_H_C() => CompileAndCompare(@"MOV H,C", 0x61);
        [TestMethod] public void MOV_H_D() => CompileAndCompare(@"MOV H,D", 0x62);
        [TestMethod] public void MOV_H_E() => CompileAndCompare(@"MOV H,E", 0x63);
        [TestMethod] public void MOV_H_H() => CompileAndCompare(@"MOV H,H", 0x64);
        [TestMethod] public void MOV_H_L() => CompileAndCompare(@"MOV H,L", 0x65);
        [TestMethod] public void MOV_H_M() => CompileAndCompare(@"MOV H,M", 0x66);
        [TestMethod] public void MOV_H_A() => CompileAndCompare(@"MOV H,A", 0x67);
        [TestMethod] public void MOV_L_B() => CompileAndCompare(@"MOV L,B", 0x68);
        [TestMethod] public void MOV_L_C() => CompileAndCompare(@"MOV L,C", 0x69);
        [TestMethod] public void MOV_L_D() => CompileAndCompare(@"MOV L,D", 0x6A);
        [TestMethod] public void MOV_L_E() => CompileAndCompare(@"MOV L,E", 0x6B);
        [TestMethod] public void MOV_L_H() => CompileAndCompare(@"MOV L,H", 0x6C);
        [TestMethod] public void MOV_L_L() => CompileAndCompare(@"MOV L,L", 0x6D);
        [TestMethod] public void MOV_L_M() => CompileAndCompare(@"MOV L,M", 0x6E);
        [TestMethod] public void MOV_L_A() => CompileAndCompare(@"MOV L,A", 0x6F);

        // ------------------ 0x70 - 0x7F

        [TestMethod] public void MOV_M_B() => CompileAndCompare(@"MOV M,B", 0x70);
        [TestMethod] public void MOV_M_C() => CompileAndCompare(@"MOV M,C", 0x71);
        [TestMethod] public void MOV_M_D() => CompileAndCompare(@"MOV M,D", 0x72);
        [TestMethod] public void MOV_M_E() => CompileAndCompare(@"MOV M,E", 0x73);
        [TestMethod] public void MOV_M_H() => CompileAndCompare(@"MOV M,H", 0x74);
        [TestMethod] public void MOV_M_L() => CompileAndCompare(@"MOV M,L", 0x75);
        [TestMethod] public void MOV_M_M() => CompileAndCompare(@"MOV M,M", 0x76);
        [TestMethod] public void MOV_M_A() => CompileAndCompare(@"MOV M,A", 0x77);
        [TestMethod] public void MOV_A_B() => CompileAndCompare(@"MOV A,B", 0x78);
        [TestMethod] public void MOV_A_C() => CompileAndCompare(@"MOV A,C", 0x79);
        [TestMethod] public void MOV_A_D() => CompileAndCompare(@"MOV A,D", 0x7A);
        [TestMethod] public void MOV_A_E() => CompileAndCompare(@"MOV A,E", 0x7B);
        [TestMethod] public void MOV_A_H() => CompileAndCompare(@"MOV A,H", 0x7C);
        [TestMethod] public void MOV_A_L() => CompileAndCompare(@"MOV A,L", 0x7D);
        [TestMethod] public void MOV_A_M() => CompileAndCompare(@"MOV A,M", 0x7E);
        [TestMethod] public void MOV_A_A() => CompileAndCompare(@"MOV A,A", 0x7F);

        // ------------------ 0x80 - 0x8F

        [TestMethod] public void ADD_B() => CompileAndCompare(@"ADD B", 0x80);
        [TestMethod] public void ADD_C() => CompileAndCompare(@"ADD C", 0x81);
        [TestMethod] public void ADD_D() => CompileAndCompare(@"ADD D", 0x82);
        [TestMethod] public void ADD_E() => CompileAndCompare(@"ADD E", 0x83);
        [TestMethod] public void ADD_H() => CompileAndCompare(@"ADD H", 0x84);
        [TestMethod] public void ADD_L() => CompileAndCompare(@"ADD L", 0x85);
        [TestMethod] public void ADD_M() => CompileAndCompare(@"ADD M", 0x86);
        [TestMethod] public void ADD_A() => CompileAndCompare(@"ADD A", 0x87);
        [TestMethod] public void ADC_B() => CompileAndCompare(@"ADC B", 0x88);
        [TestMethod] public void ADC_C() => CompileAndCompare(@"ADC C", 0x89);
        [TestMethod] public void ADC_D() => CompileAndCompare(@"ADC D", 0x8A);
        [TestMethod] public void ADC_E() => CompileAndCompare(@"ADC E", 0x8B);
        [TestMethod] public void ADC_H() => CompileAndCompare(@"ADC H", 0x8C);
        [TestMethod] public void ADC_L() => CompileAndCompare(@"ADC L", 0x8D);
        [TestMethod] public void ADC_M() => CompileAndCompare(@"ADC M", 0x8E);
        [TestMethod] public void ADC_A() => CompileAndCompare(@"ADC A", 0x8F);

        // ------------------ 0x90 - 0x9F

        [TestMethod] public void SUB_B() => CompileAndCompare(@"SUB B", 0x90);
        [TestMethod] public void SUB_C() => CompileAndCompare(@"SUB C", 0x91);
        [TestMethod] public void SUB_D() => CompileAndCompare(@"SUB D", 0x92);
        [TestMethod] public void SUB_E() => CompileAndCompare(@"SUB E", 0x93);
        [TestMethod] public void SUB_H() => CompileAndCompare(@"SUB H", 0x94);
        [TestMethod] public void SUB_L() => CompileAndCompare(@"SUB L", 0x95);
        [TestMethod] public void SUB_M() => CompileAndCompare(@"SUB M", 0x96);
        [TestMethod] public void SUB_A() => CompileAndCompare(@"SUB A", 0x97);
        [TestMethod] public void SBB_B() => CompileAndCompare(@"SBB B", 0x98);
        [TestMethod] public void SBB_C() => CompileAndCompare(@"SBB C", 0x99);
        [TestMethod] public void SBB_D() => CompileAndCompare(@"SBB D", 0x9A);
        [TestMethod] public void SBB_E() => CompileAndCompare(@"SBB E", 0x9B);
        [TestMethod] public void SBB_H() => CompileAndCompare(@"SBB H", 0x9C);
        [TestMethod] public void SBB_L() => CompileAndCompare(@"SBB L", 0x9D);
        [TestMethod] public void SBB_M() => CompileAndCompare(@"SBB M", 0x9E);
        [TestMethod] public void SBB_A() => CompileAndCompare(@"SBB A", 0x9F);

        // ------------------ 0xA0 - 0xAF

        [TestMethod] public void ANA_B() => CompileAndCompare(@"ANA B", 0xA0);
        [TestMethod] public void ANA_C() => CompileAndCompare(@"ANA C", 0xA1);
        [TestMethod] public void ANA_D() => CompileAndCompare(@"ANA D", 0xA2);
        [TestMethod] public void ANA_E() => CompileAndCompare(@"ANA E", 0xA3);
        [TestMethod] public void ANA_H() => CompileAndCompare(@"ANA H", 0xA4);
        [TestMethod] public void ANA_L() => CompileAndCompare(@"ANA L", 0xA5);
        [TestMethod] public void ANA_M() => CompileAndCompare(@"ANA M", 0xA6);
        [TestMethod] public void ANA_A() => CompileAndCompare(@"ANA A", 0xA7);
        [TestMethod] public void XRA_B() => CompileAndCompare(@"XRA B", 0xA8);
        [TestMethod] public void XRA_C() => CompileAndCompare(@"XRA C", 0xA9);
        [TestMethod] public void XRA_D() => CompileAndCompare(@"XRA D", 0xAA);
        [TestMethod] public void XRA_E() => CompileAndCompare(@"XRA E", 0xAB);
        [TestMethod] public void XRA_H() => CompileAndCompare(@"XRA H", 0xAC);
        [TestMethod] public void XRA_L() => CompileAndCompare(@"XRA L", 0xAD);
        [TestMethod] public void XRA_M() => CompileAndCompare(@"XRA M", 0xAE);
        [TestMethod] public void XRA_A() => CompileAndCompare(@"XRA A", 0xAF);

        // ------------------ 0xB0 - 0xBF

        [TestMethod] public void ORA_B() => CompileAndCompare(@"ORA B", 0xB0);
        [TestMethod] public void ORA_C() => CompileAndCompare(@"ORA C", 0xB1);
        [TestMethod] public void ORA_D() => CompileAndCompare(@"ORA D", 0xB2);
        [TestMethod] public void ORA_E() => CompileAndCompare(@"ORA E", 0xB3);
        [TestMethod] public void ORA_H() => CompileAndCompare(@"ORA H", 0xB4);
        [TestMethod] public void ORA_L() => CompileAndCompare(@"ORA L", 0xB5);
        [TestMethod] public void ORA_M() => CompileAndCompare(@"ORA M", 0xB6);
        [TestMethod] public void ORA_A() => CompileAndCompare(@"ORA A", 0xB7);
        [TestMethod] public void CMP_B() => CompileAndCompare(@"CMP B", 0xB8);
        [TestMethod] public void CMP_C() => CompileAndCompare(@"CMP C", 0xB9);
        [TestMethod] public void CMP_D() => CompileAndCompare(@"CMP D", 0xBA);
        [TestMethod] public void CMP_E() => CompileAndCompare(@"CMP E", 0xBB);
        [TestMethod] public void CMP_H() => CompileAndCompare(@"CMP H", 0xBC);
        [TestMethod] public void CMP_L() => CompileAndCompare(@"CMP L", 0xBD);
        [TestMethod] public void CMP_M() => CompileAndCompare(@"CMP M", 0xBE);
        [TestMethod] public void CMP_A() => CompileAndCompare(@"CMP A", 0xBF);

        // ------------------ 0xC0 - 0xCF

        [TestMethod] public void RNZ() => CompileAndCompare(@"RNZ", 0xC0);
        [TestMethod] public void POP_B() => CompileAndCompare(@"POP B", 0xC1);
        [TestMethod] public void JNZ() => CompileAndCompare(@"START:
JNZ START", 0xC2, 0x00, 0x00);
        [TestMethod] public void JMP() => CompileAndCompare(@"START:
JMP START", 0xC3, 0x00, 0x00);
        [TestMethod] public void CNZ() => CompileAndCompare(@"START:
CNZ START", 0xC4, 0x00, 0x00);
        [TestMethod] public void PUSH_B() => CompileAndCompare(@"PUSH B", 0xC5);
        [TestMethod] public void ADI() => CompileAndCompare(@"ADI 92H", 0xC6, 0x92);
        [TestMethod] public void RST_0() => CompileAndCompare(@"RST 0", 0xC7);
        [TestMethod] public void RZ() => CompileAndCompare(@"RZ", 0xC8);
        [TestMethod] public void RET() => CompileAndCompare(@"RET", 0xC9);
        [TestMethod] public void JZ() => CompileAndCompare(@"START:
JZ START", 0xCA, 0x00, 0x00);
        // 0xCB is not an opcode
        [TestMethod] public void CZ() => CompileAndCompare(@"START:
CZ START", 0xCC, 0x00, 0x00);
        [TestMethod] public void CALL() => CompileAndCompare(@"START:
CALL START", 0xCD, 0x00, 0x00);
        // 0xCE is not an opcode
        [TestMethod] public void RST_1() => CompileAndCompare(@"RST 1", 0xCF);

        // ------------------ 0xD0 - 0xDF

        [TestMethod] public void RNC() => CompileAndCompare(@"RNC", 0xD0);
        [TestMethod] public void POP_D() => CompileAndCompare(@"POP D", 0xD1);
        [TestMethod] public void JNC() => CompileAndCompare(@"START:
JNC START", 0xD2, 0x00, 0x00);
        [TestMethod] public void OUT() => CompileAndCompare(@"OUT 12H", 0xD3, 0x12);
        [TestMethod] public void CNC() => CompileAndCompare(@"START:
CNC START", 0xD4, 0x00, 0x00);
        [TestMethod] public void PUSH_D() => CompileAndCompare(@"PUSH D", 0xD5);
        [TestMethod] public void SUI() => CompileAndCompare(@"SUI 92H", 0xD6, 0x92);
        [TestMethod] public void RST_2() => CompileAndCompare(@"RST 2", 0xD7);
        [TestMethod] public void RC() => CompileAndCompare(@"RC", 0xD8);
        // 0xD9 is not an opcode
        [TestMethod] public void JC() => CompileAndCompare(@"START:
JC START", 0xDA, 0x00, 0x00);
        [TestMethod] public void IN() => CompileAndCompare(@"IN 12H", 0xDB, 0x12);
        [TestMethod] public void CC() => CompileAndCompare(@"START:
CC START", 0xDC, 0x00, 0x00);
        // 0xDD is not an opcode
        [TestMethod] public void SBI() => CompileAndCompare(@"SBI 92H", 0xDE, 0x92);
        [TestMethod] public void RST_3() => CompileAndCompare(@"RST 3", 0xDF);

        // ------------------ 0xE0 - 0xEF

        [TestMethod] public void RPO() => CompileAndCompare(@"RPO", 0xE0);
        [TestMethod] public void POP_H() => CompileAndCompare(@"POP H", 0xE1);
        [TestMethod] public void JPO() => CompileAndCompare(@"START:
JPO START", 0xE2, 0x00, 0x00);
        [TestMethod] public void XTHL() => CompileAndCompare(@"XTHL", 0xE3);
        [TestMethod] public void CPO() => CompileAndCompare(@"START:
CPO START", 0xE4, 0x00, 0x00);
        [TestMethod] public void PUSH_H() => CompileAndCompare(@"PUSH H", 0xE5);
        [TestMethod] public void ANI() => CompileAndCompare(@"ANI 92H", 0xE6, 0x92);
        [TestMethod] public void RST_4() => CompileAndCompare(@"RST 4", 0xE7);
        [TestMethod] public void RPE() => CompileAndCompare(@"RPE", 0xE8);
        [TestMethod] public void PCHL() => CompileAndCompare(@"PCHL", 0xE9);
        [TestMethod] public void JPE() => CompileAndCompare(@"START:
JPE START", 0xEA, 0x00, 0x00);
        [TestMethod] public void XCHG() => CompileAndCompare(@"XCHG", 0xEB);
        [TestMethod] public void CPE() => CompileAndCompare(@"START:
CPE START", 0xEC, 0x00, 0x00);
        // 0xED is not an opcode
        [TestMethod] public void XRI() => CompileAndCompare(@"XRI 92H", 0xEE, 0x92);
        [TestMethod] public void RST_5() => CompileAndCompare(@"RST 5", 0xEF);

        // ------------------ 0xF0 - 0xFF

        [TestMethod] public void RP() => CompileAndCompare(@"RP", 0xF0);
        [TestMethod] public void POP_PSW() => CompileAndCompare(@"POP PSW", 0xF1);
        [TestMethod] public void JP() => CompileAndCompare(@"START:
JP START", 0xF2, 0x00, 0x00);
        [TestMethod] public void DI() => CompileAndCompare(@"DI", 0xF3);
        [TestMethod] public void CP() => CompileAndCompare(@"START:
CP START", 0xF4, 0x00, 0x00);
        [TestMethod] public void PUSH_PSW() => CompileAndCompare(@"PUSH PSW", 0xF5);
        [TestMethod] public void ORI() => CompileAndCompare(@"ORI 92H", 0xF6, 0x92);
        [TestMethod] public void RST_6() => CompileAndCompare(@"RST 6", 0xF7);
        [TestMethod] public void RM() => CompileAndCompare(@"RM", 0xF8);
        [TestMethod] public void SPHL() => CompileAndCompare(@"SPHL", 0xF9);
        [TestMethod] public void JM() => CompileAndCompare(@"START:
JM START", 0xFA, 0x00, 0x00);
        [TestMethod] public void EI() => CompileAndCompare(@"EI", 0xFB);
        [TestMethod] public void CM() => CompileAndCompare(@"START:
CM START", 0xFC, 0x00, 0x00);
        // 0xFD is not an opcode
        [TestMethod] public void CPI() => CompileAndCompare(@"CPI 92H", 0xFE, 0x92);
        [TestMethod] public void RST_7() => CompileAndCompare(@"RST 7", 0xFF);

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void LXI_B_NoAddress() => CompileAndCompare(@"LXI B");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void LXI_D_NoAddress() => CompileAndCompare(@"LXI D");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void MVI_B_NoValue() => CompileAndCompare(@"MVI B");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void MVI_C_NoValue() => CompileAndCompare(@"MVI C");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void MVI_D_NoValue() => CompileAndCompare(@"MVI D");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void LXI_H_NoAddress() => CompileAndCompare(@"LXI H");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void MVI_H_NoValue() => CompileAndCompare(@"MVI H");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void SHLD_NoAddress() => CompileAndCompare(@"SHLD");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void LXI_SP_NoAddress() => CompileAndCompare(@"LXI SP");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void STA_NoAddress() => CompileAndCompare(@"STA");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void LDA_NoAddress() => CompileAndCompare(@"LDA");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void MVI_M_NoValue() => CompileAndCompare(@"MVI M");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void ADI_NoValue() => CompileAndCompare(@"ADI");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void OUT_NoValue() => CompileAndCompare(@"OUT");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void SUI_NoValue() => CompileAndCompare(@"SUI");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void IN_NoValue() => CompileAndCompare(@"IN");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void SBI_NoValue() => CompileAndCompare(@"SBI");

        [TestMethod, ExpectedException(typeof(InvalidDataException))]
        public void ANI_NoValue() => CompileAndCompare(@"ANI");


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
            CompileAndCompare(input, 0x21, 0x05, 0x30, 0x7E, 0x23, 0x86, 0x23, 0x77, 0x76);
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
            CompileAndCompare(input, 0x3A, 0x00, 0x50, 0x47, 0x3A, 0x00, 0x60, 0x32, 0x00, 0x50, 0x78, 0x32, 0x00, 0x60, 0x76);
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
            CompileAndCompare(input,
                0x06, 0x09, 0x21, 0x00, 0x30, 0x0E, 0x09, 0x7E,
                0x23, 0xBE, 0xDA, 0x15, 0x00, 0xCA, 0x15, 0x00,
                0x56, 0x77, 0x2B, 0x72, 0x23, 0x0D, 0xC2, 0x07,
                0x00, 0x05, 0xC2, 0x02, 0x00, 0x76);
        }
    }
}
