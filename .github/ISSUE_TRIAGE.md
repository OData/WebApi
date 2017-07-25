# Issue triage

This document describes the triage process used for the [WebApi repository].

## Prioritization

We use customer feedback as our primary input for prioritizing issues.

* P1: 5 or more asks, legal, or security issues
* P2: 3-4 asks, regressions
* P3: 2 asks
* P4: 1 ask

## What constitutes an "ask"

We do our best to interpret customer feedback as asks. The easiest thing for us to interpret is opening an issue or an explicit comment on the issue stating that the fix would be helpful to a different party. We also make a best effort to interpret positive reactions as asks, although this interpretation is more subjective.

Duplicate issues also constitute an "ask". You can help us identify duplicate issues by adding a comment to the issue that references the duplicate issue number.

Finally, we also count asks from partner teams at Microsoft. We strongly encourage our colleagues to post the "ask" directly on GitHub; however that doesn't always happen.

## Flagging regressions, legal, or security issues

If an issue is a regression from a previous release, explicitly describe the issue as a regression so that we triage it appropriately. For legal or security issues, do not report the issue on GitHub but rather send us an e-mail at odatafeedback@microsoft.com.

## Questions

In general, we ask for questions to be posted on Stack Overflow rather than on GitHub. We tag issues that are actually questions with the question tag. In some cases we may answer the question directly; however we appreciate it the community handles the questions on a forum dedicated to answering questions.

## Contributing fixes for P3/P4 issues

While we appreciate all PRs, we especially count on the community to fix P3/P4 issues. The priority does not reflect the overall importance of an issue. It simply reflects the known asks we have for issues.

Issue priority only affects the *order* in which we consider PRs. A PR for a P4 issue has as much likelihood of being accepted as a PR for a P1 issue.

To contribute a fix for any issue, see our [contribution guidance][contributing].

## Closing questions or issues with few asks

Every so often we close issues that have remained at P3 or P4 for multiple months. If you still need a fix, it is perfectly acceptable to reopen issues. The best way to get a fix for an issue with few "asks" is to [send us a PR][contributing].

We typically redirect questions to Stack Overflow, a forum better suited to getting answers for questions.

## Feedback

We are always open to feedback on how we should revise this process. Feel free to send your feedback to odatafeedback@microsoft.com.

[WebApi repository]: https://www.github.com/odata/WebApi
[contributing]: https://github.com/OData/WebApi/blob/master/.github/CONTRIBUTING.md