# DESIGN — Oracle PDF adapter for PayrollReviewSystem

**Status: DESIGN PROPOSAL ONLY / NO IMPLEMENTATION AUTHORIZED**
**Upstream reference:** [code study](../reference/ORACLEPDFPARSER-CODE-STUDY.md), current OraclePdfParser, and PDF-EXTRACTOR (physical PDF layout approach).

## Goal
Parse **genuine government Oracle/GFMIS reports** with independently verifiable identity, financial values and evidence, not just documents tuned to one private sample. Source files are not the expected lawful payroll and no reviewer action changes them.

## Architecture boundaries
Document archive -> content classifier -> parser adapter(s) -> normalized source observations -> structural/financial validators -> cross-report reconciler -> reviewer staging -> accepted immutable observations -> expected payroll/reconciliation engine.

Do not couple the parser to Employees canonical schema, approved rules or future payment engine.

## Candidate contracts (conceptual, not executable API)
- `IOriginalSourceStore`: `StoreImmutable(stream,metadata)` -> DocumentId, SHA-256, storage URL, captured date, size/page metadata.
- `IPdfExtractionAdapter`: `Extract(document,classificationHint?)` -> page text, raw segments, coordinates/cells **if supported**, typed diagnostics.
- `IOracleDocumentClassifier`: document kind(s), confidence, ambiguity reasons, period/entity evidence.
- `IOracleReportParser`: parse extracted page/cell evidence into typed source observations; no implicit employee updates.
- `IReportFinancialValidator`: per-kind totals, individual amounts, formulas only from verified report structure, missing-field flags.
- `ICrossReportReconciler`: match verified identities, period, bank/branch/group IDs, count and amount; no positional group join without validated identity.
- `IImportReviewWorkflow`: preview/import decisions, final source snapshot and separate reviewer overrides.
- `IParserQualityGate`: per-file, per-employee and per-item statuses, plus failure reasons and approval blocker.

Adapters must expose version/strategy, page span, original raw text, normalized label, account, section, money value or null, confidence/evidence and parser issues. The output contract remains provider-agnostic and stable across PDFium/text versus physical-table/cell adapters.

## Data distinctions
Use distinct models and statuses:
- `SourceDocument` and `SourceDocumentVersion`: immutable original source and hash.
- `ParsedDocument`: parser version, checksum, attempt, report period, kind and quality.
- `SourceEmployeeObservation`: report-visible identity, bank and payment code, **not** canonical Employee.
- `SourceFinancialLine`: raw row reference + code + full label + sign + amount + financial side + page/cell.
- `SourceAggregate`: printed totals, not a payable item.
- `ImportValidationIssue`: severity, evidence location and deterministic check ID.
- `ImportReviewerDecision`: explicit person and item mapping, override reason, approval time.
Unknown fields are nullable; do not persist missing numeric amounts as zero.

## Import and review lifecycle
1. Upload accepts only allowed formats and safe size/page limits; malware/file-signature checks, storage permissions.
2. Deduplicate exact source bytes by hash without destroying legitimate reimport history.
3. Identify/report kind and period from **report text** and context, not file name.
4. PDF text extraction; if confidence poor, try structured table/cell extraction in designated safe adapter.
5. Validate per-row code/label/amount context; classify gross/deduction/employer info/summary.
6. Check individually printed totals, employee boundaries and source arithmetic.
7. Cross-check compatible files for exact entity/period/identity and bank group, report missing types without fabricating them.
8. Present preview: source PDF on one side, extracted rows and warnings on the other.
9. Reviewer fixes mappings and explicitly approves staged observations; original remains immutable.
10. Reconciler compares accepted observations to independently calculated expected payroll. No legal review approval based solely on parsing.

## Hard validation cases for first technical spike
| Test ID | Scenario | Pass condition |
| --- | --- | --- |
| PDF-001 | Valid genuine text PDF, two-page payslip | One employee, complete items/pages, report-scoped totals. |
| PDF-002 | Arabic presentation forms, normal Arabic, Persian/Arabic digits | Correct logical labels and Decimal amounts, raw text retained. |
| PDF-003 | Negative amount and sign mirrored in RTL extraction | Exact sign preserved, or flagged/block; NEVER silently positive. |
| PDF-004 | Two side-by-side earning/deduction panes | No cross-pane amount reassignment; physical-cell evidence. |
| PDF-005 | Government/employer contribution between sections | Informational employer side, no employee gross double-count. |
| PDF-006 | Same 8-digit account code, different labels | Distinct rows and mapping decisions. |
| PDF-007 | Unknown account code or moved column order | No guessed code-classification; explicit review. |
| PDF-008 | Payslip net correct but gross/individual deductions incorrect | ReviewRequired even if net matches. |
| PDF-009 | Cover/payment group ordering shuffled | Match group identity, not array index. |
| PDF-010 | Wrong/missing month in one of 3 reports | ReviewRequired or reject; never clean verification. |
| PDF-011 | Missing national ID / mismatched payment code | No automatic person merge or master update. |
| PDF-012 | 1/2/3 uploaded report types | Appropriate partial coverage, no claim absent report was verified. |
| PDF-013 | Missing printed total / unknown layout / scan | Null and ReviewRequired or rejected, no invented amount. |
| PDF-014 | Same file imported twice | Exact hash tracked, no duplicated financial facts. |
| PDF-015 | Official corrected report following earlier wrong source | Both immutable versions remain and link to correction. |
| PDF-016 | RTL Excel export with missing item | Missing remains blank unless explicit absence under approved complete-source scope. |
| PDF-017 | Unexpected employee page count / merged/continued row | Identity and source page correctness preserved. |
| PDF-018 | Distinct bank and actual payment evidence | Source reported payment does not equal bank-confirmed paid. |

## Exit criteria for implementation authorization
- Owner-approved scope of first PDF formats.
- Source status and protected fixture strategy.
- Documented source model + schema/ADR + threat model.
- Prior upstream test families reviewed, existing implementation bugs recorded.
- Verified controls on more than a single source layout, privately.
- Architectural review and **separate** owner-approved coding Issue.

## Not in first parser slice
Calculating salaries/allowances/tax/insurance; direct payroll edits; person legal master overwrite; actual payment verification; legal entitlement decisions; production hosting or wider rewrite.
