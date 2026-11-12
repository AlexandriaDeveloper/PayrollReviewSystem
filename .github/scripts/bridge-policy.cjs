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
 * ينقح النصوص ورسائل الخطأ من أي أسرار أو مفاتيح أو بيانات حساسة قبل نشرها.
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
 * يحلل مخرجات تشغيل Gemini CLI ويبني نتيجة محددة خالية من الأسرار.
 */
function buildExecutionOutcome({ exitCode, stdoutText, stderrText, secretKeys = [] }) {
  const code = Number(exitCode);

  let rawError = (typeof stderrText === 'string' ? stderrText.trim() : '');
  let parsedJson = null;
  if (typeof stdoutText === 'string' && stdoutText.trim()) {
    try {
      parsedJson = JSON.parse(stdoutText.trim());
    } catch {
      // ليس JSON صالحاً
    }
  }

  if (parsedJson && parsedJson.error) {
    const jsonErrMsg = parsedJson.error.message || JSON.stringify(parsedJson.error);
    rawError = rawError ? (rawError + '\n' + jsonErrMsg) : jsonErrMsg;
  }

  const cleanError = sanitizeErrorText(rawError, secretKeys);

  if (code !== 0) {
    const reasonText = cleanError ? ` السبب: ${cleanError}` : '';
    return {
      status: 'ERROR',
      text: `تعذر تشغيل Gemini CLI بنجاح (exit code: ${code}).${reasonText}`
    };
  }

  if (!parsedJson) {
    const reasonText = cleanError ? ` السبب: ${cleanError}` : '';
    return {
      status: 'ERROR',
      text: `تعذر تحليل رد Gemini بصيغة JSON؛ لا تُعلن نجاحًا.${reasonText}`
    };
  }

  if (parsedJson.error) {
    const reasonText = cleanError ? ` السبب: ${cleanError}` : '';
    return {
      status: 'ERROR',
      text: `أرجع Gemini خطأ داخليًا.${reasonText}`
    };
  }

  const response = typeof parsedJson.response === 'string' ? parsedJson.response.trim() : '';
  if (!response) {
    return {
      status: 'ERROR',
      text: 'Gemini لم يرجع response نصيًا صالحًا؛ لا يُعد هذا تنفيذًا ناجحًا.'
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
  sanitizeErrorText,
  buildExecutionOutcome
};

