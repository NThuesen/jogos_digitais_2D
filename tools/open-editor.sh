#!/usr/bin/env bash
# Abre o editor no projeto, sem passar pelo Unity Hub.
#
# O Hub é um Electron inteiro cujo único trabalho é clicar "abrir", e nesta máquina —
# 15 GB de RAM com 975 MB de swap — foi ele que o kernel matou por falta de memória. O
# editor sozinho abre igual e custa algumas centenas de MB menos.
#
# Também checa o que a CLAUDE.md do workspace manda checar: um editor já rodando, e um
# Temp/UnityLockfile velho sem editor vivo.
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
editor="$HOME/Unity/Hub/Editor/6000.5.9f1/Editor/Unity"

if [[ ! -x "$editor" ]]; then
    echo "Editor não encontrado em $editor" >&2
    exit 1
fi

if pgrep -x Unity >/dev/null; then
    echo "Já tem um editor rodando. Esta máquina não aguenta dois." >&2
    pgrep -ax Unity >&2
    exit 1
fi

lock="$root/Temp/UnityLockfile"
if [[ -f "$lock" ]]; then
    echo "Removendo um Temp/UnityLockfile velho (nenhum editor vivo)."
    rm -f "$lock"
fi

# O editor deste projeto estabiliza perto de 3 GB. Abaixo disso o kernel começa a matar
# processos em vez de avisar, e foi o que aconteceu com o Hub.
avail_mb=$(awk '/MemAvailable/ {print int($2/1024)}' /proc/meminfo)
swap_free_mb=$(awk '/SwapFree/ {print int($2/1024)}' /proc/meminfo)

printf 'Memória disponível: %s MB (swap livre: %s MB)\n' "$avail_mb" "$swap_free_mb"

if (( avail_mb < 3500 )); then
    echo
    echo "Isso é pouco para o editor. Os maiores consumidores agora:"
    ps -eo rss,comm --sort=-rss | awk 'NR>1 && NR<8 {printf "  %6.0f MB  %s\n", $1/1024, $2}'
    echo
    echo "Fecha o navegador e o VS Code primeiro, ou roda a build pronta com tools/serve.sh,"
    echo "que não precisa de editor nenhum."
    read -r -p "Abrir mesmo assim? [s/N] " answer
    [[ "$answer" == "s" || "$answer" == "S" ]] || exit 1
fi

echo "Abrindo $root ..."

# Burst entra de carona na URP, pesa 748 MB de pacote, carrega o próprio gerador de código
# no editor — e este projeto não tem um único [BurstCompile] nem um IJob. Desligar a
# compilação não muda nada do que roda e tira do editor o processo que mais aloca.
exec "$editor" -projectPath "$root" --burst-disable-compilation
