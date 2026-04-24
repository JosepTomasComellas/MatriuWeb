#!/usr/bin/env bash
# bootstrap-lxc.sh — Prepara una LXC Debian/Ubuntu per a MatriuWeb
set -euo pipefail

echo "=== Bootstrap MatriuWeb LXC ==="

# Docker
if ! command -v docker &>/dev/null; then
    apt-get update -qq
    apt-get install -y ca-certificates curl gnupg
    install -m 0755 -d /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
    chmod a+r /etc/apt/keyrings/docker.gpg
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] \
        https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" \
        > /etc/apt/sources.list.d/docker.list
    apt-get update -qq
    apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin
    systemctl enable --now docker
    echo "Docker instal·lat"
else
    echo "Docker ja disponible: $(docker --version)"
fi

# Estructura de directoris
BASE=/docker/MatriuWeb
mkdir -p "$BASE/data/config/backups"
mkdir -p "$BASE/data/redis"
mkdir -p "$BASE/data/grafana"
mkdir -p "$BASE/data/prometheus"
mkdir -p "$BASE/data/alertmanager"
mkdir -p "$BASE/nginx/certs"
mkdir -p "$BASE/logs/web"
mkdir -p "$BASE/logs/nginx"
mkdir -p "$BASE/monitoring/grafana/provisioning"
mkdir -p "$BASE/monitoring/grafana/dashboards"

chmod 777 "$BASE/data/grafana"      # Grafana escriu com a UID 472
chmod 777 "$BASE/data/alertmanager" # Alertmanager escriu com a UID 65534 (nobody)

echo "Estructura creada a $BASE"
echo ""
echo "Passos següents:"
echo "  cd $BASE"
echo "  git clone <repo-url> ."
echo "  cp .env.example .env && nano .env"
echo "  # Col·loca els certificats a nginx/certs/server.crt i server.key"
echo "  docker compose up -d --build"
