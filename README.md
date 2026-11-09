# Expression Evaluator

Ứng dụng WPF/C# trên Windows, một project `net10.0-windows`, dùng CommunityToolkit.Mvvm. Phiên bản hiện tại đã **di trú mã nguồn sang văn phạm E/T/F** theo `ExpressionEvaluator_Docs_Refactor/`.

## Chạy ứng dụng

```powershell
dotnet restore
dotnet run -c Release
```

Máy phát triển có sẵn .NET SDK 10.0.401, không cần cài thêm .NET.

## Thiết kế đang áp dụng

- Lexer nhận số nguyên ASCII, `+ - * / ( )`, whitespace; giới hạn input 200 đơn vị UTF-16 khi nhấn Tính.
- Parser dựng **một cây dẫn xuất CFG E/T/F** bất biến, không AST, không `ExprTail`/`TermTail`/ε. Terminal hiển thị lexeme gốc. `F` unary có hai con; ngoặc có ba con.
- Cây dẫn xuất rẽ phải **không phải cây nhóm phép tính**. Evaluator đánh giá từng phép cùng mức từ trái sang phải: `10-3-2 = 5`, `8/4/2 = 1`. Trace, lỗi và provenance terminal không sửa cây.
- TreeLayoutService bố trí node tròn với đường kính động; ParseTreeView vẽ mũi tên cha → con và viền xanh cho gốc E, cuộn hai chiều. Next/Previous chỉ đổi highlight, không bố trí lại.
- UI tiếng Việt: kết quả/lỗi hiện ngay, cảnh báo input stale, xem văn phạm read-only; không có history/persistence, nhập thập phân, zoom/pan.

Tài liệu chuẩn: [`ExpressionEvaluator_Docs_Refactor/README.md`](ExpressionEvaluator_Docs_Refactor/README.md). `ExpressionEvaluator_Docs/` là **bản thiết kế cũ** theo `Expr/Term/Unary/Primary`, giữ lại để đối chiếu, không còn là quy cách cây đang triển khai. Hai bộ test có bảng kết quả **riêng**, không chuyển trạng thái Pass giữa chúng.

## Tình trạng kiểm tra

Ngày 09/11/2026: `dotnet build ExpressionEvaluator.csproj --no-restore` và `-c Release --no-restore` đều **0 lỗi/0 cảnh báo**; bản Release mở được cửa sổ `Expression Evaluator`. Đó là kiểm tra build/khởi chạy, **chưa phải nghiệm thu tương tác**.

Theo tài liệu chỉ kiểm thử **thủ công qua WPF**. Các ca của bản mới nằm trong [`ExpressionEvaluator_Docs_Refactor/TEST_PLAN.md`](ExpressionEvaluator_Docs_Refactor/TEST_PLAN.md); ghi Actual/Pass/Fail vào [`ExpressionEvaluator_Docs_Refactor/TEST_RESULTS.md`](ExpressionEvaluator_Docs_Refactor/TEST_RESULTS.md). 80 ca EV/ER/EE/UI hiện **Chưa chạy**, không được đánh dấu Pass chỉ vì build thành công. Không có test project/CI/script kiểm thử tự động.
