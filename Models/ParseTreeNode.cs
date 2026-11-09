namespace ExpressionEvaluator.Models;

/// <summary>Cây cú pháp bất biến, không lưu giá trị tính toán hay geometry.</summary>
public sealed record ParseTreeNode
{
    public int Id { get; }
    public string Symbol { get; }
    public string? Lexeme { get; }
    public SourceSpan Span { get; }
    public IReadOnlyList<ParseTreeNode> Children { get; }

    public ParseTreeNode(int Id, string Symbol, string? Lexeme, SourceSpan Span,
        IReadOnlyList<ParseTreeNode> Children)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(Symbol);
        ArgumentNullException.ThrowIfNull(Children);
        var children = Children.ToArray();
        if (children.Any(child => child is null))
            throw new ArgumentException("Node con không được null.", nameof(Children));
        if (Lexeme is not null && children.Length != 0)
            throw new ArgumentException("Terminal không có node con.", nameof(Children));

        this.Id = Id;
        this.Symbol = Symbol;
        this.Lexeme = Lexeme;
        this.Span = Span;
        this.Children = Array.AsReadOnly(children);
    }
}
