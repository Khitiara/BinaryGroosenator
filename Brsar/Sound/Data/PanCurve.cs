namespace Brsar.Sound.Data
{
    public enum PanCurve : byte
    {
        Sqrt           = 0x0,
        Sqrt0DB        = 0x1,
        Sqrt0DBClamp   = 0x2,
        SinCos         = 0x3,
        SinCos0DB      = 0x4,
        SinCos0DBClamp = 0x5,
        Linear         = 0x6,
        Linear0DB      = 0x7,
        Linear0DBClamp = 0x8
    }
}