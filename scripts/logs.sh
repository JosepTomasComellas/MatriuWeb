#!/usr/bin/env bash
# logs.sh — Consulta els logs dels contenidors
set -euo pipefail

SERVICE=${1:-}
LINES=${2:-100}

cd /docker/MatriuWeb

if [ -z "$SERVICE" ]; then
    docker compose logs --tail="$LINES" --follow
else
    docker compose logs --tail="$LINES" --follow "$SERVICE"
fi
