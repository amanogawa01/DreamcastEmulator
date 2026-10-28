using DreamcastEmulator.Core.Memory;

namespace DreamcastEmulator.Core.Cpu;

public sealed class Sh4Cpu
{
    private const uint TFlag = 1u;

    private readonly MemoryBus _memory;
    private uint? _pendingBranchTarget;
    private bool _executingDelaySlot;
    public uint[] Registers { get; } = new uint[16];

    public uint Pc { get; private set; }

    public uint Sr { get; set; }

    public bool T
    {
        get => (Sr & TFlag) != 0;
        set => Sr = value ? Sr | TFlag : Sr & ~TFlag;
    }

    public ulong InstructionsExecuted { get; private set; }

    public Sh4Cpu(MemoryBus memory, uint initialPc = 0)
    {
        _memory = memory;
        Pc = initialPc;
    }

    public void Step()
    {
        uint instructionAddress = Pc;
        ushort instruction = _memory.Read16(Pc);

        bool wasDelaySlot = _executingDelaySlot;
        uint? branchToApply = wasDelaySlot ? _pendingBranchTarget : null;

        _executingDelaySlot = false;
        _pendingBranchTarget = null;

        Pc += 2;

        Execute(instruction, instructionAddress);
        InstructionsExecuted++;

        if (wasDelaySlot)
        {
            // The branch takes effect after its delay-slot instruction.
            Pc = branchToApply!.Value;
        }
        else if (_pendingBranchTarget.HasValue)
        {
            // The next instruction is the branch delay slot.
            _executingDelaySlot = true;
        }
    }

    private void Execute(ushort instruction, uint instructionAddress)
    {
        // NOP
        if (instruction == 0x0009)
        {
            return;
        }

        // MOV #imm, Rn
        if ((instruction & 0xF000) == 0xE000)
        {
            int n = (instruction >> 8) & 0x0F;
            sbyte immediate = (sbyte)(instruction & 0xFF);

            Registers[n] = unchecked((uint)(int)immediate);
            return;
        }

        // ADD #imm, Rn
        if ((instruction & 0xF000) == 0x7000)
        {
            int n = (instruction >> 8) & 0x0F;
            sbyte immediate = (sbyte)(instruction & 0xFF);

            Registers[n] = unchecked(
                Registers[n] + (uint)(int)immediate);
            return;
        }

        // MOV Rm, Rn
        if ((instruction & 0xF00F) == 0x6003)
        {
            int n = (instruction >> 8) & 0x0F;
            int m = (instruction >> 4) & 0x0F;

            Registers[n] = Registers[m];
            return;
        }

        // ADD Rm, Rn
        if ((instruction & 0xF00F) == 0x300C)
        {
            int n = (instruction >> 8) & 0x0F;
            int m = (instruction >> 4) & 0x0F;

            Registers[n] = unchecked(Registers[n] + Registers[m]);
            return;
        }

        // SUB Rm, Rn
        if ((instruction & 0xF00F) == 0x3008)
        {
            int n = (instruction >> 8) & 0x0F;
            int m = (instruction >> 4) & 0x0F;

            Registers[n] = unchecked(Registers[n] - Registers[m]);
            return;
        }

        // MOV.B Rm, @Rn
        if ((instruction & 0xF00F) == 0x2000)
        {
            int n = (instruction >> 8) & 0x0F;
            int m = (instruction >> 4) & 0x0F;

            _memory.Write8(Registers[n], (byte)Registers[m]);
            return;
        }

        // MOV.L Rm, @Rn
        if ((instruction & 0xF00F) == 0x2002)
        {
            int n = (instruction >> 8) & 0x0F;
            int m = (instruction >> 4) & 0x0F;

            _memory.Write32(Registers[n], Registers[m]);
            return;
        }

        // MOV.L @Rm, Rn
        if ((instruction & 0xF00F) == 0x6002)
        {
            int n = (instruction >> 8) & 0x0F;
            int m = (instruction >> 4) & 0x0F;

            Registers[n] = _memory.Read32(Registers[m]);
            return;
        }

        // BT disp8: branch if T is set.
        if ((instruction & 0xFF00) == 0x8900)
        {
            if (T)
            {
                sbyte displacement = (sbyte)(instruction & 0xFF);
                _pendingBranchTarget = unchecked(
                    (uint)((int)instructionAddress + 4 + displacement * 2));
            }

            return;
        }

        // BF disp8: branch if T is clear.
        if ((instruction & 0xFF00) == 0x8B00)
        {
            if (!T)
            {
                sbyte displacement = (sbyte)(instruction & 0xFF);
                _pendingBranchTarget = unchecked(
                    (uint)((int)instructionAddress + 4 + displacement * 2));
            }

            return;
        }

        throw new NotSupportedException(
            $"Unsupported SH-4 instruction 0x{instruction:X4} " +
            $"at address 0x{instructionAddress:X8}.");
    }
}