#!/usr/bin/env bash
# usage: levels.sh <bin> <seeds> <days> <style> <policy> [args]  -> median day each level is first reached (rep level at end of day)
BIN=$1; S=$2; D=$3; ST=$4; PO=$5; shift 5
T=$(mktemp); dotnet $BIN/EconBalance.dll $S $D $ST $PO "$@" csv=$T tag=x >/dev/null
awk -F, 'NR>1{ if(!( ($2 SUBSEP $4) in f) ){ } if($4>=2&&!(($2,2) in f)){f[$2,2]=$3} if($4>=3&&!(($2,3) in f)){f[$2,3]=$3} if($4>=4&&!(($2,4) in f)){f[$2,4]=$3} if($4>=5&&!(($2,5) in f)){f[$2,5]=$3} if($4>=6&&!(($2,6) in f)){f[$2,6]=$3} }
 END{ for(l=2;l<=6;l++){ n=0; for(s=1;s<=ARGV_S;s++) if((s,l) in f) a[++n]=f[s,l]; asort(a); printf "L%d: reached %d/%d median day %s | ", l, n, ARGV_S, (n? a[int((n+1)/2)] : "-"); delete a } print "" }' ARGV_S=$S $T
rm -f $T
