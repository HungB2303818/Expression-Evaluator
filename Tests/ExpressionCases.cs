using ExpressionEvaluator.Models;
using ExpressionEvaluator.Services;
using Xunit;

namespace ExpressionEvaluator.Tests;

public class ExpressionCases
{
    private readonly ExpressionService service = new();
    private readonly NumberFormatter formatter = new();

    // Mỗi ID là một test độc lập trong output của dotnet test.
    public static TheoryData<string, string, string, decimal[]> ValidCases => new()
    {
        { "EV-01", "3+5*2", "13", [10m, 13m] },
        { "EV-02", "(3+5)*2", "16", [8m, 16m] },
        { "EV-03", "10-3-2", "5", [7m, 5m] },
        { "EV-04", "8/4/2", "1", [2m, 1m] },
        { "EV-05", "7/2", "3.5", [3.5m] },
        { "EV-06", "1/3", "0.333333", [1m/3m] },
        { "EV-07", "10/6", "1.666667", [10m/6m] },
        { "EV-08", "8/4", "2", [2m] },
        { "EV-09", "-5+2", "-3", [-5m, -3m] },
        { "EV-10", "3*-2", "-6", [-2m, -6m] },
        { "EV-11", "--5", "5", [-5m, 5m] },
        { "EV-12", "+5", "5", [5m] },
        { "EV-13", "-(2+3)*4", "-20", [5m, -5m, -20m] },
        { "EV-14", "+(-4)", "-4", [-4m, -4m] },
        { "EV-15", "42", "42", [] },
        { "EV-16", "0", "0", [] },
        { "EV-17", "00012+0003", "15", [15m] },
        { "EV-18", "3 + 5 * ( 2 - 1 )", "8", [1m, 5m, 8m] },
        { "EV-19", "5-10", "-5", [-5m] },
        { "EV-20", "1/8", "0.125", [0.125m] },
        { "EV-21", "1/10000000", "0", [0.0000001m] },
        { "EV-22", "8/(2+2)", "2", [4m, 2m] },
        { "EV-23", "+-5", "-5", [-5m, -5m] },
        { "EV-24", "-0", "0", [0m] },
        { "EV-25", "1/128", "0.007813", [0.0078125m] },
        { "EV-26", "-1/128", "-0.007813", [-1m, -0.0078125m] },
        { "EV-27", "2*3+6", "12", [6m, 12m] },
        { "EV-28", "1/10000000*10000000", "1", [0.0000001m, 1m] }
    };

    [Theory]
    [MemberData(nameof(ValidCases))]
    public void EV_Success_ValueAndOrderedSteps(string id, string input, string expected, decimal[] results)
    {
        ExpressionAnalysisResult analysis = service.Analyze(input);
        Assert.True(analysis.IsSuccess, $"{id}: {analysis.Error?.Message}");
        Assert.Equal("E", analysis.ParseTree!.Symbol);
        Assert.Equal(expected, formatter.Format(analysis.Evaluation!.Value!.Value));
        Assert.Equal(results.Length, analysis.Evaluation.Steps.Count);
        Assert.Equal(results, analysis.Evaluation.Steps.Select(step => step.Result!.Value));
        Assert.All(analysis.Evaluation.Steps, step => Assert.Equal(EvaluationStepType.Calculation, step.Type));
    }

    public static TheoryData<string, string, ExpressionErrorCategory, int?, string> InvalidCases => new()
    {
        { "ER-01", " ", ExpressionErrorCategory.Input, null, "EmptyExpression" },
        { "ER-02", "3 & 4", ExpressionErrorCategory.Lexical, 3, "InvalidCharacter" },
        { "ER-03", "3.5", ExpressionErrorCategory.Lexical, 2, "InvalidCharacter" },
        { "ER-04", "3,5", ExpressionErrorCategory.Lexical, 2, "InvalidCharacter" },
        { "ER-05", "3 + * 5", ExpressionErrorCategory.Syntax, 5, "ExpectedOperand" },
        { "ER-06", "(2+3", ExpressionErrorCategory.Syntax, 5, "MissingRightParen" },
        { "ER-07", "2(3+4)", ExpressionErrorCategory.Syntax, 2, "MissingOperator" },
        { "ER-08", "(2+3)(4+1)", ExpressionErrorCategory.Syntax, 6, "MissingOperator" },
        { "ER-09", "3 4", ExpressionErrorCategory.Syntax, 3, "MissingOperator" },
        { "ER-10", "3+", ExpressionErrorCategory.Syntax, 3, "ExpectedOperand" },
        { "ER-11", "()", ExpressionErrorCategory.Syntax, 2, "ExpectedOperand" },
        { "ER-12", "3)", ExpressionErrorCategory.Syntax, 2, "UnexpectedRightParen" },
        { "ER-13", "1e3", ExpressionErrorCategory.Lexical, 2, "InvalidCharacter" },
        { "ER-14", "79228162514264337593543950336", ExpressionErrorCategory.Lexical, 1, "NumberOutOfRange" },
        { "ER-16", new string(' ', 200) + "1", ExpressionErrorCategory.Input, null, "InputTooLong" },
        { "ER-18", "3+@4", ExpressionErrorCategory.Lexical, 3, "InvalidCharacter" },
        { "ER-19", "2**3", ExpressionErrorCategory.Syntax, 3, "ExpectedOperand" },
        { "ER-20", "3/(2-2", ExpressionErrorCategory.Syntax, 7, "MissingRightParen" }
    };

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void ER_RejectsInvalidInputWithoutTreeOrTrace(string id, string input,
        ExpressionErrorCategory category, int? position, string code)
    {
        ExpressionAnalysisResult analysis = service.Analyze(input);
        Assert.False(analysis.IsSuccess, id);
        Assert.Equal(category, analysis.Error!.Category);
        Assert.Equal(code, analysis.Error.Code);
        Assert.Equal(position, analysis.Error.Span?.DisplayPosition);
        Assert.Null(analysis.ParseTree);
        Assert.Null(analysis.Evaluation);
    }

    [Theory]
    [InlineData("ER-15", "79228162514264337593543950335", "79228162514264337593543950335", 29)]
    [InlineData("ER-17", null, "1", 200)]
    public void ER_BoundarySuccess(string id, string? input, string expected, int length)
    {
        input ??= new string(' ', 199) + "1";
        Assert.Equal(length, input.Length);
        ExpressionAnalysisResult analysis = service.Analyze(input);
        Assert.True(analysis.IsSuccess, $"{id}: {analysis.Error?.Message}");
        Assert.Equal(expected, formatter.Format(analysis.Evaluation!.Value!.Value));
        Assert.Empty(analysis.Evaluation.Steps);
        Assert.NotNull(analysis.ParseTree);
    }

    public static TheoryData<string, string, string, decimal[]> EvaluationErrors => new()
    {
        { "EE-01", "8/(4-4)", "DivisionByZero", [0m] },
        { "EE-02", "1/0", "DivisionByZero", [] },
        { "EE-03", "79228162514264337593543950335+1", "ArithmeticOverflow", [] },
        { "EE-04", "0/0", "DivisionByZero", [] },
        { "EE-05", "1/(3-3)+2", "DivisionByZero", [0m] },
        { "EE-06", "1/(2-2)", "DivisionByZero", [0m] }
    };

    [Theory]
    [MemberData(nameof(EvaluationErrors))]
    public void EE_PreservesTreeAndTraceWithTerminalError(string id, string input,
        string code, decimal[] successfulSteps)
    {
        ExpressionAnalysisResult analysis = service.Analyze(input);
        Assert.Equal(ExpressionErrorCategory.Evaluation, analysis.Error!.Category);
        Assert.Equal(code, analysis.Error.Code);
        Assert.NotNull(analysis.ParseTree);
        EvaluationResult evaluation = Assert.IsType<EvaluationResult>(analysis.Evaluation);
        Assert.Null(evaluation.Value);
        Assert.Same(analysis.Error, evaluation.Error);
        Assert.Equal(successfulSteps.Length + 1, evaluation.Steps.Count);
        Assert.Equal(successfulSteps, evaluation.Steps.SkipLast(1).Select(s => s.Result!.Value));
        Assert.Equal(EvaluationStepType.Error, evaluation.Steps[^1].Type);
        Assert.Same(analysis.Error, evaluation.Steps[^1].Error);
        Assert.DoesNotContain("NaN", evaluation.Steps[^1].Description);
        Assert.DoesNotContain("Infinity", evaluation.Steps[^1].Description);
        if (id == "EE-01") Assert.Equal(2, analysis.Error.Span?.DisplayPosition);
    }

    [Fact]
    public void EV_27_28_OperatorOrderAndUnroundedIntermediate()
    {
        EvaluationResult precedence = service.Analyze("2*3+6").Evaluation!;
        Assert.Equal(new[] { OperationKind.Multiply, OperationKind.Add },
            precedence.Steps.Select(step => step.Operation));
        Assert.Equal(new decimal?[] { 2m, 6m }, precedence.Steps.Select(step => step.LeftOperand));

        EvaluationResult precision = service.Analyze("1/10000000*10000000").Evaluation!;
        Assert.Equal(new[] { OperationKind.Divide, OperationKind.Multiply },
            precision.Steps.Select(step => step.Operation));
        Assert.Equal(0.0000001m, precision.Steps[1].LeftOperand);
        Assert.Contains("≈", precision.Steps[0].Description);
        Assert.Contains("≈", precision.Steps[1].Description);
        Assert.Equal(1m, precision.Value);
    }
}
