# Part-time cash entitlement versus insured wage base — narrow audit exception

**Business scenario confirmed by Owner, 2026-11-06.**
**Legal applicability to each cadre is NOT yet primary-source verified; do not activate automatic legal determinations.**

## Problem
A part-time/partial-work-time work arrangement may change an employee's **cash entitlement**, while the **pension/insurance contribution assessment base** remains based on the documented full-time insured wage for that person and period. Central payroll can incorrectly use the reduced cash entitlement as its pension calculation base. The accounting-unit reviewer normally does not examine central social insurance calculations, **except this case**.

## Worked example (illustrative, not legal percentage)
- Pre-arrangement cash entitlements: **1,000 EGP**.
- A specific approved arrangement permits 65% cash entitlement -> **650 EGP**.
- Approved contribution assessment base **for the illustrated employee**: **1,000 EGP**, not 650 EGP.
- If central pension calculation used **650 EGP** as its assessment base, raise objection; do not accept that simply because cash entitlement is 650 EGP.
- Important: contribution deduction **amount** is a different number from the contribution **base**. The example does NOT mean deduct 1,000 EGP as pension. It also does NOT prove a universal legal requirement for all employees; verify the applicable rules.

## Distinct facts to model
| Field (conceptual) | Meaning / source |
| --- | --- |
| WorkArrangementType | Full-time / legally defined part-time arrangement, approved by administrative decision |
| WorkArrangementEffectiveFrom/To | Actual financially applicable time range |
| CashEntitlementFraction | 0.65 only for illustrated decision, never general default |
| FullTimeReferenceCashEntitlement | Historical reference used to calculate adjusted cash entitlement |
| ReducedCashEntitlement | Rule-generated proposed or reviewer-approved amount due as cash |
| ApprovedInsuredWageBase | Distinct authoritative wage-base fact for insured employee and contribution period |
| CentralReportedInsuredWageBase | As reported by official central-entitlement evidence, if available |
| CentralReportedInsuranceDeduction | Amount withheld per Oracle PDF, source-observed |
| RuleAndEvidenceVersion | Primary legal rule, administrative decision, effective period and approval evidence |
| AuditFlag | Mismatch / BaseUnknown / RuleNotVerified / NoException / ReviewedException |

Do NOT conflate full-time cash entitlement with approved insured wage base just because both equal 1,000 in a synthetic example.

## Review-only algorithm (requires verified legal basis before active calculation)
1. Determine whether the approved employee work arrangement is part time in the current entitlement period.
2. Find documentary evidence for the **full insured contribution wage base**, with correct time, legal regime and employee scope.
3. If the central insured base is explicitly provided, compare it to approved base using an approved exact/rounding rule.
4. If central base missing, mark insufficient documentary evidence. Do not guess that a premium amount implies a particular insured base without the approved exact calculation rule.
5. If actual base is shown as 650 but approved base documented as 1,000 in the same period, raise a **pension-base mismatch** finding, retaining actual source and evidence.
6. Reviewer accepts/requests documents/objects and requests correction from central entitlements. Original Oracle entry never modified. Final approval behavior follows authorized organizational workflow.

## Minimum regression cases
- **PT-001**: documented full base 1000, reduced cash due 650, central base 1000 -> no pension-base objection.
- **PT-002**: documented full base 1000, reduced cash due 650, central base 650 -> objection / correction requested, despite source net arithmetically matching.
- **PT-003**: only central deduction 75 provided, central insured base not printed -> no guessed base; documentary verification needed.
- **PT-004**: normal full-time employee -> no part-time exceptional check; no general insurance recalculation.
- **PT-005**: unverified decision, missing date, or competing legal applicability -> reviewed warning / no automatic legal judgment.
- **PT-006**: reduced entitlement changes partway through month -> use effective interval and verified partial-month arrangement, no arbitrary all-month multiplier.
- **PT-007**: 65% applies to one group but not another -> never global constant; per employee/decision validity.
- **PT-008**: approved insured wage base != full pre-arrangement cash gross -> use independently verified insured base, not the prior gross.
- **PT-009**: reimport of corrected official report -> original plus correction coexist, finding can be reconciled without overwriting source.

## Governance
This is a narrowly scoped **business-review exception**, not a request to implement a tax/insurance calculation module. Any future rule activation requires original law/circular and correct cadre scope, approved dates and tests. Reviewer-initiated objection cannot directly modify central-payroll deductions.
