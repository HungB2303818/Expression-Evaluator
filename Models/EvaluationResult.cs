namespace ExpressionEvaluator.Models;

public sealed record EvaluationResult
{
    public decimal? Value { get; }
    public IReadOnlyList<EvaluationStep> Steps { get; }
    public ExpressionError? Error { get; }
    public bool IsSuccess => Error is null && Value.HasValue;

    public EvaluationResult(decimal? Value, IReadOnlyList<EvaluationStep> Steps, ExpressionError? Error)
    {
        ArgumentNullException.ThrowIfNull(Steps);
        var steps = Steps.ToArray();
        for (int i = 0; i < steps.Length; i++)
        {
            if (steps[i] is null || steps[i].StepNumber != i + 1)
                throw new ArgumentException("Các bước phải liên tục từ 1 và không null.", nameof(Steps));
            if (steps[i].Type == EvaluationStepType.Error && i != steps.Length - 1)
                throw new ArgumentException("Bước lỗi chỉ được nằm cuối trace.", nameof(Steps));
        }

        if (Error is null)
        {
            if (!Value.HasValue || steps.Any(step => step.Type == EvaluationStepType.Error))
                throw new ArgumentException("Thành công phải có Value và không có bước lỗi.", nameof(Value));
        }
        else if (Value.HasValue || Error.Category != ExpressionErrorCategory.Evaluation
            || steps.Length == 0 || steps[^1].Type != EvaluationStepType.Error
            || !ReferenceEquals(steps[^1].Error, Error))
        {
            throw new ArgumentException("Thất bại phải giữ trace có bước lỗi cuối cùng và cùng đối tượng Error.", nameof(Error));
        }

        this.Value = Value;
        this.Steps = Array.AsReadOnly(steps);
        this.Error = Error;
    }
}
