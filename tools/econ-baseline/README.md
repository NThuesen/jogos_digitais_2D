# ECON-01A — runner de diagnóstico

Requer SDK .NET 8+ com runtime .NET 8 e Python 3.10+. Não usa Unity, NUnit, NuGet externo, saves ou cópias de fórmulas econômicas. O projeto compila diretamente `Assets/_Game/Scripts/Sim/**/*.cs`, como o simtest existente; não o altera.

Na raiz do checkout:

```sh
dotnet run --project tools/econ-baseline/EconBaseline.csproj -- Logs/econ-run1
python3 tools/econ-baseline/report.py Logs/econ-run1
dotnet run --no-build --project tools/econ-baseline/EconBaseline.csproj -- Logs/econ-run2
python3 tools/econ-baseline/report.py Logs/econ-run2
```

Os argumentos do programa são somente o diretório de saída (relativo ou absoluto). Cada execução gera `days.csv`; o agregador gera `RESULTS.md` e `metadata.json`. Compare os três arquivos das duas pastas, por exemplo `cmp` em Linux ou `fc /b` no Windows, e compare `days.csv` com [a evidência versionada](../../docs/economy/econ-01a/days.csv). Falhas de simulação, ledger inconsistente, timeout, seeds repetidas e séries incompletas encerram com erro.

[Relatório e metodologia](../../docs/economy/econ-01a/README.md) · [Tabelas geradas](../../docs/economy/econ-01a/RESULTS.md).

A execução desta entrega usou SDK 10.0.400, runtime 8.0.14 no Windows, target `net8.0`. Na sessão WSL um wrapper temporário apenas traduziu caminhos absolutos para Windows antes de chamar o dotnet instalado. Ele não é necessário em checkout nativo Linux/Windows. Compilação com SDK 8 especificamente não foi executada nesta máquina; não há APIs ou sintaxe superiores ao target/SDK 8 no projeto.

Validação existente, sem alterar runner ou expectativas:

```sh
bash tools/simtest.sh --noresult
python3 tools/ci/verify.py boundaries
```

Para metadados, SHA do CSV usa bytes; SHA de fontes/harness agrega `caminho_relativo + NUL + SHA256(texto UTF-8 com newline LF) + LF`, na ordem de caminhos do script. O SHA do código de produção é a revisão imutável de entrada, enquanto o hash de fontes verifica o conteúdo efetivamente lido. Nenhum timestamp entra na saída determinística. As seeds exatas são registradas em cada linha.
