# Balatro → Tarotro: event queue and T/VT motion layer

Port notes and a working C# implementation of the two systems that carry most of Balatro's game feel. Source analysed: the decompiled LÖVE/Lua tree in `Desktop/Balatro` (`engine/`, `functions/`, `card.lua`, `game.lua`, `cardarea.lua`).

Everything here is reimplemented from the _mechanism_, not transliterated. Line references point at the original so you can check my reading, not so you can copy the file.

---

## 1. Why these two first

Almost everything people call "Balatro's juice" comes from two decisions that have nothing to do with art:

1. **Gameplay code never animates anything.** It writes a target transform. A separate integrator moves the visual transform toward it. Interrupting, re-targeting and cancelling are free because there is no tween object to kill.
2. **Presentation is a queue of blocking events, not coroutines.** The scoring rules resolve synchronously in one pass; what you _see_ is a list of events that plays out over the next few seconds. The rules stay unit-testable and the sequence stays ordered.

Port these two and the rest of the game feels right almost by accident. Port the joker system first and you will spend months wondering why it looks flat.

---

## 2. What is in the drop

```
Tarotro/
  Sequencing/
    GameEvent.cs        — one unit of sequenced work (5 trigger types)
    EventQueue.cs       — named lanes, fixed-step processing, pause handling
  Motion/
    Moveable.cs         — Transform2D, MotionFrame, the T/VT integrator
    MotionTuning.cs     — every magic number from the source, as a ScriptableObject
    MotionSystem.cs     — owns and ticks all moveables in one flat loop
    MoveableView.cs     — the only place a Unity Transform is written
    HandLayout.cs       — the fan layout, as a worked example of "logic writes T"
  Runtime/
    GameLoopRunner.cs   — the single MonoBehaviour that drives both systems
  Examples/
    ScoringSequenceExample.cs — a hand-scoring sequence as straight-line code
  Tests/
    MotionAndSequencingChecks.cs — 26 behavioural checks (all passing)
    UnityStub.cs        — minimal UnityEngine surface so the checks run headless
```

About 1,100 lines excluding tests and comments. No third-party dependencies, no DOTween, no UniTask, no allocation in the per-frame path.

---

## 3. The T/VT layer

Source: `engine/moveable.lua`, plus the coefficient block at `game.lua:2617-2624`.

Every object carries two transforms:

||meaning|who writes it|
|---|---|---|
|`T`|target — where the object _should_ be, right now|gameplay, layout|
|`VT`|visual — where it is actually drawn this frame|the integrator|

Each frame, `VT` chases `T`:

```csharp
float e    = Mathf.Exp(-50f * dt);                  // framerate-independent
float pull = (1f - e) * 35f * moveDelta;            // stiffness
velocity.x = e * velocity.x + (T.X - VT.X) * pull;  // lagged velocity
// clamp |velocity| to 70 * moveDelta, then:
VT.X += velocity.x;
```

Three things to notice, because they are the whole design:

**It is second-order, not a lerp.** The velocity itself is smoothed, so the object carries momentum. Measured on the shipped tuning: a 5-unit jump **overshoots by 6.6%**, rings back under the target, and settles exactly at **0.27 s**. That arrive-and-settle is what DOTween users approximate with `Ease.OutBack` — here it falls out of the model, identically, for every object, for free.

**`exp(-k·dt)` is the reason it is framerate-independent.** Not `Lerp(a, b, k*dt)`, which is subtly wrong at variable framerates. My check: the same 0.25 s of wall time lands within 0.015 units of the same place at 60 fps and at 144 fps.

**Rotation is driven by horizontal speed.** This one line is most of why cards read as objects rather than sprites:

```csharp
desiredR = T.R + 0.015f * velocity.x / moveDelta + juiceRotation * 2f;
```

It is aggressive: at clamped max speed the lean peaks at **1.09 rad ≈ 62°**. That is faithful — watch a Balatro card fly from the deck — but it is the first knob to turn down if Tarotro's art reads badly at that angle. `MotionTuning.RotationFromSpeed`.

### Juice

`Moveable.JuiceUp(amount, rotationAmount)`. Source: `moveable.lua:250`. A fast sine inside a decaying envelope over 0.4 s, plus an immediate squash:

```
VT.Scale = 1 - 0.6 * amount              // squash NOW, so the pop reads as recoil
scale    = amount * sin(50.8 t) * remaining³
rotation = rAmt   * sin(40.8 t) * remaining²
```

Default rotation amplitude is a random ±0.6·amount. That randomness is deliberate — it is why a row of cards wobbles out of phase instead of pulsing in unison. Pass `0` explicitly for text and HUD numbers, where a wobble looks broken.

This one method should be called on essentially every meaningful event: card scored, joker triggered, counter incremented, button pressed, blind defeated. In the source it is called from well over a hundred sites.

### Tuning constants

All in `MotionTuning`, with the source value in the tooltip.

|Knob|Value|Source|
|---|---|---|
|`PositionRate`|50|`exp(-50*dt)`|
|`ScaleRate`|60|`exp(-60*dt)`|
|`RotationRate`|190|`exp(-190*dt)` — much stiffer than position|
|`Stiffness`|35|`(T-VT)*35*dt`|
|`MaxSpeed`|70|`70*move_dt`|
|`MaxMoveDelta`|1/20|`min(1/20, real_dt)`|
|`SnapDistance`|0.01|position snap threshold|
|`SizeRate`|8|W/H is linear, not smoothed|
|`RotationFromSpeed`|0.015|the lean|
|`HoverScaleBonus` / `DragScaleBonus`|0.05 / 0.1||
|`JuiceDuration`|0.4 s||
|`ParallaxStrength`|1.5|shadow offset from screen centre|

> **Unit scale is load-bearing.** These numbers assume the source's game units, where a card is ~2.05 × 2.75 units (`G.CARD_W = 2.4*35/41`, `TILESIZE = 20` px). `Stiffness = 35` and `SnapDistance = 0.01` are meaningless without that scale. If Tarotro cards are 1 unit or 100 pixels wide, the feel will be wrong until you either author at a comparable scale or re-tune stiffness and snap _together_. **Decide your unit scale before you tune anything, and tune globally, never per prefab.**

---

## 4. The event queue

Source: `engine/event.lua`.

Five trigger types, and they cover more than they look like they should:

|Trigger|Behaviour|
|---|---|
|`Immediate`|runs once this step|
|`After`|waits, then runs — the workhorse|
|`Before`|runs every step, releases the queue after the delay|
|`Ease`|drives a float to a target over a duration|
|`Condition`|runs until it returns true, no timeout|

Two flags do the sequencing:

- **`Blocking`** — holds up everything behind it in the same lane.
- **`Blockable`** — if false, runs even while something ahead is blocking.

And the removal rule that makes it work: an event leaves the queue only when it has **both** completed _and_ had its time elapse. That is why `After(0.4, …)` reliably occupies 0.4 s of the sequence whether its body takes a microsecond or a frame.

**Lanes** (`base`, `unlock`, `achievement`, `tutorial`, `other`) are independent, so an achievement popup can never stall scoring.

The queue is evaluated on a fixed 1/60 s cadence, at most one pass per frame, as in the source. This throttles how often the queue is _checked_; event timing is measured against wall clocks, so a 30 fps Yandex device does not run the sequence at half speed.

### Clocks and pausing

Two clocks: `RealTime` (always advances) and `TotalTime` (frozen while paused, scaled by `Speed`). Events pick one. An event created while unpaused does not tick during a pause; one created during a pause does.

**Pause with the queue's flag, not `Time.timeScale`.** Everything is driven by `unscaledDeltaTime`, so hover, juice and menus keep animating while the game sits still — which is exactly how Balatro feels when you open the shop.

### Do not replace this with UniTask or R3

For the scoring path specifically. I know your stack has both, and they are the right tools for UI and meta flows. But ordered, synchronous, single-threaded resolution is what makes a seed reproducible, and `async` reintroduces scheduling you do not control. Keep MessagePipe for "blind defeated" style broadcasts; keep the scoring pipeline in one loop.

---

## 5. Wiring it up

```csharp
// LifetimeScope
builder.RegisterInstance(motionTuning);
builder.Register<EventQueue>(Lifetime.Singleton);
builder.Register<MotionSystem>(Lifetime.Singleton);
builder.RegisterComponentInHierarchy<GameLoopRunner>();
```

Order per frame is not optional:

```
Update      → Queue.Tick(dt)     // writes targets
            → Motion.Tick(dt)    // reads targets, moves VT
LateUpdate  → view.Apply()       // pushes VT into Unity transforms
```

`GameLoopRunner` has `[DefaultExecutionOrder(-100)]` so it runs before gameplay scripts that read positions.

### Coordinate conversion

The source is Y-down with a top-left origin, and `T.X/T.Y` is the object's _corner_ (so `W/H` participate in the centring maths). Unity is Y-up and pivots at centre. `MoveableView` converts once, in one place. **Keep it there.** If the flip leaks into gameplay code you will spend a week on sign errors.

---

## 6. What the example shows

`ScoringSequenceExample.ScoreHand` resolves a hand and presents it. Read it next to `G.FUNCS.evaluate_play` (`functions/state_events.lua:571`). It is ordinary top-to-bottom code — no coroutines, no async, no callbacks — and it plays out over about four seconds, in order, and stays correct if the player mashes buttons.

The split is the point:

- **rules run now**, synchronously → deterministic, unit-testable without a PlayMode harness
- **presentation is enqueued** → plays out later

Note the retrigger pattern, lifted from the source: repetitions are collected into a list _first_, then replayed. That is how seals, "retrigger this card" and "retrigger all" compose without special cases. Worth adopting before you write your first joker.

---

## 7. Verification

`Tests/MotionAndSequencingChecks.cs` — 26 checks, all passing. Compiled with `mcs -langversion:latest` against `Tests/UnityStub.cs` and run headless; it has no Unity dependency beyond the stub, so you can also drop it into an EditMode NUnit fixture by replacing `Check` with `Assert`.

Covered: event ordering, blocking vs unblockable, wait accuracy (1.0 s fires at 1.05 s — one 1/60 step of granularity, as designed), ease landing exactly on target, `EaseInt` staying integral, pause/resume, position convergence and exact snap, bounded overshoot, speed clamp, 60 vs 144 fps equivalence, juice pre-squash and decay to rest, and hand layout ordering/fan/highlight.

The two results worth remembering: **6.6% overshoot, 0.27 s settle** and **62° peak lean**. If a tuning change moves those a lot, you have changed the feel, not just a number.

---

## 8. Known gaps — deliberately not ported

- **The Major/Minor role hierarchy.** The source welds child moveables to parents with `Strong`/`Weak` bonds per channel (xy, wh, r, scale). Unity's Transform parenting covers most of it. Add it only if you hit a case where you want a child to inherit position but compute its own rotation.
- **`pinch` on W/H** is ported but unused by anything in this drop; it is there for collapsing panels.
- **Idle bob in `HandLayout`** is phase-offset by `x`, not by index — keep that if you rewrite the layout, it is why the hand ripples instead of pulsing.
- **Reduced-motion** is a flag on `MotionTuning`, honoured by `JuiceUp` and `HandLayout`. Wire it to a settings toggle; Poki and Yandex both reward it.

## 9. WebGL notes

- `MotionSystem` ticks a flat `List<Moveable>` with an indexed for-loop: no enumerator allocation, no per-component `Update` dispatch. A full hand plus jokers plus HUD is easily 60+ moveables, and Unity's Update dispatch is comparatively expensive on WebGL.
- `EventQueue` reuses one `EventStep` struct; nothing allocates per step.
- The closures in the scoring sequence _do_ allocate. That is fine at a few dozen per hand, but if you ever queue hundreds, pool the delegates or switch to a struct-based command.
- Float behaviour differs between the editor and WebGL. Anything that must be deterministic (seeded RNG, scoring) should avoid accumulating floats across frames. The motion layer accumulates deliberately — it is presentation only, keep it that way.

---

## 10. Next from the same source

Roughly in order of value, once these two land:

1. **Context-dispatch joker pipeline** — `Card:calculate_joker` + `eval_card` (`functions/common_events.lua:580`). ~40 context hooks; one `ScoreContext` struct and one `ICardEffect` interface in C#. The resolution order inside the hand loop is the part clones get wrong: `before` jokers → blind's `modify_hand` → per card (repetitions, then card effect, then each joker's `individual`) → held-in-hand → `joker_main`, with blueprint/brainstorm recursing through the same path behind a depth guard.
2. **Seeded RNG with per-stream keys** — `pseudohash` / `pseudoseed` / `pseudorandom` (`functions/misc_functions.lua:279-320`). Port the _architecture_ (a named stream per consumer, advanced on use, mixed with a hashed run seed), not the hash — `math.randomseed` on LuaJIT is not `System.Random`. Use PCG32 or xoshiro.
3. **Balance curves** — blind scaling `misc_functions.lua:919` (authored table for antes 1–8, formula beyond), interest `$1 per $5 held, capped` (`state_events.lua:1191`), edition odds `common_events.lua:2055`.
4. **`number_format` / `score_number_scale`** (`misc_functions.lua:956`) — scientific notation past 1e11 and font scale shrinking with magnitude. You will need this the moment x-mult compounds.

---

## 11. On provenance

Systems, maths and architecture are fair to learn from and reimplement. The Lua files, the sprites, shaders and sounds, and the joker names and description text are Playstack / LocalThunk's. Nothing in this drop is copied source — it is a reimplementation from behaviour, with the original cited so the reasoning is checkable. Keep it that way for Tarotro: your own card set, your own copy, your own art.