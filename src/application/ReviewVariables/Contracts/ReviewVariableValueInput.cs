using System.Globalization;
using PayrollReviewSystem.Core.ReviewVariables;

namespace PayrollReviewSystem.Application.ReviewVariables.Contracts;

/// <summary>
/// مدخل قيمة متغير المراجعة إلى حالات الاستخدام.
/// يفرض استخدام <see cref="decimal"/> للتمثيل المعلوم (موجبًا أو صفرًا أو سالبًا) أو التصريح الصريح بحالة <see cref="Unknown"/>،
/// ويرفض أي محاولة لإدخال <c>float</c> أو <c>double</c> أو سلاسل غير صالحة لضمان الحتمية المالية.
/// </summary>
public sealed record ReviewVariableValueInput
{
    /// <summary>القيمة المفحوصة من النواة إذا كانت صالحة، أو <c>null</c> إذا فشل الفحص في مدخل خام غير صالح.</summary>
    internal ReviewVariableValue? ValidatedCoreValue { get; }

    /// <summary>سبب رفض المدخل إن وُجد (مثل <c>float</c>/<c>double</c> غير حتمي أو نص غير رقمي).</summary>
    internal string? ValidationError { get; }

    private ReviewVariableValueInput(ReviewVariableValue? validatedCoreValue, string? validationError)
    {
        ValidatedCoreValue = validatedCoreValue;
        ValidationError = validationError;
    }

    /// <summary>
    /// ينشئ مدخلًا لقيمة معلومة باستخدام <see cref="decimal"/> فقط (موجبة أو صفرًا أو سالبة دون فرض قيود مالية عامة بلا سند).
    /// </summary>
    public static ReviewVariableValueInput Known(decimal amount)
        => new(ReviewVariableValue.Known(amount), null);

    /// <summary>
    /// ينشئ مدخلًا صريحًا لقيمة مجهولة / ناقصة البيانات (<c>UNKNOWN / INPUT_MISSING</c>) دون تحويلها إلى صفر.
    /// </summary>
    public static ReviewVariableValueInput Unknown()
        => new(ReviewVariableValue.Unknown, null);

    /// <summary>
    /// يتحقق من مدخل خارجي غير منمط؛ يرفض صراحةً أنواع النقطة العائمة (<c>float</c> و<c>double</c>)
    /// والقيم غير المنتهية (<c>NaN</c>/<c>Infinity</c>) والنصوص غير الرقمية، ويقبل <see cref="decimal"/> الموقع.
    /// </summary>
    public static ReviewVariableValueInput FromExternalInput(object? rawValue)
    {
        if (rawValue is null)
        {
            return new ReviewVariableValueInput(null, "القيمة المرسلة فارغة؛ استخدم Unknown() صراحةً للمدخل المفقود.");
        }

        // رفض صريح للتمثيل العائم غير الحتمي (IEEE 754 float/double) التزامًا بمعايير Deterministic Money
        if (rawValue is double or float)
        {
            return new ReviewVariableValueInput(
                null,
                "ممنوع استخدام float أو double في القيم المالية؛ يجب تمرير decimal حتمي.");
        }

        if (rawValue is decimal decimalValue)
        {
            return Known(decimalValue);
        }

        if (rawValue is int or long or short or byte or sbyte)
        {
            return Known(Convert.ToDecimal(rawValue, CultureInfo.InvariantCulture));
        }

        if (rawValue is string text)
        {
            var trimmed = text.Trim();
            if (trimmed.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase))
            {
                return Unknown();
            }

            if (decimal.TryParse(
                trimmed,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture,
                out var parsedDecimal))
            {
                return Known(parsedDecimal);
            }

            return new ReviewVariableValueInput(null, "النص المدخل ليس رقمًا عشريًا حتميًا صالحًا.");
        }

        return new ReviewVariableValueInput(null, "نوع القيمة المدخلة غير مدعوم؛ يُشترط decimal حتمي أو Unknown.");
    }
}
