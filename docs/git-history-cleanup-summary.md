# Git History Cleanup Summary

**Date:** 2026-05-28  
**Repository:** OData/WebApi (git@github.com:OData/WebApi.git)

## Problem

Push to Azure DevOps failed with error:

> VS403655: The push was rejected because `WebApiOData.` ends with '.', which isn't a valid file or directory ending character.

## Root Cause

A file named `sln/WebApiOData.` (with a trailing dot) existed in the git history:

- **Introduced in:** commit `0237942` — "Reorganize WebApi source code to eliminate the OData folder" (Biao Li, 2017-06-05)
- **Removed in:** commit `6e43ad9` — "Fix project and solution files so that build & test can pass"

Although the file was already deleted from the latest `master` branch, Azure DevOps validates **all commits in history**, causing the push/import to be rejected.

## Steps Taken

### 1. Identified the Problematic File
- Searched all commits and found `sln/WebApiOData.` (a 204-line solution file with an invalid trailing dot in its name).

### 2. Cloned to a New Directory
```bash
git clone git@github.com:OData/WebApi.git C:\github\odata\WebApi-filter
```

### 3. Rewrote History with `git filter-repo`
```bash
cd C:\github\odata\WebApi-filter
python C:\Python312\Lib\site-packages\git_filter_repo.py --path "sln/WebApiOData." --invert-paths --force
```
This removed the file from all historical commits. Commit hashes were rewritten (e.g., `0237942` → `ad01f16`).

### 4. Verified the Cleanup
- Confirmed the file no longer appears in any commit in the filtered repo.
- Compared the original and filtered repos to confirm only `sln/WebApiOData.` was removed.

### 5. Backed Up Original Master
```bash
# From original repo
git push origin master:master-backup
```

### 6. Force-Pushed Cleaned History
```bash
cd C:\github\odata\WebApi-filter
git remote add origin git@github.com:OData/WebApi.git
git push --force origin master
```
- Temporarily disabled branch protection to allow force-push.
- Re-enabled branch protection after push.

### 7. Cleaned Up Backup Branch
```bash
# Saved backup locally
cd C:\github\odata\WebApi
git fetch origin master-backup
git branch master-backup origin/master-backup

# Deleted from remote (to avoid blocking Azure DevOps import)
git push origin --delete master-backup
```

## Current State

| Item | Status |
|------|--------|
| Remote `master` | ✅ Clean history (no trailing-dot file) |
| Local `master-backup` (in `C:\github\odata\WebApi`) | ✅ Preserved as safety net |
| Remote `master-backup` | ❌ Deleted (would block Azure import) |
| Filtered repo location | `C:\github\odata\WebApi-filter` |

## Notes

- **Other remote branches** still have the original (unfiltered) history. If they also need to be imported to Azure DevOps, they will need the same `filter-repo` treatment or be deleted from the remote.
- The local backup at `C:\github\odata\WebApi\master-backup` retains the original history if ever needed for reference.
