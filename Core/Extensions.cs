namespace Core
{
    public static class Extensions
    {
        public static bool ParityFlag(this byte value)
        {
            var onesCount = 0;
            onesCount += (value & 0x01) == 0x00 ? 0 : 1;
            onesCount += (value & 0x02) == 0x00 ? 0 : 1;
            onesCount += (value & 0x04) == 0x00 ? 0 : 1;
            onesCount += (value & 0x08) == 0x00 ? 0 : 1;
            onesCount += (value & 0x10) == 0x00 ? 0 : 1;
            onesCount += (value & 0x20) == 0x00 ? 0 : 1;
            onesCount += (value & 0x40) == 0x00 ? 0 : 1;
            onesCount += (value & 0x80) == 0x00 ? 0 : 1;

            return onesCount % 2 == 0;
        }

        public static bool SignFlag(this byte value)
        {
            return (value & 0x80) == 0x80;
        }

        public static bool AuxiliaryCarryFlag(this byte a, ushort b, bool increment)
        {
            var valueLower = a & 0xF;
            var previousLower = b & 0xF;

            return increment ? (valueLower + previousLower > 0xF) : (valueLower - previousLower > valueLower);
        }

        public static byte GetHLMemoryValue(this Emulator emulator)
        {
            return emulator[emulator.Get16BitValue(Register.H, Register.L)];
        }
    }
}
