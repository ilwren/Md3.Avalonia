# MdDismissible desktop motion follow-up — 2026-10-31

## Reported defect

On desktop, the release motion felt abrupt. After a drag the action label could disappear before the foreground had covered it, and an end-to-start dismissal looked like the complete control vanished without a coherent exit.

## Existing Avalonia implementation

`MdDismissible` already used Avalonia pointer events, pointer capture, a translated foreground, directional backgrounds, confirmation, velocity/threshold dismissal, and a delayed height collapse. The defects were in orchestration rather than a missing control:

- the duration transition restarted the foreground from zero velocity when the pointer was released;
- `:dismissed` faded the foreground text before its translation completed;
- returning `DragOffset` to zero hid both backgrounds immediately although the foreground was still physically offset;
- both backgrounds were present at rest and only the opposite one was hidden after direction selection;
- the Gallery gave the control a fixed `Height`, preventing the dismissed row from collapsing its layout slot;
- the Gallery scroll viewer also accepted mouse-drag scrolling, competing with the horizontal desktop gesture.

## Original/Flutter reference

Flutter's official `widgets/dismissible.dart` remains the behavioral reference. It separates direct drag progress, a 200 ms movement settle, confirmation, and a subsequent 300 ms cross-axis collapse. It keeps the selected background behind the moving child and does not fade the child away before translation finishes. Its direction is logical and therefore resolves through text direction.

This implementation does not copy Dart source or Flutter rendering. It maps those behavioral contracts onto Avalonia and the library's existing Material motion system.

## Decisions

1. Keep direct pointer manipulation one-to-one after a four-DIP horizontal slop and yield vertical gestures to the parent scroller. A desktop fling must also travel at least 12% of the row, preventing a tiny high-velocity mouse sample from unexpectedly removing the control.
2. Transfer the sampled release velocity to `MdSpatialSpringRunner`; no duration transition is attached to the foreground transform.
3. Keep the revealed action selected by the visual settle direction until the foreground actually reaches rest. This also prevents a spring overshoot from briefly switching backgrounds.
4. Do not fade the foreground on committed dismissal. Translation carries its text out, then the existing height collapse removes the row.
5. Stretch one directional background across the row and keep both hidden at rest.
6. Resolve `StartToEnd` and `EndToStart` through `FlowDirection`.
7. In Gallery, put the 72-DIP minimum on content rather than `MdDismissible`, and disable desktop mouse-drag scrolling on the parent viewer.
8. Reduced motion and no-motion still snap spatial movement while retaining the established short effects semantics.

## Regression evidence

Headless coverage verifies that:

- the foreground uses the spring runner and has no competing transform transition;
- action text and foreground content remain present during start-to-end and end-to-start settle;
- only the correct directional background is visible;
- the foreground no longer receives an early dismissed opacity;
- no-motion still completes presence removal and height collapse deterministically;
- confirmation rejection settles the row back to zero.
