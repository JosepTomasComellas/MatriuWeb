#!/usr/bin/env bash
# restore-config.sh — Restaura frame-config.json des d'un backup
set -euo pipefail

BACKUP_DIR=/docker/MatriuWeb/data/config/backups
CONFIG=/docker/MatriuWeb/data/config/frame-config.json

if [ -z "${1:-}" ]; then
    echo "Backups disponibles:"
    ls -lt "$BACKUP_DIR"/*.json 2>/dev/null | awk '{print NR". "$NF}'
    echo ""
    echo "Ús: $0 <nom-fitxer>"
    exit 0
fi

SRC="$BACKUP_DIR/$1"
[ -f "$SRC" ] || { echo "No trobat: $SRC"; exit 1; }

# Backup del fitxer actual abans de restaurar
cp "$CONFIG" "$BACKUP_DIR/frame-config-pre-restore-$(date +%Y%m%d-%H%M%S).json" 2>/dev/null || true

cp "$SRC" "$CONFIG"
echo "Restaurat: $SRC -> $CONFIG"
echo "Reinicia el contenidor web per aplicar canvis:"
echo "  docker compose restart web"
