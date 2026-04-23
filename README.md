# MatriuWeb · v0.1.0

Dashboard de monitorització web amb matriu configurable d'iframes. Blazor (.NET 10), MudBlazor, Docker Compose, Nginx, Redis, Prometheus i Grafana. Pensada per funcionar en una LXC Proxmox amb Ubuntu Server 24.

---

## Stack

| Capa | Tecnologia |
|------|-----------|
| Frontend | Blazor Server (.NET 10) + MudBlazor |
| Reverse proxy | Nginx 1.27 (HTTPS, port 4444) |
| Cache | Redis 7 (intern, no exposat) |
| Persistència | JSON (`data/config/frame-config.json`) |
| Monitorització | Prometheus + Grafana + node_exporter + cAdvisor + redis-exporter |
| PWA | Service Worker + manifest.webmanifest |

---

## Requisits previs (LXC)

- Ubuntu Server 24 LTS
- Docker + Docker Compose plugin
- Certificats SSL a `nginx/certs/server.crt` i `nginx/certs/server.key`
- Ruta de desplegament: `/docker/MatriuWeb`

---

## Estructura del projecte

```
MatriuWeb/
├── compose.yaml                  # Stack Docker complet
├── .env.example                  # Variables d'entorn (còpia a .env)
├── .gitignore
├── README.md
├── CLAUDE.md                     # Context de l'agent
├── ARCHITECTURE.md
├── RELEASE.md
├── scripts/
│   ├── bootstrap-lxc.sh          # Prepara la LXC (Docker + carpetes)
│   ├── deploy.sh                 # git pull + docker compose up
│   ├── backup-config.sh          # Backup manual de la configuració
│   ├── restore-config.sh         # Restauració des de backup
│   ├── logs.sh                   # Consulta de logs
│   ├── check.ps1                 # Validació local (Windows/PowerShell)
│   └── release.ps1               # Script de release (Windows/PowerShell)
├── nginx/
│   ├── conf.d/matriuweb.conf     # Configuració Nginx
│   └── certs/                    # Certificats SSL (no al git)
├── monitoring/
│   ├── prometheus/prometheus.yml
│   └── grafana/
│       ├── provisioning/
│       │   ├── datasources/      # Datasource Prometheus auto-provisionat
│       │   └── dashboards/       # Proveïdor de dashboards
│       └── dashboards/           # JSON dels dashboards
├── data/                         # Dades persistents (no al git)
│   ├── config/
│   │   ├── frame-config.json     # Configuració principal
│   │   └── backups/              # Còpies de seguretat
│   ├── redis/
│   ├── grafana/
│   └── prometheus/
├── logs/                         # Logs (no al git)
└── src/
    └── MatriuWeb/                # Projecte Blazor
        ├── Components/
        │   ├── Layout/
        │   ├── Pages/
        │   └── Shared/
        ├── Models/
        ├── Services/
        ├── wwwroot/
        ├── Program.cs
        ├── appsettings.json
        └── MatriuWeb.csproj
```

---

## Execució local (Windows)

```powershell
# Clonar o obrir el repositori
cd D:\Claude\MatriuWeb

# Compilar i verificar
.\scripts\check.ps1

# Executar l'aplicació (requereix Redis local o Docker)
cd src\MatriuWeb
dotnet run
```

Per a l'stack complet en local amb Docker:
```powershell
docker compose up -d --build
```
Accés: `https://localhost:4444` (requereix certificats locals)

---

## Desplegament a la LXC

### 1. Primera vegada
```bash
bash /tmp/bootstrap-lxc.sh      # Instal·la Docker i crea carpetes
cd /docker/MatriuWeb
git clone <url-del-repo> .
cp .env.example .env
# Col·loca els certificats:
#   nginx/certs/server.crt
#   nginx/certs/server.key
docker compose up -d --build
```

### 2. Actualitzacions
```bash
cd /docker/MatriuWeb
git pull
docker compose up -d --build
```
O directament:
```bash
bash scripts/deploy.sh
```

---

## Configuració SSL / Nginx

Els certificats han d'estar a `nginx/certs/`:
- `server.crt` — certificat (cadena completa si cal)
- `server.key` — clau privada

Aquesta carpeta **no s'inclou al repositori** (`.gitignore`).

La configuració de Nginx escolta al port `4444` amb HTTPS i fa de proxy invers cap al servei Blazor intern al port `8080`.

Grafana és accessible via `/grafana/` (intern, via proxy Nginx).

---

## Persistència JSON

La configuració de l'aplicació es desa a:
```
data/config/frame-config.json
```

Regles de robustesa:
- Si el fitxer no existeix → es crea una configuració mínima per defecte (2 frames)
- Si el fitxer és corrupte → intenta restaurar l'últim backup vàlid de `data/config/backups/`
- Si no hi ha backup vàlid → crea una configuració mínima segura i registra l'error

**Redis NO és la font de veritat.** Redis és cache i pot fallar sense perdre dades.

---

## Redis

- Servei intern (`redis:6379`), no exposat públicament
- Si Redis no arranca o és inaccessible, l'aplicació segueix funcionant (degraded mode)
- Dades persistides amb `appendonly yes`

---

## PWA

MatriuWeb és instal·lable com a PWA:
- Manifest: `wwwroot/manifest.webmanifest`
- Service worker: caché d'assets estàtics propis
- Els iframes externs **no** es garanteixen offline (depenen de les seves URLs)
- Compatible amb mode kiosk

---

## Monitorització amb Grafana / Prometheus

### Accés
- Grafana: `https://tuteapps.ddns.net:4444/grafana/` (credencials: `admin/admin` — canvia-les!)
- Prometheus (intern): `http://prometheus:9090` (no exposat públicament)

### Exportadors inclosos
| Servei | Port intern | Mètriques |
|--------|-------------|-----------|
| node_exporter | 9100 | CPU, memòria, disc, xarxa del host |
| cAdvisor | 8080 | Contenidors Docker |
| redis-exporter | 9121 | Connexions, memòria, comandes Redis |

### Embedding de Grafana en iframes
Grafana ja té `GF_SECURITY_ALLOW_EMBEDDING=true` al `compose.yaml`.

Per permetre embedding des d'un origen extern, configura a Grafana:
```ini
[security]
allow_embedding = true
cookie_secure = true
cookie_samesite = none
```
O via env vars al compose:
```yaml
GF_SECURITY_COOKIE_SAMESITE: none
GF_SECURITY_COOKIE_SECURE: "true"
```

Limitació: alguns navegadors bloquegen cookies de tercers en iframes. Grafana és accessible directament via `/grafana/` des del mateix domini, la qual cosa evita aquest problema.

### Consideracions LXC / Proxmox
- cAdvisor i node_exporter poden tenir mètriques limitades si la LXC no té accés complet a cgroups
- Per a cgroups v2, verifica que la LXC tingui `features: nesting=1` al Proxmox
- `node_exporter` munta el filesystem de l'host com a read-only

---

## Backups i restauració

### Backup automàtic
Cada operació de desar configuració des de la UI crea un backup automàtic a `data/config/backups/`.

### Backup manual
```bash
bash scripts/backup-config.sh
```

### Llistar backups
```bash
bash scripts/restore-config.sh
```

### Restaurar un backup
```bash
bash scripts/restore-config.sh frame-config-20250101-120000.json
docker compose restart web
```

### Recuperació de JSON corrupte
Si el fitxer principal és corrupte i no es pot restaurar des de la UI:
```bash
bash scripts/restore-config.sh  # llistat de backups disponibles
bash scripts/restore-config.sh <nom-del-backup>
docker compose restart web
```

---

## Seguretat

- `/config` s'ha de protegir a nivell de Nginx (basic auth o restricció IP)
  - Documentació a `nginx/conf.d/matriuweb.conf`
- Redis no s'exposa públicament
- Grafana i Prometheus no s'exposen directament (accessibles via proxy intern)
- Canvia les credencials per defecte de Grafana en producció

---

## Flux d'actualització des de GitHub

```
[Desenvolupament local Windows]
    → scripts/check.ps1
    → git commit + git push
        → [LXC: git pull + docker compose up -d --build]
```

El desplegament al servidor és **sempre manual**. No hi ha CI/CD automàtic.

---

## Changelog

### v0.1.0
- Estructura inicial del repositori
- Stack Docker complet (Blazor, Nginx, Redis, Prometheus, Grafana)
- Esquelet Blazor amb tema fosc (família visual AutoCo)
- Serveis de persistència JSON, backups, cache Redis
- Dashboard, Config, Kiosk i Health
- PWA configurable
- Scripts operacionals
