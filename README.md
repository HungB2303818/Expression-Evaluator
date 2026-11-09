# Expression Evaluator

Ứng dụng **Windows desktop (WPF, C#)** để tính giá trị biểu thức số học, xem cây phân tích cú pháp **E/T/F** và theo dõi từng bước tính. Giao diện và thông báo lỗi bằng tiếng Việt.

## Yêu cầu

- **Windows** (WPF không chạy trực tiếp trên macOS/Linux).
- **.NET 10 SDK** để tải mã nguồn và chạy bằng `dotnet`. Tải từ [trang .NET chính thức](https://dotnet.microsoft.com/download/dotnet/10.0); kiểm tra sau khi cài bằng `dotnet --version` (phiên bản bắt đầu bằng `10.`).
- Kết nối Internet ở lần `dotnet restore` đầu tiên để tải gói NuGet, bao gồm `CommunityToolkit.Mvvm`.
- Cần **Git** nếu dùng lệnh `git clone`; cũng có thể tải ZIP từ GitHub rồi giải nén.

Không cần cài Visual Studio nếu chạy bằng .NET CLI. Nếu dùng Visual Studio, hãy chọn bản hỗ trợ .NET 10 và workload **.NET desktop development**.

## Tải và chạy

Mở PowerShell trong thư mục bạn muốn lưu dự án:

```powershell
git clone https://github.com/HungB2303818/Expression-Evaluator.git
cd Expression-Evaluator
dotnet restore ExpressionEvaluator.csproj
dotnet run --project ExpressionEvaluator.csproj -c Release
```

Nếu tải ZIP, mở PowerShell tại thư mục đã giải nén rồi chạy hai lệnh `dotnet restore` và `dotnet run` ở trên. Để chỉ biên dịch ứng dụng:

```powershell
dotnet build ExpressionEvaluator.csproj -c Release
```

File chạy sau khi build nằm ở `bin/Release/net10.0-windows/ExpressionEvaluator.exe` (máy chạy cần có .NET Desktop Runtime 10; khi dùng `dotnet run` đã có SDK tương ứng). `bin/` và `obj/` là sản phẩm build, không được đưa lên GitHub.

## Cách sử dụng

1. Nhập biểu thức vào ô **Biểu thức**, ví dụ `3+5*2`.
2. Nhấn **Enter** hoặc chọn **Tính toán**. Kết quả `13`, cây E/T/F và số bước xuất hiện ngay.
3. Chọn **Tiếp** / **Trước** để xem phép tính và những phần tương ứng được đánh dấu trên cây. Chọn **Xem văn phạm** để đọc các quy tắc E/T/F.

Ví dụ: `(3+5)*2` cho kết quả `16`; `10-3-2` cho kết quả `5` vì các phép trừ cùng mức được **tính từ trái sang phải**. Cây E/T/F mô tả *luật sinh*, không phải cây nhóm thứ tự tính toán.

**Quy tắc đầu vào:** số nguyên ASCII (`0`–`9`), `+`, `-`, `*`, `/`, dấu ngoặc và khoảng trắng; tối đa **200 ký tự** (tính cả khoảng trắng). Có thể dùng dấu `+`/`-` một ngôi, ví dụ `3*-2`. Không hỗ trợ số thập phân trong đầu vào (`3.5`, `3,5`), nhân ngầm (`2(3+4)`) hay ký hiệu `^`. Kết quả tính bằng `decimal`, hiển thị tối đa 6 chữ số sau dấu `.`; chỉ làm tròn **khi hiển thị**. Biểu thức sai có thông báo lỗi; lỗi trong lúc tính như chia cho 0 vẫn giữ cây và các bước đã thực hiện.

Khi sửa ô nhập mà chưa nhấn Tính toán, kết quả/cây trước đó vẫn được giữ kèm cảnh báo dữ liệu cũ. Ứng dụng không lưu lịch sử giữa các lần mở.

## Chạy kiểm thử tự động

Từ thư mục gốc dự án, trên Windows có .NET 10 SDK:

```powershell
dotnet test Tests/ExpressionEvaluator.Tests.csproj -c Release
```

Bộ xUnit kiểm tra logic đánh giá, phân loại lỗi, cây và một phần trạng thái ViewModel/layout. Lần chạy được ghi trong [kết quả kiểm thử refactor](ExpressionEvaluator_Docs_Refactor/TEST_RESULTS.md) đạt **60/60 test**; bạn nên chạy lại trên máy mình. **Test tự động không thay cho kiểm thử giao diện WPF:** 80 ca thao tác thủ công trong [test plan](ExpressionEvaluator_Docs_Refactor/TEST_PLAN.md) chưa được xác nhận Pass.

## Cấu trúc dự án và tài liệu

| Đường dẫn | Nội dung |
|---|---|
| `Parsing/` | Lexer, parser đệ quy xuống và định nghĩa văn phạm E/T/F |
| `Evaluation/`, `Services/` | Tính giá trị, định dạng, điều phối phân tích và bố trí cây |
| `Models/` | Token, cây phân tích, kết quả, lỗi và các bước tính |
| `ViewModels/`, `Views/`, `Resources/` | Trạng thái MVVM, giao diện WPF và giao diện cây |
| `Tests/` | Project xUnit độc lập với ứng dụng WPF |
| [`ExpressionEvaluator_Docs_Refactor/`](ExpressionEvaluator_Docs_Refactor/README.md) | Đặc tả E/T/F và kế hoạch/kết quả kiểm thử đang áp dụng |
| `ExpressionEvaluator_Docs/` | Tài liệu của thiết kế văn phạm **cũ**, giữ để đối chiếu |

Nếu `dotnet` không được nhận diện, cài .NET 10 SDK rồi mở lại PowerShell. Nếu khôi phục gói thất bại, kiểm tra kết nối mạng và chạy lại `dotnet restore ExpressionEvaluator.csproj` (đối với test: `dotnet restore Tests/ExpressionEvaluator.Tests.csproj`).
