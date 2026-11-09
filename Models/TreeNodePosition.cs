namespace ExpressionEvaluator.Models;

public sealed record TreeNodePosition
{
    public int NodeId { get; }
    public double X { get; }
    public double Y { get; }

    public TreeNodePosition(int NodeId, double X, double Y)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(NodeId);
        if (!double.IsFinite(X) || X < 0) throw new ArgumentOutOfRangeException(nameof(X));
        if (!double.IsFinite(Y) || Y < 0) throw new ArgumentOutOfRangeException(nameof(Y));
        this.NodeId = NodeId;
        this.X = X;
        this.Y = Y;
    }
}
