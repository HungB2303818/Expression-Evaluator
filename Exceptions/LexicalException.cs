using ExpressionEvaluator.Models;

namespace ExpressionEvaluator.Exceptions;

/// <summary>Lỗi từ vựng có vị trí nguồn để ExpressionService chuyển thành kết quả phân tích.</summary>
public sealed class LexicalException : Exception
{
    public ExpressionError Error { get; }

    public LexicalException(ExpressionError error) : base(error?.Message)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error.Category != ExpressionErrorCategory.Lexical)
            throw new ArgumentException("LexicalException phải chứa lỗi từ vựng.", nameof(error));

        Error = error;
    }
}
