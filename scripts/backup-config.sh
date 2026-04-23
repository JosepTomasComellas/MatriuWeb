#!/usr/bin/env bash
# backup-config.sh — Crea una còpia manual de frame-config.json
set -euo pipefail

SRC=/docker/MatriuWeb/data/config/frame-config.json
DST=/docker/MatriuWeb/data/config/backups/frame-config-$(date +%Y%m%d-%H%M%S).json
MAX_BACKUPS=${1:-20}

[ -f "$SRC" ] || { echo "No existeix $SRC"; exit 1; }

cp "$SRC" "$DST"
echo "Backup creat: $DST"

# Netejar els més antics
ls -t /docker/MatriuWeb/data/config/backups/*.json | tail -n +$((MAX_BACKUPS + 1)) | xargs -r rm --
echo "Backups conservats: $MAX_BACKUPS"
