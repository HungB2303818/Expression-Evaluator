using ExpressionEvaluator.Models;
using ExpressionEvaluator.Services;
using ExpressionEvaluator.ViewModels;
using Xunit;

namespace ExpressionEvaluator.Tests;

// Kiểm tra phần logic của các ca UI; KHÔNG thay thế kiểm thử hiển thị WPF.
public class StateAndTreeCases
{
    private static MainViewModel NewViewModel() =>
        new(new ExpressionService(), new NumberFormatter(), new StubGrammarDialog());

    private sealed class StubGrammarDialog : IGrammarDialogService
    {
        public void ShowGrammar() { }
    }

    private static IEnumerable<ParseTreeNode> Walk(ParseTreeNode root)
    {
        yield return root;
        foreach (ParseTreeNode child in root.Children)
            foreach (ParseTreeNode node in Walk(child))
                yield return node;
    }

    [Fact]
    public void UI_01_03_04_05_06_07_CalculationAndNavigationState()
    {
        MainViewModel vm = NewViewModel();
        vm.InputExpression = "3+5*2";
        vm.CalculateCommand.Execute(null);
        Assert.Equal("13", vm.ResultText);
        Assert.True(vm.HasTree);
        Assert.Equal("Bước 0/2", vm.StepCounterText);
        Assert.Empty(vm.HighlightedNodeIds);
        Assert.False(vm.PreviousStepCommand.CanExecute(null));
        vm.NextStepCommand.Execute(null);
        Assert.Equal("Bước 1/2", vm.StepCounterText);
        Assert.Equal(10m, vm.CurrentStep!.Result);
        Assert.NotEmpty(vm.HighlightedNodeIds);
        vm.NextStepCommand.Execute(null);
        Assert.Equal(13m, vm.CurrentStep!.Result);
        Assert.False(vm.NextStepCommand.CanExecute(null));
        vm.PreviousStepCommand.Execute(null);
        Assert.Equal(10m, vm.CurrentStep!.Result);
        vm.PreviousStepCommand.Execute(null);
        Assert.Empty(vm.HighlightedNodeIds);
        Assert.Equal("13", vm.ResultText);
    }

    [Fact]
    public void UI_08_09_10_11_StaleSubmitAndInvalidSubmitState()
    {
        MainViewModel vm = NewViewModel();
        Assert.False(vm.HasAnalysis);
        vm.InputExpression = "42";
        vm.CalculateCommand.Execute(null);
        Assert.Equal("42", vm.ResultText);
        Assert.False(vm.NextStepCommand.CanExecute(null));
        vm.InputExpression = "3+5*2";
        vm.CalculateCommand.Execute(null);
        TreeLayoutResult? oldLayout = vm.TreeLayout;
        vm.InputExpression = "3+5*4";
        Assert.True(vm.IsResultOutdated);
        Assert.Equal("3+5*2", vm.LastAnalyzedExpression);
        Assert.Equal("13", vm.ResultText);
        Assert.Same(oldLayout, vm.TreeLayout);
        vm.CalculateCommand.Execute(null);
        Assert.Equal("23", vm.ResultText);
        Assert.False(vm.IsResultOutdated);
        Assert.Equal(0, vm.CurrentStepIndex);
        Assert.NotSame(oldLayout, vm.TreeLayout);
        vm.InputExpression = "3+*5";
        vm.CalculateCommand.Execute(null);
        Assert.Equal(ExpressionErrorCategory.Syntax, vm.CurrentAnalysis!.Error!.Category);
        Assert.False(vm.HasResult);
        Assert.False(vm.HasTree);
        Assert.Empty(vm.Steps);
        Assert.Null(vm.TreeLayout);
    }

    [Fact]
    public void UI_12_13_ErrorIsNavigableWithoutLosingTree()
    {
        MainViewModel vm = NewViewModel();
        vm.InputExpression = "8/(4-4)";
        vm.CalculateCommand.Execute(null);
        Assert.True(vm.HasError);
        Assert.True(vm.HasTree);
        Assert.False(vm.HasResult);
        Assert.Equal("Bước 0/2", vm.StepCounterText);
        vm.NextStepCommand.Execute(null);
        Assert.False(vm.IsCurrentStepError);
        vm.NextStepCommand.Execute(null);
        Assert.True(vm.IsCurrentStepError);
        Assert.NotEmpty(vm.HighlightedNodeIds);
        vm.PreviousStepCommand.Execute(null);
        Assert.False(vm.IsCurrentStepError);
        Assert.Equal("Bước 1/2", vm.StepCounterText);
    }

    [Fact]
    public void UI_15_22_25_RightBranchGrammarAndFoldLeftProvenance()
    {
        var service = new ExpressionService();
        ParseTreeNode simple = service.Analyze("3+5*2").ParseTree!;
        Assert.Equal("E", simple.Symbol);
        Assert.All(Walk(simple).Where(n => n.Lexeme is null),
            node => Assert.Contains(node.Symbol, new[] { "E", "T", "F" }));
        Assert.Equal(new[] { "3", "+", "5", "*", "2" },
            Walk(simple).Where(n => n.Lexeme is not null).Select(n => n.Lexeme));

        ExpressionAnalysisResult subtraction = service.Analyze("10-3-2");
        Assert.Equal("E", subtraction.ParseTree!.Children[2].Symbol);
        Assert.Equal("E", subtraction.ParseTree.Children[2].Children[2].Symbol);
        Assert.Equal(new decimal?[] { 7m, 5m }, subtraction.Evaluation!.Steps.Select(s => s.Result));
        var terminals = Walk(subtraction.ParseTree).Where(n => n.Lexeme is not null)
            .ToDictionary(n => n.Lexeme! + "@" + n.Span.StartIndex, n => n.Id);
        Assert.All(terminals.Values, id =>
            Assert.Contains(id, subtraction.Evaluation.Steps[1].HighlightedNodeIds));

        ExpressionAnalysisResult unary = service.Analyze("--5");
        ParseTreeNode outerF = unary.ParseTree!.Children[0].Children[0];
        Assert.Equal(2, outerF.Children.Count);
        Assert.Equal(2, outerF.Children[1].Children.Count);
        Assert.Equal(new decimal?[] { -5m, 5m }, unary.Evaluation!.Steps.Select(s => s.Result));
        Assert.Contains(outerF.Children[0].Id, unary.Evaluation.Steps[1].HighlightedNodeIds);
        Assert.Contains(outerF.Children[1].Children[0].Id, unary.Evaluation.Steps[0].HighlightedNodeIds);
    }

    [Fact]
    public void UI_23_26_LayoutReservesSpaceForLongNumbers()
    {
        string literal = "79228162514264337593543950335";
        string input = literal + string.Concat(Enumerable.Repeat("+0", 84));
        Assert.Equal(197, input.Length);
        ParseTreeNode tree = new ExpressionService().Analyze(input).ParseTree!;
        TreeLayoutResult layout = new TreeLayoutService().Arrange(tree);
        var positions = layout.Positions.ToDictionary(p => p.NodeId);
        ParseTreeNode number = Walk(tree).First(n => n.Lexeme == literal);
        Assert.True(TreeLayoutService.GetDiameter(number) >= literal.Length * 11 + 36);
        Assert.True(layout.Width > TreeLayoutService.GetDiameter(number));
        Assert.True(layout.Height > TreeLayoutService.GetDiameter(number));
        Assert.All(layout.Edges, edge =>
            Assert.True(positions[edge.ChildId].Y > positions[edge.ParentId].Y));
    }
}
