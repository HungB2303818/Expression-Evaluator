using ExpressionEvaluator.Parsing;

namespace ExpressionEvaluator.ViewModels;

/// <summary>Dữ liệu chỉ đọc; không cung cấp setter để sửa văn phạm.</summary>
public sealed class GrammarViewModel
{
    public string GrammarText => GrammarDefinition.Text;
}
