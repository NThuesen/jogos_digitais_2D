#!/usr/bin/env bash
# usage: matrix.sh <bin-dir> <seeds> <days> "<styles>" "<policies>" [extra args...]
# One summary line per (style, policy): survival, median death day, survivor cash/level/maids, mean final cash.
BIN=$1; SEEDS=$2; DAYS=$3; STYLES=$4; POLS=$5; shift 5
for st in $STYLES; do for po in $POLS; do
  dotnet $BIN/EconBalance.dll $SEEDS $DAYS $st $po "$@" tag=$st/$po | awk -v st=$st -v po=$po '
    /^SURVIVE/ {s=$2; md=$0; sub(/.*median death day /,"",md)}
    /^SURVIVOR/ {sv=$0; sub(/^SURVIVOR@[0-9]+: /,"",sv)}
    /^MEAN-FINAL/ {m=$NF}
    END {printf "%-7s %-13s surv %-8s dday %-3s mean$ %-6s | %s\n", st, po, s, md, m, sv}'
done; done
