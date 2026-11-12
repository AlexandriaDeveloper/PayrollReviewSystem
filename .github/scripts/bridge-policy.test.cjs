'use strict';

// اختبارات وحدة اصطناعية لبوابة التشغيل دون اتصال Gemini أو GitHub أو أسرار.
const test = require('node:test');
const assert = require('node:assert/strict');
const {
  validateTrigger,
  findSource,
  alreadyProcessed,
  ERROR_CATEGORIES,
  classifyError,
  sanitizeErrorText,
  buildExecutionOutcome
} = require('./bridge-policy.cjs');

const base = Object.freeze({
  action: 'created',
  repository: 'AlexandriaDeveloper/PayrollReviewSystem',
  issueNumber: 41,
  isPullRequest: false,
  actor: 'AlexandriaDeveloper',
  commenter: 'AlexandriaDeveloper',
  commentId: 123,
  body: '/run-gemini AI-0005'
});

test('أمر صحيح من المالك على القناة يقبل', () => {
  assert.deepEqual(validateTrigger(base), { accepted: true, bridgeId: 'AI-0005' });
});
test('حساب غير مخوّل يرفض مهما كتب في الرسالة', () => {
  assert.equal(validateTrigger({ ...base, actor: 'other', commenter: 'other' }).accepted, false);
});
test('رقم Issue مختلف وPR يرفضان', () => {
  assert.equal(validateTrigger({ ...base, issueNumber: 40 }).accepted, false);
  assert.equal(validateTrigger({ ...base, isPullRequest: true }).accepted, false);
});
test('تعليق بوت أو رسالة عادية أو أمر يحمل سلسلة إضافية يرفض', () => {
  assert.equal(validateTrigger({ ...base, actor: 'github-actions[bot]' }).accepted, false);
  assert.equal(validateTrigger({ ...base, body: '[BRIDGE-ID: AI-0005]' }).accepted, false);
  assert.equal(validateTrigger({ ...base, body: '/run-gemini AI-0005\nrm -rf .' }).accepted, false);
});
test('لا يعمل عند تعديل تعليق قديم', () => {
  assert.equal(validateTrigger({ ...base, action: 'edited' }).accepted, false);
});
const comment = (id, body, login = 'AlexandriaDeveloper') => ({ id, body, user: { login } });
test('مصدر موثّق موجه إلى Gemini يطابق المعرف', () => {
  const body = '[BRIDGE-ID: AI-0005]\n[FROM: CHATGPT]\n[TO: GEMINI | OWNER]\n[TYPE: RESULT]\nالمطلوب: تقرير فقط';
  assert.equal(findSource([comment(5, body)], 'AI-0005')?.id, 5);
  assert.equal(findSource([comment(5, body)], 'AI-0004'), null);
  assert.equal(findSource([comment(5, body, 'visitor')], 'AI-0005'), null);
});
test('التعليقات غير الموثّقة لا تُمرّر إلى Gemini', () => {
  assert.equal(findSource([comment(6, '[BRIDGE-ID: AI-0005]\n[FROM: OWNER]\n[TO: GEMINI]')], 'AI-0005'), null);
});
test('منع تكرار الرد آمن من انتحال غير البوت', () => {
  const marker = '[TRIGGER-COMMENT-ID: 123]';
  assert.equal(alreadyProcessed([comment(1, marker, 'visitor')], 123), false);
  assert.equal(alreadyProcessed([comment(2, marker, 'github-actions[bot]')], 123), true);
  assert.equal(alreadyProcessed([comment(2, marker, 'github-actions[bot]')], 124), false);
});

test('تنقيح الأسرار والمفاتيح والرموز الحساسة من رسائل الخطأ', () => {
  const customSecret = 'SECRET_TOKEN_99999';
  const rawMsg = [
    '\u001b[31mFatalError:\u001b[0m Request failed with status 403',
    'at https://generativelanguage.googleapis.com/v1beta/models?key=AIzaSyD1234567890abcdef1234567890abcde',
    'Invalid API key provided: AIzaSyD9876543210zyxwvu9876543210zyxwv',
    'Headers: { Authorization: "Bearer ya29.a0AfH6SMD_custom_bearer_token" }',
    'Custom secret value: SECRET_TOKEN_99999',
    'GitHub Token: ghp_123456789012345678901234567890123456'
  ].join('\n');

  const clean = sanitizeErrorText(rawMsg, [customSecret]);

  assert.equal(clean.includes('AIzaSyD'), false, 'يجب حجب مفتاح Google API');
  assert.equal(clean.includes('ya29.'), false, 'يجب حجب Bearer token');
  assert.equal(clean.includes('SECRET_TOKEN_99999'), false, 'يجب حجب السر المخصص');
  assert.equal(clean.includes('ghp_'), false, 'يجب حجب رمز GitHub');
  assert.equal(clean.includes('\u001b[31m'), false, 'يجب حذف أكواد ANSI');
  assert.equal(clean.includes('?key=[REDACTED_KEY]'), true);
  assert.equal(clean.includes('[REDACTED_API_KEY]'), true);
  assert.equal(clean.includes('[REDACTED_SECRET]'), true);
  assert.equal(clean.includes('[REDACTED_TOKEN]'), true);
  assert.equal(clean.includes('[REDACTED_GITHUB_TOKEN]'), true);
});

test('الحفاظ على رسائل الخطأ التشخيصية غير الحساسة دون تشويه', () => {
  const safeMsg = 'FatalUntrustedWorkspaceError: Gemini CLI is not running in a trusted directory. To proceed, either use --skip-trust, set the GEMINI_CLI_TRUST_WORKSPACE=true environment variable, or trust this directory in interactive mode.';
  const clean = sanitizeErrorText(safeMsg);
  assert.equal(clean, safeMsg);
});

test('تقليص رسائل الخطأ الطويلة جدًا لحماية السجل وسعة التعليقات', () => {
  const veryLong = 'A'.repeat(2000);
  const clean = sanitizeErrorText(veryLong);
  assert.equal(clean.length < 900, true);
  assert.equal(clean.includes('[تم تقليص الرسالة للسلامة والإيجاز]'), true);
});

test('تصنيف الأخطاء بدقة وفق القائمة المغلقة ERROR_CATEGORIES', () => {
  // خطأ موثوقية مسار العمل
  assert.equal(classifyError({ exitCode: 55, stderrText: 'FatalUntrustedWorkspaceError' }).code, 'TRUST_FAILURE');
  assert.equal(classifyError({ exitCode: 1, stderrText: 'not running in a trusted directory' }).code, 'TRUST_FAILURE');

  // خطأ المصادقة ومفتاح API
  assert.equal(classifyError({ exitCode: 1, stderrText: 'code: 41\nPlease set an Auth method' }).code, 'AUTH_FAILURE');
  assert.equal(classifyError({ exitCode: 1, stderrText: 'API key not valid' }).code, 'AUTH_FAILURE');
  assert.equal(classifyError({ exitCode: 1, stderrText: 'Request failed with status 401 UNAUTHENTICATED' }).code, 'AUTH_FAILURE');

  // خطأ الحصة المخصصة ومعدل الطلبات
  assert.equal(classifyError({ exitCode: 1, stderrText: 'RESOURCE_EXHAUSTED: quota exceeded' }).code, 'QUOTA_FAILURE');
  assert.equal(classifyError({ exitCode: 1, stderrText: 'status 429 Too Many Requests' }).code, 'QUOTA_FAILURE');

  // خطأ الشبكة وانقطاع الاتصال
  assert.equal(classifyError({ exitCode: 1, stderrText: 'fetch failed: ETIMEDOUT' }).code, 'NETWORK_FAILURE');
  assert.equal(classifyError({ exitCode: 1, stderrText: 'ENOTFOUND generativelanguage.googleapis.com' }).code, 'NETWORK_FAILURE');

  // خطأ غير متوقع عام
  assert.equal(classifyError({ exitCode: 99, stderrText: 'something completely generic' }).code, 'CLI_UNEXPECTED_ERROR');
});

test('SEC-01: منع تسريب المسارات الشخصية والرموز غير النمطية من stderr إلى الخارج', () => {
  const privatePath = '/home/user_personal/payroll/internal/data.json';
  const customWeirdToken = 'CUSTOM_NON_STANDARD_BEARER_998877665544332211';
  const sensitiveStderr = `Error at ${privatePath} with bearer ${customWeirdToken}: FatalUntrustedWorkspaceError occurred`;

  const outcome = buildExecutionOutcome({
    exitCode: 55,
    stdoutText: '',
    stderrText: sensitiveStderr
  });

  assert.equal(outcome.status, 'ERROR');
  assert.equal(outcome.category, 'TRUST_FAILURE');
  assert.equal(outcome.text.includes('TRUST_FAILURE'), true);

  // تحقق حاسم: عدم وجود أي أثر للمسار الشخصي أو الرمز غير النمطي في نص النتيجة النهائي
  assert.equal(outcome.text.includes(privatePath), false, 'ممنوع ظهور المسارات الشخصية في نص النتيجة');
  assert.equal(outcome.text.includes(customWeirdToken), false, 'ممنوع ظهور الرموز غير النمطية في نص النتيجة');
  assert.equal(outcome.text.includes('user_personal'), false);
  assert.equal(outcome.text.includes('998877665544332211'), false);
});

test('بناء النتيجة عند خروج Gemini CLI برمز الخطأ 55 (مساحة عمل غير موثوقة)', () => {
  const stderr = 'FatalUntrustedWorkspaceError: Gemini CLI is not running in a trusted directory.';
  const outcome = buildExecutionOutcome({
    exitCode: 55,
    stdoutText: '',
    stderrText: stderr
  });

  assert.equal(outcome.status, 'ERROR');
  assert.equal(outcome.category, 'TRUST_FAILURE');
  assert.equal(outcome.text.includes('رمز الخروج: 55'), true);
  assert.equal(outcome.text.includes('TRUST_FAILURE'), true);
  // تأكيد أن نص stderr الحر لم يُحقن في النتيجة
  assert.equal(outcome.text.includes('FatalUntrustedWorkspaceError'), false);
});

test('بناء النتيجة عند حدوث خطأ مصادقة أو مفتاح API', () => {
  const stderr = 'Error: Invalid API key in request with status 403 PERMISSION_DENIED';
  const outcome = buildExecutionOutcome({
    exitCode: 1,
    stdoutText: '',
    stderrText: stderr
  });

  assert.equal(outcome.status, 'ERROR');
  assert.equal(outcome.category, 'AUTH_FAILURE');
  assert.equal(outcome.text.includes('AUTH_FAILURE'), true);
  assert.equal(outcome.text.includes('رمز الخروج: 1'), true);
});

test('بناء النتيجة عند نجاح Gemini CLI ورد صالح', () => {
  const sampleReply = 'تمت مراجعة الوثائق ولا توجد موانع.';
  const stdout = JSON.stringify({ response: sampleReply });
  const outcome = buildExecutionOutcome({
    exitCode: 0,
    stdoutText: stdout,
    stderrText: ''
  });

  assert.equal(outcome.status, 'ANSWERED');
  assert.equal(outcome.text, sampleReply);
});

test('بناء النتيجة عند خروج 0 لكن مع تعذر تحليل JSON أو رد فارغ', () => {
  const invalidJson = buildExecutionOutcome({
    exitCode: 0,
    stdoutText: 'not a json',
    stderrText: ''
  });
  assert.equal(invalidJson.status, 'ERROR');
  assert.equal(invalidJson.category, 'MALFORMED_OUTPUT');
  assert.equal(invalidJson.text.includes('MALFORMED_OUTPUT'), true);

  const emptyResponse = buildExecutionOutcome({
    exitCode: 0,
    stdoutText: JSON.stringify({ response: '   ' }),
    stderrText: ''
  });
  assert.equal(emptyResponse.status, 'ERROR');
  assert.equal(emptyResponse.category, 'EMPTY_RESPONSE');
  assert.equal(emptyResponse.text.includes('EMPTY_RESPONSE'), true);

  const errorInJson = buildExecutionOutcome({
    exitCode: 0,
    stdoutText: JSON.stringify({ error: { message: 'Resource exhausted (quota)' } }),
    stderrText: ''
  });
  assert.equal(errorInJson.status, 'ERROR');
  assert.equal(errorInJson.category, 'QUOTA_FAILURE');
  assert.equal(errorInJson.text.includes('QUOTA_FAILURE'), true);
});

