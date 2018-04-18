namespace App
{
    public enum FunctionCode : byte
    {
        ReadSerial = 0x01,
        ReadStatus = 0x02,
        SetRegistry = 0x03,
        ReadDateTime = 0x04,
        ReadEnergyValue = 0x05,
        Error = 0xFF
    }
}
