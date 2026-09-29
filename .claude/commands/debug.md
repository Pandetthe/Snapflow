---
description: Debug issues by investigating logs, git history, and build/test output
---

# Debug

You are tasked with helping debug issues during development or testing. Investigate problems by examining logs, recent changes, and build output - without editing files.

## Initial Response

When invoked WITH a plan/context file:
```
I'll help debug issues with [file]. What specific problem are you encountering?
- What were you trying to implement/test?
- What went wrong?
- Any error messages or build failures?
```

When invoked WITHOUT parameters:
```
I'll help debug your current issue.

Please describe:
- What are you working on?
- What specific problem occurred?
- Error messages (if any)?
```

## Investigation Strategy

### Step 1: Understand the Problem

After the user describes the issue, check:
- Current git branch and recent commits
- Uncommitted changes that might be relevant
- When the issue started

### Step 2: Investigate

Spawn parallel Task agents:

```
Task 1 - Build output:
Run `scripts/check.sh` and capture errors/warnings.
Return: Exact error messages with file:line references.
```

```
Task 2 - Test failures:
Run `scripts/check.sh` and capture failures.
Return: Failing tests and assertion messages.
```

```
Task 3 - Git state:
Check `git status`, `git log --oneline -10`, `git diff`.
Return: Recent changes that might be related to the issue.
```

```
Task 4 - Code investigation:
Use codebase-analyzer to trace the relevant code path described in the issue.
Return: Relevant file:line references and logic flow.
```

### Step 3: Present Findings

```markdown
## Debug Report

### What's Wrong
[Clear statement based on evidence]

### Evidence Found

**Build/Test output**:
- [Error with file:line]

**From Git**:
- [Recent changes that might be related]

**From Code**:
- [Logic flow issue]

### Root Cause
[Most likely explanation]

### Next Steps
1. **Try This First**: [specific command or action]
2. **If That Doesn't Work**: [alternative]
```

## Important Notes

- **Pure investigation** - no file editing
- **Always require problem description** before diving in
- **Some issues require manual inspection** (browser console, runtime behavior) - guide the user there
