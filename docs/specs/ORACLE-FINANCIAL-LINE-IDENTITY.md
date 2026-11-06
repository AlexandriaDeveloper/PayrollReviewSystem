# Oracle line identity and Arabic-label normalization specification

**Status: design proposal; pending approved scoped Issue.**
**Owner requirement:** same account code can legitimately appear multiple times within earnings or deductions, with different labels, accounting meaning, dates or values. Arabic labels are often visually reversed/corrupted by PDF extraction. Must preserve all original occurrences.

## Treat a source line as an OCCURRENCE, not a dictionary key
Do **not** store financial input as \`Dictionary<AccountCode, amount>\` or \`Dictionary<AccountCode + Label, amount>\`.

### Proposed SourceFinancialOccurrence
| Field | Purpose |
| --- | --- |
| SourceLineId | Internal opaque unique ID generated for this specific extracted occurrence |
| SourceDocumentId / OriginalHash | Exact source PDF version |
| ImportAttemptId / ParserVersion / AdapterName | How source was interpreted |
| ReportType / ReportPeriod / EmployerUnit | Context e.g. payslip, cover, payment report; 30200105 if explicitly printed/verified |
| SourceRecipientOccurrenceId | Distinguish one reported payment occurrence from canonical person |
| PageNumber, TableIndex, RowIndex, PaneIndex, CellIds / TextOffsets | Physical and logical source anchors; null when adapter lacks geometry |
| OccurrenceOrder | Deterministic order within this exact source; not a legal component identity |
| FinancialSide | Earnings / Deductions / Informational employer contribution / Aggregate / Unknown |
| RawCode | Code as displayed, nullable; never infer from institutional entity number |
| RawLabel | Exact original glyphs, punctuation, line breaks and visual order |
| NormalizedLabelCandidates | Ordered normalized variants, not destructive replacement |
| RawAmountToken | Exact printed number with minus/parentheses/digit formatting |
| ParsedAmount / SignStatus | Decimal nullable + proved/uncertain sign |
| ClassificationStatus / ExtractionIssues | Verified source parse / ReviewRequired / Rejected with reasons |
| CanonicalComponentId? / MappingDecisionId? | **Later** reviewer-approved mapping, not needed to preserve the source line |
| EntitlementPeriod? / MovementType? | Current / Retroactive / Unknown only when proved by source or signed decision |

### Example of repeated code — fictional values
| SourceLineId | Side | Code | Raw/normalized name | Value | Treatment |
| --- | --- | --- | --- | --- | --- |
| L-01 | Earning | 21110505 | العلاوة الخاصة 2014 | 15.00 | Independent occurrence, historic component candidate |
| L-02 | Earning | 21110505 | العلاوة الخاصة 2015 | 25.00 | Independent occurrence, not merged with L-01 |
| L-03 | Earning | 21110101 | المرتب الأساسي | 500.00 | Current movement candidate |
| L-04 | Earning | 21110101 | المرتب الأساسي أثر رجعي | 35.00 | Retroactive candidate, entitlement period still unproven |
| L-05 | Deduction | 41110104 | صندوق دعم متحدي الإعاقة | 10.00 | Centrally calculated owner policy (pending exact official mapping) |
These sample amounts are completely fictional, not extracted employee facts or legal amounts.

### Classification and mapping steps
1. PDF reader produces original glyphs, reading-order segments and spatial evidence where available.
2. Per **physical cell/row**, generate Arabic logical candidates (NFKC, numeric transliteration, local bidi normalization, diacritics and space punctuation variants). Never reverse the entire page just because Arabic glyphs appear reversed in one cell. Keep original.
3. Extract **separate** code token, numeric token, name fragments and printed section/pane. Validate that association uses the same physical row and appropriate section boundary.
4. Generate mapping *candidates* by hierarchy: verified source side+report context+canonical code alias+recognized name aliases+time/funding/movement context; number alone is weak evidence. Document exact reason.
5. If multiple candidate components remain, keep \`MappingAmbiguous\` and ask reviewer. Do NOT merge distinct allowances or deductions because codes or normalized strings match.
6. A reviewer chooses/creates canonical component and optionally approves an alias with scope and effective period. Alias approvals are audited and reversible/versioned; never retroactively rewrite raw text.
7. Independently validate source statement totals without deciding every legal component, but do NOT declare complete detailed financial verification if item identities/amounts remain uncertain.
8. Keep detailed line ledger separate from optional **reporting aggregation**. Aggregating identical codes for Excel may be an explicitly labelled convenience view, not evidence that they are the same financial component.
9. Even identical code+label+amount may represent two genuine occurrences or duplicated extraction. Check physical/source provenance and declared totals; flag duplicate possibilities rather than silently dropping or adding.
10. If one central payroll row aggregates several approved components, store a reviewed one-to-many allocation with the same expenditure chapter and any other approval conditions, without rewriting either source or expected lines.

## Arabic text examples / hazard classes
- Visual order: \`ةصاخلا ةوالعلا\` vs logical \`العلاوة الخاصة\`.
- Digits: Arabic/Indic/Persian versus Latin; decimal dot/comma/Arabic separators.
- Name broken across lines: \`علاوة الحد الأدنى للحزمة\` + \`الاجتماعية\`.
- Amount attached to text: \`الأساسي٥٠٠\`.
- A numeral inside label: \`العلاوة الخاصة 2015\` versus its actual value; never pick 2015 as money.
- \`x\` subtotal/overlay lines, combined raw texts, ending codes, right-left number reversal, signed values.
- Same raw account code for multiple unrelated deductions; unknown account codes must not become salary codes by prefix.
- 100% parser confidence must NOT be emitted solely from a string dictionary match or a reconciled net amount.
- Distinguish **label ambiguity** from **money ambiguity**, **side ambiguity**, **identity ambiguity**, and **legal-component mapping ambiguity**; they may have different severity/required reviewer actions.

## Public fixture acceptance
| Case | Test |
| --- | --- |
| ID-01 | Same code, two distinct historical allowance labels -> preserve two SourceLineIds. |
| ID-02 | Same code, exact same label twice with distinct physical positions -> two lines + duplicate check, no silent sum/dedupe. |
| ID-03 | Same code occurs as different side under different report context -> explicit side classification and unresolved mapping. |
| ID-04 | Same national ID has separate legitimate payee/payment relationships -> no false single payroll merge. |
| AR-01 | Visual mirrored Arabic and logical Arabic map as candidates to same alias only with proof, original unchanged. |
| AR-02 | Arabic label wraps 2–3 cells/pages -> description fragments reconstructed or flagged. |
| AR-03 | Arabic item number year (2014/2015) adjacent to amount -> exact value selected by spatial field, not LastAmount. |
| AR-04 | Arabic number precedes label and code follows at end -> correct token associations. |
| AR-05 | 'x' subtotal and retroactive item appear after regular item -> no contamination or silent double count. |
| AR-06 | Decimal -100.00 / 100.00- / (100.00) / Unicode minus -> signed interpretation or review-required. |
| AR-07 | Two panes same physical row, earning and deduction both have identical 8-digit codes -> two distinct sides/positions. |
| AR-08 | Missing table geometry or unlabeled pane -> explicit unresolved status, do not guess. |
| AR-09 | Auto mapping ambiguous due to multiple approved aliases -> reviewer-only mapping and audit record. |
| AR-10 | Snapshot export for selected employees does not leak unselected employee raw warnings. |

## Owner policy compatibility
- **Insurance/pensions**: generic central-calculated source amount, without general recalculation; narrowly documented part-time full insured-base audit exception.
- **صندوق دعم المعاقين** and **صندوق الشهداء**: central-source-trusted amount once **raw official label/context/code** is verified, with source integrity and total checks. Do not invent numeric percentages.
- Temporal allowances/promotions: mapping enables separately computed expected component values by period; Oracle raw source remains untouched.

## No automatic data mutation
No PDF parsing step can update Employees, approved legal rule, historical grade, financial facts, canonical component catalog, reviewer decision or previously confirmed import observation without a separately authorized reviewed transaction.
