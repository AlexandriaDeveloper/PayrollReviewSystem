# GitHub collaboration — Owner / ChatGPT Architect / Gemini Developer

## Single source of truth
Use this repository's **Issues** to assign decisions and tasks, **Pull Requests** to submit/test/review code, **ADRs** for architecture choices, and **docs/** for sustained domain knowledge. Chat transcripts are supporting discussion, not a hidden implementation authorization.

## Parties
- **Owner**: business policy, legal interpretation approvals, priority, scheduling, merge and deployment permission.
- **ChatGPT Architect**: analyze requirements, produce explicit Issue handoff, review PR diff/tests, label findings by severity, recommend approve/revise; does not represent personal legal authority.
- **Gemini Developer**: implement accepted Issues with branch + PR; report evidence and respond to review comments.
- **GitHub**: durable communication surface; PR comments/reviews are the exchange.

## Standard lifecycle
1. Owner and architect agree requirement and scope.
2. Architect records Issue: context, status, constraints, file/module boundaries, scenarios, test cases, approval and explicit exclusions.
3. Owner authorizes implementation of that Issue.
4. Gemini creates branch from checked main HEAD, develops, opens **Draft PR**, attaches tests/verification and exact SHA.
5. Gemini requests architectural review in PR. The architect reads real diff and related code/tests, responds:
   - **BLOCKER**: invalid financial result, invented legal rule, identity confusion, evidence loss, broken audit control, unsafe mutation, PII exposure.
   - **MAJOR**: missing acceptance criterion, broken integration/migration, important untested edge case.
   - **MINOR**: maintainability, nonblocking documentation/UI issue.
   - **QUESTION**: needs Owner rule or original source.
6. Gemini fixes findings with new commits and replies referencing test evidence. Architect rereviews **new HEAD**; earlier approval does not cover later unrelated code.
7. Only Owner-designated maintainers merge and authorize deployment. Merge is not deployment.
8. Update docs/decision register and change log for substantive changes.

## Owner delegation: autonomous Markdown-only merges (2026-11-06)
The Owner explicitly authorized the ChatGPT architect to **review and merge Markdown (`.md`)-only changes in this repository without asking for separate merge confirmation each time**. This is an exception to Owner-only merge approval **for documentation artifacts only**.

- Read the full changed-file list and diff (or compare actual documents); confirm every changed path is a Markdown file and that the changes are genuinely documentation, instructions, specifications, ADRs or tests-as-text.
- Check for confidential personnel/financial data, credentials, unsupported legal formulas, or contradictions with verified Owner policy. Do not silently introduce unverified requirements as approved law.
- Check mergeability and reconcile concurrent documentation edits without discarding earlier content; use a PR linked to a GitHub Issue where practical.
- Merge docs-only PRs after completing this review; do not await a fresh Owner message **solely** for approval to merge Markdown.
- **Never extend this permission** to application code, configuration, CI workflows, binaries, database migrations, secrets, production deployment or changes to other repositories. Mixed PRs require independent Owner authorization for non-Markdown changes or extraction of a docs-only PR.
- Gemini must not self-merge its PRs. ChatGPT's delegated docs permission does not give Gemini merge privileges.
- Document merge SHA and any review findings in GitHub. If the documentation itself includes a material business/legal change not grounded in Owner decisions or verified sources, mark it proposed/review-required rather than declaring the rule approved.
- Existing implementation, legal-rule activation, schema and deployment authorization gates remain unchanged.

This policy records explicit delegated authority; the Markdown itself does not create permissions or background monitoring. A scheduled review/merge automation, if configured, must use the same safety restrictions.

## Handoff message convention
PR body: Task, motivation, approved reference, change summary, test results, source/evidence handling, open questions, backward compatibility, privacy assessment.
Review comment: severity + precise file/behavior + reproduction + expected behavior + recommended test.
After each Gemini change: issue/PR URL, branch, SHA, changed files, tests run, unanswered blockers.

## No false unattended automation
AI models do not inherently read each other's replies on GitHub in real time. At present ChatGPT review occurs when explicitly invoked in this chat or through a **separately configured scheduled automation**. A GitHub mention or comment does **not** guarantee an automatic review, and no scheduled monitoring is established by this document alone.

## Definition of Done
Owner-approved scope implemented; review blockers resolved; deterministic tests pass; confidential fixtures excluded; docs/source citations updated; PR inspected against intended base; no uncontrolled production changes.

## First task
Documentation baseline Issue/PR is governance-only. No application development until acceptance of design and an implementation Issue is authorized.
