using PayrollReviewSystem.Application.ReviewVariables.Ports;
using PayrollReviewSystem.Core.ReviewVariables;

namespace PayrollReviewSystem.Application.ReviewVariables.Policies;

/// <summary>
/// سياسة التفويض الافتراضية لمتغيرات المراجعة في شريحة CODE-001.
/// تطبّق قرار المالك AI-0093 / D-035:
/// - أي مراجع مخوّل داخل نطاقه (<see cref="TrustedReviewerContext.AuthorizedScopes"/>) يملك تعديل أو تسجيل المتغيرات فورًا
///   دون أي بوابة موافقة إجبارية من «مراجع أساسي».
/// - في المقابل، تمنع أي مستخدم غير مخوّل بالتعديل أو خارج نطاق وحدته/مجموعته.
/// - لا تمنح هذه السياسة أي صلاحيات تخص D-031 أو إثبات السداد البنكي (D-033) أو اعتماد أوامر التسوية (D-034).
/// </summary>
public sealed class DefaultReviewerAuthorizationPolicy : IAuthorizationPolicyPort
{
    /// <inheritdoc />
    public AuthorizationDecision Evaluate(
        TrustedReviewerContext context,
        ReviewVariableOperation operation,
        ActorScope targetScope)
    {
        if (context is null
            || context.ActorId is null
            || targetScope is null
            || context.AuthorizedScopes is null)
        {
            return AuthorizationDecision.Deny("سياق التفويض أو النطاق المستهدف غير مكتمل.");
        }

        if (!context.AuthorizedScopes.Contains(targetScope))
        {
            return AuthorizationDecision.Deny(
                $"المراجع '{context.ActorId.Value}' غير مخوّل للعمل داخل النطاق '{targetScope.ToCanonicalString()}'.");
        }

        return operation switch
        {
            ReviewVariableOperation.ViewReviewVariableHistory => AuthorizationDecision.Allow(),
            ReviewVariableOperation.RecordReviewVariable or ReviewVariableOperation.ReviseReviewVariable =>
                context.CanModifyReviewVariables
                    ? AuthorizationDecision.Allow()
                    : AuthorizationDecision.Deny(
                        $"الدور '{context.RoleName}' لا يملك صلاحية إدخال أو تعديل متغيرات المراجعة."),
            _ => AuthorizationDecision.Deny("العملية غير مدعومة في سياسة تفويض متغيرات المراجعة.")
        };
    }
}
