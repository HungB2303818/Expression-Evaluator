# TEST_RESULTS — Kết quả kiểm thử E/T/F

**Trạng thái nghiệm thu WPF:** Chưa chạy thủ công. Mã nguồn hiện đã có parser/evaluator E/T/F nhưng chưa xác nhận toàn bộ hành vi WPF. Kết quả xUnit ở dưới là kiểm thử riêng, không tự động biến 80 ca WPF thành Pass.

Môi trường/ngày/người kiểm thử: __________. Chạy ứng dụng WPF trên Windows theo `TEST_PLAN.md`. Với mỗi ID ghi Actual Result, trạng thái **Pass / Fail** (hoặc giữ **Chưa chạy**) và ảnh/bước tái hiện nếu Fail. Build hoặc đọc mã nguồn không tính là Pass cho ca UI/logic.

Kiểm tra kỹ thuật ngày 09/11/2026 (không tính trong 80 ca): build Debug và Release `net10.0-windows` **0 lỗi, 0 cảnh báo**; bản Release mở cửa sổ `Expression Evaluator`, đóng bình thường với exit code 0. Chưa xác nhận bằng tay kết quả biểu thức, mũi tên, nút tròn và các trạng thái lỗi.

Kiểm tra xUnit ngày 09/11/2026 (riêng, **không tính là Pass cho 80 ca WPF**): chạy `dotnet test Tests/ExpressionEvaluator.Tests.csproj -c Release --nologo` trên Windows / .NET SDK 10.0.401: **60 Passed, 0 Failed, 0 Skipped**. Gồm 28 EV, 20 ER và 6 EE (kiểm tra logic), thêm 6 bài kiểm tra bổ sung về thứ tự phép tính, state/ViewModel/CFG/layout cho một phần các ca UI. Chưa kiểm thử thao tác Enter, hình vẽ thực, scroll, dialog và highlight nhìn thấy trên WPF.

| ID | Actual Result / ngày / ảnh lỗi | Trạng thái |
|---|---|---|
| EV-01 | — | Chưa chạy |
| EV-02 | — | Chưa chạy |
| EV-03 | — | Chưa chạy |
| EV-04 | — | Chưa chạy |
| EV-05 | — | Chưa chạy |
| EV-06 | — | Chưa chạy |
| EV-07 | — | Chưa chạy |
| EV-08 | — | Chưa chạy |
| EV-09 | — | Chưa chạy |
| EV-10 | — | Chưa chạy |
| EV-11 | — | Chưa chạy |
| EV-12 | — | Chưa chạy |
| EV-13 | — | Chưa chạy |
| EV-14 | — | Chưa chạy |
| EV-15 | — | Chưa chạy |
| EV-16 | — | Chưa chạy |
| EV-17 | — | Chưa chạy |
| EV-18 | — | Chưa chạy |
| EV-19 | — | Chưa chạy |
| EV-20 | — | Chưa chạy |
| EV-21 | — | Chưa chạy |
| EV-22 | — | Chưa chạy |
| EV-23 | — | Chưa chạy |
| EV-24 | — | Chưa chạy |
| EV-25 | — | Chưa chạy |
| EV-26 | — | Chưa chạy |
| EV-27 | — | Chưa chạy |
| EV-28 | — | Chưa chạy |
| ER-01 | — | Chưa chạy |
| ER-02 | — | Chưa chạy |
| ER-03 | — | Chưa chạy |
| ER-04 | — | Chưa chạy |
| ER-05 | — | Chưa chạy |
| ER-06 | — | Chưa chạy |
| ER-07 | — | Chưa chạy |
| ER-08 | — | Chưa chạy |
| ER-09 | — | Chưa chạy |
| ER-10 | — | Chưa chạy |
| ER-11 | — | Chưa chạy |
| ER-12 | — | Chưa chạy |
| ER-13 | — | Chưa chạy |
| ER-14 | — | Chưa chạy |
| ER-15 | — | Chưa chạy |
| ER-16 | — | Chưa chạy |
| ER-17 | — | Chưa chạy |
| ER-18 | — | Chưa chạy |
| ER-19 | — | Chưa chạy |
| ER-20 | — | Chưa chạy |
| EE-01 | — | Chưa chạy |
| EE-02 | — | Chưa chạy |
| EE-03 | — | Chưa chạy |
| EE-04 | — | Chưa chạy |
| EE-05 | — | Chưa chạy |
| EE-06 | — | Chưa chạy |
| UI-01 | — | Chưa chạy |
| UI-02 | — | Chưa chạy |
| UI-03 | — | Chưa chạy |
| UI-04 | — | Chưa chạy |
| UI-05 | — | Chưa chạy |
| UI-06 | — | Chưa chạy |
| UI-07 | — | Chưa chạy |
| UI-08 | — | Chưa chạy |
| UI-09 | — | Chưa chạy |
| UI-10 | — | Chưa chạy |
| UI-11 | — | Chưa chạy |
| UI-12 | — | Chưa chạy |
| UI-13 | — | Chưa chạy |
| UI-14 | — | Chưa chạy |
| UI-15 | — | Chưa chạy |
| UI-16 | — | Chưa chạy |
| UI-17 | — | Chưa chạy |
| UI-18 | — | Chưa chạy |
| UI-19 | — | Chưa chạy |
| UI-20 | — | Chưa chạy |
| UI-21 | — | Chưa chạy |
| UI-22 | — | Chưa chạy |
| UI-23 | — | Chưa chạy |
| UI-24 | — | Chưa chạy |
| UI-25 | — | Chưa chạy |
| UI-26 | — | Chưa chạy |

**Nghiệm thu:** 80/80 ID Pass sau khi chạy thật, không còn Fail/Chưa chạy; với lỗi phải sửa và kiểm tra hồi quy.
