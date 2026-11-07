using Neko.Sim.Core;
using Neko.Sim.Save;
using Neko.Sim.Economy;
using Neko.Sim.Cafe;
using Neko.Sim.Team;
using Neko.Sim.Maids;
using Neko.Sim.Service;
using Neko.Sim.Shift;

// usage: EconBalance <seeds> <days> <style: idle|lazy|casual|pro> <invest: yes|no>
// Full careers: lineup, boxes, hiring and furniture decisions every morning, then the day.
int seeds = int.Parse(args[0]), days = int.Parse(args[1]);
string style = args.Length > 2 ? args[2] : "casual";
bool invest = args.Length <= 3 || args[3] == "yes";
bool verbose = args.Length > 4 && args[4] == "v";

var surv = new int[days + 1]; var rev = new double[days + 1]; var prof = new double[days + 1];
var n = new int[days + 1]; var lvl = new double[days + 1]; var cash = new double[days + 1];
var roster = new double[days + 1]; var wages = new double[days + 1]; var taps = new double[days + 1];
var seatsAvg = new double[days + 1]; var chips = new double[days + 1];
var bankrupt = 0;

for (uint seed = 1; seed <= seeds; seed++)
{
    var p = Profile.NewGame(seed * 2654435761u);
    var rng = new Rng(p.Seed == 0 ? 1u : p.Seed);
    var play = new Rng(seed + 99);
    var board = new ApplicantBoard();
    var wanted = new Queue<string>(new[] { "stove", "stove", "table_2", "table_2", "teabar", "table_4", "table_4" });

    for (int day = 1; day <= days && !p.Bankrupt; day++)
    {
        if (invest) Morning(p, rng, board, wanted);
        p.Lineup.AutoFill(p.Maids);
        var sh = new Shift(DayPlan.Build(p, rng.NextUInt()));
        float acc = 0;
        while (!sh.Finished)
        {
            sh.Tick(0.05f); acc += 0.05f;
            if (style == "idle") continue;
            float period = style == "pro" ? 0.05f : style == "casual" ? 1.2f : 4f;
            float perfect = style == "pro" ? 1f : style == "casual" ? 0.5f : 0.3f;
            if (acc < period) continue; acc = 0;
            foreach (var c in sh.Customers.ToList())
            {
                if (c.State == CustomerState.Queueing) sh.TryGreet(c.Id);
                else if (c.State == CustomerState.Ordering) sh.TryTakeOrder(c.Id);
                else if (c.Patience < 0.45f) sh.TryPamper(c.Id);
            }
            foreach (var s in sh.Seats.ToList()) if (s.Dirty) sh.TryClean(s.Index);
            foreach (var t in sh.Tickets.ToList())
            {
                var pr = sh.CookProgressOf(t);
                if (pr >= Shift.PerfectMin + 0.02f && pr <= Shift.PerfectMax && (perfect >= 1f || play.NextFloat() < perfect)) sh.TryPerfectCook(t.Id);
            }
        }
        var res = sh.Finish();
        int rosterNow = p.Maids.Count;
        var l = Books.Close(p, res);
        n[day]++; rev[day] += l.Revenue; prof[day] += l.Profit; lvl[day] += p.CafeLevel; cash[day] += l.BalanceAfter;
        roster[day] += rosterNow; wages[day] += l.Wages; taps[day] += res.Assists; chips[day] += p.Wallet.Points;
        seatsAvg[day] += p.Grid.Metrics().Seats;
        if (!l.Bankrupt) surv[day]++; else { bankrupt++; if (verbose) Console.WriteLine($"DEAD seed {seed} day {day}"); }
    }
}

Console.WriteLine($"== style={style} invest={invest} seeds={seeds} bankrupt={bankrupt}");
foreach (var d in new[] { 1, 2, 3, 5, 7, 10, 15, 20, 30, 40 }.Where(d => d <= days))
{
    double k = Math.Max(1, n[d]);
    Console.WriteLine($"day {d,2}: alive {surv[d],3}/{seeds}  rev {rev[d]/k,5:F0} profit {prof[d]/k,5:F0} lvl {lvl[d]/k,3:F1} cash {cash[d]/k,6:F0} maids {roster[d]/k,3:F1} wages {wages[d]/k,4:F0} seats {seatsAvg[d]/k,4:F1} chips {chips[d]/k,4:F0} taps {taps[d]/k,4:F0}");
}

void Morning(Profile p, Rng rng, ApplicantBoard board, Queue<string> wanted)
{
    int fixedCosts() => Books.FixedCostsFor(p);
    // 1. boxes: spend chips as they come, best box the chips can afford.
    for (int guard = 0; guard < 6; guard++)
    {
        var id = p.Wallet.Points >= 520 ? "box_gold" : p.Wallet.Points >= 180 ? "box_silver" : p.Wallet.Points >= 40 ? "box_simple" : null;
        if (id == null) break;
        if (!Lootbox.Open(p, id, rng).Ok) break;
    }
    // 2. hire when there is room and a comfortable reserve afterwards.
    board.Refresh(rng, p.CafeLevel, p.NextMaidId, p.Maids);
    if (p.Maids.Count < p.Unlocks.RosterCap)
    {
        var best = -1; double bestScore = 0;
        for (int i = 0; i < board.Candidates.Count; i++)
        {
            var a = board.Candidates[i]; if (a == null) continue;
            var t = a.Maid.Total; double worth = t.Serve + t.Cook + t.Charm + t.Stamina;
            if (p.Wallet.Money < a.Price + 4 * (fixedCosts() + a.Maid.Salary)) continue;
            if (worth / a.Price > bestScore) { bestScore = worth / a.Price; best = i; }
        }
        if (best >= 0) board.Hire(p, best);
    }
    // 3. one piece of furniture a morning, in a plausible order, when it is clearly affordable.
    if (wanted.Count > 0)
    {
        var id = wanted.Peek();
        var def = PlaceableCatalog.ById(id);
        // Tables only pay with a fourth maid on the floor: with three, every extra seat is another
        // party the same team cannot serve (see the README). A sensible player learns this fast.
        var staffed = def.Kind != PlaceableKind.Table || p.Maids.Count >= 4;
        if (staffed && def.UnlockCafeLevel <= p.CafeLevel && p.Wallet.Money >= def.Price + 3 * fixedCosts())
        {
            var cell = (from y in Enumerable.Range(0, p.Grid.Height) from x in Enumerable.Range(0, p.Grid.Width) where x >= 2 && p.Grid.CanPlace(def, x, y) select (x, y)).Cast<(int x, int y)?>().FirstOrDefault();
            if (cell != null && Shop.Buy(p, id).Ok)
            {
                if (p.Grid.Place(def, cell.Value.x, cell.Value.y)) p.OwnedPlaceables.Remove(id);
                wanted.Dequeue();
            }
        }
    }
}
