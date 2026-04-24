#!/usr/bin/env bash
# deploy.sh — Desplegament automatitzat de MatriuWeb
# Ús: bash /docker/MatriuWeb/scripts/deploy.sh [--force]
#   --force  reconstrueix les imatges encara que no hi hagi canvis
set -euo pipefail

# ── Configuració ─────────────────────────────────────────────
BASE=/docker/MatriuWeb
LOG_FILE="$BASE/logs/deploy.log"
HEALTH_TIMEOUT=180   # segons màxims per esperar que els serveis siguin healthy
HEALTH_INTERVAL=5    # interval de comprovació en segons
FORCE_BUILD=false

[[ "${1:-}" == "--force" ]] && FORCE_BUILD=true

# ── Colors ────────────────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'
CYAN='\033[0;36m'; BOLD='\033[1m'; NC='\033[0m'

_ts()  { date '+%Y-%m-%d %H:%M:%S'; }
_log() { local m="[$(_ts)] $*"; echo -e "$m"; echo -e "$m" | sed 's/\x1B\[[0-9;]*m//g' >> "$LOG_FILE"; }
ok()   { _log "${GREEN}✔${NC}  $*"; }
warn() { _log "${YELLOW}⚠${NC}  $*"; }
fail() { _log "${RED}✘${NC}  $*"; exit 1; }
info() { _log "${CYAN}→${NC}  $*"; }
hr()   { _log "────────────────────────────────────────────────────"; }

# ── Pre-checks ────────────────────────────────────────────────
[[ "$(id -u)" -eq 0 ]] || fail "Cal executar com a root"
command -v docker &>/dev/null     || fail "Docker no instal·lat"
docker compose version &>/dev/null 2>&1 || fail "Docker Compose plugin no disponible"
[[ -d "$BASE" ]]           || fail "Directori $BASE no existeix"
[[ -f "$BASE/compose.yaml" ]] || fail "compose.yaml no trobat a $BASE"

mkdir -p "$BASE/logs" "$BASE/data/config/backups"
cd "$BASE"

# ── Capçalera del log ─────────────────────────────────────────
{ echo ""; hr; _log "${BOLD}MatriuWeb — Deploy${NC}"; _log " Inici: $(_ts)"; hr; } 2>/dev/null || true

# ── Backup pre-deploy ─────────────────────────────────────────
if [[ -f "data/config/frame-config.json" ]]; then
    BACKUP="data/config/backups/frame-config-predeploy-$(date '+%Y%m%d-%H%M%S').json"
    cp data/config/frame-config.json "$BACKUP"
    ok "Backup config: $(basename "$BACKUP")"
fi

# ── Git: detectar canvis ──────────────────────────────────────
info "Comprovant repositori..."

# Avís si hi ha canvis locals no committats
if ! git diff --quiet HEAD 2>/dev/null; then
    warn "Hi ha canvis locals no committats — s'ignoraran (git stash per desar-los)"
fi

git fetch origin main 2>&1 | tail -1

LOCAL=$(git rev-parse HEAD)
REMOTE=$(git rev-parse origin/main)
HAS_NEW_CODE=false

if [[ "$LOCAL" == "$REMOTE" ]]; then
    warn "Ja és la versió més recent: $(git log -1 --format='%h %s')"
else
    info "Actualitzant: ${LOCAL:0:7} → ${REMOTE:0:7}"
    git pull origin main
    HAS_NEW_CODE=true
    ok "Codi actualitzat: $(git log -1 --format='%h %s')"
fi

# Versió desplegada
VERSION=$(grep -oP '(?<="Version": ")[^"]+' src/MatriuWeb/appsettings.json 2>/dev/null || echo "?")
_log " Versió: ${BOLD}v${VERSION}${NC}"

# ── Build i arrencada ─────────────────────────────────────────
if $HAS_NEW_CODE || $FORCE_BUILD; then
    info "Construint imatge i arrencant contenidors..."
    docker compose up -d --build 2>&1 | tee -a "$LOG_FILE"
else
    info "Arrencant contenidors (sense reconstrucció)..."
    docker compose up -d 2>&1 | tee -a "$LOG_FILE"
fi

# ── Espera que els serveis siguin healthy ─────────────────────
info "Esperant que els serveis siguin healthy (màx ${HEALTH_TIMEOUT}s)..."

# Llista de contenidors que tenen healthcheck configurat
SERVICES=("matriuweb-redis" "matriuweb-web" "matriuweb-nginx")

wait_for_healthy() {
    local elapsed=0
    while [[ $elapsed -lt $HEALTH_TIMEOUT ]]; do
        local pending=0
        local failed=0

        for svc in "${SERVICES[@]}"; do
            local state health
            state=$(docker inspect --format '{{.State.Status}}' "$svc" 2>/dev/null || echo "absent")
            health=$(docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{end}}' "$svc" 2>/dev/null || echo "")

            if [[ "$state" == "exited" || "$state" == "dead" ]]; then
                echo ""
                fail "Contenidor $svc ha parat (estat: $state). Comprova: docker logs $svc"
            fi

            # Si té healthcheck, ha de ser healthy; si no en té, n'hi ha prou amb running
            if [[ -n "$health" && "$health" != "healthy" ]]; then
                if [[ "$health" == "unhealthy" ]]; then
                    failed=$((failed + 1))
                else
                    pending=$((pending + 1))
                fi
            fi
        done

        if [[ $failed -gt 0 ]]; then
            echo ""
            fail "${failed} contenidor/s unhealthy. Comprova: docker compose ps && docker compose logs web --tail=50"
        fi

        if [[ $pending -eq 0 ]]; then
            echo ""
            return 0
        fi

        sleep $HEALTH_INTERVAL
        elapsed=$((elapsed + HEALTH_INTERVAL))
        printf "\r  ${CYAN}→${NC}  Esperant... %ds (%d pendent/s)   " "$elapsed" "$pending"
    done

    echo ""
    return 1
}

if wait_for_healthy; then
    ALL_OK=true
else
    ALL_OK=false
fi

# ── Estat final ───────────────────────────────────────────────
hr
docker compose ps
hr

if $ALL_OK; then
    ok "Deploy completat — ${BOLD}v${VERSION}${NC} — $(git log -1 --format='%h %s')"
    _log " Fi: $(_ts)"
else
    warn "Timeout esperant serveis healthy (${HEALTH_TIMEOUT}s)"
    warn "Comprova: docker compose ps"
    warn "Logs web:  docker compose logs web --tail=50"
    warn "Logs nginx: docker compose logs nginx --tail=20"
    exit 1
fi
