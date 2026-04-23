# BOOTSTRAP.md — Inici del projecte MatriuWeb amb Claude

## Objectiu

Aquest fitxer serveix per arrencar MatriuWeb amb el context correcte des del primer prompt.

La idea és que Claude treballi des de `D:\Claude\MatriuWeb`, però tenint en compte:

- el prompt base del projecte
- el flux real Windows → GitHub → LXC
- la restricció visual forta basada en AutoCo a `D:\Claude\AutoCo`

---

## Fitxers que Claude ha de llegir primer

Abans de generar cap codi, Claude ha de llegir:

1. `CLAUDE.md`
2. `ARCHITECTURE.md`
3. `RELEASE.md`
4. `prompt_matriuweb.txt`

I, quan necessiti prendre decisions visuals o de llenguatge UI, ha de revisar la referència local:
- `D:\Claude\AutoCo`

---

## Primer prompt recomanat

Enganxa aquest text a Claude en iniciar el projecte:

```text
Llegeix primer CLAUDE.md, ARCHITECTURE.md, RELEASE.md i prompt_matriuweb.txt.

Aquest projecte s'ha de desenvolupar a D:\Claude\MatriuWeb.
La referència visual forta és el projecte AutoCo ubicat a D:\Claude\AutoCo.
No tractis AutoCo com una inspiració dèbil: és una restricció forta de llenguatge visual.

Treballa respectant aquest flux real:
- desenvolupament local a Windows amb PowerShell
- commit i push a GitHub
- desplegament manual posterior a la LXC Ubuntu 24 amb git pull i docker compose up -d --build

Ara vull que generis només la fase 1 del projecte:
1. proposta d'estructura final del repositori
2. fitxers base de documentació
3. compose.yaml inicial
4. esquelet inicial de la solució Blazor
5. estructura de nginx, monitoring, scripts i data

No generis encara tota la lògica final de negoci ni tots els fitxers detallats de monitorització si abans no has deixat l'estructura base coherent.
Indica clarament quins fitxers crees i per què.
```

---

## Estratègia recomanada de construcció

Per obtenir un resultat millor, demana el projecte per fases:

### Fase 1 — Estructura base
- estructura de carpetes
- `compose.yaml`
- `.env.example`
- `.gitignore`
- `README.md`
- esquelet de Blazor
- esquelet de Nginx
- esquelet de monitoring
- scripts base

### Fase 2 — Nucli funcional
- models
- serveis
- persistència JSON
- configuració de perfils
- backups
- health

### Fase 3 — UI real
- dashboard
- configuració
- kiosk
- estats visuals d'iframe
- coherència visual forta amb AutoCo

### Fase 4 — Infraestructura i observabilitat
- Prometheus
- Grafana
- exporters
- dashboards provisionats
- healthchecks
- endureixement de Nginx

### Fase 5 — Release i deploy
- scripts finals
- ajustos de README
- revisió de ports, volums i persistència
- comprovació final del flux GitHub → LXC

---

## Regles d'ús

- Si Claude tendeix a generar massa de cop, talla el treball per fases
- Si la UI se separa del llenguatge visual d'AutoCo, corregeix-ho immediatament
- Si proposa una base de dades, rebutja-ho
- Si exposa Redis o serveis interns, corregeix-ho
- Si assumeix deploy automàtic, recorda que el flux és manual a la LXC

---

## Segon prompt recomanat després de la fase 1

```text
Continua amb la fase 2.
Mantén el llenguatge visual i arquitectònic definit.
No simplifiquis cap decisió obligatòria del prompt base.
Quan hi hagi diverses opcions vàlides, prioritza la més simple, robusta i mantenible.
```

---

## Nota final

La qualitat del resultat dependrà molt de mantenir aquestes tres coses sempre visibles:

- prompt base del projecte
- context d'entorn real
- restricció visual forta basada en AutoCo
