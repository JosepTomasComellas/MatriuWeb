#!/usr/bin/env bash
# deploy.sh — Actualitza i reinicia MatriuWeb a la LXC
set -euo pipefail

cd /docker/MatriuWeb

echo "=== Deploy MatriuWeb ==="
git pull
docker compose up -d --build
docker compose ps
echo "Deploy completat"
