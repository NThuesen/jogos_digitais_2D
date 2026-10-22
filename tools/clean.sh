#!/usr/bin/env bash
# Limpa o que faz o editor entrar em loop de crash, e o cache que ele reconstrói sozinho.
#
# O loop: quando o editor morre, ele salva a cena aberta em Temp/__Backupscenes/ e uma
# cópia em Assets/_Recovery/. Na abertura seguinte ele encontra as duas, importa, carrega,
# e se morrer de novo escreve mais uma — Assets/_Recovery/ chegou a ter "0.unity" e
# "0 (1).unity", um por crash. Apagar isso quebra o ciclo.
#
# Nada aqui é fonte: Temp/ e Library/ são gerados e estão no .gitignore, e as cenas de
# recuperação são cópias de Assets/_Game/Scenes/Boot.unity, que está commitada.
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"

if pgrep -x Unity >/dev/null; then
    echo "Fecha o editor primeiro — ele está rodando." >&2
    exit 1
fi

freed_before=$(df --output=avail -BM / | tail -1 | tr -dc '0-9')

echo "== quebrando o loop de recuperação =="
for path in "Assets/_Recovery" "Assets/_Recovery.meta" "Temp"; do
    if [[ -e "$path" ]]; then
        printf '  %-24s %s\n' "$path" "$(du -sh "$path" 2>/dev/null | cut -f1)"
        rm -rf "$path"
    fi
done

echo "== cache que o editor reconstrói =="
# Bee é o backend de build (só precisa na hora de buildar), BurstCache é de um compilador
# que este projeto não usa, e ShaderCache volta na primeira vez que algo é desenhado.
for path in "Library/Bee" "Library/BurstCache" "Library/ShaderCache"; do
    if [[ -e "$path" ]]; then
        printf '  %-24s %s\n' "$path" "$(du -sh "$path" 2>/dev/null | cut -f1)"
        rm -rf "$path"
    fi
done

freed_after=$(df --output=avail -BM / | tail -1 | tr -dc '0-9')

echo
printf 'Liberado: %s MB. Livre agora: %s MB.\n' \
    "$(( freed_after - freed_before ))" "$freed_after"
echo
echo "A primeira build depois disso leva alguns minutos a mais (o Bee volta do zero)."
echo "Os testes e o editor continuam rápidos: a Library de assets não foi tocada."
