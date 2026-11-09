# REQUIREMENTS — Đặc tả yêu cầu phần mềm (SRS)

## 1. Tổng quan

**Sản phẩm:** Expression Evaluator — công cụ Windows desktop minh họa CFG/Recursive Descent Parser.  
**Người dùng:** Người học nguyên lý ngôn ngữ lập trình, CFG, phân tích cú pháp và quá trình đánh giá biểu thức.  
**Mục tiêu:** Nhập biểu thức, xem ngay kết quả sau yêu cầu tính, quan sát Parse Tree và từng bước tính.

## 2. Yêu cầu chức năng

### 2.1. Nhập và xác nhận biểu thức

| ID | Yêu cầu |
|---|---|
| FR-01 | Nhập/chỉnh sửa/sao chép biểu thức qua TextBox trên cửa sổ chính. |
| FR-02 | Nhấn **Enter** hoặc **Tính toán** để chạy cùng `CalculateCommand`. |
| FR-03 | Chỉ phân tích và tính toán khi có yêu cầu từ FR-02; không parse theo từng lần gõ. |
| FR-04 | Giới hạn đầu vào **200 đơn vị mã UTF-16 (`string.Length`)**, kể cả khoảng trắng; kiểm tra tại thời điểm Calculate. Có thể hiển thị bộ đếm. |
| FR-05 | Chấp nhận khoảng trắng (`char.IsWhiteSpace`) ở giữa/trước/sau token. |
| FR-06 | Số đầu vào chỉ gồm `0–9`, một hoặc nhiều chữ số; không cho dấu thập phân hoặc ký pháp khoa học. |

### 2.2. Phân tích CFG

| ID | Yêu cầu |
|---|---|
| FR-07 | Hỗ trợ `+`, `-`, `*`, `/`, cặp ngoặc `(` `)` và `+`/`-` một ngôi. |
| FR-08 | Chỉ hỗ trợ phép nhân tường minh `*`; `2(3+4)` phải báo lỗi cú pháp. |
| FR-09 | CFG cố định theo `GRAMMAR.md`; không cho sửa grammar ở giao diện. |
| FR-10 | Lexer trả danh sách token với vị trí và độ dài trong chuỗi nguồn. |
| FR-11 | Recursive Descent Parser tự cài đặt, sử dụng các phương thức tương ứng non-terminal. |
| FR-12 | Phân tích đúng ưu tiên: ngoặc > unary > nhân/chia > cộng/trừ; toán tử hai ngôi cùng mức tính kết hợp trái. |
| FR-13 | Sinh **một Parse Tree**; không sinh AST. |
| FR-14 | Chỉ có Parse Tree của lần Submit mới nếu toàn bộ cú pháp hợp lệ; lỗi từ vựng/cú pháp không được hiển thị cây dở dang. |

### 2.3. Đánh giá biểu thức

| ID | Yêu cầu |
|---|---|
| FR-15 | Tính với `decimal`, không dùng phép chia nguyên. |
| FR-16 | Mọi toán tử một ngôi và hai ngôi đều tạo **một EvaluationStep**. Dấu ngoặc không tạo bước. |
| FR-17 | Hiển thị kết quả cuối **ngay** sau Calculate thành công. |
| FR-18 | Hiển thị tối đa **6 chữ số sau dấu thập phân**, làm tròn `MidpointRounding.AwayFromZero`, bỏ số 0 dư. Chỉ làm tròn khi hiển thị. |
| FR-19 | Với lỗi chia cho 0 hoặc tràn số khi đánh giá: không công bố giá trị cuối hợp lệ; vẫn giữ cây và các bước đã thành công cùng bước lỗi. |

### 2.4. Trực quan hóa và thao tác

| ID | Yêu cầu |
|---|---|
| FR-20 | Bố cục hai cột: Parse Tree trái, điều khiển tính toán phải; khu vực nhập/kết quả ở trên. |
| FR-21 | Parse Tree tĩnh, tự bố trí, hỗ trợ cuộn ngang và dọc; không Zoom, Pan, Fit. |
| FR-22 | Giữ ký hiệu terminal/non-terminal của CFG trên cây; **ẩn nút `ε`** ở giao diện. |
| FR-23 | Next/Previous chỉ duyệt các **bước tính**, không duyệt production của CFG. |
| FR-24 | Mỗi bước làm nổi bật toán tử và các thành phần (toán hạng nguồn) tham gia phép tính, bao gồm nguồn tạo ra giá trị trung gian. |
| FR-25 | Chuyển bước không parse/tính lại biểu thức, không sửa Parse Tree, không thay đổi kết quả cuối. |
| FR-26 | Nút **Xem văn phạm** mở Dialog chỉ đọc, chứa đầy đủ CFG và mô tả ký hiệu. |

### 2.5. Thông báo lỗi, trạng thái

| ID | Yêu cầu |
|---|---|
| FR-27 | Thông báo cụ thể loại lỗi, nguyên nhân, vị trí (với lỗi có thể định vị); vị trí hiển thị bắt đầu từ **1**. |
| FR-28 | Dùng đánh dấu vị trí trong TextBox cho lỗi ký tự/số/toán tử; với EOF sử dụng vị trí `length+1` và mô tả rõ. |
| FR-29 | Khi chỉ **chỉnh sửa** input, giữ phân tích/cây/kết quả/trace trước đó, gắn thông báo **Kết quả thuộc biểu thức trước**. |
| FR-30 | Khi Calculate lại, kết quả trước được thay bằng **trạng thái của lần phân tích mới**, kể cả lỗi; không có lịch sử đa biểu thức. |
| FR-31 | Không lưu biểu thức, trace hay cấu hình xuống đĩa; dữ liệu mất khi đóng ứng dụng. |

## 3. Phân loại lỗi

| Nhóm | Ví dụ | Phản ứng |
|---|---|---|
| Input Error | Rỗng/toàn khoảng trắng; dài hơn 200 | Không gọi parser; thông báo cụ thể; xóa phân tích cũ sau Submit |
| Lexical Error | `3 & 4`, `3.5`, số nguyên vượt `decimal` | Không tạo cây mới; chỉ vị trí token/ký tự |
| Syntax Error | `3 + * 5`, `2(3+4)`, thiếu `)` | Không tạo cây mới; chỉ vị trí lỗi (hoặc EOF) |
| Evaluation Error | `8/(4-4)`, tràn số khi cộng/nhân | Giữ Parse Tree và trace gồm bước lỗi; không có `Value` hợp lệ |

**Quy ước chỉ số:** `SourceSpan.StartIndex` lưu **0-based** phục vụ C#; giao diện hiển thị `StartIndex + 1`. Với EOF, `StartIndex = expression.Length`; vị trí người dùng thấy là `Length + 1`. Khi lỗi không gắn với một token (ví dụ rỗng), dùng thông báo vùng nhập thay vì mũi tên ký tự giả.

## 4. Use case

### UC-01 — Tính thành công

**Tiền điều kiện:** Ứng dụng đã mở.  
**Luồng chính:** (1) nhập biểu thức → (2) Enter/Calculate → (3) kiểm tra độ dài/rỗng → (4) tokenize → (5) parse toàn bộ → (6) evaluate tạo trace → (7) cập nhật result/cây/trace → (8) `CurrentStepIndex = 0`.  
**Hậu điều kiện:** Có result hợp lệ và Parse Tree; Next bật nếu có ít nhất một bước.

### UC-02 — Duyệt bước

**Tiền điều kiện:** Đã có trace với ít nhất một bước.  
**Luồng:** Next chuyển `i → i+1`; Previous chuyển `i → i-1`; highlight theo bước hiện tại; index 0 không highlight.  
**Hậu điều kiện:** Không thay đổi result, tree, trace.

### UC-03 — Sửa mà chưa tính lại

**Luồng:** Người dùng sửa TextBox; `InputExpression != LastAnalyzedExpression` → bật cờ stale; hiển thị thông báo và biểu thức của kết quả đang hiển thị. Có thể tiếp tục Next/Previous trace cũ.  
**Hậu điều kiện:** Không tự động phân tích input mới.

### UC-04 — Lỗi cú pháp hoặc từ vựng

**Luồng:** Submit biểu thức mới sai → hiển thị lỗi loại/vị trí/giải thích; xóa result/cây/trace của lần Submit trước.  
**Hậu điều kiện:** Không có cây chưa hoàn chỉnh; stale = false vì input vừa được phân tích.

### UC-05 — Lỗi tính toán

**Luồng:** Submit biểu thức đúng cú pháp nhưng gây lỗi khi tính → hiển thị cây; trace kết thúc bằng `EvaluationStepType.Error`; cho phép Next/Previous đến bước lỗi.  
**Hậu điều kiện:** `Value = null`, có `Error` và trace.

### UC-06 — Xem CFG

Nhấn **Xem văn phạm** → mở dialog read-only chứa grammar đã chốt → đóng dialog quay về trạng thái cũ, không tác động quá trình tính.

## 5. Các trạng thái hiển thị

| Trạng thái | Result | Parse Tree | Steps | Cảnh báo |
|---|---|---|---|---|
| Mở app chưa Submit | Trống | Trống | Trống | Không |
| Submit hợp lệ | Có ngay | Có | Có thể rỗng nếu chỉ có số | Không |
| Submit lỗi input/lexical/syntax | Trống | Trống | Trống | Chi tiết lỗi |
| Submit lỗi evaluation | Không có giá trị hợp lệ | Có | Có cả bước lỗi | Chi tiết lỗi |
| Đang sửa sau bất kỳ Submit nào | **Giữ trạng thái Submit cũ** | Giữ nếu có | Giữ nếu có | **Dữ liệu cũ** |

Khi input trở lại giống chính xác chuỗi `LastAnalyzedExpression`, có thể bỏ cảnh báo stale; không cần Submit lại vì dữ liệu cũ ứng với cùng chuỗi.

## 6. Yêu cầu phi chức năng

- **NFR-01 — Tính đúng:** ưu tiên toán tử, unary, kết hợp trái, dấu ngoặc và biểu diễn số đúng đặc tả.
- **NFR-02 — Dễ theo dõi:** highlight rõ ràng; nút bước có trạng thái vô hiệu ở biên; kết quả và thông báo lỗi dễ đọc.
- **NFR-03 — Ổn định:** biểu thức không hợp lệ hoặc chia cho 0 không làm đóng ứng dụng; giới hạn 200 ký tự để giảm rủi ro cây quá lớn/đệ quy sâu.
- **NFR-04 — Tách lớp:** Lexer/Parser/Evaluator thuần C#, không tham chiếu WPF; ViewModel điều phối trạng thái; View chỉ hiển thị.
- **NFR-05 — Dễ bảo trì:** đặt tên kỹ thuật tiếng Anh, một nghĩa duy nhất; thông báo UI tiếng Việt.
- **NFR-06 — Dễ triển khai:** một project WPF, không cần database, server, parser generator, thư viện đồ thị.
- **NFR-07 — Kiểm thử:** bộ ca kiểm thử thủ công có mã và kỳ vọng rõ ràng.

## 7. Ràng buộc và tiêu chí nghiệm thu

- Hệ điều hành hỗ trợ: Windows có WPF/.NET 10.
- Không có tính năng nhập số thập phân (dù **kết quả** có thể thập phân).
- Không tự sửa cú pháp, không ngầm chuyển `2(3)` thành `2*(3)`.
- Không xuất AST; không lưu lịch sử; không tạo project test tự động.
- Chấp nhận khi các test case bắt buộc trong [`TEST_PLAN.md`](TEST_PLAN.md) đạt và mọi chức năng tương tác tuân thủ bảng trạng thái.

**Nguồn thiết kế:** Các quyết định được thống nhất trong quá trình phân tích yêu cầu của dự án; không tự thêm tính năng ngoài phạm vi.
