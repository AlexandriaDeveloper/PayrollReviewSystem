# OraclePdfParser: audited failure catalogue and remediation plan

**Analysis date:** 2026-11-06
**Reviewed upstream:** [OraclePdfParser @ 23f75d7354cb0a5981d02b35b1a5942ab9c0eeed](https://github.com/AlexandriaDeveloper/OraclePdfParser/tree/23f75d7354cb0a5981d02b35b1a5942ab9c0eeed)
**Sources:** upstream current code/test files, PR #1–#4 discussions and inline reviews, earlier private architect diagnosis \`oracle-parser-review-ar.md\` and PDF-EXTRACTOR source. This is an engineering audit of the current code and known failures, **NOT** proof that all real PDF variants work or fail. No code changed in OraclePdfParser.

## Baseline decision
**Keep OraclePdfParser as the preferred EXISTING starting codebase**, specifically the PDFium Unicode extraction, Arabic parser, three report types, .NET 10 contracts and tests, *while correcting and broadening its extraction architecture*. Do not restart with a speculative parser from scratch; do not take upstream \`Verified\` as legally valid or as proof that every employee item is accurate.

PDF-EXTRACTOR is **complementary** evidence for visual/physical table cells. The existing C# parser is a flattened text stream and must gain a layout-aware fallback/adapter for two-pane Arabic rows; neither extraction strategy should rewrite the other's evidence silently.

## Source-supported historical progression
- PR #1: initial .NET + Angular + PDFium text extraction, 3 types, local SQLite and private verifier. PR review recorded concerns: Angular zone polyfill, empty payment pages and duplicate payment-sheet employee IDs.
- PR #2: net10 migration. No new extraction functionality proven.
- PR #3: fix for wrongly treating **employee header metadata as informational salary rows**, and distinguish entity code (e.g. 30200105) from financial account code. Helpful test coverage added.
- PR #4: item-selective XLSX export. Review recorded **employee-subset privacy leak via report-wide notes**. No fix for that is present in upstream exporter logic.
- GitHub Actions builds/tests completed successfully for latest main, but private 2026 payroll PDFs were **not** run through the compiled importer in this review. A prior independent review says 24 upstream tests and 5 diagnostic tests executed locally; the 5 diagnostics reproduced existing failures. Avoid claiming fixes based on green CI alone.
- Earlier trials on PDF-EXTRACTOR: semantically strict text-only extraction could return **zero items on a different 386-page source**; physical-table recovery restored provisional items, explicitly marked ReviewRequired. The 438-page reference set was a different layout/fixture. This is evidence for multi-adapter behavior, not proof the C# parser also fails the identical 386-page source.

## Confirmed source-code defects / previously reproduced defects

| ID | Severity | Finding | Code location / reason | Required outcome |
| --- | --- | --- | --- | --- |
| F-01 | **BLOCKER** | **Negative value loses sign** | \`PayrollImportService.cs\` Money/Currency regex omit minus and Flush extracts positive substring. Previously reproduced with synthetic -100 turning +100. | Signed decimal/token with negative, parentheses, trailing minus and RTL sign variants; preserve original sign and flag unparseable. |
| F-02 | **BLOCKER** | **Gross / deductions not cross-compared** | \`PayrollBatchReconciler.cs\` compares payslip/payment-sheet net and payment reference, not both gross/deduction figures; previously reproduced source gross 100/deduction 20 vs 120/40, net 80 in both, both Verified. | Compare all **comparable** report-defined figures, with separate government-share semantics; never net-only Verified. |
| F-03 | **BLOCKER** | **Bank group identity ignored** | \`PayrollBatchReconciler.cs\` compares \`cover.Groups[i]\` and \`sheet.Groups[i]\` by index, amount/count only; earlier synthetic equal totals for different bank/branch passed. | Verify bank+branch+group reference and entity/period, handle reordered or repeated group identities; unresolved stays ReviewRequired. |
| F-04 | **BLOCKER** | **Unverified payslip name overrides payment-sheet source name** | \`PayrollBatchReconciler.cs\` builds canonical national-id dictionary from all unique national IDs regardless of payslip status/name warnings; previous reproduced case with bad name overrode a Verified payment-sheet name. | Source name immutable; canonical identity only from separately verified person mapping, not an unverified payslip. |
| F-05 | **BLOCKER** | **Rejected becomes ReviewRequired** | \`PayrollBatchReconciler.cs\` unconditionally assigns ReviewRequired when any issues are present; unknown report Rejected has issues and is downgraded after Reconcile. Previously reproduced. | Preserve Rejected as terminal import classification; separate blocking parse status from item review. |
| F-06 | **MAJOR** | **Same code and label repeated lacks line-instance identity** | \`PayrollEntry\` has code/label/amount/kind/page/raw text only, no line ID. Excel ItemKey = Kind+AccountCode+Label and sums identical matches. | Keep every occurrence separately; allow **explicit optional reporting aggregation**, never overwrite/discard a source row. |
| F-07 | **MAJOR** | **Exact Arabic label keys fragment equivalent components** | Excel ItemKey includes raw label string. Arabic mirror/order, glyph variations, whitespace, dates, 'x' summary remnants yield separate keys; direct label normalization is insufficient as sole identity. | Separate raw/normalized/display labels; versioned reviewer-approved aliases + confidence and provenance. |
| F-08 | **MAJOR** | **Last numeric token can be wrong amount** | \`Flush()\` scans all digits and picks last decimal-looking token, with incomplete handling of trailing notes, percentages, years and interleaved two-pane data. | Extract from **spatial same-row/cell relation**, with exact amount token/source coordinates; ambiguous row stays unresolved. |
| F-09 | **MAJOR** | **Text-only reader lacks physical table geometry** | \`PdfiumTextSource\` calls \`FPDFText_GetText\` into flat page string; no bounding boxes or table cell anchors. | Add physical-cell text adapter and compare on independent fixtures. |
| F-10 | **MAJOR** | **Report and employee boundaries overly brittle** | Payslip starts only when page contains \`اسم الموظف\` **and 14-digit national ID**; header without ID can be omitted instead of creating incomplete employee. | Capture incomplete record with source page and missing-ID flag; track multiple legitimate payments per person. |
| F-11 | **MAJOR** | **Payment sheet assumes exactly 11 decimal values per half-row** | \`Payment.ParsePair\` and iteration test \`Currency.Matches == 11\`; code includes index-window heuristics, split-page patterns, reordered net positions. | Define layout schema by verified source variant, reconstruct physical columns and compare data with printed totals. |
| F-12 | **MAJOR** | **Missing amount/base ambiguities can become false Verified** | Unknown report class can be rejected, but other acceptance paths only validate available fields. Currency regex cannot parse alternative decimal precision; enum Verified means parser structural result not item-by-item certainty. | Typed quality per field/line, positive evidence for each critical amount and independent document arithmetic. |
| F-13 | **MAJOR** | **Metadata/financial section ambiguity despite previous repair** | PR #3 fixed one class; current classification uses account codes and 'حكومة' position, \`2111\` prefix may change Informational to Earning after summary; nonstandard account codes exist. | Prefer real section/pane/document context, not prefix and a government-keyword guess alone. |
| F-14 | **MAJOR** | **Government share check hard-codes count >= 4** | \`Payslips\` calls incomplete when government contributions count <4; does not ensure expected identities or cadre-dependent relevance. | Report-type financial equation by verified semantics, not universal fixed count; user's general insurance rate review is excluded. |
| F-15 | **MAJOR** | **Employee identity duplicates in payment-only report not flagged** | PR #1 review recorded lack of per-payment-sheet duplicate national-ID checking; code \`Payment()\` does not perform it; cross-report only handles if payslip present. | Flag duplicate *source identity within same logical payment relationship*, but do not prohibit legitimate multi-payment cases blindly. |
| F-16 | **MAJOR** | **Full source PDF not archived** | Upstream archives extracted report snapshots in SQLite, not original PDF bytes/hash/reader version. | Original private evidence archive SHA-256, import/parser version and page/cell offsets; corrections overlay. |
| F-17 | **MAJOR / PRIVACY** | **Selected-employee XLSX may contain others' line text in issue list** | \`PayrollExcelExporter\` filters summary/detail employees, but notes append **all** \`report.Issues\`; prior PR #4 review reported unanchored source line details. | Scope employee-specific issue rows or redact before selected-employee export; authorization scope checks server side. |
| F-18 | **MAJOR** | **Payment blank page exception** | PR #1 review: \`Array.FindLastIndex(lines,0,0,...)\` when \`Arabic.Lines(t)\` empty; current \`Payment()\` still lacks an empty-array guard. | Empty or image-only page must be diagnostic/ReviewRequired, not entire upload crash. |
| F-19 | **MAJOR** | **Period read from first four pages only** | \`Import()\` obtains \`sample=...Take(4)\`, passes same into \`FindPeriod\`; periods beyond first pages and mixed-period pages not independently verified. | Period scoped to each source report/employee where available; mismatch flags, no fabricated month. |
| F-20 | **MAJOR** | **File count and duplicate kind constraints too narrow** | API permits 1–3 files; reconciler assumes one report per type and \`FirstOrDefault\`. Real payroll can have multiple batches/regions/corrected versions. | Import batch with arbitrary authorized files; stable report identity and revision links, explicit source groups. |
| F-21 | **MAJOR** | **Hard-coded absolute tolerance of 0.02** | Multiple arithmetic checks use \`.02m\` across reports without approved document rounding policy. | Exact Decimal/precision and rule-specific rounding/tolerance, independent of legal payroll amounts. |
| F-22 | **MAJOR / UX** | **Employee UI says complete based only on employee warning count** | Angular \`app.component.ts\` shows \`e.warnings.length?'مراجعة':'مكتمل'\`; cross-file discrepancies can be report-level issues. | Propagate report-wide, group and matching blockers to employee review state. |
| F-23 | **MAJOR / DATA** | **Subtotals, 'x' rows, retroactive and normal lines can merge** | Parser skips some \`x\` subtotal markers, but source forms may put \`x\` within line and multiple item names/money tokens; older fixture showed 2015 allowance line contamination. | Tag subtotal/retro/current/event separately; never treat \`x\` note as payable line, never silently merge. |
| F-24 | **MAJOR / LABEL** | **Unconditional Arabic normalization is insufficient** | \`Arabic.Normalize\` NFKC + digits + basic variants; no per-cell visual-to-logical bidi reordering. Earlier outputs show reversed Arabic and '100% confidence' despite wrong labels. | Preserve raw glyphs, selectively score candidate directions from source evidence; 100% only when *all* independently validated fields pass. |
| F-25 | **MINOR / SECURITY** | **Local-only API has no multi-user ACL** | Upstream minimal API permits import, archive read and XLSX export without auth. | Do not deploy upstream API on internal network as-is. Server-side access scope, audit logging and no-PII diagnostics. |

**Evidence classification:** F-01..F-05 were independently reproduced in an earlier review against the same main SHA. F-15,F-17,F-18 were recorded by upstream PR reviewers and still have matching code paths. Other findings are direct code review risks/limitations or earlier observed user layouts, not all tested with confidential PDFs in this turn. **Do not claim every item was reproduced on actual payroll files.**

## Concrete source code links
- [PayrollImportService.cs](https://github.com/AlexandriaDeveloper/OraclePdfParser/blob/23f75d7354cb0a5981d02b35b1a5942ab9c0eeed/src/OraclePdfParser.Application/PayrollImportService.cs) — Money L13–14; financial parse L125–199; payslip identification L66–123; group/payment parse L227–391.
- [PayrollBatchReconciler.cs](https://github.com/AlexandriaDeveloper/OraclePdfParser/blob/23f75d7354cb0a5981d02b35b1a5942ab9c0eeed/src/OraclePdfParser.Application/PayrollBatchReconciler.cs) — cross-source net L18–27; group index L33–43; name substitution and status L52–60.
- [PayrollExcelExporter.cs](https://github.com/AlexandriaDeveloper/OraclePdfParser/blob/23f75d7354cb0a5981d02b35b1a5942ab9c0eeed/src/OraclePdfParser.Infrastructure/PayrollExcelExporter.cs) — ItemKey and grouping L18,35–60; report-wide issue notes L133–135.
- [Payroll.cs](https://github.com/AlexandriaDeveloper/OraclePdfParser/blob/23f75d7354cb0a5981d02b35b1a5942ab9c0eeed/src/OraclePdfParser.Domain/Payroll.cs) — flat payroll entry contract has no occurrence ID.
- [PdfiumTextSource.cs](https://github.com/AlexandriaDeveloper/OraclePdfParser/blob/23f75d7354cb0a5981d02b35b1a5942ab9c0eeed/src/OraclePdfParser.Infrastructure/PdfiumTextSource.cs) — text stream only.
- [Angular UI](https://github.com/AlexandriaDeveloper/OraclePdfParser/blob/23f75d7354cb0a5981d02b35b1a5942ab9c0eeed/web/src/app/app.component.ts) — employee status derived from local warnings.
- [PR #1 comments](https://github.com/AlexandriaDeveloper/OraclePdfParser/pull/1), [PR #3 fix](https://github.com/AlexandriaDeveloper/OraclePdfParser/pull/3), [PR #4 security review](https://github.com/AlexandriaDeveloper/OraclePdfParser/pull/4).
- [PDF-EXTRACTOR\`payroll_oracle_tables.py\`](https://github.com/AlexandriaDeveloper/PDF-EXTRACTOR/blob/main/payroll_oracle_tables.py) — source-row/two-pane cell reconstruction and verification, not a replacement application.

## Examples demonstrating why code alone is not identity
A prior extraction dump (with redacted employee data) showed these **item label/code shapes**:
- Earnings code **21110505** repeated for **2014** and **2015** separate special allowances.
- Earnings code **21110101** used for **normal basic salary** and **basic salary retroactive effect**.
- Deduction code **41110104** used for several unrelated deductions including **صندوق دعم متحدي الإعاقة** and organizational/social items.
- Deduction code **41110201** appeared for distinct **insured-person** and **government-share** insurance descriptions.
- One report has **41110820** for a martyr-related fund name; its applicability, official code mapping, and relation to the Owner's shorthand remain unverified until checked in multiple real sources.
The previous extracted labels sometimes rendered **character-reversed** despite claimed 100% confidence. Its source is not a verified authoritative financial catalog and must not be copied as an approved mapping.

## Proposed fix order (strict P0 first)
**P0 — no accepted financial imports before complete:**
1. Refactor line model to preserve every physical occurrence, exact sign, raw name, raw code, source side, source position, source document hash and confidence.
2. Multiple table/text adapters; reconstruct one code/description/value from the **same** physical row, no silent cross-pane merges. Source-independent synthetic regression suite.
3. Fail-close amount signs, malformed item/ID, unknown report, unreadable source, mixed periods. Keep \`Rejected\` state.
4. Validate gross/deductions/net/printed employer shares on **document-defined** equations; match cross-report employee and bank group context.
5. Identifiers/name conflict reviewed explicitly; do not modify canonical employee or selected display name without source approval.
6. Privacy: no all-employee warnings in a selected-employee export; input authenticity and internal access permissions.

**P1 — reliable operational use:**
7. Multiple payroll types and files per cycle, payment/beneficiary relationships, dates and audit staging.
8. Reviewer-approved per-item aliases for Arabic names; source-to-canonical mapping that tolerates code reuse **without hiding the occurrence**.
9. Legacy representative PDF fixture validation, at least new independent layouts besides October reference. Preserve private originals; no real data in public GitHub.
10. UI side-by-side source PDF + row cell, confidence/reasons, manual correction overlay; recoverable partial or missing data.

**P2 — extended audit engine integration:**
11. Connect accepted source observation to independently approved financial facts; role-based review and central-source-trusted item policy.
12. Support source aggregation across one expenditure chapter only with reviewer decision, separate from true exact line matching.
13. Add post-approval corrections, temporal rules and retroactive movements without editing imported source.

## Test and evidence gate
- A passing build/green CI is necessary but **insufficient**.
- For every issue: use fictional synthetic fixtures that fail BEFORE fix and pass AFTER, and record the exact failing assertion.
- On approved private real PDFs validate independent **source totals** AND **every source line identity/code/label/amount/section** for a significant reviewed sample including edge cases. Unknown item counts must be explicit.
- Keep old, new and alternative-source parser output in diagnostic comparison; conflicts cannot silently pick whichever happens to match net.
- Report measured coverage metrics: pages, employees/payees, items, recognized/ambiguous code occurrences, unreconciled totals, group IDs, exact source-line coverage, review-required rows, false-positive rate from manually reviewed samples.
- Do not silently optimize only for one PDF length/month/layout.
