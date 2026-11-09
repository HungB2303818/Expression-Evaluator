using System.Globalization;
using ExpressionEvaluator.Exceptions;
using ExpressionEvaluator.Models;

namespace ExpressionEvaluator.Parsing;

/// <summary>Quét token theo ký tự UTF-16 của chuỗi nguồn; chỉ chữ số ASCII là literal.</summary>
public sealed class Lexer
{
    public IReadOnlyList<Token> Tokenize(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var tokens = new List<Token>();
        int index = 0;
        while (index < expression.Length)
        {
            char current = expression[index];
            if (char.IsWhiteSpace(current))
            {
                index++;
                continue;
            }

            if (current is >= '0' and <= '9')
            {
                int start = index;
                do
                {
                    index++;
                }
                while (index < expression.Length && expression[index] is >= '0' and <= '9');

                string lexeme = expression[start..index];
                var span = new SourceSpan(start, index - start);
                if (!decimal.TryParse(lexeme, NumberStyles.None, CultureInfo.InvariantCulture, out decimal value))
                    throw new LexicalException(new ExpressionError(
                        ExpressionErrorCategory.Lexical, "NumberOutOfRange",
                        $"Số nguyên '{lexeme}' vượt phạm vi decimal.", span));

                tokens.Add(new Token(TokenType.Number, lexeme, span, value));
                continue;
            }

            TokenType type = current switch
            {
                '+' => TokenType.Plus,
                '-' => TokenType.Minus,
                '*' => TokenType.Multiply,
                '/' => TokenType.Divide,
                '(' => TokenType.LeftParen,
                ')' => TokenType.RightParen,
                _ => TokenType.EndOfInput
            };

            if (type == TokenType.EndOfInput)
                throw new LexicalException(new ExpressionError(
                    ExpressionErrorCategory.Lexical, "InvalidCharacter",
                    $"Ký tự '{current}' không hợp lệ trong biểu thức.", new SourceSpan(index, 1)));

            tokens.Add(new Token(type, expression[index].ToString(), new SourceSpan(index, 1)));
            index++;
        }

        tokens.Add(new Token(TokenType.EndOfInput, string.Empty, new SourceSpan(expression.Length, 0)));
        return Array.AsReadOnly(tokens.ToArray());
    }
}
