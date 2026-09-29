---
description: Validate implementation against plan, verify success criteria, identify issues
---

# Validate Plan

You are tasked with validating that an implementation plan was correctly executed, verifying all success criteria and identifying any deviations or issues.

## Initial Setup

When invoked:
1. Locate the plan (path provided, or search recent commits for plan references)
2. Gather implementation evidence:
   ```bash
   git log --oneline -n 20
   git diff HEAD~N..HEAD
   scripts/check.sh
   ```

## Validation Process

### Step 1: Context Discovery

1. Read the implementation plan completely
2. Identify what should have changed
3. Spawn parallel research tasks to verify implementation:
   - Verify code changes match plan specifications
   - Verify tests were added/modified as specified
   - Check build and test results

### Step 2: Systematic Validation

For each phase:
1. Check completion status (look for `- [x]` in plan)
2. Run automated verification commands
3. Document pass/fail status
4. Assess what still needs manual testing

### Step 3: Validation Report

```markdown
## Validation Report: [Plan Name]

### Implementation Status
✓ Phase 1: [Name] - Fully implemented
⚠️ Phase 2: [Name] - Partially implemented (see issues)

### Automated Verification Results
✓ Build + tests: `scripts/check.sh`
✗ Issues: [details]

### Code Review Findings

#### Matches Plan:
- [what was implemented correctly]

#### Deviations from Plan:
- [differences and why they matter]

#### Potential Issues:
- [anything that looks wrong or incomplete]

### Manual Testing Required:
- [ ] [specific thing to verify]
- [ ] [edge case to test]

### Recommendations:
- [actionable next steps]
```

## Validation Checklist

Always verify:
- [ ] All phases marked complete are actually done
- [ ] Build and tests pass
- [ ] Code follows existing Clean Architecture and web feature patterns
- [ ] No regressions introduced
- [ ] Error handling is robust
- [ ] Architecture tests still pass
