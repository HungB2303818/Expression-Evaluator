using ExpressionEvaluator.Exceptions;
using ExpressionEvaluator.Models;
using ExpressionEvaluator.Parsing;
using Evaluator = ExpressionEvaluator.Evaluation.ExpressionEvaluator;

namespace ExpressionEvaluator.Services;

/// <summary>Điều phối một lần phân tích; không phụ thuộc WPF hoặc trạng thái giao diện.</summary>
public sealed class ExpressionService
{
    private const int MaxInputLength = 200;

    private readonly Lexer lexer = new();
    private readonly RecursiveDescentParser parser = new();
    private readonly Evaluator evaluator = new();

    public ExpressionAnalysisResult Analyze(string? expression)
    {
        // Kiểm tra độ dài chuỗi gốc (UTF-16), bao gồm mọi khoảng trắng, trước khi gọi Lexer.
        if (expression is not null && expression.Length > MaxInputLength)
            return InputFailure("InputTooLong",
                $"Biểu thức dài {expression.Length} ký tự, vượt giới hạn {MaxInputLength} ký tự.");

        if (string.IsNullOrWhiteSpace(expression))
            return InputFailure("EmptyExpression", "Biểu thức không được để trống hoặc chỉ chứa khoảng trắng.");

        ParseTreeNode root;
        try
        {
            IReadOnlyList<Token> tokens = lexer.Tokenize(expression);
            root = parser.Parse(tokens);
        }
        catch (LexicalException exception)
        {
            return new ExpressionAnalysisResult(null, null, exception.Error);
        }
        catch (ParseException exception)
        {
            return new ExpressionAnalysisResult(null, null, exception.Error);
        }

        EvaluationResult evaluation = evaluator.Evaluate(root);
        // Lỗi đánh giá vẫn giữ cả cây và trace, đồng thời dùng cùng đối tượng Error.
        return new ExpressionAnalysisResult(root, evaluation, evaluation.Error);
    }

    private static ExpressionAnalysisResult InputFailure(string code, string message) =>
        new(null, null, new ExpressionError(ExpressionErrorCategory.Input, code, message, null));
}
