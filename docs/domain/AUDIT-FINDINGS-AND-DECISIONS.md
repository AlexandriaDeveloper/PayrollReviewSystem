# Internal financial review: findings, decisions and correction workflow

## Audit object
An AuditCase holds target employee/payee, payroll run, entitlement period, financial component, reported source figure, independently calculated expected figure, evidence and reviewer reasoning. Cases may be prompted by positive/negative mismatch, eligibility uncertainty, missing supporting decision, incomplete extraction, incorrect grade, duplicate entry, unexplained bank/group difference or atypical trend.

## Distinct monetary columns
- **SourceAmount:** as printed in the original Oracle/central-entitlements report; immutable.
- **ExpectedAmount:** calculated independently with verified rules and employee history; versioned.
- **ReviewedAmount:** reviewer-approved interpretation after evaluation; separate overlay.
- **OfficialCorrectionAmount:** later central-entitlements corrected-source amount and original supporting document.
- **ReportedDisbursementAmount:** amount present in downstream payroll/payment sheet; not necessarily settled.
- **VerifiedPaidAmount:** proven by valid payment/settlement evidence; do not infer from expected salary or an unverified report.
- **OutstandingAmount:** reconciled balance only after allocations and relevant taxes/insurance; otherwise mark tentative.

## Reviewer action taxonomy
| Action | What it means | Closes case? |
| --- | --- | --- |
| Match / accept | Rule, source, identity, component and evidence align | Yes, subject to whole-employee gate. |
| Request documents / verify source | Missing decision or uncertain data | No. |
| Fix internal data or rule | Defect in review system, not automatically Oracle | No until recomputed and reviewed. |
| Record approved reviewed value | Verified determination with reasoning and evidence; source stays unchanged | Examination may finish; correction still open. |
| Flag central entitlement error | Reasoned finding on source amount | No until proper resolution. |
| Request official correction | Send finding to the authorized central entitlements unit | No until returned source checked. |
| Defer / hold | Work cannot be completed; preserve blocker | No. |
| Follow up for settlement/recovery | Wrong amount already reported/possibly paid, action by authorized unit | Financial follow-up stays open until evidenced. |
| Withdraw finding / not applicable | Evidence disproves finding; reason and approver retained | Yes, with full audit trail. |

Do not conflate an **action**, a **finding disposition**, a **reviewed employee approval** and a **payment settlement status**.

## Before vs after payment
- Before disbursement: report central error, request official recomputation/correction, reimport official response as distinct source version. Program does not modify Oracle or release payroll.
- After disbursement: record confirmed excess/shortfall if evidence supports; authorized body decides lawful recovery/supplementary payroll. No direct auto employee deduction.
- In both situations, a reviewer can finish their analysis while finding remains open for implementation or payment evidence.

## State model
Case: New -> UnderReview -> WaitingForEvidence / CorrectionRequested / Deferred -> DecisionRecorded -> FollowUpPending -> Resolved/Closed (or Reopened with version).
Whole-employee review: Incomplete, ReviewRequired, ReviewerCompleted, EligibleForApproval, Approved, ExceptionApproved (only with owner-approved privilege/policy), Rejected. Do not automatically approve on a matched net total or empty detail list.

## Evidence and audit trail
For every action capture actor, timestamp, scope, source references, versions of expected calculation and rule, before/after, reason and reviewer/approver. Never hard-delete a reviewed case or a correction history to conceal earlier conclusions.

## Separation of duties
Staff who proposes or edits a legal rule should not approve its live effect on their own. Intake may not approve final review. Implementation/admin permissions may not override financial authority.

## Example — illustrative
Central report shows extra incentive 1200, expected 1000, gross difference +200. Reviewer can request official correction, record independent reviewed value 1000 or request source evidence; none of these changes original SourceAmount=1200. If 200 was actually overpaid, follow a lawful documented recovery process instead of silently deducting next month.
