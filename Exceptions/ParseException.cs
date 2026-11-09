using ExpressionEvaluator.Models;

namespace ExpressionEvaluator.Exceptions;

/// <summary>Lỗi cú pháp có vị trí token/EOF để ExpressionService chuyển thành kết quả phân tích.</summary>
public sealed class ParseException : Exception
{
    public ExpressionError Error { get; }

    public ParseException(ExpressionError error) : base(error?.Message)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error.Category != ExpressionErrorCategory.Syntax)
            throw new ArgumentException("ParseException phải chứa lỗi cú pháp.", nameof(error));

        Error = error;
    }
}
