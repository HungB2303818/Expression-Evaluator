namespace ExpressionEvaluator.Models;

public sealed record TreeLayoutResult
{
    public double Width { get; }
    public double Height { get; }
    public IReadOnlyList<TreeNodePosition> Positions { get; }
    public IReadOnlyList<TreeEdge> Edges { get; }

    public TreeLayoutResult(double Width, double Height,
        IReadOnlyList<TreeNodePosition> Positions, IReadOnlyList<TreeEdge> Edges)
    {
        if (!double.IsFinite(Width) || Width < 0) throw new ArgumentOutOfRangeException(nameof(Width));
        if (!double.IsFinite(Height) || Height < 0) throw new ArgumentOutOfRangeException(nameof(Height));
        ArgumentNullException.ThrowIfNull(Positions);
        ArgumentNullException.ThrowIfNull(Edges);
        var positions = Positions.ToArray();
        var edges = Edges.ToArray();
        var ids = new HashSet<int>();
        foreach (var position in positions)
        {
            if (position is null || !ids.Add(position.NodeId)
                || position.X > Width || position.Y > Height)
                throw new ArgumentException("Vị trí phải khác null, có ID duy nhất và nằm trong layout.", nameof(Positions));
        }
        if (edges.Any(edge => edge is null || !ids.Contains(edge.ParentId) || !ids.Contains(edge.ChildId)))
            throw new ArgumentException("Cạnh phải tham chiếu các node trong layout.", nameof(Edges));

        this.Width = Width;
        this.Height = Height;
        this.Positions = Array.AsReadOnly(positions);
        this.Edges = Array.AsReadOnly(edges);
    }
}
