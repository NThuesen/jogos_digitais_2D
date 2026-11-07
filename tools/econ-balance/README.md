# ECON-02 — curva de dinheiro por estilo de jogo

Roda carreiras completas (`Profile.NewGame` → `DayPlan.Build` → `Shift` → `Books.Close`) com escalação natural e três estilos:
`idle` (nenhum toque), `casual` (toque a cada 1,2 s, acerta metade das faixas douradas) e `pro` (robô perfeito).

```sh
dotnet run --project tools/econ-balance/EconBalance.csproj -- 40 40 casual   # seeds dias estilo
```

Imprime sobrevivência, receita, lucro, nível, caixa e fichas por dia, e a linha `DEAD` de cada empresa que quebrou.
Resultados e decisões: seção 10 do [GDD](../../docs/GDD.md).
