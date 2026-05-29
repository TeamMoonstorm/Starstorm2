# Starstorm 2 — PT-BR Translation

> Translation management & audit log for the Brazilian Portuguese localization of the Starstorm 2 mod for Risk of Rain 2.

**Reference (EN wiki):** <https://starstorm2.wiki.gg/>

---

## Rules

1. **EN (American) is the absolute base.** Any divergence, PT is corrected to match EN. Never the opposite.
2. **Thousand separators are mandatory in PT numerals:** `1.200%`, `3.000%`, `1.000-4.000%`.
3. **Every change** must be documented in this file with token, before/after values, and reason.
4. **Lore + DESCRIPTION (briefing/dicas)** must be verified alongside skills — always check both.
5. **Skins + achievement names** with puns/references stay in English (game falls back to EN).

---

## 1. Quick Status Dashboard

| Category | Status | Files | Progress |
|----------|--------|-------|----------|
| **Characters** | ✅ Done | 17/17 files | 100% |
| **Skins** | ✅ Done | 1/1 files | 100% |
| **Unlocks** | ✅ Done | 6/6 files | 100% |
| **ItemsT1** | ✅ Done | 1/1 files | 100% |
| **ItemsT2** | ✅ Done | 1/1 files | 53/53 tokens |
| **ItemsT3** | ✅ Done | 1/1 files | 44/44 tokens |
| **ItemsBoss** | ✅ Done | 1/1 files | 31/31 tokens |
| **ItemsLunar** | ✅ Done | 1/1 files | 29/29 tokens |
| **ItemsCurio** | ✅ Done | 1/1 files | 48/48 tokens |
| **Equip** | ✅ Done | 1/1 files | 37/37 tokens |
| **Entity Names** | ✅ Done | — | 10/10 names decided |
| **Drones** | ✅ Done | 1/1 files | 17/17 tokens |
| **Rules** | ✅ Done | 1/1 files | 25/25 tokens |
| **Variants** | ✅ Done | 1/1 files | 6/6 tokens |
| **WikiFormat** | ✅ Done | 1/1 files | 1/1 tokens |
| **Elites** | ✅ Done | 1/1 files | 27/27 tokens (realinhado com EN) |
| **Difficulty** | ✅ Done | 1/1 files | 8/8 tokens |
| **Events** | ✅ Done | 1/1 files | — |
| **Expansion** | ✅ Done | 1/1 files | — |
| **Interactables** | ✅ Done | 1/1 files | — |
| **Stages** | ✅ Done | 1/1 files | 4/4 tokens |
| **TOTAL** | ✅ **100%** | **53/53 files** | **1057/1057 tokens** |

---

## 2. Characters — Complete Status

### 2.1 Legend

| Status | Meaning |
|--------|---------|
| ✅ | Fully translated, audited against EN, corrections applied |
| ⏳ | Blocked by missing dev content |

### 2.2 Table

| # | Character | Hero Name (PT) | Skills | Briefing | Lore | Notes |
|---|-----------|----------------|--------|----------|------|-------|
| 1 | Executioner | Carrasco | ✅ | ✅ | ✅ | 36/36 tokens; full audit with 8 corrections + lore fix |
| 2 | Executioner 2 | — | ✅ | ✅ | ✅ | Skills: Guilhotina, Consagração, Sangria, Tensão Mortal |
| 3 | Knight | — | ✅ | ✅ | ✅ | Full rewrite (character was reworked) |
| 4 | Warden | Warden | ✅ | ✅ | ✅ | Dev prototype — EN placeholders preserved |
| 5 | Nemesis Captain | Capitão Nêmesis | ✅ | ✅ | ✅ | 6 corrections; obsolete tokens removed |
| 6 | Nemesis Executioner | Executor Nêmesis | ✅ | ✅ | ✅ | Keys realigned to EN pattern |
| 7 | Pyro | — | ✅ | ✅ | ✅ | 28/28 tokens |
| 8 | Nemesis Commando | — | ✅ | ✅ | ✅ | All tokens translated |
| 9 | Borg | Borg | ✅ | ✅ | ✅ | Lore translated (Dr. Rayell audio log) |
| 10 | Cyborg 2 | — | ✅ | ✅ | ⏳ | Lore: `[temp]CyborgLore` — dev placeholder |
| 11 | Chirr | — | ✅ | ✅ | ✅ | Soothing Venom: 450% → 750% (EN fix) |
| 12 | Nemmando | Nêmesis Comando | ✅ | ✅ | ✅ | 5 lore corrections |
| 13 | Nemesis Bandit | Bandido Nêmesis | ✅ | ✅ | ✅ | EN skills are placeholders — PT follows |
| 14 | Nemesis Mercenary | Nêmesis Mercenário | ✅ | ✅ | ✅ | 3 skill damage values corrected |
| 15 | MULE | MULA (PROTÓTIPO BETA) | ✅ | ✅ | ⏳ | 11 corrections; lore empty (dev blocked) |
| 16 | Nemesis Huntress | Caçadora Nêmesis | ✅ | ✅ | ✅ | 8 corrections incl. gender/grammar |
| 17 | DUT | DU-T (PROTÓTIPO BETA) | ✅ | ✅ | ✅ | Created from scratch (was missing) |

### 2.3 Detailed Corrections per Character

#### Executioner (Carrasco)
| Token | Before | After | Reason |
|-------|--------|-------|--------|
| DESCRIPTION | "Rajada de ions" / "alvos sozinhos" | "Rajada de Íons" / "alvo único" | Grammar + EN match |
| OUTRO_FLAVOR | "uma armadura vazia" | "uma casca de armadura vazia" | EN: "an empty shell of armor" |
| PASSIVE_DESC | "infligidos com medo" / "de todas habilidades" | "afetados por medo" / "de todas as habilidades" | Preposition + article |
| IONBLAST_DESC | "multiplas" | "múltiplas" | Accent |
| DASH_DESC | "em todos inimigos" | "em todos os inimigos" | Missing article |
| AXESCEPTER_DESC | "um machado" | "seu machado" | EN: "his axe" |
| ARMOR_NAME | "Fanático Ionizado" | "Juggernaut de Íons" | EN match |
| Lore | "velhice" | "desgaste do tempo" | EN: "wear of time" |

#### Nemesis Captain (Capitão Nêmesis)
| Token | Before | After | Reason |
|-------|--------|-------|--------|
| PASSIVE_NAME / PASSIVE_DESC | Had values | **Removed** | Don't exist in EN (obsolete) |
| ORDERS_DESCRIPTION | Old mechanic description | Rewritten | Match current EN |
| SECONDARY_DESCRIPTION | 1.100% + "perfurante" | 680% + removed "perfurante" | EN values |
| STRESSPASSIVE_DESC | Old mechanic | Rewritten | Match current EN |
| KEYWORD_OVERSTRESS | Incomplete penalties | -30% move, -20 armor, -50% damage | EN match |
| HTML tags | `</style>6%`, `</syle>` | Fixed | Broken markup |

#### MULE (MULA - PROTÓTIPO BETA)
| Token | Before | After | Reason |
|-------|--------|-------|--------|
| NAME | "MULA" | "MULA (PROTÓTIPO BETA)" | Pattern: prototype suffix |
| DESCRIPTION | (empty) | "Uma unidade MULE reutilizada..." | Was missing entirely |
| KEYWORD_OVERCLOCKING | "extender" / "aumenta" | "prolongar" / "aumentar" | Spelling + infinitive |
| KEYWORD_OVERCLOCKING2 | "aumenta" | "aumentar" | Infinitive consistency |
| PASSIVE_DRONE_DESC | Different wording | "ativa seu Drone de Reparo" | EN match |
| KEYWORD_PUNCH_CALIBRATED | "grande quantidade de dano" | "mais dano" | EN match |
| SPIN_NAME / CHARGE_NAME | "Calibração Torque" | "Calibração de Torque" | Missing preposition |
| NET_DESC | "a 12m" / "impedindo eles" | "dentro de 12m" / "impedindo-os" | Range + pronoun |
| KEYWORD_CALIBRATING | "habiliade" / "segundária" / "performar" | "habilidade" / "secundária" / "realizar" | 3 spelling fixes |

#### Nemesis Huntress (Caçadora Nêmesis)
| Token | Before | After | Reason |
|-------|--------|-------|--------|
| NAME | "Nêmesis Caçadora" | "Caçadora Nêmesis" | Pattern: class + Nêmesis |
| SUBTITLE | "Perseguidor Astuto" | "Perseguidora Astuta" | Feminine gender |
| OUTRO | "eles" / "procurando" | "ela" / "encontrando" | Gender + EN: "finding" |
| PASSIVE_DESC | "que pode ser atingido" | "que podem ser atingidos" | Plural |
| PRIMARY_BOW_NAME | "Raio Poderoso" | "Flecha Pesada" | EN: "Heavy Bolt" |
| EXPLOSIVE_BOW_NAME | "Raio Explosivo" | "Flecha Explosiva" | EN: "Explosive Bolt" |
| KEYWORD_REARM | "cargas" | "projéteis" | EN: "bolts" |
| UTILITY_DESC | "se teleporte a" | "se teleporte por" | Preposition |

#### Nemmando (Nêmesis Comando) — Lore
| Before | After |
|--------|-------|
| "pistola do trabalho" | "pistola de serviço" |
| "cargas de carregamento que desembarcaram" | "cargas que pousaram" |
| "ninguém os fará" | "ninguém os levará" |
| "Ele diz" | "Ele diria" |
| Tokens `{230}`, `{180}` | `{0}` |
| "causão" | "causam" |

#### Nemesis Mercenary (Nêmesis Mercenário)
| Token | Before | After | Reason |
|-------|--------|-------|--------|
| Devitalize damage | 300% | **600%** | EN value |
| Exploit damage | 1.500% | **1.200%** | EN value |
| Extirpate | Wrong values/format | Corrected | EN match |
| Tour de Force | Inline CETRO | Separate line `CETRO:` | EN format |

#### Chirr
| Token | Before | After | Reason |
|-------|--------|-------|--------|
| Soothing Venom damage | 450% | **750%** | EN value |
| Lore key | `SS2_CHIRR_LORE` | `SS2_CHIRR_BODY_LORE` | Correct EN key |
| Unicode | `\\u201C` | `\u201C` | Escaped vs literal |
| Lore grammar | "foi dado" | "foi dada" | Gender agreement |

#### DUT
| Token | Value |
|-------|-------|
| NAME | "DU-T (PROTÓTIPO BETA)" |
| DESCRIPTION | "Este é um protótipo cru do DU-T. Bugado, mas utilizável." |

> Created from scratch — PT file was missing entirely.

---

## 3. Skins

**File:** `SS2Lang_Skins_pt.json` — ✅ Complete (10/10)

### Tokens Added (were missing)
| Token | Value |
|-------|-------|
| SS2_SKIN_COMMANDO_ALT1 | Shock |
| SS2_SKIN_COMMANDO_ALT2 | Scorpion |
| SS2_SKIN_COMMANDO_ALT3 | Praetor |

### Tokens Kept (already existed)
Predecessor (Nostalgic), Furtivo (Stealth), Clandestino (Clandestine), Construto (Construct), Arma (Weapon), Prestígio (Prestige), Calcanhar (Heel)

---

## 4. Achievements (Unlocks)

### 4.1 UnlocksVanilla_pt — Corrections
| Token | Before | After |
|-------|--------|-------|
| CAPTAIN_GRANDMASTERY_DESC | "Captitão" | "Capitão" |

### Tokens Added
| Token | EN | PT |
|-------|----|----|
| RECOLOR1_COMMANDO_NAME | Commando: Scorpion | Comando: Escorpião |
| RECOLOR1_COMMANDO_DESC | As Commando, find the hidden Prismatic Crystal on Titanic Plains. | Como Comando, encontre o Cristal Prismático oculto nas Planícies Titânicas. |

### 4.2 UnlocksNemmando_pt — Corrections
| Token | Before | After | Reason |
|-------|--------|-------|--------|
| MASTERY_DESC | "vença o jogo aniquile" | "vença o jogo **ou** aniquile" | Missing "ou" |
| BOSSATTACK_DESC | "inflija 50 acúmulos de Goivar em um único inimigo" | "mate um inimigo com 10 acúmulos de Goivar ou mais" | Wrong condition + value |

### 4.3 UnlocksItems_pt — Corrections
| Token | EN (correct) | PT (before) |
|-------|-------------|-------------|
| ERRATICGADET_NAME | Lord of Lightning | ESMAAAAGA!! |
| ERRATICGADET_DESC | Carry 5 items that create lightning | Carregue 10 itens que aumentem a chance de acerto crítico |
| MALICE_DESC | Complete the Stage 3 Teleporter Event on Typhoon difficulty | Alcance o estágio 3 na dificuldade Tufão |

### Achievements Kept in EN (puns/references — game falls back)
> One Punch, Too Much Stuff!, Pest Control, Going Long, Sooo Toxic, Overloaded, Another Bite, Assemble, tRet 12 Pro Max Plus, Grand Prix, I Am The Storm, Apex Predator, Even More Ethereal

### 4.4 UnlocksArtifacts_pt — Added
| Token | Value |
|-------|-------|
| HAVOC_NAME | Trial of Havoc |
| HAVOC_DESC | Complete the Trial of Havoc |

### 4.5 UnlocksCyborg (Borg)
✅ Already translated via `UnlocksBorg_pt.json` (same `SS2_ACHIEVEMENT_BORG_*` tokens). File named differently from EN but content matches.

---

## 5. Items & Equipment — Status

### 5.1 ItemsT1 (White — 90 tokens) ✅

**File:** `SS2Lang_ItemsT1_pt.json`

**Items:** Armed Backpack, Blood Tester, Coffee Bag, Detritive Trematode, Diary, Dormant Fungus, Fork, Guarding Amulet, Malice, Molten Coin, Nanobots, Needles, X-4 Stimulant, Santa's Hat, Chuckling Fungus, Universal Charger, Uranium Horseshoe, Ice Tool, Miracle Lightbulb

#### Corrections Applied
| Token | Before | After | Reason |
|-------|--------|-------|--------|
| Nanobots (4 tokens) | (missing) | All 4 tokens created | Item was entirely absent |
| Universal Charger PICKUP | "recarda" | "recarga" | Typo |
| Chuckling Fungus DESC | "dentro de<style>" | "dentro de <style>" | Missing space before tag |
| Coffee Bag DESC | Missing "Maximum cap" line + style errado (mov. speed em cIsDamage) | Linha adicionada + `velocidade de movimento` em cIsUtility | EN match |
| Blood Tester PICKUP | "com base no ouro" | "com base no ouro." | Missing period |
| Diary Consumed (3 tokens) | (missing) | NAME/DESC/PICKUP criados | Item was entirely absent |
| Blood Tester DESC | "em {1}..." (sem "de saúde") | "{1} de saúde" | missing noun |
| Coffee Bag PICKUP | sem ponto final | ponto adicionado | punctuation |
| Diary LORE | "em por" / "cerca de cem páginas" / "coisa .\"" | "por" / "cerca de cem páginas depois" / "coisa.\"" | 3 Portuguese grammar fixes |
| Chuckling Fungus DESC | "1 segundos" | "1 segundo" | singular/plural agreement |
| Chuckling Fungus LORE | "oroborreana" | "ouroboriana" | spelling |
| Molten Coin DESC | Structure: "Scales" dentro do cStack | Separado em cIsUtility próprio | EN structure match |
| Detritive Trematode DESC | Missing "por segundo" | Added "por segundo" | EN match |
| Detritive Trematode LORE | "Trematóide" (inconsistente) | "Trematódeo" | consistency with NAME |
| Dormant Fungus LORE | "Espécime 18305" (sem hífen) | "Espécime-18305" | consistency with earlier entries |
| Ice Tool PICKUP | sem ponto final | ponto adicionado | punctuation |

#### Notes
- Coffee Bag lore: address/named changed (Kentucky→Pensilvânia, Rose→Eleni) — flagged but left untouched (may be intentional localization).
- Nanobots LORE "lembra do meme?" kept as intentional creative choice by the translator.

### 5.2 ItemsT2 (Green — 53 tokens) ✅

**File:** `SS2Lang_ItemsT2_pt.json`

**Items:** Blast Knuckles, Cryptic Source, Field Accelerator, Hottest Sauce, Hunter's Sigil, Jet Boots, Man-o'-War, Low Quality Speakers, Neurotoxin Gland, Sticky Overloader, Strange Can, Watch Metronome, Rainbow Root

#### Todas as Correções (23 + 13 tokens adicionados)
| Categoria | Qtd | Detalhes |
|-----------|-----|----------|
| Erros de português corrigidos | 21 | Verbo sem R, espaços antes de pontuação, tags HTML quebradas, "têm" sem acento, "incedeia"/"incendeia", "damage"/"dano", "bonus"/"bônus", "acumula acumula", etc. |
| Conteúdo errado corrigido | 2 | Rainbow Root DESC/PICKUP (mecânica completamente diferente), Cryptic Source LORE (inventado → custom "O mundo choca quando se para pra ver") |
| Conteúdo faltante adicionado | 3 | Hunter's Sigil DESC ("to all allies"), Low Quality Speakers DESC ("and destroy projectiles"), Watch Metronome DESC (2ª sentença) |
| Itens inteiros adicionados | 3 | Blast Knuckles (4 tok.), Neurotoxin Gland (5 tok.), Sticky Overloader (4 tok.) |

### 5.3 ItemsT3 (Red — 44 tokens) ✅
**File:** `SS2Lang_ItemsT3_pt.json`

**Items:** Baby's Toys, Composite Injector, Droid Head, Erratic Gadget, Green Chocolate, Insecticide, Nkota's Heritage, Portable Reactor, Swift Skateboard, Bane Flask, Galvanic Core

#### Corrections Applied
| Token | Before | After | Reason |
|-------|--------|-------|--------|
| Erratic Gadget PICKUP | "Relampagos" | "Relâmpagos" | Accent |
| Erratic Gadget LORE | Trailing space / "Relator A" / ".. ......" | Fixed / "Voz A" / "..........." | Grammar + format |
| Green Chocolate DESC | "do sua" / "acumulativo" | "da sua" / removed | Grammar + EN match |
| Green Chocolate LORE | "vocês está" / "só está tem" | "você está" / "só tem" | Grammar |
| Insecticide DESC | "per stack" | "por acúmulo" | EN→PT translation |
| Nkota's Heritage DESC | "items" | "itens" | EN→PT translation |
| Portable Reactor DESC | Missing "invencibilidade total e 100% de velocidade de movimento" | Added full sentence | EN match |
| Skateboard LORE | Dev note placeholder | Full translation of poetic lore (two brothers, two pages) | EN match |
| Bane Flask PICKUP | "Efeitos Negativos espalham..." | "Ataques aplicam maldição. Cada efeito negativo..." | EN match |
| Galvanic Core (4 tok.) | (missing) | NAME/DESC/PICKUP/LORE created | Item was entirely absent |

### 5.4 ItemsBoss (Boss — 31 tokens) ✅
**File:** `SS2Lang_ItemsBoss_pt.json`

**Items:** Augury, Scavenger's Fortune, Condemned Bond (VoidRock), Haunted Lamp (ShackledLamp), Stirring Soul, Moxie, Remuneration

#### Corrections Applied
| Token | Before | After | Reason |
|-------|--------|-------|--------|
| Augury DESC | "que não sejam chefe" | "que não são chefes" | Plural agreement |
| Shackled Lamp PICKUP | "Disapre" / "uso" | "Dispare" / "usos" | Spelling + plural |
| Stirring Soul LORE | Malformed `<color= #FFCCED>` + `</link>` | `<link=\"textWavy\">` + `</link>` | Broken HTML tag |
| Moxie PICKUP | "Quer mais?" | "Quanto mais?" | EN: "How much more?" |
| Remuneration DESC | "proximo" | "próximo" | Accent |
| Remuneration TERMINAL_CONTEXT | "Transação Concluída" | "Concluir transação" | EN: "Complete transaction" (imperative) |
| Remuneration LORE | "além pequena" / "quert" | "além de pequena" / "quer" | Missing preposition + typo |
| Condemned Bond (5 tok.) | (missing) | NAME/PICKUP/DESC/LORE/FAILURE created | Item + token were entirely absent |

### 5.5 ItemsLunar (Lunar — 29 tokens) ✅
**File:** `SS2Lang_ItemsLunar_pt.json`

**Items:** Relic of Duality, Relic of Extinction, Relic of Force, Relic of Mass, Relic of Termination, Relic of Echelon, Primal Birthright

#### Corrections Applied
| Token | Before | After | Reason |
|-------|--------|-------|--------|
| Duality DESC | "alcúmulo" / "congelo" | "acúmulo" / "congela" | Spelling + verb conjugation |
| Duality LORE | "ardor" / "cuidadoso" | "aquecimento" / "cuidado" | EN: "warmth" / "taken care of" |
| Extinction PICKUP | "Descrição em construção" | Translation of EN PICKUP | Was placeholder despite EN having real text |
| Force DESC | `{0}%` (wrong param index) | `{1}%` | EN uses `{1}` for damage, `{0}` for cooldown |
| Termination DESC | "nao" | "não" | Accent |
| Termination LORE | "cruel outro viajante" | "cruel para outro viajante" | Missing preposition |
| Echelon LORE | "preprarar" | "preparar" | Spelling |
| Primal Birthright (4 tok.) | (missing) | NAME/DESC/PICKUP/LORE created | Item was entirely absent |

### 5.6 ItemsCurio (Curio — 48 tokens) ✅
**File:** `SS2Lang_ItemsCurio_pt.json` — Created from scratch.

**Items:** Bleedout, DebuffMissiles, EliteDamageBonus, HitList, ItemOnBossKill, ItemOnEliteKill, MaxHpOnKill, OptionFromChest, ScrapFromChest, ShellPiece, ShieldGate, SnakeEyes

All 48 tokens translated. LORE entries keep `"???"` (matching EN placeholder).

### 5.7 Equip (Equipment — 37 tokens) ✅
**File:** `SS2Lang_Equip_pt.json`

**Items:** Back Thruster, Cloaking Headband, Greater Warbanner, Simple Magnet, M.I.D.A.S., Pressurized Canister, Rock Fruit, White Flag, Morleys (NemCapEquip), SKILL_DISABLED

#### Corrections Applied
| Token | Before | After | Reason |
|-------|--------|-------|--------|
| Back Thruster LORE | "HOMEN" / "HOMEM 1 :" | "HOMEM" / "HOMEM 1:" | Spelling + space |
| Greater Warbanner DESC | "um bandeira" | "uma bandeira" | Gender agreement |
| MIDAS LORE | "Perfeio" | "Perfeito" | Missing T |
| Pressurized Canister NAME | "Recipiente Pressurizador" | "Recipiente Pressurizado" | "Pressurizador" = pressurizer (wrong) |
| Pressurized Canister DESC | "Ganhando" | "Ganhe" | Imperative mood (EN match) |
| Simple Magnet (4 tok.) | (missing) | All 4 tokens created | File was entirely absent |
| Rock Fruit (4 tok.) | (missing) | All 4 tokens created | File was entirely absent |
| White Flag (4 tok.) | (missing) | All 4 tokens created | File was entirely absent |
| SKILL_DISABLED (2 tok.) | (missing) | Both tokens created | File was entirely absent |

---

## 6. Pending Decisions

### 6.1 Entity / Enemy Name Translations — Completed

| Enemy | EN | PT | Decision |
|-------|----|----|----------|
| Archer Bug | Archer Bug | Inseto Arqueiro | ✅ Translated |
| Clay Monger | Clay Monger | Mercador de Argila | ✅ Translated |
| Security Chest (Mimic) | Security Chest | Baú de Segurança | ✅ Translated |
| Hero's Shade | Hero's Shade | Sombra do Herói | ✅ Translated |
| Ultra Mithrix | Ultra Mithrix | Ultra Mithrix | Kept in EN (proper noun) |
| Wayfarer | Wayfarer | Viajante | ✅ Translated |
| Follower | Follower | Adepto | ✅ Translated |
| Agarthan | Agarthan | Agarthan | Kept in EN (proper noun) |
| Rushrum | Rushrum | Fungo Veloz | ✅ Translated |
| Zanzan the Faded | Zanzan the Faded | Zanzan o Fadado | ✅ Translated |

### 6.2 Blocked Items

| Item | Reason |
|------|--------|
| Cyborg 2 lore | Dev placeholder `[temp]CyborgLore` — no real EN text |
| MULE lore | Dev left `""` empty — no EN text to translate |

---

## 7. Key Decisions Log

| Date | Decision |
|------|----------|
| — | Thousand separators (dots) mandatory in PT even when EN omits them |
| — | Nêmesis naming pattern: class/role first (e.g., "Capitão Nêmesis", not "Nêmesis Capitão") |
| — | Prototype characters: suffix "(PROTÓTIPO BETA)" |
| — | Puns/references in achievements: keep in EN (game falls back to EN token) |
| — | Lore address/name divergences flagged but left untouched — may be intentional localization choices |
| Entity names | All 10 entity names translated; proper nouns (Ultra Mithrix, Agarthan) kept in EN |

---

## 8. Next Steps

- [x] **ItemsT1**: ✅ Complete. 16 corrections including Nanobots + Diary Consumed.
- [x] **ItemsT2**: ✅ Complete. 23 Portuguese fixes + 13 tokens added (Blast Knuckles, Neurotoxin Gland, Sticky Overloader).
- [x] **ItemsT3**: ✅ Complete. 15 Portuguese fixes + Galvanic Core (4 tok.) added.
- [x] **ItemsBoss**: ✅ Complete. 9 corrections + Condemned Bond (5 tok.) added.
- [x] **ItemsLunar**: ✅ Complete. 9 corrections + Primal Birthright (4 tok.) added.
- [x] **ItemsCurio**: ✅ Complete. Created from scratch (48 tokens).
- [x] **Equip**: ✅ Complete. 5 corrections + Magnet/Rock Fruit/White Flag/SKILL_DISABLED (14 tok.) added.
- [x] **Entity Names**: All 10 names decided and translated.
- [x] **SurvivorNemExecutioner**: File restored (was corrupted by external AI). Keys realigned to `SS2_NEMEXECUTIONER_*` pattern.
- [x] **Elites**: Realigned with EN (added Gilded, Stormborn, Infernal, Boreal, Supercharged, Tectonic + equipment aspects).
- [x] **UnlocksItems**: All missing achievement tokens added (24 tokens for 12 achievements).
- [x] **UnlocksBorg**: Added CYBORG key aliases for compatibility.
- [x] **Drones**: New PT file created (17 tokens: Shock, Duplicator, Security drones).
- [x] **Rules**: New PT file created (25 tokens: damage/armor rules).
- [x] **Variants**: New PT file created (6 tokens: engineer, bandit, railgunner variant skills).
- [x] **WikiFormat**: New PT file created (1 token).
- [x] **Stages_pt**: Fixed lore (was divergent from EN), added missing log token.
- [x] **UnlocksChirr_pt**: Fixed trailing comma (JSON syntax error).
- [ ] **All files**: Final grammar pass across all PT files.
