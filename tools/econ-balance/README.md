# ECON-02 — curva de dinheiro por estilo de jogo

Roda carreiras completas (`Profile.NewGame` → `DayPlan.Build` → `Shift` → `Books.Close`) com quatro estilos:
`idle` (nenhum toque), `lazy` (olha a cada 4 s), `casual` (a cada 1,2 s, acerta metade das faixas douradas) e `pro` (robô perfeito).

```sh
dotnet run --project tools/econ-balance/EconBalance.csproj -- 40 40 casual yes   # seeds dias estilo investir
```

Imprime sobrevivência, receita, lucro, nível, caixa, elenco, folha, lugares, fichas e toques por dia.
Resultados e decisões: seção 10 do [GDD](../../docs/GDD.md).

## v2 — carreiras completas e cap alternativo

`dotnet run --project tools/econ-balance/EconBalance.csproj -- <seeds> <dias> <idle|lazy|casual|pro> <yes|no> [v]` (último argumento `yes/no` = investir: caixas, contratação, móveis). `-p:CapMode=plus1` troca `CafeLevelTable` pela versão da PR #20. `run-all.sh` roda tudo. Relatório: [ECON-02](../../docs/economy/econ-02/README.md).
