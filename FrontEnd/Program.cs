using Core;
using System;
using Terminal.Gui;

namespace FrontEnd
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();
            var top = Application.Top;

            var registers = new FrameView("Registers") { X = 0, Width = 22, Height = 11 };
            var programCounterLabel = new Register16BitLabel() { Y = 5, X = 11 };
            var acumulatorRegisterLabel = new Register8BitLabel() { X = 14 };
            var bcRegisterLabel = new Register16BitLabel() { X = 11, Y = 3, };
            var hlRegisterLabel = new Register16BitLabel() { X = 11, Y = 1, };
            var deRegisterLabel = new Register16BitLabel() { X = 11, Y = 2, };

            registers.Add(
                new Label("A") { X = 1 },
                acumulatorRegisterLabel,
                new Label("BC") { X = 1, Y = 1 },
                bcRegisterLabel,
                new Label("DE") { X = 1, Y = 2 },
                deRegisterLabel,
                new Label("HL") { X = 1, Y = 3 },
                hlRegisterLabel,
                new Label("PSW") { X = 1, Y = 4 },
                new Register16BitLabel() { Y = 4, X = 11 },
                new Label("PC") { X = 1, Y = 5 },
                programCounterLabel,
                new Label("SP") { X = 1, Y = 6 },
                new Register16BitLabel() { Y = 6, X = 11 },
                new Label("Int-Reg") { X = 1, Y = 7 },
                new Register8BitLabel() { Y = 7, X = 14 }
                );

            var flags = new FrameView("Flags") { X = Pos.Right(registers), Width = 9, Height = 11 };
            flags.Add(new Label("S") { X = 1 },
                new Register1BitLabel() { X = 4 },
                new Label("Z") { X = 1, Y = 1 },
                new Register1BitLabel() { Y = 1, X = 4 },
                new Label("AC") { X = 1, Y = 2 },
                new Register1BitLabel() { Y = 2, X = 4 },
                new Label("P") { X = 1, Y = 3 },
                new Register1BitLabel() { Y = 3, X = 4 },
                new Label("C") { X = 1, Y = 4 },
                new Register1BitLabel() { Y = 4, X = 4 }
            );

            var emulator = new Emulator();

            top.Add(registers, flags, new Button("Step")
            {
                Clicked = () => emulator.Step(),
                X = 1,
                Y = 12
            });

            var program = new byte[] { 0x21, 0x05, 0x30, 0x7E, 0x23, 0x86, 0x23, 0x77, 0x76 };
            emulator.SetMemory(0x0, program);
            emulator.SetMemory(0x3005, 0x14, 0x89);

            emulator.ProgramCounterChanged += (_, pc) => { programCounterLabel.SetValue(pc); };
            emulator.RegisterChanged += (_, eventArgs) =>
            {
                switch (eventArgs.Register)
                {
                    case Register.A:
                        acumulatorRegisterLabel.SetValue(eventArgs.Value);
                        break;
                    case Register.B:
                    case Register.C:
                        bcRegisterLabel.SetValue(emulator[Register.B], emulator[Register.C]);
                        break;
                    case Register.D:
                    case Register.E:
                        deRegisterLabel.SetValue(emulator[Register.D], emulator[Register.E]);
                        break;
                    case Register.H:
                    case Register.L:
                        hlRegisterLabel.SetValue(emulator[Register.H], emulator[Register.L]);
                        break;
                }
            };

            emulator.Step();

            Application.Run();
        }
    }
}
