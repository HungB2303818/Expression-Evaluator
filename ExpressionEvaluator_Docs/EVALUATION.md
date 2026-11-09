# EVALUATION — Đánh giá trực tiếp Parse Tree E/T/F

## 1. Bất biến

`ExpressionEvaluator.Evaluate(ParseTreeNode root)` chỉ đọc cây CFG cố định, không tạo AST, không ghi giá trị vào node. Dùng `decimal`; `EvaluationResult(Value, Steps, Error)` chứa trace bất biến theo thứ tự thực hiện. **Mỗi** dấu unary/binary sinh một `EvaluationStep`; literal và dấu ngoặc không sinh step. Không làm tròn khi tính; chỉ `NumberFormatter` định dạng lúc hiển thị (`Math.Round(value, 6, MidpointRounding.AwayFromZero)` rồi `"0.######"`/`InvariantCulture`). Bước đầu đánh số 1; UI bắt đầu ở index 0 nhưng hiện ngay kết quả cuối nếu thành công.

## 2. Cây dẫn xuất phải, giá trị kết hợp trái

`E(T, op, E)` và `T(F, op, T)` **không được** tính đệ quy toàn bộ nhánh bên phải trước khi áp dụng toán tử: `10-3-2` phải thành `(10-3)-2=5`, `8/4/2` phải thành `(8/4)/2=1`. Cây mô tả **production áp dụng**, không trực tiếp biểu diễn cây nhóm phép toán. Evaluator không tự động đánh giá `rightE`/`rightT` như một toán hạng duy nhất.

```text
EvaluateE(e):
    current = EvaluateT(e.Children[0])
    cursor = e
    while cursor.Children.Count == 3:              // E = [T, operator, E]
        op = cursor.Children[1]
        rightE = cursor.Children[2]
        rhs = EvaluateT(rightE.Children[0])        // chỉ T kế tiếp, KHÔNG EvaluateE(rightE)
        current = ApplyBinaryAndTrace(op, current, rhs)
        cursor = rightE
    return current                                 // cursor có đúng 1 con T

EvaluateT(t):
    current = EvaluateF(t.Children[0])
    cursor = t
    while cursor.Children.Count == 3:              // T = [F, operator, T]
        op = cursor.Children[1]
        rightT = cursor.Children[2]
        rhs = EvaluateF(rightT.Children[0])        // chỉ F kế tiếp, KHÔNG EvaluateT(rightT)
        current = ApplyBinaryAndTrace(op, current, rhs)
        cursor = rightT
    return current

EvaluateF(f):
    [NUMBER]   -> parse decimal invariant; no step; provenance = {NUMBER.Id}
    [op, F]    -> EvaluateF(operand), rồi ApplyUnaryAndTrace(op, operand)
    ['(', E, ')'] -> EvaluateE(E); không tạo bước cho dấu ngoặc
```

`ApplyBinaryAndTrace` đánh giá RHS xong **trước** khi tạo bước của phép toán cha (ví dụ `3+5*2`: `5*2` rồi `3+10`). Khi có lỗi ở RHS, dừng ngay, không thêm bước của cha. Trong chuỗi cùng mức, chỉ lần lượt tính một F/T kế tiếp; dừng tại toán tử gây lỗi, không tính tiếp toán tử sau.

## 3. Highlight và provenance

Giá trị trung gian nội bộ: `ComputedValue(decimal Value, IReadOnlySet<int> ProvenanceNodeIds)`. `NUMBER` có provenance ID terminal số; unary hợp provenance operand + ID terminal dấu; binary hợp provenance trái + ID terminal toán tử + provenance phải. Step dùng tập hợp này làm `HighlightedNodeIds` (không highlight mọi non-terminal tổ tiên; dấu ngoặc không tự sinh bước). Tập node của trung gian **có thể nằm rải trên nhiều nhánh E/T** chứ không là một subtree: bước 2 của `10-3-2` phải highlight `10`, dấu `-` thứ nhất, `3`, dấu `-` thứ hai và `2`. Bước cuối của `2*3+6` highlight nguồn `2`, `*`, `3`, dấu `+` và `6`; không đòi một node đơn lẻ chứa giá trị `6` trung gian.

`ParseTreeNode.Id` chỉ duy nhất trong **một lần Parse**; Submit mới phải xây root, trace và geometry mới. Đổi Trước/Tiếp chỉ chọn `Steps[index-1].HighlightedNodeIds`, không parse/tính/layout lại. Với index 0 tập highlight rỗng. Lỗi evaluation vẫn cho duyệt tới Error step cuối.

## 4. Lỗi và giá trị hiển thị

Trước khi chia, nếu RHS `Value==0`, tạo `ExpressionError(Evaluation, "DivisionByZero", ..., op.Span)`; nếu phép toán decimal unary/binary ném `OverflowException`, tạo `ArithmeticOverflow` tại span toán tử. **Giữ** các step đã thành công, thêm đúng **một step `Type=Error` cuối**, `Result=null`, `Value` toàn cục `null`, dùng **cùng đối tượng Error** ở step, `EvaluationResult` và `ExpressionAnalysisResult`. Chỉ bắt lỗi số học dự kiến, không biến lỗi lập trình thành Syntax Error.

Ví dụ `8/(4-4)`: step 1 `4-4=0`; step 2 Error `8/0`, vị trí token `/` là 2; còn Parse Tree và 2 steps, không có result thành công. `1/(3-3)+2`: dừng sau bước chia lỗi, không tạo bước `+2`. Literal vượt phạm vi thuộc **Lexical**, xảy ra trước Evaluation.

`Description` của bước là công thức dễ đọc với số đã format nhưng **không dùng chuỗi này để tính lại**. Khi một toán hạng/kết quả hiển thị khác giá trị decimal chưa làm tròn (ví dụ `1/10000000` thành `0`), dùng ký hiệu `≈` cho **số hiển thị đã làm tròn** trong mô tả bước hoặc một chú thích tương đương; không ngụ ý `0*10000000=1` là đẳng thức chính xác. `ResultText` vẫn dùng định dạng 6 chữ số theo yêu cầu. Thử `1/10000000*10000000` phải ra `1`.

## 5. Thứ tự mẫu để nghiệm thu

| Biểu thức | Trace mong đợi | Result |
|---|---|---|
| `--5+2*3` | `-(5)=-5` → `-(-5)=5` → `2*3=6` → `5+6=11` | `11` |
| `10-3-2` | `10-3=7` → `7-2=5` | `5` |
| `8/4/2` | `8/4=2` → `2/2=1` | `1` |
| `-(2+3)*4` | `2+3=5` → `-(5)=-5` → `-5*4=-20` | `-20` |
| `8/(4-4)` | `4-4=0` → Error chia 0 | không có Value |

Chi tiết ca lỗi, rounding và trạng thái UI xem `TEST_PLAN.md`.
