# UI_DESIGN — Giao diện WPF / XAML (Light Theme)

## 1. Mục tiêu trải nghiệm

UI là công cụ **quan sát CFG và đánh giá biểu thức**, không phải bản sao máy tính cầm tay. Người dùng nhập trực tiếp vào TextBox, xem ngay kết quả sau Calculate và có thể nhấn Trước/Tiếp để quan sát phép toán được highlight trên **Parse Tree đầy đủ**.

**Ngôn ngữ UI:** Tiếng Việt. **Tên lập trình:** tiếng Anh. **Theme:** sáng/tối giản duy nhất, không có chuyển theme.

## 2. Bố cục chính

```text
┌──────────────────────────────────────────────────────────────────────────┐
│  Expression Evaluator                              [Xem văn phạm]       │
├──────────────────────────────────────────────────────────────────────────┤
│  Biểu thức                                                               │
│  [ 3 + 5 * 2                                               ] [Tính toán] │
│  9 / 200 ký tự                                                           │
│  Kết quả: 13                                                             │
│  [Nếu input thay đổi: Kết quả dưới đây thuộc biểu thức trước: ...]       │
├───────────────────────────────────────────────┬──────────────────────────┤
│  CÂY PHÂN TÍCH CÚ PHÁP                       │  CÁC BƯỚC TÍNH TOÁN     │
│                                               │                          │
│  (ScrollViewer + Canvas)                      │  Bước 1/2                │
│        Expr                                   │  5 * 2 = 10              │
│       /    \                                  │                          │
│     Term   ExprTail                           │  [Trước]       [Tiếp]    │
│        ...                                    │                          │
│                                               │                          │
│  [scroll ngang / dọc nếu cần]                 │                          │
└───────────────────────────────────────────────┴──────────────────────────┘
```

Đây là bố cục **định hướng**, không phải cấu trúc pixel bắt buộc. Khuyến nghị kích thước khởi động khoảng `1100×700`, tỷ lệ cột trái:phải khoảng `2:1`; có thể đặt `MinWidth`/`MinHeight` phù hợp sau khi dựng UI.

## 3. Thành phần trên MainWindow

| Thành phần | Nội dung / Binding | Hành vi |
|---|---|---|
| Header | `Expression Evaluator` | Có nút `Xem văn phạm` |
| TextBox | `InputExpression` (TwoWay, UpdateSourceTrigger=PropertyChanged) | Nhập bằng bàn phím; không tự parse |
| Button `Tính toán` | `CalculateCommand` | Parse/evaluate khi click |
| Enter | Cùng `CalculateCommand` | Chức năng giống click |
| Counter | `InputExpression.Length / 200` | Hiển thị để định hướng, không chặn nhập cứng |
| Result | `ResultText` | Hiển thị ngay khi Calculate thành công |
| Error panel | `ErrorText` | Loại lỗi, nguyên nhân, vị trí nếu có |
| Stale banner | `IsResultOutdated` + `LastAnalyzedExpression` | Báo rằng cây/result là dữ liệu trước |
| ParseTreeView | `CurrentAnalysis.ParseTree`, `TreeLayout` | Cây tĩnh, ẩn ε, cuộn 2 chiều |
| Step panel | `Steps`, `CurrentStepIndex` | Thông tin một bước đang chọn |
| Button `Trước` | `PreviousStepCommand` | Giảm step index nếu >0 |
| Button `Tiếp` | `NextStepCommand` | Tăng step index nếu <N |

Không có nút số kiểu máy tính, nhập số thập phân, màn lịch sử hay Tab AST.

## 4. Quy tắc trạng thái UI

### 4.1. Trước Calculate

- TextBox có thể trống; result/tree/steps ẩn hoặc placeholder nhẹ.
- `Trước`, `Tiếp` disabled.

### 4.2. Sau Calculate thành công

- Hiện `ResultText` ngay, ví dụ `13`.
- Hiện Parse Tree hoàn chỉnh.
- Step index = 0; panel hướng dẫn **“Nhấn Tiếp để xem từng bước tính”** nếu có steps.
- Nếu không có toán tử (ví dụ `42`), thông báo **“Biểu thức không cần bước tính trung gian”** và disable Trước/Tiếp.

### 4.3. Khi Next/Previous

- Không thay kết quả cuối, không thay cây hay layout.
- Node toán tử và các toán hạng nguồn của step hiện tại được highlight.
- Vùng bước hiển thị `Bước i/N` và diễn giải, ví dụ `5 × 2 = 10`.
- Khi về `Bước 0/N`, bỏ mọi highlight và hiển thị hướng dẫn.
- Nếu đang ở step lỗi, dùng màu lỗi cho step và các thành phần phù hợp.

### 4.4. Khi chỉnh sửa input nhưng chưa Calculate

- Giữ nguyên result/tree/steps **cũ**.
- Cảnh báo rõ: **“Biểu thức đã thay đổi. Kết quả và cây bên dưới thuộc biểu thức đã phân tích trước đó: [LastAnalyzedExpression].”**
- Vẫn cho xem steps của dữ liệu cũ.
- Không báo lỗi cú pháp của nội dung đang gõ; không chạy lexer/parser.

### 4.5. Khi Submit bị lỗi

| Loại lỗi | Result | Tree | Steps | Hiển thị |
|---|---|---|---|---|
| Rỗng / quá dài | Không | Không | Không | Thông báo đầu vào |
| Lexical | Không | Không | Không | Lỗi và vị trí trong TextBox |
| Syntax | Không | Không | Không | Lỗi và vị trí/EOF |
| Evaluation | Không có giá trị hợp lệ | **Có** | **Có**, kết thúc bằng Error step | Thông báo lỗi và trace |

Với Evaluation Error, lỗi xuất hiện ngay sau Calculate, **không cần** đợi người dùng Next tới bước cuối mới thấy lỗi.

## 5. Grammar Dialog

- Mở bằng nút **Xem văn phạm**.
- Tiêu đề: **Văn phạm phi ngữ cảnh (CFG)**.
- Hiển thị các production `Expr`, `ExprTail`, `Term`, `TermTail`, `Unary`, `Primary` và giải nghĩa `ε`.
- Nội dung `TextBlock`/`TextBox` read-only, có thể cuộn nếu cao.
- Nút **Đóng**; không cho sửa, import hoặc thay thế grammar.
- Đóng dialog không reset kết quả/cây/step.

## 6. Phong cách trực quan

Bảng màu gợi ý (có thể tinh chỉnh mà không thay đổi thiết kế):

| Mục đích | Màu |
|---|---|
| Nền chính | `#FFFFFF` |
| Background panel | `#F8FAFC` |
| Border | `#E2E8F0` |
| Text chính | `#0F172A` |
| Text phụ | `#64748B` |
| Primary action / highlight | `#2563EB` |
| Thành công | `#15803D` |
| Lỗi | `#B91C1C` |

- Nút và panel có bo góc nhẹ; phân cấp chữ rõ; không hoạt hình phức tạp.
- Màu không phải tín hiệu duy nhất: lỗi có icon/text, step có nhãn, node highlight có viền dày tương phản.
- Giữ font UI quen thuộc với Windows (ví dụ Segoe UI), đủ cỡ để nhìn được ở 100% scaling.

## 7. ParseTreeView — hiện thực đề xuất

1. Bọc `Canvas` bởi `ScrollViewer` (`HorizontalScrollBarVisibility=Auto`, `VerticalScrollBarVisibility=Auto`).
2. `Canvas.Width/Height` từ `TreeLayoutResult` (không chỉ kích thước viewport).
3. Vẽ cạnh nối node ở layer dưới; khối node ở layer trên.
4. Bảo đảm thứ tự con theo production; mỗi node hiển thị `Symbol` và `Lexeme` nếu terminal.
5. Vẽ **tất cả non-terminal** kể cả `ExprTail`, `TermTail` nhưng bỏ node `IsEpsilon`.
6. Đánh dấu nút theo `HighlightedNodeIds`; không tự tính toán trong view.
7. Update brush/border khi bước đổi; không gọi lại `TreeLayoutService` khi Next/Previous.
8. Không Zoom/Pan/Fit; chỉ thanh cuộn chuẩn. Tự cuộn đến node highlight **không phải yêu cầu bắt buộc** (có thể làm sau, không thuộc MVP đã chốt).

## 8. Xử lý vị trí lỗi trên TextBox

- `SourceSpan.StartIndex` là zero-based. Nếu `Length > 0`, có thể dùng `TextBox.Select(StartIndex, Length)` để chỉ vị trí liên quan, sau khi UI cập nhật.
- Nếu `Length = 0` tại EOF, đưa caret về cuối TextBox và hiển thị “vị trí N+1 (cuối biểu thức)”.
- Với lỗi input toàn chuỗi (rỗng/quá dài), không cần mũi tên gắn một token giả.
- Chỉ chọn lỗi khi người dùng **Submit**; khi họ bắt đầu sửa nội dung mới, không kiểm tra hay cập nhật lỗi real-time.

## 9. Gợi ý ResourceDictionary và MVVM

- `Resources/Theme.xaml`: `SolidColorBrush` cho background, border, text, primary, success, error.
- `MainViewModel` cấp `ResultText`, `ErrorText`, stale flag, step state và tree layout.
- `ParseTreeNodeViewModel` cấp nhãn/vị trí/highlight; `GrammarViewModel` cấp chuỗi grammar read-only.
- Command gồm `Calculate`, `NextStep`, `PreviousStep`, `ViewGrammar`.
- Code-behind chỉ để xử lý những thao tác WPF thuần UI (focus, selection, khởi tạo control); không chứa lexer/parser/evaluator.

## 10. Tiêu chí chấp nhận giao diện

- Bố cục hai cột hoạt động trên kích thước cửa sổ tối thiểu.
- Parse Tree đủ node, ε không hiện, nhãn và cạnh không chồng lên nhau trong bài kiểm thử nhỏ/trung bình.
- Thanh cuộn xuất hiện khi cây lớn hơn vùng hiển thị.
- Highlight đúng toán tử và operand(s) của step; dữ liệu stale hiển thị rõ.
- Enter và click cho cùng kết quả; Grammar Dialog read-only; thông báo lỗi tiếng Việt và chỉ vị trí.

Các quy tắc chức năng chi tiết xem [`REQUIREMENTS.md`](REQUIREMENTS.md), mô hình bước xem [`EVALUATION.md`](EVALUATION.md).
