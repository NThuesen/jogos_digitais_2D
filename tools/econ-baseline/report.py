"""Aggregate real ledger rows; no economic formulas. Python 3 standard library only."""
import csv, hashlib, json, statistics, sys
from pathlib import Path

root = Path(__file__).resolve().parents[2]
out = Path(sys.argv[1])
rows = list(csv.DictReader((out / 'days.csv').open(encoding='utf-8')))
assert all(None not in r for r in rows), 'Malformed CSV'
policies = ['competent', 'learner', 'passive', 'box_addict', 'wrong']
scenarios = list(dict.fromkeys(r['scenario'] for r in rows))
lines = ['# ECON-01A — resultados gerados', '', 'Seeds 1–60 nas cinco políticas; mixed: i×2654435761 mod 2³² (i=1–60), pareadas entre políticas; 1–200 no dia 1; 1–100 nos oito planos de compra.', '',
         '| Cenário | n empresas | n dias | sobreviventes 7/20/50 | falência mediana (n) | L2 (n) |', '|---|---:|---:|---|---|---:|']
for scenario in scenarios:
    group = [r for r in rows if r['scenario'] == scenario]
    seeds = sorted({int(r['seed']) for r in group})
    expected = sorted((i * 2654435761) & 0xffffffff for i in range(1,61)) if scenario.startswith('mixed_') else list(range(1, (60 if scenario in policies else 200 if scenario == 'day1' else 100)+1))
    assert seeds == expected
    failures = [int(r['day']) for r in group if r['bankrupt'] == '1']
    alive = [sum(any(int(r['seed']) == s and int(r['day']) == day and r['bankrupt'] == '0' for r in group) for s in seeds) for day in [7,20,50]]
    l2 = len({r['seed'] for r in group if int(r['level_end']) >= 2})
    med = statistics.median(failures) if failures else 'NA'
    lines.append(f'| {scenario} | {len(seeds)} | {len(group)} | {"/".join(map(str,alive)) if scenario != "day1" else "NA"} | {med} ({len(failures)}) | {l2} |')
    for seed in seeds:
        trace = [r for r in group if int(r['seed']) == seed]
        assert [int(r['day']) for r in trace] == list(range(1,len(trace)+1))
        assert trace[-1]['bankrupt'] == '1' or len(trace) == (1 if scenario == 'day1' else 50)
    first = [r for r in group if r['day']=='1']
    assert len({r['shift_seed'] for r in first}) == len(seeds), 'Repeated shift seeds'
    assert len({r['opener'] for r in first}) == len(seeds), 'Repeated opener fingerprints'
lines += ['', 'Sobrevivência = caixa positivo depois do fechamento do dia indicado; sem imputar dias após falência.', '',
          '| Cenário/faixa | n empresas | n dias | receita | salários | aluguel | ingredientes | lucro | caixa final | satisfação | rep/dia | fichas/dia | fichas saldo | nível final |', '|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|']
fields = ['revenue','wages','rent','ingredients','profit','cash_end','satisfaction','reputation_delta','points_earned','points','level_end']
for policy in policies + ['mixed_' + p for p in policies] + ['day1']:
    for lo,hi in [(1,1),(2,5),(2,50)]:
        group=[r for r in rows if r['scenario']==policy and lo <= int(r['day']) <= hi]
        if not group: continue
        means=[f'{statistics.mean(float(r[f]) for r in group):.3f}' for f in fields]
        lines.append(f'| {policy} {lo}–{hi} | {len({r["seed"] for r in group})} | {len(group)} | '+ ' | '.join(means)+' |')
day1=[r for r in rows if r['scenario']=='day1']
lines += ['', 'Dia 1 (n=200): '+json.dumps({f:statistics.median(float(r[f]) for r in day1) for f in ['revenue','profit','wages','rent','ingredients']})+'.',
          f'Prejuízo dia 1: {sum(int(r["profit"])<0 for r in day1)}/200.', '',
          '| Compra dia 1 | n | gasto | vivos dia 10 | Δ lucro médio dia 1 vs controle (pareado) |', '|---|---:|---:|---:|---:|']
control={r['seed']:r for r in rows if r['scenario']=='purchase_0' and r['day']=='1'}
for i in range(8):
    group=[r for r in rows if r['scenario']==f'purchase_{i}']
    first=[r for r in group if r['day']=='1']
    delta=statistics.mean(int(r['profit'])-int(control[r['seed']]['profit']) for r in first)
    alive=sum(r['day']=='10' and r['bankrupt']=='0' for r in group)
    lines.append(f'| purchase_{i} | {len(first)} | {first[0]["purchases"]} | {alive} | {delta:.3f} |')
(out/'RESULTS.md').write_text('\n'.join(lines)+'\n',encoding='utf-8', newline='\n')
files=sorted(root.glob('Assets/_Game/Scripts/Sim/**/*.cs'))+[root/'Assets/_Game/Scripts/Game/Core/GameSession.cs']
hash_sources=lambda paths: hashlib.sha256(''.join(f'{p.relative_to(root).as_posix()}\0{hashlib.sha256(p.read_text(encoding='utf-8').replace('\r\n','\n').encode()).hexdigest()}\n' for p in paths).encode()).hexdigest()
metadata=dict(production_commit='7d42ab1f7dd3981c56defdd0b3edaee08918749f',step_seconds=0.05,max_days=50,rows=len(rows),
              days_sha256=hashlib.sha256((out/'days.csv').read_bytes()).hexdigest(),production_sources_sha256=hash_sources(files),
              harness_sha256=hash_sources([root/'tools/econ-baseline/Program.cs',root/'tools/econ-baseline/EconBaseline.csproj',Path(__file__).resolve()]),
              seed_checks='PASS: expected seed sets, unique day-one shift seeds and opener fingerprints in each scenario')
(out/'metadata.json').write_text(json.dumps(metadata,indent=2)+'\n',encoding='utf-8', newline='\n')
print(json.dumps(metadata,indent=2))
