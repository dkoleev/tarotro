
# Abyss — Game Design Document

### Tarot Roguelike · The Reading Edition

**Version:** 0.4 · **Engine:** Defold · **Platform:** HTML5 (Yandex Games, Poki) · **Secondary:** Steam

---

## Changelog: v0.3 → v0.4

|Area|Change|
|---|---|
|Slot system|**Past / Present / Future replaced** by Origin / Crossing / Outcome — same 3 positions, intuitive story logic|
|Card design|**Single identity per card** — one set of stats, one dramatic role. Position modifies _expression_, not _identity_|
|Position modifiers|Each position applies a **simple universal modifier** (Origin: +Wounds carryover; Crossing: base; Outcome: +Mult deferred)|
|Narrative Chains|**New core system** — Affinity chains and Tension pairs between adjacent cards create bonus effects and one-line story text|
|Reading display|**New UX element** — before every Pull, game shows a one-line narrative sentence for the placed reading|
|Chain table|**Full Affinity and Tension chain table** for all 22 arcana, with flavor narrative and mechanical bonus|
|Slots 4–5|Renamed **Echo** positions — the story reverberates beyond its ending|
|Everything else|Rings, Symbols, Hold, Curses, Zodiac retained from v0.3 unchanged|

---

## 1. Vision & Pillars

### Elevator Pitch

A deckbuilding roguelike where you descend through Dante's nine circles of Hell. Each turn you place three Major Arcana cards into three story positions — **Origin, Crossing, Outcome** — and the cards _talk to each other_. Adjacent cards with narrative affinity form chains. Opposing cards create tension. The story you tell with three cards determines how hard fate strikes.

Then the slot machine pulls. Fate adds its word to your sentence.

### Design Pillars

**1. Every card is a myth.** The 22 Major Arcana carry centuries of symbolic weight. The Tower collapses _something_. Death transforms. The Fool risks everything. Each card has one dramatic identity — the position reveals a different facet of that identity, but never contradicts it.

**2. The reading is the skill.** You don't optimize stats — you author a story. The Hermit followed by The Tower followed by Death means something. The game rewards that meaning with chain bonuses. The player who thinks in narrative finds the best lines.

**3. Rings are temptation.** Every Ring is powerful. Every Ring costs something. The game is about deciding how corrupted you're willing to become.

**4. Symbols are the chaos variable.** The reels spin and you cannot stop them. Your build determines what those symbols mean. A Skull is a counter-attack to a naked player and a buff to a Wrath build.

**5. Compression over expansion.** Exactly 22 Arcana. No more. The design must be maximally interesting within that closed set.

**6. Legible at a glance.** HTML5 audience, 480×854. Cards communicate in under one second. The narrative sentence is always visible before the Pull. No hidden math.

---

## 2. The Three-Layer System

Every turn, three layers fire in sequence and interact.

```
┌─────────────────────────────────────────────────────────────────┐
│  LAYER 1 — RINGS                                                │
│  Always active. Passive identity. Reshapes what the other       │
│  layers do. Equipped before and during the run.                 │
├─────────────────────────────────────────────────────────────────┤
│  LAYER 2 — ARCANA                                               │
│  Active each turn. Player places 3 cards into story positions.  │
│  Adjacent cards form Chains. The Reading is the decision.       │
├─────────────────────────────────────────────────────────────────┤
│  LAYER 3 — SYMBOLS                                              │
│  Fires once per turn on Pull. Slot machine reels add chaos.     │
│  Neutral raw material — Rings and Chains determine meaning.     │
└─────────────────────────────────────────────────────────────────┘
```

|Layer|When Active|Player Control|Randomness Source|
|---|---|---|---|
|**Rings**|Always|Chosen pre-run / found during run|Reward pool draw|
|**Arcana**|Each turn|Player places from hand, builds Chains|Draw order|
|**Symbols**|Each pull|Indirectly via build and Chain bonuses|Reel spin|

---

## 3. Layer 2 — The Reading (Arcana Placement)

### The Three Story Positions

The player places exactly 3 cards (or 2–5 with unlocked slots) into ordered positions. These positions are **not time** — they are **narrative structure**.

```
┌───────────┐     ┌───────────┐     ┌───────────┐
│  ORIGIN   │ ──► │ CROSSING  │ ──► │  OUTCOME  │
│           │     │           │     │           │
│ The wound │     │ The act   │     │ What it   │
│ the cause │     │ the force │     │ becomes   │
└───────────┘     └───────────┘     └───────────┘
    Card A    ◄── Chain? ──►    Card B    ◄── Chain? ──►    Card C
```

Cards resolve left to right. Each adjacent pair (A→B, B→C) is checked for **Affinity** or **Tension**. These relationships are the core of v0.4.

### Position Modifiers (Universal, Simple)

Every card has one set of stats. The position wraps those stats with a **single universal modifier**:

| Position             | Modifier                                | What it means                                      |
| -------------------- | --------------------------------------- | -------------------------------------------------- |
| **Origin**           | +50% Wounds, Wounds carry into Crossing | The source is powerful but feeds the action        |
| **Crossing**         | Base stats, no modifier                 | The act itself — reliable, what the card promises  |
| **Outcome**          | +50% Mult, deferred +2 Wounds next hand | Consequences amplify and linger                    |
| **Echo** (slots 4–5) | ×2 to Chain bonus only, no base stats   | The story reverberates — Chains alone echo forward |

**Teaching this in one sentence:** _"Origin hits harder. Crossing is what the card says. Outcome multiplies more and lingers."_

That's the whole position system. It is learnable in 30 seconds.

### The Reading — Narrative Sentence

When all cards are placed and before the Pull, the game displays a **one-line narrative sentence** in the center of the screen:

```
┌──────────────────────────────────────────────────────────┐
│   THE HERMIT → THE TOWER → DEATH                         │
│                                                          │
│   "A long solitude. Then everything falls. Then change." │
│                                                          │
│   Chain: The Seeker's Fall   ×2 Mult bonus               │
└──────────────────────────────────────────────────────────┘
```

This sentence is **always shown**, even when there is no Chain bonus. Three random cards still produce a reading. This makes every hand feel authored, not mechanical.

The player reads the sentence. Then pulls.

### Hand Flow

```
1. Draw 5 cards (base). Ring of Gluttony: 7. Ring of Lust: +1 per placed card.

2. Place cards into positions:
   — Origin first, then Crossing, then Outcome
   — As each card is placed, the game shows forming chain indicators

3. Narrative sentence assembles in real time as cards are placed

4. For each remaining unplaced card, choose:
   HOLD  — stays for next turn (blocks 1 draw next turn)
   DISCARD — goes to discard pile

5. Pull (Layer 3 fires)

6. Resolution: Origin → Crossing → Outcome → Chain bonuses → Symbols → Rings

7. Score stored. Outcome deferred bonus stored for next hand.
```

### Scoring Pipeline

```lua
-- slot_resolver.lua

function SlotResolver.resolve_hand(placed_cards, game_state)
    -- Apply deferred from previous Outcome
    local wounds = game_state.deferred_bonus.wounds
    local mult   = game_state.deferred_bonus.mult

    -- Resolve each position with its universal modifier
    local results = {}
    for i, entry in ipairs(placed_cards) do
        local pos  = entry.position  -- "origin", "crossing", "outcome", "echo"
        local card = entry.card
        local base = CardStats[card.id]  -- single stat block per card

        local result = {
            wounds = base.wounds,
            mult   = base.mult,
        }

        -- Apply universal position modifier
        if pos == "origin" then
            result.wounds = result.wounds * 1.5
            game_state.carryover_wounds = result.wounds * 0.3  -- fed into Crossing
        elseif pos == "crossing" then
            result.wounds = result.wounds + (game_state.carryover_wounds or 0)
        elseif pos == "outcome" then
            result.mult   = result.mult * 1.5
            result.deferred = { wounds = 2, mult = 1 }
        elseif pos == "echo" then
            result.wounds = 0
            result.mult   = 1
            -- Echo only amplifies chain bonuses — handled in chain resolver
        end

        results[i] = result
        wounds = wounds + result.wounds
        mult   = mult   * result.mult
    end

    -- Resolve Chains (pairs: 1→2, 2→3, 3→4, 4→5)
    local chain_bonus = ChainResolver.resolve(placed_cards, game_state)
    wounds = wounds + chain_bonus.wounds
    mult   = mult   * chain_bonus.mult

    -- Symbol bonuses (after Arcana and Chains, before Rings)
    local sym_bonus = SymbolResolver.resolve(game_state.this_turn_symbols, game_state)
    wounds = wounds + sym_bonus.wounds
    mult   = mult   * sym_bonus.mult

    -- Rings
    local score = wounds * mult
    for _, ring in ipairs(game_state.active_rings) do
        score = ring.apply(score, game_state)
    end

    -- Store deferred from Outcome
    for _, r in ipairs(results) do
        if r.deferred then
            game_state.deferred_bonus = r.deferred
            break
        end
    end

    game_state.cumulative_score = game_state.cumulative_score + score
    return score
end
```

---

## 4. The Narrative Chain System

This is the heart of v0.4. When two adjacent cards share a **narrative relationship**, they form a Chain. Chains produce bonus effects and generate the one-line reading text.

There are two types of chains:

**Affinity** — cards that flow into each other naturally. The story builds. Bonus: positive, amplifying.

**Tension** — cards that oppose each other. The story grinds. Bonus: different in nature — often converts conflict into power in an unexpected way.

A three-card reading can have 0, 1, or 2 chains (pair A→B and pair B→C are checked independently). A reading with 2 chains fires both bonuses. When slots 4–5 are unlocked, the Echo position doubles the last chain bonus it touches.

### Chain Resolver (Lua)

```lua
-- chain_resolver.lua
local ChainResolver = {}

function ChainResolver.resolve(placed_cards, game_state)
    local total = { wounds = 0, mult = 1 }
    local reading_parts = {}

    for i = 1, #placed_cards - 1 do
        local a = placed_cards[i].card.id
        local b = placed_cards[i + 1].card.id
        local chain = ChainResolver.find(a, b)

        if chain then
            -- Echo position doubles the chain bonus it touches
            local echo_mult = 1
            if placed_cards[i].position == "echo" or
               placed_cards[i+1].position == "echo" then
                echo_mult = 2
            end

            total.wounds = total.wounds + (chain.wounds or 0) * echo_mult
            total.mult   = total.mult   * ((chain.mult or 1) ^ echo_mult)
            table.insert(reading_parts, chain.narrative)

            -- Fire chain side effect
            if chain.effect then
                chain.effect(game_state)
            end
        end
    end

    game_state.reading_sentence = table.concat(reading_parts, " ")
    return total
end

function ChainResolver.find(a, b)
    -- Check both directions — some chains are directional, some are not
    return CHAINS[a .. "+" .. b] or CHAINS[b .. "+" .. a]
end

return ChainResolver
```

---

## 5. Arcana — Card Stats & Dramatic Identities

Each card now has **one stat block** and one **dramatic identity**. The position reveals a facet of that identity — it does not replace it.

|Card|Wounds|Mult|Dramatic Identity|
|---|---|---|---|
|**0 · The Fool**|2|×2|The innocent who risks everything|
|**I · The Magician**|3|×1|The transformer, the channel of will|
|**II · The High Priestess**|1|×3|The keeper of hidden knowledge|
|**III · The Empress**|2|×2|Abundance that multiplies what it touches|
|**IV · The Emperor**|5|×1|Order enforced without mercy|
|**V · The Hierophant**|3|×1|Tradition as both shield and cage|
|**VI · The Lovers**|2|×2|The choice that defines everything after|
|**VII · The Chariot**|4|×1|Victory through relentless motion|
|**VIII · Strength**|3|×2|Mastery earned through suffering|
|**IX · The Hermit**|0|×4|Solitude as the source of deep power|
|**X · Wheel of Fortune**|Rnd 0–6|Rnd ×1–×3|Fate without favor or malice|
|**XI · Justice**|3|×1|Exact measure, no more and no less|
|**XII · The Hanged Man**|0|×4|Sacrifice as investment, not loss|
|**XIII · Death**|0|×5|Not an ending — a necessary transition|
|**XIV · Temperance**|1|×2|Balance restored, excess contained|
|**XV · The Devil**|4|×2|The chain that feels like a gift|
|**XVI · The Tower**|7|×1|The catastrophic truth that cannot be avoided|
|**XVII · The Star**|3|×1|Hope that persists even in the pit|
|**XVIII · The Moon**|1|×3|Illusion — what is seen is never what is there|
|**XIX · The Sun**|5|×1|Clarity that burns away everything false|
|**XX · Judgement**|6|×1|The final accounting, the ledger opened|
|**XXI · The World**|4|×3|Completion — the end that is also a beginning|

### Position Expression (Per Card)

Instead of three separate behaviors, each card has **one passive effect** that expresses differently depending on position:

|Position|Effect timing|Expression|
|---|---|---|
|**Origin**|Before resolution|Setup, accumulation, protection|
|**Crossing**|During resolution|Direct action, the card's core power|
|**Outcome**|After resolution|Consequence, transformation, next-hand setup|

Each card section below shows: stats, passive (always active), and position expression notes.

---

### 0 · The Fool

_"He who knows nothing fears nothing."_ **Wounds:** 2 · **Mult:** ×2

**Passive:** Once per run, when your score falls short by 10% or less, The Fool adds exactly enough Wounds to meet the target. Burns after triggering.

**Origin:** Wounds doubled if this is not your first encounter. The Fool _remembers_ surviving. **Crossing:** If this is the first hand of the encounter, Mult becomes ×4. Beginner's luck. **Outcome:** Deferred 10 Wounds banked for the next encounter start. Sacrifice now, survive later.

---

### I · The Magician

_"As above, so below."_ **Wounds:** 3 · **Mult:** ×1

**Passive:** While in your deck, all Ring effects trigger before score calculation.

**Origin:** Passes the highest Wound value from your discard pile as bonus Wounds into Crossing. **Crossing:** Copies the Wounds of the highest-Wound card among unplaced cards in hand. **Outcome:** Deferred: next hand guarantees the highest-Mult card in your deck is drawn.

---

### II · The High Priestess

_"She does not speak. She remembers."_ **Wounds:** 1 · **Mult:** ×3

**Passive:** Never reduced by Circle Mult penalties.

**Origin:** Reveal top 3 deck cards. Add chosen card's Mult to Crossing's running Mult. **Crossing:** Reveal top 3 deck cards. Draw 1 immediately, return the rest in any order. **Outcome:** Deferred: next hand you see all drawn cards before assigning any to positions.

---

### III · The Empress

_"What is given, multiplies."_ **Wounds:** 2 · **Mult:** ×2

**Passive:** If 2+ copies in deck, both gain +1 Mult permanently (once per run).

**Origin:** +1 Wounds per hand played this encounter before this one. Patience rewarded. **Crossing:** +1 Wounds for each other card placed in any position this hand. **Outcome:** Deferred: +1 Wounds per card in discard pile next hand.

---

### IV · The Emperor

_"Order, or nothing."_ **Wounds:** 5 · **Mult:** ×1

**Passive:** If drawn in your opening hand, first enemy debuff this encounter is negated.

**Origin:** Wounds this hand cannot be reduced by enemy Curses or Circle modifiers. **Crossing:** Wounds AND Mult this hand cannot be reduced by any effect. **Outcome:** Deferred: next encounter's first enemy debuff is negated entirely.

---

### V · The Hierophant

_"Tradition is the weight of the dead."_ **Wounds:** 3 · **Mult:** ×1

**Passive:** Ring inscriptions cost –1 gold while in deck.

**Origin:** If you placed a card in Origin last hand, +5 Wounds and +×1 Mult. **Crossing:** If you used the same 3 cards last hand, +5 Wounds and +×1 Mult. **Outcome:** Deferred: all Ring inscriptions cost –1 gold in next Shop visit.

---

### VI · The Lovers

_"The wound that never closes."_ **Wounds:** 2 · **Mult:** ×2

**Passive:** After any encounter where you lost HP, gains +1 base Wounds permanently (max +5).

**Origin:** +1 Wounds per HP lost since run start (max +10). Past wounds made manifest. **Crossing:** Choose: sacrifice 5 HP → ×3 Mult, or gain nothing and keep HP. **Outcome:** Deferred: if you lose HP this encounter, ×2 Mult bonus next hand.

---

### VII · The Chariot

_"Victory is not arrival. It is motion."_ **Wounds:** 4 · **Mult:** ×1

**Passive:** +1 discard per encounter while in deck.

**Origin:** Wounds carried into Crossing are applied twice. Motion carries forward. **Crossing:** Wounds applied twice this hand (score as if doubled; Mult does not double). **Outcome:** Deferred: +1 hold slot next hand (hold one extra card without losing a draw).

---

### VIII · Strength

_"The beast bows when the hand is steady."_ **Wounds:** 3 · **Mult:** ×2

**Passive:** Cannot be discarded after placement (cannot be among set-aside cards).

**Origin:** If you took damage in any previous encounter this run, ×3 Mult instead. **Crossing:** If you took damage this encounter, ×3 Mult instead. **Outcome:** Deferred: if no damage taken next hand, retroactive +1 Mult applied.

---

### IX · The Hermit

_"The lamp illuminates only the next step."_ **Wounds:** 0 · **Mult:** ×4

**Passive:** Top 2 deck cards always visible while in deck.

**Origin:** +×1 Mult per hand played this encounter before this one. **Crossing:** Draw 2 additional cards for next hand's pool immediately. **Outcome:** Deferred: next hand draw 7 cards instead of 5.

---

### X · Wheel of Fortune

_"It turns whether you hold it or not."_ **Wounds:** Random 0–6 · **Mult:** Random ×1–×3

**Passive:** Once per encounter, may spin: discard hand, draw 5 new cards.

**Origin:** If any Ring produced a bonus last hand, all randomness resolves to maximum. **Crossing:** If any Ring produced a bonus this hand, all randomness resolves to maximum. **Outcome:** Deferred: may reroll entire draw once before assigning next hand.

---

### XI · Justice

_"Exact measure, no more, no less."_ **Wounds:** 3 · **Mult:** ×1

**Passive:** After each encounter, gold rounded up to nearest 5.

**Origin:** If cumulative score this encounter divisible by 10, ×3 Mult. **Crossing:** If current score before this hand divisible by 10, ×2 Mult. **Outcome:** Deferred: gold rounded up to nearest 5 after next encounter regardless.

---

### XII · The Hanged Man

_"Sacrifice is not loss. It is investment."_ **Wounds:** 0 · **Mult:** ×4

**Passive:** If discarded without placing (not in any position), +2 gold.

**Origin:** Last hand's Outcome deferred bonus is doubled before it expires. **Crossing:** Cannot play another hand this encounter — this is your final hand. **Outcome:** No deferred bonus stored. Future surrendered. Gain ×5 Mult now instead.

---

### XIII · Death

_"Not an ending. A transition."_ **Wounds:** 0 · **Mult:** ×5

**Passive:** Cannot be discarded, Burned, or Inscribed. Death is immutable.

**Origin:** Return the last permanently removed card this run to your deck. **Crossing:** After scoring, permanently remove the lowest-Wound card from your deck. **Outcome:** Deferred: next hand automatically contains the highest-Mult card in your deck.

---

### XIV · Temperance

_"Pour slowly, or spill all."_ **Wounds:** 1 · **Mult:** ×2

**Passive:** None.

**Origin:** Restore 3 HP. Next position's Mult cannot be reduced below ×1 this hand. **Crossing:** Restore 3 HP. If any card this hand has Wounds ≥ 5, Mult becomes ×1. **Outcome:** Deferred: restore 2 HP at start of next hand.

---

### XV · The Devil

_"The chain is the gift he gives."_ **Wounds:** 4 · **Mult:** ×2

**Passive:** Each Ring purchased in Shop permanently adds +1 base Wounds (max +8).

**Origin:** +1 Wounds per equipped Ring. The Devil counts his debts. **Crossing:** At start of next encounter, lose 1 Ring slot until you win. **Outcome:** Deferred: all Ring bonuses doubled next hand — lose 1 HP per active Ring.

---

### XVI · The Tower

_"What falls, was never truly standing."_ **Wounds:** 7 · **Mult:** ×1

**Passive:** After resolving in any position, next drawn card is guaranteed high-Mult (Hanged Man, Death, Moon, or World — if any in deck).

**Origin:** Wounds equal to twice the Wounds of the last card in Origin position. Charge builds. **Crossing:** Destroy one of your own Rings (your choice). If no Rings: +4 additional Wounds. **Outcome:** Deferred: schedule Ring destruction — fires after next hand, that hand gains +6 Wounds.

---

### XVII · The Star

_"Even in the pit, something light remains."_ **Wounds:** 3 · **Mult:** ×1

**Passive:** Minimum score per hand is 5 while in deck.

**Origin:** Restore 2 HP. Score floor this hand raised to 15. **Crossing:** Restore 2 HP. Draw 1 card for next hand. **Outcome:** Deferred: next hand's minimum score is 10.

---

### XVIII · The Moon

_"What you see is not what is there."_ **Wounds:** 1 · **Mult:** ×3

**Passive:** Copies in deck are invisible until drawn.

**Origin:** Enemy passive ability hidden until next encounter start. **Crossing:** All enemy passives hidden until next encounter. **Outcome:** Deferred: next hand your own position assignments hidden until all placed.

---

### XIX · The Sun

_"Clarity burns away the false."_ **Wounds:** 5 · **Mult:** ×1

**Passive:** Cannot have negative effects applied by Inscriptions or enemies.

**Origin:** Remove all negative status effects from this run. **Crossing:** Remove all negative effects. Reveal all hidden information. **Outcome:** Deferred: all hidden information revealed before next hand's assignments.

---

### XX · Judgement

_"The ledger is opened. Nothing is hidden."_ **Wounds:** 6 · **Mult:** ×1

**Passive:** At end of each Circle, reveal Boss ability before entering.

**Origin:** Score from all previous hands this encounter recalculated at ×1.1. Difference added now. **Crossing:** Score calculated and displayed before hand resolves. You see the number before committing. **Outcome:** Deferred: before next encounter, reveal Boss full ability and score target.

---

### XXI · The World

_"The end is also the beginning."_ **Wounds:** 4 · **Mult:** ×3

**Passive:** If placed last in Outcome (final hand of encounter), next encounter score target –5% additionally.

**Origin:** Shuffle entire discard pile back into deck before this hand resolves. **Crossing:** Shuffle entire discard pile back into deck immediately after scoring. **Outcome:** Deferred: next encounter's score target –10%.

---

## 6. Narrative Chain Table

All 22 Arcana can form chains with adjacent cards. Each chain entry contains:

- **Type** — Affinity or Tension
- **Narrative** — the one-line reading text shown to the player
- **Wounds bonus** — flat Wounds added this hand
- **Mult bonus** — Mult multiplier added this hand
- **Effect** — optional side effect

Chains are **directional** only when marked _(ordered)_. Otherwise, either card may appear first.

---

### Affinity Chains

---

**The Fool + The Hermit** _"The innocent walks into the dark alone."_ Type: Affinity · Wounds +3 · Mult ×1 Effect: If The Hermit is in Origin, Mult becomes ×6 instead of ×4.

**The Fool + The World** _"The journey that started with nothing ends with everything."_ Type: Affinity · Wounds +0 · Mult ×2 Effect: Shuffle 2 cards from discard into deck immediately.

**The Fool + The Star** _"Hope and recklessness are the same thing, seen from different heights."_ Type: Affinity · Wounds +2 · Mult ×1 Effect: Restore 2 HP.

**The Magician + The High Priestess** _"The will to act meets the wisdom to wait."_ Type: Affinity · Wounds +0 · Mult ×2 Effect: Reveal top 2 deck cards. Swap them or leave them.

**The Magician + The Empress** _"The channel and the source. What flows, multiplies."_ Type: Affinity · Wounds +4 · Mult ×1 Effect: None.

**The Magician + The World** _(ordered: Magician must precede World)_ _"The act of will brings the cycle to completion."_ Type: Affinity · Wounds +0 · Mult ×3 Effect: All Rings fire an additional time this hand.

**The High Priestess + The Moon** _"She who remembers. She who obscures. Two faces of the same secret."_ Type: Affinity · Wounds +0 · Mult ×2 Effect: Enemy passive hidden for this fight AND next fight.

**The High Priestess + The Hermit** _"Knowledge withdrawn from the world. Power that does not announce itself."_ Type: Affinity · Wounds +0 · Mult ×3 Effect: Top 3 deck cards revealed and reordered freely.

**The Empress + The Lovers** _"Abundance given to the wrong thing. Or the right thing. Only time will say."_ Type: Affinity · Wounds +3 · Mult ×1 Effect: If HP lost since run start ≥ 10, Wounds bonus becomes +8.

**The Empress + The World** _"What was nurtured grows until it fills everything."_ Type: Affinity · Wounds +0 · Mult ×2 Effect: +1 Wounds to every card in hand permanently for this encounter.

**The Emperor + The Chariot** _"The decree. The march. There is no room for hesitation."_ Type: Affinity · Wounds +5 · Mult ×1 Effect: Wounds this hand cannot be reduced by any effect (Emperor's protection extends).

**The Emperor + The Hierophant** _"Law and tradition. The weight that holds the world in place."_ Type: Affinity · Wounds +3 · Mult ×1 Effect: First enemy debuff next encounter is also negated (stacks with Emperor's passive).

**The Hierophant + The Emperor** _(see above — same chain, non-directional)_

**The Hierophant + Justice** _"Tradition weighed against the exact measure. Both demand the same thing."_ Type: Affinity · Wounds +0 · Mult ×2 Effect: If cumulative score divisible by 10, Mult becomes ×3 for entire hand.

**The Lovers + The Devil** _(ordered: Lovers must precede Devil)_ _"The choice made. The chain accepted."_ Type: Affinity · Wounds +4 · Mult ×1 Effect: +2 Wounds per equipped Ring this hand (the debt acknowledged).

**The Lovers + Strength** _(ordered: Lovers must precede Strength)_ _"The wound chosen becomes the source of power."_ Type: Affinity · Wounds +0 · Mult ×2 Effect: ×3 Mult if HP was lost choosing The Lovers Crossing sacrifice.

**The Chariot + The Tower** _(ordered: Chariot must precede Tower)_ _"Unstoppable motion. Then the wall. Then silence."_ Type: Affinity · Wounds +6 · Mult ×1 Effect: Tower's Crossing Ring destruction also grants +10 Wounds (Ring of the Tower not required).

**Strength + The Hermit** _"The beast mastered. The lamp raised. Earned solitude."_ Type: Affinity · Wounds +0 · Mult ×3 Effect: Hermit's Mult gains +1 for each encounter where damage was taken this run.

**Strength + The Star** _"Even battered, something persists."_ Type: Affinity · Wounds +2 · Mult ×2 Effect: Restore 3 HP.

**The Hermit + The Moon** _"The one who retreated. The light that obscures rather than reveals."_ Type: Affinity · Wounds +0 · Mult ×3 Effect: Enemy passive hidden AND Hermit's Mult increases by +1 for each remaining hand this encounter.

**The Hermit + The Hanged Man** _"Two kinds of waiting. One prepares. One surrenders."_ Type: Affinity · Wounds +0 · Mult ×4 Effect: Hanged Man's deferred from last hand doubled again (triple the original deferred).

**The Wheel of Fortune + The Fool** _"The gambler and the innocent. Neither knows what comes next."_ Type: Affinity · Wounds +0 · Mult ×1 Effect: All random values resolve to maximum this hand.

**The Wheel of Fortune + The World** _"The turn of fate brings the cycle to completion."_ Type: Affinity · Wounds +0 · Mult ×2 Effect: All random values resolve to maximum. Shuffle discard into deck.

**Justice + Judgement** _"The measure. Then the reckoning. The numbers agree."_ Type: Affinity · Wounds +0 · Mult ×2 Effect: If cumulative score divisible by 10, entire hand scored twice.

**Justice + Temperance** _"Balance through precision. The exact right amount."_ Type: Affinity · Wounds +0 · Mult ×2 Effect: Restore 2 HP. Mult cannot be reduced below ×2 this hand.

**The Hanged Man + Death** _(ordered: Hanged Man must precede Death)_ _"The surrender. Then the transformation it made possible."_ Type: Affinity · Wounds +0 · Mult ×5 Effect: Card removed by Death's Crossing is returned to deck at run end.

**The Hanged Man + The World** _(ordered: Hanged Man must precede World)_ _"What was given up becomes the world itself."_ Type: Affinity · Wounds +0 · Mult ×4 Effect: Score target for next encounter reduced by 15%.

**Death + The Fool** _(ordered: Death must precede Fool)_ _"An ending. Then an innocent beginning."_ Type: Affinity · Wounds +2 · Mult ×2 Effect: The Fool's passive (score shortfall rescue) resets and can fire again this run.

**Death + The World** _(ordered: Death must precede World)_ _"The ending that contains all endings. And then — completion."_ Type: Affinity · Wounds +0 · Mult ×5 Effect: Hand scored twice.

**Temperance + The Star** _"Restraint and hope. The two things the pit cannot take."_ Type: Affinity · Wounds +3 · Mult ×1 Effect: Restore 4 HP total. Score floor raised to 20.

**The Devil + The Tower** _(ordered: Devil must precede Tower)_ _"The chain pulled too tight. Everything collapses."_ Type: Affinity · Wounds +5 · Mult ×1 Effect: Tower's Ring destruction grants +15 Wounds instead of base.

**The Devil + The Lovers** _(ordered: Devil must precede Lovers)_ _"The bondage offered as a gift. The choice, too late."_ Type: Affinity · Wounds +3 · Mult ×2 Effect: None — the numbers carry the weight.

**The Tower + Death** _(ordered: Tower must precede Death)_ _"The collapse was necessary. Now transformation is possible."_ Type: Affinity · Wounds +4 · Mult ×2 Effect: Card removed by Death's Crossing may be returned to hand immediately.

**The Star + The Sun** _"The small light. Then the great one. Darkness retreating."_ Type: Affinity · Wounds +4 · Mult ×1 Effect: Remove all negative effects. Restore 3 HP.

**The Star + The World** _"Hope sustained until the journey ends."_ Type: Affinity · Wounds +3 · Mult ×2 Effect: Score floor raised to 25 this hand.

**The Moon + The High Priestess** _"See above — same chain, non-directional."_

**The Sun + Judgement** _"Clarity. Then the reckoning in full light. Nothing hidden, nothing spared."_ Type: Affinity · Wounds +4 · Mult ×1 Effect: All previous hands this encounter recalculated at ×1.2.

**Judgement + The World** _(ordered: Judgement must precede World)_ _"The ledger opened. Then the cycle completed. The sentence passed."_ Type: Affinity · Wounds +0 · Mult ×3 Effect: Score target this encounter reduced by 10%. Next encounter Boss ability revealed.

---

### Tension Chains

Tension chains fire when adjacent cards **oppose** each other thematically. The friction generates power — but of a stranger kind.

---

**The Fool + Justice** _"The one who knows nothing. The one who measures everything. Neither wins."_ Type: Tension · Wounds +0 · Mult ×1 Effect: All random values this hand resolve to exactly median value. No maximum, no minimum. Perfect middle.

**The Fool + The Emperor** _"Innocence and absolute order cannot share the same breath."_ Type: Tension · Wounds +6 · Mult ×1 Effect: Wounds cannot be reduced this hand. The Emperor protects the Fool's chaos.

**The Magician + The Hanged Man** _"The force of will. The surrender of will. They cancel — and what remains is pure."_ Type: Tension · Wounds +0 · Mult ×4 Effect: Both cards' Mult values added together instead of multiplied. (Magician ×1 + Hanged Man ×4 = ×5 flat, not ×4.)

**The High Priestess + The Sun** _"The hidden and the revealed. One must yield."_ Type: Tension · Wounds +0 · Mult ×2 Effect: All hidden information revealed immediately. High Priestess loses Mult this hand (×1), Sun gains +3 Wounds.

**The Emperor + The Fool** _(see above — same chain)_

**The Hierophant + The Tower** _"Tradition meets the thing that destroys traditions. No contest."_ Type: Tension · Wounds +8 · Mult ×1 Effect: Tower's Wounds doubled this hand. Hierophant's Mult lost (×1). The old structure fell.

**The Lovers + Justice** _"The choice that defies measure. Justice cannot weigh what the heart decides."_ Type: Tension · Wounds +0 · Mult ×2 Effect: If HP sacrifice was made this hand (Lovers Crossing), ×4 Mult instead.

**The Chariot + The Hermit** _"Motion and stillness. The warrior and the one who stopped."_ Type: Tension · Wounds +3 · Mult ×2 Effect: Chariot's Wounds halved, Hermit's Mult doubled. Speed traded for depth.

**Strength + The Hanged Man** _"Mastery of force. Surrender of force. Which is greater?"_ Type: Tension · Wounds +0 · Mult ×3 Effect: Whichever card has lower base Wounds borrows the other's Wounds value this hand.

**The Hermit + The Sun** _"The one who hid from the light meets the light itself."_ Type: Tension · Wounds +3 · Mult ×1 Effect: Hermit loses accumulated Mult bonus (resets to base ×4). Sun gains +4 Wounds. The solitude is broken.

**The Wheel of Fortune + Justice** _"The paradox. Fate and measure. Neither can coexist."_ Type: Tension · Wounds +0 · Mult ×1 Effect: All randomness resolves to exact median. Gold rounded up to nearest 10. The universe compromises.

**The Wheel of Fortune + The Emperor** _"Chaos and order. The wheel stops — once."_ Type: Tension · Wounds +0 · Mult ×2 Effect: This pull's Symbols are chosen by the player rather than randomly spun. Order overrides fate.

**Justice + The Devil** _"Exact measure against the one who corrupts all scales."_ Type: Tension · Wounds +0 · Mult ×1 Effect: +1 Wounds per Ring equipped. Mult becomes ×(number of Rings). Justice uses the Devil's own weights.

**The Hanged Man + The Chariot** _(ordered: Hanged Man must precede Chariot)_ _"Surrender followed by motion. The still point before the charge."_ Type: Tension · Wounds +4 · Mult ×2 Effect: Chariot's Wounds applied three times instead of twice. The wait made the motion stronger.

**Death + Temperance** _(ordered: Death must precede Temperance)_ _"The ending. Then — balance. Measured grief."_ Type: Tension · Wounds +0 · Mult ×2 Effect: Restore 4 HP. Removed card (Death Crossing) is held in limbo — returned to deck at next Rest.

**Death + The Star** _(ordered: Death must precede Star)_ _"Everything ends. And then — a small light persists anyway."_ Type: Tension · Wounds +0 · Mult ×3 Effect: Restore 3 HP. Score floor raised to 20.

**The Devil + Temperance** _"The chain and the balance. They have never agreed."_ Type: Tension · Wounds +0 · Mult ×1 Effect: Temperance's HP restore triples. Devil's Wounds halved. The excess is contained — at a cost.

**The Devil + The Star** _"The corruptor and the hope. The star does not go out."_ Type: Tension · Wounds +3 · Mult ×2 Effect: Lose 2 HP. Restore 5 HP. The Devil takes — the Star gives more.

**The Tower + The Star** _"The collapse. Then — something small, surviving."_ Type: Tension · Wounds +4 · Mult ×1 Effect: After Tower's Ring destruction, restore 3 HP. The Star catches what falls.

**The Tower + Temperance** _"The catastrophe. Then — measured response. Too late for measure."_ Type: Tension · Wounds +5 · Mult ×1 Effect: HP restoration from Temperance doubled. The collapse demanded more healing.

**The Moon + The Sun** _"The great opposition. Illusion and clarity. Only one survives."_ Type: Tension · Wounds +0 · Mult ×1 Effect: Moon's Mult (×3) and Sun's Wounds (5) are swapped this hand. The opposition inverts both. Combined: Moon deals 5 Wounds, Sun multiplies ×3. Strange but powerful.

**The Moon + Justice** _"What cannot be seen cannot be measured. Justice is blinded."_ Type: Tension · Wounds +0 · Mult ×3 Effect: Enemy passive hidden until fight end. Gold rounded up to nearest 10.

**The Sun + The Moon** _(see above — same chain)_

**The Sun + The Devil** _"Clarity burns away the chains — or tries to."_ Type: Tension · Wounds +6 · Mult ×1 Effect: One Ring of the player's choice is temporarily freed this hand (its negative effects suspended, bonuses remain).

**Judgement + The Fool** _(ordered: Judgement must precede Fool)_ _"The reckoning comes for the one who claimed to know nothing."_ Type: Tension · Wounds +5 · Mult ×1 Effect: The Fool's passive (shortfall rescue) fires immediately — regardless of whether score is short.

**Judgement + The Moon** _(ordered: Judgement must precede Moon)_ _"The ledger opened. But the light was wrong."_ Type: Tension · Wounds +0 · Mult ×2 Effect: Score shown to player is false (inflated by 20%). Actual score correct — player must trust without seeing.

**The World + The Tower** _(ordered: World must precede Tower)_ _"Completion. And then — catastrophe at the height of everything."_ Type: Tension · Wounds +8 · Mult ×1 Effect: Tower destroys a Ring AND gains +12 Wounds. The World's completion made the fall more terrible.

**The World + Death** _(ordered: World must precede Death)_ _"The cycle complete. And then — it ends again. Deeper this time."_ Type: Tension · Wounds +0 · Mult ×4 Effect: Card removed by Death is guaranteed to be the card with highest Mult (not lowest Wounds). A different cost.

---

## 7. Three-Card Reading Examples

These examples show what a player sees when they place their reading. They illustrate how the narrative system communicates to the player organically.

---

**The Seeker's Fall** Origin: The Hermit · Crossing: The Tower · Outcome: Death

_Reading: "A long solitude. Then everything falls. Then change."_ Chain 1 (Hermit → Tower): No affinity chain exists between these cards — but individual effects fire. Chain 2 (Tower → Death): **Affinity** — "The collapse was necessary. Now transformation is possible." +4 Wounds, ×2 Mult. Card removed by Death returned to hand.

A common early-run discovery. The player realizes holding Death makes the Tower feel less like loss.

---

**The Blind Gambler** Origin: The Wheel of Fortune · Crossing: The Moon · Outcome: The Fool

_Reading: "Fate rolled without looking. The mist closed in. The innocent stepped forward anyway."_ Chain 1 (Wheel → Moon): No chain — but Moon's passive hides enemy info, creating the thematic "blind" state. Chain 2 (Moon → Fool): No chain — but Fool's passive is primed.

No mechanical chain bonus here. But the narrative sentence creates the feeling of a character. The player chose this story. It has weight even without a chain bonus.

---

**The Sentence** _(named combo from v0.3)_ Origin: Judgement · Crossing: Death · Outcome: The Tower

_Reading: "The ledger opened. The sentence passed. Everything collapsed."_ Chain 1 (Judgement → Death): **Affinity** — "The accounting finds its end." Mult ×3. Chain 2 (Death → Tower): No chain in this direction (Tower→Death has a chain, but Death→Tower does not).

Special Combo trigger: Judgement + Death + Tower in correct positions fires the **"The Sentence"** named combo: instant kill regardless of score.

---

**The Inversion** Origin: The Sun · Crossing: The Moon · Outcome: Justice

_Reading: "Clarity burned away the mist. What was revealed was measured."_ Chain 1 (Sun → Moon): **Tension** — Moon's Mult and Sun's Wounds swap. Strange output. Chain 2 (Moon → Justice): **Tension** — "What cannot be seen cannot be measured." ×3 Mult, enemy passive hidden, gold rounded up to 10.

Two tension chains in one hand. The player has stumbled into a bizarre, powerful combination. Neither affinity nor opposition — collision.

---

## 8. Named Combos (Positional, Retained from v0.3)

Named Combos are special conditions that fire **in addition to** Chain bonuses. They require specific cards in specific positions.

|Combo|Condition|Bonus|
|---|---|---|
|**The Sentence**|Judgement in Origin → Death in Crossing → Tower in Outcome|Instant kill — the judgment, the death, the collapse|
|**The Reckoning**|Judgement + Death + Tower, any positions, same encounter|Instant kill regardless of order|
|**Eternal Return**|The World in Outcome|Reshuffle discard before scoring — shuffled cards available next hand|
|**Limbo's Loop**|The Fool in any position, three encounters in a row|The Fool becomes permanent — cannot be discarded or Burned|
|**The Abyss**|The World in any position + Death in any position, same hand|Score the entire hand twice|
|**Vigil**|Origin + Crossing + Outcome all have 0 base Wounds (Hermit, Hanged Man, Death)|×4 Mult to entire hand|
|**Damnation**|Any 2 of 3 placed cards share the same Circle|×2 Mult for entire hand|
|**Prophecy**|Same card in Outcome position two hands in a row|That card's Outcome deferred bonus fires twice next use|
|**Hubris**|Combined base Wounds across all 3 positions ≥ 14|Score target –15% this encounter|
|**Temptation**|Devil + Lovers + Tower, any positions|+15 Wounds, destroy 1 Ring (player's choice)|
|**Wheel Spin**|Wheel of Fortune in any position + at least 1 Ring active|Reroll all random effects once, then resolve|
|**Skull Harvest**|Triple Skull Symbol + Ring of Wrath active|+15 Wounds (triple the Ring of Wrath conversion)|
|**The River Styx**|Same card in Origin position two hands in a row|That card gains permanent +1 Wounds across all positions|

---

## 9. The Zodiac — Slot Modifiers

Retained unchanged from v0.3. Zodiac signs now apply to **positions** rather than slots, but the mapping is identical: signs reference Origin (was Past), Crossing (was Present), or Outcome (was Future).

|Zodiac|Position|Effect|
|---|---|---|
|**Aries ♈**|Origin|All Wounds from Origin doubled. Carryover into Crossing lost.|
|**Taurus ♉**|Crossing|Mult +×1. If no HP lost this encounter, +×2.|
|**Gemini ♊**|Outcome|Deferred bonus applies to next two hands.|
|**Cancer ♋**|Origin|Carryover Wounds tripled. Origin Mult halved.|
|**Leo ♌**|Crossing|If Crossing card base Wounds ≥ 4, ×2 Mult entire hand.|
|**Virgo ♍**|Outcome|Deferred bonus cannot be lost, stolen, or reduced.|
|**Libra ♎**|Crossing|Crossing Wounds and Mult averaged with Origin values.|
|**Scorpio ♏**|Origin|Origin Wounds doubled. Cannot restore HP this encounter.|
|**Sagittarius ♐**|Outcome|Deferred bonus applies to this hand and the next.|
|**Capricorn ♑**|Crossing|Every 10 Wounds from Crossing this encounter grants +1 gold.|
|**Aquarius ♒**|Outcome|Deferred bonus tripled, expires if not claimed within 1 hand.|
|**Pisces ♓**|Origin|Origin card uses Crossing behavior instead.|

### Zodiac × Circle Season

|Circles|Season|Signs|
|---|---|---|
|1–3|Fire (Cardinal)|Aries, Leo, Sagittarius|
|4–6|Earth (Fixed)|Taurus, Virgo, Capricorn|
|7–8|Air (Mutable)|Gemini, Libra, Aquarius|
|9|Water (Transcendent)|Cancer, Scorpio, Pisces|

---

## 10. Layer 1 — Rings

Retained from v0.3. Full ring list unchanged. Rings apply after Chain resolution and after Symbol bonuses, in equipped-slot order.

### Sin-Themed Core Rings

**Ring of Wrath** _(Men)_ — Skull symbols add +5 Wounds instead of triggering counter-attack. **Ring of Greed** _(Men)_ — Ember symbols also grant 1 gold. **Ring of Gluttony** _(Men)_ — Draw 7 cards instead of 5. Reshuffles cost 3 HP. **Ring of Sloth** _(Men)_ — Origin fires at double strength. Outcome does not trigger. **Ring of Pride** _(Dwarves)_ — First Pull per fight always shows three identical Symbols. **Ring of Envy** _(Dwarves)_ — Copy the weakest active Enemy Curse as a buff. **Ring of Lust** _(Men)_ — After placing a card, draw 1 extra card for remaining placements.

### Additional Rings

**Ring of the Hermit** _(Dwarves)_ — Hermit's Mult becomes ×5 in Origin and Crossing, ×4 in Outcome. **Ring of the Tower** _(Dwarves)_ — Tower's Ring destruction grants +10 Wounds (Crossing) or +12 (Outcome deferred). **Ring of Inversion** _(Elves)_ — Swap all card Wounds and Mult values before resolution. **Ring of the Abyss** _(Elves)_ — Circle 8–9 cards: Mult doubled. All other cards: Wounds halved. **Ring of Seasons** _(Elves)_ — Zodiac modifier applies to two positions instead of one. **The One Ring** _(Legendary)_ — All Arcana Mult +3. Every 3 hands: –5 HP. At 0 HP from this: shatters, final hand ×10 Mult.

---

## 11. Layer 3 — Symbols (The Slot Machine)

Retained from v0.3. Six symbols, triple results, reel weight modification.

|Symbol|Base Effect|Example with Build|
|---|---|---|
|🔥 Ember|+6 flat Wounds|Ring of Greed: +1 gold also|
|✨ Glow|Crossing fires again|Ring of Sloth: Origin fires again instead|
|💀 Skull|Counter-attack|Ring of Wrath: +5 Wounds instead|
|⚖️ Scale|Origin and Outcome swap triggers|With Pisces: triple-swap|
|🌀 Void|Wounds convert to stacking enemy status|Devil in Crossing: stacks count as Ring count|
|👁️ Eye|Reveal one hidden enemy stat permanently|Moon in Outcome: also reveals hidden positions|

### Triples

|Triple|Effect|
|---|---|
|🔥🔥🔥|+18 Wounds|
|✨✨✨|All placed Arcana fire twice|
|💀💀💀|Enemy attacks twice — Ring of Wrath fires twice too|
|⚖️⚖️⚖️|All three positions rotate: Origin→Crossing→Outcome→Origin|
|🌀🌀🌀|Status converts to immediate Wounds equal to all accumulated stacks|
|👁️👁️👁️|Full enemy stat block revealed + full deck order until reshuffle|

---

## 12. Enemy Design — Curses

Retained from v0.3. Each enemy has one Curse revealed before the fight.

### Curse Examples

**Warping Layer 2 (Arcana/Chains):** _"The Silence"_ — Outcome position Arcana do not trigger. Deferred bonuses lost. _"Mirror Soul"_ — 20% of Wounds dealt reflected as HP damage. _"Chain Break"_ — No Affinity chains fire this fight. Tension chains still fire. _"Corrosion"_ — Each hand, one random Arcana loses its Mult contribution.

**Warping Layer 3 (Symbols):** _"Weighted Wheel"_ — Skull weight doubled. _"The Still Reel"_ — Glow symbol disabled. _"Hollow Fortune"_ — Ember deals 0 Wounds.

**Warping Layer 1 (Rings):** _"Envy's Grip"_ — Rings with Mult bonuses disabled. _"The Unbinding"_ — Rings apply in reverse order.

**Warping the rules:** _"Hungry Void"_ — Each turn without 30+ Wounds dealt, enemy heals 10. _"Patience"_ — Enemy does nothing for 3 turns, then one-shots. _"Reversal"_ — Score must land between 80 and 120. Overshoot: enemy unharmed. _"Narrative Break"_ — No Chain bonuses fire this fight. Cards resolve as individuals only.

---

## 13. Structure of a Run

### Nine Circles

|Circle|Name|Rooms|Boss|Modifier|Boss Zodiac|
|---|---|---|---|---|---|
|1|Limbo|3|Charon|All hands start with Wound floor 2|Pisces ♓|
|2|Lust|3|Minos|Outcome cards discarded randomly after play|Sagittarius ♐|
|3|Gluttony|4|Cerberus|Score target +25% per room|Leo ♌|
|4|Greed|4|Plutus|Rings cost 2× this act|Capricorn ♑|
|5|Anger|4|Phlegyas|All Mult halved, Wounds doubled|Aries ♈|
|6|Heresy|4|Farinata|Inscriptions cost HP instead of gold|Virgo ♍|
|7|Violence|5|Minotaur|Discard pile inaccessible|Scorpio ♏|
|8|Fraud|5|Geryon|Origin and Outcome positions visually swapped|Gemini ♊|
|9|Treachery|5|Lucifer|No Rings. No Inscriptions. No Zodiac. Chains only.|None|

### Starting Archetypes

**The Descent** _(Origin-focused, Ring-synergy)_ Fool, Magician, Emperor, Chariot, Tower, Strength, Devil

**The Vigil** _(Outcome-focused, Chain-patient)_ High Priestess, Hermit, Hanged Man, Moon, Temperance, Justice, Star

**The Judgment** _(Crossing-focused, Symbol-reactive)_ Empress, Hierophant, Lovers, Wheel of Fortune, Sun, Judgement, World

---

## 14. Inscriptions

Retained from v0.3 with one addition: **Resonant Chain** — new inscription unique to v0.4.

|Inscription|Effect|Cost|
|---|---|---|
|**Bleeding**|+3 base Wounds|4 gold|
|**Resonant**|+×0.5 base Mult|5 gold|
|**Chained**|Cannot be discarded; +2 Wounds in Crossing|3 gold|
|**Cursed**|+5 Wounds +×1 Mult; deals 1 HP in Crossing|2 gold|
|**Ethereal**|Vanishes after 3 placements|1 gold|
|**Mirrored**|Copies effect of previous card in same position last hand|8 gold|
|**Hollow**|Removes base Wounds; Mult = ×(Crossing Wounds ÷ 2)|6 gold|
|**Sealed**|Immune to enemy, Circle, Zodiac effects|7 gold|
|**Anchored**|This card's Origin carryover always fires, even from Crossing or Outcome|9 gold|
|**Temporal**|+×1 Mult per different position placed in this run|6 gold|
|**Weighted**|+1 weight to one chosen Symbol while in deck|5 gold|
|**Resonant Chain** _(new)_|This card's Chain bonuses are doubled when it is adjacent to any Chain partner|10 gold|

---

## 15. Economy

### Gold Sources

Combat victory: 2–4 gold · Elite victory: 5–7 gold · Omen events: 3–6 gold · Ring of Greed · Hanged Man discarded: +2 gold · Capricorn Zodiac · Justice passive

### Gold Sinks

New Ring: 6–12 gold · Inscription: 1–10 gold · Second card copy: 8 gold · Burn card: 3 gold · HP at Rest: 2 gold/HP · Reel Weight change: 5 gold

---

## 16. UX & Platform Notes

### HTML5 Constraints (480×854 primary)

**Reading display:** Center screen, between slot row and reel display. Always visible after all 3 cards placed. Shows: card names with arrows, narrative sentence, chain name and bonus (or blank if no chain). 2-second fade-in animation. This is the most important new UI element in v0.4.

**Chain indicators:** As player places cards, a subtle glowing thread appears between adjacent cards when they have Affinity. A cracked-stone visual appears for Tension. Player sees chains forming in real time during placement.

**Position labels:** Origin / Crossing / Outcome with visual frames. Tooltip on tap: "Origin — the cause. Hits harder, feeds the action." Three words. No more.

**Card info panel:** Tapping a card shows: stats, passive, and three position expressions (not three full behaviors — just a line each). Plus a "Chain partner" section listing known affinities and tensions for that card.

**Symbol display:** Three reel windows below the reading. Animate on Pull. Brief pause between each reel.

**Deferred display:** "Echo Bonus" counter above Outcome slot. Pulses when it applies next hand.

**Enemy Curse display:** Always visible in enemy panel. Tapping shows full description.

**Session length:** Full run 35–50 min · Single Circle 6–8 min · Save at every room exit.

### Monetization (Yandex Games)

Rewarded ad: Resurrect once per run at 10 HP · Reroll Shop Rings once · Reroll Reel Weights once per encounter. No pay-to-win.

---

## 17. Implementation Notes (Defold / Lua)

### Chain Data Structure

```lua
-- chains.lua
-- Each chain: key = "card_a+card_b" (alphabetical, unless directional)
-- Directional chains: key = "card_a>card_b"

local CHAINS = {
    -- Affinity
    ["fool+hermit"] = {
        type      = "affinity",
        narrative = "The innocent walks into the dark alone.",
        wounds    = 3,
        mult      = 1,
        effect    = function(state)
            -- If Hermit in Origin, boost its mult to 6
            if state.positions["origin"] and
               state.positions["origin"].id == "hermit" then
                state.positions["origin"].mult_override = 6
            end
        end
    },

    ["tower>death"] = {  -- directional: Tower must precede Death
        type      = "affinity",
        narrative = "The collapse was necessary. Now transformation is possible.",
        wounds    = 4,
        mult      = 2,
        effect    = function(state)
            -- Card removed by Death's Crossing returned to hand immediately
            state.death_return_to_hand = true
        end
    },

    ["sun+moon"] = {
        type      = "tension",
        narrative = "The great opposition. Illusion and clarity. Only one survives.",
        wounds    = 0,
        mult      = 1,
        effect    = function(state)
            -- Swap Moon's mult and Sun's wounds for this hand
            local moon = find_card_in_positions(state, "moon")
            local sun  = find_card_in_positions(state, "sun")
            if moon and sun then
                local tmp = moon.mult
                moon.mult = sun.wounds  -- moon now deals sun's wounds as mult
                sun.wounds = tmp        -- sun now deals moon's mult as wounds
            end
        end
    },

    -- ... all 50+ chains follow this pattern
}

return CHAINS
```

### Chain Resolver

```lua
-- chain_resolver.lua
local ChainResolver = {}
local CHAINS = require("cards.chains")

function ChainResolver.resolve(placed_cards, game_state)
    local total   = { wounds = 0, mult = 1 }
    local reading = {}

    for i = 1, #placed_cards - 1 do
        local a   = placed_cards[i].card.id
        local b   = placed_cards[i + 1].card.id

        -- Check directional first, then non-directional
        local key   = a .. ">" .. b
        local chain = CHAINS[key]
        if not chain then
            -- Try alphabetical non-directional
            local ids = { a, b }
            table.sort(ids)
            chain = CHAINS[ids[1] .. "+" .. ids[2]]
        end

        if chain then
            local echo_mult = 1
            if placed_cards[i].position   == "echo" or
               placed_cards[i+1].position == "echo" then
                echo_mult = 2
            end

            -- Resonant Chain inscription doubles bonus
            if placed_cards[i].inscriptions
               and placed_cards[i].inscriptions["resonant_chain"] then
                echo_mult = echo_mult * 2
            end

            total.wounds = total.wounds + (chain.wounds or 0) * echo_mult
            total.mult   = total.mult   * math.pow((chain.mult or 1), echo_mult)

            if chain.effect then chain.effect(game_state) end
            table.insert(reading, chain.narrative)
        end
    end

    game_state.reading_sentence = table.concat(reading, " ")
    return total
end

return ChainResolver
```

### Reading Sentence Builder

```lua
-- reading_display.lua
local ReadingDisplay = {}

-- Called in real-time as player places cards
function ReadingDisplay.update(placed_so_far, game_state)
    local names = {}
    for _, entry in ipairs(placed_so_far) do
        table.insert(names, entry.card.display_name)
    end

    local header   = table.concat(names, " → ")
    local chains   = ChainResolver.preview(placed_so_far)  -- no side effects
    local sentence = chains.sentence or ReadingDisplay.default_sentence(placed_so_far)
    local bonus    = chains.wounds > 0 or chains.mult > 1

    return {
        header   = header,
        sentence = sentence,
        chain    = chains.name or nil,
        bonus    = bonus and (
            (chains.wounds > 0 and "+" .. chains.wounds .. " Wounds" or "") ..
            (chains.mult   > 1 and " ×" .. chains.mult .. " Mult" or "")
        ) or nil,
    }
end

-- Fallback sentence when no chain exists
-- Every 3-card combo gets a generated sentence from card narratives
function ReadingDisplay.default_sentence(placed)
    local beats = {}
    for _, entry in ipairs(placed) do
        local pos   = entry.position
        local card  = entry.card
        -- Each card has a short_narrative per position
        local line  = card.position_narrative[pos] or card.short_narrative
        table.insert(beats, line)
    end
    return table.concat(beats, " ")
end

return ReadingDisplay
```

---

## 18. Open Design Questions

1. **Chain discovery:** Should the player be told which chain partners each card has, or discover them organically? Suggested: card info panel shows "known affinities" — unlocked as the player encounters each chain for the first time. Creates a meta-game of chain discovery.
    
2. **Directional chain clarity:** Directional chains (Tower must precede Death) require the player to place cards in a specific order. On mobile, this may be easy to miss. Suggested: when a non-directional pair lands adjacent in wrong order, the reading shows a greyed-out chain indicator: "This chain requires Tower → Death in order." Teaches without punishing.
    
3. **Chain Break curse:** _"No Affinity chains fire this fight"_ is powerful but potentially frustrating for Chain-heavy decks. Consider: Chain Break only applies to Affinity chains; Tension chains still fire. Encourages building both types.
    
4. **Echo position (slots 4–5) and named combos:** Currently Echo doubles Chain bonuses but does not interact with Named Combos (The Sentence, The Reckoning, etc.). Should Echo trigger a named combo if the relevant cards are in Echo position? Suggested: no — named combos require the core three positions. Echo is a chain amplifier only.
    
5. **Zodiac + Chain interaction:** Does a Zodiac modifier on Origin affect a chain that includes Origin? Suggested: yes — Zodiac modifies the card's base stats before chain resolution. Aries doubling Origin Wounds would therefore make The Tower in Origin even more devastating in a Tower→Death chain.
    
6. **Circle 9 — "No Zodiac, No Rings, No Inscriptions":** Now reads as "Chains only." The purest test of narrative placement skill. Consider: Lucifer's three phases each disable one layer (Phase 1: Rings, Phase 2: Inscriptions, Phase 3: Symbols) but chains always remain. The story is always the thing.
    
7. **Default sentence generation:** Every hand needs a narrative sentence even without a chain. The `default_sentence` function uses `position_narrative` per card per position. This requires 22 × 3 = 66 short narrative fragments. Small writing task, large UX payoff.
    
8. **Score target tuning:** All numbers placeholder. Chain bonuses can compound heavily (two ×5 Mult chains + Gemini ♊ doubling deferred). Requires playtesting with a per-hand chain bonus cap.
    





# Abyss — Position Narrative Fragments

### 66 Short Lines for Reading Sentence Assembly

**Version:** 1.0 · Companion to GDD v0.4

---

## How These Are Used

When three cards are placed and no Chain exists between an adjacent pair, the game assembles a sentence from these fragments:

```
[Card A — Origin fragment] + [Card B — Crossing fragment] + [Card C — Outcome fragment]
```

Example (no chains):

> The Fool (Origin) + Justice (Crossing) + The Moon (Outcome) "Nothing was feared." + "The measure was applied." + "What remained was hidden." → _"Nothing was feared. The measure was applied. What remained was hidden."_

When a Chain exists between two adjacent cards, the Chain's own narrative replaces both fragments for that pair. The remaining unpaired fragment still appends.

Each fragment is a **complete clause** ending in a period. Past tense. Third-person-adjacent (implied "he", "the soul", "it") — never "you" or "I". Tone: mythic, compressed, matter-of-fact.

---

## The 66 Fragments

---

### 0 · The Fool

|Position|Fragment|
|---|---|
|**Origin**|"Nothing was feared."|
|**Crossing**|"The step was taken blindly."|
|**Outcome**|"What followed, no one expected."|

---

### I · The Magician

|Position|Fragment|
|---|---|
|**Origin**|"The will was gathered."|
|**Crossing**|"The impossible was made plain."|
|**Outcome**|"Something changed its nature."|

---

### II · The High Priestess

|Position|Fragment|
|---|---|
|**Origin**|"The secret was held close."|
|**Crossing**|"What was known was not spoken."|
|**Outcome**|"Memory became the weapon."|

---

### III · The Empress

|Position|Fragment|
|---|---|
|**Origin**|"The ground was made fertile."|
|**Crossing**|"What was given multiplied."|
|**Outcome**|"Abundance outlasted the wound."|

---

### IV · The Emperor

|Position|Fragment|
|---|---|
|**Origin**|"Order was declared without mercy."|
|**Crossing**|"Nothing bent that was not meant to bend."|
|**Outcome**|"The decree outlived the one who gave it."|

---

### V · The Hierophant

|Position|Fragment|
|---|---|
|**Origin**|"The old way was invoked."|
|**Crossing**|"Tradition held — or pretended to."|
|**Outcome**|"The weight of the past settled on what came next."|

---

### VI · The Lovers

|Position|Fragment|
|---|---|
|**Origin**|"A choice was made that could not be unmade."|
|**Crossing**|"The wound was accepted willingly."|
|**Outcome**|"The pain compounded, as pain does."|

---

### VII · The Chariot

|Position|Fragment|
|---|---|
|**Origin**|"Motion began and did not ask permission."|
|**Crossing**|"Nothing was allowed to slow it."|
|**Outcome**|"Arrival changed nothing — the moving did."|

---

### VIII · Strength

|Position|Fragment|
|---|---|
|**Origin**|"The beast was met directly."|
|**Crossing**|"The hand was steady."|
|**Outcome**|"Mastery was earned at the cost of ease."|

---

### IX · The Hermit

|Position|Fragment|
|---|---|
|**Origin**|"The retreat was chosen, not forced."|
|**Crossing**|"The lamp was raised in the dark."|
|**Outcome**|"Solitude became a kind of power."|

---

### X · Wheel of Fortune

|Position|Fragment|
|---|---|
|**Origin**|"The wheel turned without being asked."|
|**Crossing**|"Fate offered no explanation."|
|**Outcome**|"What landed, landed."|

---

### XI · Justice

|Position|Fragment|
|---|---|
|**Origin**|"The measure was prepared."|
|**Crossing**|"The exact weight was applied."|
|**Outcome**|"The scale came to rest."|

---

### XII · The Hanged Man

|Position|Fragment|
|---|---|
|**Origin**|"The surrender was deliberate."|
|**Crossing**|"Everything was given up at once."|
|**Outcome**|"What was sacrificed returned as something else."|

---

### XIII · Death

|Position|Fragment|
|---|---|
|**Origin**|"An ending was accepted."|
|**Crossing**|"The old thing was cut away."|
|**Outcome**|"What replaced it was not what was lost."|

---

### XIV · Temperance

|Position|Fragment|
|---|---|
|**Origin**|"The excess was restrained."|
|**Crossing**|"The measure was kept, barely."|
|**Outcome**|"Balance held — for now."|

---

### XV · The Devil

|Position|Fragment|
|---|---|
|**Origin**|"The chain was noticed too late."|
|**Crossing**|"The gift was accepted."|
|**Outcome**|"The debt came due."|

---

### XVI · The Tower

|Position|Fragment|
|---|---|
|**Origin**|"The flaw in the foundation was always there."|
|**Crossing**|"Everything fell."|
|**Outcome**|"The silence after was absolute."|

---

### XVII · The Star

|Position|Fragment|
|---|---|
|**Origin**|"A small light was still burning."|
|**Crossing**|"Hope persisted without reason."|
|**Outcome**|"Something survived that should not have."|

---

### XVIII · The Moon

|Position|Fragment|
|---|---|
|**Origin**|"The truth was dressed as something else."|
|**Crossing**|"What was seen was not what was there."|
|**Outcome**|"The illusion held longer than expected."|

---

### XIX · The Sun

|Position|Fragment|
|---|---|
|**Origin**|"The false was burned away first."|
|**Crossing**|"Clarity arrived without gentleness."|
|**Outcome**|"What remained was only what was real."|

---

### XX · Judgement

|Position|Fragment|
|---|---|
|**Origin**|"The ledger was opened."|
|**Crossing**|"Every action was accounted for."|
|**Outcome**|"The sentence was final."|

---

### XXI · The World

|Position|Fragment|
|---|---|
|**Origin**|"The cycle was nearly complete."|
|**Crossing**|"The last piece fell into place."|
|**Outcome**|"The ending was also the beginning."|

---

## Sentence Assembly Examples (No Chains)

These show how fragments read when concatenated without any Chain narrative. The goal: even a random draw should feel like a mythic sentence.

---

**The Fool + The Tower + Death**

> "Nothing was feared. Everything fell. What replaced it was not what was lost."

**The Hermit + Judgement + The Star**

> "The retreat was chosen, not forced. Every action was accounted for. Something survived that should not have."

**The Empress + The Moon + The Devil**

> "The ground was made fertile. What was seen was not what was there. The debt came due."

**Strength + The Lovers + The Hanged Man**

> "The beast was met directly. The wound was accepted willingly. What was sacrificed returned as something else."

**The Wheel of Fortune + The High Priestess + The World**

> "The wheel turned without being asked. What was known was not spoken. The ending was also the beginning."

**The Emperor + The Tower + Justice**

> "Order was declared without mercy. Everything fell. The scale came to rest."

**The Magician + Death + The Fool**

> "The will was gathered. The old thing was cut away. What followed, no one expected."

**Temperance + The Moon + The Sun**

> "The excess was restrained. What was seen was not what was there. What remained was only what was real."

**The Hanged Man + The Hermit + The World**

> "The surrender was deliberate. The lamp was raised in the dark. The ending was also the beginning."

**The Lovers + The Devil + The Tower**

> "A choice was made that could not be unmade. The gift was accepted. Everything fell."

---

## Lua Data Table

```lua
-- cards/position_narratives.lua
-- Used by reading_display.lua for default sentence assembly

local PositionNarratives = {
    fool = {
        origin   = "Nothing was feared.",
        crossing = "The step was taken blindly.",
        outcome  = "What followed, no one expected.",
    },
    magician = {
        origin   = "The will was gathered.",
        crossing = "The impossible was made plain.",
        outcome  = "Something changed its nature.",
    },
    high_priestess = {
        origin   = "The secret was held close.",
        crossing = "What was known was not spoken.",
        outcome  = "Memory became the weapon.",
    },
    empress = {
        origin   = "The ground was made fertile.",
        crossing = "What was given multiplied.",
        outcome  = "Abundance outlasted the wound.",
    },
    emperor = {
        origin   = "Order was declared without mercy.",
        crossing = "Nothing bent that was not meant to bend.",
        outcome  = "The decree outlived the one who gave it.",
    },
    hierophant = {
        origin   = "The old way was invoked.",
        crossing = "Tradition held — or pretended to.",
        outcome  = "The weight of the past settled on what came next.",
    },
    lovers = {
        origin   = "A choice was made that could not be unmade.",
        crossing = "The wound was accepted willingly.",
        outcome  = "The pain compounded, as pain does.",
    },
    chariot = {
        origin   = "Motion began and did not ask permission.",
        crossing = "Nothing was allowed to slow it.",
        outcome  = "Arrival changed nothing — the moving did.",
    },
    strength = {
        origin   = "The beast was met directly.",
        crossing = "The hand was steady.",
        outcome  = "Mastery was earned at the cost of ease.",
    },
    hermit = {
        origin   = "The retreat was chosen, not forced.",
        crossing = "The lamp was raised in the dark.",
        outcome  = "Solitude became a kind of power.",
    },
    wheel_of_fortune = {
        origin   = "The wheel turned without being asked.",
        crossing = "Fate offered no explanation.",
        outcome  = "What landed, landed.",
    },
    justice = {
        origin   = "The measure was prepared.",
        crossing = "The exact weight was applied.",
        outcome  = "The scale came to rest.",
    },
    hanged_man = {
        origin   = "The surrender was deliberate.",
        crossing = "Everything was given up at once.",
        outcome  = "What was sacrificed returned as something else.",
    },
    death = {
        origin   = "An ending was accepted.",
        crossing = "The old thing was cut away.",
        outcome  = "What replaced it was not what was lost.",
    },
    temperance = {
        origin   = "The excess was restrained.",
        crossing = "The measure was kept, barely.",
        outcome  = "Balance held — for now.",
    },
    devil = {
        origin   = "The chain was noticed too late.",
        crossing = "The gift was accepted.",
        outcome  = "The debt came due.",
    },
    tower = {
        origin   = "The flaw in the foundation was always there.",
        crossing = "Everything fell.",
        outcome  = "The silence after was absolute.",
    },
    star = {
        origin   = "A small light was still burning.",
        crossing = "Hope persisted without reason.",
        outcome  = "Something survived that should not have.",
    },
    moon = {
        origin   = "The truth was dressed as something else.",
        crossing = "What was seen was not what was there.",
        outcome  = "The illusion held longer than expected.",
    },
    sun = {
        origin   = "The false was burned away first.",
        crossing = "Clarity arrived without gentleness.",
        outcome  = "What remained was only what was real.",
    },
    judgement = {
        origin   = "The ledger was opened.",
        crossing = "Every action was accounted for.",
        outcome  = "The sentence was final.",
    },
    world = {
        origin   = "The cycle was nearly complete.",
        crossing = "The last piece fell into place.",
        outcome  = "The ending was also the beginning.",
    },
}

return PositionNarratives
```

---

_Companion to: `Abyss/GDD/abyss_gdd_v0.4.md`_ _Used by: `reading_display.lua` → `default_sentence()`_ _Last updated: 2026-07-23_

---

_Document maintained in Obsidian vault: `Abyss/GDD/abyss_gdd_v0.4.md`_ _Supersedes: `Abyss/GDD/abyss_gdd_v0.3.md` (v0.3)_ _Last updated: 2026-07-23_