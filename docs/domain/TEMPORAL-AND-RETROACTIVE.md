# Promotions, effective dates and retroactive entitlement/payments

## Fundamental separation
1. **Administrative event/decision**: what happened, who issued it, decision date.
2. **Entitlement**: which salary items were due, from what lawful financial-effect date.
3. **Central payroll application**: when Oracle/GFMIS began reflecting the new rate.
4. **Reported arrears**: whether prior period differences appeared on later payroll sheets.
5. **Actual settlement**: what payment/bank or equivalent independent evidence proves, often later and partial.

**Never assume a promotion's retroactive difference will be paid in the same month or at all until evidenced.** Missing in October statement means not shown in THAT reviewed statement, not proof about every other channel.

## Required dates per event
DecisionIssuedAt; FunctionalEffectiveDate; FinancialEffectiveDate; RecordedAt/KnowledgeAt; SalaryImplementationPeriod; SourceReportPeriod; OriginalEntitlementPeriod; SettlementOrDisbursementPeriod; PaymentConfirmedAt (if independently evidenced). Tax/fiscal periods may be different.

## Monthly recomputation
For each employee and affected month:
1. Reconstruct grade, legal regime, employment relationships and events effective in that period.
2. Resolve legal rule versions and reference bases for **that** month/grade/classification.
3. Compute individual components independently; track dependencies and inputs.
4. Compare originally expected/current recalculation to source paid/reported amounts for the original entitlement month, excluding any double-counted retro payments.
5. Emit **RetroEntitlement** keyed to employee+component+entitlement period+trigger+calculation version, with before/after amounts.
6. Reconcile later central disbursement records and independently verified payment evidence; allocate only where linkage to component/period is established.
7. Track open, reported-in-payroll, under-review, partially paid, fully settled, overpaid/recovery-review. No automatic payroll deduction.

## Example (synthetic, not law)
Promotion from second to first grade effective July; change implemented October; difference per month: function wage +300, grade allowance +120, percentage-linked incentive +30, fixed allowance +0, so gross difference +450. July–September = 3 x 450 = 1350 before tax/insurance effects. Whether October pays none, part, or all arrears is unknown until source and payment evidence are reviewed.

## Partial unallocated payment
If a later statement shows a single retroactive amount without reliable component/month attribution, record UnallocatedReportedAmount; do not split it heuristically across three items. Reviewer must link it to a substantiating sheet/decision or leave unresolved.

## Recalculation after newly found historic fact
A ruling changes a prior reference fact or a promotion grade: create a new calculation version. Preserve prior expected calculation and prior reviewer decision in history. Compute delta and assess impact on earlier periods with actual other changes (leave, additional allowances, disciplinary impact). Do not overwrite an approved historical run.

## Critical scenarios
- Backdated promotion order arrives after several pay periods.
- Date of decision differs from salary/financial commencement.
- Two promotions and an annual allowance affect consecutive subperiods.
- Grade changes an allowance while 30-Jun-2015 base stays fixed.
- One component exempt from promotion effect; another recalculated from base.
- Negative differences and overpayments; some items already adjusted.
- Retro amount paid across multiple later periods, or no reliable proof of bank settlement.
- Multiple payroll files/duplicate import, rerun and changed rule version.
- Fiscal-year boundaries and annual tax/social-insurance treatment may require specialist rule confirmation.

## Payment evidence hierarchy
Source amount on Oracle statement = "reported in source"; bank/official settlement evidence = "paid/settled" as far as scope allows. Never equate statuses.
