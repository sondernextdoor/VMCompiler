namespace VirtualMachine
{
    public class CPU
    {
        public struct Registers { }
    }
}

public static class Assembler
{
    public static VirtualMachine.CPU.Registers Assemble(string s) => new();
}
