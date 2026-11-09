# REQUIREMENTS — Đặc tả yêu cầu phần mềm (SRS)

## 1. Tổng quan
**Sản phẩm:** Expression Evaluator — công cụ Windows desktop minh họa CFG (E, T, F) và Recursive Descent Parser.
**Người dùng:** Người học nguyên lý ngôn ngữ lập trình, CFG, phân tích cú pháp và quá trình đánh giá biểu thức.
**Mục tiêu:** Nhập biểu thức số học, tính toán với decimal, quan sát cây phân tích trực quan (E, T, F với các nút hình tròn, mũi tên) và duyệt từng bước tính toán.

## 2. Yêu cầu chức năng

### 2.1. Nhập và xác nhận biểu thức
| ID | Yêu cầu |
| :--- | :--- |
| FR-01 | Nhập/chỉnh sửa/sao chép biểu thức qua TextBox trên cửa sổ chính. |
| FR-02 | Nhấn **Enter** hoặc **Tính toán** để chạy cùng CalculateCommand. |
| FR-03 | Chỉ phân tích và tính toán khi có yêu cầu từ FR-02; không parse theo từng lần gõ. |
| FR-04 | Giới hạn đầu vào **200 đơn vị mã UTF-16 (string.Length)**, kể cả khoảng trắng; kiểm tra tại thời điểm Calculate. |
| FR-05 | Chấp nhận khoảng trắng (`char.IsWhiteSpace`) ở giữa/trước/sau token. |
| FR-06 | Số đầu vào chỉ gồm 0–9; không cho dấu thập phân hoặc ký pháp khoa học. |

### 2.2. Phân tích cú pháp CFG (E, T, F)
| ID | Yêu cầu |
| :--- | :--- |
| FR-07 | Hỗ trợ `+`, `-`, `*`, `/`, cặp ngoặc `( )` và toán tử một ngôi `+`, `-`. |
| FR-08 | Chỉ hỗ trợ phép nhân tường minh `*`; biểu thức `2(3+4)` phải báo lỗi cú pháp. |
| FR-09 | CFG cố định theo chuẩn rút gọn `E, T, F` (quy định tại GRAMMAR.md); không cho sửa grammar ở giao diện. |
| FR-10 | Lexer trả danh sách token với vị trí và độ dài trong chuỗi nguồn (`SourceSpan`). |
| FR-11 | Recursive Descent Parser tự cài đặt bằng các hàm `ParseE()`, `ParseT()`, `ParseF()`. |
| FR-12 | Phân tích đúng ưu tiên: `F` (số/ngoặc/unary) > `T` (nhân/chia) > `E` (cộng/trừ). Các toán tử hai ngôi cùng mức **đánh giá** kết hợp trái bằng fold-left, dù cây dẫn xuất rẽ phải. |
| FR-13 | Sinh một **cây dẫn xuất CFG E/T/F** duy nhất; không sinh AST; không sử dụng các nút trung gian `ExprTail`, `TermTail` hay nút rỗng `ε`. Cây không được diễn giải là cây nhóm phép tính. |
| FR-14 | Chỉ có Parse Tree mới nếu toàn bộ cú pháp hợp lệ; lỗi từ vựng/cú pháp không được hiển thị cây dở dang. |

### 2.3. Đánh giá biểu thức
| ID | Yêu cầu |
| :--- | :--- |
| FR-15 | Tính toán bằng `decimal`, không dùng phép chia nguyên. |
| FR-16 | Mọi toán tử một ngôi và hai ngôi đều tạo **một EvaluationStep**. Dấu ngoặc không tạo bước riêng. |
| FR-17 | Hiển thị kết quả cuối ngay sau khi Tính toán thành công. |
| FR-18 | Hiển thị tối đa 6 chữ số sau dấu thập phân, làm tròn `MidpointRounding.AwayFromZero`, bỏ số 0 dư. Chỉ làm tròn khi hiển thị. |
| FR-19 | Với lỗi chia cho 0 hoặc tràn số khi đánh giá: không có giá trị cuối hợp lệ; vẫn giữ cây và các bước tính thành công cùng bước lỗi cuối. |

### 2.4. Trực quan hóa và thao tác
| ID | Yêu cầu |
| :--- | :--- |
| FR-20 | Bố cục hai cột: Cây phân tích cú pháp bên trái, Điều khiển và danh sách bước tính bên phải. |
| FR-21 | Cây phân tích tĩnh, tự bố trí, cuộn ngang/dọc; các nút hình tròn **đủ đường kính chứa toàn bộ nhãn**, cạnh có đầu mũi tên từ cha xuống con, không đè/chồng chữ hoặc node. |
| FR-22 | Hiển thị trực tiếp các nhãn `E`, `T`, `F` và các token terminal (`+`, `*`, `2`...); nút gốc `E` có màu viền nổi bật. |
| FR-23 | Nút Trước / Tiếp chỉ duyệt các bước tính toán (`EvaluationStep`), không duyệt production của CFG. |
| FR-24 | Mỗi bước làm nổi bật **các terminal** của toán tử và toán hạng nguồn (provenance) trên cây. Giá trị trung gian có thể trải qua nhiều nhánh, không đòi tồn tại một subtree tương ứng; dấu hiệu gốc E không được mất khi highlight. |
| FR-25 | Chuyển bước không parse/tính lại biểu thức, không sửa cây, không đổi kết quả cuối. |
| FR-26 | Nút Xem văn phạm mở Dialog chỉ đọc hiển thị luật sinh `E, T, F`. |

### 2.5. Thông báo lỗi và trạng thái
| ID | Yêu cầu |
| :--- | :--- |
| FR-27 | Thông báo rõ loại lỗi, nguyên nhân, vị trí hiển thị bắt đầu từ 1 (one-based). |
| FR-28 | Đánh dấu vùng lỗi trong TextBox; với lỗi EOF chỉ rõ vị trí cuối chuỗi (`length + 1`). |
| FR-29 | Khi chỉnh sửa TextBox mà chưa nhấn Tính toán, giữ nguyên kết quả/cây cũ và hiện cảnh báo dữ liệu cũ (stale). |
| FR-30 | Khi Tính toán lại, thay thế toàn bộ trạng thái cũ bằng kết quả mới; không lưu lịch sử. |
| FR-31 | Không lưu trữ biểu thức xuống đĩa hay cơ sở dữ liệu. |

## 3. Phân loại lỗi
* **Input Error:** Rỗng, toàn khoảng trắng hoặc dài hơn 200 ký tự.
* **Lexical Error:** Ký tự lạ (ví dụ `&`, `.`), số vượt phạm vi `decimal`.
* **Syntax Error:** `3 + * 5`, `2(3+4)`, thiếu ngoặc `)`.
* **Evaluation Error:** Chia cho 0 (`8/(4-4)`), tràn số khi tính toán. Parse Tree và các bước đã tính được giữ lại, kết thúc bằng một bước Error.

## 4. Bất biến nghiệm thu và phạm vi

* Input/Lexical/Syntax failure → ParseTree=null, Evaluation=null, Error thuộc nhóm tương ứng; không giữ kết quả cũ sau Submit. Evaluation failure → cây còn, Value=null, trace kết thúc bằng Error step; lỗi hiện ngay. Success → cây và Value có, Error=null.
* `SourceSpan.StartIndex` zero-based, vị trí UI one-based; EOF ở `input.Length+1`. Giới hạn 200 tính **cả khoảng trắng**, không chặn cứng việc gõ, chỉ kiểm tra khi Submit.
* `10-3-2` phải có 2 bước `10-3=7` rồi `7-2=5`; `8/4/2` phải có 2 bước `8/4=2` rồi `2/2=1`. Cây CFG cho hai chuỗi này vẫn rẽ phải: đó là dẫn xuất, **không phải** sơ đồ thứ tự nhóm phép toán. Giao diện phải có chú giải phân biệt.
* Ngoài phạm vi: AST, lịch sử/lưu xuống đĩa, sửa CFG, số thập phân đầu vào, phép nhân ngầm, zoom/pan và test tự động. Kiểm thử thủ công bắt buộc gồm đủ nhóm **EV, ER, EE, UI** theo `TEST_PLAN.md`; build thành công không thay thế các ca này.
