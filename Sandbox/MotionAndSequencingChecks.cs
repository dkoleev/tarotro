using System;
using System.Collections.Generic;
using Tarotro.Motion;
using Tarotro.Sequencing;

static class Harness
{
    static int fails = 0;
    static void Check(bool ok, string label, object detail = null)
    {
        if (!ok) { fails++; Console.WriteLine("FAIL  " + label + (detail != null ? "  -> " + detail : "")); }
        else Console.WriteLine("ok    " + label + (detail != null ? "  (" + detail + ")" : ""));
    }

    const float DT = 1f / 60f;

    static float Run(float dt, float seconds, MotionTuning tune)
    {
        var mm = new Moveable(new Transform2D(0, 0, 2.05f, 2.75f));
        mm.T.X = 5f;
        float tt = 0f;
        int n = (int)(seconds / dt);
        for (int i = 0; i < n; i++) { tt += dt; mm.Tick(new MotionFrame(dt, tt, tune), 20f); }
        return mm.VT.X;
    }

    static void Main()
    {
        // ---------------- 1. ordering + blocking -------------------------
        var q = new EventQueue();
        var log = new List<string>();

        q.Do(() => log.Add("a"));
        q.Wait(0.5f);
        q.Do(() => log.Add("b"));
        q.Add(GameEvent.Immediate(() => log.Add("unblockable")).Unblockable().NonBlocking());

        // First tick: 'a' runs; the wait blocks 'b'; the unblockable one still runs.
        q.Tick(DT);
        Check(log.Contains("a"), "immediate event ran on first step");
        Check(!log.Contains("b"), "blocking wait held the next event");
        Check(log.Contains("unblockable"), "unblockable event ran past the block");

        // Advance past the wait.
        for (int i = 0; i < 40; i++) q.Tick(DT);
        Check(log.Contains("b"), "event released after the wait elapsed");
        Check(log.IndexOf("a") < log.IndexOf("b"), "order preserved");
        Check(q.Count() == 0, "queue drained", q.Count());

        // ---------------- 2. wait duration is accurate -------------------
        var q2 = new EventQueue();
        float firedAt = -1f;
        q2.Wait(1.0f);
        q2.Do(() => firedAt = q2.TotalTime);
        for (int i = 0; i < 120 && firedAt < 0f; i++) q2.Tick(DT);
        Check(firedAt >= 1.0f && firedAt < 1.05f, "1.0s wait fired at the right time", firedAt);

        // ---------------- 3. ease ----------------------------------------
        var q3 = new EventQueue();
        float v = 0f;
        q3.Add(GameEvent.Ease(() => v, x => v = x, 100f, 0.5f));
        float seenMid = -1f;
        for (int i = 0; i < 60; i++)
        {
            q3.Tick(DT);
            if (i == 15) seenMid = v;
        }
        Check(Math.Abs(v - 100f) < 0.001f, "ease landed exactly on target", v);
        Check(seenMid > 5f && seenMid < 95f, "ease was mid-flight at t=0.25s", seenMid);

        float iv = 0f;
        var q4 = new EventQueue();
        q4.Add(GameEvent.EaseInt(() => iv, x => iv = x, 7f, 0.3f));
        bool allWhole = true;
        for (int i = 0; i < 40; i++) { q4.Tick(DT); if (iv != (float)Math.Floor(iv)) allWhole = false; }
        Check(allWhole && Math.Abs(iv - 7f) < 0.001f, "EaseInt stayed integral and landed", iv);

        // ---------------- 4. pause ----------------------------------------
        var q5 = new EventQueue();
        bool ran = false;
        q5.Wait(0.2f);
        q5.Do(() => ran = true);
        q5.Paused = true;
        for (int i = 0; i < 60; i++) q5.Tick(DT);
        Check(!ran, "paused queue did not advance game-clock events");
        q5.Paused = false;
        for (int i = 0; i < 30; i++) q5.Tick(DT);
        Check(ran, "queue resumed after unpause");

        // ---------------- 5. Moveable convergence -------------------------
        var tune = MotionTuning.Default;
        var m = new Moveable(new Transform2D(0, 0, 2.05f, 2.75f));
        m.T.X = 5f; m.T.Y = 3f;

        float t = 0f;
        int steps = 0;
        while (steps < 600 && (m.VT.X != m.T.X || m.VT.Y != m.T.Y))
        {
            t += DT; steps++;
            m.Tick(new MotionFrame(DT, t, tune), 20f);
        }
        Check(m.VT.X == 5f && m.VT.Y == 3f, "moveable snapped exactly onto target");
        Check(steps > 10 && steps < 180, "convergence took a sane number of frames", steps + " frames / " + (steps * DT).ToString("0.00") + "s");

        // The integrator is a second-order system (lagged velocity + proportional pull),
        // so it overshoots and settles. That is intentional: it is the "arrive and
        // settle" that DOTween users fake with OutBack. Assert it is bounded, not absent.
        var m2 = new Moveable(new Transform2D(0, 0, 2.05f, 2.75f));
        m2.T.X = 5f;
        float maxX = 0f; float minAfterPeak = 999f; t = 0f;
        for (int i = 0; i < 300; i++)
        {
            t += DT; m2.Tick(new MotionFrame(DT, t, tune), 20f);
            maxX = Math.Max(maxX, m2.VT.X);
            if (maxX > 5f) minAfterPeak = Math.Min(minAfterPeak, m2.VT.X);
        }
        float overshootPct = (maxX - 5f) / 5f * 100f;
        Check(overshootPct > 2f && overshootPct < 12f, "overshoot present but bounded", overshootPct.ToString("0.0") + "%");
        Check(m2.VT.X == 5f, "settles exactly on target after the ring-down");

        // Rotation lean peaks hard at max speed — the most aggressive term in the model.
        var m5 = new Moveable(new Transform2D(0, 0, 2.05f, 2.75f));
        m5.T.X = 5f; t = 0f;
        float peakLean = 0f;
        for (int i = 0; i < 60; i++) { t += DT; m5.Tick(new MotionFrame(DT, t, tune), 20f); peakLean = Math.Max(peakLean, Math.Abs(m5.VT.R)); }
        Check(peakLean > 0.5f, "fast movement produces a strong lean (radians)", peakLean.ToString("0.00") + " rad = " + (peakLean * 57.3f).ToString("0") + " deg");

        // ---------------- 6. speed clamp ----------------------------------
        var m3 = new Moveable(new Transform2D(0, 0, 2.05f, 2.75f));
        m3.T.X = 10000f;
        t = 0f;
        float prev = m3.VT.X; float maxStep = 0f;
        for (int i = 0; i < 60; i++)
        {
            t += DT; m3.Tick(new MotionFrame(DT, t, tune), 20f);
            maxStep = Math.Max(maxStep, m3.VT.X - prev);
            prev = m3.VT.X;
        }
        float allowed = tune.MaxSpeed * Math.Min(tune.MaxMoveDelta, DT);
        Check(maxStep <= allowed + 1e-3f, "per-frame displacement clamped to MaxSpeed", maxStep.ToString("0.0000") + " <= " + allowed.ToString("0.0000"));

        // ---------------- 7. framerate independence -----------------------
        // Same wall-clock elapsed at 60fps vs 144fps should land in the same place.
        float at60 = Run(1f / 60f, 0.25f, tune);
        float at144 = Run(1f / 144f, 0.25f, tune);
        Check(Math.Abs(at60 - at144) < 0.15f, "60fps vs 144fps land within 0.15 units after 0.25s",
              at60.ToString("0.000") + " vs " + at144.ToString("0.000"));

        // ---------------- 8. juice decays ---------------------------------
        var m4 = new Moveable(new Transform2D(0, 0, 2.05f, 2.75f));
        t = 0f;
        m4.Tick(new MotionFrame(DT, t, tune), 20f);
        m4.JuiceUp(0.8f, 0.4f, t);
        Check(Math.Abs(m4.VT.Scale - (1f - 0.6f * 0.8f)) < 1e-4f, "juice pre-squashed the visual scale", m4.VT.Scale);

        float peak = 0f;
        for (int i = 0; i < 60; i++) { t += DT; m4.Tick(new MotionFrame(DT, t, tune), 20f); peak = Math.Max(peak, Math.Abs(m4.VT.R)); }
        Check(peak > 0.01f, "juice produced visible rotation", peak);
        Check(Math.Abs(m4.VT.R) < 0.01f && Math.Abs(m4.VT.Scale - 1f) < 0.02f, "juice settled back to rest after 1s",
              "r=" + m4.VT.R.ToString("0.0000") + " scale=" + m4.VT.Scale.ToString("0.000"));

        // ---------------- 9. hand layout ----------------------------------
        var cards = new List<Moveable>();
        for (int i = 0; i < 8; i++) cards.Add(new Moveable(new Transform2D(0, 0, 2.05f, 2.75f)));
        var hi = new List<bool>(new bool[8]);
        hi[3] = true;
        HandLayout.Apply(new Transform2D(0, 0, 20f, 3f), cards, hi, 2.05f, 8, 0f, reducedMotion: true);

        bool ascending = true;
        for (int i = 1; i < cards.Count; i++) if (cards[i].T.X <= cards[i - 1].T.X) ascending = false;
        Check(ascending, "hand lays out left to right");
        Check(cards[0].T.R < 0f && cards[7].T.R > 0f, "hand fans outward",
              cards[0].T.R.ToString("0.000") + " .. " + cards[7].T.R.ToString("0.000"));
        Check(cards[3].T.Y < cards[2].T.Y, "highlighted card offset (lower Y = up in a Y-down space)",
              cards[3].T.Y.ToString("0.000") + " vs " + cards[2].T.Y.ToString("0.000"));
        Check(Math.Abs(cards[0].T.X - 0f) < 0.01f, "first card sits at the left edge", cards[0].T.X);

        Console.WriteLine();
        Console.WriteLine(fails == 0 ? "ALL CHECKS PASSED" : fails + " CHECK(S) FAILED");
        Environment.Exit(fails == 0 ? 0 : 1);
    }
}
