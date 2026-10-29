# Economy and pricing proposal (ECON-01)

**Status:** proposal for the owner's decision. No repository file was changed.
**Process:** follows `docs/roadmap/UX-01_FIRST_DAY.md`: hypothesis, comparison with controlled seeds, trade-offs, and a decision to record in `docs/GDD.md`.

## 1. How this was measured

- The real `Neko.Sim` code, compiled outside Unity and run headless. Days tick at 0.05 s (what the skip button does), and one RNG stream drives days and boxes, as in `GameSession`.
- Variants run on a temporary copy of `Neko.Sim` where each number below reads an override. With no override the copy reproduces the shipped game exactly: identical output, and the repo's 257 simulation tests pass on it.
- 60 seeds × 50 days per configuration unless noted. Five scripted players:

| Player | What it does |
| --- | --- |
| Competent | Best cards in natural positions. Leaves optional slots empty instead of playing someone out of position. Opens a box only when a slot is empty and cash covers 4 days of fixed costs. Keeps at most one bench maid. After two losing days with thin cash, lets the most expensive non-essential card go. Never buys furniture. |
| Learner | Everyone out of position on days 1–3, then plays like Competent. |
| Passive | Presses Auto every morning. Never buys anything. |
| Box addict | Opens every box it can afford each morning. Never dismisses anyone. |
| Wrong lineup | Every maid shifted one slot along, so nobody is in position. |

These are proxies, not people. Unity, the UI and WebGL were not run.

## 2. What the current build does

### 2.1 No business survives the first week

| Measure | Current build |
| --- | --- |
| Businesses alive on day 7 (every player type above) | 0 of 60 |
| Median bankruptcy day | 4 (earliest: 3) |
| Day 1 (4 scripted easy parties) | Loss in 200 of 200 new games. Median revenue 119, costs 254 |
| Average day at level 1 (days 2+) | Revenue 106. Wages 57 + rent 144 + ingredients 68. Profit −163 |
| Satisfaction, days 2–5 | 0.14 |
| Reputation per day / fichas per day | 0.2 / 0.6 (level 2 needs 60 reputation, a Silver box 180 fichas) |
| Businesses that reached level 2 | 0 |

Buying something on day one does not change this. Eight purchase plans (plant, table, stove, two stoves, combinations) × 100 seeds: 0 % alive on day 10.

### 2.2 Why

**1. Demand is 3 to 9 times past the point where revenue peaks.** Arrivals per second are `0.055 × seats + 0.008 × (day − 1)`. Holding everything else fixed and varying only the arrival rate (natural-position commons, 30 seeds):

| Café | Revenue peaks at | Peak revenue/day | Current rate | Revenue at current rate |
| --- | --- | --- | --- | --- |
| L1, 4 seats | 0.09/s | 130 | 0.29/s (day 10) | ≈ 88 |
| L4, 12 seats | 0.12/s | 436 | 0.73/s | ≈ 250 |
| L8, 20 seats | 0.12–0.18/s | ≈ 880 | 1.10/s | 359 |

Past the peak, the room seats more parties than the kitchen can feed. On day 2, 3.8 of the 3.9 parties that leave angry do so while waiting for food, after the kitchen has already paid ingredients for part of their order. The day term also contradicts the `DayPlan` comment ("driven by how many seats there are, not by the day").

**2. Rent is most of the opening bill.** At level 1, rent is 144 of the 198 fixed costs (73 %). GDD §6 says the pressure should come from the payroll.

**3. The kitchen is slow for the patience on offer.** On day 1, with four parties arriving 16 s apart, a party waits 18.4 s for food from one common cook and pays with 0.53 patience on average. A party counts as happy from 0.45.

**4. Ingredients take most of what is left.** 22 % of the average unlocked menu price, charged on every dish cooked, including burnt ones and dishes for parties who left. Early days: 47–64 % of revenue.

No single lever fixes it. Measured with the passive player: demand alone leaves 5 % alive on day 7, rent alone 10 % on day 10, demand plus rent 93 % on day 7 but 18 % on day 40.

### 2.3 The roster cap is below the lineup at every level

| Level | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Lineup slots | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 10 | 10 |
| Roster cap today | 2 | 3 | 4 | 5 | 6 | 6 | 7 | 7 | 8 | 8 |

- A new business has 3 maids against a cap of 2, so every box at levels 1–2 turns all of its cards into fichas. The 40 starting fichas ("enough for one box on day one, so the shop is not a locked door", in `Profile`) buy a Simple box that returns 10.9 fichas on average.
- Stage and Management can never be staffed without dismissing someone. The 10-maid lineup in GDD §2.2 is unreachable (maximum 8).
- `CafeUnlocks.ShiftSlots` is not read anywhere. It looks like a field from before the lineup pivot.
- One existing test depends on this state (§6), so I can't tell whether it is intended.

### 2.4 Things that cost money, or are shown, and do nothing

| Item | What happens |
| --- | --- |
| **Roupas** (23 priced items, 90–520, ≈ 7,100 in total) | Can be bought. No screen equips them: `MaidProgression.Equip` is never called from `Neko.Game`. Money spent here is lost. |
| **Presentes** (80 / 200 / 450) | Only effect: +1 to every stat per bond level. The bond-5 "Marca registrada" shown on the card has no effect in `Neko.Sim`. The recipient rotates to the next maid on each purchase. |
| **Crítico** | `Judges` is never read. GDD §2.7 says the day's reputation depends on him. |
| **Habilidades** | Their effects exist in the shift, but no screen equips a skill. |
| **Hiring board** (`ApplicantBoard`) | Hiring with money exists in `Neko.Sim`. No screen uses it. |
| **Second cook** | A station cooks one dish at a time. From L4 the lineup has two cook slots and the starting room has one stove, so the second cook stands idle. A second stove adds +72/day at L4 (+127 with Tier 1) and pays back in 1–2 days. Nothing tells the player. |
| **Mesa para 4** (320) | Same 4 cells and 4 seats as two Mesa para 2 (240), but costs more and unlocks later. |

## 3. Proposal

### Tier 1: needed for the loop to work

| Lever | Where | Now | Proposed |
| --- | --- | --- | --- |
| Arrivals per second | `DayPlan.ArrivalsPerSecondFor` | `clamp(0.055·seats + 0.008·(day−1), 0.12, 1.1)` | `clamp(0.06 + 0.005·seats, 0.08, 0.30)` |
| Recipe times | `RecipeBook` | see below | × 0.7 for all 14 recipes |
| Rent | `Books.BaseRent`, `Books.RentPerTile` | 60 + 2.4 per tile | 20 + 1.2 per tile |
| Ingredient cost | `Books.IngredientFraction` | 0.22 | 0.16 |
| Roster cap | `CafeLevelTable` | 2, 3, 4, 5, 6, 6, 7, 7, 8, 8 | lineup slots + 1: 4, 5, 6, 7, 8, 9, 10, 11, 11, 11 |
| Reputation per day | `DayPlan.ReputationFor` | round(served × satisfaction × **2**) − angry | same with × **8** |
| Fichas per happy party | `Books.PointsPerHappyParty` | 2 | 4 |
| Starting money | `Profile.StartingMoney` | 600 | 800 |

Recipe times in seconds, now → proposed: Leite morno 3 → 2.1 · Omurice 8 → 5.6 · Curry da casa 11 → 7.7 · Lámen do chef 14 → 9.8 · Omurice imperial 16 → 11.2 · Matcha 5 → 3.5 · Chá de bolhas 7 → 4.9 · Chá real 11 → 7.7 · Panqueca de gato 6 → 4.2 · Parfait 9 → 6.3 · Sundae estelar 12 → 8.4 · Parfait real 15 → 10.5 · Pãozinho de gato 7 → 4.9 · Bolo decorado 13 → 9.1. The ordering by time and price is unchanged, so the recipe tests still hold.

Rent by level:

| Level | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9–10 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Now | 144 | 175 | 190 | 228 | 245 | 271 | 290 | 319 | 341 |
| Proposed | 62 | 78 | 85 | 104 | 112 | 126 | 135 | 150 | 160 |

Arrivals per day: L1 (4 seats, 75 s) 17–28 → 6. L4 (12 seats, 108 s) ≈ 72 → 13. L8 (20 seats, 152 s) 167 → 24. With the faster kitchen, revenue peaks at 0.12/s (L1), 0.12–0.18/s (L4) and 0.18/s (L8). The proposal sits at or just below each peak, on the side that keeps satisfaction up. A busier café is one constant away (base 0.08 instead of 0.06).

800 starting money is 6.9 days of the opening fixed costs under the new rent, which matches GDD §1 ("o caixa mal cobrindo a semana"). Today 600 covers 3.0 days, not the four the `Profile` comment states.

Unchanged on purpose: salaries (18 / 44 / 95 / 210), severance, box prices and odds, customer patience, the wrong-order penalty (−3), the flawless-day bonus (+15), the 40 starting fichas, and furniture and clothes prices apart from Tier 2.

### Tier 2: optional

| Change | Measured effect (together, on top of Tier 1) |
| --- | --- |
| Charge ingredients on the dishes actually cooked (recipe price × 0.16) instead of the unlocked-menu average. A result with no per-dish detail falls back to the average, so the existing test still passes. | Learner alive on day 50: 72 % → 82 % |
| Reputation for L6–L10: 900 / 1350 / 1950 / 2700 / 3700 → 850 / 1200 / 1600 / 2100 / 2700 | Competent reaches L7 on day 36 (was 39) and L8 on day 42 (was 47, in fewer than half the runs) |
| Mesa para 4: 320 → 200 | Removes a purchase that is strictly worse than two Mesa para 2 |

## 4. Results

Survival, % of 60 businesses alive on day 7 / 20 / 50:

| Player | Current | Tier 1 | Tier 1 + 2 |
| --- | --- | --- | --- |
| Competent | 0 / 0 / 0 | 100 / 97 / 92 | 100 / 92 / 88 |
| Learner | 0 / 0 / 0 | 100 / 83 / 72 | 100 / 88 / 82 |
| Passive | 0 / 0 / 0 | 100 / 97 / 93 | 100 / 98 / 97 |
| Box addict | 0 / 0 / 0 | 90 / 63 / 55 | 92 / 67 / 58 |
| Wrong lineup | 0 / 0 / 0 | 32 / 0 / 0 | 40 / 0 / 0 |

One business is 1.7 points, so gaps of a few points between Tier 1 and Tier 1 + 2 are seed noise.

Competent player under Tier 1 (the current build reaches none of these):

- **Levels:** L2 on day 4, L3 day 8, L4 day 14, L5 day 21, L6 day 29, L7 day 39, L8 day 47 (23 of 60 runs by day 50).
- **Fichas:** 10.9 per day at L1, rising to 32.5 at L8. Cumulative 162 by day 10, 310 by day 20, 667 by day 40. Saving from the start, the first Silver box is affordable on day 12 and the first Gold on day 34.
- **Day 1:** loss in 132 of 200 new games (was 200 of 200), median −24 (was −129).

Average day per level, competent player, Tier 1:

| Level | Revenue | Wages | Rent | Ingredients | Profit | Losing days |
| --- | --- | --- | --- | --- | --- | --- |
| L1 | 160 | 56 | 62 | 53 | −11 | 64 % |
| L2 | 242 | 82 | 78 | 76 | +6 | 50 % |
| L3 | 342 | 108 | 85 | 100 | +49 | 33 % |
| L4 | 438 | 145 | 104 | 118 | +70 | 31 % |
| L5 | 552 | 188 | 112 | 151 | +101 | 27 % |
| L6 | 860 | 271 | 126 | 181 | +282 | 16 % |
| L7 | 1158 | 368 | 135 | 214 | +442 | 9 % |
| L8 | 1594 | 486 | 150 | 246 | +712 | 4 % |

Payroll overtakes rent from level 2, which is what GDD §6 describes. These numbers are conservative: this player never buys a second station.

**Furniture payback under Tier 1** (bought into the starting room, full natural-position lineup of commons):

- Stations from L4: 1–7 days. A second stove adds +127/day at L4, +187 at L6, +273 at L8.
- Decor: mostly 5–18 days.
- Extra tables rarely pay back with the starting staff. Mesa para 2: +18/day at L1, −9 at L2, −3 at L4, −34 at L6, +15 at L8. Seats only pay when the floor and the kitchen have spare capacity.

**Rarity under Tier 1** (a card in her natural position instead of a common, net of the wage difference, per day, levels 2–10):

- Floor: Rare +34 to +203, Epic +38 to +495, Legendary +56 to +763.
- Kitchen: Rare −23 to +63, Epic −66 to +162, Legendary −153 to +1.
- Reception, Stage, Management: negative at almost every rarity and level.

No salary change is proposed. "Tirar a lendária é o começo do problema" (GDD §2.8) still holds, and the lineup teaches one clear rule: rare cards go to the floor. Making rarity worth it on Stage and Management would be a separate retune of `StagePatiencePerCharm` and `ManagementTipPerCharm`.

## 5. Trade-offs and risks

- **The late game gets rich.** A competent business has a median 5,950 in cash on day 40. Once the furniture is bought there is nothing left to spend money on, because clothes and hiring can't be reached (§2.4). Bankruptcy stays real for the box addict (45 % by day 50) and for the wrong lineup (100 % by day 10).
- **Quieter rooms.** With fewer arrivals, occupancy crosses 60 % (the Tsundere bonus) and 70 % (the music change) less often. The "no archetype is dead weight" test still passes, but this needs a playtest.
- **Kitchen × 0.7** raises revenue per second of station time by 43 %. Archetype multipliers stay relative to each other.
- **Proxies.** The scripted players never buy furniture and always know the natural positions. Real numbers need the UX-01 playtest.
- **Scope.** ROADMAP restriction 7 rules out balance changes in this cycle without your authorization.

## 6. Tests and documents

- The repo's 257 simulation tests, compiled against the copy with Tier 1 (and with Tier 1 + 2) applied: 256 pass. `LootboxTests.AFullRosterTurnsACardIntoFichasRatherThanLosingIt` fails because its setup relies on "café level one holds two maids and a new business already has three". Without the roster change, 257 of 257 pass. The test's intent (a full roster turns the card into fichas) survives if the test fills the roster to the cap first, but test files are protected, so that edit needs your maintenance exception.
- Not run: EditMode, PlayMode, Unity, WebGL. Nothing has been implemented yet.
- Documents to update in the implementation PR: GDD §2.2 (one bench slot above the lineup), §6 (demand curve, rent, payroll), §10 (new verified claims, such as "a competent opening survives the first week in N of 60 seeds"). `ITCH_PAGE.md` needs no number change, since salaries, box prices and odds stay the same.

## 7. Suggested implementation

1. One branch and one PR for Tier 1 in `Neko.Sim` plus the GDD, with a new seeded test file asserting the survival and "wrong lineup goes bankrupt" claims. If you grant the test exception, the roster cap goes in the same PR. Otherwise it ships separately.
2. Validation: `tools/simtest.sh`, `tools/run-tests.sh EditMode`, `tools/run-tests.sh PlayMode`, and the first five days played in the editor at 960×600, with captures of Empresa and Balanço.
3. Tier 2 and each §2.4 item as separate tasks.

## 8. Decisions for you

1. **Targets.** Are these the right ones? Competent week-one survival near 100 %, level 2 around day 4, first Silver box around day 12, first Gold around day 34, the box addict losing about half the time, an all-wrong lineup bankrupt within 10 days.
2. **Tier 1** as written, or a variant (busier café: arrival base 0.08; harsher start: keep 600)?
3. **Roster cap below the lineup:** bug or intended? If it's a bug, grant the exception for the one test.
4. **Tier 2:** yes or no.
5. **§2.4:** hide Roupas until an equip screen exists, or build the screen. Implement or remove the bond-5 and Crítico texts. Wire up or delete the hiring board.

## Reproducing

The harness is in `/tmp/neko-econ`, outside the repo and temporary:

```sh
cd /tmp/neko-econ
dotnet build -c Release -o bin                                   # shipped Neko.Sim, read-only link
dotnet build -c Release -p:SimRoot=/tmp/neko-econ/sim-variant \
  -p:BaseIntermediateOutputPath=/tmp/neko-econ/objv/ -o binv     # tunable copy (NEKO_* overrides)
bash sweep11.sh                                                  # survival table in §4
bash tier1-report.sh                                             # per-level books in §4
```
