'use strict';

// اختبارات وحدة اصطناعية لبوابة التشغيل دون اتصال Gemini أو GitHub أو أسرار.
const test = require('node:test');
const assert = require('node:assert/strict');
const { validateTrigger, findSource, alreadyProcessed } = require('./bridge-policy.cjs');

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
