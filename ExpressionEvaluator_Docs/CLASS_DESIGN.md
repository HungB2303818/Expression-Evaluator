# CLASS_DESIGN — Mô hình dữ liệu và hợp đồng lớp C#

> Các đoạn mã ở đây là **hợp đồng thiết kế/skeleton**, không phải mã nguồn đã triển khai hoàn chỉnh. Cần hoàn thiện constructor, kiểm tra bất biến và xử lý nullable khi lập trình.

## 1. Các model độc lập WPF

### 1.1. Token và SourceSpan

```csharp
public enum TokenType
{
    Number, Plus, Minus, Multiply, Divide,
    LeftParen, RightParen, EndOfInput
}

// StartIndex zero-based; Length có thể bằng 0 tại EOF hoặc epsilon.
public readonly record struct SourceSpan(int StartIndex, int Length)
{
    public int EndExclusive => StartIndex + Length;
    public int DisplayPosition => StartIndex + 1;
}

public sealed record Token(
    TokenType Type,
    string Lexeme,
    SourceSpan Span,
    decimal? NumericValue = null);
```

Bất biến: `Number` có `NumericValue != null`; các token toán tử/ngoặc/EOF không có giá trị. Toán tử unary/binary dùng cùng `TokenType` cho `+`/`-` và được xác định theo vị trí trong CFG.

### 1.2. ParseTreeNode — không chứa giá trị tính toán

```csharp
public sealed record ParseTreeNode(
    int Id,
    string Symbol,
    string? Lexeme,
    SourceSpan Span,
    IReadOnlyList<ParseTreeNode> Children,
    bool IsEpsilon = false);
```

- `Symbol`: tên non-terminal hoặc terminal. `Lexeme`: ký hiệu thực tế với terminal.
- `Id` đánh số tăng dần trong một lần Parse, không cần ổn định giữa các lần Calculate.
- Dữ liệu node được chốt khi Parser tạo xong; Evaluator chỉ đọc.
- Nút `ε` có thể được lưu trong cây nội bộ, nhưng view luôn ẩn.
- Không có thuộc tính `CalculatedValue` và không tạo AST.

### 1.3. EvaluationStep và EvaluationResult

```csharp
public enum OperationKind
{
    UnaryPlus, UnaryMinus,
    Add, Subtract, Multiply, Divide
}

public enum EvaluationStepType { Calculation, Error }

public sealed record EvaluationStep(
    int StepNumber,
    EvaluationStepType Type,
    OperationKind Operation,
    decimal? LeftOperand,
    decimal RightOperand,
    decimal? Result,
    string Description,
    IReadOnlySet<int> HighlightedNodeIds,
    ExpressionError? Error = null);

public sealed record EvaluationResult(
    decimal? Value,
    IReadOnlyList<EvaluationStep> Steps,
    ExpressionError? Error)
{
    public bool IsSuccess => Error is null && Value.HasValue;
}
```

Quy ước:

- Unary: `LeftOperand = null`; `RightOperand` là giá trị toán hạng đã tính.
- Binary: cả `LeftOperand` và `RightOperand` có giá trị.
- Thành công: `Type=Calculation`, `Result` có giá trị, `Error=null`.
- Lỗi đánh giá: `Type=Error`, `Result=null`, `Error!=null`, **là step cuối cùng**.
- Bước thứ nhất đánh số 1; trạng thái UI trước bước đầu là `CurrentStepIndex=0`.
- Không có bước cho literal đơn độc hay dấu ngoặc.
- `HighlightedNodeIds` chứa ID của toán tử và node nguồn tham gia phép tính; không phụ thuộc số lượng bước.

### 1.4. Error và kết quả phân tích tổng hợp

```csharp
public enum ExpressionErrorCategory
{
    Input, Lexical, Syntax, Evaluation
}

public sealed record ExpressionError(
    ExpressionErrorCategory Category,
    string Code,
    string Message,
    SourceSpan? Span);

public sealed record ExpressionAnalysisResult(
    ParseTreeNode? ParseTree,
    EvaluationResult? Evaluation,
    ExpressionError? Error)
{
    public bool IsSuccess => Error is null && Evaluation?.IsSuccess == true;
}
```

**Bất biến ở ranh giới `ExpressionService`:**

- Input/Lexical/Syntax failure → `ParseTree=null`, `Evaluation=null`, `Error!=null`.
- Evaluation failure → `ParseTree!=null`, `Evaluation!=null`, `Evaluation.Value=null`, `Evaluation.Error!=null`, và `Error` cấp ngoài tham chiếu cùng lỗi đó.
- Success → `ParseTree!=null`, `Evaluation.Value!=null`, `Error=null`.

### 1.5. Geometry model cho WPF

```csharp
public sealed record TreeNodePosition(int NodeId, double X, double Y);
public sealed record TreeEdge(int ParentId, int ChildId);
public sealed record TreeLayoutResult(
    double Width,
    double Height,
    IReadOnlyList<TreeNodePosition> Positions,
    IReadOnlyList<TreeEdge> Edges);
```

Không trộn tọa độ vào `ParseTreeNode`: tree là dữ liệu cú pháp, geometry là dữ liệu trình bày.

## 2. Parsing classes

### `Lexer`

```csharp
public sealed class Lexer
{
    public IReadOnlyList<Token> Tokenize(string expression);
}
```

- Bỏ qua `char.IsWhiteSpace`.
- Chỉ nhận `[0-9]+`, parse bằng `decimal.TryParse` + `CultureInfo.InvariantCulture` + số nguyên không dấu. Gặp literal quá lớn → lỗi `NumberOutOfRange` ngay vị trí token.
- Chuyển tất cả dấu + - * / ( ) thành token tương ứng.
- Ký tự lạ → `LexicalException` (với `ExpressionError`).
- Thêm EOF ở `expression.Length`, `Length=0`.
- Không tự chèn phép nhân ngầm.

### `RecursiveDescentParser`

```csharp
public sealed class RecursiveDescentParser
{
    public ParseTreeNode Parse(IReadOnlyList<Token> tokens);
    // ParseExpr, ParseExprTail, ParseTerm,
    // ParseTermTail, ParseUnary, ParsePrimary
}
```

- Mỗi phương thức tạo node đúng grammar; thứ tự con theo production.
- `Match`/`Expect` phát sinh `ParseException` kèm token/EOF span.
- Luôn yêu cầu kết thúc tại `EndOfInput`.
- `Id` cấp tự động khi tạo các node trong lần Parse, không sửa ID về sau.
- `ExprTail`, `TermTail` phải kiểm tra lookahead theo FOLLOW hợp lệ, không bỏ qua ký tự sai.

### `GrammarDefinition`

- Nguồn dữ liệu read-only (hằng chuỗi hoặc danh sách quy tắc bất biến) dùng cho Grammar Dialog.
- Nội dung phải khớp chính xác với các nhánh hàm Parser; không cung cấp chức năng sửa.

## 3. Evaluation classes

### `ExpressionEvaluator`

```csharp
public sealed class ExpressionEvaluator
{
    public EvaluationResult Evaluate(ParseTreeNode root);

    // Nội bộ: EvaluateExpr, EvaluateTerm,
    // EvaluateUnary, EvaluatePrimary,
    // FoldExprTail, FoldTermTail
}
```

Evaluator có thể dùng một `EvaluationContext` **nội bộ** (không buộc xuất thành file) để chứa `List<EvaluationStep>`, ID nguồn toán hạng và trạng thái lỗi. Mỗi hàm trả **giá trị decimal + tập node provenance**, không phải chỉ giá trị. Nếu phát hiện lỗi, thêm step lỗi cuối và trả `EvaluationResult(Value=null)`.

### `NumberFormatter`

```csharp
public sealed class NumberFormatter
{
    public string Format(decimal value);
}
```

- `Math.Round(value, 6, MidpointRounding.AwayFromZero)`.
- Format invariant với dấu `.` thập phân và `0.######` để bỏ 0 dư (dấu phân cách chuẩn hóa cho ví dụ, không phụ thuộc culture hệ thống).
- Làm tròn **chỉ khi hiển thị**, không sửa giá trị nội bộ.
- Có thể chuyển sang dấu thập phân theo locale UI ở tương lai, nhưng **ngoài phạm vi hiện tại**.

## 4. Application services

### `ExpressionService`

```csharp
public sealed class ExpressionService
{
    public ExpressionAnalysisResult Analyze(string expression);
}
```

Luồng:

```text
ValidateInput (null/blank, Length <= 200)
    ↓
Lexer.Tokenize
    ↓
RecursiveDescentParser.Parse
    ↓
ExpressionEvaluator.Evaluate
    ↓
ExpressionAnalysisResult
```

`Analyze` bắt riêng `LexicalException`, `ParseException`, chuyển sang `ExpressionAnalysisResult`. Evaluation Error là kết quả bình thường có `Error`, không được ném bỏ trace. Kiểm tra input dùng chính `string.Length`, không dùng độ dài sau `Trim()`.

### `TreeLayoutService`

```csharp
public sealed class TreeLayoutService
{
    public TreeLayoutResult Arrange(ParseTreeNode root);
}
```

- Bỏ qua nút `IsEpsilon` khi dựng layout.
- Dùng khoảng cách ngang/dọc cố định theo kích thước nhãn; bảo đảm không chồng nút.
- Node con theo thứ tự `Children`; kết nối các ID tương ứng.
- Trả Width/Height để Canvas bên trong ScrollViewer dùng được scrollbar.

### `GrammarDialogService`

- Có thể dùng `IGrammarDialogService.ShowGrammar()` như abstraction đơn giản cho ViewModel, với một implementation duy nhất trong cùng project. **Không** xây thêm framework DI/navigation.

## 5. ViewModels

### `MainViewModel` — State

| Property | Loại gợi ý | Ý nghĩa |
|---|---|---|
| `InputExpression` | `string` | Nội dung TextBox hiện tại |
| `LastAnalyzedExpression` | `string?` | Chuỗi của lần Calculate gần nhất |
| `HasAnalysis` | `bool` | Có lần Calculate trước đó |
| `CurrentAnalysis` | `ExpressionAnalysisResult?` | Kết quả Submit gần nhất |
| `IsResultOutdated` | computed `bool` | `HasAnalysis && InputExpression != LastAnalyzedExpression` |
| `CurrentStepIndex` | `int` | 0 trước bước; 1..N là các bước |
| `Steps` | `IReadOnlyList<EvaluationStep>` | Danh sách đã tính sẵn |
| `HighlightedNodeIds` | `IReadOnlySet<int>` | Tập ID highlight của bước hiện tại |
| `ResultText` | `string` | Kết quả được format hoặc rỗng nếu lỗi |
| `ErrorText` | `string?` | Lỗi tiếng Việt (nếu có) |
| `TreeLayout` | `TreeLayoutResult?` | Geometry cho cây của Submit hiện tại |

**Command:** `CalculateCommand`, `NextStepCommand`, `PreviousStepCommand`, `ViewGrammarCommand`.

Quy ước `CurrentStepIndex=0`: không highlight, nhưng **ResultText vẫn hiển thị ngay** sau Calculate. Với biểu thức không có toán tử (`42`), `Steps.Count=0`, cả hai nút điều hướng bị vô hiệu.

Ví dụ sử dụng CommunityToolkit.Mvvm:

```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsResultOutdated))]
    private string inputExpression = string.Empty;

    public bool IsResultOutdated =>
        HasAnalysis && InputExpression != LastAnalyzedExpression;

    [RelayCommand]
    private void NextStep() { /* change index only */ }
}
```

Khi `HasAnalysis` hoặc `LastAnalyzedExpression` đổi, phải thông báo `IsResultOutdated` thay đổi. Khi index/steps đổi, phải thông báo command thay đổi `CanExecute`.

### `GrammarViewModel`

Chứa text grammar cố định và phần chú giải; không expose setter sửa production.

### `ParseTreeNodeViewModel`

Chứa `NodeId`, `Label`, `X`, `Y`, `IsHighlighted` observable; **không chứa logic tính toán**. Có thể tạo một map `NodeId → ParseTreeNodeViewModel` để cập nhật highlight hiệu quả.

## 6. API semantics quan trọng

1. Không dùng `double` để tính hoặc để lưu operand trong step.
2. Không tính trực tiếp `ExprTail` theo kiểu gộp từ phải sang trái.
3. Không tham chiếu `Canvas`, `TextBox`, `Brush` ở `Models/Parsing/Evaluation`.
4. Không sửa Parse Tree khi Next/Previous.
5. Không tạo Step cho number/ngoặc; có Step cho **tất cả** unary và binary operators.
6. Không làm tròn `decimal` trong Evaluation Engine.
7. Không mất trace khi chia cho 0/tràn số.

Chi tiết pseudocode của thuật toán: [`EVALUATION.md`](EVALUATION.md).
