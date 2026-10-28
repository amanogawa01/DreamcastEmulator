namespace DreamcastEmulator.Core.Memory;

public sealed class MemoryBus
{
    private readonly byte[] _ram;

    public int RamSize => _ram.Length;

    public MemoryBus(int ramSize = 16 * 1024 * 1024)
    {
        if (ramSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ramSize));
        }

        _ram = new byte[ramSize];
    }

    public byte Read8(uint address)
    {
        return _ram[TranslateAddress(address)];
    }

    public ushort Read16(uint address)
    {
        int offset = TranslateAddress(address);
        EnsureRange(offset, 2);

        return (ushort)(
            _ram[offset] |
            (_ram[offset + 1] << 8));
    }

    public uint Read32(uint address)
    {
        int offset = TranslateAddress(address);
        EnsureRange(offset, 4);

        return (uint)(
            _ram[offset] |
            (_ram[offset + 1] << 8) |
            (_ram[offset + 2] << 16) |
            (_ram[offset + 3] << 24));
    }

    public void Write8(uint address, byte value)
    {
        _ram[TranslateAddress(address)] = value;
    }

    public void Write16(uint address, ushort value)
    {
        int offset = TranslateAddress(address);
        EnsureRange(offset, 2);

        _ram[offset] = (byte)value;
        _ram[offset + 1] = (byte)(value >> 8);
    }

    public void Write32(uint address, uint value)
    {
        int offset = TranslateAddress(address);
        EnsureRange(offset, 4);

        _ram[offset] = (byte)value;
        _ram[offset + 1] = (byte)(value >> 8);
        _ram[offset + 2] = (byte)(value >> 16);
        _ram[offset + 3] = (byte)(value >> 24);
    }

    private int TranslateAddress(uint address)
    {
        if (address >= (uint)_ram.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(address),
                $"Address 0x{address:X8} is outside this RAM region.");
        }

        return (int)address;
    }

    private void EnsureRange(int offset, int length)
    {
        if (offset > _ram.Length - length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                "Memory access exceeds RAM.");
        }
    }
}
