using System.Collections.Frozen;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpressionEvaluator.Models;
using ExpressionEvaluator.Services;

namespace ExpressionEvaluator.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private static readonly IReadOnlySet<int> EmptyHighlights = new HashSet<int>().ToFrozenSet();
    private readonly ExpressionService expressionService;
    private readonly NumberFormatter numberFormatter;
    private readonly IGrammarDialogService grammarDialogService;
    private readonly TreeLayoutService treeLayoutService = new();

    public MainViewModel() : this(new ExpressionService(), new NumberFormatter(), new GrammarDialogService()) { }

    public MainViewModel(ExpressionService expressionService, NumberFormatter numberFormatter,
        IGrammarDialogService grammarDialogService)
    {
        this.expressionService = expressionService ?? throw new ArgumentNullException(nameof(expressionService));
        this.numberFormatter = numberFormatter ?? throw new ArgumentNullException(nameof(numberFormatter));
        this.grammarDialogService = grammarDialogService ?? throw new ArgumentNullException(nameof(grammarDialogService));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsResultOutdated))]
    [NotifyPropertyChangedFor(nameof(InputLengthText))]
    private string inputExpression = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsResultOutdated))]
    private string? lastAnalyzedExpression;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAnalysis))]
    [NotifyPropertyChangedFor(nameof(IsResultOutdated))]
    [NotifyPropertyChangedFor(nameof(Steps))]
    [NotifyPropertyChangedFor(nameof(ResultText))]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(HasTree))]
    [NotifyPropertyChangedFor(nameof(ErrorText))]
    [NotifyPropertyChangedFor(nameof(CurrentStep))]
    [NotifyPropertyChangedFor(nameof(IsCurrentStepError))]
    [NotifyPropertyChangedFor(nameof(HighlightedNodeIds))]
    [NotifyPropertyChangedFor(nameof(StepText))]
    [NotifyPropertyChangedFor(nameof(StepCounterText))]
    private ExpressionAnalysisResult? currentAnalysis;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentStep))]
    [NotifyPropertyChangedFor(nameof(IsCurrentStepError))]
    [NotifyPropertyChangedFor(nameof(HighlightedNodeIds))]
    [NotifyPropertyChangedFor(nameof(StepText))]
    [NotifyPropertyChangedFor(nameof(StepCounterText))]
    private int currentStepIndex;

    [ObservableProperty]
    private TreeLayoutResult? treeLayout;

    public bool HasAnalysis => CurrentAnalysis is not null;
    public bool HasResult => CurrentAnalysis?.Evaluation?.Value.HasValue == true;
    public bool HasError => CurrentAnalysis?.Error is not null;
    public bool HasTree => CurrentAnalysis?.ParseTree is not null;
    public bool IsResultOutdated => HasAnalysis && InputExpression != LastAnalyzedExpression;
    public string InputLengthText => $"{InputExpression.Length} / 200 ký tự";
    public IReadOnlyList<EvaluationStep> Steps => CurrentAnalysis?.Evaluation?.Steps ?? Array.Empty<EvaluationStep>();
    public EvaluationStep? CurrentStep => CurrentStepIndex > 0 && CurrentStepIndex <= Steps.Count
        ? Steps[CurrentStepIndex - 1] : null;
    public bool IsCurrentStepError => CurrentStep?.Type == EvaluationStepType.Error;
    public IReadOnlySet<int> HighlightedNodeIds => CurrentStep?.HighlightedNodeIds ?? EmptyHighlights;
    public string ResultText => CurrentAnalysis?.Evaluation?.Value is decimal value
        ? numberFormatter.Format(value) : string.Empty;
    public string? ErrorText
    {
        get
        {
            ExpressionError? error = CurrentAnalysis?.Error;
            if (error is null) return null;

            string category = error.Category switch
            {
                ExpressionErrorCategory.Input => "Lỗi đầu vào",
                ExpressionErrorCategory.Lexical => "Lỗi từ vựng",
                ExpressionErrorCategory.Syntax => "Lỗi cú pháp",
                _ => "Lỗi tính toán"
            };
            string location = error.Span is SourceSpan span
                ? $" (vị trí {span.DisplayPosition}{(span.Length == 0 ? ", cuối biểu thức" : "")})"
                : string.Empty;
            return $"{category}{location}: {error.Message}";
        }
    }

    public string StepCounterText => $"Bước {CurrentStepIndex}/{Steps.Count}";
    public string StepText => CurrentStep?.Description ?? (Steps.Count == 0
        ? "Biểu thức không cần bước tính trung gian."
        : "Nhấn Tiếp để xem từng bước tính.");

    partial void OnCurrentAnalysisChanged(ExpressionAnalysisResult? value) => UpdateStepCommands();
    partial void OnCurrentStepIndexChanged(int value) => UpdateStepCommands();

    private void UpdateStepCommands()
    {
        NextStepCommand.NotifyCanExecuteChanged();
        PreviousStepCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void Calculate()
    {
        string submitted = InputExpression;
        ExpressionAnalysisResult analysis = expressionService.Analyze(submitted);
        TreeLayoutResult? layout = analysis.ParseTree is null
            ? null : treeLayoutService.Arrange(analysis.ParseTree);
        LastAnalyzedExpression = submitted;
        CurrentStepIndex = 0;
        // Không để view ghép Parse Tree mới với geometry của lần Submit trước.
        TreeLayout = null;
        CurrentAnalysis = analysis;
        TreeLayout = layout;
    }

    [RelayCommand(CanExecute = nameof(CanNextStep))]
    private void NextStep() => CurrentStepIndex++;

    private bool CanNextStep() => CurrentStepIndex < Steps.Count;

    [RelayCommand(CanExecute = nameof(CanPreviousStep))]
    private void PreviousStep() => CurrentStepIndex--;

    private bool CanPreviousStep() => CurrentStepIndex > 0;

    [RelayCommand]
    private void ViewGrammar() => grammarDialogService.ShowGrammar();

}
