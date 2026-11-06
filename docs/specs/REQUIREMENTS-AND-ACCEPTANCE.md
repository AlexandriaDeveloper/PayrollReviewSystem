# Requirements baseline and acceptance scenarios

**Status:** requirements/design proposed from Owner discussion. Any specific implementation task needs separate approved Issue. **No legal numerical constants authorized here.**

## User roles
Intake operator; auditor; senior reviewer; legal/rule approver; payroll/central-entitlements respondent; system administrator; read-only oversight. Permission matrix needs Owner approval before production.

## Functional requirement groups
| ID | Requirement |
| --- | --- |
| FR-001 | Import multiple genuine Oracle report forms as immutable, versioned evidence. |
| FR-002 | Parse Arabic RTL and preserve source page/line as reviewer evidence. |
| FR-003 | Resolve identity safely; person != employee != outside recipient. |
| FR-004 | Model historical grades, legal status, affiliations and orders by effect period. |
| FR-005 | Catalogue payable, deductible, employer, informational, historical-base and aggregate financial data distinctly. |
| FR-006 | Store multiple approved legal rule versions with sources, date conditions and traceable inputs. |
| FR-007 | Recompute expected earnings by employee/component/period, with dependency ordering. |
| FR-008 | Calculate promotion/periodic allowance effect, reference-date conditions, and partial-period cases if verified. |
| FR-009 | Compute retroactive entitlements by component/original month independently of anticipated payment month. |
| FR-010 | Compare expected/Oracle/reviewed/official-correction and payment evidence without overwriting originals. |
| FR-011 | Record findings, request documents, verify decisions, defer, approve, reopen and follow up, using proper review gates. |
| FR-012 | Allocate later demonstrated retroactive payments to original claims; track unsettled or overpaid balances. |
| FR-013 | Review tax/insurance bases and remittances when source/approved rules permit. |
| FR-014 | Provide Arabic RTL employee workpapers, dashboards and audit report exports. |
| FR-015 | Log all material actions, rule changes, imports and official corrections with tamper-evident references. |
| FR-016 | RBAC, confidentiality and non-destructive corrections; no central payroll edit/payment. |
| FR-017 | Handle incomplete evidence as open review issues without invented amounts. |

## Acceptance scenario matrix (non-exhaustive)
| ID | Input / event | Expected behavior |
| --- | --- | --- |
| AC-001 | One person has two concurrent financial relationships | No false duplicate employee/payment entitlement; each relation tracked separately. |
| AC-002 | Foreign payee has passport and no national ID | Supports identity review without fabricated 14-digit ID. |
| AC-003 | Oracle grade differs from approved promotion grade | Source conflict raised; canonical grade not overwritten. |
| AC-004 | Salary month historical and current grade changed | Calculates historic entitlement from grade effective in old month. |
| AC-005 | Frozen basic 30-06-2015 = 500, used to compute allowance | Basic recorded as calculation-only; never added to net as separate earning. |
| AC-006 | 2015 basic unknown | No default derived fictitious amount or "zero"; needs evidence. |
| AC-007 | Ruling revises historic 2015 figure | Recompute affected items in new run, preserve old decision/source and old base. |
| AC-008 | Periodic allowance increases job wage but not all other components | Only dependencies specified by approved rule change. |
| AC-009 | Special allowance independent of job wage | Do not automatically incorporate or recalculate dependent incentives. |
| AC-010 | Promotion affects different grade-based allowances differently | Component-by-component results and legal source references. |
| AC-011 | Decision issued September, financial effect July, applied in October | July–September retro entitlement calculated; October payment not assumed. |
| AC-012 | Some arrears reported in December, remainder later | Allocated partial settlement, no duplicate claim. |
| AC-013 | Later lump-sum arrears have no component/period breakdown | Remain Unallocated; no heuristic distribution. |
| AC-014 | Two promotions and allowance across yearly boundary | Segment by intervals and correct grade/rule per period. |
| AC-015 | Underpayment/overpayment with a negative adjustment | Preserves sign; no implicit employee deduction. |
| AC-016 | Employee shows in cover but not individual payment sheet | ReviewRequired missing evidence, not Verified. |
| AC-017 | Net matches but earnings/deductions disagree | Financial mismatch flagged. |
| AC-018 | Bank group mismatches despite net match | Group discrepancy flagged. |
| AC-019 | Mirrored Arabic amount loses negative sign | Import blocked or corrected with evidence and test. |
| AC-020 | One Oracle code appears with two different labels | Distinct mapping decisions, no code-only classification. |
| AC-021 | Financial source has missing value | Unknown/null, not assumed zero. |
| AC-022 | Same PDF imported twice | Idempotent detection; do not duplicate approved events or claims. |
| AC-023 | Reviewer enters corrected amount | Overlay with reason; original Oracle amount immutable. |
| AC-024 | Official central correction received | Separate version verified and linked, source original retained. |
| AC-025 | Review completed but official correction pending | Review completion independent from finding resolution/payment settlement. |
| AC-026 | Money reported in payroll sheet but bank evidence absent | State "reported", not "paid". |
| AC-027 | Payment confirmed for part of an arrears claim | Outstanding amount reflects only supported allocations. |
| AC-028 | Rule source version superseded after historical approval | Old snapshot untouched; new review run generated if needed. |
| AC-029 | Unknown legal applicability | Rule cannot auto-approve monetary amount. |
| AC-030 | Unauthorized user tries rule/approval mutation | Access denied and audit event recorded. |
| AC-031 | Personal ID or bank detail included in exported diagnostic | Redacted or export denied; no data published to GitHub. |
| AC-032 | Actual report has unexpected RTL physical table layout | Parser adapter produces evidence/review issues, not guessed verified values. |

## Nonfunctional
- Currency arithmetic precision, period determinism, repeatable runs, latency targets measured from sanitized realistic-sized fixtures.
- Persistence and audit integrity; role-limited access to PII; recoverable encrypted backups; no uncontrolled file uploads.
- Arabic RTL display including mixed English codes/numbers; leading zeros retained in external identifiers; Excel formula injection prevention.
- Test coverage on independently established total/identity/bank-group control figures. A green CI does not validate private report accuracy.
