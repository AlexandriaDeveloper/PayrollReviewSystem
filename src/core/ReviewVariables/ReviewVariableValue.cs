namespace PayrollReviewSystem.Core.ReviewVariables;

/// <summary>
/// كائن قيمة حتمي يمثل قيمة متغير المراجعة العامة في النواة.
/// القواعد الثابتة:
/// 1. القيمة المجهولة (<see cref="Unknown"/> / INPUT_MISSING) لا تساوي أبدًا صفرًا معلومًا (<c>Known(0)</c>).
/// 2. يُستخدم <see cref="decimal"/> حصرًا لتجنب أخطاء التقريب غير الحتمية في <c>float</c> و<c>double</c>.
/// 3. يقبل النوع العام القيم العشرية الموقعة (موجبة أو صفرًا أو سالبة)، وتُعزل قيود الإشارة الخاصة بكل متغير مستقبلًا
///    ضمن سياسة قواعد مؤرخة وسند قانوني معتمد (B-03).
/// </summary>
public sealed record ReviewVariableValue
{
    /// <summary>قيمة مفردة (Singleton) تمثل الحالة المجهولة / نقص المدخلات (UNKNOWN / INPUT_MISSING).</summary>
    public static ReviewVariableValue Unknown { get; } = new(isKnown: false, amount: null);

    /// <summary>تحدد ما إذا كانت القيمة معلومة وموثقة (<c>true</c>) أم مجهولة (<c>false</c>).</summary>
    public bool IsKnown { get; }

    /// <summary>تحدد ما إذا كانت القيمة مجهولة / ناقصة البيانات (<c>INPUT_MISSING</c>).</summary>
    public bool IsUnknown => !IsKnown;

    /// <summary>
    /// المبلغ العشري الحتمي عند كون القيمة معلومة فقط؛ ويكون <c>null</c> عند <see cref="Unknown"/> لمنع تحويل المجهول إلى صفر.
    /// </summary>
    public decimal? Amount { get; }

    private ReviewVariableValue(bool isKnown, decimal? amount)
    {
        IsKnown = isKnown;
        Amount = amount;
    }

    /// <summary>
    /// ينشئ قيمة معلومة حتمية من <see cref="decimal"/> فقط (موجبة أو صفرًا أو سالبة).
    /// </summary>
    public static ReviewVariableValue Known(decimal amount)
        => new(isKnown: true, amount: amount);

    /// <summary>
    /// يقرأ المبلغ العشري بأمان فقط إذا كانت القيمة معلومة؛ ويرجع <c>false</c> عند <see cref="Unknown"/> دون إرجاع صفر.
    /// </summary>
    public bool TryGetKnownAmount(out decimal amount)
    {
        if (IsKnown && Amount.HasValue)
        {
            amount = Amount.Value;
            return true;
        }

        amount = default;
        return false;
    }

    /// <inheritdoc />
    public override string ToString()
        => IsKnown && Amount.HasValue
            ? Amount.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : "UNKNOWN(INPUT_MISSING)";
}
