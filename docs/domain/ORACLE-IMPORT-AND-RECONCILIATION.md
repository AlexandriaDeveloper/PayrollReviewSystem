# Central payroll ingestion, parsing, matching and source quality

## Report classes (confirm by actual samples)
Oracle/GFMIS inputs may include **individual detailed employee payslips**, **cover/bank group summaries** (sometimes described as كافر), and **payment sheet/kشوف صرف**. Official 47/55/132 form references and their exact content must be confirmed from **the actual official source** before coding hard-coded classification.

## Required import steps
1. Secure upload and immutable original archive; hash, timestamp, filename and MIME verification; detect duplicate hash.
2. Classify file by **content**, not filename alone; verify period, employer, page count and recognized report headings.
3. Extract Unicode/Arabic normalized text and/or structured table cells; preserve raw original text order and exact page/row/cell evidence. No universal reversed Arabic rule.
4. Segment employee records independent of fixed pages-per-employee or employee-count assumptions.
5. Parse identity separately from financial line items; full disbursement number, national ID and organizational prefix never masquerade as an account code.
6. Distinguish earning, deduction, informational employer contribution, printed aggregate, metadata and unknown line. Account code prefix alone is insufficient.
7. Validate line amounts, printed total(s), appropriate report-specific net equation, employee counts and bank-group totals.
8. Match multiple files by verified stable identifiers, period, organizational context and amount — no fuzzy name match as sole authoritative identity.
9. Produce granular parser issues, ambiguous fields, confidence/evidence; unresolved becomes ReviewRequired, not fabricated 0 or auto Verified.
10. Reviewer accepts/overrides mapping and identity through an audited staging workflow; only eligible records are promoted to canonical source observations.
11. Reimport corrected files as new versions; never mutate an immutable historical import.

## Current known failure-risk regression families (from earlier source notes; retest in new code)
- Negative/mirrored amount sign lost.
- Net matches but gross/deductions item subtotals do not.
- Net matches, but bank group coverage/total differs.
- Identity field from header incorrectly classified as an informational financial entry.
- Rejected/unknown report silently degraded to nonblocking ReviewRequired or "verified".
- Separate Arabic RTL side-by-side columns interleaved.
- Different number of pages per employee; continued lines/employer contributions on next page.
- Two rows share same numeric account code but have distinct descriptions and treatment.
- Missing/repeated national ID, changed payment code, unknown account.
- Partial source with missing total: missing is NOT zero.

## Legacy technique references (research only)
- Payroll-Auditor uses a Syncfusion adapter in an Employees import flow.
- OraclePdfParser uses PDFium Unicode stream, cross-file group matching and exports.
- PDF-EXTRACTOR uses pdfplumber physical-table reconstruction for visual-order two-pane pages.
Comparison of these is a **design discovery**, not evidence that any is completely accurate on every PDF. Prefer adapter interface + independent fixture matrix and reconciliation checks. Do not copy code without verifying library license/security/compatibility.

## Import state
Uploaded -> Classified -> Parsing -> ParsedWithIssues / ParseFailed -> IdentityReview -> ComponentMappingReview -> ConfirmedSourceSnapshot. Only verified source **observations** are candidates for downstream reconciliation. No import may directly alter approved employment grade or financial entitlement.

## Reviewer tools
Side-by-side PDF original page and extracted fields; edit only a separate proposed correction; per-line source page anchor; show rejected/ambiguous and unmatched group; export Excel with RTL and text identifiers (avoid formula injection and leading-zero loss).

## Cross-file precision
All money decimal fixed precision; no general .02 tolerance unless specifically approved for a given report/rounding context. Treat informational shares per **documented** source equation rather than one universal gross - deductions = net rule.
