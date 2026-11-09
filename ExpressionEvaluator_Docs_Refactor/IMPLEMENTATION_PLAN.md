# IMPLEMENTATION_PLAN — Triển khai/di trú theo giai đoạn

**Phạm vi thiết kế ứng dụng:** một project WPF .NET 10, CommunityToolkit.Mvvm, một Parse Tree CFG E/T/F không có tail/ε hay AST, tính `decimal`. Có thêm project xUnit riêng tại `Tests/` theo yêu cầu bổ sung, không thêm ANTLR, history/zoom/pan. Việc di trú từ `Expr/Term/Unary/Primary` + `ExprTail/TermTail` là **thay đổi cấu trúc Parser/Evaluator và renderer**, không thể chỉ sửa nhãn node hoặc XAML. Kiểm thử thủ công WPF vẫn bắt buộc cho nghiệm thu.

| Giai đoạn | Công việc chính | Điều kiện nghiệm thu |
|---|---|---|
| 0 — Nền tảng | WPF `net10.0-windows`, CommunityToolkit.Mvvm, theme sáng | Build/chạy cửa sổ; SDK đã có thì không cài lại |
| 1 — Models | `SourceSpan`, Token, ParseTreeNode **không IsEpsilon**, error/analysis/result/step/geometry | Bất biến trạng thái và collection read-only theo `CLASS_DESIGN.md` |
| 2 — Lexer | ASCII `[0-9]+`, toán tử, whitespace, EOF và lỗi Lexical | Vị trí token/EOF đúng, decimal invariant, lỗi số vượt phạm vi |
| 3 — Grammar/Parser | `GrammarDefinition` E/T/F, `ParseE/T/F` theo tiền tố chung, FOLLOW/EOF, node 1/2/3 con | `3+5*2`, `--5`, `-(2+3)` cho cây đúng; `3+*5`, `2(3+4)`, `(2+3` báo lỗi không có cây |
| 4 — Evaluator/Trace | Fold-left dọc `rightE/rightT` (chỉ đánh giá T/F kế tiếp); unary, provenance terminal, NumberFormatter, Error step | `10-3-2=5`, `8/4/2=1`, trace đúng thứ tự; chia 0/tràn giữ trace; không làm tròn trung gian |
| 5 — ExpressionService | Kiểm tra blank/200 `string.Length` trước Lexer, hợp nhất success/error | Input/Lexical/Syntax không cây; Evaluation Error còn cây/trace; Error cùng đối tượng |
| 6 — MainViewModel | Calculate/Next/Previous/stale, Grammar dialog abstraction, notify/CanExecute | Chỉ Submit mới thay state; sửa input giữ state cũ; Next không tính/layout lại |
| 7 — Layout/renderer | Đo đường kính tròn theo nhãn, bề rộng subtree/cấp theo kích thước max, mũi tên, root viền xanh | Literal 29 chữ số không cắt; node/cạnh không chồng, cuộn hai chiều; đổi step không relayout |
| 8 — UI | Hai cột, Enter=Calculate, lỗi/EOF selection, stale, Grammar Dialog read-only và chú giải cây dẫn xuất | Các nhánh trạng thái `UI_DESIGN.md` có thể thao tác trực tiếp |
| 9 — Kiểm thử | Chạy xUnit cho logic; **kiểm thử WPF thủ công** theo `TEST_PLAN.md`: EV → ER → EE → UI, ghi Actual/Pass/Fail | **Toàn bộ ca WPF bắt buộc Pass**; không giả định Pass từ build/xUnit |

## Thứ tự di trú từ bản đang có

1. Giữ Lexer/SourceSpan/input validation/NumberFormatter nếu chúng vẫn đúng hợp đồng, **không đổi ngữ nghĩa token**. Đồng bộ `GrammarDefinition` với `GRAMMAR.md` mới.
2. Đổi Parser: xóa node `ExprTail`/`TermTail`, tạo `E/T/F`, mọi terminal; `F(op,F)` đúng **2 con**; dùng FOLLOW và kiểm tra EOF. Nếu đổi model `IsEpsilon`, sửa mọi code tham chiếu.
3. Đổi Evaluator **cùng lúc**: root là `E`, fold-left qua `E(T,op,E)`/`T(F,op,T)` và provenance theo terminal; không dùng hàm đệ quy đánh giá toàn bộ `rightE/rightT`. Không để parser mới chạy với Evaluator cũ.
4. Đổi TreeLayoutService/ParseTreeView: đường kính động, tính row height theo node lớn nhất, mũi tên, root accent và lexeme terminal. Chỉ thay `TreeLayout` sau khi clear geometry cũ và thay root cho cùng một Submit.
5. Đổi grammar dialog/chú giải, cập nhật test manual. Chỉ tuyên bố ứng dụng theo kiến trúc refactor khi các ca **EV/ER/EE/UI** đều được chạy và ghi kết quả.

**Rủi ro cần kiểm tra đặc biệt:** cây dẫn xuất phải không là cây nhóm phép toán; intermediate provenance không nhất thiết là một subtree; `F` unary 2 con; literal dài cần circle/row lớn; lỗi RHS phải dừng step cha; cú pháp sai không hiện cây; thay Submit phải xóa layout cũ; không đánh dấu ca lỗi Pass chỉ vì EV pass.
