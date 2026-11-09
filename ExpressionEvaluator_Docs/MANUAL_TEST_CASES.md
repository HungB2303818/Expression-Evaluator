# MANUAL_TEST_CASES — Ca kiểm thử thao tác trực tiếp trên WPF

Tài liệu này là **hướng dẫn thực thi** của [`TEST_PLAN.md`](TEST_PLAN.md), không phải mã test. Theo đặc tả: **không tạo Unit Test, test project, CI hoặc script chạy test tự động**. Ghi kết quả thực tế và Pass/Fail cho từng ID trong [`TEST_RESULTS.md`](TEST_RESULTS.md); không đánh dấu Pass khi chưa quan sát ứng dụng.

## Chuẩn bị và quy tắc chung

1. Trên Windows, chạy `dotnet run -c Release` ở thư mục project. Khởi động lại app trước mỗi nhóm EV → ER → EE → UI hoặc khi cần trạng thái ban đầu.
2. Với mỗi ca EV/ER/EE độc lập: chọn toàn bộ nội dung ô **Biểu thức**, nhập/dán đúng chuỗi ở cột Input, nhấn **Tính toán** (trừ ca yêu cầu Enter). Đọc ngay kết quả/lỗi; bấm **Tiếp** lần lượt đến hết bước rồi **Trước** nếu ca yêu cầu. Đối chiếu cả số bước `Bước 0/N`, thứ tự bước, cây và nút bị vô hiệu ở hai đầu.
3. Mỗi ca thành công: có kết quả ngay, cây đầy đủ; không có thông báo lỗi. Mỗi ca Input/Lexical/Syntax Error: không còn kết quả/cây/bước của Submit trước. Mỗi ca Evaluation Error: không có kết quả hợp lệ, **vẫn có cây**, có thể duyệt đến **Error step cuối**, không có bước sau lỗi.
4. Vị trí lỗi hiển thị **one-based**; lỗi EOF hiển thị `input.Length + 1`, caret ở cuối. Không chấm Pass chỉ theo kết quả số: kiểm tra cả bước và trạng thái UI được nêu trong ca.
5. Nếu Fail, ghi **Actual Result**, ngày, môi trường, thao tác tái hiện và ảnh màn hình vào `TEST_RESULTS.md` (hoặc liên kết ảnh); sau khi sửa chạy lại ca đó và ca liên quan.

## EV — Biểu thức hợp lệ

Thao tác cho mỗi dòng: nhập Input → **Tính toán** → xác nhận kết quả và `Bước 0/N` → bấm **Tiếp** N lần, đọc bước theo thứ tự. Dấu `→` là **bước kế tiếp**, không phải ký tự phải xuất hiện trong UI; mô tả bước có thể có dấu ngoặc hoặc khoảng trắng khác nhưng toán hạng/kết quả phải tương đương.

| ID | Input | Kết quả | N | Thứ tự bước cần quan sát |
|---|---|---:|---:|---|
| EV-01 | `3+5*2` | `13` | 2 | `5*2=10` → `3+10=13` |
| EV-02 | `(3+5)*2` | `16` | 2 | `3+5=8` → `8*2=16` |
| EV-03 | `10-3-2` | `5` | 2 | `10-3=7` → `7-2=5` (không phải 9) |
| EV-04 | `8/4/2` | `1` | 2 | `8/4=2` → `2/2=1` (không phải 4) |
| EV-05 | `7/2` | `3.5` | 1 | `7/2=3.5` |
| EV-06 | `1/3` | `0.333333` | 1 | `1/3` hiển thị 6 chữ số lẻ |
| EV-07 | `10/6` | `1.666667` | 1 | `10/6` làm tròn lên ở chữ số thứ 6 |
| EV-08 | `8/4` | `2` | 1 | `8/4=2`, không hiện `.000000` |
| EV-09 | `-5+2` | `-3` | 2 | `-(5)=-5` → `-5+2=-3` |
| EV-10 | `3*-2` | `-6` | 2 | `-(2)=-2` → `3*-2=-6` |
| EV-11 | `--5` | `5` | 2 | `-(5)=-5` → `-(-5)=5` |
| EV-12 | `+5` | `5` | 1 | `+(5)=5` (unary plus vẫn tạo bước) |
| EV-13 | `-(2+3)*4` | `-20` | 3 | `2+3=5` → `-(5)=-5` → `-5*4=-20` |
| EV-14 | `+(-4)` | `-4` | 2 | `-(4)=-4` → `+(-4)=-4` |
| EV-15 | `42` | `42` | 0 | Không có step; Trước/Tiếp đều disabled |
| EV-16 | `0` | `0` | 0 | Không có step; Trước/Tiếp đều disabled |
| EV-17 | `00012+0003` | `15` | 1 | Giá trị toán hạng là `12+3=15`; cây giữ lexeme có số 0 đầu |
| EV-18 | `3 + 5 * ( 2 - 1 )` | `8` | 3 | `2-1=1` → `5*1=5` → `3+5=8` |
| EV-19 | `5-10` | `-5` | 1 | `5-10=-5` |
| EV-20 | `1/8` | `0.125` | 1 | `1/8=0.125`, không có số 0 dư |
| EV-21 | `1/10000000` | `0` | 1 | Có 1 bước chia; giá trị chỉ bị làm tròn khi hiển thị |
| EV-22 | `8/(2+2)` | `2` | 2 | `2+2=4` → `8/4=2` |
| EV-23 | `+-5` | `-5` | 2 | `-(5)=-5` → `+(-5)=-5` |
| EV-24 | `-0` | `0` | 1 | `-(0)=0` |
| EV-25 | `1/128` | `0.007813` | 1 | Làm tròn `0.0078125` theo AwayFromZero |
| EV-26 | `-1/128` | `-0.007813` | 2 | `-(1)=-1` → `-1/128`, làm tròn số âm |

**Kiểm tra phụ EV-21 (không thêm ID):** nhập `1/10000000*10000000`, xác nhận kết quả cuối là `1` dù bước chia trung gian hiển thị `0`. Nếu kết quả `0`, evaluator đã làm tròn giữa các phép tính → EV-21 Fail. Với các bước có số trung gian bị làm tròn, số in ra chỉ là dạng hiển thị; không dùng nó làm đầu vào tính lại.

## ER — Input, lexical, syntax

Thao tác cho mỗi dòng: nhập Input → **Tính toán** → đọc nhóm lỗi/thông báo/vị trí và quan sát TextBox. Trước khi chạy một ca lỗi, có thể tính `3+5*2` rồi Submit ca lỗi để xác nhận **cây/kết quả/steps cũ biến mất**. Ngoại lệ: ER-15/17 là ca thành công, phải có cây và kết quả.

| ID | Input cần nhập/dán | Nhóm; kết quả; vị trí mong đợi |
|---|---|---|
| ER-01 | một dấu cách ` ` | Input: rỗng/toàn khoảng trắng; không có cây/trace |
| ER-02 | `3 & 4` | Lexical: `&` không hợp lệ; vị trí **3**, chọn `&` |
| ER-03 | `3.5` | Lexical: `.` không hợp lệ; vị trí **2**, chọn `.` |
| ER-04 | `3,5` | Lexical: `,` không hợp lệ; vị trí **2**, chọn `,` |
| ER-05 | `3 + * 5` | Syntax: thiếu toán hạng trước `*`; vị trí **5** |
| ER-06 | `(2+3` | Syntax: thiếu `)`; EOF vị trí **5**, caret cuối |
| ER-07 | `2(3+4)` | Syntax: thiếu toán tử `*` trước `(`; vị trí **2**; không nhân ngầm |
| ER-08 | `(2+3)(4+1)` | Syntax: thiếu `*` trước `(` thứ hai; vị trí **6** |
| ER-09 | `3 4` | Syntax: thiếu toán tử giữa hai số; vị trí **3** |
| ER-10 | `3+` | Syntax: thiếu toán hạng; EOF vị trí **3** |
| ER-11 | `()` | Syntax: thiếu toán hạng trong ngoặc; vị trí **2** |
| ER-12 | `3)` | Syntax: `)` thừa; vị trí **2** |
| ER-13 | `1e3` | Lexical: `e` không hợp lệ; vị trí **2** |
| ER-14 | `79228162514264337593543950336` | Lexical: số vượt phạm vi `decimal`; vị trí **1** |
| ER-15 | `79228162514264337593543950335` | Thành công: kết quả chính xác số tối đa `decimal`, **0 bước** |
| ER-16 | đúng 200 dấu cách rồi `1` | Input: bộ đếm **201/200**, báo dài quá 200, không có cây |
| ER-17 | đúng 199 dấu cách rồi `1` | Thành công: bộ đếm **200/200**, kết quả `1`, **0 bước** |
| ER-18 | `3+@4` | Lexical: `@` không hợp lệ; vị trí **3** |
| ER-19 | `2**3` | Syntax: dấu `*` thứ hai sai vị trí; vị trí **3** |
| ER-20 | `3/(2-2` | Syntax: thiếu `)` tại EOF (**7**); **không** báo chia 0, không cây |

**Chuẩn bị ER-16/17:** tạo chuỗi 200/199 dấu cách và `1` theo hướng dẫn trong `TEST_PLAN.md`, dán vào TextBox **không Trim**, đối chiếu bộ đếm ký tự trước khi Submit. Nếu số đếm sai, tạo/dán lại input rồi mới chấm ca. Không gán vị trí token giả cho lỗi toàn chuỗi ER-01/16.

## EE — Lỗi đánh giá

Thao tác cho mỗi dòng: nhập Input → **Tính toán**; lỗi phải xuất hiện ngay, không cần Next; kiểm tra còn cây, **không có kết quả số**. Bấm **Tiếp** tới Error step cuối, đối chiếu thứ tự/độ dài trace và màu highlight lỗi.

| ID | Input | Thứ tự bước / kết quả mong đợi |
|---|---|---|
| EE-01 | `8/(4-4)` | `4-4=0` → Error `8/0`; **2 bước**, dấu `/` vị trí **2** |
| EE-02 | `1/0` | **1 bước** Error chia 0; vẫn có cây |
| EE-03 | `79228162514264337593543950335+1` | **1 bước** Error `ArithmeticOverflow`; vẫn có cây |
| EE-04 | `0/0` | **1 bước** Error; không hiện `NaN` hay `Infinity` |
| EE-05 | `1/(3-3)+2` | `3-3=0` → Error chia 0; **2 bước**, không có bước `+2` |
| EE-06 | `1/(2-2)` | `2-2=0` → Error chia 0; **đúng 2 bước** |

## UI — Tương tác, trạng thái, cây và văn phạm

Chạy theo chuỗi tiền điều kiện trong cột Thao tác; ca yêu cầu trạng thái sạch thì **đóng và mở lại app**. Quan sát cả nút disabled, highlight và biểu thức đang hiển thị khi đánh dấu Pass.

| ID | Thao tác cụ thể | Cần quan sát |
|---|---|---|
| UI-01 | Mở app mới; nhập `3+5*2`; click **Tính toán** | Kết quả `13` ngay, cây xuất hiện, `Bước 0/2`, chưa highlight |
| UI-02 | Mở app mới; nhập `3+5*2`; nhấn **Enter** trong TextBox | Như UI-01, không cần click |
| UI-03 | Sau UI-01, click **Tiếp** một lần | `Bước 1/2`, `5*2=10`; highlight `5`, `*`, `2` |
| UI-04 | Sau UI-03, click **Tiếp** lần nữa | `Bước 2/2`, `3+10=13`; highlight `3`, `+` và nguồn `5`, `*`, `2` |
| UI-05 | Sau UI-04, click **Trước** hai lần | Qua bước 1 rồi 0; highlight đổi rồi tắt; kết quả luôn `13` |
| UI-06 | Tại `Bước 0/2` của UI-01, quan sát/click **Trước** | Nút disabled; vẫn 0, không có index âm |
| UI-07 | Tại `Bước 2/2` của UI-04, quan sát/click **Tiếp** | Nút disabled; không quá 2 |
| UI-08 | Mở app mới; tính `42` | Kết quả `42` ngay, `Bước 0/0`; cả hai nút disabled |
| UI-09 | Tính `3+5*2`, sau đó **chỉ sửa** thành `3+5*4` | Vẫn `13`, vẫn cây/trace cũ; cảnh báo chứa biểu thức `3+5*2` |
| UI-10 | Từ UI-09, click **Tính toán** | `23`, cây/trace mới, index 0, hết cảnh báo stale |
| UI-11 | Tính `3+5*2`, sau đó thay input bằng `3+*5`, Submit | Chỉ còn lỗi Syntax; kết quả/cây/trace cũ biến mất |
| UI-12 | Submit `8/(4-4)` | Lỗi hiện ngay, có cây mới và 2 bước kể cả Error cuối |
| UI-13 | Từ UI-12, Tiếp hai lần rồi Trước một lần | Đến Error step tô đỏ node liên quan; lùi về bước trừ, highlight đổi |
| UI-14 | Nhập biểu thức hợp lệ dài, ví dụ 60 số `1` nối bằng `+`; Submit | Có scrollbar ngang/dọc khi cây vượt vùng; không có Zoom/Pan |
| UI-15 | Tính `3+5*2`, quan sát cây | Có `Expr`, `Term`, `ExprTail`, `TermTail`, `Unary`, `Primary` và terminal; không hiện node `ε` |
| UI-16 | Tính `3+5*2`, chọn một bước, click **Xem văn phạm**, thử sửa text, đóng | Dialog chỉ đọc, đủ 6 production và `ε`; trạng thái cây/bước không đổi |
| UI-17 | Submit `3 & 4`, sau đó Submit `(2+3` | Lần 1 chọn `&` và báo vị trí 3; lần 2 caret EOF và báo vị trí 5, thông báo tiếng Việt |
| UI-18 | Tính `3+5*2`, **đóng hẳn app** rồi mở lại | Input trống; không còn kết quả, cây, trace hoặc history |
| UI-19 | Tính biểu thức dài như UI-14; thu cửa sổ về gần kích thước tối thiểu | Cây vẫn cuộn được; node không chồng nhau; hai cột còn hoạt động |
| UI-20 | Tính `--5+2*3`, Tiếp qua 4 bước | Unary âm hai bước đầu, `2*3=6` bước 3, `5+6=11` bước 4; highlight tương ứng |

**Kết thúc:** sau khi chạy **tất cả 72 ca**, điền Actual Result và Pass/Fail từng ID trong `TEST_RESULTS.md`. Nếu còn một ca Chưa chạy hoặc Fail thì **chưa nghiệm thu** theo `TEST_PLAN.md`.
