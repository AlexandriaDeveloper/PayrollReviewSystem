using System.Globalization;
using PayrollReviewSystem.Core.ReviewVariables;

namespace PayrollReviewSystem.Application.ReviewVariables.Contracts;

/// <summary>
/// مدخل قيمة متغير المراجعة إلى حالات الاستخدام.
/// يفرض استخدام <see cref="decimal"/> للتمثيل المعلوم أو التصريح الصريح بحالة <see cref="Unknown"/>،
/// ويرفض أي محاولة لإدخال <c>float</c> أو <c>double</c> أو سلاسل غير صالحة لضمان الحتمية المالية.
/// </summary>
public sealed record ReviewVariableValueInput
{
    /// <summary>القيمة المفحوصة من النواة إذا كانت صالحة، أو <c>null</c> إذا فشل الفحص في مدخل خام غير صالح.</summary>
    internal ReviewVariableValue? ValidatedCoreValue { get; }

    /// <summary>سبب رفض المدخل إن وُجد (مثل قيمة سالبة أو <c>float</c>/<c>double</c> غير حتمي).</summary>
    internal string? ValidationError { get; }

    private ReviewVariableValueInput(ReviewVariableValue? validatedCoreValue, string? validationError)
    {
        ValidatedCoreValue = validatedCoreValue;
        ValidationError = validationError;
    }

    /// <summary>
    /// ينشئ مدخلًا لقيمة معلومة باستخدام <see cref="decimal"/> فقط (تشمل <c>0m</c> كصفر معلوم متميز عن المجهول).
    /// </summary>
    public static ReviewVariableValueInput Known(decimal amount)
    {
        if (ReviewVariableValue.TryCreateKnown(amount, out var value, out var failureReason) && value is not null)
        {
            return new ReviewVariableValueInput(value, null);
        }

        return new ReviewVariableValueInput(null, failureReason ?? "قيمة المتغير غير صالحة.");
    }

    /// <summary>
    /// ينشئ مدخلًا صريحًا لقيمة مجهولة / ناقصة البيانات (<c>UNKNOWN / INPUT_MISSING</c>) دون تحويلها إلى صفر.
    /// </summary>
    public static ReviewVariableValueInput Unknown()
        => new(ReviewVariableValue.Unknown, null);

    /// <summary>
    /// يتحقق من مدخل خارجي غير منمط؛ يرفض صراحةً أنواع النقطة العائمة (<c>float</c> و<c>double</c>)
    /// والقيم غير المنتهية (<c>NaN</c>/<c>Infinity</c>) والنصوص غير الرقمية والقيم السالبة.
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

        if (rawValue is int or long or short or byte)
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

            if (decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedDecimal))
            {
                return Known(parsedDecimal);
            }

            return new ReviewVariableValueInput(null, "النص المدخل ليس رقمًا عشريًا حتميًا صالحًا.");
        }

        return new ReviewVariableValueInput(null, "نوع القيمة المدخلة غير مدعوم؛ يُشترط decimal حتمي أو Unknown.");
    }
}
