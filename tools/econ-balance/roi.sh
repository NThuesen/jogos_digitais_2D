#!/usr/bin/env bash
# usage: roi.sh <bin> <seeds> <days> <style> <label> <setup-list...> -- <extra args>
# Paired ROI: for each setup, mean sum(profit, days 2..N) minus the control's, divided into days -> per-day gain and payback.
BIN=$1; SEEDS=$2; DAYS=$3; STYLE=$4; shift 4
SETUPS=(); while [ "$1" != "--" ] && [ -n "$1" ]; do SETUPS+=("$1"); shift; done; shift
OUT=$(mktemp)
dotnet $BIN/EconBalance.dll $SEEDS $DAYS $STYLE none "$@" csv=$OUT tag=control >/dev/null
for s in "${SETUPS[@]}"; do dotnet $BIN/EconBalance.dll $SEEDS $DAYS $STYLE none "$@" setup=$s csv=$OUT tag="${s//,/+}" >/dev/null; done
awk -F, -v seeds=$SEEDS -v days=$DAYS 'NR>1 && $3>=2 {p[$1]+=$9; r[$1]+=$8; s[$1]+=$17; a[$1]+=$18; if($30==1) d[$1]++}
 END{ c=p["control"]/seeds; cr=r["control"]/seeds; n=days-1;
  printf "%-22s %9s %9s %7s %7s %7s %5s\n","setup","profit/d","gain/d","rev/d","served","angry","dead";
  for(k in p){ printf "%-22s %9.1f %9.1f %7.0f %7.1f %7.1f %5d\n", k, p[k]/seeds/n, (p[k]/seeds-c)/n, r[k]/seeds/n, s[k]/seeds/n, a[k]/seeds/n, d[k] } }' $OUT | sort -k3 -n
rm -f $OUT
