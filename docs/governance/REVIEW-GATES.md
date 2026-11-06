# Audit-grade engineering review gates

## Gate 0 — Authority
- Correct repository, branch, Issue, explicit implementation authorization.
- Confirm applicable ADRs, unresolved domain questions, legal-source verification.
- No implementation based solely on a design document.

## Gate 1 — Data trust
- Original document checksum, type, period, import timestamp, version, source page/row anchors.
- Unverified parsing is visible; no manufactured identifiers or zeros.
- External source cannot silently overwrite canonical employment/financial data.

## Gate 2 — Accounting
- Positive/negative amounts preserved as printed, decimals not floats.
- Separate cash earning, deduction, employer contribution, reference base, calculated total, adjustment.
- Reconcile statement gross, deductions, net, contributions only under applicable report-specific equation.
- Cross-report same employee/period/bank group appropriately verified, not assumed by list position.
- Partial and duplicate processing idempotent with exact provenance.

## Gate 3 — Legal & temporal
- Correct legal regime, employment relationship, grade and entitlement date.
- Law/rule effective dates, independent revision/recording dates, clear decision/eligibility date.
- Retroactive calculation by component and underlying month with no duplicate entitlement.
- Explicit data insufficiency where legal applicability/rate/source not proven.

## Gate 4 — Reviewer control
- Distinct expected/source/reviewed/corrected/paid values.
- ReviewRequired is blocking where applicable; unknown payment cannot be called unpaid/paid.
- Adjustments and settled amounts are separate ledger events; immutable history.
- No payroll disbursement, payroll edit or automatic recovery from the review app.

## Gate 5 — Privacy and security
- No real financial records or identity numbers in git, logs, screenshots or synthetic fixtures.
- RBAC and separation of duties for rules, review, evidence, correction and administration.
- Integrity/retention of sources, decisions and audit logs.

## Gate 6 — Delivery
- Tests for boundary conditions, promotions, 2015 reference, arrears, duplicate imports, missing data and wrong source period.
- PR review includes actual diff + clear test evidence.
- Owner approval for breaking schema, data migrations, production access and merging.
