# OraclePdfParser — practical technical baseline and gap analysis

**Status:** code study / engineering reference, not implementation authorization. Reviewed 2026-11-06 based on the GitHub repository's main branch at that time. Pin the exact upstream SHA before any code reuse or regression claim.

**Upstream:** https://github.com/AlexandriaDeveloper/OraclePdfParser
**Project target:** AlexandriaDeveloper/PayrollReviewSystem — separate independent audit system; no Git submodule/dependency/code copied in this documentation PR.

## Why this repository is important
OraclePdfParser is an existing working *starting point* for Arabic payroll PDF ingestion, not just an architectural idea. It already contains a .NET 10 parser, input abstraction, PDFium text extraction, Arabic text/number normalization, three Oracle report types, cross-document checks, local import archive, review statuses, Excel export and focused unit/integration tests.

A separate existing PDF-EXTRACTOR project uses **physical-table cell reconstruction** (pdfplumber) to handle mirrored two-pane Arabic layouts where flattened Unicode text loses column meaning. Evaluate both strategies without assuming either covers every source.

## Existing code map — read these first
| Upstream path | Role |
| --- | --- |
| README.md | Current requirements, run commands, supported types and limits. |
| src/OraclePdfParser.Application/Contracts.cs | `IPdfTextSource.ReadPages(Stream)` and `IPayrollImportService.Import(fileName,pages)`. |
| src/OraclePdfParser.Infrastructure/PdfiumTextSource.cs | Page-by-page PDFium Unicode text stream; no X/Y geometry; native dependency. |
| src/OraclePdfParser.Infrastructure/PdfPigTextSource.cs | Alternate page text source based on PdfPig content order. |
| src/OraclePdfParser.Application/Arabic.cs | Arabic normalization, digits/separators, decimal conversion. |
| src/OraclePdfParser.Application/PayrollImportService.cs | Kind recognition and individual payslip / cover / payment-sheet parsing. |
| src/OraclePdfParser.Domain/Payroll.cs | `ReportKind`, `PayrollEmployee`, `PayrollEntry`, `CoverGroup`, `VerificationStatus` etc. |
| src/OraclePdfParser.Application/PayrollBatchReconciler.cs | Cross-source employee/period/count/net/group comparisons. |
| src/OraclePdfParser.Api/Program.cs | `POST /api/import`, archive/review/export endpoints; multi-file upload. |
| src/OraclePdfParser.Infrastructure/SqliteImportArchive.cs | Local import snapshot archive; **raw PDF bytes are not saved there**. |
| src/OraclePdfParser.Infrastructure/PayrollExcelExporter.cs | Native XLSX, RTL sheets, numeric cells, source page and text identifiers. |
| tests/OraclePdfParser.Tests | Import, Arabic, edge-case, cross-file, metadata, Excel, PDFium and archive tests. |
| tools/OraclePdfParser.Verify/Program.cs | Local strict and sample-specific private verification CLI. |

## Upstream report kinds and parsing approach
- `EmployeePayslip` — individual employee report: identity, payment number, bank, earnings/deductions, employer informational shares, printed gross/deductions/net, line-item/page evidence. Employees may occupy more than one page.
- `Cover` (كافر) — bank-group totals, staff counts, continuation pages.
- `PaymentSheet` (كشوف صرف) — employee rows, split/RTL values and bank-group controls.
- `Unknown` — reject / flag; never invent a report kind from filename.
- Existing statuses: `Verified`, `ReviewRequired`, `Rejected`. These are **extraction/reconciliation** statuses, not a legal entitlement decision.

Existing approach: PDF file -> PDFium Unicode pages -> normalize matching text -> detect content-based document type -> parse employees/groups/items -> report-specific numerical checks -> cross-file reconciler -> archive parsed snapshots. The original PDF is processed in memory and **not** archived by upstream; the NEW audit system must archive the original securely with hash and page references.

## How to run existing OraclePdfParser independently on a Windows dev machine
Prerequisites: .NET 10 SDK, Node.js 22 and npm, native PDFium runtime supported by upstream. Clone or open existing repository. Run from repository root:

```powershell
dotnet restore .\src\OraclePdfParser.Api\OraclePdfParser.Api.csproj
dotnet run --project .\src\OraclePdfParser.Api\OraclePdfParser.Api.csproj --urls http://localhost:5080
```

In another terminal:

```powershell
cd web
npm install
npm start
```

Open http://localhost:4200. Upload 1–3 files from the locally protected machine; inspect kinds, employee count, IDs, page evidence, warning list, bank groups and exported XLSX. **Do not expose this local unauthenticated app on the public internet.**

Strict CLI verification of private test PDFs without putting originals in GitHub:
```powershell
dotnet run --project tools\OraclePdfParser.Verify -- --strict "C:\PrivatePayroll\payslip.pdf" "C:\PrivatePayroll\cover.pdf" "C:\PrivatePayroll\payment.pdf"
```
(The example paths are placeholders; keep real source files in approved secure local storage, not the repository.)

The upstream README also describes a `--golden-october-2026` mode tied to ONE private reference set and hard-coded controls. That is a regression fixture only, **not** proof that other reports/layouts/periods parse correctly; do not copy its confidential control amounts into this new public repository.

## Known implementation limitations / verification gaps
**Code-observed risk:** `PayrollImportService` has `Money` / `Currency` regular expressions that do not include a leading minus sign even though `Arabic.Amount` can parse negative values. Any sign-sensitive extraction must receive a fail-first regression test before reuse.

**Code-observed risk:** `PayrollBatchReconciler` compares cover/payment groups by their **position** in a list in one path rather than guaranteed bank+branch+batch identity. Equal net with wrong group/ordering is not an acceptable financial validation.

**Architecture risk:** PDFium Unicode stream has no physical X/Y cell layout. Two-column RTL reports, continuation pages, separated codes/amounts and mirrored Arabic text can require an alternate cell-aware adapter; PDF-EXTRACTOR is evidence that this is material.

**Source-evidence gap:** upstream SQLite archives only parsed reports/issues, not original PDF bytes; the independent auditor must preserve source bytes with SHA-256 and version, type/period/source page/cell mapping to reproduce findings.

**Trust gap:** matching net/aggregate figures does NOT prove that gross, deduction line items, informational employer contributions, identities, groups, taxes or legal entitlements are correct. Preserve quality and reconciled-dimensions separately.

**Operational gap:** upstream API is local-first without production RBAC/SQL Server enterprise audit; do not reuse unauthenticated API design for payroll data.

**Report limitation:** PDFium+PdfPig approach is text-layer based. Image-only or unknown-layout PDFs need an explicit unsupported/review workflow; OCR cannot be assumed correct.

**License/dependency gap:** before copying upstream code, review current code/dependency licenses, native PDFium packaging, security update strategy and portability.

## Minimum local verification matrix
Use independently controlled private originals and **synthetic fixtures committed to CI**, across varied months, groups and layouts:
1. All three report kinds and mixed input ordering; unknown document rejected.
2. Employee/payer unique stable identification; missing/duplicate/mismatched national/payment codes.
3. All monetary signs, comma/Arabic separators, thousands separators, Decimal precision.
4. No net-only approval: printed gross, deductions, net and report-specific employer-share equation checked.
5. Distinct physical-pane earning/deduction items and government contribution as informational, never cash.
6. Continuation across page, unexpected pages per employee, unknown account codes and same code/multiple labels.
7. Cross-file mismatching bank group / branch, group count, period and employee count.
8. Missing printed values leave null/ReviewRequired, not invented zeros.
9. Source page/row/cell evidence for each item, report hash and reimport idempotency.
10. Corrected report imports as distinct version, never updates earlier snapshots.

## Recommended adoption
1. Do **not** create a completely new regex-only parser without studying upstream.
2. Derive an adapter contract from existing `IPdfTextSource`, `IPayrollImportService`, `PayrollBatchReconciler` but **extend** it for physical layout information, original bytes/hashes and granularity of evidence.
3. Evaluate PDFium text adapter + structured table adapter (based on PDF-EXTRACTOR techniques) over an independent fixture suite.
4. Prefer a single validated output domain contract and explicitly preserve original/normalized text and confidence.
5. Authorize a narrow parser technical spike and test plan before any production implementation; no automatic dependency or code port is authorized by this documentation.

## Important distinctions
- "Parsed successfully": source was readable and data extraction passed a parser's structural tests.
- "Financially reconciled": expected cross-source arithmetic and groups match in the verified scope.
- "Legally entitled": requires independent approved employee facts and legal rule; not proven by PDF.
- "Paid": requires accepted payment/settlement evidence, not a payslip alone.
