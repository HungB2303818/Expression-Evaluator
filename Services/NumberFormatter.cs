using System.Globalization;

namespace ExpressionEvaluator.Services;

/// <summary>Chỉ làm tròn khi hiển thị; không thay đổi giá trị decimal dùng để tính.</summary>
public sealed class NumberFormatter
{
    public string Format(decimal value)
    {
        decimal rounded = Math.Round(value, 6, MidpointRounding.AwayFromZero);
        return rounded.ToString("0.######", CultureInfo.InvariantCulture);
    }
}
