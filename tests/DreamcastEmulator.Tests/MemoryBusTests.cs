using DreamcastEmulator.Core.Memory;

namespace DreamcastEmulator.Tests;

public class MemoryBusTests
{
    [Fact]
    public void Write8_Read8_ReturnsSameValue()
    {
        var memory = new MemoryBus();

        memory.Write8(0, 0xAB);

        Assert.Equal((byte)0xAB, memory.Read8(0));
    }

    [Fact]
    public void Write16_UsesLittleEndianByteOrder()
    {
        var memory = new MemoryBus();

        memory.Write16(0, 0x1234);

        Assert.Equal((byte)0x34, memory.Read8(0));
        Assert.Equal((byte)0x12, memory.Read8(1));
        Assert.Equal((ushort)0x1234, memory.Read16(0));
    }

    [Fact]
    public void Write32_UsesLittleEndianByteOrder()
    {
        var memory = new MemoryBus();

        memory.Write32(0, 0x12345678);

        Assert.Equal((byte)0x78, memory.Read8(0));
        Assert.Equal((byte)0x56, memory.Read8(1));
        Assert.Equal((byte)0x34, memory.Read8(2));
        Assert.Equal((byte)0x12, memory.Read8(3));
        Assert.Equal(0x12345678u, memory.Read32(0));
    }

    [Fact]
    public void Read32_AtEndOfRam_Throws()
    {
        var memory = new MemoryBus(ramSize: 8);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => memory.Read32(5));
    }

    [Fact]
    public void Write16_AtEndOfRam_Throws()
    {
        var memory = new MemoryBus(ramSize: 8);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => memory.Write16(7, 0x1234));
    }
}
