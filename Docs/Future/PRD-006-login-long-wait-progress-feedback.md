# Login Long-Wait Progress Feedback Plan

Status: Complete
Priority: High
Last Updated: 2026-04-06

## Problem Statement

Depending on the schedule of scaling rules, a cloud deployment can take significantly longer than normal to respond during login (for example, cold start conditions). During this period, users can interpret the current login state as frozen when there is no immediate visible progress update.

## Goals

- Keep normal login behavior unchanged for standard response times.
- Keep login fast by default with no preflight readiness ping.
- For rare long waits, show clear progress updates so users know the application is still working.
- Maintain existing final error handling surface when login ultimately fails.

## Non-Goals

- No global timeout changes for all API calls.
- No explicit refresh/retry buttons added to this flow.
- No infrastructure-specific wording (for example, server wake-up terminology).

## Functional Requirements

1. Timeout scope:

- Apply a longer timeout to login requests only.
- Target timeout: 2 minutes.

2. Progress indication start:

- If login has not completed within 5 seconds, start showing progress feedback.

3. Progress cadence:

- Continue UI updates every 5 seconds until login succeeds or times out at 2 minutes.
- Each update must include a visibly changing signal (elapsed time and/or message state), not a static repeated sentence.

4. UX messaging:

- Messaging must communicate that processing is ongoing and the app has not frozen.
- The messaging should be periodically updated so the user knows the application has not frozen.
- Avoid infrastructure jargon that users may not understand.
- Progress updates should include elapsed wait time so each refresh is obvious at a glance.
- Use neutral tone copy.

5. Placement and visual treatment:

- Progress feedback is shown inside the login button.
- Progress state includes a subtle spinner and elapsed time.

6. Accessibility:

- Progress updates are announced via a polite aria-live region.

7. Failure behavior:

- If the 2 minute login-only timeout is reached, use the existing ErrorSheet final failure behavior.
- Keep final failure copy generic and unchanged so it remains consistent with broader API failure behavior.

## Suggested UX Copy Direction (Draft)

- Initial long-wait message at 5 seconds:
  - "Signing you in. This is taking a little longer than usual."
- Follow-up every 5 seconds:
  - "Still signing you in (10s)."
  - "Still signing you in (15s)."
  - "Still signing you in (20s)."
  - Continue every 5 seconds until success or 2 minute timeout.
- Keep copy concise to preserve mobile readability.
- Final failure remains in existing error sheet surface.

## Product Decisions (Confirmed)

1. Placement:

- Show long-wait progress inside the login button.

2. Visual treatment:

- Use spinner plus elapsed time.

3. Accessibility:

- Use aria-live="polite" updates for long-wait progress.

4. Message tone:

- Use neutral tone copy.

5. Timeout failure copy:

- Keep existing generic ErrorSheet behavior and copy.

## Technical Notes (Implemented)

- Candidate implementation area: login page and login form state handling.
- Keep timeout override local to login operation to avoid side effects in other API operations.
- Ensure progress timer lifecycle is cleaned up when login completes, fails, or component unmounts.

## Acceptance Criteria

1. Standard login responses are unchanged in behavior and perceived speed.
2. No preflight ping occurs before login submission.
3. At 5 seconds of pending login, visible progress feedback appears.
4. Feedback updates every 5 seconds while pending.
5. Each update visibly changes from the previous one (for example elapsed seconds increase).
6. Long-wait progress is shown in the login button with spinner plus elapsed time.
7. Long-wait progress updates are announced through a polite aria-live region.
8. Login request times out at 2 minutes only for login flow.
9. Timeout failure ends in existing ErrorSheet behavior with generic final error copy unchanged.
