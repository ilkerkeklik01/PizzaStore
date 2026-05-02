---
name: update-docs
description: Update README.md and CHANGELOG.md after a completed feature. Always asks for explicit user confirmation before writing. Follows the existing changelog format.
---

You are updating the project documentation for the PizzaStore project. The CLAUDE.md rules require **explicit user confirmation** before modifying `README.md` or `CHANGELOG.md`.

## Step 1 — Summarise what changed

Read the recent git log to understand what was completed:
```bash
cd /Users/ilkerkeklik/Desktop/Projects/PizzaStore && git log --oneline -10
```

Also read the current README and CHANGELOG to understand their existing format before proposing changes.

## Step 2 — Ask for confirmation

Present the user with a clear summary of the proposed changes to each file:
- Which section of README would be updated and why
- What CHANGELOG entry would be added (version, date, description)

**Do NOT write to either file until the user explicitly says yes.**

## Step 3 — Write the updates

### CHANGELOG.md format

Follow the existing format in the file. Typical pattern:
```markdown
## [Unreleased] / ## [x.y.z] - YYYY-MM-DD

### Added
- Short description of new feature

### Changed
- Description of changes to existing functionality

### Fixed
- Bug fix descriptions
```

Always insert new entries at the top of the file, below the header.

### README.md format

- Keep updates concise — add to existing sections rather than creating new ones
- Update feature lists, API endpoint tables, or setup instructions as relevant
- Do not remove existing content unless it is clearly outdated

## Rules

- Never skip the confirmation step, even if the user's request seems unambiguous
- If the user says "yes" to one file but not the other, only update the approved file
- Today's date for changelog entries: use the current date from the system
