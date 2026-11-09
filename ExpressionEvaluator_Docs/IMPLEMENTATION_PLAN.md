# IMPLEMENTATION_PLAN — Các giai đoạn triển khai và nghiệm thu

**Phạm vi:** một project ứng dụng WPF .NET 10, CommunityToolkit.Mvvm, một Parse Tree CFG E/T/F không có tail/ε hay AST, tính `decimal`; project xUnit riêng tại `Tests/`. Không thêm ANTLR, history/zoom/pan. Mã hiện đã có parser/evaluator E/T/F và bộ xUnit, nhưng **80 ca WPF chưa nghiệm thu thủ công**. Phần dưới ghi lại các giai đoạn thiết kế và lưu ý khi bảo trì.

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

## Lưu ý khi bảo trì kiến trúc E/T/F

1. Giữ Lexer/SourceSpan/input validation/NumberFormatter đúng hợp đồng, **không đổi ngữ nghĩa token**. Đồng bộ `GrammarDefinition` với `GRAMMAR.md` khi thay văn phạm.
2. Giữ Parser tạo node `E/T/F` với mọi terminal; `F(op,F)` có đúng **2 con**; tuân thủ FOLLOW và kiểm tra EOF. Không đưa lại `ExprTail`/`TermTail` hoặc `IsEpsilon`.
3. Khi sửa Parser phải xem lại Evaluator: root là `E`, fold-left qua `E(T,op,E)`/`T(F,op,T)` và provenance theo terminal; không đánh giá đệ quy toàn bộ `rightE/rightT` trước phép bên trái.
4. TreeLayoutService/ParseTreeView phải giữ đường kính động, row height theo node lớn nhất, mũi tên, root accent và lexeme terminal. Clear geometry cũ trước khi thay root cho cùng một Submit.
5. Giữ grammar dialog/chú giải và các ca test đồng bộ với CFG; chỉ tuyên bố **nghiệm thu WPF** khi các ca **EV/ER/EE/UI** đã được chạy và ghi kết quả.

**Rủi ro cần kiểm tra đặc biệt:** cây dẫn xuất phải không là cây nhóm phép toán; intermediate provenance không nhất thiết là một subtree; `F` unary 2 con; literal dài cần circle/row lớn; lỗi RHS phải dừng step cha; cú pháp sai không hiện cây; thay Submit phải xóa layout cũ; không đánh dấu ca lỗi Pass chỉ vì EV pass.
