namespace ExpressionEvaluator.Models;

public sealed record ExpressionError
{
    public ExpressionErrorCategory Category { get; }
    public string Code { get; }
    public string Message { get; }
    public SourceSpan? Span { get; }

    public ExpressionError(ExpressionErrorCategory Category, string Code, string Message, SourceSpan? Span)
    {
        if (!Enum.IsDefined(Category)) throw new ArgumentOutOfRangeException(nameof(Category));
        ArgumentException.ThrowIfNullOrWhiteSpace(Code);
        ArgumentException.ThrowIfNullOrWhiteSpace(Message);
        this.Category = Category;
        this.Code = Code;
        this.Message = Message;
        this.Span = Span;
    }
}
