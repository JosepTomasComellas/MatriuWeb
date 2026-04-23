# ARCHITECTURE.md — MatriuWeb

## Objectiu

Descriure l'arquitectura tècnica, el flux de dades, els components d'infraestructura i les decisions operatives del projecte MatriuWeb.

---

## Resum executiu

MatriuWeb és una aplicació web Blazor (.NET 10) desplegada amb Docker Compose, publicada mitjançant Nginx amb HTTPS i pensada per funcionar de manera lleugera i mantenible dins una LXC de Proxmox amb Ubuntu Server 24.

La configuració funcional de l'aplicació es desa en JSON persistent. Redis s'utilitza només com a cache. La monitorització es basa en Grafana i Prometheus amb exportadors adequats.

La interfície s'ha de construir com una reinterpretació fidel de la família visual d'AutoCo, adaptada a un dashboard de monitorització.

---

## Entorns

### Desenvolupament local
- Sistema operatiu: **Windows**
- Shell habitual: **PowerShell**
- Repositori local: `D:\Claude\MatriuWeb`
- Referència visual local: `D:\Claude\AutoCo`

### Entorn de desplegament
- Plataforma: **Proxmox**
- Tipus: **LXC**
- Sistema operatiu: **Ubuntu Server 24**
- Ruta de desplegament: `/docker/MatriuWeb`

---

## Flux real de treball

1. Desenvolupament local a Windows
2. Interacció amb l'agent des de PowerShell
3. Validació local
4. Commit i push a GitHub
5. Entrada manual a la LXC
6. `git pull`
7. `docker compose up -d --build`

Aquest flux és la referència de treball del projecte.

---

## Components principals

### Aplicació web
- **Blazor (.NET 10)**
- **MudBlazor**
- PWA instal·lable
- Dashboard configurable basat en iframes

### Persistència
- Font de veritat: fitxer JSON persistent
- Ruta objectiu:
  - `/docker/MatriuWeb/data/config/frame-config.json`
- Backups:
  - `/docker/MatriuWeb/data/config/backups/`

### Cache
- **Redis**
- Només intern dins la xarxa Docker
- No exposat públicament

### Reverse proxy
- **Nginx**
- HTTPS
- Port públic `4444`
- Host públic `tuteapps.ddns.net`

### Monitorització
- **Grafana**
- **Prometheus**
- **node_exporter**
- **cAdvisor**
- **redis-exporter**
- **nginx-exporter** només si compensa en simplicitat/valor

---

## Decisió de monitorització

Per defecte, es recomana una sola solució `compose.yaml` amb tots els serveis principals i de monitorització, perquè:

- simplifica el desplegament
- facilita manteniment en una sola LXC
- encaixa millor amb un entorn lleuger
- evita fragmentació operativa

Si més endavant la monitorització creix molt, es podrà separar en un stack específic.

---

## Estructura funcional prevista

```text
MatriuWeb/
├── compose.yaml
├── .env.example
├── .gitignore
├── README.md
├── CLAUDE.md
├── ARCHITECTURE.md
├── RELEASE.md
├── BOOTSTRAP.md
├── scripts/
├── nginx/
├── monitoring/
├── data/
├── logs/
└── src/
    └── MatriuWeb/
```

---

## Capa d'aplicació

Serveis mínims previstos:

- `FrameConfigurationService`
- `JsonPersistenceService`
- `BackupService`
- `RedisCacheService`
- `DashboardStateService`
- `IframeStatusService`
- `ProfileService`
- `HealthService`

Aquests serveis han de mantenir separació de responsabilitats i evitar acoblaments innecessaris.

---

## Decisions tècniques clau

- Persistència robusta amb inicialització mínima segura
- Recuperació davant JSON corrupte
- Mínim de 2 frames sempre garantit
- Refresh global i per frame
- Perfil actiu configurable
- Backups amb timestamp
- Embedding de Grafana documentat i configurable

---

## Restricció visual

La UI de MatriuWeb ha de seguir el llenguatge visual d'AutoCo com a restricció forta.

Això implica:

- tema fosc principal
- degradats subtils i profunditat visual
- topbar compacta
- panells foscos amb contrast suau
- vores suaus i arrodonides
- jerarquia visual clara
- aspecte modern, net i professional
- accents de color controlats
- ús elegant de MudBlazor

S'ha d'evitar:

- UI genèrica de plantilla
- estètica clara per defecte
- disseny que no sembli de la mateixa família visual

---

## Seguretat

- `/config` s'ha de protegir a nivell de Nginx
- Redis no s'ha d'exposar
- Grafana i Prometheus només s'han d'exposar si és realment necessari
- La seguretat ha de ser raonable però sense complicar innecessàriament el desplegament

---

## Publicació

Configuració prevista de Nginx:

- `listen 4444 ssl;`
- `server_name tuteapps.ddns.net;`
- reverse proxy cap al servei web intern
- suport per headers forwarded
- suport correcte per connexions persistents de Blazor

Certificats:
- ubicació: `nginx/certs/`

---

## Consideracions específiques de LXC

- Evitar stack massa pesat
- Volums persistents obligatoris per dades i configuració
- Validar compatibilitat de monitorització amb Docker dins LXC
- cAdvisor i exporters poden tenir limitacions parcials segons configuració del contenidor

---

## Regles d'alineació

Qualsevol canvi rellevant ha de mantenir coherència entre:

- codi
- README
- compose
- monitorització
- configuració Nginx
- flux real de desplegament
- restricció visual basada en AutoCo
