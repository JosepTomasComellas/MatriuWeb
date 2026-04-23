# 🧩 MatriuWeb · v0.1.0

Dashboard web modular basat en Blazor per visualitzar múltiples fonts (iframes) amb configuració persistent en JSON.

---

## 🎯 Objectiu

MatriuWeb és una aplicació pensada per:

- centralitzar dashboards (Grafana, apps internes, etc.)
- funcionar en entorns lleugers (LXC Proxmox)
- mantenir persistència simple (JSON)
- tenir una UI moderna basada en l’estil d’AutoCo

---

## 🧱 Stack tecnològic

- Blazor (.NET 10)
- MudBlazor
- Docker Compose
- Nginx (reverse proxy)
- Redis (cache)
- Prometheus + Grafana (monitorització)

---

## 🖥️ Entorn de desenvolupament

- Windows
- PowerShell
- Ruta local: `D:\Claude\MatriuWeb`

---

## 🚀 Flux de treball

1. Desenvolupament local
2. Validació (`scripts/check.ps1`)
3. Commit + push a GitHub
4. Deploy manual a LXC:
   ```bash
   git pull
   docker compose up -d --build
