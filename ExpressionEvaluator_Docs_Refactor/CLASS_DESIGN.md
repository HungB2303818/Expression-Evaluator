# CLASS_DESIGN — Hợp đồng dữ liệu và lớp C# cho E/T/F

Các đoạn sau là **hợp đồng thiết kế**, không phải source đã triển khai. Constructor phải kiểm tra bất biến, sao chép collection có thể thay đổi và công bố dạng read-only; `Models`, `Parsing`, `Evaluation` **không phụ thuộc WPF**.

## 1. Dữ liệu và lỗi

```csharp
public enum TokenType { Number, Plus, Minus, Multiply, Divide,
                        LeftParen, RightParen, EndOfInput }
public readonly record struct SourceSpan(int StartIndex, int Length)
{
    public int EndExclusive => StartIndex + Length;
    public int DisplayPosition => StartIndex + 1;
}
public sealed record Token(TokenType Type, string Lexeme, SourceSpan Span,
                           decimal? NumericValue = null);

public sealed record ParseTreeNode(int Id, string Symbol, string? Lexeme,
    SourceSpan Span, IReadOnlyList<ParseTreeNode> Children);
```

`SourceSpan` zero-based, không âm, `EndExclusive` không overflow; EOF có `Length=0` và vị trí hiển thị `expression.Length+1`. Chỉ `Number` có `NumericValue` khác null, các token khác không có; EOF có lexeme rỗng. Trong cây: non-terminal chỉ `E`, `T`, `F` (`Lexeme=null`); terminal `NUMBER/PLUS/MINUS/MULTIPLY/DIVIDE/LEFT_PAREN/RIGHT_PAREN` có lexeme gốc và không có con. **Không có node ε và không có thuộc tính `IsEpsilon`.** ID duy nhất trong lần Parse, không cần ổn định giữa các lần Submit. `E`/`T` có 1 hoặc 3 con; `F` có 1 (`NUMBER`), 2 (unary) hoặc 3 (ngoặc); terminal 0 con. Cây bất biến, không có `CalculatedValue` hay tọa độ.

```csharp
public enum ExpressionErrorCategory { Input, Lexical, Syntax, Evaluation }
public sealed record ExpressionError(ExpressionErrorCategory Category,
                                     string Code, string Message, SourceSpan? Span);
public enum OperationKind { UnaryPlus, UnaryMinus, Add, Subtract, Multiply, Divide }
public enum EvaluationStepType { Calculation, Error }
public sealed record EvaluationStep(int StepNumber, EvaluationStepType Type,
    OperationKind Operation, decimal? LeftOperand, decimal RightOperand,
    decimal? Result, string Description, IReadOnlySet<int> HighlightedNodeIds,
    ExpressionError? Error = null);
public sealed record EvaluationResult(decimal? Value,
    IReadOnlyList<EvaluationStep> Steps, ExpressionError? Error)
{
    public bool IsSuccess => Error is null && Value.HasValue;
}
public sealed record ExpressionAnalysisResult(ParseTreeNode? ParseTree,
    EvaluationResult? Evaluation, ExpressionError? Error)
{
    public bool IsSuccess => Error is null && Evaluation?.IsSuccess == true;
}
```

`StepNumber` từ 1, liên tục; unary có `LeftOperand=null`, binary có giá trị trái. Calculation có `Result` và không `Error`; Error step có `Result=null`, `Error.Category=Evaluation` và là step cuối. `HighlightedNodeIds` chỉ gồm ID terminal hiện có trong cây. Trace/list/tập ID là dữ liệu đã chốt, không để caller sửa được. Không tạo step cho literal/dấu ngoặc; `RightOperand` luôn chứa giá trị đã tính (kể cả bằng 0 khi lỗi chia 0).

**Ranh giới trạng thái `ExpressionService`:**

| Nhánh | ParseTree | Evaluation | Error cấp ngoài |
|---|---|---|---|
| Input/Lexical/Syntax failure | null | null | thuộc nhóm tương ứng, `Span` có thể null với Input |
| Evaluation failure | có | có, `Value=null`, Error step cuối | **cùng đối tượng** với `Evaluation.Error` và lỗi của step cuối |
| Success | có | có `Value`, `IsSuccess=true` | null |

`LexicalException` và `ParseException` mang `ExpressionError` thuộc category tương ứng; lỗi Evaluation là kết quả có trace, không ném exception ra UI. `Code` tối thiểu gồm `EmptyExpression`, `InputTooLong`, `InvalidCharacter`, `NumberOutOfRange`, `ExpectedOperand`, `MissingOperator`, `MissingRightParen`, `UnexpectedRightParen`, `DivisionByZero`, `ArithmeticOverflow`. Message tiếng Việt và span theo `GRAMMAR.md`.

## 2. Geometry tách khỏi Parse Tree

```csharp
public sealed record TreeNodePosition(int NodeId, double X, double Y);
public sealed record TreeEdge(int ParentId, int ChildId);
public sealed record TreeLayoutResult(double Width, double Height,
    IReadOnlyList<TreeNodePosition> Positions, IReadOnlyList<TreeEdge> Edges);
```

`X,Y` là góc trên-trái bounding box **hình tròn**. `TreeLayoutService.GetDiameter(ParseTreeNode node)` dùng cùng một quy tắc đo nhãn với renderer (đo text có padding), nên đường kính được lấy lại từ node, không phải giá trị tính toán. Canvas đủ kích thước chứa toàn bộ hình tròn và đầu mũi tên; không trùng ID, vị trí hữu hạn. Nếu đổi font/độ phóng đại hệ thống thì đo/lập layout **một lần cho lần Submit đó** bằng cùng thông số mà View dùng, không layout lại khi Next/Previous. Không cắt literal dài để giả có đường kính cố định.

## 3. Hợp đồng các tầng

| Lớp | API/Trách nhiệm | Không làm |
|---|---|---|
| `Lexer` | `IReadOnlyList<Token> Tokenize(string expression)`; EOF, span, invariant decimal | Parse ngữ pháp, nhân ngầm |
| `RecursiveDescentParser` | `Parse(IReadOnlyList<Token>)` → root `E`; `ParseE/T/F`, FOLLOW, EOF | AST, giá trị tính toán |
| `GrammarDefinition` | `Text` read-only đúng **ba production E/T/F** và chú giải cây dẫn xuất | Cho phép sửa CFG |
| `ExpressionEvaluator` | `EvaluationResult Evaluate(ParseTreeNode root)`; fold-left và provenance, Error step | Làm tròn giữa các phép tính, sửa cây |
| `NumberFormatter` | `string Format(decimal value)` theo `0.######`, AwayFromZero | Dùng output để tính tiếp |
| `ExpressionService` | `ExpressionAnalysisResult Analyze(string? expression)`; validate **trước Lexer** → parse → evaluate | Nuốt lỗi lập trình thành lỗi cú pháp |
| `TreeLayoutService` | `TreeLayoutResult Arrange(ParseTreeNode root)`; đo node tròn, bbox/cạnh | Đọc step, WPF controls |
| `IGrammarDialogService`/`GrammarDialogService` | `ShowGrammar()` qua dialog read-only | Cấu hình lại CFG |

`MainViewModel : ObservableObject` dùng `[ObservableProperty]`/`[RelayCommand]`. Lưu `InputExpression`, `LastAnalyzedExpression`, `CurrentAnalysis`, `CurrentStepIndex`, `TreeLayout`. Tính `HasAnalysis`, `IsResultOutdated`, `Steps`, `HighlightedNodeIds`, `ResultText`, `ErrorText`; command `Calculate`, `NextStep`, `PreviousStep`, `ViewGrammar`. Next/Previous chỉ đổi index và highlight, thông báo `CanExecute`; `ResultText` không đổi. Submit mới reset index 0 và thay state dù lỗi; khi thay cây, **clear layout cũ trước khi gắn cây mới** để View không ghép nhầm geometry. `ParseTreeNodeViewModel` giữ nhãn/ID/vị trí/highlight, không tính toán. `GrammarViewModel` chỉ có grammar read-only. Code-behind chỉ nhận Enter, focus/chọn lỗi/khởi tạo view.
