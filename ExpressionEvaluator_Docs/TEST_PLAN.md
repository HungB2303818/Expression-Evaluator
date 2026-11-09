# TEST_PLAN — Kiểm thử thủ công kiến trúc E/T/F

## 1. Phạm vi và phương pháp

**Kiểm thử thủ công qua ứng dụng WPF là điều kiện nghiệm thu 80 ca.** Theo yêu cầu bổ sung, có thêm bộ xUnit tại `Tests/ExpressionEvaluator.Tests.csproj` để chạy tự động phần logic EV/ER/EE và một phần trạng thái/cấu trúc cây của UI. Unit test **không thay thế** việc nhập biểu thức vào TextBox, nhấn Enter/Tính toán, quan sát kết quả, cây, từng bước, highlight và lỗi trên WPF. Chạy tự động bằng `dotnet test Tests/ExpressionEvaluator.Tests.csproj -c Release`.

**Đây là bộ ca chính thức của ứng dụng E/T/F.** Mã ứng dụng đã có parser/evaluator E/T/F, nhưng build và unit test không chứng minh cây được hiển thị đúng; phải trực tiếp kiểm tra WPF trước khi chấm các ca cấu trúc cây.

**Quy ước trong bảng:**

- Kết quả phải được trình bày với tối đa 6 chữ số thập phân.
- Với test lỗi Evaluation, cây vẫn phải xuất hiện và trace phải có **Error step cuối**.
- Với Input/Lexical/Syntax Error, **không** hiển thị cây/trace của lần Submit mới.
- Các vị trí trong kỳ vọng là **one-based**; EOF dùng `input.Length + 1`.
- Dấu `.` là dấu thập phân của kết quả được trình bày; đầu vào không chấp nhận `.`.

## 2. Kịch bản test chức năng — Kết quả đúng

| ID | Biểu thức | Kết quả mong đợi | Steps | Mục đích |
|---|---|---:|---:|---|
| EV-01 | `3+5*2` | `13` | 2 | Nhân trước cộng |
| EV-02 | `(3+5)*2` | `16` | 2 | Ngoặc đổi ưu tiên |
| EV-03 | `10-3-2` | `5` | 2 | Trừ kết hợp trái |
| EV-04 | `8/4/2` | `1` | 2 | Chia kết hợp trái |
| EV-05 | `7/2` | `3.5` | 1 | Chia thực |
| EV-06 | `1/3` | `0.333333` | 1 | Làm tròn khi hiển thị |
| EV-07 | `10/6` | `1.666667` | 1 | Làm tròn 6 chữ số |
| EV-08 | `8/4` | `2` | 1 | Bỏ số 0 cuối |
| EV-09 | `-5+2` | `-3` | 2 | Unary minus + binary plus |
| EV-10 | `3*-2` | `-6` | 2 | Unary minus sau phép nhân |
| EV-11 | `--5` | `5` | 2 | Unary lồng nhau |
| EV-12 | `+5` | `5` | 1 | Unary plus tạo step |
| EV-13 | `-(2+3)*4` | `-20` | 3 | Ngoặc, unary, nhân |
| EV-14 | `+(-4)` | `-4` | 2 | Unary trong ngoặc |
| EV-15 | `42` | `42` | 0 | Không có operator |
| EV-16 | `0` | `0` | 0 | Zero literal |
| EV-17 | `00012+0003` | `15` | 1 | Số có số 0 đầu |
| EV-18 | `3 + 5 * ( 2 - 1 )` | `8` | 3 | Whitespace và lồng ngoặc |
| EV-19 | `5-10` | `-5` | 1 | Kết quả âm |
| EV-20 | `1/8` | `0.125` | 1 | Kết quả hữu hạn, bỏ 0 dư |
| EV-21 | `1/10000000` | `0` | 1 | Giá trị rất nhỏ chỉ làm tròn lúc render |
| EV-22 | `8/(2+2)` | `2` | 2 | Đánh giá ngoặc trước phép chia |
| EV-23 | `+-5` | `-5` | 2 | Kết hợp hai unary khác dấu |
| EV-24 | `-0` | `0` | 1 | Unary zero |
| EV-25 | `1/128` | `0.007813` | 1 | Làm tròn tại chữ số 5 theo AwayFromZero |
| EV-26 | `-1/128` | `-0.007813` | 2 | Làm tròn giá trị âm và unary minus |
| EV-27 | `2*3+6` | `12` | 2 | Trace `2*3=6` rồi `6+6=12`; hình cây E/T/F |
| EV-28 | `1/10000000*10000000` | `1` | 2 | Bước chia hiển thị gần 0, **giá trị nội bộ không được làm tròn** trước nhân |

**Đối chiếu biểu thức đặc biệt:** `3 + 5 * (2 - 1)` có thứ tự `2-1`, `5*1`, `3+5`, nên **3 steps**.

## 3. Kịch bản test Lexer / Parser / Input Error

| ID | Input | Nhóm lỗi | Kỳ vọng chính |
|---|---|---|---|
| ER-01 | ` ` | Input | Biểu thức rỗng/toàn khoảng trắng; không có cây |
| ER-02 | `3 & 4` | Lexical | Ký tự `&` không hợp lệ, vị trí **3** |
| ER-03 | `3.5` | Lexical | Dấu `.` không hợp lệ, vị trí **2** |
| ER-04 | `3,5` | Lexical | Dấu `,` không hợp lệ, vị trí **2** |
| ER-05 | `3 + * 5` | Syntax | Không mong đợi `*`, vị trí **5** |
| ER-06 | `(2+3` | Syntax | Thiếu `)` tại EOF, vị trí **5** |
| ER-07 | `2(3+4)` | Syntax | Thiếu `*` trước `(`, vị trí **2** |
| ER-08 | `(2+3)(4+1)` | Syntax | Thiếu `*` trước `(` thứ hai, vị trí **6** |
| ER-09 | `3 4` | Syntax | Thiếu toán tử giữa hai số, vị trí **3** |
| ER-10 | `3+` | Syntax | Thiếu toán hạng, EOF vị trí **3** |
| ER-11 | `()` | Syntax | Thiếu toán hạng bên trong ngoặc, vị trí **2** |
| ER-12 | `3)` | Syntax | `)` thừa, vị trí **2** |
| ER-13 | `1e3` | Lexical | Ký tự `e` không hợp lệ, vị trí **2** |
| ER-14 | `79228162514264337593543950336` | Lexical | `NumberOutOfRange` tại vị trí **1** |
| ER-15 | `79228162514264337593543950335` | Thành công | Literal tối đa của `decimal`, Steps=0 |
| ER-16 | 200 khoảng trắng + `1` | Input | Dài 201 (`string.Length`), báo vượt 200 |
| ER-17 | 199 khoảng trắng + `1` | Thành công | Độ dài đúng 200, kết quả `1` |
| ER-18 | `3+@4` | Lexical | `@` không hợp lệ, vị trí **3** |
| ER-19 | `2**3` | Syntax | `*` thứ hai không đúng vị trí, vị trí **3** |
| ER-20 | `3/(2-2` | Syntax | Lỗi ngoặc **ưu tiên trước** đánh giá chia 0 |

**Gợi ý tạo input kiểm thử độ dài:** Dán `new string(' ', 199) + "1"` (200 ký tự) và `new string(' ', 200) + "1"` (201 ký tự) được sinh sẵn từ một đoạn C# nhỏ khi chuẩn bị dữ liệu; việc chạy ứng dụng vẫn kiểm thử bằng tay.

## 4. Kịch bản test Evaluation Error

| ID | Input | Kỳ vọng |
|---|---|---|
| EE-01 | `8/(4-4)` | Bước 1: `4-4=0`, bước 2: Error `8/0`; Parse Tree vẫn hiện, `Value=null`, vị trí lỗi token `/` là **2** |
| EE-02 | `1/0` | Một Error step; cây hiện; không có result hợp lệ |
| EE-03 | `79228162514264337593543950335+1` | `ArithmeticOverflow`, Error step, cây hiện |
| EE-04 | `0/0` | Một Error step, cây hiện, không hiển thị `NaN` hoặc `Infinity` |
| EE-05 | `1/(3-3)+2` | Bước tính `3-3`, bước lỗi chia 0, **không** có bước `+2` |
| EE-06 | `1/(2-2)` | Có đúng 2 steps: trừ thành công và chia lỗi |

## 5. Kịch bản test giao diện và trạng thái

| ID | Thao tác | Kỳ vọng |
|---|---|---|
| UI-01 | Nhập `3+5*2`, click **Tính toán** | `13` xuất hiện ngay, cây xuất hiện, index=0 |
| UI-02 | Nhập `3+5*2`, nhấn **Enter** | Kết quả/cây/steps tương tự UI-01 |
| UI-03 | Sau UI-01, nhấn **Tiếp** | Bước 1/2: `5*2=10`, toán tử `*` và số `5`, `2` nổi bật |
| UI-04 | Nhấn **Tiếp** lần nữa | Bước 2/2: `3+10=13`, provenance của `10` và toán tử `+` nổi bật |
| UI-05 | Từ bước 2, nhấn **Trước** rồi tiếp tục về 0 | Highlight tương ứng bước 1, rồi tất cả tắt; result cuối vẫn là `13` |
| UI-06 | Ở index=0, quan sát **Trước** | Disabled; không chuyển về index âm |
| UI-07 | Ở index=N, quan sát **Tiếp** | Disabled; không vượt N |
| UI-08 | Nhập `42`, tính | `42` hiển thị ngay; cả Trước/Tiếp disabled |
| UI-09 | Tính `3+5*2`, sửa thành `3+5*4` chưa Submit | Vẫn hiển thị `13` và cây cũ, có cảnh báo dữ liệu cũ |
| UI-10 | Từ UI-09, nhấn Tính toán | Kết quả thành `23`, cây/steps mới, index=0, hết cảnh báo stale |
| UI-11 | Tính `3+5*2`, rồi Submit `3+*5` | Chỉ hiển thị Syntax Error mới, không còn cây/trace/result cũ |
| UI-12 | Submit `8/(4-4)` | Có cây mới và bước lỗi, hiển thị lỗi ngay |
| UI-13 | Từ UI-12, Next tới Error step rồi Previous | Có thể điều hướng cả bước lỗi, highlight cập nhật |
| UI-14 | Tính biểu thức dài nhưng hợp lệ | Có scrollbar ngang/dọc khi cây vượt viewport, không Zoom/Pan |
| UI-15 | Quan sát cây `3+5*2` | Chỉ có non-terminal `E`, `T`, `F`, đủ terminal; không có `ExprTail`, `TermTail`, `ε` |
| UI-16 | Nhấn **Xem văn phạm** | Dialog chỉ đọc có đúng production `E/T/F`, chú giải dẫn xuất ≠ thứ tự tính; đóng không mất trạng thái |
| UI-17 | Có Lexical/Syntax Error | Select vị trí/token lỗi trong TextBox hoặc đặt caret EOF; có mô tả tiếng Việt |
| UI-18 | Đóng và mở ứng dụng | Không còn expression/result/history trước đó |
| UI-19 | Dùng cửa sổ đủ nhỏ để tree cần cuộn | Có scrollbar và node không chồng nhau |
| UI-20 | Nhập `--5+2*3`, duyệt bước | Tổng 4 bước, highlight unary ở các bước đầu |
| UI-21 | Tính `2*3+6`, xem cây | Node **tròn**, nhãn terminal `2`, `*`, `3`, `+`, `6`; root `E` viền xanh; cạnh có mũi tên **cha → con** |
| UI-22 | Tính `10-3-2`, xem hình và duyệt bước | Cây dẫn xuất **rẽ phải** theo E→T−E (đủ F/terminal), **trace trái** `10-3=7` → `7-2=5`; bước 2 highlight nguồn `10,-,3` cộng toán tử `-` thứ hai và `2`; có chú giải cây ≠ AST |
| UI-23 | Tính literal `79228162514264337593543950335` | Hình tròn NUMBER đủ đường kính chứa **toàn bộ 29 chữ số**, không bị cắt/chồng; cuộn để xem khi cần |
| UI-24 | Tính `1/10000000*10000000`, duyệt bước | Kết quả `1`; bước chia hiển thị gần `0` có nhãn `≈`/chú thích làm tròn, không trình bày `0*10000000=1` như đẳng thức chính xác |
| UI-25 | Tính `--5`, xem nhánh F rồi duyệt 2 bước | Nhánh `F → MINUS F` có **2 con**; hai bước unary và highlight dấu tương ứng; không có tail/ε |
| UI-26 | Tính biểu thức gần 200 ký tự với literal dài, thu nhỏ cửa sổ | Node tròn lớn không cắt nhãn, cấp/cạnh không chồng nhau, có thể cuộn ngang và dọc |

## 6. Mẫu phiếu ghi kết quả

| Test ID | Ngày | Môi trường | Actual Result | Status (Pass/Fail) | Ghi chú / ảnh lỗi |
|---|---|---|---|---|---|
| EV-01 |  | Windows / .NET 10 |  |  |  |
| ER-05 |  | Windows / .NET 10 |  |  |  |
| EE-01 |  | Windows / .NET 10 |  |  |  |
| UI-09 |  | Windows / .NET 10 |  |  |  |

Có thể sao chép bảng này khi thực thi. Khi một test lỗi, ghi input, bước tái hiện và ảnh màn hình để sửa chính xác.

## 7. Quy trình kiểm thử và tiêu chí hoàn thành

1. Khởi động lại ứng dụng để tránh trạng thái dư.
2. Chạy từng nhóm **EV**, **ER**, **EE**, **UI**.
3. Với EV, kiểm tra **giá trị, số bước và thứ tự bước**.
4. Với ER, kiểm tra **loại, nguyên nhân, vị trí** và việc không hiển thị cây.
5. Với EE, kiểm tra **trace không bị mất**, có Error step cuối, không có result hợp lệ.
6. Với UI, kiểm tra stale banner, input Enter, Next/Previous, highlight, scrollbar, Grammar Dialog.
7. Chạy thêm bộ xUnit để kiểm tra hồi quy logic; đánh dấu Pass/Fail **thủ công** riêng cho 80 ID sau khi trực tiếp quan sát WPF, sửa lỗi và chạy lại các ca liên quan.

**Nghiệm thu:** Toàn bộ **80 ca** (28 EV, 20 ER, 6 EE, 26 UI) bắt buộc được ghi Actual Result và Pass/Fail. Không coi build/smoke là Pass; không crash do input sai; không tự thay đổi những quyết định nằm ngoài phạm vi. Các ca mới UI-21..26 phải được thực hiện trên cây E/T/F, không trên bản cây cũ.

Bộ xUnit là phần mở rộng theo yêu cầu sau giai đoạn đã chốt; kết quả `dotnet test` được ghi riêng, không tự chuyển trạng thái của 80 ca WPF sang Pass.
