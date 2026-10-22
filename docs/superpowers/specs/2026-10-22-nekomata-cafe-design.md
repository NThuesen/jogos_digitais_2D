# Nekomata Café — Design

**Status:** accepted base design, 2026-10-22. The owner opted out of the brainstorming
questions and handed every open choice to this session; everything below that is not in
the course brief is therefore **this session's call**, not an agreed requirement, and the
owner expects to revise UX by playing.

**Course brief:** `Projeto_2D-2026-1.docx.pdf` at the workspace root — Insper, Jogos
Digitais 2026.1, Projeto 2D, team of 4, delivery **2026-11-11 23:50**, WebGL build on
itch.io, GitHub repository with asset credits in the README, living GDD in Milanote,
graded on 12 rubrics of which #11 (state and error management) and #12 (GDD) cannot be
dropped.

---

## 1. Intent

A time-management restaurant game in the Café Mania / Diner Dash lineage, themed as a
cat-eared maid café. The player is the **manager**: they do not win by serving faster,
they win by **making a team of maids good at their job** and by laying out a room that
lets those maids work.

The brief's own words set the emphasis: *"grande parte do apelo vai ser você ser capaz de
melhorar o nível das maids"*, *"diversidade de maids com personalidades distintas e quero
que isso afete a gameplay diretamente"*, and *"a ênfase sempre vai ser no áudio/visual
para gerar uma experiência engraçada, diferente e agradável"*.

### The one sentence

*You hire cats, dress them, teach them, and watch them stop needing you.*

Early game the manager does everything by hand because the maids are slow and few. Late
game the manager stands still and the café runs itself. **That transition is the game**,
and it is what every upgrade buys.

## 2. Pillars

1. **The maids are the game.** Every system feeds "my maids got better", and every
   improvement is *visible on the sprite* and *audible in her voice*. An upgrade the
   player cannot see is a bug.
2. **The room is a machine.** Where a table sits decides how far someone walks.
   Customization has teeth: it is a layout puzzle wearing decoration's clothes.
3. **Charm is a mechanic, not a coat of paint.** Rubric 3 asks for information carried by
   the world rather than the HUD. Here the world is cute, so the cuteness *is* the
   signalling: an impatient customer's ears flatten, a ready plate steams, a maid at her
   limit sits down.

## 3. Goals, non-goals, anti-goals

**Goals**
- A complete, finishable game: menu → shifts → progression → endless, with a save.
- Nine maid archetypes whose personality is a **rule**, not a stat line.
- Four independent scaling axes on a maid (level, skills, equipment, bond).
- A grid café editor whose numbers (Ambience, Flow, Seats, Theme) visibly move outcomes.
- Audio and visual polish treated as a first-class system, not a final pass.
- An A on rubrics 3, 4, 7, 8, 11, 12 — the ones this design is built to win.

**Non-goals**
- Mobile/touch. Target is **desktop browser, mouse + keyboard**. WebGL is the delivery.
- Minigames for cooking. Skill expresses itself as speed and failure chance, so that
  investment in a maid — not the player's reflexes — is what shows.
- Story mode, cutscenes, voice acting, dating-sim routes.
- Any imported art or audio asset. See §8.
- Monetization, ads, analytics, accounts.

**Anti-goals** — things that would make this a failure even if it ran
- **The player clicking as fast as they can.** If the optimal play at café level 8 is the
  same frantic clicking as level 1, the upgrade fantasy died and the game is a clicker.
- **Maids that are a number on a card.** If swapping a Tsundere for a Kuudere changes only
  a multiplier, the diversity requirement failed.
- **Decoration without consequence.** If the café editor is a dress-up screen, the
  customization requirement failed.
- **A dead end.** A player who misses a quota must always have a way forward (rubric 11).
- **Cute as a texture.** If the charm is in the sprites and not in the feedback, rubric 3
  scores C.

## 4. Constraints

| Constraint | Value | Why |
| --- | --- | --- |
| Engine | Unity **6000.5.9f1** | the only editor installed on this machine |
| Pipeline | **2D URP** (`Renderer2D`) | 2D lights and post-processing without hand-rolled shaders |
| Build target | **WebGL** | the brief requires it; module is installed |
| Input | Unity Input System, mouse + keyboard | starter baseline; no legacy `UnityEngine.Input` |
| Art | **generated at runtime, zero imported textures** | studio rule; also removes every asset-provenance and plagiarism question the brief raises |
| Audio | **synthesised**, committed as `.wav` | same reason; produced by the `audio` MCP server |
| Scenes | **one scene, built in code** | hand-authored Unity YAML is unreviewable, unmergeable in a team of 4, and untestable |
| Simulation | pure C#, no `UnityEngine` reference | so the whole game can be tested in EditMode, headless, deterministically |
| Save | `JsonUtility` → `PlayerPrefs` | WebGL-backed by IndexedDB; versioned and checksummed |
| Language | code, identifiers, commits in **English**; player-facing text and the GDD in **Portuguese** | studio convention; the course is Brazilian |
| Frame budget | 60 fps at 1280×720 on an integrated GPU | WebGL on a student laptop |

## 5. The shift — core loop

A **day** is one shift, 75–180 seconds depending on café level.

```
door → queue → seat → think → order → ticket → cook → plate → deliver
     → eat → pay → leave → bus → table free
```

Every arrow is a **task** that some worker must walk to and perform.

- Customers arrive on a per-day **arrival curve**; they queue at the door up to the door's
  capacity, and a full queue turns arrivals away (lost money, small reputation hit).
- Each customer carries a **Patience** meter that drains while they are waiting for
  anything. It does not drain while they are eating. Zero patience → they leave angry:
  no money, reputation penalty, and the table still needs bussing.
- An order is 1–3 dishes drawn from the unlocked recipe set.
- A ticket is cooked at a **station** that supports its recipe. Cook time is
  `recipe.seconds / maid.CookSpeed`; a fumble (archetype- and skill-dependent) wastes a
  portion of it.
- Payment is `sum(recipe.price) * ambienceTipMultiplier * charmMultiplier *
  (0.5 + 0.5 * patienceRemaining)`.

### Who does the work

- **Maids** are autonomous. Each idle maid repeatedly claims the highest-priority task she
  is allowed to do, walks to it, and performs it. Priority is a fixed weighting (a
  customer about to time out outranks a dirty table), so that the player's lever is
  *which maids are on shift and how good they are*, not micro-command.
- **The manager** is the player's avatar. Click a customer, a ticket, a station or a dirty
  table and he walks there and does it himself. He is a competent generalist: 1.0× at
  everything, never fumbles, never tires — and there is only one of him.
- **Rush** (right-click or hold on a maid) gives her +60% speed for 6 seconds and costs
  Mood. It is the panic button, and over-using it puts a maid on the bench.

This is the mechanical statement of the pillar: the manager is a constant, the maids are
the variable, so every hour invested in them is an hour the player's hands get quieter.

## 6. The maids

### 6.1 Stats

| Stat | Reads as | Drives |
| --- | --- | --- |
| **Serve** | how fast she is on the floor | greet, order, deliver, bus speed; walk speed |
| **Cook** | how good she is at a station | cook speed, fumble chance |
| **Charm** | how much customers like her | tip multiplier; patience regen for customers within 2 tiles |
| **Stamina** | how long she lasts | Mood drain rate during a shift, recovery between days |

Stats are `1..100`. Growth per level is drawn from the archetype's growth profile, so a
Genki levels into Serve and a Mukuchi levels into Cook without the player choosing.

### 6.2 Archetypes — personality as a rule

Nine. Each is a **rule that changes how a shift plays**, plus a silhouette tell and a
palette accent so she is recognisable at a glance, plus a voice blip of her own.

| Archetype | Rule | Tell |
| --- | --- | --- |
| **Genki** | +35% move speed; Mood drains 1.6× | twin tails, high |
| **Kuudere** | never fumbles a dish; −20% move speed | straight bob, half-lidded eyes |
| **Tsundere** | +30% to every speed while ≥60% of tables are occupied; −15% tips while idle | side ponytail, large ribbon |
| **Dojikko** | 12% chance to drop a plate; every drop gives all seated customers +8% patience | oversized apron bow, ahoge |
| **Ojou-sama** | doubles VIP arrival weight; refuses to bus tables | drill curls, tiara |
| **Onee-san** | maids within 2 tiles gain +15% speed; gives 25% of her XP to the lowest-level maid | long hair, shawl |
| **Idol** | active *Encore*: refills all seated customers to 70% patience, 90 s cooldown | star hairpin, cape |
| **Gourmand** | cooks 1.8× faster; eats 1 serving in 5 (that dish's price is lost) | round face, chef scarf |
| **Mukuchi** | −40% greet speed, +50% cook speed; Charm counts double while at a station | hair over one eye, mask down |

The rules are deliberately uneven — Ojou-sama's refusal to bus is a real cost, Dojikko is
high variance. A roster is a composition problem, which is what makes the diversity
requirement land in the gameplay rather than in the art.

### 6.3 The four scaling axes

1. **Level 1 → 20.** XP from completed tasks during a shift. Each level grants stat growth;
   every 4th level opens a **skill slot** (3 slots at level 20).
2. **Skills.** Three branches — Floor, Kitchen, Charm — six skills each. A maid equips up
   to 3. Examples: *Double Tray* (carries two plates per trip), *Mise en Place* (first
   ticket of a shift is instant), *Regulars* (a customer she greeted returns the next day
   spending +20%), *Second Wind* (Mood refills once per shift at 0).
3. **Equipment.** Four slots — Outfit, Headwear, Accessory, Shoes. Every piece gives stats
   **and is drawn on her sprite**. Pieces carry a theme tag; wearing 3 of a theme gives a
   set bonus.
4. **Bond 0 → 5.** Raised by gifts (shop) and by shifts worked. Each level unlocks an
   archetype-specific perk and a line of dialogue; Bond 5 unlocks her **Signature**, a
   unique passive that only she can have.

Four axes that move independently is what "escalar isso o máximo possível" buys: a level-12
Tsundere in a full Sakura set with *Double Tray* and Bond 4 is a different worker from the
same Tsundere at Bond 0, and both are legible on screen.

### 6.4 Roster and hiring

- **Applicants board**: 3 rotating candidates, refreshes each day, hire cost scales with
  the candidate's rolled stats and rarity.
- **Roster cap** grows with café level: 2 → 8.
- **Shift slots** grow with café level: 2 → 6. Maids not on shift recover Mood faster.

## 7. The café

A grid room, **7×5 at café level 1, 13×9 fully expanded** across four expansions.

| Placeable | Kind | What it does |
| --- | --- | --- |
| 2-seat / 4-seat table, counter stool | Table | seats; party size matching |
| Stove, Dessert bench, Tea bar, Bakery | Station | cooks a recipe subset, in parallel |
| Plant, lantern, cat tower, stage, rug, window, poster | Décor | **Ambience**, theme tag |
| Door, kitchen rail, sink | Fixture | queue capacity, ticket buffer, bus speed |

Four derived numbers, all shown live in the editor and all affecting the shift:

- **Ambience** — sum of décor, scaled by room size → patience and tip multipliers.
- **Flow** — mean walk distance over door↔table and table↔station pairs, shown as a number
  and as a heat overlay on the floor. Lower is better. This is the layout puzzle.
- **Seats** — simultaneous customers the room can hold.
- **Theme match** — ≥70% of décor sharing one theme grants that theme's set bonus.

## 8. Audio-visual direction

Rubrics 3, 7 and 8 are the ones this section is built to win, and the owner named
audio/visual as the highest priority.

### Palette

A warm cream base, and **one saturated colour reserved for things that need attention** —
a customer about to leave, a plate that is ready, a level-up. Everything else is held
below it. Per the studio's art-direction rule, value separation is checked in CIE L\* with
`pixels palette_check`, and no target shares a hue with what it sits on.

### Art

Every sprite is generated at runtime into a `Texture2D`: no file is imported, nothing has
a licence to defend, and the plagiarism section of the brief is answered by construction.

A maid is **a stack of tinted procedural layers** — body, tail, ears, hair, outfit,
headwear, accessory, face — which is also exactly what makes equipment visible: equipping
a piece swaps a layer. Chibi, roughly two heads tall, built to read as a silhouette at
64 px because at that size a silhouette is all there is.

### Feel

Squash-and-stretch on every pickup and drop; pop-in on spawn; heart, sparkle and steam
particles; floating damage-style numbers for money; a bounce on every button; micro-shake
when a plate drops; a held frame on level-up. Separate clocks for gameplay and UI so that
a pause or a slow-mo never desynchronises the interface.

### Sound

A synthesised set with a single tuning so it never goes shrill: door bell, order pop,
ticket slap, sizzle, plate chime, register, coin, heart, **nine "nya" blips (one per
archetype, so a maid is identifiable with eyes closed)**, angry puff, level-up arpeggio,
UI tick/confirm/back.

Three music beds — Menu, Shift Calm, Shift Rush — crossfading when the room passes 70%
occupancy or the clock drops under 20 seconds, so the music reports the state of the game.

## 9. Progression and difficulty

- **Café level 1 → 10**, from cumulative Reputation. Each level unlocks some of: a room
  expansion, recipes, furniture, a maid slot, a shift slot, a customer type, a station.
- **Day quota**: `quota(n) = 120 * n^1.35`, checked with `balance progression_check`.
- Difficulty rises along four dials at once: arrival rate up, starting patience down,
  order size up, and new customer modifiers.

### Customer types

| Type | Modifier |
| --- | --- |
| Regular | baseline |
| Student | pays little, very patient |
| Salaryman | impatient, spends well |
| Critic | silently scores the service; grants or removes Reputation at the end |
| Otaku | wants a photo with a specific archetype; huge tip if that archetype is on shift |
| Cat | orders only milk, never runs out of patience, raises Ambience while seated |
| VIP | spends hugely, almost no patience |

### Replayability (rubric 9)

- **Endless mode** unlocked after day 20: the curve keeps climbing until the player fails.
- **Daily modifiers** on endless ("Rush Hour", "Critic's Week", "Cat Festival").
- Archetype composition and the layout puzzle make a second run genuinely different.
- Per-run records kept in the save and shown on the menu.

## 10. State and error management (rubric 11, cannot be dropped)

- **Explicit state machine** for the application: `Boot → Menu → Café (editor/roster/shop)
  → Shift → Results → Café`, with pause available from Shift and Café.
- **No dead ends.** Missing a quota is a *slow day*: no bonus, and the next day's quota
  does not advance. Money never goes negative; the shop refuses rather than allowing debt.
  A maid at 0 Mood goes to the break room for the rest of the shift and comes back — she
  never quits.
- **Save robustness.** A save carries a schema version and a checksum. A save from an older
  version is migrated; one that fails its checksum is replaced by a fresh profile and the
  player is told, in a dialog, rather than the game crashing or silently losing progress.
- **Every long operation is interruptible.** Pause stops the shift clock, the music ducks,
  and no task resolves while paused.
- **Unhandled exceptions during a shift** are caught at the simulation boundary, logged,
  and the shift is ended as a slow day rather than left in a broken state.

## 11. Technical shape

| Assembly | References Unity | Holds |
| --- | --- | --- |
| `Neko.Sim` | **no** | the whole simulation: shift, tasks, maids, economy, progression, layout, save model |
| `Neko.Game` | yes | runtime scene construction, procedural art, audio, UI, input, the Unity↔Sim bridge |
| `Neko.Tests.EditMode` | — | the simulation, headless and deterministic |
| `Neko.Tests.PlayMode` | — | boot, a scripted shift, and the capture test the `unity` MCP server drives |

`Neko.Sim` having no `UnityEngine` reference is the load-bearing decision: it means a full
day of café simulation can be run in milliseconds in a unit test with a fixed seed, which
is the only way the balance claims in §9 can be checked rather than asserted.

## 12. What is left to the builder

- Exact tuning numbers beyond those pinned here (walk speeds, particle counts, easing
  curves) — tune by measurement with the `balance` and `pixels` servers.
- The precise shape vocabulary of the procedural sprites.
- UI layout within the stated screens.
- Which of the nine archetypes are available at café level 1 (at least Genki and Mukuchi,
  so that the floor/kitchen split is teachable on day 1).
