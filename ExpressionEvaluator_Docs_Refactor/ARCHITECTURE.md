# ARCHITECTURE — Một project WPF, Parse Tree E/T/F

## 1. Quyết định tổng quan

Một project `ExpressionEvaluator` (`net10.0-windows`), MVVM qua CommunityToolkit.Mvvm; không tách Core/Tests, không database/persistence/server. Lexer, Recursive Descent Parser và Evaluator là **C# thuần**, không tham chiếu `System.Windows`, ViewModel hoặc Canvas. Lexer → Parser → **một Parse Tree CFG E/T/F** → Evaluator; không tạo AST. Cây bất biến, trace và geometry tách khỏi cây. TreeLayoutService tính geometry cho View; View chỉ vẽ, không tokenize/parse/tính.

## 2. Cấu trúc một project

```text
ExpressionEvaluator/
├── ExpressionEvaluator.csproj
├── App.xaml(.cs)
├── Models/
│   ├── TokenType.cs, Token.cs, SourceSpan.cs, ParseTreeNode.cs
│   ├── OperationKind.cs, EvaluationStepType.cs, EvaluationStep.cs, EvaluationResult.cs
│   ├── ExpressionError.cs, ExpressionErrorCategory.cs, ExpressionAnalysisResult.cs
│   └── TreeNodePosition.cs, TreeEdge.cs, TreeLayoutResult.cs
├── Parsing/GrammarDefinition.cs, Lexer.cs, RecursiveDescentParser.cs
├── Evaluation/ExpressionEvaluator.cs
├── Services/ExpressionService.cs, NumberFormatter.cs, TreeLayoutService.cs,
│            IGrammarDialogService.cs, GrammarDialogService.cs
├── Exceptions/LexicalException.cs, ParseException.cs
├── ViewModels/MainViewModel.cs, GrammarViewModel.cs, ParseTreeNodeViewModel.cs
├── Views/MainWindow.xaml(.cs), GrammarDialog.xaml(.cs),
│         Controls/ParseTreeView.xaml(.cs)
└── Resources/Theme.xaml
```

## 3. Ranh giới trách nhiệm

| Tầng | Nhận / tạo | Không được làm |
|---|---|---|
| `Lexer` | Input → token có span + EOF | Hiểu CFG, thêm nhân ngầm |
| `RecursiveDescentParser` | Token → cây `E/T/F`, mỗi node đúng production | Trả cây dở dang, tạo AST, tính giá trị |
| `ExpressionEvaluator` | Cây → decimal, trace/provenance hoặc Evaluation Error | Sửa cây, dùng rounded output để tính |
| `ExpressionService` | Validate → Lexer → Parser → Evaluator, hợp nhất lỗi | Dính WPF, đổi lỗi lập trình thành Syntax Error |
| `TreeLayoutService` | Cây + kích thước nhãn → tọa độ/cạnh/bounding box | Đọc step, thay đổi Parse Tree |
| `MainViewModel` | Điều phối Submit, stale, step/highlight, dialog abstraction | Vẽ Canvas, tự parse |
| `ParseTreeView` | Layout + root + ID highlight → hình tròn/mũi tên | Tính toán hoặc dựng lại layout khi đổi bước |

Parser: `E/T` có **1 hoặc 3 con**, `F` có **1 (literal), 2 (unary) hoặc 3 (ngoặc) con**; terminal không có con. Chi tiết thứ tự con và FOLLOW set xem `GRAMMAR.md`. Cây phân nhánh phải thể hiện **phép dẫn xuất**, không khẳng định kết quả được tính bằng cách đánh giá subtree `E/T` bên phải. Fold-left phải đi dọc chuỗi cùng mức; provenance ID có thể thuộc nhiều nhánh, xem `EVALUATION.md`.

## 4. Vòng đời Submit và lỗi

1. `CalculateCommand` gửi đúng chuỗi gốc từ TextBox; `ExpressionService` kiểm tra null/blank và `string.Length <= 200` **trước Lexer**.
2. Lexer lỗi → Input/Lexical result không cây/trace; Parser lỗi → Syntax result không cây/trace. Parser chỉ xuất root khi gặp EOF. Evaluator lỗi → **giữ cây**, các step đã thành công và Error step cuối, `Value=null`.
3. `MainViewModel` ghi `LastAnalyzedExpression` dù thất bại; reset `CurrentStepIndex=0`, bỏ layout cũ **trước** khi gắn `CurrentAnalysis` mới, chỉ bố trí geometry **một lần** nếu có cây (thành công hoặc Evaluation Error). Thành công hiện ngay `ResultText`.
4. Khi gõ trước Submit mới: `IsResultOutdated = HasAnalysis && InputExpression != LastAnalyzedExpression`, vẫn giữ toàn bộ dữ liệu cũ và cho phép duyệt bước cũ. Khi input khớp lại chuỗi đã Submit thì hết stale, không tự động parse.
5. Next/Previous chỉ thay index 0..N và tập ID highlight, thông báo `CanExecute`; không thay cây, layout, kết quả. Error text Evaluation hiện ngay từ lúc Submit, không chờ Next.

`GrammarDialogService` là adapter WPF duy nhất để ViewModel mở dialog chỉ đọc, không cần DI framework. Code-behind chỉ xử lý focus, Enter gửi **cùng CalculateCommand**, chọn span lỗi/caret EOF sau Submit; không có logic Lexer/Parser/Evaluator.

## 5. Geometry và tính nhất quán

`TreeLayoutService` tính đủ mọi non-terminal/terminal, không lọc node hợp lệ nào: CFG mới **không có ε**. Mỗi nhãn được chứa trọn trong **hình tròn có đường kính động**; khoảng cách ngang theo bề rộng từng subtree, khoảng cách dọc theo đường kính lớn nhất của từng tầng. Các node ở cùng tầng không chồng nhau; các cạnh/mũi tên kết thúc ở rìa hình tròn, không đâm xuyên chữ. Canvas có Width/Height theo bounding box cả đầu mũi tên; ScrollViewer cho cuộn hai chiều. Gốc `E` có viền xanh; step highlight của toán tử/nguồn là trạng thái riêng, không đè mất dấu hiệu gốc. Khi đổi step chỉ cập nhật brush/border của node VM, **không sắp xếp lại cây**. Quy tắc đo/nhãn xem `UI_DESIGN.md`.

## 6. Kiểm thử và bảo trì

Chỉ kiểm thử **thủ công qua app WPF** theo `TEST_PLAN.md`: EV (giá trị và thứ tự), ER (vị trí và không có cây), EE (cây và Error step), UI (nhánh E/T/F, hình tròn/mũi tên, stale, điều hướng). Không test project/CI. Khi đổi CFG phải cập nhật đồng bộ `GrammarDefinition`, parser, evaluator, renderer, dialog và test plan; không chỉ thay XAML. Mã nguồn hiện theo văn phạm cũ phải được **di trú** trước khi tuyên bố đã triển khai kiến trúc E/T/F.
