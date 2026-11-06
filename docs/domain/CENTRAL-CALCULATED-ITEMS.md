# Centrally calculated deductions — source-trusted amounts (design)

**Owner-confirmed scope, 2026-11-06. Documentation only; no coding authorization.**

## Core policy
Some financial items are calculated by **central entitlements**. The accounting-unit payroll reviewers do **not** independently calculate or approve/reject the legal formula/rate of these items. Instead, the new application imports the central figure as a **source-trusted amount**, within the verified report and scope.

This is a rule about **responsibility for recalculation**, not blanket trust in corrupt or mismatched PDF extraction. Data completeness, sign, identity, month, duplicates, arithmetic reconciliation, and report authenticity remain subject to import-quality checks.

## Owner-named items
| Owner's wording | Proposed treatment | Exact Oracle mapping |
| --- | --- | --- |
| صندوق دعم المعاقين | CENTRAL_SOURCE_AMOUNT; do not independently recalculate contribution/deduction | **UNVERIFIED** — obtain official printed label, eight-digit code and source column from actual report |
| صندوق الشهداء | CENTRAL_SOURCE_AMOUNT; do not independently recalculate contribution/deduction | **UNVERIFIED** — obtain official printed label, code and source column |
| معاشات/تأمينات (general) | CENTRAL_SOURCE_AMOUNT; no general recomputation | Only as verified in original report |
| معاشات/تأمينات (part-time exceptional case) | General source amount remains unchanged; run **separate exceptional assessment-base control** when law and evidence permit | See PART-TIME-INSURANCE-CONTROL.md |

The Owner's names are colloquial business identifiers. Do not invent official legal names, account codes, rates, expenditure chapters, charge allocation or whether each line is a deduction/employer cost until the real report mapping is confirmed. Do not collapse 'صندوق دعم المعاقين' and 'صندوق الشهداء' into one item even if report code/text is ambiguous; flag for manual source mapping.

## Proposed item control policy
Component audit treatment should be date/scoped/versioned, not hard-coded in the employee entity:
- \`INDEPENDENT_CALCULATION\` — reviewer supplies verified facts; engine proposes formula-based expected value.
- \`CENTRAL_SOURCE_AMOUNT\` — the report's verified amount is carried as source observation; **no independent expected calculation**. The absence of a locally calculated figure is NOT discrepancy and is NOT expected amount 0.
- \`REFERENCE_ONLY\` — non-payable factual financial base (e.g. frozen basic 30-06-2015).
- \`EXCEPTION_CONTROL\` — specifically approved audit check with separate legal and factual conditions, never a blanket general-rate review.

Such policy may vary by institution, payment kind, employee legal regime, effective period and actual observed Oracle line. Reviewer-authorized changes require versioning and audit event, not hidden runtime override.

## Required reviewer UX
Show the value from Oracle as reported, with badge/text "يُقبل من الاستحقاقات المركزية دون إعادة احتساب" ONLY after mapping/report quality verification. It should still be included once in appropriate gross/deduction/total equations according to actual report classification. Do not create a fictitious ExpectedAmount equal to SourceAmount and then claim the value was independently validated.

If the import is missing/unreadable/duplicate, label **SourceUnverified / ReviewRequired**. A user may still report a concern or request official correction. The exception does not remove data quality checks, suspicious inconsistencies or financial oversight of the overall payroll.

## Acceptance tests
- **CM-001:** a readable, correctly classified named central-source item is shown/imported without independent rate recomputation or automatic legal discrepancy.
- **CM-002:** no local expected formula exists for central source item -> no false zero or missing-calculation blocker.
- **CM-003:** duplicate imported contribution -> report duplicate/amount control, do not double-deduct.
- **CM-004:** amount sign or two-column RTL misparsed -> reject/ReviewRequired, never silently trust a corrupt extraction.
- **CM-005:** source line belongs to different month/employee -> mismatch despite central-source calculation policy.
- **CM-006:** unknown ambiguous Oracle account/label for either fund -> mapping review; no guessed official code.
- **CM-007:** social-insurance general item -> no general recomputation; part-time case with verified full-base mismatch -> separate flagged objection.
- **CM-008:** central item included in total deduction -> aggregate financial reconciliation still validates total exactly once.
- **CM-009:** central-source item changes in an official corrected PDF -> preserve original source version, import corrected value and track reviewer follow-up.
