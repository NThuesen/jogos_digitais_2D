"""Compare ECON-01B with frozen ECON-01A rows. No balance formulas."""
import csv
import hashlib
import json
import statistics
import sys
from pathlib import Path

root = Path(__file__).resolve().parents[3]
baseline = root / 'docs/economy/econ-01a/days.csv'
current = Path(sys.argv[1])
a = list(csv.DictReader(baseline.open(encoding='utf-8')))
b = list(csv.DictReader(current.open(encoding='utf-8')))
result = {'baseline_sha256': hashlib.sha256(baseline.read_bytes()).hexdigest(),
          'current_sha256': hashlib.sha256(current.read_bytes()).hexdigest(), 'scenarios': {}}
for name in ['passive', 'mixed_passive', 'box_addict', 'mixed_box_addict']:
    old = [r for r in a if r['scenario'] == name]
    new = [r for r in b if r['scenario'] == name]
    assert len({r['seed'] for r in new}) == 60
    summary = {}
    if name.endswith('passive'):
        strip_cap = lambda rows: [{k:v for k,v in r.items() if k != 'cap'} for r in rows]
        assert strip_cap(old) == strip_cap(new), 'Unexpected passive regression'
        summary['identical_except_cap'] = True
    else:
        assert all(r['roster'] == '4' for r in new if r['day'] == '1')
    for label, rows in [('before', old), ('after', new)]:
        first = [r for r in rows if r['day'] == '1']
        summary[label] = dict(days=len(rows), day1_roster=sorted({int(r['roster']) for r in first}),
            day1_mean_wages=statistics.mean(int(r['wages']) for r in first),
            median_bankruptcy_day=statistics.median(int(r['day']) for r in rows if r['bankrupt'] == '1'),
            survivors_day7=sum(r['day'] == '7' and r['bankrupt'] == '0' for r in rows),
            boxes=sum(int(r['boxes']) for r in rows), duplicates=sum(int(r['duplicates']) for r in rows))
    result['scenarios'][name] = summary
print(json.dumps(result, indent=2))
