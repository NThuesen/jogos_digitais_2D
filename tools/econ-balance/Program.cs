using System.Globalization;
using System.Text;
using Neko.Sim.Core;
using Neko.Sim.Save;
using Neko.Sim.Economy;
using Neko.Sim.Cafe;
using Neko.Sim.Team;
using Neko.Sim.Maids;
using Neko.Sim.Service;
using Neko.Sim.Shift;

// EconBalance v3 (ECON-02b). Full careers on the real Neko.Sim rules.
//
//   EconBalance <seeds> <days> <style> <policy> [key=value ...]
//
//   style   idle | lazy | casual | pro
//   policy  none | legacy | conservative | balanced | overexpand | hire | hire-excess | full | tables-only
//           (legacy = the ECON-02 "invest" robot; "yes"/"no" are accepted as legacy/none)
//   options lineup=auto|smart|random   start=common|rare|epic|legendary   csv=<file> tag=<label>
//           setup=<k:v,k:v>  ablation at day 0 (t2, t4, stool, stove, tea, maids)   v=1 verbose deaths
//
// RNG discipline: the day seed is a pure function of (seed, day), and the shop/board/box draws use
// their own stream. So two policies on the same seed see the same days; they diverge only through
// the state their own decisions produce, never because one of them consumed more random numbers.
var A = args.ToList();
int seeds = int.Parse(A[0]), days = int.Parse(A[1]);
string style = A.Count > 2 ? A[2] : "casual";
string policy = A.Count > 3 ? A[3] : "none";
if (policy == "yes") policy = "legacy"; if (policy == "no") policy = "none";
var opt = new Dictionary<string, string>();
foreach (var a in A.Skip(4)) { var kv = a.Split('=', 2); opt[kv[0]] = kv.Length > 1 ? kv[1] : "1"; }
string lineupMode = opt.GetValueOrDefault("lineup", "auto");
string startRarity = opt.GetValueOrDefault("start", "common");
string tag = opt.GetValueOrDefault("tag", $"{style}/{policy}");
bool verbose = opt.ContainsKey("v");

var all = new List<DayRec>();
for (uint seed = 1; seed <= seeds; seed++) all.AddRange(Career(seed));

Report(all);
if (opt.TryGetValue("csv", out var csv)) WriteCsv(csv, all);
return;

// ---------------------------------------------------------------------------------------------

List<DayRec> Career(uint seed)
{
    var recs = new List<DayRec>();
    var p = Profile.NewGame(seed * 2654435761u);
    var shop = new Rng(unchecked(seed * 40503u + 7u));
    var play = new Rng(seed + 99);
    var board = new ApplicantBoard();
    var legacy = new Queue<string>(new[] { "stove", "stove", "table_2", "table_2", "teabar", "table_4", "table_4" });
    ApplyStart(p, shop);
    if (opt.TryGetValue("rep", out var repS)) { p.Wallet.AddReputation(int.Parse(repS)); p.ApplyUnlocks(); }
    if (opt.TryGetValue("setup", out var setup)) ApplySetup(p, setup);

    for (int day = 1; day <= days && !p.Bankrupt; day++)
    {
        var rec = new DayRec { Tag = tag, Seed = seed, Day = day, Cash0 = p.Wallet.Money, Rep0 = p.Wallet.Reputation };
        Morning(p, shop, board, legacy, rec);
        SetLineup(p, shop);
        rec.Level = p.CafeLevel;
        var m = p.Grid.Metrics();
        rec.Seats = m.Seats; rec.Stations = m.StationCount; rec.Maids = p.Maids.Count;
        rec.Fielded = p.Lineup.MaidIds().Count;
        rec.Tasking = p.FieldedMaids().Count(x => RoleTable.For(p.Lineup.RoleOf(x.Id) ?? Role.Floor).TakesTasks);
        rec.Rent = Books.RentFor(p); rec.Wages = Books.WagesFor(p);

        var setupDay = DayPlan.Build(p, DaySeed(seed, day));
        rec.Arrivals = setupDay.ArrivalsPerSecond; rec.Duration = setupDay.DurationSeconds;
        var sh = new Shift(setupDay);
        float acc = 0; long busy = 0, slots = 0, occ = 0, ticks = 0, cookBusy = 0;
        var tasking = sh.Workers.Where(w => w.TakesTasks).ToList();
        while (!sh.Finished)
        {
            sh.Tick(0.05f); acc += 0.05f; ticks++;
            foreach (var w in tasking) { slots++; if (w.Claimed != null) busy++; }
            occ += (long)(sh.Occupancy * 1000);
            cookBusy += sh.Tickets.Count(t => t.State == TicketState.Cooking);
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
        var l = Books.Close(p, res);
        rec.Revenue = l.Revenue; rec.Profit = l.Profit; rec.Cash1 = l.BalanceAfter; rec.Ingredients = l.Ingredients;
        rec.Served = l.Served; rec.Angry = l.LeftAngry; rec.TurnedAway = l.TurnedAway; rec.Wrong = l.WrongOrders;
        rec.Satisfaction = l.Satisfaction; rec.Chips = p.Wallet.Points; rec.Rep1 = p.Wallet.Reputation;
        rec.Taps = res.Assists; rec.Dead = l.Bankrupt;
        rec.WorkerUtil = slots == 0 ? 0 : busy / (double)slots;
        rec.SeatOcc = ticks == 0 ? 0 : occ / 1000.0 / ticks;
        rec.StationUtil = ticks == 0 || rec.Stations == 0 ? 0 : cookBusy / (double)ticks / rec.Stations;
        recs.Add(rec);
        if (l.Bankrupt && verbose) Console.WriteLine($"DEAD seed {seed} day {day}");
    }
    return recs;
}

uint DaySeed(uint seed, int day) => unchecked((seed * 2246822519u) ^ ((uint)day * 3266489917u) ^ 0x9E3779B9u);

void ApplyStart(Profile p, Rng shop)
{
    if (startRarity == "common") return;
    var rar = startRarity switch { "rare" => Rarity.Rare, "epic" => Rarity.Epic, _ => Rarity.Legendary };
    var roles = p.Maids.Select(x => x.NaturalRole).ToList();
    p.Maids.Clear(); p.Lineup.Clear();
    foreach (var r in roles)
    {
        var cands = MaidCatalog.FreeReleased(p.Maids).Where(i => RoleTable.NaturalFor(i.Archetype) == r).ToList();
        var id = cands.Count > 0 ? shop.Pick(cands) : shop.Pick(MaidCatalog.FreeReleased(p.Maids));
        var m = Lootbox.Mint(p, rar, shop, id); m.NaturalRole = r; p.Add(m);
    }
}

void ApplySetup(Profile p, string setup)
{
    foreach (var part in setup.Split(','))
    {
        var kv = part.Split(':'); int n = int.Parse(kv[1]);
        string id = kv[0] switch { "t2" => "table_2", "t4" => "table_4", "stool" => "stool", "stove" => "stove", "tea" => "teabar", _ => null };
        if (kv[0] == "maids") { AddCheapMaids(p, n); continue; }
        for (int i = 0; i < n; i++) PlaceAnywhere(p, PlaceableCatalog.ById(id));
    }
}

void AddCheapMaids(Profile p, int n)
{
    var r = new Rng(12345);
    for (int i = 0; i < n; i++)
    {
        var free = MaidCatalog.FreeReleased(p.Maids);
        var m = Lootbox.Mint(p, Rarity.Common, r, r.Pick(free)); p.Add(m);
    }
}

bool PlaceAnywhere(Profile p, PlaceableDef def)
{
    for (int y = 0; y < p.Grid.Height; y++)
        for (int x = 2; x < p.Grid.Width; x++)
            if (p.Grid.CanPlace(def, x, y)) return p.Grid.Place(def, x, y);
    return false;
}

void SetLineup(Profile p, Rng shop)
{
    if (lineupMode == "auto") { p.Lineup.AutoFill(p.Maids); SkipRole(p); return; }
    p.Lineup.Clear();
    if (lineupMode == "random")
    {
        var pool = p.Maids.OrderBy(_ => shop.NextUInt()).ToList();
        int k = 0;
        foreach (var s in p.Lineup.All) if (k < pool.Count) p.Lineup.Assign(s.Role, s.Index, pool[k++].Id);
        return;
    }
    // smart: tasking roles first (they carry the shift), best primary stat, natural role as tiebreak bonus.
    var left = p.Maids.ToList();
    foreach (var role in new[] { Role.Kitchen, Role.Floor, Role.Reception, Role.Stage, Role.Management })
        foreach (var s in p.Lineup.All.Where(x => x.Role == role))
        {
            var prof = RoleTable.For(role);
            var best = left.OrderByDescending(m => Score(m, role, prof)).FirstOrDefault();
            if (best == null) break;
            p.Lineup.Assign(role, s.Index, best.Id); left.Remove(best);
        }
}

void SkipRole(Profile p)
{
    if (!opt.TryGetValue("skiprole", out var r)) return;
    var role = Enum.Parse<Role>(r);
    foreach (var s in p.Lineup.All) if (s.Role == role) s.MaidId = -1;
}

double Score(Maid m, Role role, RoleProfile prof)
{
    var t = m.Total;
    double s = t[prof.Primary] + 0.4 * t[prof.Secondary];
    return m.NaturalRole == role ? s : s * (1 - RoleTable.OutOfPositionPenalty);
}

int Fixed(Profile p) => Books.FixedCostsFor(p);

void Boxes(Profile p, Rng shop)
{
    for (int guard = 0; guard < 6; guard++)
    {
        var id = p.Wallet.Points >= 520 ? "box_gold" : p.Wallet.Points >= 180 ? "box_silver" : p.Wallet.Points >= 40 ? "box_simple" : null;
        if (id == null) break;
        if (!Lootbox.Open(p, id, shop).Ok) break;
    }
}

// score of a candidate for "who do I need": raw usefulness per coin, role-aware when asked.
bool Hire(Profile p, ApplicantBoard board, double reserveMult, bool roleAware)
{
    if (p.Maids.Count >= p.Unlocks.RosterCap) return false;
    int best = -1; double bestScore = 0;
    var slots = p.Lineup.All;
    for (int i = 0; i < board.Candidates.Count; i++)
    {
        var a = board.Candidates[i]; if (a == null) continue;
        if (p.Wallet.Money < a.Price + reserveMult * (Fixed(p) + a.Maid.Salary)) continue;
        double worth;
        if (roleAware)
        {
            // gain over the current occupant of the weakest tasking slot this maid fits, per daily wage.
            double gain = 0;
            foreach (var s in slots.Where(x => RoleTable.For(x.Role).TakesTasks))
            {
                var prof = RoleTable.For(s.Role);
                var cur = s.Empty ? null : p.MaidById(s.MaidId);
                double now = cur == null ? 0 : Score(cur, s.Role, prof);
                gain = Math.Max(gain, Score(a.Maid, s.Role, prof) - now);
            }
            worth = gain / (a.Maid.Salary + 1);
            if (gain <= 0) continue;
        }
        else { var t = a.Maid.Total; worth = (t.Serve + t.Cook + t.Charm + t.Stamina) / (double)a.Price; }
        if (worth > bestScore) { bestScore = worth; best = i; }
    }
    return best >= 0 && board.Hire(p, best).Ok;
}

bool Buy(Profile p, string id, double reserveMult)
{
    var def = PlaceableCatalog.ById(id);
    if (def.UnlockCafeLevel > p.CafeLevel || p.Wallet.Money < def.Price + reserveMult * Fixed(p)) return false;
    var cell = (from y in Enumerable.Range(0, p.Grid.Height) from x in Enumerable.Range(0, p.Grid.Width) where x >= 2 && p.Grid.CanPlace(def, x, y) select (x, y)).Cast<(int x, int y)?>().FirstOrDefault();
    if (cell == null || !Shop.Buy(p, id).Ok) return false;
    if (p.Grid.Place(def, cell.Value.x, cell.Value.y)) p.OwnedPlaceables.Remove(id);
    return true;
}

int Count(Profile p, StationKind k) => p.Grid.Stations().Count(s => s.Kind == k);

void Morning(Profile p, Rng shop, ApplicantBoard board, Queue<string> legacy, DayRec rec)
{
    int before = p.Wallet.Money;
    if (policy != "none") Boxes(p, shop);
    if (policy != "none") board.Refresh(shop, p.CafeLevel, p.NextMaidId, p.Maids);
    int maids0 = p.Maids.Count, seats0 = p.Grid.Metrics().Seats, st0 = p.Grid.Metrics().StationCount;
    switch (policy)
    {
        case "none": break;
        case "legacy":
            Hire(p, board, 4, false);
            if (legacy.Count > 0)
            {
                var def = PlaceableCatalog.ById(legacy.Peek());
                var staffed = def.Kind != PlaceableKind.Table || p.Maids.Count >= 4;
                if (staffed && Buy(p, legacy.Peek(), 3)) legacy.Dequeue();
            }
            break;
        case "conservative":
            Hire(p, board, 8, true);
            if (Count(p, StationKind.Stove) < 2) Buy(p, "stove", 6);
            break;
        case "hire": Hire(p, board, 4, true); break;
        case "hire-excess": Hire(p, board, 1, false); break;
        case "tables-only": Buy(p, "table_2", 2); break;
        case "overexpand":
            if (!Buy(p, "table_4", 1)) Buy(p, "table_2", 1);
            break;
        case "balanced":
        case "full":
            if (policy == "full") Hire(p, board, 4, true);
            Expand(p);
            break;
    }
    Prune(p, shop);
    rec.Hired = p.Maids.Count - maids0; rec.BoughtSeats = p.Grid.Metrics().Seats - seats0; rec.BoughtStations = p.Grid.Metrics().StationCount - st0;
    rec.Invested = before - p.Wallet.Money;
}


// A sensible owner lets the most expensive benched maid go when the bank gets thin (the game offers Dismiss
// for exactly this); without it a robot that over-hires could never recover and would overstate bankruptcies.
void Prune(Profile p, Rng shop)
{
    if (policy == "none" || opt.ContainsKey("noprune")) return;
    for (int guard = 0; guard < 4 && p.Wallet.Money < 5 * Fixed(p); guard++)
    {
        SetLineup(p, shop);
        var bench = p.Maids.Where(m => !p.Lineup.Contains(m.Id)).OrderByDescending(m => m.Salary).FirstOrDefault();
        if (bench == null) return;
        p.Dismiss(bench.Id);
    }
}
// "Balanced" = capacity-aware: each purchase answers a measured shortage, with a cash cushion.
void Expand(Profile p)
{
    int cooks = p.FieldedMaids().Count(m => p.Lineup.RoleOf(m.Id) == Role.Kitchen);
    int taskers = p.FieldedMaids().Count(m => RoleTable.For(p.Lineup.RoleOf(m.Id) ?? Role.Stage).TakesTasks);
    int stations = p.Grid.Metrics().StationCount, seats = p.Grid.Metrics().Seats;
    // kitchen first: one station per cook, plus the tea bar once there is a second cook.
    if (stations < Math.Max(2, cooks + 1) && Buy(p, "stove", 3)) return;
    if (cooks >= 2 && Count(p, StationKind.TeaBar) == 0 && Buy(p, "teabar", 3)) return;
    // seats follow the Floor: one waiter turns about four seats (measured on ECON-02b), so a table pair only follows a Floor maid.
    int floor = p.FieldedMaids().Count(m => p.Lineup.RoleOf(m.Id) == Role.Floor);
    if (seats < 4 * floor && Buy(p, "table_2", 3)) return;
    if (seats < 4 * floor && Buy(p, "stool", 3)) return;
    if (cooks >= 2 && Count(p, StationKind.Dessert) == 0) Buy(p, "dessert", 4);
}

// ---------------------------------------------------------------------------------------------

void Report(List<DayRec> recs)
{
    var byDay = recs.GroupBy(r => r.Day).ToDictionary(g => g.Key, g => g.ToList());
    int dead = recs.Count(r => r.Dead);
    Console.WriteLine($"== {tag} seeds={seeds} days={days} dead={dead} lineup={lineupMode} start={startRarity}" + (opt.ContainsKey("setup") ? $" setup={opt["setup"]}" : ""));
    Console.WriteLine("day alive  rev profit   cash lvl  rep maid seat st  serv lost angry turn sat% util  occ  chips taps");
    foreach (var d in new[] { 1, 2, 3, 5, 7, 10, 15, 20, 25, 30, 40, 50 }.Where(d => d <= days))
    {
        if (!byDay.TryGetValue(d, out var g)) { Console.WriteLine($"{d,3}     0"); continue; }
        double k = g.Count;
        double Av(Func<DayRec, double> f) => g.Average(f);
        Console.WriteLine($"{d,3} {g.Count(r => !r.Dead),5} {Av(r => r.Revenue),4:F0} {Av(r => r.Profit),6:F0} {Av(r => r.Cash1),6:F0} {Av(r => r.Level),3:F1} {Av(r => r.Rep1),4:F0} {Av(r => r.Maids),4:F1} {Av(r => r.Seats),4:F1} {Av(r => r.Stations),2:F1} {Av(r => r.Served),4:F1} {Av(r => r.TurnedAway),4:F1} {Av(r => r.Angry),5:F1} {Av(r => r.TurnedAway),4:F1} {Av(r => r.Satisfaction * 100),3:F0} {Av(r => r.WorkerUtil),4:F2} {Av(r => r.SeatOcc),4:F2} {Av(r => r.Chips),5:F0} {Av(r => r.Taps),4:F0} su{Av(r => r.StationUtil),4:F2}");
    }
    // survivors at the horizon, and the median day of death.
    var deaths = recs.Where(r => r.Dead).Select(r => r.Day).OrderBy(x => x).ToList();
    int alive = seeds - deaths.Count;
    Console.WriteLine($"SURVIVE {alive}/{seeds} ({100.0 * alive / seeds:F0}%)  median death day {(deaths.Count == 0 ? "-" : deaths[deaths.Count / 2].ToString())}");
    var last = recs.Where(r => !r.Dead && r.Day == days).ToList();
    if (last.Count > 0)
        Console.WriteLine($"SURVIVOR@{days}: cash {last.Average(r => r.Cash1):F0} (p10 {Pct(last.Select(r => (double)r.Cash1), 0.1):F0}, p90 {Pct(last.Select(r => (double)r.Cash1), 0.9):F0})  profit {last.Average(r => r.Profit):F0}  lvl {last.Average(r => r.Level):F1}  maids {last.Average(r => r.Maids):F1}  rev/served {last.Sum(r => r.Revenue) / (double)Math.Max(1, last.Sum(r => r.Served)):F0}");
    // area under the cash curve across everyone (dead = 0 beyond death) is a robust single score.
    double meanFinalCash = Enumerable.Range(1, (int)seeds).Select(s => (double)(recs.Where(r => r.Seed == s).OrderBy(r => r.Day).LastOrDefault()?.Cash1 ?? 0)).Select(c => Math.Max(0, c)).Average();
    Console.WriteLine($"MEAN-FINAL-CASH(all seeds, dead=0) {meanFinalCash:F0}");
}

double Pct(IEnumerable<double> xs, double q) { var a = xs.OrderBy(x => x).ToList(); return a[Math.Min(a.Count - 1, (int)(q * a.Count))]; }

void WriteCsv(string path, List<DayRec> recs)
{
    bool exists = File.Exists(path);
    var sb = new StringBuilder();
    if (!exists) sb.AppendLine("tag,seed,day,level,rep,cash0,cash1,revenue,profit,wages,rent,ingredients,seats,stations,maids,tasking,served,angry,turned,wrong,sat,chips,taps,workerUtil,seatOcc,stationUtil,arrivals,invested,dead");
    var ci = CultureInfo.InvariantCulture;
    foreach (var r in recs)
        sb.AppendLine(string.Join(",", r.Tag, r.Seed, r.Day, r.Level, r.Rep1, r.Cash0, r.Cash1, r.Revenue, r.Profit, r.Wages, r.Rent, r.Ingredients, r.Seats, r.Stations, r.Maids, r.Tasking, r.Served, r.Angry, r.TurnedAway, r.Wrong, r.Satisfaction.ToString("F3", ci), r.Chips, r.Taps, r.WorkerUtil.ToString("F3", ci), r.SeatOcc.ToString("F3", ci), r.StationUtil.ToString("F3", ci), r.Arrivals.ToString("F3", ci), r.Invested, r.Dead ? 1 : 0));
    File.AppendAllText(path, sb.ToString());
}

class DayRec
{
    public string Tag = ""; public uint Seed; public int Day, Level, Rep0, Rep1, Cash0, Cash1, Revenue, Profit, Wages, Rent, Ingredients;
    public int Seats, Stations, Maids, Fielded, Tasking, Served, Angry, TurnedAway, Wrong, Chips, Taps, Hired, BoughtSeats, BoughtStations, Invested;
    public float Satisfaction, Arrivals, Duration; public double WorkerUtil, SeatOcc, StationUtil; public bool Dead;
}
