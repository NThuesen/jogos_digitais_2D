#!/usr/bin/env bash
# ECON-02b matrix: every play style x every investment policy, for the PR #72 baseline and the ECON-02b
# candidate, each with the current roster cap ("base") and the PR #20 cap ("plus1").
# usage: run-all.sh [seeds=100] [days=40]      (bin dirs are git-ignored: tools/.econ-bin-*)
cd "$(dirname "$0")"
S=${1:-100}; D=${2:-40}
for eco in econ02 econ02b; do
  for cap in base plus1; do
    dotnet build -nologo -v quiet -clp:ErrorsOnly -p:CapMode=$cap $([ $eco = econ02 ] && echo -p:Economy=econ02) -o ../.econ-bin-$eco-$cap EconBalance.csproj || exit 1
    echo "######## economy=$eco cap=$cap"
    ./matrix.sh ../.econ-bin-$eco-$cap $S $D "idle lazy casual pro" "none conservative balanced full hire hire-excess tables-only overexpand legacy"
  done
done
