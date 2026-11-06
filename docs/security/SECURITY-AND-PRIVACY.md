# Payroll audit confidentiality and security baseline

## Public repository notice
This repo is PUBLIC as of initial inspection. Do not commit actual:
- National IDs, employee names linked to pay, employment codes, passport/bank/insurance numbers, salary PDFs, XLSX exports or personal screenshots.
- User's legal library if confidential or restricted, database backups, SQL dumps, debug logs or example outputs containing confidential employee data.
- .env, appsettings secrets, tokens, passwords, private endpoints, production config, private signing keys.
Use fictional synthetically generated fixtures and anonymized general designs.

## Authorization boundaries
- Source intake: upload/read and validate only.
- Domain/legal approver: approve rule version after actual source verification.
- Reviewer: inspect differences and enter separate approved overlay; no central payroll edit.
- Senior reviewer: approve final review findings under defined policy.
- Administrator: configure users/system, not necessarily entitled to approve money.
- Owner-authorized deployment operator: distinct from architect review.
Scope permissions by organization, role and task where practical.

## Technical controls for later implementation
Validate file types/signatures, size/page limits and parser resource limits; isolate untrusted PDF processing. Encrypt protected data at rest and in transit. Minimize/segment personal data; limit export and mask identifiers. Log meaningful access and changes without storing full PII payload in logs. Retain and version originals, hash to detect accidental changes, secure backups and access logs, recovery procedures, audit retention policy and safe deletion/redaction under governance. Consider formal threat model before production.

## High-risk anti-patterns
- Central imported PDF overwrites validated master record or tribunal/court ruling fact.
- Source amounts change after reviewer finds mismatch.
- Generic admin can purge audit events, evidence or completed runs.
- Blind formula execution from untrusted source.
- Exposing local API publicly without auth or TLS.
- Debug file with employee IDs/financial details committed to GitHub.
- Auto employee repayment or salary changes initiated by reviewer without official delegated authority.

## Security review of Gemini PRs
Before merge check authentication and authorization at API (not only UI), data minimization, audit attribution, privacy tests and no secret leaks. Production secrets must be supplied outside Git, and any leak requires rotation and incident review.
