using System.Diagnostics;
using System.Globalization;
using ExpressionEvaluator.Models;
using ExpressionEvaluator.Services;

namespace ExpressionEvaluator.Evaluation;

/// <summary>Đánh giá trực tiếp Parse Tree, giữ trace và provenance của mỗi toán hạng.</summary>
public sealed class ExpressionEvaluator
{
    private readonly NumberFormatter formatter = new();

    public EvaluationResult Evaluate(ParseTreeNode root)
    {
        ArgumentNullException.ThrowIfNull(root);
        if (root.Symbol != "E")
            throw new ArgumentException("Nút gốc phải là E.", nameof(root));

        // Trạng thái trace chỉ tồn tại trong một lần Evaluate.
        var context = new EvaluationContext(formatter);
        return context.Evaluate(root);
    }

    private sealed record ComputedValue(decimal Value, HashSet<int> ProvenanceNodeIds);

    // Chỉ dùng bên trong để dừng mọi phép toán cha sau một bước lỗi.
    private sealed class EvaluationHaltedException : Exception;

    private sealed class EvaluationContext(NumberFormatter formatter)
    {
        private readonly List<EvaluationStep> steps = [];
        private ExpressionError? error;

        public EvaluationResult Evaluate(ParseTreeNode root)
        {
            try
            {
                ComputedValue value = EvaluateE(root);
                return new EvaluationResult(value.Value, steps, null);
            }
            catch (EvaluationHaltedException)
            {
                return new EvaluationResult(null, steps, error);
            }
        }

        private ComputedValue EvaluateE(ParseTreeNode expression)
        {
            CheckChain(expression, "E", "PLUS", "MINUS");
            ComputedValue current = EvaluateT(expression.Children[0]);
            ParseTreeNode cursor = expression;
            while (true)
            {
                if (cursor.Children.Count == 1) return current;

                ParseTreeNode op = cursor.Children[1];
                ParseTreeNode rightE = cursor.Children[2];
                CheckChain(rightE, "E", "PLUS", "MINUS");
                // Chỉ đánh giá T kế tiếp, không đánh giá nguyên E bên phải.
                ComputedValue rhs = EvaluateT(rightE.Children[0]);
                current = ApplyBinary(op, current, rhs);
                cursor = rightE;
            }
        }

        private ComputedValue EvaluateT(ParseTreeNode term)
        {
            CheckChain(term, "T", "MULTIPLY", "DIVIDE");
            ComputedValue current = EvaluateF(term.Children[0]);
            ParseTreeNode cursor = term;
            while (true)
            {
                if (cursor.Children.Count == 1) return current;

                ParseTreeNode op = cursor.Children[1];
                ParseTreeNode rightT = cursor.Children[2];
                CheckChain(rightT, "T", "MULTIPLY", "DIVIDE");
                ComputedValue rhs = EvaluateF(rightT.Children[0]);
                current = ApplyBinary(op, current, rhs);
                cursor = rightT;
            }
        }

        private ComputedValue EvaluateF(ParseTreeNode factor)
        {
            if (factor.Symbol != "F")
                throw new InvalidOperationException("Cây không có nhánh F hợp lệ.");

            if (factor.Children.Count == 1)
            {
                ParseTreeNode number = factor.Children[0];
                if (number.Symbol != "NUMBER" || number.Lexeme is null
                    || !decimal.TryParse(number.Lexeme, NumberStyles.None,
                        CultureInfo.InvariantCulture, out decimal value))
                    throw new InvalidOperationException("Literal NUMBER trong cây không hợp lệ.");

                return new ComputedValue(value, [number.Id]);
            }

            if (factor.Children.Count == 3)
            {
                if (factor.Children[0].Symbol != "LEFT_PAREN"
                    || factor.Children[2].Symbol != "RIGHT_PAREN")
                    throw new InvalidOperationException("Nhánh ngoặc trong cây không hợp lệ.");

                return EvaluateE(factor.Children[1]);
            }

            Check(factor, "F", 2);
            ParseTreeNode op = factor.Children[0];
            if (op.Symbol is not ("PLUS" or "MINUS"))
                throw new InvalidOperationException("Toán tử một ngôi không hợp lệ.");

            ComputedValue operand = EvaluateF(factor.Children[1]);
            var ids = Combine(operand.ProvenanceNodeIds, op.Id);
            OperationKind operation = op.Symbol == "PLUS" ? OperationKind.UnaryPlus : OperationKind.UnaryMinus;
            string symbol = op.Symbol == "PLUS" ? "+" : "-";
            string prefix = $"{symbol}({FormatForStep(operand.Value)})";
            try
            {
                decimal result = op.Symbol == "PLUS" ? operand.Value : -operand.Value;
                AddCalculation(operation, null, operand.Value, result,
                    $"{prefix} = {FormatForStep(result)}", ids);
                return new ComputedValue(result, ids);
            }
            catch (OverflowException)
            {
                StopWithError(op, operation, null, operand.Value, prefix, ids,
                    "ArithmeticOverflow", "Kết quả phép tính vượt phạm vi decimal.");
                throw new UnreachableException();
            }
        }

        private ComputedValue ApplyBinary(ParseTreeNode op, ComputedValue left, ComputedValue right)
        {
            OperationKind operation = op.Symbol switch
            {
                "PLUS" => OperationKind.Add,
                "MINUS" => OperationKind.Subtract,
                "MULTIPLY" => OperationKind.Multiply,
                "DIVIDE" => OperationKind.Divide,
                _ => throw new InvalidOperationException("Toán tử hai ngôi không hợp lệ.")
            };
            string symbol = operation switch
            {
                OperationKind.Add => "+",
                OperationKind.Subtract => "-",
                OperationKind.Multiply => "*",
                _ => "/"
            };
            var ids = Combine(left.ProvenanceNodeIds, op.Id, right.ProvenanceNodeIds);
            string prefix = $"{FormatForStep(left.Value)} {symbol} {FormatForStep(right.Value)}";

            if (operation == OperationKind.Divide && right.Value == 0)
            {
                StopWithError(op, operation, left.Value, right.Value, prefix, ids,
                    "DivisionByZero", "Không thể chia cho 0.");
                throw new UnreachableException();
            }

            try
            {
                decimal result = operation switch
                {
                    OperationKind.Add => left.Value + right.Value,
                    OperationKind.Subtract => left.Value - right.Value,
                    OperationKind.Multiply => left.Value * right.Value,
                    _ => left.Value / right.Value
                };
                AddCalculation(operation, left.Value, right.Value, result,
                    $"{prefix} = {FormatForStep(result)}", ids);
                return new ComputedValue(result, ids);
            }
            catch (OverflowException)
            {
                StopWithError(op, operation, left.Value, right.Value, prefix, ids,
                    "ArithmeticOverflow", "Kết quả phép tính vượt phạm vi decimal.");
                throw new UnreachableException();
            }
        }

        private void AddCalculation(OperationKind operation, decimal? left, decimal right,
            decimal result, string description, HashSet<int> ids) =>
            steps.Add(new EvaluationStep(steps.Count + 1, EvaluationStepType.Calculation,
                operation, left, right, result, description, ids));

        private string FormatForStep(decimal value) =>
            (Math.Round(value, 6, MidpointRounding.AwayFromZero) != value ? "≈" : "")
            + formatter.Format(value);

        private void StopWithError(ParseTreeNode op, OperationKind operation, decimal? left,
            decimal right, string prefix, HashSet<int> ids, string code, string message)
        {
            error = new ExpressionError(ExpressionErrorCategory.Evaluation, code, message, op.Span);
            steps.Add(new EvaluationStep(steps.Count + 1, EvaluationStepType.Error,
                operation, left, right, null, $"{prefix}: {message}", ids, error));
            throw new EvaluationHaltedException();
        }

        private static HashSet<int> Combine(HashSet<int> source, int operatorId,
            HashSet<int>? right = null)
        {
            var ids = new HashSet<int>(source) { operatorId };
            if (right is not null) ids.UnionWith(right);
            return ids;
        }

        private static void Check(ParseTreeNode node, string symbol, int count)
        {
            if (node.Symbol != symbol || node.Children.Count != count)
                throw new InvalidOperationException($"Cây không có nhánh {symbol} hợp lệ.");
        }

        private static void CheckChain(ParseTreeNode node, string symbol, string first, string second)
        {
            string operand = symbol == "E" ? "T" : "F";
            if (node.Symbol != symbol || node.Children.Count is not (1 or 3)
                || node.Children[0].Symbol != operand
                || (node.Children.Count == 3 &&
                    (node.Children[1].Symbol != first && node.Children[1].Symbol != second
                     || node.Children[2].Symbol != symbol)))
                throw new InvalidOperationException($"Cây không có nhánh {symbol} hợp lệ.");
        }
    }
}
