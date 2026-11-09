using ExpressionEvaluator.Models;

namespace ExpressionEvaluator.Services;

/// <summary>Bố trí node tròn theo subtree và đường kính lớn nhất ở từng tầng.</summary>
public sealed class TreeLayoutService
{
    public const double SiblingGap = 24;
    public const double LevelGap = 34;
    public const double Margin = 30;

    public static string GetLabel(ParseTreeNode node) => node.Lexeme ?? node.Symbol;

    // Terminal chỉ gồm chữ số ASCII/toán tử/ngoặc; mức dư này chứa trọn nhãn Segoe UI 14.
    public static double GetDiameter(ParseTreeNode node) =>
        Math.Max(56, GetLabel(node).Length * 11 + 36);

    public TreeLayoutResult Arrange(ParseTreeNode root)
    {
        ArgumentNullException.ThrowIfNull(root);
        if (root.Symbol != "E") throw new ArgumentException("Nút gốc phải là E.", nameof(root));

        var levelDiameters = new List<double>();
        LayoutBranch branch = BuildBranch(root, 0, levelDiameters);
        var levelY = new double[levelDiameters.Count];
        double y = Margin;
        for (int i = 0; i < levelY.Length; i++)
        {
            levelY[i] = y;
            y += levelDiameters[i] + LevelGap;
        }

        var positions = new List<TreeNodePosition>();
        var edges = new List<TreeEdge>();
        Place(branch, Margin, 0, levelY, positions, edges);
        return new TreeLayoutResult(branch.Width + 2 * Margin,
            levelY[^1] + levelDiameters[^1] + Margin, positions, edges);
    }

    private static LayoutBranch BuildBranch(ParseTreeNode node, int depth, List<double> levelDiameters)
    {
        double diameter = GetDiameter(node);
        if (levelDiameters.Count <= depth) levelDiameters.Add(diameter);
        else levelDiameters[depth] = Math.Max(levelDiameters[depth], diameter);

        LayoutBranch[] children = node.Children
            .Select(child => BuildBranch(child, depth + 1, levelDiameters)).ToArray();
        double childrenWidth = children.Sum(child => child.Width)
            + Math.Max(0, children.Length - 1) * SiblingGap;
        return new LayoutBranch(node, children, Math.Max(diameter, childrenWidth));
    }

    private static void Place(LayoutBranch branch, double startX, int depth, double[] levelY,
        List<TreeNodePosition> positions, List<TreeEdge> edges)
    {
        double centerX = startX + branch.Width / 2;
        positions.Add(new TreeNodePosition(branch.Node.Id,
            centerX - GetDiameter(branch.Node) / 2, levelY[depth]));

        double childrenWidth = branch.Children.Sum(child => child.Width)
            + Math.Max(0, branch.Children.Length - 1) * SiblingGap;
        double childX = startX + (branch.Width - childrenWidth) / 2;
        foreach (LayoutBranch child in branch.Children)
        {
            edges.Add(new TreeEdge(branch.Node.Id, child.Node.Id));
            Place(child, childX, depth + 1, levelY, positions, edges);
            childX += child.Width + SiblingGap;
        }
    }

    private sealed record LayoutBranch(ParseTreeNode Node, LayoutBranch[] Children, double Width);
}
