# Legal knowledge governance — no unsupported payroll rules

## Scope
Relevant research includes university employee statutes, service/civil-service laws, salary packages, specific allowances, tax payroll withholding, insurance law, public finance, payment/document controls, disciplinary and administrative cases. This document **does not approve numerical values or declare current applicability**.

## Legal rule activation checklist
Before a calculation can be marked APPROVED_FOR_CALCULATION, store:
- Primary original source and secure document ID/hash; issuing authority, law/decision number/year.
- Verified **article, subsection and source page** and exact passage as permitted by licensing.
- Effective date of original rule, amendments/repeal/suspension; overlap precedence if necessary.
- Covered relationship/cadre (civil service, teaching faculty, emeritus, other distinct relationship).
- Tax/insurance/financial impact, contribution bases where specified.
- Calculation method, input reference-date basis, grade/rank conditions, minimum/maximum, proration/rounding.
- Explicit exceptions, transitional provisions, judicial rulings and local authorized decisions where applicable.
- Legal reviewer name/date, source conflict status, tests and approval.
- Semantic version and immutable evidence of rules used in historical calculations.

## Source status distinctions
UNLOCATED -> ACQUIRED -> TEXT_READ -> PAGE_VERIFIED -> APPLICABILITY_ASSESSED -> DRAFT_RULE -> LEGALLY_REVIEWED -> OWNER_APPROVED -> IMPLEMENTED_AND_TESTED -> ACTIVE. A searchable Markdown summary is **not** a PAGE_VERIFIED authoritative source. Validity can later become REVOKED / SUPERSEDED.

## Known law families to investigate (not blanket applicability statements)
- Law 81/2016 Civil Service and executive rules, amendments, special exceptions.
- University Law 49/1972 and implementing law/regulation materials, historical and updated provisions.
- Relevant 2015 transitional salary history, frozen base and incorporated/independent allowances.
- Law 148/2019 Social Insurance and Pensions and implementing regulation 2437/2021.
- Current income tax/payroll withholding instructions, cumulative settlements and annual rules.
- Public finance framework including 2022/2024 amendments and Ministry of Finance instructions.
- Specialized employee legal regimes, healthcare or other categories, outside consultant/wage recipient cases, and applicable Alexandria University decisions.

No numeric rate, allowance amount or asserted law status is authorized merely by appearing in these names.

## Conflicts and safeguards
If two secondary research notes disagree (e.g., tax/insurance percentage, allowance calculation or legislative amendment), raise a SOURCE_CONFLICT Issue; quote primary original sections and resolve only with a verified source and authorized reviewer. Never pick the newest-looking note automatically.

## Rule catalogue design
LegalSource -> LegalProvisionVersion -> ApplicabilityCondition -> RuleVersion -> ComponentDependency -> RuleTestCase. The engine does not execute arbitrary expressions from PDFs or AI text; use bounded interpretable operations and checked input types.

## Ongoing updates
When a new legal amendment or ruling arrives: register source; assess applicability/retroactive start; create new rule version; identify affected past calculations; recompute new proposed version without rewriting history; route findings to reviewer. Rule author and approver should be distinct.

## Public repository safety
Store metadata, synthetic templates and links only. Do not commit confidential copies of the official library, personal salary reports, private PDF scans or unlicensed full book texts.
