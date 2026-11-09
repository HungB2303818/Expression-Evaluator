# IMPLEMENTATION_PLAN — Kế hoạch triển khai theo giai đoạn

## 1. Phạm vi công việc

Triển khai **một project WPF** (.NET 10, C#, CommunityToolkit.Mvvm), grammar cố định, Recursive Descent Parser tự viết, đánh giá trực tiếp trên Parse Tree và giao diện trực quan hóa từng phép tính. Không xây Unit Test tự động, không dùng ANTLR/AST, không lưu lịch sử.

Ưu tiên làm **đúng logic lõi trước, rồi ghép UI**, dù toàn bộ nằm trong cùng project. Mỗi giai đoạn có tiêu chí nghiệm thu qua kiểm tra thủ công.

## 2. Giai đoạn 0 — Khởi tạo nền tảng

**Công việc**

- Cài SDK .NET 10, chuẩn bị IDE trên Windows.
- Tạo một WPF project `ExpressionEvaluator` target `net10.0-windows`.
- Cài package `CommunityToolkit.Mvvm`.
- Tạo thư mục theo `ARCHITECTURE.md`; cấu hình `App.xaml`, resource theme sáng.

**Hoàn thành khi** ứng dụng WPF build/chạy được, có cửa sổ trắng cơ bản và package MVVM hoạt động.

## 3. Giai đoạn 1 — Models và quy ước lỗi

**Tạo** `SourceSpan`, `TokenType`, `Token`, `ParseTreeNode`, `ExpressionError`, `ExpressionAnalysisResult`, `EvaluationStep`, `EvaluationResult`, enums, `TreeLayoutResult`.

**Yêu cầu chất lượng**

- Sử dụng `decimal`, `IReadOnlyList`, cấu trúc dữ liệu bất biến ở ranh giới Parser/Evaluator.
- 0-based ở model, 1-based khi hiển thị.
- Xác định rõ trạng thái Success / Input Failure / Parse Failure / Evaluation Failure.

**Hoàn thành khi** các class đúng hợp đồng `CLASS_DESIGN.md`, project biên dịch.

## 4. Giai đoạn 2 — Lexer

**Công việc**

- Quét `[0-9]+`, toán tử, ngoặc và whitespace; tạo EOF.
- Ghi `SourceSpan`, parse số sang `decimal` bằng invariant culture.
- Bắt lỗi ký tự lạ và literal ngoài phạm vi.

**Kiểm thử thủ công sớm:** Dùng UI debug đơn giản hoặc breakpoint tại `ExpressionService`; nhập các biểu thức EV-01, ER-02, ER-03, ER-14, không tạo project test.

**Hoàn thành khi** token và vị trí đúng; không chấp nhận số thập phân hoặc implicit multiplication ở cấp lexer.

## 5. Giai đoạn 3 — Grammar và Recursive Descent Parser

**Công việc**

- Viết `GrammarDefinition` khớp `GRAMMAR.md`.
- Cài `ParseExpr`, `ParseExprTail`, `ParseTerm`, `ParseTermTail`, `ParseUnary`, `ParsePrimary`.
- Kiểm tra FOLLOW set khi chọn `ε`; consume hết EOF.
- Tạo đủ terminal/non-terminal và các node epsilon (nếu lưu nội bộ).
- Bắt `ParseException` chứa vị trí và message tiếng Việt tương ứng.

**Hoàn thành khi** `3+5*2`, `--5`, `-(2+3)` có cây đúng; `3+*5`, `2(3+4)`, `(2+3` được nhận diện lỗi; **không trả cây thiếu**.

## 6. Giai đoạn 4 — Evaluator + Trace

**Công việc**

- Viết `EvaluateExpr/Term/Unary/Primary`, fold-left các tail.
- Duy trì `ComputedValue(Value, ProvenanceNodeIds)` nội bộ.
- Với mỗi toán tử unary/binary, sinh một `EvaluationStep`, gắn `HighlightedNodeIds`.
- Bắt division by zero/overflow thành **Error step cuối**, giữ trace.
- Viết `NumberFormatter` làm tròn 6 số và bỏ 0 cuối.

**Kiểm thử thủ công bắt buộc:** EV-03/04/06/11/13/15, EE-01/03, số bước và kết quả như `TEST_PLAN.md`.

**Hoàn thành khi** không có rounding giữa các phép tính; trace theo thứ tự; phép trừ/chia kết hợp trái; giá trị lỗi không được trình bày là thành công.

## 7. Giai đoạn 5 — ExpressionService / điều phối

**Công việc**

- `Analyze(expression)` kiểm tra blank và giới hạn `string.Length <= 200` **trước Lexer**.
- Chạy Lexer → Parser → Evaluator, trả `ExpressionAnalysisResult` thống nhất.
- Chuyển Lexical/Syntax exceptions sang lỗi trả về.
- Khi Evaluation Error, giữ root và result failure.

**Hoàn thành khi** mọi nhánh Input/Lexical/Syntax/Evaluation/Success trả dữ liệu đúng bất biến trong `CLASS_DESIGN.md`.

## 8. Giai đoạn 6 — MainViewModel và step navigation

**Công việc**

- Cài `MainViewModel : ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`.
- Lưu `InputExpression`, `LastAnalyzedExpression`, `CurrentAnalysis`, stale flag và index.
- `CalculateCommand` dùng `ExpressionService.Analyze`, thay kết quả cũ kể cả khi lỗi, reset index=0.
- `NextStepCommand`/`PreviousStepCommand` chỉ thay index + highlighted ID.
- `ViewGrammarCommand` mở dialog read-only.
- Đồng bộ notify properties và `CanExecute`.

**Hoàn thành khi** Next/Previous hoạt động trên dữ liệu có sẵn; sửa input vẫn giữ kết quả/cây trước Submit; Submit mới thay state.

## 9. Giai đoạn 7 — ParseTree layout & rendering

**Công việc**

- `TreeLayoutService`: bố trí tất cả node terminal/non-terminal **trừ `ε`**; không chồng các node.
- Trả geometry và kích thước Canvas.
- `ParseTreeView` = ScrollViewer + Canvas; vẽ nút/cạnh; tách highlight khỏi geometry.
- Liên kết `HighlightedNodeIds` với `IsHighlighted` trên node VM.

**Hoàn thành khi** cây có thể cuộn ở hai chiều, đủ non-terminal, không hiển thị epsilon, highlight chuyển đúng step mà không layout lại.

## 10. Giai đoạn 8 — Hoàn thiện WPF UI

**Công việc**

- Dựng MainWindow hai cột: vùng trên Input/Result; cây trái; bước phải.
- UI tiếng Việt, theme sáng, trạng thái disabled, thông báo lỗi, cảnh báo stale.
- Enter dùng `CalculateCommand` giống click; không parse khi gõ.
- Grammar Dialog read-only đúng production.
- Chọn đoạn có lỗi trong TextBox hoặc đặt caret EOF nếu có vị trí.

**Hoàn thành khi** đáp ứng toàn bộ `UI_DESIGN.md`; không có tính năng lịch sử/zoom/pan/AST.

## 11. Giai đoạn 9 — Kiểm thử và hoàn thiện

**Thực hiện:** `TEST_PLAN.md` theo thứ tự EV → ER → EE → UI; ghi Pass/Fail, sửa và regression test thủ công.  
**Không làm:** viết test framework/CI chạy test tự động hoặc thêm một project khác.

**Nghiệm thu:** mọi test case đã ghi là bắt buộc đạt; công cụ chạy ổn với biểu thức <=200 ký tự; không crash trước các lỗi dự kiến.

## 12. Checklist theo thứ tự ưu tiên

- [ ] Tạo WPF project + cài CommunityToolkit.Mvvm.
- [ ] Hoàn thiện models/enums/SourceSpan.
- [ ] Lexer đọc đúng token, vị trí và EOF.
- [ ] Parser dựng cây CFG đầy đủ và bắt sai cú pháp.
- [ ] Evaluator fold-left, unary steps, trace và lỗi.
- [ ] ExpressionService đóng gói flow, ranh giới lỗi.
- [ ] MainViewModel state, stale, Calculate/Next/Previous.
- [ ] Tree layout + ScrollViewer/Canvas + highlight.
- [ ] Giao diện sáng, tiếng Việt, dialog xem grammar.
- [ ] Chạy toàn bộ test thủ công và xử lý lỗi.

## 13. Rủi ro kỹ thuật và phương án giảm thiểu

| Rủi ro | Cách xử lý |
|---|---|
| `ExprTail`/`TermTail` gây kết hợp phải | Duyệt tail theo thứ tự và fold-left; thử `10-3-2`, `8/4/2` |
| Epsilon/EOF che lỗi cú pháp | FOLLOW set rõ ràng; `Expect(EndOfInput)`; thử `3 4`, `2(3+4)` |
| Giá trị `decimal` ngoài phạm vi | Tách number literal out-of-range và arithmetic overflow; kiểm tra `decimal.TryParse` và bắt overflow |
| Chia 0 làm mất trace | Trả Failure result với steps đã có, Error step cuối |
| Highlight giá trị trung gian không có node | Duy trì provenance node IDs trên mọi kết quả nội bộ |
| UI parse tree bị chồng/không cuộn | TreeLayoutService tính bounding box; Canvas có kích thước hữu hạn, ScrollViewer Auto |
| Thay input làm kết quả cũ gây nhầm | Stale banner và nhãn `LastAnalyzedExpression` |
| Nút steps thay đổi mà view chưa cập nhật | Observable properties + NotifyCanExecuteChanged; mapping ID/node VM |
| Lỗi quá sâu hoặc render rất lớn | Giới hạn input 200 `string.Length`; thử biểu thức ngoặc/unary dài |

## 14. Hướng dẫn dùng bộ tài liệu với AI coding agent

Đưa agent các file theo thứ tự: `README.md` → `REQUIREMENTS.md` → `GRAMMAR.md` → `CLASS_DESIGN.md` → `EVALUATION.md` → `ARCHITECTURE.md` → `UI_DESIGN.md` → `TEST_PLAN.md` → `IMPLEMENTATION_PLAN.md`.

Prompt gợi ý:

> Hãy hiện thực giai đoạn [N] của dự án Expression Evaluator dựa trên toàn bộ tài liệu Markdown đã cung cấp. Tuân thủ các yêu cầu đã chốt: một WPF project .NET 10, CommunityToolkit.Mvvm, CFG cố định, Recursive Descent Parser tự viết, chỉ một Parse Tree, không AST, decimal, không test tự động, không lịch sử. Không tự thêm chức năng. Trước khi sửa code, liệt kê file sẽ thay đổi và các điều kiện nghiệm thu của giai đoạn; sau khi hoàn thành, nêu cách kiểm thử thủ công.

**Quy tắc cộng tác:** triển khai từng giai đoạn, không yêu cầu AI viết toàn bộ ứng dụng trong một bước khi chưa kiểm thử; duy trì tính nhất quán về `SourceSpan`, `EvaluationStep`, `ExpressionAnalysisResult` giữa các file.

## 15. Kết quả bàn giao dự kiến

- Mã nguồn WPF một project build được trên Windows/.NET 10.
- Lexer + Parser CFG + Parse Tree + Evaluator/Trace tự xây dựng.
- Giao diện Light Theme hai cột, tiếng Việt, hỗ trợ xem văn phạm.
- Tài liệu thiết kế này và bảng kiểm thử thủ công đã điền kết quả.

**Không nằm trong đợt bàn giao:** web app, AST, chỉnh sửa CFG, màn hình history, export dữ liệu, Unit Test tự động.
