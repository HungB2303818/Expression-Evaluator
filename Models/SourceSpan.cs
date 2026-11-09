namespace ExpressionEvaluator.Models;

/// <summary>Đoạn nguồn zero-based; độ dài 0 dùng cho EOF hoặc epsilon.</summary>
public readonly record struct SourceSpan
{
    public int StartIndex { get; }
    public int Length { get; }
    public int EndExclusive => StartIndex + Length;
    public int DisplayPosition => StartIndex + 1;

    public SourceSpan(int StartIndex, int Length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(StartIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(Length);
        if (StartIndex == int.MaxValue || Length > int.MaxValue - StartIndex)
            throw new ArgumentOutOfRangeException(nameof(Length), "Đoạn nguồn vượt phạm vi chỉ số.");

        this.StartIndex = StartIndex;
        this.Length = Length;
    }
}
