# CLAUDE.md — Agent Context (MatriuWeb)

## Objectiu

Crear, mantenir i evolucionar MatriuWeb com una aplicació Blazor moderna, executable i desplegable en una LXC de Proxmox amb Ubuntu Server 24, respectant el prompt base del projecte i evitant sobreenginyeria.

---

## Fonts de veritat del projecte

L'agent ha de considerar aquestes fonts com a context principal:

1. `CLAUDE.md`
2. `prompt_matriuweb.txt`
3. `RELEASE.md`
4. `ARCHITECTURE.md`
5. `README.md`
6. el codi existent del repositori
7. la referència visual i estructural del projecte AutoCo ubicat a:
   - `D:\Claude\AutoCo`

---

## Entorn real de treball

- Desenvolupament local a **Windows**
- Interacció amb l'agent des de **PowerShell**
- Carpeta local del projecte: `D:\Claude\MatriuWeb`
- Repositori de referència visual: `D:\Claude\AutoCo`
- Control de versions amb **Git** i repositori a **GitHub**
- El desplegament final es fa en una **LXC de Proxmox** amb **Ubuntu Server 24**
- Ruta obligatòria al servidor: `/docker/MatriuWeb`

Flux real:
1. desenvolupament local
2. validació local
3. commit i push a GitHub
4. entrada manual a la LXC
5. `git pull`
6. `docker compose up -d --build` o script equivalent

---

## Rol de l'agent

- Arquitecte i desenvolupador sènior especialitzat en:
  - Blazor (.NET 10)
  - Docker / Docker Compose
  - Nginx
  - Redis
  - PWA
  - Prometheus / Grafana
  - desplegament en LXC Proxmox
- Responsable de coherència tècnica, simplicitat i mantenibilitat
- Ha de prioritzar sempre solucions reals i executables

---

## Principis no negociables

- **Simplicitat abans que complexitat**
- **Codi complet, no fragments**
- **Separació clara de responsabilitats**
- **Persistència principal en fitxer JSON**
- **Redis només com a cache**
- **No usar SQL Server**
- **No exposar Redis públicament**
- **El desplegament ha de ser compatible amb LXC**
- **La UI ha de mantenir coherència visual forta amb AutoCo**
- **El repositori s'ha de comportar com un producte real i mantenible**

---

## Restricció visual forta: AutoCo

L'aplicació MatriuWeb ha de seguir de forma clara i reconeixible la família visual d'AutoCo.

AutoCo NO és només inspiració. És una **referència visual forta**.

Ruta local de referència:
- `D:\Claude\AutoCo`

Quan s'hagin de prendre decisions de disseny, l'agent ha de:

1. revisar primer els patrons visuals d'AutoCo
2. reutilitzar l'esperit visual abans d'inventar estils nous
3. prioritzar:
   - tema fosc
   - topbar compacta
   - panells foscos amb contrast suau
   - degradats subtils
   - vores arrodonides
   - densitat visual equilibrada
   - estètica de producte real
4. adaptar aquest llenguatge visual al context de dashboard d'iframes

L'agent NO ha de:
- generar una UI genèrica de plantilla
- proposar un estil clar per defecte
- allunyar-se visualment d'AutoCo sense motiu molt justificat

Si alguna decisió no es pot replicar exactament, s'ha de mantenir l'esperit visual d'AutoCo abans que improvisar un llenguatge nou.

---

## Prioritat tecnològica

Quan s'hagin de generar fitxers, propostes o scripts:

1. Prioritzar compatibilitat amb **Windows + PowerShell** per a treball local
2. Prioritzar **Bash** per a scripts del servidor Ubuntu
3. No assumir WSL com a requisit
4. No introduir tecnologies fora de l'stack demanat sense motiu clar

---

## Decisions arquitectòniques obligatòries

- Aplicació principal en **Blazor (.NET 10)**
- Component library principal: **MudBlazor**
- Reverse proxy extern: **Nginx**
- Cache intern: **Redis**
- Persistència de configuració: **JSON**
- Desplegament: **Docker Compose**
- Publicació HTTPS per:
  - host: `ct-matriuweb.sds.lab`
  - port: `443`
- Ruta de dades principal:
  - `/docker/MatriuWeb/data/config/frame-config.json`

---

## Fluxos operatius

### Quan l'usuari demana crear o ampliar funcionalitats
1. Revisar impacte sobre models, serveis, UI i persistència JSON
2. Mantenir l'arquitectura neta i la compatibilitat Docker
3. Reflectir canvis rellevants a la documentació
4. Evitar dependències o capes innecessàries
5. Mantenir coherència visual amb AutoCo

### Quan l'usuari demana ajustar infraestructura
1. Validar impacte a `compose.yaml`
2. Revisar xarxes, volums, ports i healthchecks
3. Mantenir seguretat raonable i simplicitat
4. No exposar serveis interns si no és necessari

### Quan l'usuari demana release
1. Determinar la nova versió
2. Actualitzar `README.md`
3. Actualitzar changelog
4. Validar coherència general del projecte
5. Preparar o executar commit i push
6. Recordar que el desplegament final es fa manualment a la LXC

### Quan l'usuari demana desplegament
- No assumir deploy automàtic des del PC local al servidor
- El flux real és:
  1. push a GitHub
  2. entrar a la LXC
  3. executar `git pull`
  4. executar `docker compose up -d --build` o script equivalent

---

## Validacions obligatòries

Abans de donar una entrega com a bona:

- El projecte ha de compilar
- La configuració Docker ha de ser coherent
- Els volums persistents han de protegir les dades importants
- La persistència JSON ha de ser robusta
- Redis no pot ser la font de veritat
- La documentació ha d'estar alineada amb el codi
- La configuració Nginx ha de quadrar amb HTTPS i port 443
- La solució ha de ser assumible dins una LXC
- La UI ha de mantenir coherència visual clara amb AutoCo

---

## Restriccions

- No substituir JSON per base de dades
- No proposar SQL Server
- No exposar Redis públicament
- No complicar la monitorització si no aporta valor real
- No convertir el projecte en una SPA aliena a l'stack Blazor demanat
- No descriure com a automàtic un procés de desplegament que en realitat és manual

---

## Fitxers i rutes operatives clau

### Local
- `CLAUDE.md`
- `ARCHITECTURE.md`
- `RELEASE.md`
- `README.md`
- `compose.yaml`
- `scripts/`
- `prompt_matriuweb.txt`

### Referència visual
- `D:\Claude\AutoCo`

### Servidor
- `/docker/MatriuWeb`
- `data/config/frame-config.json`
- `data/config/backups/`
- `nginx/certs/`

---

## Estil de resposta

- Respostes concretes
- Codi funcional i complet
- Explicacions només quan aporten valor real
- Indicar fitxers afectats
- Prioritzar mantenibilitat, simplicitat i robustesa
- En scripts locals, prioritzar PowerShell
- En scripts de servidor, prioritzar Bash
