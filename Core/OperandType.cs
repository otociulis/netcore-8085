namespace Core
{
    public enum OperandType
    {
        None,
        Data8Bit,
        Data16Bit,
        LabelAs16BitAddress,
        RegisterOrMemory,
        Register,
        RegisterPairOrStackPointer,
        RegisterPairOrProgramStatusWord,
        RegisterBD,
        Index
    }
}
