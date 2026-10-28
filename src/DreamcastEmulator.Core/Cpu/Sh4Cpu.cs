
using DreamcastEmulator.Core.Memory;

namespace DreamcastEmulator.Core.Cpu;

public sealed class Sh4Cpu
{
    private readonly MemoryBus _memory;

    public uint[] Registers { get; } = new uint[16];

    public uint Pc { get; private set; }

    public ulong InstructionsExecuted { get; private set; }

    public Sh4Cpu(MemoryBus memory, uint initialPc = 0)
    {
        _memory = memory;
        Pc = initialPc;
    }

    public void Step()
    {
        ushort instruction = _memory.Read16(Pc);

        // SH-4 instructions are 16 bits wide.
        Pc += 2;

        Execute(instruction);
        InstructionsExecuted++;
    }

    private void Execute(ushort instruction)
    {
        // NOP: 0000 0000 0000 1001
        if (instruction == 0x0009)
        {
            return;
        }

        // MOV #imm, Rn: 1110 nnnn iiii iiii
        if ((instruction & 0xF000) == 0xE000)
        {
            int register = (instruction >> 8) & 0x0F;
            sbyte immediate = (sbyte)(instruction & 0xFF);

            Registers[register] = unchecked((uint)(int)immediate);
            return;
        }

        // ADD #imm, Rn: 0111 nnnn iiii iiii
        if ((instruction & 0xF000) == 0x7000)
        {
            int register = (instruction >> 8) & 0x0F;
            sbyte immediate = (sbyte)(instruction & 0xFF);

            Registers[register] = unchecked(
                Registers[register] + (uint)(int)immediate);

            return;
        }

        // MOV Rm, Rn: 0110 nnnn mmmm 0011
        if ((instruction & 0xF00F) == 0x6003)
        {
            int n = (instruction >> 8) & 0x0F;
            int m = (instruction >> 4) & 0x0F;

            Registers[n] = Registers[m];
            return;
        }

        // ADD Rm, Rn: 0011 nnnn mmmm 1100
        if ((instruction & 0xF00F) == 0x300C)
        {
            int n = (instruction >> 8) & 0x0F;
            int m = (instruction >> 4) & 0x0F;

            Registers[n] = unchecked(Registers[n] + Registers[m]);
            return;
        }

        // SUB Rm, Rn: 0011 nnnn mmmm 1000
        if ((instruction & 0xF00F) == 0x3008)
        {
            int n = (instruction >> 8) & 0x0F;
            int m = (instruction >> 4) & 0x0F;

            Registers[n] = unchecked(Registers[n] - Registers[m]);
            return;
        }

        throw new NotSupportedException(
            $"Unsupported SH-4 instruction 0x{instruction:X4} " +
            $"at address 0x{Pc - 2:X8}.");
    }
}
