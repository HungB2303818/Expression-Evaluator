# EVALUATION — Tính biểu thức và tạo EvaluationStep

## 1. Mục tiêu và bất biến

Evaluator nhận **Parse Tree của CFG cố định**, không dùng AST, tạo:

- `EvaluationResult.Value`: `decimal?` — kết quả nếu thành công.
- `EvaluationResult.Steps`: danh sách bất biến các `EvaluationStep` theo **thứ tự thực thi**.
- `EvaluationResult.Error`: lỗi đánh giá (nếu có).

**Bất biến:** Không sửa node; không làm tròn trung gian; mỗi unary/binary operator tạo đúng một step; lỗi được biểu diễn bằng một step cuối cùng, giữ những step thành công trước đó.

## 2. Thứ tự ưu tiên và thứ tự bước

Dùng cấu trúc grammar:

```text
Expr  := Term ExprTail
Term  := Unary TermTail
Unary := ('+' | '-') Unary | Primary
Primary := NUMBER | '(' Expr ')'
```

1. `Primary` đánh giá số/biểu thức trong ngoặc.
2. `Unary` đánh giá đệ quy operand trước, rồi ghi step `+`/`-` một ngôi.
3. `Term` tính lần lượt các phép nhân/chia trong `TermTail` **từ trái qua phải**.
4. `Expr` tính lần lượt phép cộng/trừ trong `ExprTail` **từ trái qua phải**.

Mỗi toán hạng con được đánh giá xong trước khi tạo step của phép toán cha. Do đó, `3+5*2` tạo `5*2=10`, rồi `3+10=13`.

## 3. Giải quyết đệ quy phải với kết hợp trái

**Sai:** nếu gộp `ExprTail` như một biểu thức con độc lập có thể diễn giải `10-3-2` thành `10-(3-2)=9`.

**Đúng:** tách tail thành chuỗi cặp (operator, next operand), sau đó **fold-left** accumulator.

Pseudocode:

```text
EvaluateExpr(Expr node):
    current = EvaluateTerm(node.Term)
    if current is Error: stop

    for (operator, termNode) in EnumerateExprTailInOrder(node.ExprTail):
        rhs = EvaluateTerm(termNode)
        if rhs is Error: stop
        current = ApplyAndTrace(operator, current, rhs)
        if current is Error: stop
    return current

EvaluateTerm(Term node):
    current = EvaluateUnary(node.Unary)
    if current is Error: stop

    for (operator, unaryNode) in EnumerateTermTailInOrder(node.TermTail):
        rhs = EvaluateUnary(unaryNode)
        if rhs is Error: stop
        current = ApplyAndTrace(operator, current, rhs)
        if current is Error: stop
    return current

EvaluateUnary(Unary node):
    if node -> PLUS Unary or MINUS Unary:
        operand = EvaluateUnary(node.childUnary)
        if operand is Error: stop
        return ApplyUnaryAndTrace(operator, operand)
    return EvaluatePrimary(node.Primary)

EvaluatePrimary(Primary node):
    if NUMBER: return decimal value + source node IDs
    if '(' Expr ')': return EvaluateExpr(node.Expr)
```

`EnumerateExprTailInOrder`/`EnumerateTermTailInOrder` chỉ theo đúng thứ tự các non-terminal tail, không dựng AST; có thể viết thành một vòng lặp lặp đi lặp lại trên tail.

## 4. Cơ chế provenance và HighlightedNodeIds

Để bước `7 - 2` sau `10 - 3` làm nổi bật các node biểu diễn **nguồn tạo ra số 7**, mỗi kết quả nội bộ không chỉ có `Value` mà còn có `ProvenanceNodeIds` (tập ID cú pháp nguồn):

```text
ComputedValue:
  Value: decimal
  ProvenanceNodeIds: Set<int>
```

- `NUMBER(10)` → provenance chứa node terminal `10`.
- Một phép toán `10 - 3` → provenance gộp provenance của `10`, node terminal `-`, provenance của `3`.
- Step của `7 - 2` → highlight provenance `7` (tức `10`, `-`, `3`), toán tử `-` **thứ hai**, và node `2`.
- Có thể thêm các node terminal unary vào provenance khi chúng đã tính xong.
- Không tự động highlight mọi non-terminal tổ tiên đến root; đó là lựa chọn khác đã loại bỏ.
- ID step tham chiếu cây hiện tại; khi Calculate lại, lập danh sách steps và ID mới.

**Ghi chú:** Nếu muốn làm nổi bật cả hình khối toán hạng (nhưng không phải mọi nút trung gian CFG), renderer có thể thêm viền nền cho vùng subtree liên quan; điều này là tinh chỉnh trình bày, không đổi dữ liệu bước.

## 5. Quy tắc tạo Step

| Cú pháp | OperationKind | Step |
|---|---|---|
| `+5` | `UnaryPlus` | `+(5) = 5` |
| `-5` | `UnaryMinus` | `-(5) = -5` |
| `3+2` | `Add` | `3 + 2 = 5` |
| `3-2` | `Subtract` | `3 - 2 = 1` |
| `3*2` | `Multiply` | `3 * 2 = 6` |
| `3/2` | `Divide` | `3 / 2 = 1.5` |
| `(3+2)` | — | Không tạo riêng một step cho dấu ngoặc |
| `3` | — | Không tạo step |

`StepNumber` bắt đầu từ 1, tăng đúng một lần cho mỗi operation. `Description` là text tiếng Việt hoặc công thức dễ đọc; số hiển thị trong mô tả **được format bằng NumberFormatter**, không thay đổi dữ liệu decimal nội bộ.

## 6. Ví dụ trace chi tiết

### 6.1. `--5+2*3`

| Step | Operation | Input đã tính | Result |
|---|---|---|---|
| 1 | UnaryMinus | `-(5)` | -5 |
| 2 | UnaryMinus | `-(-5)` | 5 |
| 3 | Multiply | `2 * 3` | 6 |
| 4 | Add | `5 + 6` | 11 |

Kết quả cuối **11** được hiển thị ngay sau Calculate; `CurrentStepIndex=0` cho đến khi bấm Next.

### 6.2. `10-3-2`

| Step | Calculation | Result |
|---|---|---|
| 1 | `10 - 3` | 7 |
| 2 | `7 - 2` | 5 |

### 6.3. `8/4/2`

| Step | Calculation | Result |
|---|---|---|
| 1 | `8 / 4` | 2 |
| 2 | `2 / 2` | 1 |

### 6.4. `-(2+3)*4`

1. `2 + 3 = 5`.
2. `-(5) = -5`.
3. `-5 * 4 = -20`.

## 7. Chia cho 0 và tràn số — vẫn giữ trace

Ví dụ `8/(4-4)`:

| Step | Type | Description | Result |
|---|---|---|---|
| 1 | Calculation | `4 - 4 = 0` | 0 |
| 2 | Error | `8 / 0: Không thể chia cho 0` | null |

Kết quả cuối: `Value=null`, `Error.Category=Evaluation`, `Error.Code=DivisionByZero`. Step 2 highlight provenance `8`, `/`, và provenance của `(4-4)`. Nguồn lỗi định vị tại token `/` (nếu cần, kèm thông tin toán hạng bằng 0).

Tương tự với cộng/trừ/nhân/chia tạo `OverflowException`, tạo step lỗi `ArithmeticOverflow` và dừng. Không được để exception đánh giá làm mất steps đã ghi.

**Mẫu control flow**:

```text
ApplyAndTrace(operator, left, right):
    ids = left.Provenance ∪ {operator.Id} ∪ right.Provenance
    try:
        if operator is Divide and right.Value == 0:
            append ErrorStep(DivisionByZero, ids)
            stop
        value = DecimalArithmetic(operator, left.Value, right.Value)
        append CalculationStep(value, ids)
        return ComputedValue(value, ids)
    catch OverflowException:
        append ErrorStep(ArithmeticOverflow, ids)
        stop
```

Bắt và chuyển lỗi chỉ tại những thao tác đánh giá dự kiến. Không dùng `double.NaN` hoặc `Infinity`: `decimal` không có các giá trị đó.

## 8. Decimal và format

- Số đầu vào là integer literal, nhưng được parse sang `decimal` để tính cả các phép chia.
- Không có bước round sau mỗi phép toán. Đặc biệt kết quả `1/3` có độ chính xác hữu hạn của `decimal`, không phải phân số chính xác tuyệt đối.
- Chỉ khi render: `Math.Round(value, 6, MidpointRounding.AwayFromZero)` rồi định dạng `0.######` bằng `InvariantCulture`.
- Kết quả ví dụ: `1/3 → 0.333333`, `10/6 → 1.666667`, `8/4 → 2`, `7/2 → 3.5`.
- Nếu số rất nhỏ sau format hiển thị `0`, đó là **giá trị trình bày** (không phải evaluator đổi thành 0). Không dùng chuỗi hiển thị để tính bước sau. Khi diễn giải một bước có số đã bị làm tròn đáng kể, có thể dùng nhãn `≈` hoặc chú thích “giá trị hiển thị đã làm tròn” để không ngụ ý giá trị nội bộ chính xác bằng 0.
- Input literal quá phạm vi `decimal` là lỗi **Lexical/NumberOutOfRange** trước bước Evaluation; overflow do **phép tính** là Evaluation Error.

## 9. Step Navigator và trạng thái

Với N = `Steps.Count`:

- Khởi đầu: `CurrentStepIndex = 0`, highlight rỗng; result cuối vẫn hiển thị nếu thành công.
- `Next`: nếu `index < N`, tăng 1; highlight `Steps[index-1].HighlightedNodeIds`.
- `Previous`: nếu `index > 0`, giảm 1; ở index 0 clear highlight.
- Next bị disable tại N, Previous bị disable tại 0.
- Nếu N=0 (chỉ số nguyên), cả hai disable; không tạo bước giả.
- Với lỗi Evaluation, N gồm bước `Error` cuối; có thể duyệt đến bước lỗi, và error text vẫn hiển thị ngay sau Submit.
- Nếu đang sửa input mới, vẫn điều hướng được trace cũ; hiển thị cảnh báo stale.
- Khi Calculate lại, reset index và thay kết quả/steps/tree bằng lần Submit mới.

## 10. Những lỗi triển khai cần tránh

1. Tính `ExprTail`/`TermTail` đệ quy từ phải sang trái làm sai `10-3-2` hoặc `8/4/2`.
2. Chỉ tạo steps cho phép toán hai ngôi, bỏ qua `+5`, `--5`.
3. Coi dấu ngoặc là phép tính và tạo step thừa.
4. Dùng giá trị đã format 6 chữ số làm đầu vào phép tính tiếp theo.
5. Ném exception ngay khi chia 0 và mất hết trace.
6. Đổi trạng thái Parse Tree khi Next/Previous.
7. Highlight node của AST (không tồn tại) thay vì node có ID trên Parse Tree.
8. Giữ result `13` như thành công sau khi Calculate mới gặp lỗi.

Đối chiếu các trường hợp bắt buộc trong `TEST_PLAN.md` trước khi nghiệm thu.
