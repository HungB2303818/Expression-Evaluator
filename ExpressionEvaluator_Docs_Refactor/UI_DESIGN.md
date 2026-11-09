# UI_DESIGN — Light Theme, cây dẫn xuất E/T/F

## 1. Bố cục và trạng thái

Cửa sổ chính WPF khoảng `1100×700` (tối thiểu vừa đủ để hai cột không chồng). Header có **Xem văn phạm**. Vùng trên: TextBox biểu thức (TwoWay, `UpdateSourceTrigger=PropertyChanged`), nút **Tính toán**, bộ đếm `string.Length / 200`, kết quả hoặc panel lỗi và banner stale. Vùng dưới: cây trái (khoảng 2/3) và bước/Trước/Tiếp phải (khoảng 1/3). Giao diện tiếng Việt, theme sáng, không có history, AST, zoom/pan hoặc bàn phím máy tính.

* Enter trong ô nhập và click Tính toán dùng **cùng `CalculateCommand`**; chỉ parse khi Submit, không khi gõ. Không giới hạn nhập cứng ở 200.
* Mở app: input trống, result/cây/steps trống hoặc placeholder, Trước/Tiếp disabled. Success: kết quả đã format **hiện ngay**, index 0, highlight rỗng; N=0 thì hai nút disabled. Next/Previous chỉ đổi index/ID highlight; vẫn giữ result cuối, khóa tại 0 và N.
* Chỉnh input nhưng chưa Submit: vẫn hiện kết quả/cây/trace lần trước và banner **“Biểu thức đã thay đổi. Kết quả và cây bên dưới thuộc biểu thức đã phân tích trước đó: [LastAnalyzedExpression]”**. Vẫn duyệt steps cũ; khi chuỗi khớp lại bản đã Submit, hết banner; không parse lại.
* Submit Input/Lexical/Syntax Error: bỏ result/cây/trace cũ, thông báo loại/nguyên nhân/vị trí. Submit Evaluation Error: bỏ giá trị thành công, giữ cây mới và trace gồm Error step cuối, hiện lỗi **ngay**. Khi đang chọn Error step, dùng viền đỏ và nhãn lỗi; không chỉ dựa vào màu.
* `SourceSpan` zero-based ở model, UI hiển thị vị trí bắt đầu từ 1. Sau **Submit** chọn span token lỗi trong TextBox; nếu `Length=0` ở EOF đặt caret cuối và báo `input.Length+1 (cuối biểu thức)`; lỗi input toàn chuỗi không cần chọn token giả. Gõ tiếp không chọn/báo lỗi mới đến lần Submit kế.

## 2. Vẽ Parse Tree bằng ScrollViewer + Canvas

`ParseTreeView` nhận root, `TreeLayoutResult`, `HighlightedNodeIds` và cờ Error step. Vẽ các cạnh ở lớp dưới, node ở lớp trên. **Tất cả node** E/T/F, terminal toán tử/số/ngoặc đều hiện; không có `ExprTail`/`TermTail`/`ε`. Nhãn non-terminal: `E`, `T`, `F`; nhãn terminal: **lexeme gốc**, gồm literal `00012`, toán tử và dấu ngoặc (không hiển thị `NUMBER: 00012` trong hình tròn). `ParseTreeNode.Symbol` vẫn giữ loại token cho Evaluator.

Mỗi node là **hình tròn thật**: chiều rộng = chiều cao = `GetDiameter(node)`, đo/ước lượng bao trọn text với padding ở font UI cố định (ví dụ Segoe UI 14). Literal tối đa 29 chữ số không được cắt/tràn: tăng đường kính theo nhãn (và theo DPI/font nếu cần), không ép mọi node thành hình tròn nhỏ. Bố trí dùng kích thước **từng subtree** và khoảng cách dương giữa hai vùng node; chiều cao mỗi tầng theo **đường kính lớn nhất ở tầng đó** cộng khoảng cách, nên node tròn lớn không đụng tầng dưới. Parent căn giữa vùng các con theo thứ tự production. `Canvas.Width/Height` bao trọn node, stroke và đầu mũi tên; `ScrollViewer` có scrollbar ngang/dọc Auto khi vượt viewport. Với input <=200 ký tự vẫn phải xem được toàn bộ cây qua cuộn; không thêm zoom/pan.

Cạnh có mũi tên **từ cha tới con**, đầu mũi tên dừng ở **chu vi hình tròn con** (không đâm xuyên chữ). Gốc `E` có viền xanh nổi bật/viền ngoài riêng kể cả khi được highlight theo step; step chọn có viền dày/tô nền, error có viền đỏ và nhãn Error ở bảng bước. Bước 0 không có highlight. Chuyển bước **không gọi lại layout hoặc parse/evaluate**, chỉ đổi style theo ID; node trung gian có thể là **tập terminal không liền một subtree**. Không tự động tô mọi non-terminal tổ tiên. Hình cây **là cây dẫn xuất phân nhánh phải**, không phải AST: đặt chú giải gần cây hoặc trong Grammar Dialog: *“Cây thể hiện luật sinh; các phép cùng mức được tính từ trái sang phải.”*

Màu gợi ý: nền `#FFFFFF`, panel `#F8FAFC`, viền `#E2E8F0`, chữ `#0F172A`, phụ `#64748B`, primary `#2563EB`, thành công `#15803D`, lỗi `#B91C1C`. Đảm bảo chữ đọc được ở 100% scaling, nút disabled nhìn rõ; nút lỗi có text cùng màu viền.

## 3. Grammar Dialog

Nút **Xem văn phạm** mở dialog tiêu đề **Văn phạm phi ngữ cảnh (CFG)**, text **chỉ đọc**, cuộn nếu dài, nút Đóng. Nội dung đúng `GrammarDefinition`:

```text
E -> T + E | T - E | T
T -> F * T | F / T | F
F -> + F | - F | ( E ) | NUMBER
```

Giải nghĩa `E/T/F`, terminal và chú giải rằng cây CFG rẽ phải **không đồng nghĩa** đánh giá toán học từ phải sang trái; ví dụ `10-3-2` có trace `10-3`, `7-2`. Đóng dialog không reset input, result, tree hoặc step. Không sửa, import hay thay thế grammar.

## 4. Kiểm tra trực quan

Chạy thủ công các ca UI trong `TEST_PLAN.md`: `2*3+6` (cây E/T/F, vòng tròn/mũi tên, root viền xanh), `10-3-2` (cây rẽ phải nhưng trace kết hợp trái), `--5` (F unary 2 con), literal `79228162514264337593543950335` (hình tròn lớn không cắt chữ), cây dài/cuộn và các nhánh lỗi/stale. Tất cả hình học phải khớp nhãn thực tế, không chỉ build thành công.
