
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
}
