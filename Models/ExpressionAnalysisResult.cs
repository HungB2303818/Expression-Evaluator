namespace ExpressionEvaluator.Models;

public sealed record ExpressionAnalysisResult
{
    public ParseTreeNode? ParseTree { get; }
    public EvaluationResult? Evaluation { get; }
    public ExpressionError? Error { get; }
    public bool IsSuccess => Error is null && Evaluation?.IsSuccess == true;

    public ExpressionAnalysisResult(ParseTreeNode? ParseTree, EvaluationResult? Evaluation, ExpressionError? Error)
    {
        bool isValid = Error?.Category switch
        {
            null => ParseTree is not null && Evaluation?.IsSuccess == true,
            ExpressionErrorCategory.Input or ExpressionErrorCategory.Lexical or ExpressionErrorCategory.Syntax
                => ParseTree is null && Evaluation is null,
            ExpressionErrorCategory.Evaluation => ParseTree is not null && Evaluation is not null
                && !Evaluation.IsSuccess && ReferenceEquals(Evaluation.Error, Error),
            _ => false
        };
        if (!isValid)
            throw new ArgumentException("Trạng thái ParseTree/Evaluation/Error không hợp lệ.");

        this.ParseTree = ParseTree;
        this.Evaluation = Evaluation;
        this.Error = Error;
    }
}
