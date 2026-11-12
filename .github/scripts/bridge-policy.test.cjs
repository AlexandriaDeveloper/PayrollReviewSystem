'use strict';

// اختبارات وحدة اصطناعية لبوابة التشغيل دون اتصال Gemini أو GitHub أو أسرار.
const test = require('node:test');
const assert = require('node:assert/strict');
const {
  validateTrigger,
  findSource,
  alreadyProcessed,
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

test('بناء النتيجة عند خروج Gemini CLI برمز الخطأ 55 (مساحة عمل غير موثوقة)', () => {
  const stderr = 'FatalUntrustedWorkspaceError: Gemini CLI is not running in a trusted directory.';
  const outcome = buildExecutionOutcome({
    exitCode: 55,
    stdoutText: '',
    stderrText: stderr
  });

  assert.equal(outcome.status, 'ERROR');
  assert.equal(outcome.text.includes('exit code: 55'), true);
  assert.equal(outcome.text.includes('FatalUntrustedWorkspaceError'), true);
});

test('بناء النتيجة عند حدوث خطأ يتضمن مفتاح API في stderr', () => {
  const stderr = 'Error: Invalid API key AIzaSyD1234567890abcdef1234567890abcde in request';
  const outcome = buildExecutionOutcome({
    exitCode: 1,
    stdoutText: '',
    stderrText: stderr,
    secretKeys: []
  });

  assert.equal(outcome.status, 'ERROR');
  assert.equal(outcome.text.includes('exit code: 1'), true);
  assert.equal(outcome.text.includes('AIzaSyD'), false, 'ممنوع ظهور المفتاح في نص النتيجة');
  assert.equal(outcome.text.includes('[REDACTED_API_KEY]'), true);
});

test('بناء النتيجة مع حجب الأسرار المحددة صراحة في secretKeys', () => {
  const customSecret = 'SUPER_SECRET_VALUE_12345';
  const stderr = 'Connection failed to host with token: SUPER_SECRET_VALUE_12345';
  const outcome = buildExecutionOutcome({
    exitCode: 2,
    stdoutText: '',
    stderrText: stderr,
    secretKeys: [customSecret]
  });

  assert.equal(outcome.status, 'ERROR');
  assert.equal(outcome.text.includes('exit code: 2'), true);
  assert.equal(outcome.text.includes('SUPER_SECRET_VALUE_12345'), false);
  assert.equal(outcome.text.includes('[REDACTED_SECRET]'), true);
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
  assert.equal(invalidJson.text.includes('تعذر تحليل رد Gemini بصيغة JSON'), true);

  const emptyResponse = buildExecutionOutcome({
    exitCode: 0,
    stdoutText: JSON.stringify({ response: '   ' }),
    stderrText: ''
  });
  assert.equal(emptyResponse.status, 'ERROR');
  assert.equal(emptyResponse.text.includes('لم يرجع response نصيًا صالحًا'), true);

  const errorInJson = buildExecutionOutcome({
    exitCode: 0,
    stdoutText: JSON.stringify({ error: { message: 'Resource exhausted (quota)' } }),
    stderrText: ''
  });
  assert.equal(errorInJson.status, 'ERROR');
  assert.equal(errorInJson.text.includes('Resource exhausted (quota)'), true);
});

