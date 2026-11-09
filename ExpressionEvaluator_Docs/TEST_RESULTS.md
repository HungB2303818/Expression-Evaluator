# TEST_RESULTS — Nhật ký kiểm thử giai đoạn 9

Ngày: 08/11/2026  
Môi trường: Windows, .NET SDK 10.0.401, `net10.0-windows`  
Phương pháp: kiểm thử **thủ công qua giao diện WPF** theo `TEST_PLAN.md`; không có test project/CI/script tự động.

Các bước thao tác và kỳ vọng chi tiết từng ID: [`MANUAL_TEST_CASES.md`](MANUAL_TEST_CASES.md). Các bảng bên dưới là **phiếu ghi kết quả thực tế**, không phải bằng chứng đã chạy.

## Kiểm tra có bằng chứng

| Kiểm tra | Kết quả | Bằng chứng / giới hạn |
|---|---|---|
| `dotnet build ExpressionEvaluator.csproj -c Release --no-restore` | Pass | 0 lỗi, 0 cảnh báo; sinh DLL Release |
| Mở ứng dụng bản Release | Pass (smoke) | Tiến trình chạy, có cửa sổ tiêu đề `Expression Evaluator` và handle; đóng cửa sổ, exit code 0 |

**Không coi build/smoke là bằng chứng Pass của EV/ER/EE/UI.** Hiện chưa thao tác trực tiếp ô nhập, nút bấm, dialog, scrollbar hoặc quan sát giá trị trên cửa sổ WPF trong phiên này. Các ca sau đang **Chưa chạy**, không phải Pass/Fail. Khi kiểm tra thủ công, thay trạng thái, ghi Actual Result; với ca Fail, thêm ngày/môi trường/bước tái hiện/đường dẫn ảnh vào ô Actual Result hoặc ghi chú bên dưới bảng tương ứng.

## EV — Kết quả đúng

| ID | Actual Result | Status |
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

## ER — Lỗi đầu vào, từ vựng, cú pháp

| ID | Actual Result | Status |
|---|---|---|
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

## EE — Lỗi đánh giá

| ID | Actual Result | Status |
|---|---|---|
| EE-01 | — | Chưa chạy |
| EE-02 | — | Chưa chạy |
| EE-03 | — | Chưa chạy |
| EE-04 | — | Chưa chạy |
| EE-05 | — | Chưa chạy |
| EE-06 | — | Chưa chạy |

## UI — Tương tác và trạng thái

| ID | Actual Result | Status |
|---|---|---|
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

## Quy trình hoàn tất

1. Chạy `dotnet run -c Release` trên Windows, đối chiếu từng input và bước ở `TEST_PLAN.md` theo thứ tự EV → ER → EE → UI.
2. Ghi kết quả thực tế, ngày và Pass/Fail cho từng ID; đặc biệt kiểm tra steps, vị trí one-based, highlight/scroll và giữ trace khi lỗi đánh giá.
3. Với ca Fail, ghi input, thao tác tái hiện và ảnh màn hình; sửa lỗi rồi chạy lại ca liên quan và ca hồi quy.
4. Chỉ xác nhận nghiệm thu giai đoạn 9 khi **tất cả ca bắt buộc Pass**. Hiện tại **chưa nghiệm thu**.
