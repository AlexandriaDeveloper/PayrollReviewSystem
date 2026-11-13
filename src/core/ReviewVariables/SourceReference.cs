namespace PayrollReviewSystem.Core.ReviewVariables;

/// <summary>
/// تصنيف قناة المصدر المُصرّح بها عند إدخال أو تعديل متغير المراجعة.
/// هذا التصنيف بيانات وصفية مُصرّح بها فقط (<c>DeclaredMetadata</c>) ولا يثبت أصالة كشف أوراكل الرسمي أو التحقق البنكي.
/// </summary>
public enum SourceKind
{
    /// <summary>ادعاء كشف أوراكل الرسمي (ORACLE_OFFICIAL) — يحتاج تحققًا مستقلًا خارج CODE-001 لإثبات الأصالة.</summary>
    OracleOfficial = 1,

    /// <summary>استيراد من ملف إكسل (EXCEL_IMPORT) — قناة بديلة لا تمنح صفة كشف أوراكل الرسمي.</summary>
    ExcelImport = 2,

    /// <summary>إدخال مباشر من شاشة المراجعة (DIRECT_FORM) — قناة بديلة لا تمنح صفة كشف أوراكل الرسمي.</summary>
    DirectForm = 3
}

/// <summary>
/// مرجع المصدر المُصرّح به المرتبط بنسخة متغير المراجعة.
/// تنبيه حوكمة (B-02): يحفظ هذا الكائن **البيان الوصفي المصرح به فقط** ولا يمتلك آلية تحقق من أصالة أوراكل؛
/// لذلك تبقى <see cref="IsAuthenticityVerified"/> دائمًا <c>false</c> حتى عند ادعاء <see cref="SourceKind.OracleOfficial"/>.
/// </summary>
public sealed record SourceReference
{
    /// <summary>قناة المصدر المصرح بها من الإدخال فقط (غير متحقق من أصالتها آليًا في هذه الشريحة).</summary>
    public SourceKind DeclaredKind { get; }

    /// <summary>معرّف نسخة المصدر المصرح به في الإدخال.</summary>
    public string SourceVersionId { get; }

    /// <summary>مرجع إضافي اختياري للمستند أو جلسة الالتقاط (دون بيانات موظفين حساسة).</summary>
    public string? SourceCaptureRef { get; }

    /// <summary>
    /// يؤكد صراحةً أن أصالة المصدر **غير متحقق منها** في شريحة CODE-001؛
    /// لا يُسمح لأي إدخال بترقية نفسه إلى «أوراكل رسمي متحقق» بمجرد اختيار <see cref="SourceKind.OracleOfficial"/>.
    /// </summary>
    public bool IsAuthenticityVerified => false;

    private SourceReference(SourceKind declaredKind, string sourceVersionId, string? sourceCaptureRef)
    {
        DeclaredKind = declaredKind;
        SourceVersionId = sourceVersionId;
        SourceCaptureRef = sourceCaptureRef;
    }

    /// <summary>
    /// ينشئ مرجع مصدر وصفيًا بعد التحقق من تعريف نوع القناة وصلاحية معرّف نسخة المصدر.
    /// </summary>
    public static bool TryCreate(
        SourceKind declaredKind,
        string? sourceVersionId,
        string? sourceCaptureRef,
        out SourceReference? reference)
    {
        if (!Enum.IsDefined(declaredKind) || string.IsNullOrWhiteSpace(sourceVersionId))
        {
            reference = null;
            return false;
        }

        var normalizedCapture = string.IsNullOrWhiteSpace(sourceCaptureRef)
            ? null
            : sourceCaptureRef.Trim();

        reference = new SourceReference(declaredKind, sourceVersionId.Trim(), normalizedCapture);
        return true;
    }
}
