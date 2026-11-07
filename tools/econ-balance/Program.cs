using Neko.Sim.Core;
using Neko.Sim.Save;
using Neko.Sim.Economy;
using Neko.Sim.Team;
using Neko.Sim.Maids;
using Neko.Sim.Service;
using Neko.Sim.Shift;
// usage: Diag <seeds> <days>
int seeds = int.Parse(args[0]), days = int.Parse(args[1]);
foreach (var mode in new[] { args.Length > 2 ? args[2] : "casual" })
{
    var surv = new int[days + 1]; var rev = new double[days + 1]; var prof = new double[days + 1]; var n = new int[days + 1]; var lvl = new double[days + 1]; var cash = new double[days + 1]; var pts = new double[days + 1];
    for (uint seed = 1; seed <= seeds; seed++)
    {
        var p = Profile.NewGame(seed * 2654435761u);
        var rng = new Rng(p.Seed == 0 ? 1u : p.Seed);
        var play = new Rng(seed + 99);
        for (int day = 1; day <= days && !p.Bankrupt; day++)
        {
            p.Lineup.Clear();
            foreach (var slot in p.Lineup.All)
            {
                var m = p.Maids.Where(x => x.NaturalRole == slot.Role && !p.Lineup.Contains(x.Id)).OrderByDescending(x => { var r = RoleTable.For(x.NaturalRole); return x.Total[r.Primary] + x.Total[r.Secondary] * 0.4f; }).FirstOrDefault();
                if (m != null) p.Lineup.Assign(slot.Role, slot.Index, m.Id);
            }
            // buy the next affordable table/stove when rich enough (simple growth policy)
            var sh = new Shift(DayPlan.Build(p, rng.NextUInt()));
            float acc = 0;
            while (!sh.Finished)
            {
                sh.Tick(0.05f); acc += 0.05f;
                if (mode == "idle") continue;
                float period = mode == "pro" ? 0.05f : 1.2f;
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
                    if (mode == "pro" ? pr >= Shift.PerfectMin + 0.02f : (pr >= Shift.PerfectMin && play.NextFloat() < 0.5f)) sh.TryPerfectCook(t.Id);
                }
            }
            var res = sh.Finish();
            var l = Books.Close(p, res);
            n[day]++; rev[day] += l.Revenue; prof[day] += l.Profit; lvl[day] += p.CafeLevel; cash[day] += l.BalanceAfter; pts[day] += l.PointsEarned;
            if (!l.Bankrupt) surv[day]++; else Console.WriteLine($"DEAD seed {seed} day {day} served {res.Served} angry {res.LeftAngry} away {res.TurnedAway} wrong {res.WrongOrders} burnt {res.Burnt} rev {l.Revenue} costs {l.Costs} sat {res.Satisfaction:F2} | " + string.Join(" ", p.Maids.Select(m => m.Archetype + ":" + m.Total.Serve + "/" + m.Total.Cook + "/" + m.Total.Charm + "/" + m.Total.Stamina)));
        }
    }
    Console.WriteLine($"== {mode}: seeds={seeds}");
    foreach (var d in new[] { 1, 2, 3, 5, 7, 10, 15, 20, 30, 40 }.Where(d => d <= days))
        Console.WriteLine($"day {d,2}: alive {surv[d],3}/{seeds}  avgRev {(n[d]>0?rev[d]/n[d]:0),6:F0} avgProfit {(n[d]>0?prof[d]/n[d]:0),6:F0} avgLevel {(n[d]>0?lvl[d]/n[d]:0),4:F1} avgCash {(n[d]>0?cash[d]/n[d]:0),6:F0} chips {(n[d]>0?pts[d]/n[d]:0),5:F1}");
}
