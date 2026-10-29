using System.Globalization;
using Neko.Sim.Core;
using Neko.Sim.Save;
using Neko.Sim.Economy;
using Neko.Sim.Team;
using Neko.Sim.Maids;
using Neko.Sim.Shift;
using Neko.Sim.Cafe;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
if (args.Length != 1) throw new ArgumentException("Usage: EconBaseline OUTPUT_DIRECTORY");
Directory.CreateDirectory(args[0]);
using var output = new StreamWriter(Path.Combine(args[0], "days.csv"), false, new System.Text.UTF8Encoding(false));
output.NewLine = "\n";
output.WriteLine("scenario,seed,day,shift_seed,level_start,level_end,roster,cap,slots,boxes,duplicates,dismissals,purchases,revenue,wages,rent,ingredients,profit,cash_start,cash_end,satisfaction,reputation_delta,reputation,points_earned,points,served,angry,turned_away,bankrupt,opener");
foreach (var policy in new[] { "competent", "learner", "passive", "box_addict", "wrong" })
    for (uint seed = 1; seed <= 60; seed++) Run(policy, seed, 50, Array.Empty<string>());
foreach (var policy in new[] { "competent", "learner", "passive", "box_addict", "wrong" })
    for (uint i = 1; i <= 60; i++) Run("mixed_" + policy, unchecked(i * 2654435761u), 50, Array.Empty<string>());
for (uint seed = 1; seed <= 200; seed++) Run("day1", seed, 1, Array.Empty<string>());
var plans = new[] { Array.Empty<string>(), new[] { "plant" }, new[] { "table_2" },
    new[] { "stove" }, new[] { "stove", "stove" }, new[] { "plant", "table_2" },
    new[] { "plant", "stove" }, new[] { "table_2", "stove" } };
for (var plan = 0; plan < plans.Length; plan++)
    for (uint seed = 1; seed <= 100; seed++) Run("purchase_" + plan, seed, 50, plans[plan]);

void Run(string policy, uint seed, int maxDays, string[] purchases)
{
    var scenario = policy;
    if (policy.StartsWith("mixed_")) policy = policy.Substring(6);
    var p = Profile.NewGame(seed);
    // GameSession creates a fresh stream from Profile.Seed after NewGame's opener stream.
    var rng = new Rng(p.Seed == 0 ? 1u : p.Seed);
    var opener = string.Join(";", p.Maids.Select(m => $"{m.Archetype}:{m.House}:{m.Base.Serve}:{m.Base.Cook}:{m.Base.Charm}:{m.Base.Stamina}"));
    int losses = 0;
    for (int day = 1; day <= maxDays && !p.Bankrupt; day++)
    {
        int boxes = 0, duplicates = 0, dismissals = 0, spent = 0;
        if (day == 1)
            foreach (var item in purchases)
            {
                // First free tile, row-major, unrotated, exactly once; fail rather than silently skip.
                var def = PlaceableCatalog.ById(item);
                var cell = (from y in Enumerable.Range(0, p.Grid.Height)
                            from x in Enumerable.Range(0, p.Grid.Width)
                            where p.Grid.CanPlace(def, x, y) select (x, y)).First();
                if (!Shop.Buy(p, item).Ok || !p.Grid.Place(def, cell.x, cell.y))
                    throw new Exception("Purchase/placement refused: " + item);
                p.OwnedPlaceables.Remove(item);
                spent += def.Price;
            }
        bool competent = policy == "competent" || policy == "learner" && day > 3;
        if (competent)
        {
            Natural(p);
            // Keep the strongest single bench card; dismiss extra bench cards.
            foreach (var m in p.Maids.Where(m => !p.Lineup.Contains(m.Id)).OrderByDescending(Score).ThenBy(m => m.Id).Skip(1).ToList())
                if (p.Dismiss(m.Id)) dismissals++;
            if (losses >= 2 && p.Wallet.Money < Books.FixedCostsFor(p) * 2)
            {
                var candidate = p.Maids.Where(m => !p.Lineup.Contains(m.Id) ||
                    m.NaturalRole == Role.Stage || m.NaturalRole == Role.Management)
                    .OrderByDescending(m => m.Salary).ThenBy(m => m.Id).FirstOrDefault();
                if (candidate != null && p.Dismiss(candidate.Id)) dismissals++;
            }
            Natural(p);
            if (p.Lineup.All.Any(s => s.Empty) && p.Wallet.Money >= Books.FixedCostsFor(p) * 4 && p.Wallet.Points >= 40)
                Open("box_simple");
            Natural(p);
        }
        else if (policy == "box_addict")
        {
            while (p.Wallet.Points >= 40)
            {
                if (boxes >= 1000) throw new Exception("Box policy did not terminate");
                Open(p.Wallet.Points >= 520 ? "box_gold" : p.Wallet.Points >= 180 ? "box_silver" : "box_simple");
            }
            p.Lineup.AutoFill(p.Maids);
        }
        else if (policy == "wrong" || policy == "learner") Wrong(p);
        else p.Lineup.AutoFill(p.Maids);
        foreach (var role in new[] { Role.Reception, Role.Floor, Role.Kitchen })
            if (p.Lineup.MaidIdsIn(role).Count == 0) throw new Exception("GameSession readiness would refuse " + policy);
        int level = p.CafeLevel, rep = p.Wallet.Reputation, roster = p.Maids.Count;
        int cap = p.Unlocks.RosterCap, slots = p.Lineup.All.Count;
        uint shiftSeed = rng.NextUInt();
        var shift = new Shift(DayPlan.Build(p, shiftSeed));
        int ticks = 0;
        while (!shift.Finished && ticks++ < 40000) shift.Tick(0.05f);
        if (!shift.Finished) throw new Exception("Shift timeout");
        var result = shift.Finish();
        if (result.Impossible) throw new Exception(result.FailureReason);
        var ledger = Books.Close(p, result);
        if (ledger.BalanceAfter != ledger.BalanceBefore + ledger.Profit) throw new Exception("Ledger mismatch");
        losses = ledger.Profit < 0 ? losses + 1 : 0;
        output.WriteLine(string.Join(",", scenario, seed, day, shiftSeed, level, p.CafeLevel, roster, cap, slots,
            boxes, duplicates, dismissals, spent, ledger.Revenue, ledger.Wages, ledger.Rent, ledger.Ingredients,
            ledger.Profit, ledger.BalanceBefore, ledger.BalanceAfter, ledger.Satisfaction.ToString("R"),
            p.Wallet.Reputation - rep, p.Wallet.Reputation, ledger.PointsEarned, p.Wallet.Points,
            ledger.Served, ledger.LeftAngry, ledger.TurnedAway, ledger.Bankrupt ? 1 : 0, opener));
        void Open(string id)
        {
            var box = Lootbox.Open(p, id, rng);
            if (!box.Ok) throw new Exception(box.Refusal);
            boxes++;
            duplicates += box.Cards.Count(c => c.Duplicate);
            // Explicit policy decision to refill after buying; not a GameSession side effect.
            p.Lineup.AutoFill(p.Maids);
        }
    }
}
float Score(Maid m)
{
    var role = RoleTable.For(m.NaturalRole);
    return m.Total[role.Primary] + m.Total[role.Secondary] * 0.4f;
}
void Natural(Profile p)
{
    p.Lineup.Clear();
    foreach (var slot in p.Lineup.All)
    {
        var maid = p.Maids.Where(m => m.NaturalRole == slot.Role && !p.Lineup.Contains(m.Id))
            .OrderByDescending(Score).ThenBy(m => m.Id).FirstOrDefault();
        if (maid != null) p.Lineup.Assign(slot.Role, slot.Index, maid.Id);
    }
}
void Wrong(Profile p)
{
    Natural(p);
    var slots = p.Lineup.All.Where(s => !s.Empty).ToArray();
    var ids = slots.Select(s => s.MaidId).ToArray();
    p.Lineup.Clear();
    for (int i = 0; i < slots.Length; i++)
        p.Lineup.Assign(slots[(i + 1) % slots.Length].Role, slots[(i + 1) % slots.Length].Index, ids[i]);
}
