'use strict';

// هوية القناة والحساب المخوّل ثابتتان في المرحلة الأولى؛ لا تُؤخذان من تعليق المستخدم.
const OWNER = 'AlexandriaDeveloper';
const ISSUE_NUMBER = 41;
const COMMAND_RE = /^\/run-gemini (AI-\d{4,6})$/;
const SOURCE_RE = /^\[BRIDGE-ID: (AI-\d{4,6})\]$/m;
const FROM_RE = /^\[FROM: CHATGPT\]$/m;
const TO_RE = /^\[TO: GEMINI(?:\s*\|[^\]]*)?\]$/m;

/**
 * يتحقق من بيانات الحدث وسلطة المرسل وصيغة الأمر قبل أي استخدام للأسرار.
 * لا يُفسر نص التعليق كتعليمات برمجية أو أوامر نظام.
 */
function validateTrigger(event) {
  if (!event || event.action !== 'created' ||
    event.repository !== 'AlexandriaDeveloper/PayrollReviewSystem' ||
    event.issueNumber !== ISSUE_NUMBER || event.isPullRequest ||
    event.actor !== OWNER || event.commenter !== OWNER ||
    !Number.isSafeInteger(event.commentId) || event.commentId <= 0) {
    return { accepted: false, reason: 'UNAUTHORIZED_OR_UNRELATED' };
  }
  const body = typeof event.body === 'string' ? event.body : '';
  const match = COMMAND_RE.exec(body.trim());
  if (!match || body.length > 120) {
    return { accepted: false, reason: 'INVALID_COMMAND' };
  }
  return { accepted: true, bridgeId: match[1] };
}

/**
 * يعثر على رسالة ChatGPT الموجهة إلى Gemini، المنشورة من حساب GitHub المخوّل.
 * أحدث رسالة مطابقة هي المصدر؛ لا نقرأ أسماء أو تعليمات من تعليق غير مخوّل.
 */
function findSource(comments, bridgeId) {
  if (!/^AI-\d{4,6}$/.test(bridgeId) || !Array.isArray(comments)) return null;
  const hits = comments.filter((item) => {
    const body = item && typeof item.body === 'string' ? item.body : '';
    return item?.user?.login === OWNER &&
      body.length <= 12000 &&
      SOURCE_RE.exec(body)?.[1] === bridgeId &&
      FROM_RE.test(body) && TO_RE.test(body) &&
      Number.isSafeInteger(item.id) && item.id > 0;
  });
  return hits.length ? hits[hits.length - 1] : null;
}

/**
 * يمنع معالجة تعليق الأمر نفسه مرة أخرى إذا كان البوت قد نشر نتيجته.
 * لا يستطيع تعليق من شخص آخر أن يمنع أمرًا مخوّلًا عن طريق انتحال الوسم.
 */
function alreadyProcessed(comments, triggerCommentId) {
  const marker = '[TRIGGER-COMMENT-ID: ' + triggerCommentId + ']';
  return Array.isArray(comments) && comments.some((item) =>
    item?.user?.login === 'github-actions[bot]' &&
    typeof item.body === 'string' &&
    item.body.split('\n').some((line) => line.trim() === marker));
}

/**
 * قائمة مغلقة لتصنيفات الأخطاء الآمنة دون تسريب نصوص stderr أو مسارات أو بيانات حساسة.
 */
const ERROR_CATEGORIES = Object.freeze({
  TRUST_FAILURE: {
    code: 'TRUST_FAILURE',
    ar: 'فشل في التحقق من موثوقية مسار العمل (Workspace Trust)'
  },
  AUTH_FAILURE: {
    code: 'AUTH_FAILURE',
    ar: 'فشل في المصادقة أو اعتماد مفتاح الوصول (Authentication/API Key)'
  },
  QUOTA_FAILURE: {
    code: 'QUOTA_FAILURE',
    ar: 'استنزاف الحصة المخصصة أو تجاوز حدود الاستخدام (Quota/Rate Limit)'
  },
  NETWORK_FAILURE: {
    code: 'NETWORK_FAILURE',
    ar: 'تعذر الاتصال بالشبكة أو انقطاع المهلة (Network/Connection Timeout)'
  },
  MALFORMED_OUTPUT: {
    code: 'MALFORMED_OUTPUT',
    ar: 'تعذر تحليل رد Gemini بصيغة JSON صالحة'
  },
  EMPTY_RESPONSE: {
    code: 'EMPTY_RESPONSE',
    ar: 'لم يُرجع النموذج أي محتوى نصي صالح'
  },
  CLI_UNEXPECTED_ERROR: {
    code: 'CLI_UNEXPECTED_ERROR',
    ar: 'خطأ تشغيلي غير متوقع في أداة Gemini CLI'
  }
});

/**
 * يصنف الخطأ إلى فئة مغلقة وآمنة بالاعتماد على كود الخروج والمؤشرات التقنية
 * دون تسريب نصوص stderr الحرة أو المسارات أو الأسرار إلى الخارج.
 */
function classifyError({ exitCode, stdoutText, stderrText }) {
  const code = Number(exitCode);
  const combined = ((typeof stderrText === 'string' ? stderrText : '') + ' ' +
    (typeof stdoutText === 'string' ? stdoutText : '')).toLowerCase();

  if (code === 55 || combined.includes('fataluntrustedworkspaceerror') || combined.includes('trusted directory')) {
    return ERROR_CATEGORIES.TRUST_FAILURE;
  }

  if (combined.includes('code: 41') || combined.includes('code": 41') ||
    combined.includes('please set an auth method') || combined.includes('api key not valid') ||
    combined.includes('unauthenticated') || combined.includes('permission_denied') ||
    combined.includes('status 401') || combined.includes('status 403') ||
    combined.includes('status": 401') || combined.includes('status": 403')) {
    return ERROR_CATEGORIES.AUTH_FAILURE;
  }

  if (combined.includes('resource_exhausted') || combined.includes('quota') ||
    combined.includes('rate limit') || combined.includes('status 429') || combined.includes('status": 429')) {
    return ERROR_CATEGORIES.QUOTA_FAILURE;
  }

  if (combined.includes('enotfound') || combined.includes('etimedout') ||
    combined.includes('econnrefused') || combined.includes('fetch failed') || combined.includes('network error')) {
    return ERROR_CATEGORIES.NETWORK_FAILURE;
  }

  return ERROR_CATEGORIES.CLI_UNEXPECTED_ERROR;
}

/**
 * ينقح النصوص ورسائل الخطأ من أي أسرار أو مفاتيح أو بيانات حساسة قبل أي استخدام محلي.
 */
function sanitizeErrorText(text, secrets = []) {
  if (typeof text !== 'string') return '';
  let sanitized = text;

  // إزالة رموز التحكم وANSI escape codes
  sanitized = sanitized.replace(/\x1B\[[0-9;]*[a-zA-Z]/g, '');

  // تنقيح الأسرار المحددة في البيئة إن وجدت
  if (Array.isArray(secrets)) {
    for (const secret of secrets) {
      if (typeof secret === 'string' && secret.length >= 6) {
        sanitized = sanitized.split(secret).join('[REDACTED_SECRET]');
      }
    }
  }

  // تنقيح أنماط المفاتيح والرموز الحساسة الشائعة
  sanitized = sanitized
    .replace(/([?&]key=)[^&\s]+/gi, '$1[REDACTED_KEY]')
    .replace(/(bearer\s+)[a-z0-9._-]+/gi, '$1[REDACTED_TOKEN]')
    .replace(/AIza[0-9A-Za-z-_]{30,40}/g, '[REDACTED_API_KEY]')
    .replace(/ghp_[a-zA-Z0-9]{36}/g, '[REDACTED_GITHUB_TOKEN]')
    .replace(/github_pat_[a-zA-Z0-9_]{82}/g, '[REDACTED_GITHUB_TOKEN]');

  // تقليم المسافات وتحديد الحد الأقصى للطول
  sanitized = sanitized.trim();
  if (sanitized.length > 800) {
    sanitized = sanitized.slice(0, 800) + '... [تم تقليص الرسالة للسلامة والإيجاز]';
  }
  return sanitized;
}

/**
 * يحلل مخرجات تشغيل Gemini CLI ويبني نتيجة محددة خالية تمامًا من أي نصوص حرة أو أسرار،
 * معتمدة على قائمة تصنيفات مغلقة ورموز خروج رقمية فقط.
 */
function buildExecutionOutcome({ exitCode, stdoutText, stderrText }) {
  const code = Number(exitCode);

  let parsedJson = null;
  if (typeof stdoutText === 'string' && stdoutText.trim()) {
    try {
      parsedJson = JSON.parse(stdoutText.trim());
    } catch {
      // ليس JSON صالحاً
    }
  }

  if (code !== 0) {
    const category = classifyError({ exitCode: code, stdoutText, stderrText });
    return {
      status: 'ERROR',
      category: category.code,
      exitCode: code,
      text: `[تصنيف الخطأ: ${category.code}] ${category.ar}. رمز الخروج: ${code}. لمزيد من التفاصيل راجع سجل Actions الموثّق.`
    };
  }

  if (!parsedJson) {
    const category = ERROR_CATEGORIES.MALFORMED_OUTPUT;
    return {
      status: 'ERROR',
      category: category.code,
      exitCode: 0,
      text: `[تصنيف الخطأ: ${category.code}] ${category.ar}. لا تُعلن نجاحًا.`
    };
  }

  if (parsedJson.error) {
    const category = classifyError({ exitCode: 0, stdoutText, stderrText });
    return {
      status: 'ERROR',
      category: category.code,
      exitCode: 0,
      text: `[تصنيف الخطأ: ${category.code}] أرجع النموذج خطأ داخليًا: ${category.ar}.`
    };
  }

  const response = typeof parsedJson.response === 'string' ? parsedJson.response.trim() : '';
  if (!response) {
    const category = ERROR_CATEGORIES.EMPTY_RESPONSE;
    return {
      status: 'ERROR',
      category: category.code,
      exitCode: 0,
      text: `[تصنيف الخطأ: ${category.code}] ${category.ar}؛ لا يُعد هذا تنفيذًا ناجحًا.`
    };
  }

  return {
    status: 'ANSWERED',
    text: response.slice(0, 14000)
  };
}

module.exports = {
  validateTrigger,
  findSource,
  alreadyProcessed,
  ERROR_CATEGORIES,
  classifyError,
  sanitizeErrorText,
  buildExecutionOutcome
};

