# MANUAL_TEST_CASES — Ca kiểm thử thủ công bản refactor E/T/F

Tài liệu này chuyển 80 ID trong [`TEST_PLAN.md`](TEST_PLAN.md) thành thao tác và tiêu chí quan sát trên ứng dụng WPF. **Không phải bằng chứng đã chạy test thủ công.** Bộ xUnit trong `Tests/` kiểm tra logic độc lập; chỉ chấm Pass các ca giao diện/cấu trúc cây sau khi trực tiếp quan sát ứng dụng E/T/F. Ghi Actual Result, ngày, môi trường, ảnh/bước tái hiện nếu Fail và Pass/Fail riêng từng ID vào [`TEST_RESULTS.md`](TEST_RESULTS.md); giữ **Chưa chạy** nếu chưa trực tiếp quan sát.

## Chuẩn bị và quy tắc chung

1. Trên Windows, mở ứng dụng WPF bằng `dotnet run -c Release` tại thư mục project. Khởi động lại trước mỗi nhóm EV → ER → EE → UI, hoặc theo tiền điều kiện của ca.
2. Với mỗi ca EV/ER/EE độc lập: chọn hết nội dung TextBox **Biểu thức**, nhập/dán đúng input, nhấn **Tính toán** (trừ ca yêu cầu Enter). Quan sát ngay kết quả/lỗi/cây và chỉ số `Bước 0/N`; nhấn **Tiếp** qua từng bước để kiểm tra trace và highlight. Đối chiếu `N`, thứ tự, không chỉ kết quả cuối.
3. Ca thành công phải có cây và kết quả ngay, không có lỗi. Input/Lexical/Syntax Error không có cây/trace/kết quả của lần Submit mới hoặc lần cũ. Evaluation Error hiển thị lỗi ngay, **vẫn có cây**, không có giá trị kết quả hợp lệ, có thể duyệt đến **Error step cuối**, không có bước sau lỗi.
4. Vị trí lỗi hiển thị **one-based**; EOF là `input.Length + 1` và caret ở cuối. Kết quả hiển thị dấu `.` và tối đa 6 chữ số lẻ, làm tròn AwayFromZero; input **không** nhận dấu `.`. Dấu `→` trong bảng dưới chỉ thứ tự quan sát, không phải văn bản UI bắt buộc. Có thể khác khoảng trắng/dấu ngoặc trong trace, nhưng phép tính và thứ tự phải tương đương.
5. Với ca lỗi, có thể tính `3+5*2` trước rồi Submit input lỗi để xác nhận không còn dữ liệu cũ; không tính thao tác chuẩn bị này là một ID mới. Không chấm Pass cho ca chưa thực hiện trên WPF, kể cả khi build thành công.

## EV — Biểu thức hợp lệ (28 ca)

**Mỗi dòng:** nhập Input → **Tính toán** → xác nhận kết quả, cây và `Bước 0/N` → bấm **Tiếp** đúng N lần và kiểm tra từng bước theo thứ tự. Ở kết quả có làm tròn, so sánh giá trị *hiển thị*, không dùng số hiển thị đã làm tròn để tính lại bước kế tiếp.

| ID | Input | Kết quả | N | Thứ tự bước / điểm cần quan sát |
|---|---|---:|---:|---|
| EV-01 | `3+5*2` | `13` | 2 | `5*2=10` → `3+10=13` |
| EV-02 | `(3+5)*2` | `16` | 2 | `3+5=8` → `8*2=16` |
| EV-03 | `10-3-2` | `5` | 2 | `10-3=7` → `7-2=5`; không tính thành `9` |
| EV-04 | `8/4/2` | `1` | 2 | `8/4=2` → `2/2=1`; không tính thành `4` |
| EV-05 | `7/2` | `3.5` | 1 | `7/2=3.5` |
| EV-06 | `1/3` | `0.333333` | 1 | Một bước chia, tối đa 6 chữ số lẻ |
| EV-07 | `10/6` | `1.666667` | 1 | Một bước chia, làm tròn lên |
| EV-08 | `8/4` | `2` | 1 | `8/4=2`, không hiện `.000000` |
| EV-09 | `-5+2` | `-3` | 2 | `-(5)=-5` → `-5+2=-3` |
| EV-10 | `3*-2` | `-6` | 2 | `-(2)=-2` → `3*(-2)=-6` |
| EV-11 | `--5` | `5` | 2 | `-(5)=-5` → `-(-5)=5` |
| EV-12 | `+5` | `5` | 1 | `+(5)=5`; unary plus vẫn tạo bước |
| EV-13 | `-(2+3)*4` | `-20` | 3 | `2+3=5` → `-(5)=-5` → `-5*4=-20` |
| EV-14 | `+(-4)` | `-4` | 2 | `-(4)=-4` → `+(-4)=-4` |
| EV-15 | `42` | `42` | 0 | Không có phép tính; hai nút Trước/Tiếp disabled |
| EV-16 | `0` | `0` | 0 | Không có phép tính; hai nút Trước/Tiếp disabled |
| EV-17 | `00012+0003` | `15` | 1 | `12+3=15`; terminal trên cây vẫn giữ lexeme `00012`, `0003` |
| EV-18 | `3 + 5 * ( 2 - 1 )` | `8` | 3 | `2-1=1` → `5*1=5` → `3+5=8` |
| EV-19 | `5-10` | `-5` | 1 | `5-10=-5` |
| EV-20 | `1/8` | `0.125` | 1 | `1/8=0.125`, không có số 0 dư |
| EV-21 | `1/10000000` | `0` | 1 | Một bước chia; chỉ làm tròn lúc hiển thị, không thay giá trị nội bộ |
| EV-22 | `8/(2+2)` | `2` | 2 | `2+2=4` → `8/4=2` |
| EV-23 | `+-5` | `-5` | 2 | `-(5)=-5` → `+(-5)=-5` |
| EV-24 | `-0` | `0` | 1 | `-(0)=0` |
| EV-25 | `1/128` | `0.007813` | 1 | `0.0078125` làm tròn AwayFromZero |
| EV-26 | `-1/128` | `-0.007813` | 2 | `-(1)=-1` → `-1/128=-0.007813`; làm tròn giá trị âm |
| EV-27 | `2*3+6` | `12` | 2 | `2*3=6` → `6+6=12`; cây dùng đúng E/T/F |
| EV-28 | `1/10000000*10000000` | `1` | 2 | Chia hiển thị gần `0` → nhân ra `1`; **không** làm tròn giá trị nội bộ trước phép nhân |

## ER — Input, Lexical, Syntax (20 ca)

**Mỗi dòng:** nhập/dán Input → **Tính toán** → đọc nhóm lỗi, nguyên nhân, vị trí và chọn ký tự lỗi hoặc caret EOF. Với ER-01/16 là lỗi toàn input, không gán vị trí token giả. Với các ca lỗi, không được còn kết quả/cây/trace cũ; riêng ER-15/17 là ca **thành công**, phải có cây và kết quả.

| ID | Input cần nhập/dán | Kỳ vọng quan sát |
|---|---|---|
| ER-01 | Một dấu cách ` ` | Input: rỗng/toàn khoảng trắng; không có cây/trace |
| ER-02 | `3 & 4` | Lexical: `&` không hợp lệ ở vị trí **3**, chọn `&` |
| ER-03 | `3.5` | Lexical: `.` không hợp lệ ở vị trí **2**, chọn `.` |
| ER-04 | `3,5` | Lexical: `,` không hợp lệ ở vị trí **2**, chọn `,` |
| ER-05 | `3 + * 5` | Syntax: không mong đợi `*` ở vị trí **5** |
| ER-06 | `(2+3` | Syntax: thiếu `)` tại EOF vị trí **5**, caret cuối |
| ER-07 | `2(3+4)` | Syntax: thiếu `*` trước `(` ở vị trí **2**; không nhân ngầm |
| ER-08 | `(2+3)(4+1)` | Syntax: thiếu `*` trước `(` thứ hai ở vị trí **6** |
| ER-09 | `3 4` | Syntax: thiếu toán tử giữa hai số, vị trí **3** |
| ER-10 | `3+` | Syntax: thiếu toán hạng tại EOF vị trí **3** |
| ER-11 | `()` | Syntax: thiếu toán hạng trong ngoặc ở vị trí **2** |
| ER-12 | `3)` | Syntax: dấu `)` thừa ở vị trí **2** |
| ER-13 | `1e3` | Lexical: `e` không hợp lệ ở vị trí **2** |
| ER-14 | `79228162514264337593543950336` | Lexical: `NumberOutOfRange` ở vị trí **1** |
| ER-15 | `79228162514264337593543950335` | Thành công: kết quả đúng nguyên văn literal lớn nhất của `decimal`, cây có mặt, **0 bước** |
| ER-16 | 200 dấu cách + `1` | Input: bộ đếm **201/200**, báo quá 200 ký tự, không có cây |
| ER-17 | 199 dấu cách + `1` | Thành công: bộ đếm **200/200**, kết quả `1`, cây có mặt, **0 bước** |
| ER-18 | `3+@4` | Lexical: `@` không hợp lệ ở vị trí **3** |
| ER-19 | `2**3` | Syntax: `*` thứ hai không đúng vị trí **3** |
| ER-20 | `3/(2-2` | Syntax: thiếu `)` tại EOF vị trí **7**, caret cuối; **không** đánh giá thành lỗi chia 0, không có cây |

**Chuẩn bị ER-16/17:** tạo chuỗi bằng một đoạn C# nhỏ theo `TEST_PLAN.md` (`new string(' ', 200) + "1"` và `new string(' ', 199) + "1"`), dán vào TextBox **không Trim**, kiểm tra bộ đếm trước khi Submit. Nếu số đếm sai, chuẩn bị/dán lại; không chấm ca trên input khác.

## EE — Lỗi đánh giá (6 ca)

**Mỗi dòng:** Submit Input → xác nhận lỗi hiện ngay, có cây, không có kết quả số hợp lệ (`Value=null`) → nhấn **Tiếp** đến Error step cuối; kiểm tra số bước, thứ tự và highlight lỗi. Không có bước tính sau Error step.

| ID | Input | Thứ tự bước / kết quả mong đợi |
|---|---|---|
| EE-01 | `8/(4-4)` | `4-4=0` → Error `8/0`; **2 bước**, token `/` vị trí **2** |
| EE-02 | `1/0` | **1 Error step** chia 0; cây vẫn hiện |
| EE-03 | `79228162514264337593543950335+1` | **1 Error step** `ArithmeticOverflow`; cây vẫn hiện |
| EE-04 | `0/0` | **1 Error step**; không hiện `NaN`/`Infinity` |
| EE-05 | `1/(3-3)+2` | `3-3=0` → Error chia 0; **2 bước**, không có bước `+2` |
| EE-06 | `1/(2-2)` | `2-2=0` → Error chia 0; **đúng 2 bước** |

## UI — Tương tác, trạng thái, cây E/T/F (26 ca)

**Mỗi dòng:** thực hiện đúng tiền điều kiện/thao tác theo thứ tự; các ca phụ thuộc UI-01/03/04/09/12 có thể tiếp tục trên cùng cửa sổ, hoặc tái tạo trạng thái đó trước khi thực hiện. Kiểm tra cả kết quả, nút disabled, highlight và hình cây. Không dùng cây của bản CFG cũ để chấm UI-15/16/21–26.

| ID | Tiền điều kiện và thao tác cụ thể | Kỳ vọng quan sát |
|---|---|---|
| UI-01 | Mở app mới; nhập `3+5*2`; click **Tính toán** | Kết quả `13` ngay, cây xuất hiện, `Bước 0/2`, chưa highlight |
| UI-02 | Mở app mới; nhập `3+5*2`; nhấn **Enter** trong TextBox | Kết quả/cây/steps như UI-01, không cần click |
| UI-03 | Sau UI-01, click **Tiếp** một lần | `Bước 1/2`, `5*2=10`; highlight terminal `5`, `*`, `2` |
| UI-04 | Sau UI-03, click **Tiếp** lần nữa | `Bước 2/2`, `3+10=13`; highlight `3`, `+` và nguồn của `10` (`5`, `*`, `2`) |
| UI-05 | Sau UI-04, click **Trước** hai lần | Qua bước 1 rồi 0; highlight đổi rồi tắt; kết quả cuối vẫn `13` |
| UI-06 | Tại `Bước 0/2` của UI-01, quan sát/thử **Trước** | Nút disabled, index vẫn 0 |
| UI-07 | Tại `Bước 2/2` của UI-04, quan sát/thử **Tiếp** | Nút disabled, không vượt quá 2 |
| UI-08 | Mở app mới; tính `42` | Kết quả `42` ngay, `Bước 0/0`, Trước/Tiếp disabled |
| UI-09 | Tính `3+5*2`, sau đó **chỉ sửa** input thành `3+5*4` | Vẫn `13`, cây/trace cũ; có cảnh báo dữ liệu cũ của `3+5*2` |
| UI-10 | Từ UI-09, click **Tính toán** | Kết quả `23`, cây/trace mới, index 0, hết cảnh báo stale |
| UI-11 | Tính `3+5*2`, thay input bằng `3+*5`, Submit | Chỉ có Syntax Error mới; không còn result/cây/trace cũ |
| UI-12 | Submit `8/(4-4)` | Lỗi hiện ngay; có cây mới và 2 bước, Error step cuối |
| UI-13 | Từ UI-12, **Tiếp** hai lần rồi **Trước** một lần | Error step tô nổi phần liên quan; lùi về bước trừ, highlight cập nhật |
| UI-14 | Nhập biểu thức hợp lệ dài, ví dụ 60 số `1` nối bằng `+` (**119 ký tự**), Submit | Khi cây vượt viewport có scrollbar ngang/dọc; không có Zoom/Pan |
| UI-15 | Tính `3+5*2`, quan sát toàn bộ cây | Chỉ có non-terminal `E`, `T`, `F` và đủ terminal; không có `ExprTail`, `TermTail`, `ε` |
| UI-16 | Tính `3+5*2`, chọn một bước, click **Xem văn phạm**, thử sửa text, đóng dialog | Chỉ đọc; production đúng E/T/F, chú giải dẫn xuất ≠ thứ tự tính; đóng không mất cây/bước |
| UI-17 | Submit `3 & 4`, sau đó Submit `(2+3` | Lần 1 chọn `&` tại vị trí 3; lần 2 caret EOF vị trí 5; thông báo tiếng Việt |
| UI-18 | Tính `3+5*2`, **đóng hẳn app** rồi mở lại | Input trống; không còn kết quả/cây/trace/history |
| UI-19 | Tính biểu thức dài như UI-14, thu cửa sổ gần kích thước tối thiểu | Cây cuộn được, node không chồng nhau |
| UI-20 | Tính `--5+2*3`, nhấn **Tiếp** qua 4 bước | `-(5)=-5` → `-(-5)=5` → `2*3=6` → `5+6=11`; highlight unary ở hai bước đầu |
| UI-21 | Tính `2*3+6`, quan sát hình cây | Node **tròn**; terminal `2`, `*`, `3`, `+`, `6`; root `E` viền xanh; cạnh có mũi tên **cha → con** |
| UI-22 | Tính `10-3-2`, xem cây, nhấn **Tiếp** hai lần | Dẫn xuất **rẽ phải** E→T−E, đủ F/terminal; trace **fold-left** `10-3=7` → `7-2=5`; bước 2 highlight nguồn `10`, dấu `-` thứ nhất, `3` cùng dấu `-` thứ hai và `2`; có chú giải cây ≠ AST |
| UI-23 | Tính `79228162514264337593543950335`, xem node NUMBER | Hình tròn đủ đường kính chứa toàn bộ **29 chữ số**, không cắt/chồng nhãn; cuộn để xem nếu cần |
| UI-24 | Tính `1/10000000*10000000`, nhấn **Tiếp** qua 2 bước | Kết quả `1`; bước chia hiển thị gần `0` có `≈`/chú thích làm tròn; không trình bày `0*10000000=1` như đẳng thức chính xác |
| UI-25 | Tính `--5`, xem nhánh F, nhấn **Tiếp** qua 2 bước | Mỗi nhánh `F → MINUS F` có **2 con**; hai bước unary highlight đúng từng dấu; không có tail/`ε` |
| UI-26 | Tính biểu thức hợp lệ **gần 200 ký tự** với literal dài (ví dụ lặp `79228162514264337593543950335` nối `+0` đến 197 ký tự), thu nhỏ cửa sổ | Node tròn lớn không cắt nhãn, cấp/cạnh không chồng nhau, cuộn được ngang và dọc |

**Kết thúc:** ghi Actual Result và Pass/Fail cho **28 EV + 20 ER + 6 EE + 26 UI = 80 ca** vào `TEST_RESULTS.md`. Nếu còn Fail/Chưa chạy thì chưa nghiệm thu. Không suy ra Pass từ tài liệu, build hoặc kết quả test của CFG cũ.
