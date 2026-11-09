# GRAMMAR — CFG, Lexer và Recursive Descent Parser

## 1. Mục tiêu

Sử dụng **Context-Free Grammar cố định** và một **Recursive Descent Parser viết tay** để phân tích biểu thức số học gồm các số nguyên ASCII, `+ - * /`, ngoặc và toán tử một ngôi. Parser phải tạo **Parse Tree đúng cấu trúc CFG**, không tạo AST.

## 2. Bộ token

| TokenType | Lexeme | Ý nghĩa |
|---|---|---|
| `Number` | `[0-9]+` | Số nguyên không dấu; giá trị parse sang `decimal` |
| `Plus` | `+` | Unary plus hoặc binary plus (phân biệt bởi Parser) |
| `Minus` | `-` | Unary minus hoặc binary minus |
| `Multiply` | `*` | Nhân tường minh |
| `Divide` | `/` | Chia thực |
| `LeftParen` | `(` | Mở ngoặc |
| `RightParen` | `)` | Đóng ngoặc |
| `EndOfInput` | `""` | Sentinel kết thúc, `SourceSpan.Length = 0` |

- Lexer bỏ qua các ký tự `char.IsWhiteSpace` nhưng giữ **vị trí theo chuỗi gốc**.
- Literal `-12` được tách thành `Minus`, `Number(12)`. Lexer không tự quyết định dấu một ngôi.
- Không nhận dấu chấm, dấu phẩy, ký hiệu mũ, chữ cái, Unicode digit ngoài `0–9`.
- **Chỉ phép nhân có `*`**: `2(3+4)` không được lexer sửa thành `2*(3+4)`.
- Số quá phạm vi `decimal` trả **Lexical Error dạng NumberOutOfRange** tại token (dù mẫu chữ số hợp lệ).
- Sau cùng luôn có token `EndOfInput` với `StartIndex = input.Length`.

## 3. Văn phạm chính thức

Ký hiệu đầu: `Expr`.

```bnf
Expr      -> Term ExprTail
ExprTail  -> PLUS Term ExprTail
           | MINUS Term ExprTail
           | ε

Term      -> Unary TermTail
TermTail  -> MULTIPLY Unary TermTail
           | DIVIDE Unary TermTail
           | ε

Unary     -> PLUS Unary
           | MINUS Unary
           | Primary

Primary   -> NUMBER
           | LEFT_PAREN Expr RIGHT_PAREN
```

Quy tắc trên dùng ký hiệu **terminal là token type**: `PLUS` ứng với `+`, `NUMBER` ứng với literal, ... Dialog có thể hiển thị một bản dễ đọc với chính các ký hiệu `+ - * / ( )`, miễn mapping rõ ràng và nhất quán.

`ε` = chuỗi rỗng. Với phân tích hoàn chỉnh, Parser phải **tiêu thụ hết input**, tức token tiếp theo sau `Expr` là `EndOfInput`.

### Ưu tiên và kết hợp

1. `Primary`: number hoặc ngoặc — cao nhất.
2. `Unary`: `+`, `-` một ngôi (liên tiếp: `--5`, `+-5`).
3. `Term`: `*`, `/` — kết hợp **trái**.
4. `Expr`: `+`, `-` hai ngôi — kết hợp **trái**.

Ví dụ:

```text
3 + 5 * 2   = 3 + (5*2)     = 13
-(2 + 3)    = -(5)           = -5
10 - 3 - 2  = (10 - 3) - 2   = 5
8 / 4 / 2   = (8 / 4) / 2    = 1
3 * -2      = 3 * (-2)       = -6
```

**Chú ý học thuật:** Quy tắc `ExprTail` và `TermTail` đệ quy phải nhằm loại đệ quy trái cho Recursive Descent, nhưng *thứ tự áp dụng phép toán* vẫn phải là kết hợp trái. Không được tính đệ quy tail một cách ngây thơ thành `10 - (3 - 2)`.

## 4. Tại sao grammar phù hợp Recursive Descent?

Văn phạm đệ quy trái trực tiếp như `Expr -> Expr + Term | Term` gây gọi vô hạn nếu hiện thực bằng phương thức `ParseExpr()` gọi chính nó ngay ở nhánh đầu. Sau biến đổi, `Expr -> Term ExprTail` và `ExprTail -> + Term ExprTail | - Term ExprTail | ε` cho phép nhìn token hiện tại để chọn production mà không đệ quy trái.

Những lựa chọn chính có lookahead một token (`LL(1)` trong phạm vi grammar này):

- `ExprTail`: `Plus`, `Minus` → nhánh toán tử; `RightParen` hoặc `EndOfInput` → `ε`; token khác ở vị trí đó → báo lỗi phù hợp.
- `TermTail`: `Multiply`, `Divide` → nhánh toán tử; `Plus`, `Minus`, `RightParen`, `EndOfInput` → `ε`; token khác → báo lỗi.
- `Unary`: `Plus`/`Minus` → unary; `Number`/`LeftParen` → `Primary`.
- `Primary`: `Number` hoặc `LeftParen`; ký hiệu khác → Syntax Error.

Những token không thuộc tập theo sau hợp lệ phải báo lỗi; không được để `ε` che mất lỗi `3 4` hoặc `2(3+4)`.

## 5. Cấu trúc Parser

Hợp đồng dự kiến:

```csharp
public sealed class RecursiveDescentParser
{
    public ParseTreeNode Parse(IReadOnlyList<Token> tokens);

    // Nội bộ:
    // ParseExpr, ParseExprTail, ParseTerm, ParseTermTail,
    // ParseUnary, ParsePrimary, Match, Current, Peek
}
```

Pseudocode:

```text
Parse(tokens):
    current = 0
    root = ParseExpr()
    Expect(EndOfInput)
    return root

ParseExpr():
    return Node("Expr", ParseTerm(), ParseExprTail())

ParseExprTail():
    if current in {Plus, Minus}:
        op = Consume()
        return Node("ExprTail", op, ParseTerm(), ParseExprTail())
    if current in {RightParen, EndOfInput}:
        return Node("ExprTail", Epsilon())
    Error(UnexpectedToken)

ParseTerm():
    return Node("Term", ParseUnary(), ParseTermTail())

ParseTermTail():
    if current in {Multiply, Divide}:
        op = Consume()
        return Node("TermTail", op, ParseUnary(), ParseTermTail())
    if current in {Plus, Minus, RightParen, EndOfInput}:
        return Node("TermTail", Epsilon())
    Error(UnexpectedToken)

ParseUnary():
    if current in {Plus, Minus}:
        return Node("Unary", Consume(), ParseUnary())
    return Node("Unary", ParsePrimary())

ParsePrimary():
    if current is Number: return Node("Primary", Consume(Number))
    if current is LeftParen:
        return Node("Primary", Consume('('), ParseExpr(), Expect(')'))
    Error(ExpectedOperand)
```

Đặc biệt: `RightParen` ở ngữ cảnh top-level có thể khiến `ExprTail` chọn `ε`, sau đó `Expect(EndOfInput)` báo `)` thừa. Trong ngoặc, `Expect(')')` báo thiếu `)` nếu gặp EOF.

## 6. Mô hình Parse Tree

Mỗi nút có:

- `Id`: duy nhất trong **một lần Parse**, ổn định khi duyệt steps.
- `Symbol`: tên non-terminal (`Expr`, `Term`, `Unary`, ...) hoặc terminal (`NUMBER`, `PLUS`, ...).
- `Lexeme`: với terminal (ví dụ `5`, `*`), `null` với non-terminal.
- `SourceSpan`: đoạn chuỗi nguồn tương ứng (zero-based).
- `Children`: danh sách theo đúng thứ tự production.
- `IsEpsilon`: đánh dấu trường hợp `ε` (nếu lưu nội bộ).

**Chốt thực hiện:** Parser có thể lưu node `ε` để giữ đủ thông tin văn phạm; `ParseTreeView` **luôn lọc ẩn** node đó. Không có nút AST. Mọi nút còn lại gồm terminal và non-terminal phải xuất hiện trên tree view.

Ví dụ biểu diễn ngắn gọn (đã ẩn các `ε`) cho `3 + 5 * 2`:

```text
Expr
├── Term
│   ├── Unary
│   │   └── Primary
│   │       └── NUMBER: 3
│   └── TermTail
└── ExprTail
    ├── PLUS: +
    ├── Term
    │   ├── Unary
    │   │   └── Primary
    │   │       └── NUMBER: 5
    │   └── TermTail
    │       ├── MULTIPLY: *
    │       ├── Unary
    │       │   └── Primary
    │       │       └── NUMBER: 2
    │       └── TermTail
    └── ExprTail
```

`TermTail`/`ExprTail` không có con hiển thị khi production của chúng sinh `ε`; **bản thân non-terminal vẫn hiển thị**.

## 7. Khả năng định vị lỗi

`SourceSpan(StartIndex, Length)` dùng chỉ số zero-based; UI hiển thị vị trí one-based.

| Input | Vấn đề | Vị trí mong đợi |
|---|---|---|
| `3 & 4` | Ký tự không nhận diện `&` | 3 |
| `3.5` | Dấu `.` không hỗ trợ | 2 |
| `3 + * 5` | Thiếu toán hạng trước `*` | 5 |
| `(2+3` | Thiếu `)` trước EOF | 5 (`Length + 1`) |
| `2(3+4)` | Thiếu toán tử trước `(` | 2 |
| `3 4` | Hai literal kề nhau | 3 |

Nếu gặp lỗi từ vựng/cú pháp, Parser/Service không xuất tree dở dang cho UI. Trên lần Submit mới, trạng thái dữ liệu cũ phải được thay đúng như `REQUIREMENTS.md`.

## 8. Thứ tự thực hiện

1. Validate input rỗng và 200 ký tự.
2. Lexer tạo token và EOF; kiểm tra số hợp phạm vi decimal.
3. Parser áp dụng CFG, yêu cầu consume EOF.
4. Chỉ trả về root nếu phân tích hoàn tất.
5. Evaluator duyệt **cây này**, tạo result và trace (xem `EVALUATION.md`).

Không thay grammar để đơn giản hóa việc vẽ: cây hiển thị phải phản ánh các non-terminal trên grammar cố định.
