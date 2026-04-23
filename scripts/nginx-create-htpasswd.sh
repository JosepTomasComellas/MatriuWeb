#!/usr/bin/env bash
# nginx-create-htpasswd.sh
# Genera el fitxer .htpasswd per protegir /config amb basic auth de Nginx.
# Requereix: openssl
#
# Ús: bash scripts/nginx-create-htpasswd.sh <usuari> <contrasenya>
set -euo pipefail

USER=${1:-admin}
PASS=${2:-}
OUT=/docker/MatriuWeb/nginx/certs/.htpasswd

if [ -z "$PASS" ]; then
    read -rsp "Contrasenya per a '$USER': " PASS
    echo
fi

[ -z "$PASS" ] && { echo "ERROR: contrasenya buida"; exit 1; }

HASH=$(openssl passwd -apr1 "$PASS")
echo "$USER:$HASH" >> "$OUT"

echo "Usuari '$USER' afegit a $OUT"
echo ""
echo "Activa basic auth descomentant a nginx/conf.d/matriuweb.conf:"
echo "  auth_basic           \"Configuració MatriuWeb\";"
echo "  auth_basic_user_file /etc/nginx/certs/.htpasswd;"
echo ""
echo "Reinicia Nginx per aplicar:"
echo "  docker compose restart nginx"
