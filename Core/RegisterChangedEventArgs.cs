using System;

namespace Core
{
    public class RegisterChangedEventArgs : EventArgs
    {
        public Register Register { get; private set; }
        public byte Value { get; private set; }

        public RegisterChangedEventArgs(Register register, byte value)
        {
            Register = register;
            Value = value;
        }
    }
}
