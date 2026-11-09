namespace ExpressionEvaluator.Parsing;

/// <summary>Văn phạm cố định của RecursiveDescentParser; chỉ phục vụ trình bày read-only.</summary>
public static class GrammarDefinition
{
    public const string Text =
        "E -> T + E | T - E | T\n" +
        "T -> F * T | F / T | F\n" +
        "F -> + F | - F | ( E ) | NUMBER\n\n" +
        "E: biểu thức cộng/trừ; T: nhân/chia; F: số, dấu ngoặc và toán tử một ngôi.\n" +
        "NUMBER: [0-9]+ (ASCII). Ký hiệu bắt đầu: E. Kết thúc: EndOfInput.\n\n" +
        "Cây thể hiện luật sinh phân nhánh phải, không phải AST hay thứ tự nhóm phép tính.\n" +
        "Các phép cùng mức được tính từ trái sang phải: 10-3-2 = (10-3)-2 = 5.";
}
