namespace ExpressionEvaluator.Models;

public sealed record TreeEdge
{
    public int ParentId { get; }
    public int ChildId { get; }

    public TreeEdge(int ParentId, int ChildId)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ParentId);
        ArgumentOutOfRangeException.ThrowIfNegative(ChildId);
        if (ParentId == ChildId)
            throw new ArgumentException("Cạnh không được nối node với chính nó.", nameof(ChildId));
        this.ParentId = ParentId;
        this.ChildId = ChildId;
    }
}
