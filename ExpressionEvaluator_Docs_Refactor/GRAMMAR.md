# GRAMMAR — CFG E/T/F, Lexer và Recursive Descent Parser

## 1. Phạm vi và token

Lexer nhận **duy nhất** chữ số ASCII `[0-9]+`, các dấu `+ - * / ( )` và bỏ qua `char.IsWhiteSpace`. Mỗi token giữ lexeme và `SourceSpan(StartIndex, Length)` theo chỉ số UTF-16 **zero-based** của chuỗi gốc; cuối chuỗi thêm `EndOfInput` với `StartIndex = expression.Length`, `Length = 0`. Dấu âm là token `Minus` riêng, không thuộc literal. Không nhận Unicode digit, số thập phân, ký pháp khoa học hay phép nhân ngầm. Parse literal sang `decimal` bằng `NumberStyles.None`/`InvariantCulture`; literal quá phạm vi gây `Lexical/NumberOutOfRange` tại span toàn bộ literal; ký tự lạ gây `Lexical/InvalidCharacter` tại ký tự đó. Giới hạn 200 `string.Length` thuộc `ExpressionService`, kiểm tra **trước** Lexer.

| TokenType | Lexeme | Terminal trong Parse Tree |
|---|---|---|
| Number | `[0-9]+` | `NUMBER` |
| Plus / Minus | `+` / `-` | `PLUS` / `MINUS` |
| Multiply / Divide | `*` / `/` | `MULTIPLY` / `DIVIDE` |
| LeftParen / RightParen | `(` / `)` | `LEFT_PAREN` / `RIGHT_PAREN` |
| EndOfInput | rỗng | **Không nằm trong cây** |

## 2. Văn phạm cố định

Ký hiệu bắt đầu `E`. Không sinh `ExprTail`, `TermTail`, AST hoặc node `ε`.

```bnf
E -> T PLUS E | T MINUS E | T
T -> F MULTIPLY T | F DIVIDE T | F
F -> PLUS F | MINUS F | LEFT_PAREN E RIGHT_PAREN | NUMBER
```

Thứ tự ưu tiên: `F` (số, ngoặc, unary) > `T` (nhân/chia) > `E` (cộng/trừ). Phép nhân phải có `*`; `2(3+4)` là Syntax Error. Mọi toán tử hai ngôi cùng mức **được đánh giá kết hợp trái** theo `EVALUATION.md`.

**Phân biệt quan trọng:** Cây này là **cây dẫn xuất của CFG đệ quy phải**, không phải cây nhóm phép toán (AST). Viết **rút gọn để giải thích** (ẩn `F`, tên terminal và một số node `T` một con), `10-3-2` có dạng `E(T(10), -, E(T(3), -, E(T(2))))`; **cây thực tế vẫn có đủ `E/T/F` và terminal**. Nếu ngây thơ tính từng subtree `E` bên phải sẽ ra `9`, **không phải hành vi của ứng dụng**. Evaluator đi dọc chuỗi `E` và fold-left: `(10-3)-2=5`. Giá trị trung gian `10-3` không có một subtree duy nhất trên hình; highlight dùng **tập ID terminal nguồn**, không dùng subtree của node `E` bên phải. Dialog văn phạm và chú giải cạnh cây phải nói rõ sự phân biệt này. Nếu muốn cây tự nó thể hiện phép kết hợp trái, phải đổi CFG/hình cây và cập nhật đồng bộ toàn bộ tài liệu; **không** được lặng lẽ coi cây hiện tại là AST.

Các nhánh `E` đều bắt đầu bằng `T` và các nhánh `T` đều bắt đầu bằng `F`: **văn phạm viết ở trên không phải LL(1) nguyên dạng**. Parser recursive descent phân tích **tiền tố chung trước**, rồi nhìn token kế tiếp để chọn nhánh (tương đương left-factoring nội bộ); không chọn production trước khi đọc `T`/`F` và không gọi `ParseE`/`ParseT` ngay đầu nhánh.

## 3. Hợp đồng Parser

`Parse(tokens)` yêu cầu danh sách token có đúng một EOF cuối; mỗi lần Parse reset chỉ số token và bộ cấp ID. Chỉ trả root `E` sau khi consume/kiểm tra `EndOfInput`; gặp lỗi không xuất cây dở dang.

```text
ParseE():
    left = ParseT()
    if lookahead in {Plus, Minus}:
        op = Consume(); right = ParseE()
        return Node("E", left, op, right)
    if lookahead in {RightParen, EndOfInput}:
        return Node("E", left)
    Error(UnexpectedToken / MissingOperator at lookahead)

ParseT():
    left = ParseF()
    if lookahead in {Multiply, Divide}:
        op = Consume(); right = ParseT()
        return Node("T", left, op, right)
    if lookahead in {Plus, Minus, RightParen, EndOfInput}:
        return Node("T", left)
    Error(UnexpectedToken / MissingOperator at lookahead)

ParseF():
    if lookahead in {Plus, Minus}:
        return Node("F", Consume(), ParseF())          // 2 con: unary
    if lookahead == Number:
        return Node("F", Consume())                   // 1 con: literal
    if lookahead == LeftParen:
        left = Consume(); inner = ParseE(); right = Expect(RightParen)
        return Node("F", left, inner, right)           // 3 con: ngoặc
    Error(ExpectedOperand at lookahead)
```

`E`/`T` có **1 hoặc 3 con**, `F` có **1, 2 hoặc 3 con**. Terminal là node lá; `Lexeme=null` với non-terminal, lexeme gốc với terminal. Mọi node có ID duy nhất trong một lần Parse, span theo chuỗi nguồn; cây bất biến sau Parse. Không lưu `CalculatedValue` hay geometry trong node.

FOLLOW hợp lệ để kết thúc `E`: `RightParen`, `EndOfInput`; để kết thúc `T`: `Plus`, `Minus`, `RightParen`, `EndOfInput`. Token khác không được coi là nhánh kết thúc hợp lệ: `3 4` và `2(3+4)` phải báo lỗi ở token thứ hai. `3)` ở top-level là `UnexpectedRightParen` tại `)` sau khi kiểm tra EOF. `(` chưa đóng đến EOF là `MissingRightParen` tại EOF; `3+*5` là `ExpectedOperand` tại `*`. Vị trí hiển thị cho người dùng = `Span.StartIndex+1`; EOF = `expression.Length+1`.

## 4. Diễn giải cây trong giao diện

Mọi non-terminal `E`, `T`, `F` và **mọi terminal** (kể cả dấu ngoặc và unary) đều có mặt. Nhãn nhìn thấy của terminal là `Lexeme` (`5`, `+`, `(` ...), nhãn non-terminal là `Symbol`; terminal vẫn giữ `Symbol` là loại token để Evaluator phân biệt unary/binary theo **vị trí trong production**. Cây bố trí node tròn, mũi tên cha → con, gốc `E` viền xanh. Không có `ExprTail`, `TermTail` hoặc node `ε` để ẩn. Quy tắc kích thước node và provenance xem `UI_DESIGN.md`, `EVALUATION.md`.
