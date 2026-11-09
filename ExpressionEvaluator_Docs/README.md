# Expression Evaluator — Bộ tài liệu thiết kế

> **Trạng thái:** Đặc tả đã được thống nhất, sẵn sàng triển khai.  
> **Ngày chốt:** 07/11/2026  
> **Ngôn ngữ tài liệu:** Tiếng Việt; tên kiểu dữ liệu, lớp, phương thức và ký hiệu CFG bằng tiếng Anh.

## 1. Mục tiêu

Xây dựng ứng dụng desktop **Expression Evaluator** bằng **C# / WPF / .NET 10 LTS** nhằm:

1. Nhận biểu thức số học với toán hạng là **số nguyên** và các phép `+`, `-`, `*`, `/`, dấu ngoặc.
2. Phân tích cú pháp bằng **CFG cố định** và **Recursive Descent Parser tự viết**.
3. Hiển thị **Parse Tree** (không hiển thị AST), ẩn nhánh epsilon, cho phép cuộn ngang/dọc.
4. Tính toán bằng `decimal`, hiển thị kết quả ngay sau **Tính toán**, tối đa **6 chữ số thập phân**.
5. Cho phép duyệt **từng bước tính** bằng **Trước / Tiếp**, làm nổi bật toán tử và các thành phần tham gia phép tính trên Parse Tree.
6. Thông báo lỗi cụ thể về loại, nguyên nhân, vị trí; với lỗi đánh giá, giữ cây và các bước dẫn đến lỗi.

## 2. Công nghệ và quyết định kiến trúc

| Hạng mục | Quyết định đã chốt |
|---|---|
| Nền tảng | Windows desktop, WPF / XAML |
| Ngôn ngữ / Runtime | C# / .NET 10 LTS (`net10.0-windows`) |
| MVVM | CommunityToolkit.Mvvm |
| Solution | **Một project WPF**, tách các thành phần bằng thư mục |
| Lexer, Parser, Evaluator | Tự xây dựng; không sử dụng ANTLR hoặc parser generator |
| Grammar | CFG cố định; xem qua Dialog read-only |
| Mô hình cú pháp | **Chỉ một Parse Tree**, dùng để hiển thị và đánh giá; không tạo AST |
| Phép nhân | Chỉ tường minh bằng `*` |
| Toán tử một ngôi | Cả `+` và `-`, có bước tính riêng |
| Dữ liệu số | Input là số nguyên ASCII; tính nội bộ bằng `decimal` |
| Định dạng | Tối đa 6 số lẻ, làm tròn `AwayFromZero`, bỏ số 0 dư |
| Giao diện | Tiếng Việt, Light Theme, bố cục 2 cột |
| Vùng cây | Bên trái, tĩnh, cuộn; không Zoom/Pan/Fit; ẩn `ε` |
| Điều khiển bước | Bên phải, Next/Previous; kết quả cuối hiển thị ngay |
| Nhập liệu | TextBox; Enter hoặc nút Tính toán; không phân tích khi gõ |
| Khi sửa biểu thức | Giữ kết quả/cây/bước của lần phân tích trước, báo dữ liệu cũ |
| Giới hạn | Tối đa 200 đơn vị mã UTF-16 (`string.Length`), tính cả khoảng trắng |
| Lịch sử | Không lưu lịch sử và không lưu xuống ổ đĩa |
| Kiểm thử | **Chỉ kiểm thử thủ công**, không tạo test project |

## 3. Phạm vi và ngoài phạm vi

**Trong phạm vi:** biểu thức số nguyên; bốn phép toán hai ngôi; cộng/trừ một ngôi; dấu ngoặc; độ ưu tiên, kết hợp trái; phát hiện lỗi; cây CFG; diễn giải từng phép toán; định dạng kết quả.

**Ngoài phạm vi:** nhập số thập phân, lũy thừa, phần dư, hàm toán học, phép nhân ngầm, chỉnh sửa grammar, AST, ảnh cây, xuất lịch sử, database, tự động tính khi nhập, Zoom/Pan, nút bàn phím kiểu máy tính, Unit Test tự động, nhiều project.

## 4. Danh mục tài liệu

Đọc theo thứ tự khuyến nghị:

1. [`REQUIREMENTS.md`](REQUIREMENTS.md) — yêu cầu, use case, trạng thái UI và quy tắc lỗi.
2. [`GRAMMAR.md`](GRAMMAR.md) — token, CFG, ưu tiên, cách tạo Parse Tree, xử lý đệ quy phải.
3. [`ARCHITECTURE.md`](ARCHITECTURE.md) — một project, phân lớp MVVM, luồng dữ liệu, cấu trúc thư mục.
4. [`CLASS_DESIGN.md`](CLASS_DESIGN.md) — mô hình và hợp đồng API giữa các lớp.
5. [`EVALUATION.md`](EVALUATION.md) — thuật toán tính `decimal`, trace và highlight.
6. [`UI_DESIGN.md`](UI_DESIGN.md) — bố cục Light Theme, behavior và state.
7. [`TEST_PLAN.md`](TEST_PLAN.md) — bộ test case thủ công và tiêu chí kiểm tra.
8. [`IMPLEMENTATION_PLAN.md`](IMPLEMENTATION_PLAN.md) — thứ tự hiện thực, mốc nghiệm thu và hướng dẫn làm việc với AI coding agent.

## 5. Quy trình chạy ứng dụng theo thiết kế

```text
TextBox (InputExpression)
    │ Enter / Tính toán
    ▼
ExpressionService.Analyze(expression)
    ├─ Input validation: rỗng / >200
    ├─ Lexer.Tokenize
    ├─ RecursiveDescentParser.Parse
    └─ ExpressionEvaluator.Evaluate(ParseTree)
         ├─ EvaluationResult.Value hoặc Error
         └─ EvaluationStep[] + HighlightedNodeIds
    ▼
MainViewModel
    ├─ Result / Error
    ├─ ParseTreeView (bên trái)
    ├─ StepNavigator (bên phải)
    └─ View Grammar (Dialog read-only)
```

**Bất biến quan trọng:** Parse Tree không bị sửa khi đánh giá hoặc khi Next/Previous; dữ liệu tính toán lưu ngoài cây. `EvaluationStep` tham chiếu tới ID nút cú pháp. Nếu phân tích cú pháp thất bại, không có Parse Tree cho lần phân tích mới. Nếu lỗi tính toán, vẫn có Parse Tree và các bước dẫn đến lỗi.

## 6. Thiết lập dự án dự kiến

Yêu cầu máy **Windows**, cài **.NET 10 SDK** và IDE hỗ trợ WPF/.NET 10.

```powershell
dotnet new wpf -n ExpressionEvaluator
cd ExpressionEvaluator
dotnet add package CommunityToolkit.Mvvm
dotnet run
```

Cấu hình cơ bản trong `ExpressionEvaluator.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**Chú ý:** Đây là file `.csproj` cơ bản hợp lệ. Chạy `dotnet add package CommunityToolkit.Mvvm` sau khi tạo project để NuGet thêm `PackageReference` với phiên bản tương thích thực tế; tài liệu không cố định số phiên bản package.

## 7. Tiêu chí hoàn thành (Definition of Done)

- Chạy được ứng dụng WPF tiếng Việt với giao diện sáng hai cột.
- Phân tích và tính đúng biểu thức hợp lệ; xử lý đúng `+/-` một ngôi, ưu tiên toán tử, kết hợp trái.
- Có Parse Tree đúng CFG, ẩn `ε`, có cuộn và highlight theo mỗi bước.
- Next/Previous có thể quay lui và tiến tới mà không tính lại; kết quả cuối vẫn hiển thị liên tục.
- Phân biệt lỗi đầu vào, từ vựng, cú pháp và tính toán; giữ trace nếu lỗi tính toán.
- Giữ kết quả cũ khi sửa input và ghi nhãn rõ kết quả cũ cho đến lần Calculate mới.
- Đạt bộ kiểm thử thủ công trong `TEST_PLAN.md`.
- Không phát sinh chức năng nằm ngoài phạm vi đã chốt.

## 8. Tài liệu kỹ thuật chính thức để tra cứu khi triển khai

- [What's new in WPF for .NET 10 — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net100)
- [MVVM Toolkit source generators — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/overview)
- [RelayCommand attribute — MicrosoftDocs/CommunityToolkit](https://github.com/MicrosoftDocs/CommunityToolkit/blob/main/docs/mvvm/generators/RelayCommand.md)

> Tài liệu này là **bản thiết kế và kế hoạch triển khai**, chưa phải mã nguồn ứng dụng hoạt động.
