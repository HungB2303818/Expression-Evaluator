using CommunityToolkit.Mvvm.ComponentModel;

namespace ExpressionEvaluator.ViewModels;

/// <summary>Trạng thái hiển thị của một node, tách khỏi Parse Tree bất biến.</summary>
public partial class ParseTreeNodeViewModel(int nodeId, string label, double x, double y) : ObservableObject
{
    public int NodeId { get; } = nodeId;
    public string Label { get; } = label;
    public double X { get; } = x;
    public double Y { get; } = y;

    [ObservableProperty]
    private bool isHighlighted;

    [ObservableProperty]
    private bool isErrorHighlighted;
}
