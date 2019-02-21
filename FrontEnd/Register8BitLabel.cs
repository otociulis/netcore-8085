using Terminal.Gui;

namespace FrontEnd
{
    internal class Register8BitLabel : Label
    {
        public Register8BitLabel() : base(ToString(0))
        {

        }

        private static string ToString(byte value)
        {
            return $"{value.ToString("X2")}";
        }

        public void SetValue(byte value)
        {
            Text = ToString(value);
        }
    }
}
