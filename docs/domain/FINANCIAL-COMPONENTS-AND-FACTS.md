# Financial items, non-payable reference data and dependencies

## Five distinct concepts
1. **Reference fact / calculation basis**: amount/date used for another calculation, e.g. frozen basic on **30 June 2015**. Not a cash earning by itself.
2. **Entitlement component**: money owed to payee under verified rule/decision; may be fixed, proportional, contingent, recurring, one-off.
3. **Deduction**: reduction from employee cash settlement (tax, insured-person contribution etc) only when law requires.
4. **Employer cost / informational component**: e.g. employer-side social insurance, not cash payable to worker.
5. **Aggregate/display total**: gross, deductions total, net or financial summary; avoid adding it again to line items.

Do not rely on a universal IsPaid boolean. Keep separate **classification**, **calculation role**, **cash-net effect** and **treatment in each legal or accounting base**, effective by context and period.

## Reference case: 30-06-2015
- Record the actual **dated baseline fact** with amount, person/employment, type, verification, provenance and revised/superseded facts.
- A 2026 promotion does **not** automatically rewrite the historic 2015 number. Some grade-linked allowances may change due to grade *without* a change to that reference base.
- New employees without an actual 2015 historical base must not be assigned an invented value or a default guessed from another grade.
- Court rulings/administrative corrections may require a new verified reference fact; retain old historical information, dates and affected-item recalculation.
- A legacy calculation of 2015 base from a percentage or salary ladder may serve as **candidate for investigation** only, not an auto-approved fact.
- The 2015 item must never be added to payable gross merely because it is in employee financial data.

## Calculated components
- For each employee+date derive only from verified input facts, employment status, event history, grade and versioned rule.
- Dependency graph: base component -> eligible grade/incentive/allowance -> lawful gross -> legally defined tax/insurance base -> deductions/net.
- Some amounts use a **fixed historical reference date**, not current wage; the rule specifies.
- Per-rule minimum/maximum, proration and rounding must be explicit, not hard-coded globally.
- Frozen/special historical allowances may have their own fixed identity and historical amount; never auto-recalculate all of them when baseline changes.

## Versioning and history
- Store base assignments with effective intervals; calculated values are immutable per-run lines/snapshots, not an endlessly overwritten value.
- Every change: event/decision reference, authority, valid date, knowledge/record date, before/after, reason, affected components.
- Duplicate imports or duplicate administrative decisions must not generate duplicate entitlement.
- Separate internal ComponentCode from Oracle code; mapping considers description, side, period and source metadata.

## Example (illustrative ONLY, NOT law)
Historical reference 500 -> illustrative grade allowance 40% = 200; functional wage 6500. Cash gross from those payable items = 6700, not 7200. Different rule may keep the grade allowance fixed or disallow it entirely. Do not turn example percentages into production config.
