using Terminal.Gui;

namespace FrontEnd
{
    internal class Register16BitLabel : Label
    {
        public Register16BitLabel() : base(ToString(0))
        {}

        private static string ToString(ushort value)
        {
            var upper = (byte)((value >> 8) & 0xFF);
            var lower = (byte)(value & 0xFF);
            return ToString(upper, lower);
        }

        private static string ToString(byte upper, byte lower)
        {
            return $"{upper.ToString("X2")}    {lower.ToString("X2")}";
        }

        public void SetValue(ushort value)
        {
            Text = ToString(value);
        }

        public void SetValue(byte upper, byte lower)
        {
            Text = ToString(upper, lower);
        }
    }
}
