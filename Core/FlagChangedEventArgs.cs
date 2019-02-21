namespace Core
{
    public class FlagChangedEventArgs
    {
        public Flag Flag { get; private set; }
        public bool Value { get; private set; }

        public FlagChangedEventArgs(Flag flag, bool value)
        {
            Flag = flag;
            Value = value;
        }
    }
}
