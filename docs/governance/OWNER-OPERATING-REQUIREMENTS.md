# Owner-confirmed operating requirements: Medical Complex Accounting Unit

**Status:** owner-confirmed operational requirements, captured on 2026-11-06. This is a specification, not implementation authorization or a verified legal ruling.

## Organizational boundary
- Accounting unit institutional code **30200105**.
- Three colleges: **Medicine**, **Dentistry**, **Pharmacy** (Alexandria University Medical Complex accounting unit).
- Recipient groups include public-service employees, teaching/research faculty, contractual workers and nonemployee reward beneficiaries.
- Employee and nonemployee payees must have separate relationship models. No accidental national ID or salary matching as sole canonical authority.

## Review stages and changes after approval
- Routine review occurs **before** payroll-disbursement approval.
- Exceptional administrative events can be received **after** review approval: death, secondment, partial-month entitlement and similar changes.
- Support controlled **post-approval review amendment/request-to-reopen**, with original approved version, event time, legal effective time, reason, proof, reviewer, approver and modified/replacement source.
- The program does NOT send payment cancellation/edits to central payroll or bank automatically; the competent administrative process is external. No "death" event should automatically annul payments without evidence and authoritative decision.
- Distinguish approved internal review, official revised central payroll report, and actual bank settlement.

## Independent approved reference data
Reviewers will provide the already approved and currently available employee financial data. Oracle PDF values may be compared against it but cannot overwrite it. If a missing historical source or a newly effective decision changes a fact, stage it for reviewer approval and retain old revision/history.

## Changes / entitlement calculations
For promotions, allowances, leave and financial events the engine **proposes** affected component calculations, and the reviewer explicitly accepts, corrects with documented explanation or rejects them. Engine calculations do not automatically create a legally approved employee entitlement or affect central payroll.

## Aggregated central payroll lines
Central entitlements may report several distinct payable components aggregated under one Oracle line (e.g. 4 components totaling 1,000 EGP).
- Retain the expected **original 4 component lines** and the actual **single reported line** separately.
- Show item-level allocation/distribution mismatch even if grand total matches.
- Reviewer MAY mark an aggregation exception **accepted with recorded reason** where components are within the **same expenditure chapter/bab (باب الصرف)** and all required supporting controls permit it.
- Same spending chapter is a necessary consideration from Owner's workflow, NOT an unconditional legal authorization. Verify other component-specific accounting constraints and reviewer authority; no automatic approval/auto-distribution.
- Distinguish "amount accepted with distribution exception" from "each line matched".

## Pension / social insurance
- **General scope exclusion:** the accounting-unit reviewers accept insurance/pension amounts produced by central entitlement system without checking/approving/challenging their generic rate calculations; figures can be displayed/reconciled with source totals.
- **One explicit exception:** part-time/partial-work-time arrangements. Owner's illustrated case: former full-time cash entitlement **1,000 EGP** -> cash entitlement at 65% = **650 EGP**, while the **pension-contribution assessment base is still the approved full basis of 1,000 EGP**. The central system sometimes uses 650 instead, and the reviewer must object/seek correction.
- The 65% is a **specific illustrated part-time fraction**, not a universal contribution percentage, not the insurance deduction amount, and not authority to apply the rule to all job classes without verified applicable rule.
- The **approved full-time insured wage base** is a separate reference fact; full cash pay of 1,000 only equals it when explicitly documented and established for that employee/period. Never blindly use gross payroll or full earnings as the insured contribution base.
- Do not build universal insurance recalculation in MVP. Build an **exceptional risk control** comparing documented approved insurance calculation base to actual central-calculation base **if that base is available**. If only the withheld monetary premium is printed and central base/formula are not available, do not infer the base simply from premium without a verified legal rate/rule; flag for documentary verification.
- For a confirmed base mismatch: raise ReviewRequired / central correction request with source evidence; neither change original amount nor automatically recompute payroll deduction.
- Source law/decision, employee scope, dates, contractual/work arrangement and any exceptions still require authenticated primary documentation before activating a deterministic alert. Until then flag as a **proposed rule/check** requiring reviewer confirmation, not a proven legal finding.

## Other centrally calculated deductions / fund items
The Owner also explicitly excludes independent recalculation of the amounts reported by central entitlements for **صندوق دعم المعاقين** and **صندوق الشهداء**. Ingest the actual amounts as verified original-source observations and include them in source total checks without validating their rates/eligibility independently. Their exact Oracle account codes, official document labels, deduction/employer classification and legal bases are **not yet verified**. See [centrally calculated item policy](../domain/CENTRAL-CALCULATED-ITEMS.md). The existing pension partial-work-time assessment-base exception remains.

## Multiple reviewers and security
- Several reviewers work on the same internal-network installation.
- In regular monthly payroll, each reviewer is responsible for an assigned group of employees **per payroll review cycle**; preserve historic assignments, substitutions and reassignment audit.
- Other payroll/disbursement types can be accessed by all authorized reviewers as Owner directs.
- Store actor/time/field before-after/document/version and reviewer decision for every material modification; guard conflicting concurrent updates.
- Deploy on a protected internal network with role-based authorization; no assumption that intranet is inherently secure.
- Repository currently public in verified GitHub state; Owner intends to make it private. Until confirmed, no personal/payroll attachments or secrets.

## Verification / open questions before implementation
- Specific legal treatment of part-time pension bases in each covered cadre, source article/regulation and evidence, and whether percentage is constant per decision.
- Which original document or system field establishes full insured wage base and actual central insured wage base.
- How to define the allowed expenditure chapter code and acceptable aggregation exception; what else must be checked.
- Named authorities to approve post-approval review reopening or emergency payment suspension request.
- Proof and final approval flow for assignment/review coverage.
