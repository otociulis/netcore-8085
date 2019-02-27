using System;

namespace Core
{
    public class InstructionAttribute : Attribute
    {
        public OperandType OperandType { get; private set; }
        public byte Code { get; private set; }
        public string Description { get; set; }
        public byte InstructionSpacing { get; set; } = 1;

        public InstructionAttribute(byte code, OperandType operandType)
        {
            OperandType = operandType;
            Code = code;
        }
    }
}
