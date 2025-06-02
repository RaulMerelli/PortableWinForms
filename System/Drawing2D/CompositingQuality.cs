namespace System.Drawing.Drawing2D
{
    public enum CompositingQuality
    {
        Invalid = QualityMode.Invalid,
        Default = QualityMode.Default,
        HighSpeed = QualityMode.Low,
        HighQuality = QualityMode.High,
        GammaCorrected,
        AssumeLinear
    }
}