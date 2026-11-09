namespace ExpressionEvaluator.Models;

public sealed record Token
{
    public TokenType Type { get; }
    public string Lexeme { get; }
    public SourceSpan Span { get; }
    public decimal? NumericValue { get; }

    public Token(TokenType Type, string Lexeme, SourceSpan Span, decimal? NumericValue = null)
    {
        if (!Enum.IsDefined(Type)) throw new ArgumentOutOfRangeException(nameof(Type));
        ArgumentNullException.ThrowIfNull(Lexeme);
        if ((Type == TokenType.Number) != NumericValue.HasValue)
            throw new ArgumentException("Chỉ token Number phải có NumericValue.", nameof(NumericValue));

        this.Type = Type;
        this.Lexeme = Lexeme;
        this.Span = Span;
        this.NumericValue = NumericValue;
    }
}
