using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using ExpressionEvaluator.Models;
using ExpressionEvaluator.Services;
using ExpressionEvaluator.ViewModels;

namespace ExpressionEvaluator.Views.Controls;

/// <summary>Vẽ geometry khi Submit; khi đổi step chỉ cập nhật IsHighlighted qua binding.</summary>
public partial class ParseTreeView : UserControl
{
    public static readonly DependencyProperty TreeRootProperty = DependencyProperty.Register(
        nameof(TreeRoot), typeof(ParseTreeNode), typeof(ParseTreeView),
        new PropertyMetadata(null, OnGeometryChanged));

    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
        nameof(Layout), typeof(TreeLayoutResult), typeof(ParseTreeView),
        new PropertyMetadata(null, OnGeometryChanged));

    public static readonly DependencyProperty HighlightedNodeIdsProperty = DependencyProperty.Register(
        nameof(HighlightedNodeIds), typeof(IReadOnlySet<int>), typeof(ParseTreeView),
        new PropertyMetadata(null, OnHighlightChanged));

    public static readonly DependencyProperty IsErrorStepProperty = DependencyProperty.Register(
        nameof(IsErrorStep), typeof(bool), typeof(ParseTreeView),
        new PropertyMetadata(false, OnHighlightChanged));

    private readonly Dictionary<int, ParseTreeNodeViewModel> displayedNodes = [];

    public ParseTreeNode? TreeRoot
    {
        get => (ParseTreeNode?)GetValue(TreeRootProperty);
        set => SetValue(TreeRootProperty, value);
    }

    public TreeLayoutResult? Layout
    {
        get => (TreeLayoutResult?)GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public IReadOnlySet<int>? HighlightedNodeIds
    {
        get => (IReadOnlySet<int>?)GetValue(HighlightedNodeIdsProperty);
        set => SetValue(HighlightedNodeIdsProperty, value);
    }

    public bool IsErrorStep
    {
        get => (bool)GetValue(IsErrorStepProperty);
        set => SetValue(IsErrorStepProperty, value);
    }

    public ParseTreeView() => InitializeComponent();

    private static void OnGeometryChanged(DependencyObject target, DependencyPropertyChangedEventArgs args) =>
        ((ParseTreeView)target).RenderTree();

    private static void OnHighlightChanged(DependencyObject target, DependencyPropertyChangedEventArgs args) =>
        ((ParseTreeView)target).UpdateHighlights();

    private void RenderTree()
    {
        TreeCanvas.Children.Clear();
        displayedNodes.Clear();
        if (TreeRoot is null || Layout is null)
        {
            TreeCanvas.Width = 0;
            TreeCanvas.Height = 0;
            return;
        }

        TreeCanvas.Width = Layout.Width;
        TreeCanvas.Height = Layout.Height;
        var nodes = new Dictionary<int, ParseTreeNode>();
        CollectVisibleNodes(TreeRoot, nodes);
        var positions = Layout.Positions.ToDictionary(position => position.NodeId);
        Brush edgeBrush = (Brush)FindResource("BorderBrush");

        // Vẽ các cạnh dưới lớp node. Toạ độ vị trí là góc trên bên trái.
        foreach (TreeEdge edge in Layout.Edges)
        {
            TreeNodePosition parent = positions[edge.ParentId];
            TreeNodePosition child = positions[edge.ChildId];
            double parentDiameter = TreeLayoutService.GetDiameter(nodes[edge.ParentId]);
            double childDiameter = TreeLayoutService.GetDiameter(nodes[edge.ChildId]);
            var parentCenter = new Point(parent.X + parentDiameter / 2, parent.Y + parentDiameter / 2);
            var childCenter = new Point(child.X + childDiameter / 2, child.Y + childDiameter / 2);
            Vector direction = childCenter - parentCenter;
            direction.Normalize();
            Point start = parentCenter + direction * (parentDiameter / 2);
            Point tip = childCenter - direction * (childDiameter / 2);
            var line = new Line
            {
                X1 = start.X, Y1 = start.Y,
                X2 = tip.X, Y2 = tip.Y,
                Stroke = edgeBrush,
                StrokeThickness = 1.5,
                IsHitTestVisible = false
            };
            TreeCanvas.Children.Add(line);
            Point arrowBase = tip - direction * 10;
            var perpendicular = new Vector(-direction.Y, direction.X);
            TreeCanvas.Children.Add(new Polygon
            {
                Points = new PointCollection
                {
                    tip, arrowBase + perpendicular * 4, arrowBase - perpendicular * 4
                },
                Fill = edgeBrush,
                IsHitTestVisible = false
            });
        }

        Style nodeStyle = CreateNodeStyle();
        foreach (TreeNodePosition position in Layout.Positions)
        {
            ParseTreeNode node = nodes[position.NodeId];
            var viewModel = new ParseTreeNodeViewModel(node.Id,
                TreeLayoutService.GetLabel(node), position.X, position.Y);
            displayedNodes.Add(node.Id, viewModel);
            double diameter = TreeLayoutService.GetDiameter(node);

            if (node.Id == TreeRoot.Id)
            {
                var rootRing = new Ellipse
                {
                    Width = diameter + 8,
                    Height = diameter + 8,
                    Stroke = (Brush)FindResource("PrimaryBrush"),
                    StrokeThickness = 3,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(rootRing, position.X - 4);
                Canvas.SetTop(rootRing, position.Y - 4);
                TreeCanvas.Children.Add(rootRing);
            }

            var border = new Border
            {
                Width = diameter,
                Height = diameter,
                CornerRadius = new CornerRadius(diameter / 2),
                DataContext = viewModel,
                Style = nodeStyle,
                Child = new TextBlock
                {
                    Text = viewModel.Label,
                    Foreground = (Brush)FindResource("TextBrush"),
                    FontSize = 14,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(8),
                    TextTrimming = TextTrimming.None
                }
            };
            Canvas.SetLeft(border, position.X);
            Canvas.SetTop(border, position.Y);
            TreeCanvas.Children.Add(border);
        }

        UpdateHighlights();
    }

    private Style CreateNodeStyle()
    {
        var style = new Style(typeof(Border));
        style.Setters.Add(new Setter(Border.BackgroundProperty, (Brush)FindResource("BackgroundBrush")));
        style.Setters.Add(new Setter(Border.BorderBrushProperty, (Brush)FindResource("BorderBrush")));
        style.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(1)));

        var highlighted = new DataTrigger
        {
            Binding = new Binding(nameof(ParseTreeNodeViewModel.IsHighlighted)),
            Value = true
        };
        highlighted.Setters.Add(new Setter(Border.BackgroundProperty,
            new SolidColorBrush(Color.FromRgb(219, 234, 254))));
        highlighted.Setters.Add(new Setter(Border.BorderBrushProperty, (Brush)FindResource("PrimaryBrush")));
        highlighted.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(2)));
        style.Triggers.Add(highlighted);
        var errorHighlighted = new DataTrigger
        {
            Binding = new Binding(nameof(ParseTreeNodeViewModel.IsErrorHighlighted)),
            Value = true
        };
        errorHighlighted.Setters.Add(new Setter(Border.BackgroundProperty,
            (Brush)FindResource("ErrorBackgroundBrush")));
        errorHighlighted.Setters.Add(new Setter(Border.BorderBrushProperty,
            (Brush)FindResource("ErrorBrush")));
        errorHighlighted.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(2)));
        style.Triggers.Add(errorHighlighted);
        return style;
    }

    private void UpdateHighlights()
    {
        foreach (ParseTreeNodeViewModel node in displayedNodes.Values)
        {
            bool highlighted = HighlightedNodeIds?.Contains(node.NodeId) == true;
            node.IsHighlighted = highlighted;
            node.IsErrorHighlighted = highlighted && IsErrorStep;
        }
    }

    private static void CollectVisibleNodes(ParseTreeNode root, Dictionary<int, ParseTreeNode> nodes)
    {
        nodes.Add(root.Id, root);
        foreach (ParseTreeNode child in root.Children)
            CollectVisibleNodes(child, nodes);
    }
}
