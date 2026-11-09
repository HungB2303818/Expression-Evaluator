using System.Collections.Frozen;

namespace ExpressionEvaluator.Models;

public sealed record EvaluationStep
{
    public int StepNumber { get; }
    public EvaluationStepType Type { get; }
    public OperationKind Operation { get; }
    public decimal? LeftOperand { get; }
    public decimal RightOperand { get; }
    public decimal? Result { get; }
    public string Description { get; }
    public IReadOnlySet<int> HighlightedNodeIds { get; }
    public ExpressionError? Error { get; }

    public EvaluationStep(int StepNumber, EvaluationStepType Type, OperationKind Operation,
        decimal? LeftOperand, decimal RightOperand, decimal? Result, string Description,
        IReadOnlySet<int> HighlightedNodeIds, ExpressionError? Error = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(StepNumber, 1);
        if (!Enum.IsDefined(Type)) throw new ArgumentOutOfRangeException(nameof(Type));
        if (!Enum.IsDefined(Operation)) throw new ArgumentOutOfRangeException(nameof(Operation));
        ArgumentException.ThrowIfNullOrWhiteSpace(Description);
        ArgumentNullException.ThrowIfNull(HighlightedNodeIds);
        bool isUnary = Operation is OperationKind.UnaryPlus or OperationKind.UnaryMinus;
        if (isUnary == LeftOperand.HasValue)
            throw new ArgumentException("Unary không có toán hạng trái; binary phải có toán hạng trái.", nameof(LeftOperand));
        if (Type == EvaluationStepType.Calculation
            ? !Result.HasValue || Error is not null
            : Result.HasValue || Error?.Category != ExpressionErrorCategory.Evaluation)
            throw new ArgumentException("Result và Error không khớp loại bước.", nameof(Result));
        var ids = HighlightedNodeIds.ToFrozenSet();
        if (ids.Any(id => id < 0))
            throw new ArgumentException("ID node không được âm.", nameof(HighlightedNodeIds));

        this.StepNumber = StepNumber;
        this.Type = Type;
        this.Operation = Operation;
        this.LeftOperand = LeftOperand;
        this.RightOperand = RightOperand;
        this.Result = Result;
        this.Description = Description;
        this.HighlightedNodeIds = ids;
        this.Error = Error;
    }
}
