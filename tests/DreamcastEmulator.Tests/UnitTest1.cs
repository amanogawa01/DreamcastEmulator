
using DreamcastEmulator.Core.Cpu;
using DreamcastEmulator.Core.Memory;

namespace DreamcastEmulator.Tests;

public class Sh4CpuTests
{
    [Fact]
    public void Nop_AdvancesProgramCounter()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0x0009);

        var cpu = new Sh4Cpu(memory);

        cpu.Step();

        Assert.Equal(2u, cpu.Pc);
        Assert.Equal(1ul, cpu.InstructionsExecuted);
    }

    [Fact]
    public void MovImmediate_SetsRegister()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0xE005); // MOV #5, R0

        var cpu = new Sh4Cpu(memory);

        cpu.Step();

        Assert.Equal(5u, cpu.Registers[0]);
    }

    [Fact]
    public void MovImmediate_SignExtendsNegativeValue()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0xE0FF); // MOV #-1, R0

        var cpu = new Sh4Cpu(memory);

        cpu.Step();

        Assert.Equal(uint.MaxValue, cpu.Registers[0]);
    }

    [Fact]
    public void AddImmediate_AddsToRegister()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0x7003); // ADD #3, R0

        var cpu = new Sh4Cpu(memory);
        cpu.Registers[0] = 5;

        cpu.Step();

        Assert.Equal(8u, cpu.Registers[0]);
    }

    [Fact]
    public void AddImmediate_WrapsAt32Bits()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0x7001); // ADD #1, R0

        var cpu = new Sh4Cpu(memory);
        cpu.Registers[0] = uint.MaxValue;

        cpu.Step();

        Assert.Equal(0u, cpu.Registers[0]);
    }

    [Fact]
    public void MovRegister_CopiesValue()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0x6123); // MOV R2, R1

        var cpu = new Sh4Cpu(memory);
        cpu.Registers[2] = 42;

        cpu.Step();

        Assert.Equal(42u, cpu.Registers[1]);
        Assert.Equal(42u, cpu.Registers[2]);
    }

    [Fact]
    public void AddRegister_AddsSourceToDestination()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0x312C); // ADD R2, R1

        var cpu = new Sh4Cpu(memory);
        cpu.Registers[1] = 10;
        cpu.Registers[2] = 7;

        cpu.Step();

        Assert.Equal(17u, cpu.Registers[1]);
        Assert.Equal(7u, cpu.Registers[2]);
    }

    [Fact]
    public void SubRegister_SubtractsSourceFromDestination()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0x3128); // SUB R2, R1

        var cpu = new Sh4Cpu(memory);
        cpu.Registers[1] = 10;
        cpu.Registers[2] = 3;

        cpu.Step();

        Assert.Equal(7u, cpu.Registers[1]);
    }

    [Fact]
    public void UnsupportedInstruction_Throws()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0xFFFF);

        var cpu = new Sh4Cpu(memory);

        Assert.Throws<NotSupportedException>(() => cpu.Step());
    }
    [Fact]
    public void MovByte_StoresLowestByteInMemory()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0x2120); // MOV.B R2, @R1

        var cpu = new Sh4Cpu(memory);
        cpu.Registers[1] = 0x100;
        cpu.Registers[2] = 0x123456AB;

        cpu.Step();

        Assert.Equal((byte)0xAB, memory.Read8(0x100));
    }

    [Fact]
    public void MovLong_StoresRegisterInMemory()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0x2122); // MOV.L R2, @R1

        var cpu = new Sh4Cpu(memory);
        cpu.Registers[1] = 0x100;
        cpu.Registers[2] = 0x12345678;

        cpu.Step();

        Assert.Equal(0x12345678u, memory.Read32(0x100));
    }

    [Fact]
    public void MovLong_LoadsMemoryIntoRegister()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0x6122); // MOV.L @R2, R1
        memory.Write32(0x100, 0x12345678);

        var cpu = new Sh4Cpu(memory);
        cpu.Registers[2] = 0x100;

        cpu.Step();

        Assert.Equal(0x12345678u, cpu.Registers[1]);
    }

    [Fact]
    public void Bt_BranchesAfterExecutingDelaySlot()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0x8902); // BT +2
        memory.Write16(2, 0xE001); // MOV #1, R0 (delay slot)
        memory.Write16(4, 0xE002); // MOV #2, R0 (should be skipped)
        memory.Write16(8, 0x0009); // NOP at branch target

        var cpu = new Sh4Cpu(memory);
        cpu.T = true;

        cpu.Step();
        Assert.Equal(2u, cpu.Pc);

        cpu.Step();
        Assert.Equal(8u, cpu.Pc);
        Assert.Equal(1u, cpu.Registers[0]);
    }

    [Fact]
    public void Bt_DoesNotBranchWhenTFlagIsClear()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0x8902);

        var cpu = new Sh4Cpu(memory);
        cpu.T = false;

        cpu.Step();

        Assert.Equal(2u, cpu.Pc);
    }

    [Fact]
    public void Bf_BranchesAfterExecutingDelaySlot()
    {
        var memory = new MemoryBus();
        memory.Write16(0, 0x8B02); // BF +2
        memory.Write16(2, 0xE003); // MOV #3, R0 (delay slot)

        var cpu = new Sh4Cpu(memory);
        cpu.T = false;

        cpu.Step();
        cpu.Step();

        Assert.Equal(8u, cpu.Pc);
        Assert.Equal(3u, cpu.Registers[0]);
    }

}