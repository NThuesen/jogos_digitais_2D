# econ-balance — carreiras completas sobre o `Neko.Sim` real

Simulador de economia (ECON-02 → ECON-02b). Compila as regras de `Assets/_Game/Scripts/Sim` (nada é reimplementado)
e joga carreiras: `Profile.NewGame` → manhã (caixas, contratação, compras, escalação) → `DayPlan` → `Shift` → `Books.Close`.

```sh
dotnet run --project tools/econ-balance/EconBalance.csproj -- <seeds> <dias> <estilo> <política> [opções]
```

* estilo: `idle` · `lazy` (olha a cada 4 s, ~20 toques/dia) · `casual` (1,2 s, ~35) · `pro` (robô perfeito)
* política: `none` · `legacy` (o robô "invest" do ECON-02) · `conservative` · `balanced` (só expande, guiado pela capacidade)
  · `full` (balanced + contrata por função) · `hire` · `hire-excess` · `tables-only` · `overexpand`
* opções: `setup=stove:1,t2:2,maids:1` (ablação no dia 0) · `start=common|rare|epic|legendary` · `rep=200` (começa no nível 3)
  · `lineup=auto|smart|random` · `skiprole=Stage` · `csv=arquivo` · `tag=nome` · `noprune`

Interruptores de compilação (não tocam o jogo): `-p:CapMode=plus1` usa o cap da PR #20 (`alt/CafeLevelTable.plus1.cs`);
`-p:Economy=econ02` usa as regras de demanda/reputação da PR #72 (`alt/DayPlan.econ02.cs`), o baseline de ECON-02b.

**RNG:** a semente de cada dia é função pura de (semente, dia) e compras/caixas/candidatas usam outro fluxo; duas políticas
na mesma semente veem os mesmos dias e só divergem pelo estado que as próprias decisões produzem. Mudar a taxa de chegada
muda quantos clientes são sorteados, então baseline × candidata são pareadas por empresa inicial e dias, não por cliente.

Scripts: `matrix.sh` (uma linha por estilo×política) · `roi.sh` (ROI pareado de uma compra) · `levels.sh` (dia em que cada nível é alcançado)
· `run-all.sh`. Relatórios: [ECON-02](../../docs/economy/econ-02/README.md), [ECON-02b](../../docs/economy/econ-02b/README.md).
