# MatriuWeb · v0.1.10

Dashboard de monitorització web amb matriu configurable d'iframes. Blazor (.NET 10), MudBlazor, Docker Compose, Nginx HTTPS, Redis, Prometheus i Grafana. Desplegada en una LXC Proxmox amb Ubuntu Server 24.

---

## Stack

| Capa | Tecnologia |
|------|-----------|
| Frontend | Blazor Server (.NET 10) + MudBlazor |
| Reverse proxy | Nginx 1.27 — HTTPS, port 443 |
| Cache | Redis 7 — intern, no exposat |
| Persistència | JSON (`data/config/frame-config.json`) |
| Monitorització | Prometheus + Grafana + node_exporter + cAdvisor + redis-exporter |
| PWA | Service Worker + manifest |

---

## Funcionalitats principals

- **Dashboard configurable** — matriu d'iframes amb layout adaptatiu (1–6+ frames)
- **Vista enfocada** — frame principal gran + la resta en sidebar lateral
- **Perfils** — múltiples perfils de configuració, canvi en temps real des de la topbar
- **Gestió de frames** — afegir, editar, eliminar, reordenar (botons ↑↓), activar/desactivar
- **Detecció d'estat** — probe HTTP de servidor detecta `X-Frame-Options`/CSP i URLs inaccessibles
- **Auto-refresh** — global i per frame, pausable des de la topbar
- **Import/Export** — configuració en JSON, validada abans d'aplicar
- **Backups** — automàtics en cada desa, amb timestamp; restauració des de la UI
- **Mode kiosk** — `/kiosk`, pantalla completa, rotació automàtica entre frames
- **PWA** — instal·lable com a aplicació d'escriptori
- **Health endpoint** — `/health` en JSON per a healthchecks i Prometheus
- **Mètriques** — `/metrics` (prometheus-net) per a integració Grafana
- **Grafana integrat** — accessible via `/grafana/`, embedding permès

---

## Requisits previs (LXC)

- Ubuntu Server 24 LTS
- Docker + Docker Compose plugin
- Git
- Certificats SSL a `nginx/certs/server.crt` i `nginx/certs/server.key`
- Ruta: `/docker/MatriuWeb`

---

## Estructura del projecte

```
MatriuWeb/
├── compose.yaml                      # Stack Docker complet
├── .env.example                      # Variables d'entorn de referència
├── .gitattributes                    # Normalització de finals de línia
├── .gitignore
├── README.md · CLAUDE.md · ARCHITECTURE.md · RELEASE.md · BOOTSTRAP.md
├── scripts/
│   ├── bootstrap-lxc.sh             # Prepara la LXC (Docker + carpetes)
│   ├── deploy.sh                    # git pull + docker compose up
│   ├── backup-config.sh             # Backup manual
│   ├── restore-config.sh            # Restauració des de backup
│   ├── logs.sh                      # Consulta de logs
│   ├── nginx-create-htpasswd.sh     # Genera .htpasswd per basic auth /config
│   ├── check.ps1                    # Validació local (Windows/PowerShell)
│   └── release.ps1                  # Script de release (Windows/PowerShell)
├── nginx/
│   ├── conf.d/matriuweb.conf        # HTTPS, rate limiting, gzip, basic auth
│   └── certs/                       # Certificats SSL (no al git)
├── monitoring/
│   ├── prometheus/prometheus.yml
│   └── grafana/
│       ├── provisioning/
│       │   ├── datasources/         # Prometheus auto-provisionat
│       │   └── dashboards/          # Proveïdor de dashboards
│       └── dashboards/
│           ├── host.json            # CPU, RAM, disc, xarxa (node_exporter)
│           ├── docker.json          # CPU/RAM per contenidor (cAdvisor)
│           ├── redis.json           # Clients, memòria, hit rate
│           └── overview.json        # Vista general de la plataforma
├── data/                            # Dades persistents (no al git)
│   ├── config/
│   │   ├── frame-config.json        # Configuració principal (versionada)
│   │   └── backups/
│   ├── redis/ · grafana/ · prometheus/
└── src/
    └── MatriuWeb/
        ├── Components/
        │   ├── Layout/MainLayout.razor    # Topbar, tema fosc, selector de perfil
        │   ├── Pages/
        │   │   ├── Dashboard.razor        # Grid d'iframes + vista enfocada
        │   │   ├── Config.razor           # Perfils, frames, import/export, backups
        │   │   ├── Kiosk.razor            # Pantalla completa amb rotació
        │   │   └── Health.razor           # Estat visual dels components
        │   └── Shared/
        │       ├── FramePanel.razor       # Iframe + detecció d'estat + overlays
        │       ├── ProfilesPanel.razor    # CRUD de perfils i frames
        │       ├── ImportExportPanel.razor
        │       └── BackupsPanel.razor
        ├── Models/   FrameConfig · FrameProfile · FrameItem · HealthStatus
        ├── Services/ JsonPersistence · Backup · FrameConfiguration ·
        │             Redis · DashboardState · IframeStatus · Health
        ├── wwwroot/css/app.css            # Tema fosc família AutoCo
        └── wwwroot/js/app.js             # JS interop: initFrame, openTab
```

---

## Primera posada en marxa (LXC)

```bash
# 1. Pujar el script de bootstrap a la LXC i executar-lo
bash bootstrap-lxc.sh

# 2. Clonar el repositori
cd /docker/MatriuWeb
git clone https://github.com/<usuari>/MatriuWeb.git .

# 3. Col·locar els certificats SSL
#    nginx/certs/server.crt  (cadena completa)
#    nginx/certs/server.key

# 4. Copiar i ajustar les variables d'entorn (opcional)
cp .env.example .env

# 5. Arrencar l'stack
docker compose up -d --build

# 6. Verificar
docker compose ps
curl -k https://localhost/health
```

Grafana: `https://ct-matriuweb.sds.lab/grafana/` — credencials inicials `admin/admin` (**canvia-les!**)

---

## Actualitzacions

```bash
cd /docker/MatriuWeb
git pull
docker compose up -d --build
```

O amb el script:
```bash
bash scripts/deploy.sh
```

---

## Configuració SSL / Nginx

Els certificats han d'estar a `nginx/certs/` (no s'inclouen al repositori):
- `server.crt` — certificat (cadena completa si cal)
- `server.key` — clau privada

Nginx escolta al port `443` amb TLS 1.2+. Headers de seguretat inclosos (HSTS, X-Content-Type-Options, etc.).

**Protecció de /config** (opcional) — basic auth:
```bash
bash scripts/nginx-create-htpasswd.sh admin "LaTevaContrasenya"
```
Descomenta les línies `auth_basic` a `nginx/conf.d/matriuweb.conf` i reinicia Nginx.

---

## Persistència JSON

```
data/config/frame-config.json    ← font de veritat
data/config/backups/             ← backups automàtics (timestamp)
```

Robustesa:
- Si el fitxer no existeix → configuració mínima per defecte (2 frames)
- Si és corrupte → restauració automàtica de l'últim backup vàlid
- Mínim de 2 frames sempre garantit

**Redis NO és la font de veritat.** Falla en silenci sense perdre dades.

---

## Backups i restauració

```bash
# Backup manual
bash scripts/backup-config.sh

# Llistar backups
bash scripts/restore-config.sh

# Restaurar un backup concret
bash scripts/restore-config.sh frame-config-20250101-120000.json
docker compose restart web
```

Des de la UI: `Config → Backups → Restaurar`.

---

## Monitorització (Grafana + Prometheus)

| Dashboard | UID | Contingut |
|-----------|-----|-----------|
| Overview | `mw-overview` | CPU/RAM host, HTTP req/s, serveis actius |
| Host / LXC | `mw-host` | CPU, RAM, disc, xarxa, load |
| Contenidors Docker | `mw-docker` | CPU/RAM/xarxa per contenidor |
| Redis | `mw-redis` | Clients, memòria, hit rate, comandes/s |

Els dashboards s'autoprovisionen en arrencar Grafana.

**Embedding de Grafana en iframes:**
Grafana ja té `GF_SECURITY_ALLOW_EMBEDDING=true` i `cookie_samesite=none`. Des de MatriuWeb, afegeix una URL com `https://ct-matriuweb.sds.lab/grafana/d/mw-overview` com a frame.

**Consideracions LXC/Proxmox:**
- Activa `nesting=1` a les opcions de l'LXC al Proxmox per a cAdvisor
- node_exporter munta el filesystem del host com a read-only a `/host`
- Les mètriques de cgroups poden tenir limitacions si la LXC no té accés complet

---

## PWA

MatriuWeb és instal·lable com a PWA (escriptori, tauleta, mòbil):
- Assets propis en cache (CSS, JS, MudBlazor)
- Els iframes externs **no** es garanteixen offline
- Compatible amb mode kiosk

---

## Seguretat

- `/config` protegit per rate limiting (5r/s) i opció de basic auth
- Redis i exporters de monitorització a la xarxa `internal` (no accessibles des de l'exterior)
- Grafana i Prometheus no s'exposen directament (accessible via `/grafana/`)
- TLS 1.2+ amb cipher suites moderns
- HSTS actiu

---

## Flux de treball (resum)

```
[Windows — PowerShell]
  .\scripts\check.ps1              # validació local
  git add . && git commit -m "..."
  git push

[LXC — manual]
  cd /docker/MatriuWeb
  git pull
  docker compose up -d --build
```

El desplegament és **sempre manual**. No hi ha CI/CD automàtic.

---

## Changelog

### v0.1.10 — 2026-04-24
#### Millores
- `RedisCacheService`: eliminat ping per operació — ara usa `IsConnected` (latència reduïda)
- `Dashboard`: layout 3 frames en fila única (era 2+1 amb cel·la buida)
- `FramePanel`: URL de refresh robusta davant fragments `#`
- `BackupService`: `PruneOldBackupsAsync` asíncron real amb `Task.Run`

### v0.1.9 — 2026-04-24
#### Correccions crítiques
- `Program.cs`: afegit `UseForwardedHeaders` + DataProtection persistent — fix circuit Blazor SignalR darrere nginx HTTPS
- `nginx/conf.d/matriuweb.conf`: `map $http_upgrade $connection_upgrade` per a gestió correcta del WebSocket; `Host $host` sense port
- `compose.yaml`: volum `data/keys` per persistir claus DataProtection entre reinicis del contenidor
- `Models/FrameConfig.cs`: URLs per defecte canviades de `localhost:3000/8080` a `https://example.com`
- `Services/IframeStatusService.cs`: clau de cache Redis ara usa la URL directament (era `GetHashCode()`, no determinista entre reinicis)
- `scripts/bootstrap-lxc.sh`: afegit `data/keys`, eliminats directoris de monitorització (stack independent)

### v0.1.8 — 2026-04-24
#### Infraestructura
- `nginx/conf.d/matriuweb.conf`: eliminats bloc `stub_status` (port 8081) i location `/grafana/` — serveis extrets a stack independent

### v0.1.7 — 2026-04-24
#### Correccions
- Healthcheck web: `wget --spider` (HEAD) → `wget -O /dev/null` (GET) per evitar falsos negatius amb Kestrel

### v0.1.6 — 2026-04-24
#### Infraestructura
- `compose.yaml` simplificat: eliminats Alertmanager, Prometheus, Grafana, node-exporter, cAdvisor, nginx-exporter i redis-exporter (stack de monitorització independent)
- Serveis inclosos: `web`, `nginx`, `redis`

### v0.1.5 — 2026-04-24
#### Scripts
- `scripts/deploy.sh` reescrit: backup pre-deploy, detecció de canvis git, espera de serveis healthy amb timeout, log persistent a `logs/deploy.log`, suport `--force`

### v0.1.4 — 2026-04-24
#### Correccions
- `Program.cs`: `UseMetricServer(port:8080)` → `MapMetrics("/metrics")` — eliminat conflicte de port amb Kestrel que impedia arrencar la web
- `alertmanager.yml`: receptor `null` natiu per defecte (sintaxi `${VAR:-default}` no suportada pel parser YAML d'Alertmanager)
- `data/alertmanager`: afegits permisos `777` al bootstrap (UID 65534)

#### Canvis d'entorn
- Entorn de desplegament: `ct-matriuweb.sds.lab` (172.25.1.12), port HTTPS 443
- Eliminada monitorització del `compose.yaml` principal (stack independent)

#### Fase 7
- Drag & drop per reordenar frames (HTML5 `draggable`)
- Export de configuració descàrrega fitxer JSON directament al navegador
- Cache Redis per a lectures de `FrameConfigurationService` (TTL 5 min, invalidació en escriptura)
- Alertmanager integrat a Prometheus
- `.gitignore`: afegit `.claude/`

### v0.1.0 — 2026-04-23
#### Nova funcionalitat
- Dashboard configurable amb matriu d'iframes (1–6+ frames, layout adaptatiu)
- Vista enfocada amb sidebar
- Múltiples perfils de configuració, selector a la topbar
- Gestió de frames: CRUD, reordenació, activació/desactivació
- Detecció d'estat d'iframe: probe HTTP servidor (X-Frame-Options, CSP, inaccessible)
- Overlays visuals per estat error/bloqueig/càrrega
- Import/export de configuració en JSON
- Backups automàtics amb timestamp; restauració des de la UI i scripts
- Mode kiosk `/kiosk` amb rotació automàtica i controls de navegació
- PWA instal·lable (manifest + service worker)
- Endpoints `/health` i `/metrics` per Prometheus
- Dashboards Grafana provisionats: overview, host, docker, redis

#### Infraestructura
- Nginx amb TLS 1.2+, gzip, rate limiting, basic auth per `/config`
- Redis amb maxmemory 128mb LRU
- Xarxa Docker `internal:true` per a serveis interns
- Prometheus retention 30 dies

#### Família visual
- Tema fosc basat en AutoCo (ardosia #020617/#1e293b/#0f172a, accent blau #3b82f6)
- Topbar compacta, panells foscos amb vores arrodonides, densitat visual equilibrada
