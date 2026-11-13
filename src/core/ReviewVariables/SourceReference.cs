namespace PayrollReviewSystem.Core.ReviewVariables;

/// <summary>
/// تصنيف قناة المصدر التي استُقي منها متغير المراجعة.
/// هذا التصنيف وصفي (Metadata) فقط ولا يُنشئ إثبات دفع ماليًا تلقائيًا ولا يسمح لإدخال Excel أو الشاشة بانتحال صفة Oracle الرسمي.
/// </summary>
public enum SourceKind
{
    /// <summary>كشف أوراكل الرسمي (ORACLE_OFFICIAL).</summary>
    OracleOfficial = 1,

    /// <summary>استيراد من ملف إكسل (EXCEL_IMPORT) — لا يُعد كشف أوراكل رسميًا.</summary>
    ExcelImport = 2,

    /// <summary>إدخال مباشر من شاشة المراجعة (DIRECT_FORM) — لا يُعد كشف أوراكل رسميًا.</summary>
    DirectForm = 3
}

/// <summary>
/// مرجع المصدر المرتبط بنسخة متغير المراجعة (بيانات وصفية فقط لحفظ تتبع الأصل دون تعديل مستندات أو استنتاج سداد).
/// </summary>
public sealed record SourceReference
{
    /// <summary>قناة المصدر الفعلية (<see cref="SourceKind"/>).</summary>
    public SourceKind Kind { get; }

    /// <summary>معرّف نسخة المصدر الذي أُخذ منه الإدخال أو التعديل.</summary>
    public string SourceVersionId { get; }

    /// <summary>مرجع إضافي اختياري للمستند أو جلسة الالتقاط (دون بيانات موظفين حساسة).</summary>
    public string? SourceCaptureRef { get; }

    /// <summary>
    /// يوضح صراحةً ما إذا كان المصدر كشف أوراكل رسميًا أم قناة بديلة؛
    /// تظل قناتا ExcelImport وDirectForm دائمًا false هنا لمنع الخلط المالي.
    /// </summary>
    public bool IsOfficialOracle => Kind == SourceKind.OracleOfficial;

    private SourceReference(SourceKind kind, string sourceVersionId, string? sourceCaptureRef)
    {
        Kind = kind;
        SourceVersionId = sourceVersionId;
        SourceCaptureRef = sourceCaptureRef;
    }

    /// <summary>
    /// ينشئ مرجع مصدر بعد التحقق من تعريف نوع القناة وصلاحية معرّف نسخة المصدر.
    /// </summary>
    public static bool TryCreate(
        SourceKind kind,
        string? sourceVersionId,
        string? sourceCaptureRef,
        out SourceReference? reference)
    {
        if (!Enum.IsDefined(kind) || string.IsNullOrWhiteSpace(sourceVersionId))
        {
            reference = null;
            return false;
        }

        var normalizedCapture = string.IsNullOrWhiteSpace(sourceCaptureRef)
            ? null
            : sourceCaptureRef.Trim();

        reference = new SourceReference(kind, sourceVersionId.Trim(), normalizedCapture);
        return true;
    }
}
