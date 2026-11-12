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

module.exports = { validateTrigger, findSource, alreadyProcessed };
