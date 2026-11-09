using ExpressionEvaluator.Exceptions;
using ExpressionEvaluator.Models;

namespace ExpressionEvaluator.Parsing;

/// <summary>Dựng cây dẫn xuất E/T/F; phân tích tiền tố chung trước khi chọn production.</summary>
public sealed class RecursiveDescentParser
{
    public ParseTreeNode Parse(IReadOnlyList<Token> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        if (tokens.Count == 0 || tokens[^1] is null || tokens[^1].Type != TokenType.EndOfInput
            || tokens.Take(tokens.Count - 1).Any(token => token is null || token.Type == TokenType.EndOfInput))
            throw new ArgumentException("Danh sách token phải kết thúc bằng đúng một EndOfInput.", nameof(tokens));

        return new ParseSession(tokens).Parse();
    }

    private sealed class ParseSession(IReadOnlyList<Token> tokens)
    {
        private int index;
        private int nextId;
        private Token Current => tokens[index];

        public ParseTreeNode Parse()
        {
            ParseTreeNode root = ParseE();
            if (Current.Type != TokenType.EndOfInput)
                throw Current.Type == TokenType.RightParen
                    ? Error("UnexpectedRightParen", "Dấu ')' thừa.")
                    : Error("UnexpectedToken", "Ký hiệu không mong đợi sau biểu thức.");

            return root;
        }

        private ParseTreeNode ParseE()
        {
            ParseTreeNode left = ParseT();
            if (Current.Type is TokenType.Plus or TokenType.Minus)
            {
                ParseTreeNode op = Consume();
                return Branch("E", left, op, ParseE());
            }

            if (Current.Type is TokenType.RightParen or TokenType.EndOfInput)
                return Branch("E", left);

            throw UnexpectedAfterOperand();
        }

        private ParseTreeNode ParseT()
        {
            ParseTreeNode left = ParseF();
            if (Current.Type is TokenType.Multiply or TokenType.Divide)
            {
                ParseTreeNode op = Consume();
                return Branch("T", left, op, ParseT());
            }

            if (Current.Type is TokenType.Plus or TokenType.Minus
                or TokenType.RightParen or TokenType.EndOfInput)
                return Branch("T", left);

            throw UnexpectedAfterOperand();
        }

        private ParseTreeNode ParseF()
        {
            if (Current.Type is TokenType.Plus or TokenType.Minus)
            {
                ParseTreeNode op = Consume();
                return Branch("F", op, ParseF());
            }

            if (Current.Type == TokenType.Number)
                return Branch("F", Consume());

            if (Current.Type == TokenType.LeftParen)
            {
                ParseTreeNode left = Consume();
                ParseTreeNode inner = ParseE();
                if (Current.Type != TokenType.RightParen)
                    throw Error("MissingRightParen", "Thiếu dấu ')' đóng ngoặc.");

                return Branch("F", left, inner, Consume());
            }

            throw Error("ExpectedOperand", Current.Type == TokenType.EndOfInput
                ? "Thiếu toán hạng tại cuối biểu thức."
                : "Thiếu toán hạng: cần số nguyên hoặc dấu '('.");
        }

        private ParseTreeNode Consume()
        {
            Token token = Current;
            index++;
            return new ParseTreeNode(nextId++, TerminalSymbol(token.Type), token.Lexeme,
                token.Span, []);
        }

        private ParseTreeNode Branch(string symbol, params ParseTreeNode[] children) =>
            new(nextId++, symbol, null,
                new SourceSpan(children[0].Span.StartIndex,
                    children[^1].Span.EndExclusive - children[0].Span.StartIndex), children);

        private ParseException UnexpectedAfterOperand() =>
            Error("MissingOperator", Current.Type is TokenType.LeftParen or TokenType.Number
                ? "Thiếu toán tử giữa hai toán hạng (phép nhân phải dùng '*')."
                : "Ký hiệu không mong đợi sau toán hạng.");

        private ParseException Error(string code, string message) =>
            new(new ExpressionError(ExpressionErrorCategory.Syntax, code, message, Current.Span));

        private static string TerminalSymbol(TokenType type) => type switch
        {
            TokenType.Number => "NUMBER",
            TokenType.Plus => "PLUS",
            TokenType.Minus => "MINUS",
            TokenType.Multiply => "MULTIPLY",
            TokenType.Divide => "DIVIDE",
            TokenType.LeftParen => "LEFT_PAREN",
            TokenType.RightParen => "RIGHT_PAREN",
            _ => throw new ArgumentOutOfRangeException(nameof(type), "EOF không thuộc Parse Tree.")
        };
    }
}
