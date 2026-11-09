# Expression Evaluator — Tài liệu chính thức E/T/F

**Trạng thái:** Mã nguồn đã có parser/evaluator E/T/F; test logic tự động đã chạy, nhưng **chưa nghiệm thu 80 ca WPF thủ công**. Ngôn ngữ tài liệu: tiếng Việt; tên API/ký hiệu CFG: tiếng Anh. Mục tiêu ứng dụng: WPF / C# / .NET 10, CommunityToolkit.Mvvm.

## Quyết định cốt lõi

* Lexer nhận số nguyên ASCII, `+ - * / ( )`, whitespace; input tối đa 200 `string.Length`, tính `decimal`, hiển thị tối đa 6 số lẻ AwayFromZero. Không nhân ngầm/số thập phân đầu vào.
* Recursive Descent Parser xây **một Parse Tree** CFG `E/T/F`, không AST, không `ExprTail/TermTail/ε`; terminal hiện lexeme, nút tròn, mũi tên cha → con, gốc `E` viền xanh.
* **Cây CFG phân nhánh phải mô tả dẫn xuất, không phải AST biểu diễn thứ tự tính.** Evaluator fold-left toán tử cùng mức: `10-3-2=5`, `8/4/2=1`; provenance của giá trị trung gian có thể là **tập terminal thuộc nhiều nhánh**, không phải một subtree. Giao diện/dialog phải giải thích rõ.
* Lexer/Parser/Evaluator thuần C#, cây bất biến; `ExpressionService` hợp nhất Input/Lexical/Syntax/Evaluation/Success, giữ cây và trace khi lỗi đánh giá. MVVM điều hướng steps không tính/layout lại; sửa input trước Submit giữ dữ liệu cũ và báo stale.
* Nghiệm thu **thủ công qua WPF** theo `TEST_PLAN.md` (**80 ca**); có thêm xUnit tại `Tests/` để kiểm tra hồi quy logic EV/ER/EE và một phần state/cây. Test tự động không xác nhận UI WPF Pass. Không thêm AST, history, zoom/pan, persistence.

## Đọc theo thứ tự

1. [`REQUIREMENTS.md`](REQUIREMENTS.md): phạm vi, trạng thái/lỗi và bất biến nghiệp vụ.
2. [`GRAMMAR.md`](GRAMMAR.md): token, CFG, left-factoring khi Parse, FOLLOW/EOF và ý nghĩa cây.
3. [`ARCHITECTURE.md`](ARCHITECTURE.md): ranh giới các tầng và trình tự Submit.
4. [`CLASS_DESIGN.md`](CLASS_DESIGN.md): contract model, error/result/step, geometry, VM.
5. [`EVALUATION.md`](EVALUATION.md): fold-left, provenance, Error step và rounding.
6. [`UI_DESIGN.md`](UI_DESIGN.md): node tròn có đường kính động, mũi tên, các trạng thái UI và dialog.
7. [`IMPLEMENTATION_PLAN.md`](IMPLEMENTATION_PLAN.md): các giai đoạn triển khai và điều kiện nghiệm thu.
8. [`TEST_PLAN.md`](TEST_PLAN.md): kiểm thử thủ công đầy đủ EV/ER/EE/UI.
9. [`MANUAL_TEST_CASES.md`](MANUAL_TEST_CASES.md): thao tác và kỳ vọng chi tiết cho toàn bộ 80 ca kiểm thử thủ công.
10. [`TEST_RESULTS.md`](TEST_RESULTS.md): kết quả xUnit và phiếu ghi Actual/Pass/Fail của 80 ca WPF (hiện toàn bộ Chưa chạy).

**Lưu ý nghiệm thu:** Cần chạy thủ công 80 ca WPF trước khi tuyên bố hoàn thành kiểm thử. Kết quả xUnit không xác nhận giao diện WPF.
