# Arabic RTL audit workbench — proposed screens

## UX principles
Native Arabic RTL, readable mixed Arabic/English accounting identifiers, precise amounts, right-aligned Arabic and clear monetary column alignment. Never use color alone to convey risk, and do not present preliminary differences as proven losses.

## Main navigation
Dashboard; Source Import; Payroll Runs; Review Workbench; Employee File; Legal Sources & Rule Versions; Retro/Settlements Follow-up; Tax/Insurance Review; Workpapers & Reports; Settings and Audit Log.

## Dashboard
Summary of reviewed employees, covered payroll groups, open findings, incomplete documents, proposed differences versus verified over/underpayments, correction turnaround time. Numbers must always state the filter period and evidence status.

## Employee workbench
Top: person/employee, employment/legal classification, date-scoped grade, originating organization, payroll period, source file status.
Middle: side-by-side SourceAmount / ExpectedAmount / ReviewedAmount / OfficialCorrectionAmount and variance, by component with null explicitly distinguished from 0.
Bottom: source PDF page viewer + rule/source article + decision/evidence tab + change history; actions Request Evidence, Record Reviewed Value, Flag Central Error, Request Correction, Defer, Refer for Adjustment, Verify Resolution.
An action is not equal to payment approval.

## Historical/retro review
Timeline of grade and rule changes; split financial impact by original entitlement month and financial component; ledger of reported versus independently proved payments; partial/unallocated amounts; warning on future expected dates without proof.

## Import quality view
Report type, period, employee counts, per-line and aggregate totals, bank-group reconciliation, unverified/unknown rows. Importer cannot auto-silence warnings.

## Visual states
Unverified, ParsingFailed, ReviewRequired, SourceMatched, DifferenceDetected, ReviewedWithException, CorrectionRequested, OfficialCorrectionReceived, ReportedInPayroll, PaidVerified, PartiallySettled, ClosedWithEvidence. Never equate one of these states with another.
